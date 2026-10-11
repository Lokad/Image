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
#include <libavutil/mem.h>

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
    AVFrame *source, *result;
    uint8_t *original;
    int width, height, depth, row_bytes, output_width, output_height, output_row_bytes, cpu;
    uint64_t completed, observed;
} Work;

static void apply(Work *work)
{
    work->source->pts = (int64_t)work->completed;
    require(av_buffersrc_add_frame_flags(work->input, work->source, AV_BUFFERSRC_FLAG_KEEP_REF), "Submit source");
    require(av_buffersink_get_frame(work->output, work->result), "Receive nearest resize");
    if (work->result->width != work->output_width || work->result->height != work->output_height
        || work->result->format != work->source->format || !work->result->buf[0]
        || work->result->buf[0]->buffer == work->source->buf[0]->buffer
        || work->result->linesize[0] < work->output_row_bytes)
        fail("Result is not an independent owned RGBA raster.");
    work->observed += work->result->data[0][0];
    work->completed++;
}

static void check_source(Work *work)
{
    for (int y=0;y<work->height;y++)
        if (memcmp(work->source->data[0]+(size_t)y*work->source->linesize[0],work->original+(size_t)y*work->row_bytes,(size_t)work->row_bytes)) fail("Source was changed.");
}
static void check_frame(Work *work, AVFrame *frame)
{
    for (int y=0;y<work->output_height;y++) {
        int sy=(int)(((uint64_t)2*y+1)*work->height/((uint64_t)2*work->output_height));
        for (int x=0;x<work->output_width;x++) {
            int sx=(int)(((uint64_t)2*x+1)*work->width/((uint64_t)2*work->output_width));
            if (memcmp(frame->data[0]+(size_t)y*frame->linesize[0]+x*4,work->original+(size_t)sy*work->row_bytes+sx*4,4)) fail("Independent nearest coordinates differ.");
        }
    }
}
static void check_ownership(Work *work)
{
    AVFrame *first=av_frame_alloc(); if(!first) fail("Allocate ownership frame failed.");
    apply(work);av_frame_move_ref(first,work->result);apply(work);
    if(first->buf[0]->buffer==work->result->buf[0]->buffer)fail("Two live outputs alias.");
    check_frame(work,first);check_frame(work,work->result);
    first->data[0][0]^=255;check_frame(work,work->result);check_source(work);
    av_frame_free(&first);av_frame_unref(work->result);
}
static void check_and_write(Work *work,const char *path)
{
    apply(work);check_frame(work,work->result);check_source(work);
    FILE *output=fopen(path,"wb");if(!output)fail("Open result failed.");
    for(int y=0;y<work->output_height;y++)
        if(fwrite(work->result->data[0]+(size_t)y*work->result->linesize[0],1,(size_t)work->output_row_bytes,output)!=(size_t)work->output_row_bytes)fail("Write result failed.");
    if(fclose(output))fail("Close result failed.");
    av_frame_unref(work->result);
}

int main(int argc, char **argv)
{
    if (argc != 7 || strcmp(argv[1], "nearest-resize"))
        fail("Usage: ffmpeg-nearest-warm nearest-resize INPUT_RAW WIDTH HEIGHT DEPTH OUTPUT_RAW");
    av_log_set_level(AV_LOG_ERROR);
    Work work = {0}; work.cpu = affinity_cpu();
    work.width = number(argv[3]); work.height = number(argv[4]); work.depth = number(argv[5]);
    if (work.depth != 8 || work.width < 2 || work.height < 2) fail("Use RGBA8 dimensions at least two.");
    work.output_width=work.width/2;work.output_height=work.height/2;work.output_row_bytes=work.output_width*4;
    work.row_bytes = work.width * (work.depth / 8) * 4;
    size_t bytes = (size_t)work.row_bytes * work.height;
    if (bytes > 512U * 1024U * 1024U) fail("Reference input exceeds 512 MiB.");
    work.original = malloc(bytes); if (!work.original) fail("Allocate original input failed.");
    FILE *input = fopen(argv[2], "rb"); if (!input) fail("Open raw input failed.");
    if (fread(work.original, 1, bytes, input) != bytes || fgetc(input) != EOF || ferror(input))
        fail("Input geometry/precision differs.");
    fclose(input);
    work.source = av_frame_alloc(); work.result = av_frame_alloc();
    if (!work.source || !work.result) fail("Allocate frame wrappers failed.");
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
    require(avfilter_graph_create_filter(&work.output,avfilter_get_by_name("buffersink"),"output",NULL,NULL,work.graph),"Create output");
    AVFilterInOut *outputs=avfilter_inout_alloc(),*inputs=avfilter_inout_alloc();
    if(!outputs||!inputs)fail("Allocate graph endpoints failed.");
    outputs->name=av_strdup("in");outputs->filter_ctx=work.input;outputs->pad_idx=0;
    inputs->name=av_strdup("out");inputs->filter_ctx=work.output;inputs->pad_idx=0;
    char graph[2048];
    snprintf(graph,sizeof(graph),"[in]extractplanes=r+g+b+a[r][g][b][a];"
        "[g][b][r]mergeplanes=0x001020:format=gbrp,zscale=w=%d:h=%d:filter=point:rangein=full:range=full:matrixin=gbr:matrix=gbr:dither=none,format=gbrp,setsar=1,extractplanes=r+g+b[rs][gs][bs];"
        "[a]mergeplanes=0x000000:format=gbrp,zscale=w=%d:h=%d:filter=point:rangein=full:range=full:matrixin=gbr:matrix=gbr:dither=none,format=gbrp,setsar=1,extractplanes=g[as];"
        "[gs][bs][rs][as]mergeplanes=0x00102030:format=gbrap,format=rgba[out]",
        work.output_width,work.output_height,work.output_width,work.output_height);
    require(avfilter_graph_parse_ptr(work.graph,graph,&inputs,&outputs,NULL),"Parse nearest graph");
    if(inputs||outputs)fail("Unconnected graph endpoint.");
    avfilter_inout_free(&inputs);avfilter_inout_free(&outputs);
    require(avfilter_graph_config(work.graph, NULL), "Configure graph");
    if (work.graph->nb_threads != 1 || work.graph->thread_type != 0) fail("Graph threading differs.");
    check_ownership(&work);check_and_write(&work, argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"nearest-resize\",\"width\":%d,\"height\":%d,\"depth\":%d,\"cpu\":%d,\"affinityCount\":1,\"filterThreads\":1,\"threadType\":0,\"outputWidth\":%d,\"outputHeight\":%d,\"independentLiveOutputs\":true,\"initialCompletedOperations\":%" PRIu64 ",\"inputStride\":%d,\"ffmpegVersion\":\"%s\",\"avfilterVersion\":%u,\"avutilVersion\":%u}\n",
           work.width, work.height, work.depth, work.cpu, work.output_width,work.output_height,work.completed,work.source->linesize[0],
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
    av_frame_free(&work.result); av_frame_free(&work.source); avfilter_graph_free(&work.graph);
    free(work.original); return 0;
}
