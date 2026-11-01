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
    uint8_t *original, *canvas;
    int cpu;
    uint64_t completed, observed, bands;
} Work;
static int move(uint8_t *canvas, int sx, int sy, int dx, int dy, int *bands)
{
    const int stride = 513 * 4, width = 256, height = 129;
    int shift = dy > sy ? dy - sy : sy - dy;
    if (!shift) return 1;
    for (int remaining = height; remaining > 0;) {
        int count = remaining < shift ? remaining : shift;
        int first = dy > sy ? remaining - count : height - remaining;
        size_t source = (size_t)(sy + first) * stride + sx * 4;
        size_t destination = (size_t)(dy + first) * stride + dx * 4;
        size_t span = (size_t)(count - 1) * stride + width * 4;
        if (!(source + span <= destination || destination + span <= source)) return 2;
        av_image_copy_plane(canvas + destination, stride, canvas + source, stride, width * 4, count);
        (*bands)++;
        remaining -= count;
    }
    return 0;
}
static void apply(Work *work)
{
    int bands = 0;
    if (move(work->canvas, 17, 19, 127, 65, &bands) ||
        move(work->canvas, 127, 65, 17, 19, &bands) || bands != 6)
        fail("Disjoint-band snapshot pair failed.");
    work->observed += work->canvas[(19 * 513 + 17) * 4];
    work->bands += (uint64_t)bands; work->completed++;
}
static void check_and_write(Work *work, const char *path)
{
    const size_t bytes = 513 * 259 * 4;
    apply(work);
    uint8_t *first = malloc(bytes), *expected = malloc(bytes);
    if (!first || !expected) fail("Allocate verification pixels failed.");
    memcpy(first, work->original, bytes);
    for (int y = 0; y < 129; y++)
        memcpy(first + ((y + 65) * 513 + 127) * 4, work->original + ((y + 19) * 513 + 17) * 4, 256 * 4);
    memcpy(expected, first, bytes);
    for (int y = 0; y < 129; y++)
        memcpy(expected + ((y + 19) * 513 + 17) * 4, first + ((y + 65) * 513 + 127) * 4, 256 * 4);
    if (memcmp(expected, work->canvas, bytes)) fail("Snapshot pair full canvas differs.");
    free(expected); free(first);
    FILE *output = fopen(path, "wb"); if (!output) fail("Open result failed.");
    if (fwrite(work->canvas, 1, bytes, output) != bytes || fclose(output)) fail("Write result failed.");
}
int main(int argc, char **argv)
{
    if (argc != 7 || strcmp(argv[1], "copy-region-overlap")) fail("Usage: ffmpeg-copy-region-overlap-warm copy-region-overlap INPUT WIDTH HEIGHT DEPTH OUTPUT");
    if (number(argv[3]) != 513 || number(argv[4]) != 259 || number(argv[5]) != 8) fail("Fixed RGBA8 canvas differs.");
    const size_t bytes = 513 * 259 * 4;
    Work work = {0}; work.cpu = affinity_cpu();
    work.original = malloc(bytes); work.canvas = malloc(bytes);
    if (!work.original || !work.canvas) fail("Allocate resident canvas failed.");
    FILE *input = fopen(argv[2], "rb"); if (!input) fail("Open source failed.");
    if (fread(work.original, 1, bytes, input) != bytes || fgetc(input) != EOF || ferror(input)) fail("Input differs.");
    if (fclose(input)) fail("Close input failed.");
    memcpy(work.canvas, work.original, bytes); check_and_write(&work, argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"copy-region-overlap\",\"width\":513,\"height\":259,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"workerThreads\":1,\"outputWidth\":513,\"outputHeight\":259,\"publicMovesPerPair\":2,\"nativeCallsPerPair\":6,\"bandHeight\":46,\"sourceAliasesCanvas\":true,\"borrowedCanvas\":true,\"canvasMutationTimed\":true,\"restorationTimed\":true,\"canvasPreparationTimed\":false,\"sourcePreparationTimed\":false,\"outputAllocationTimed\":false,\"temporaryPixelBytes\":0,\"disjointNativeRanges\":true,\"initialCompletedOperations\":1,\"inputStride\":2052,\"ffmpegVersion\":\"%s\",\"avutilVersion\":%u}\n",
           work.cpu, av_version_info(), avutil_version()); fflush(stdout);
    char line[128];
    while (fgets(line, sizeof(line), stdin)) {
        if (!strcmp(line, "quit\n")) break;
        if (!strcmp(line, "check\n")) {
            check_and_write(&work, argv[6]);
            puts("{\"state\":\"checked\",\"exact\":true,\"sourceAliasesCanvas\":true,\"snapshotSemanticsExact\":true}"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if (sscanf(line, "sample %" SCNu64 " %c", &count, &extra) != 1 || !count || count > UINT64_C(10000000)) fail("Invalid bounded sample request.");
        uint64_t completed = work.completed, start = now_ns();
        for (uint64_t i = 0; i < count; i++) apply(&work);
        uint64_t elapsed = now_ns() - start;
        if (sched_getcpu() != work.cpu || affinity_cpu() != work.cpu || work.completed - completed != count) fail("CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":2052,\"cpu\":%d,\"publicMoves\":%" PRIu64 ",\"nativeBandCalls\":%" PRIu64 "}\n",
               count, elapsed, work.completed, work.observed, work.cpu, 2 * work.completed, work.bands); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    free(work.canvas); free(work.original); return 0;
}
