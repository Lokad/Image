/* Original development-only adapter around public FFmpeg APIs. No codec source
 * or implementation tables are copied. Build outputs belong in external/. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <libavfilter/avfilter.h>
#include <libavfilter/buffersink.h>
#include <libavfilter/buffersrc.h>
#include <libavutil/avutil.h>
#include <libavutil/frame.h>
#include <libavutil/pixfmt.h>

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
static void require(int code, const char *message)
{
    if (code < 0) { char error[AV_ERROR_MAX_STRING_SIZE]; av_strerror(code, error, sizeof(error));
        fprintf(stderr, "%s: %s\n", message, error); exit(1); }
}
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
        fail("The warm reference requires affinity to exactly one logical CPU.");
    for (int cpu = 0; cpu < CPU_SETSIZE; cpu++) if (CPU_ISSET(cpu, &mask)) return cpu;
    fail("No affinity CPU."); return -1;
}

typedef struct {
    AVFilterGraph *graph;
    AVFilterContext *input, *output;
    AVFrame *source, *view, *result;
    uint8_t *original;
    int width, height, depth, row_bytes, cpu;
    uint64_t completed, observed;
} Work;

static void apply(Work *work)
{
    work->source->pts = (int64_t)work->completed;
    require(av_buffersrc_add_frame_flags(work->input, work->source, AV_BUFFERSRC_FLAG_KEEP_REF), "Submit source");
    require(av_buffersink_get_frame(work->output, work->view), "Receive crop view");
    /* crop returns a shared view. Independent materialization is timed, using
     * public frame allocation/copy APIs, before releasing that view. */
    work->result->format = work->view->format;
    work->result->width = work->view->width; work->result->height = work->view->height;
    require(av_frame_get_buffer(work->result, 32), "Allocate owned crop pixels");
    require(av_frame_copy(work->result, work->view), "Copy owned crop pixels");
    av_frame_unref(work->view);
    if (work->result->width != 256 || work->result->height != 129
        || work->result->format != work->source->format || !work->result->buf[0]
        || work->result->buf[0]->buffer == work->source->buf[0]->buffer
        || work->result->linesize[0] < 256 * (work->depth / 8) * 4)
        fail("Result is not an independent owned RGBA raster.");
    work->observed += work->result->data[0][0];
    work->completed++;
}

static void check_and_write(Work *work, const char *path)
{
    apply(work);
    FILE *output = fopen(path, "wb"); if (!output) fail("Open result file failed.");
    int pixel_bytes = work->depth / 8 * 4;
    for (int y = 0; y < work->height; y++)
        if (memcmp(work->source->data[0] + (size_t)y * work->source->linesize[0],
                   work->original + (size_t)y * work->row_bytes, (size_t)work->row_bytes)) fail("Source was changed.");
    for (int y = 0; y < 129; y++) {
        const uint8_t *expected = work->original + (size_t)(y + 65) * work->row_bytes + 127 * pixel_bytes;
        const uint8_t *result = work->result->data[0] + (size_t)y * work->result->linesize[0];
        if (memcmp(expected, result, (size_t)256 * pixel_bytes)) fail("Independent crop placement differs.");
        if (fwrite(result, 1, (size_t)256 * pixel_bytes, output) != (size_t)256 * pixel_bytes)
            fail("Write result failed.");
    }
    if (fclose(output)) fail("Close result failed.");
    av_frame_unref(work->result);
}

int main(int argc, char **argv)
{
    if (argc != 7 || strcmp(argv[1], "crop"))
        fail("Usage: ffmpeg-crop-warm crop INPUT_RAW WIDTH HEIGHT DEPTH OUTPUT_RAW");
    av_log_set_level(AV_LOG_ERROR);
    Work work = {0}; work.cpu = affinity_cpu();
    work.width = number(argv[3]); work.height = number(argv[4]); work.depth = number(argv[5]);
    if (work.depth != 8 && work.depth != 16) fail("Depth must be 8 or 16.");
    if (work.width != 513 || work.height != 259) fail("The fixed crop requires 513x259 input.");
    work.row_bytes = work.width * (work.depth / 8) * 4;
    size_t bytes = (size_t)work.row_bytes * work.height;
    if (bytes > 512U * 1024U * 1024U) fail("Reference input exceeds 512 MiB.");
    work.original = malloc(bytes); if (!work.original) fail("Allocate original input failed.");
    FILE *input = fopen(argv[2], "rb"); if (!input) fail("Open raw input failed.");
    if (fread(work.original, 1, bytes, input) != bytes || fgetc(input) != EOF || ferror(input))
        fail("Input geometry/precision differs.");
    fclose(input);
    work.source = av_frame_alloc(); work.view = av_frame_alloc(); work.result = av_frame_alloc();
    if (!work.source || !work.view || !work.result) fail("Allocate frame wrappers failed.");
    work.source->format = work.depth == 8 ? AV_PIX_FMT_RGBA : AV_PIX_FMT_RGBA64LE;
    work.source->width = work.width; work.source->height = work.height;
    require(av_frame_get_buffer(work.source, 32), "Allocate source pixels");
    for (int y = 0; y < work.height; y++)
        memcpy(work.source->data[0] + (size_t)y * work.source->linesize[0],
               work.original + (size_t)y * work.row_bytes, (size_t)work.row_bytes);
    work.graph = avfilter_graph_alloc(); if (!work.graph) fail("Allocate graph failed.");
    work.graph->nb_threads = 1; work.graph->thread_type = 0;
    char arguments[256];
    snprintf(arguments, sizeof(arguments), "video_size=%dx%d:pix_fmt=%d:time_base=1/1:pixel_aspect=1/1",
             work.width, work.height, work.source->format);
    require(avfilter_graph_create_filter(&work.input, avfilter_get_by_name("buffer"), "input", arguments, NULL, work.graph), "Create input");
    AVFilterContext *flip;
    require(avfilter_graph_create_filter(&flip, avfilter_get_by_name("crop"), "crop", "w=256:h=129:x=127:y=65:exact=1", NULL, work.graph), "Create crop");
    require(avfilter_graph_create_filter(&work.output, avfilter_get_by_name("buffersink"), "output", NULL, NULL, work.graph), "Create output");
    require(avfilter_link(work.input, 0, flip, 0), "Link input");
    require(avfilter_link(flip, 0, work.output, 0), "Link output");
    require(avfilter_graph_config(work.graph, NULL), "Configure graph");
    if (work.graph->nb_threads != 1 || work.graph->thread_type != 0) fail("Graph threading differs.");
    check_and_write(&work, argv[6]);
    apply(&work);
    AVFrame *held = av_frame_alloc(); if (!held) fail("Allocate ownership check wrapper failed.");
    av_frame_move_ref(held, work.result);
    uint8_t first = held->data[0][0];
    apply(&work);
    if (held->buf[0]->buffer == work.result->buf[0]->buffer) fail("Live outputs alias.");
    held->data[0][0] ^= 255;
    if (work.result->data[0][0] != first) fail("Live outputs share pixels.");
    av_frame_free(&held); av_frame_unref(work.result);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"crop\",\"width\":%d,\"height\":%d,\"depth\":%d,\"cpu\":%d,\"affinityCount\":1,\"filterThreads\":1,\"threadType\":0,\"outputWidth\":256,\"outputHeight\":129,\"cropX\":127,\"cropY\":65,\"independentLiveOutputs\":true,\"ownedCopyTimed\":true,\"initialCompletedOperations\":3,\"inputStride\":%d,\"ffmpegVersion\":\"%s\",\"avfilterVersion\":%u,\"avutilVersion\":%u}\n",
           work.width, work.height, work.depth, work.cpu, work.source->linesize[0],
           av_version_info(), avfilter_version(), avutil_version()); fflush(stdout);
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
        uint64_t before_completed = work.completed;
        int stride = 0;
        uint64_t start = now_ns();
        for (uint64_t i = 0; i < count; i++) {
            apply(&work); stride = work.result->linesize[0]; av_frame_unref(work.result);
        }
        uint64_t elapsed = now_ns() - start;
        if (sched_getcpu() != work.cpu || affinity_cpu() != work.cpu || work.completed - before_completed != count)
            fail("Sample CPU/workload count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"cpu\":%d}\n",
               count, elapsed, work.completed, work.observed, stride, work.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_frame_free(&work.result); av_frame_free(&work.view); av_frame_free(&work.source); avfilter_graph_free(&work.graph);
    free(work.original); return 0;
}
