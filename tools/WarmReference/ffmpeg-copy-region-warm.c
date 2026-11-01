/* Original development-only adapter around FFmpeg's public image-copy API.
 * Resident source/canvas ownership matches ImageDrawing.CopyRegion.
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
    uint8_t *original, *source, *canvas;
    int depth, pixel_bytes, stride, cpu;
    size_t bytes;
    uint64_t completed, observed;
} Work;

static void apply(Work *work)
{
    av_image_copy_plane(work->canvas + 19 * work->stride + 17 * work->pixel_bytes, work->stride,
                        work->source + 65 * work->stride + 127 * work->pixel_bytes, work->stride,
                        256 * work->pixel_bytes, 129);
    work->observed += work->canvas[19 * work->stride + 17 * work->pixel_bytes];
    work->completed++;
}
static void check_and_write(Work *work, const char *path)
{
    apply(work);
    if (memcmp(work->source, work->original, work->bytes)) fail("Borrowed source changed.");
    uint8_t *expected = malloc(work->bytes); if (!expected) fail("Allocate check pixels failed.");
    memcpy(expected, work->original, work->bytes);
    for (int y = 0; y < 129; y++)
        memcpy(expected + (y + 19) * work->stride + 17 * work->pixel_bytes,
               work->original + (y + 65) * work->stride + 127 * work->pixel_bytes,
               256 * work->pixel_bytes);
    if (memcmp(expected, work->canvas, work->bytes)) fail("Canvas pixels/untouched region differ.");
    free(expected);
    FILE *output = fopen(path, "wb"); if (!output) fail("Open result file failed.");
    if (fwrite(work->canvas, 1, work->bytes, output) != work->bytes || fclose(output))
        fail("Write result failed.");
}
int main(int argc, char **argv)
{
    if (argc != 7 || strcmp(argv[1], "copy-region"))
        fail("Usage: ffmpeg-copy-region-warm copy-region INPUT_RAW WIDTH HEIGHT DEPTH OUTPUT_RAW");
    av_log_set_level(AV_LOG_ERROR);
    Work work = {0}; work.cpu = affinity_cpu();
    int width = number(argv[3]), height = number(argv[4]); work.depth = number(argv[5]);
    if (width != 513 || height != 259 || (work.depth != 8 && work.depth != 16))
        fail("The fixed copy requires 513x259 RGBA8 or RGBA16.");
    work.pixel_bytes = work.depth / 8 * 4; work.stride = width * work.pixel_bytes;
    work.bytes = (size_t)work.stride * height;
    work.original = malloc(work.bytes); work.source = malloc(work.bytes); work.canvas = malloc(work.bytes);
    if (!work.original || !work.source || !work.canvas) fail("Allocate resident rasters failed.");
    FILE *input = fopen(argv[2], "rb"); if (!input) fail("Open raw input failed.");
    if (fread(work.original, 1, work.bytes, input) != work.bytes || fgetc(input) != EOF || ferror(input))
        fail("Input geometry/precision differs.");
    if (fclose(input)) fail("Close input failed.");
    memcpy(work.source, work.original, work.bytes); memcpy(work.canvas, work.original, work.bytes);
    check_and_write(&work, argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"copy-region\",\"width\":513,\"height\":259,\"depth\":%d,\"cpu\":%d,\"affinityCount\":1,\"workerThreads\":1,\"outputWidth\":513,\"outputHeight\":259,\"sourceX\":127,\"sourceY\":65,\"copyWidth\":256,\"copyHeight\":129,\"destinationX\":17,\"destinationY\":19,\"borrowedSource\":true,\"borrowedCanvas\":true,\"canvasMutationTimed\":true,\"sourcePreparationTimed\":false,\"canvasPreparationTimed\":false,\"outputAllocationTimed\":false,\"initialCompletedOperations\":1,\"inputStride\":%d,\"ffmpegVersion\":\"%s\",\"avutilVersion\":%u}\n",
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
            fail("Sample CPU/workload count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"cpu\":%d}\n",
               count, elapsed, work.completed, work.observed, work.stride, work.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    free(work.canvas); free(work.source); free(work.original); return 0;
}
