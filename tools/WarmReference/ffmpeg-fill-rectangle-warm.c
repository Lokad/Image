/* Original development-only adapter around FFmpeg's public image-fill API.
 * Resident borrowed-canvas ownership matches ImageDrawing.FillRectangle.
 * No codec implementation or upstream source is included. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <libavutil/avutil.h>
#include <libavutil/imgutils.h>

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
static int number(const char *text)
{
    char *end; errno = 0; long value = strtol(text, &end, 10);
    if (errno || !*text || *end || value < 1 || value > 32768) fail("Invalid geometry/depth.");
    return (int)value;
}
static uint64_t now_ns(void)
{
    struct timespec value;
    if (clock_gettime(CLOCK_MONOTONIC, &value)) fail("Monotonic clock failed.");
    return (uint64_t)value.tv_sec * UINT64_C(1000000000) + (uint64_t)value.tv_nsec;
}
static int affinity_cpu(void)
{
    cpu_set_t mask;
    if (sched_getaffinity(0, sizeof(mask), &mask) || CPU_COUNT(&mask) != 1)
        fail("The warm reference requires exactly one logical CPU.");
    for (int cpu = 0; cpu < CPU_SETSIZE; cpu++) if (CPU_ISSET(cpu, &mask)) return cpu;
    fail("No affinity CPU."); return -1;
}
typedef struct {
    uint8_t *original, *canvas;
    uint32_t rgba16[4];
    int depth, pixel_bytes, stride, cpu, x, y, width, height;
    size_t bytes;
    uint64_t completed, observed;
} Work;

static void apply(Work *work)
{
    int left = work->x < 0 ? 0 : work->x, top = work->y < 0 ? 0 : work->y;
    int right = work->x + work->width, bottom = work->y + work->height;
    if (right > 513) right = 513;
    if (bottom > 259) bottom = 259;
    uint32_t color[4];
    for (int i = 0; i < 4; i++)
        color[i] = work->depth == 8 ? (work->rgba16[i] + 128) / 257 : work->rgba16[i];
    uint8_t *data[4] = { work->canvas + top * work->stride + left * work->pixel_bytes, NULL, NULL, NULL };
    ptrdiff_t strides[4] = { work->stride, 0, 0, 0 };
    if (av_image_fill_color(data, strides, work->depth == 8 ? AV_PIX_FMT_RGBA : AV_PIX_FMT_RGBA64LE,
                            color, right - left, bottom - top, 0))
        fail("Public FFmpeg fill failed.");
    work->observed += work->canvas[19 * work->stride];
    work->completed++;
}
static void check_and_write(Work *work, const char *path)
{
    apply(work);
    for (int y = 0; y < 259; y++)
    for (int x = 0; x < 513; x++)
    for (int c = 0; c < 4; c++) {
        int offset = y * work->stride + x * work->pixel_bytes + c * work->depth / 8;
        int expected = work->original[offset], actual = work->canvas[offset];
        if (work->depth == 16) {
            expected |= work->original[offset + 1] << 8;
            actual |= work->canvas[offset + 1] << 8;
        }
        if (x < 256 && y >= 19 && y < 148)
            expected = work->depth == 8 ? (work->rgba16[c] + 128) / 257 : work->rgba16[c];
        if (actual != expected) fail("Full fill canvas/untouched pixels differ.");
    }
    FILE *output = fopen(path, "wb"); if (!output) fail("Open output failed.");
    if (fwrite(work->canvas, 1, work->bytes, output) != work->bytes || fclose(output))
        fail("Write output failed.");
}
int main(int argc, char **argv)
{
    if (argc != 7 || strcmp(argv[1], "fill-rectangle"))
        fail("Usage: ffmpeg-fill-rectangle-warm fill-rectangle INPUT_RAW WIDTH HEIGHT DEPTH OUTPUT_RAW");
    Work work = {0}; work.cpu = affinity_cpu();
    int width = number(argv[3]), height = number(argv[4]); work.depth = number(argv[5]);
    if (width != 513 || height != 259 || (work.depth != 8 && work.depth != 16)) fail("Fixed RGBA8/16 geometry differs.");
    work.pixel_bytes = work.depth / 8 * 4; work.stride = width * work.pixel_bytes; work.bytes = (size_t)work.stride * height;
    work.x = -17; work.y = 19; work.width = 273; work.height = 129;
    uint32_t color8[4] = { 23, 57, 91, 128 }, color16[4] = { 12345, 23456, 34567, 45678 };
    for (int c = 0; c < 4; c++) work.rgba16[c] = work.depth == 8 ? color8[c] * 257 : color16[c];
    work.original = malloc(work.bytes); work.canvas = malloc(work.bytes);
    if (!work.original || !work.canvas) fail("Allocate resident canvas failed.");
    FILE *input = fopen(argv[2], "rb"); if (!input) fail("Open input failed.");
    if (fread(work.original, 1, work.bytes, input) != work.bytes || fgetc(input) != EOF || ferror(input))
        fail("Input geometry/precision differs.");
    if (fclose(input)) fail("Close input failed.");
    memcpy(work.canvas, work.original, work.bytes);
    check_and_write(&work, argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"fill-rectangle\",\"width\":513,\"height\":259,\"depth\":%d,\"cpu\":%d,\"affinityCount\":1,\"workerThreads\":1,\"outputWidth\":513,\"outputHeight\":259,\"x\":-17,\"y\":19,\"fillWidth\":273,\"fillHeight\":129,\"clippedX\":0,\"clippedY\":19,\"clippedWidth\":256,\"clippedHeight\":129,\"borrowedCanvas\":true,\"canvasMutationTimed\":true,\"colorNormalizationTimed\":true,\"clippingTimed\":true,\"sourcePreparationTimed\":false,\"canvasPreparationTimed\":false,\"outputAllocationTimed\":false,\"initialCompletedOperations\":1,\"inputStride\":%d,\"ffmpegVersion\":\"%s\",\"avutilVersion\":%u}\n",
           work.depth, work.cpu, work.stride, av_version_info(), avutil_version()); fflush(stdout);
    char line[128];
    while (fgets(line, sizeof(line), stdin)) {
        if (!strcmp(line, "quit\n")) break;
        if (!strcmp(line, "check\n")) {
            check_and_write(&work, argv[6]);
            puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if (sscanf(line, "sample %" SCNu64 " %c", &count, &extra) != 1 || !count || count > UINT64_C(10000000))
            fail("Invalid bounded sample request.");
        uint64_t completed = work.completed, start = now_ns();
        for (uint64_t i = 0; i < count; i++) apply(&work);
        uint64_t elapsed = now_ns() - start;
        if (sched_getcpu() != work.cpu || affinity_cpu() != work.cpu || work.completed - completed != count)
            fail("Sample CPU/counters differ.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"cpu\":%d}\n",
               count, elapsed, work.completed, work.observed, work.stride, work.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    free(work.canvas); free(work.original); return 0;
}
