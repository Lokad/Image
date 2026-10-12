/* Original development-only public-API adapter. JPEG framing follows T.81.
 * Reuses Lokad.Image's own protocol design; no upstream implementation copied. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <libavcodec/avcodec.h>
#include <libavutil/avutil.h>
#include <libavutil/frame.h>
#include <libavutil/opt.h>
#include <libswscale/swscale.h>

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
static void require(int code, const char *message)
{
    if (code < 0) { char error[AV_ERROR_MAX_STRING_SIZE]; av_strerror(code, error, sizeof(error));
        fprintf(stderr, "%s: %s\n", message, error); exit(1); }
}
static int number(const char *text)
{
    char *end; errno = 0; long value = strtol(text, &end, 10);
    if (errno || !*text || *end || value < 1 || value > 32768) fail("Invalid bounded integer.");
    return (int)value;
}
static int affinity_cpu(void)
{
    cpu_set_t mask;
    if (sched_getaffinity(0, sizeof(mask), &mask) || CPU_COUNT(&mask) != 1) fail("Exactly one CPU required.");
    for (int cpu = 0; cpu < CPU_SETSIZE; cpu++) if (CPU_ISSET(cpu, &mask)) return cpu;
    fail("No affinity CPU."); return -1;
}
static uint64_t now_ns(void)
{
    struct timespec value; if (clock_gettime(CLOCK_MONOTONIC, &value)) fail("Clock failed.");
    return (uint64_t)value.tv_sec * UINT64_C(1000000000) + (uint64_t)value.tv_nsec;
}
static uint8_t *read_file(const char *path, size_t *size)
{
    FILE *file = fopen(path, "rb"); if (!file) fail("Open JPEG failed.");
    if (fseek(file, 0, SEEK_END)) fail("JPEG seek failed.");
    long length = ftell(file);
    if (length < 4 || length > 512L*1024*1024 || fseek(file, 0, SEEK_SET)) fail("Invalid bounded JPEG size.");
    uint8_t *data = malloc((size_t)length); if (!data) fail("Allocate JPEG failed.");
    if (fread(data, 1, (size_t)length, file) != (size_t)length || ferror(file) || fclose(file)) fail("JPEG read failed.");
    if (data[0] != 255 || data[1] != 216 || data[length-2] != 255 || data[length-1] != 217) fail("Complete JPEG required.");
    *size = (size_t)length; return data;
}
typedef struct {
    AVCodecContext *codec;
    struct SwsContext *conversion, *chroma;
    AVFrame *decoded, *rgba, *u, *v;
    AVPacket *input;
    uint8_t *encoded, *expected;
    size_t encoded_bytes, raw_bytes;
    int width, height, cpu, row_bytes, stride, subsampled, gray;
    uint64_t completed, observed;
} Work;
static void initialize_conversion(Work *work)
{
    work->conversion = sws_alloc_context(); if (!work->conversion) fail("Allocate conversion failed.");
    require(av_opt_set_int(work->conversion, "srcw", work->width, 0), "Set source width");
    require(av_opt_set_int(work->conversion, "srch", work->height, 0), "Set source height");
    require(av_opt_set_int(work->conversion, "dstw", work->width, 0), "Set target width");
    require(av_opt_set_int(work->conversion, "dsth", work->height, 0), "Set target height");
    require(av_opt_set_int(work->conversion, "src_format", work->gray ? AV_PIX_FMT_GRAY8 : AV_PIX_FMT_YUVJ444P, 0), "Set source format");
    require(av_opt_set_int(work->conversion, "dst_format", AV_PIX_FMT_RGBA, 0), "Set target format");
    require(av_opt_set_int(work->conversion, "sws_flags", SWS_BILINEAR|SWS_ACCURATE_RND|SWS_FULL_CHR_H_INT, 0), "Set conversion flags");
    require(av_opt_set_int(work->conversion, "src_h_chr_pos", 0, 0), "Set horizontal chroma position");
    require(av_opt_set_int(work->conversion, "src_v_chr_pos", 0, 0), "Set vertical chroma position");
    require(av_opt_set_int(work->conversion, "threads", 1, 0), "Set conversion threads");
    require(sws_init_context(work->conversion, NULL, NULL), "Initialize conversion");
    const int *matrix = sws_getCoefficients(SWS_CS_ITU601);
    require(sws_setColorspaceDetails(work->conversion, matrix, 1, matrix, 1, 0, 1<<16, 1<<16), "Set full-range BT.601");
    int64_t threads; require(av_opt_get_int(work->conversion, "threads", 0, &threads), "Read conversion threads");
    if (threads != 1) fail("Conversion threading differs.");
    if (work->subsampled) {
        int vertical=work->subsampled==420 ? 2 : 1;
        int cw=(work->width+1)/2, ch=(work->height+vertical-1)/vertical;
        work->chroma=sws_alloc_context(); if (!work->chroma) fail("Allocate chroma expansion failed.");
        require(av_opt_set_int(work->chroma,"srcw",cw,0),"Set chroma width");
        require(av_opt_set_int(work->chroma,"srch",ch,0),"Set chroma height");
        require(av_opt_set_int(work->chroma,"dstw",cw*2,0),"Set expanded chroma width");
        require(av_opt_set_int(work->chroma,"dsth",ch*vertical,0),"Set expanded chroma height");
        require(av_opt_set_int(work->chroma,"src_format",AV_PIX_FMT_GRAY8,0),"Set chroma input format");
        require(av_opt_set_int(work->chroma,"dst_format",AV_PIX_FMT_GRAY8,0),"Set chroma output format");
        require(av_opt_set_int(work->chroma,"sws_flags",SWS_BILINEAR|SWS_ACCURATE_RND,0),"Set chroma expansion flags");
        require(av_opt_set_int(work->chroma,"threads",1,0),"Set chroma threads");
        require(sws_init_context(work->chroma,NULL,NULL),"Initialize chroma expansion");
        require(av_opt_get_int(work->chroma,"threads",0,&threads),"Read chroma threads");
        if (threads!=1) fail("Chroma threading differs.");
    }
}
static void release(Work *work)
{
    av_frame_unref(work->decoded); av_frame_unref(work->rgba);
    if (work->subsampled) { av_frame_unref(work->u); av_frame_unref(work->v); }
}
static void apply(Work *work)
{
    // Reestablish independent still-image decoder state on every operation.
    avcodec_flush_buffers(work->codec);
    require(avcodec_send_packet(work->codec, work->input), "Submit complete JPEG");
    require(avcodec_receive_frame(work->codec, work->decoded), "Receive JPEG pixels");
    AVFrame *decoded = work->decoded;
    if (decoded->width != work->width || decoded->height != work->height || decoded->format != (work->gray ? AV_PIX_FMT_GRAY8 : work->subsampled==420 ? AV_PIX_FMT_YUVJ420P : work->subsampled==422 ? AV_PIX_FMT_YUVJ422P : AV_PIX_FMT_YUVJ444P)
        // The software decoder leaves one-component GRAY8 range untagged.
        // The declared JPEG interpretation and conversion context are full range.
        || (decoded->color_range != AVCOL_RANGE_JPEG && (!work->gray || decoded->color_range != AVCOL_RANGE_UNSPECIFIED))
        || !decoded->buf[0]) fail("Expected owned full-range JPEG pixels at the declared sampling.");
    const uint8_t *planes[4]={decoded->data[0],decoded->data[1],decoded->data[2],NULL};
    int strides[4]={decoded->linesize[0],decoded->linesize[1],decoded->linesize[2],0};
    if (work->subsampled) {
        int vertical=work->subsampled==420 ? 2 : 1;
        int cw=(work->width+1)/2, ch=(work->height+vertical-1)/vertical;
        work->u->width=work->v->width=cw*2; work->u->height=work->v->height=ch*vertical;
        work->u->format=work->v->format=AV_PIX_FMT_GRAY8;
        require(av_frame_get_buffer(work->u,32),"Allocate independent expanded U");
        require(av_frame_get_buffer(work->v,32),"Allocate independent expanded V");
        const uint8_t *u[4]={decoded->data[1],NULL,NULL,NULL}, *v[4]={decoded->data[2],NULL,NULL,NULL};
        int us[4]={decoded->linesize[1],0,0,0}, vs[4]={decoded->linesize[2],0,0,0};
        if (sws_scale(work->chroma,u,us,0,ch,work->u->data,work->u->linesize)!=ch*vertical
            || sws_scale(work->chroma,v,vs,0,ch,work->v->data,work->v->linesize)!=ch*vertical) fail("Chroma expansion incomplete.");
        // Consume only the original bounds: a crop view keeps the doubled grid
        // at odd dimensions without a redundant full-frame packing copy.
        planes[1]=work->u->data[0]; planes[2]=work->v->data[0];
        strides[1]=work->u->linesize[0]; strides[2]=work->v->linesize[0];
    }
    work->rgba->width = work->width; work->rgba->height = work->height; work->rgba->format = AV_PIX_FMT_RGBA;
    require(av_frame_get_buffer(work->rgba, 32), "Allocate independent RGBA output");
    if (sws_scale(work->conversion, planes, strides,
        0, work->height, work->rgba->data, work->rgba->linesize) != work->height) fail("Color conversion incomplete.");
    work->stride = work->rgba->linesize[0];
    work->observed += work->rgba->data[0][work->row_bytes/2]; work->completed++;
}
static void check_source(const Work *work)
{
    if (work->input->size != (int)work->encoded_bytes || memcmp(work->input->data, work->encoded, work->encoded_bytes))
        fail("Encoded source changed.");
}
static void check_frame(const Work *work, const AVFrame *value)
{
    if (value->width != work->width || value->height != work->height || value->format != AV_PIX_FMT_RGBA
        || !value->buf[0] || value->linesize[0] < work->row_bytes) fail("RGBA geometry/ownership differs.");
    for (int y = 0; y < work->height; y++) {
        const uint8_t *row = value->data[0] + (size_t)y*value->linesize[0];
        if (memcmp(row, work->expected + (size_t)y*work->row_bytes, (size_t)work->row_bytes)) fail("Repeated decoded pixels differ.");
        for (int x = 0; x < work->width; x++) if (row[x*4+3] != 255) fail("Opaque alpha differs.");
    }
}
static void check_and_write(Work *work, const char *path)
{
    apply(work); check_frame(work, work->rgba); check_source(work);
    FILE *file = fopen(path, "wb"); if (!file) fail("Open RGBA result failed.");
    for (int y = 0; y < work->height; y++)
        if (fwrite(work->rgba->data[0] + (size_t)y*work->stride, 1, (size_t)work->row_bytes, file) != (size_t)work->row_bytes)
            fail("Write RGBA result failed.");
    if (fclose(file)) fail("Close result failed.");
    release(work);
}
int main(int argc, char **argv)
{
    if (argc != 7 || (strcmp(argv[1], "jpeg-decode") && strcmp(argv[1], "jpeg420-decode") && strcmp(argv[1], "jpeg422-decode") && strcmp(argv[1], "jpeg-gray-decode"))) fail("Usage: ffmpeg-jpeg-decode-warm jpeg-decode|jpeg420-decode|jpeg422-decode|jpeg-gray-decode INPUT WIDTH HEIGHT 8 OUTPUT");
    Work work = {0}; work.cpu = affinity_cpu(); work.width = number(argv[3]); work.height = number(argv[4]);
    work.gray=!strcmp(argv[1],"jpeg-gray-decode");
    work.subsampled=!strcmp(argv[1],"jpeg420-decode") ? 420 : !strcmp(argv[1],"jpeg422-decode") ? 422 : 0;
    if (number(argv[5]) != 8) fail("JPEG decode reference requires depth 8.");
    work.row_bytes = work.width*4; work.raw_bytes = (size_t)work.row_bytes*work.height;
    if (work.raw_bytes > 512U*1024U*1024U) fail("Reference output exceeds 512 MiB.");
    av_log_set_level(AV_LOG_ERROR); work.encoded = read_file(argv[2], &work.encoded_bytes);
    const AVCodec *codec = avcodec_find_decoder_by_name("mjpeg");
    if (!codec || codec->id != AV_CODEC_ID_MJPEG) fail("Pinned software JPEG decoder missing.");
    work.codec = avcodec_alloc_context3(codec); if (!work.codec) fail("Allocate decoder failed.");
    work.codec->thread_count = 1; work.codec->thread_type = 0;
    work.codec->max_pixels = (int64_t)(work.width+255)*(work.height+64);
    require(avcodec_open2(work.codec, codec, NULL), "Open JPEG decoder");
    if (work.codec->thread_count != 1 || work.codec->thread_type != 0 || work.codec->active_thread_type != 0) fail("Codec threading differs.");
    work.input = av_packet_alloc(); work.decoded = av_frame_alloc(); work.rgba = av_frame_alloc();
    if (!work.input || !work.decoded || !work.rgba) fail("Allocate packet/frame wrappers failed.");
    if (work.subsampled) { work.u=av_frame_alloc(); work.v=av_frame_alloc(); if (!work.u || !work.v) fail("Allocate chroma wrappers failed."); }
    require(av_new_packet(work.input, (int)work.encoded_bytes), "Allocate padded resident input");
    memcpy(work.input->data, work.encoded, work.encoded_bytes); work.input->flags |= AV_PKT_FLAG_KEY;
    initialize_conversion(&work); apply(&work);
    work.expected = malloc(work.raw_bytes); if (!work.expected) fail("Allocate untimed validation pixels failed.");
    for (int y = 0; y < work.height; y++) memcpy(work.expected + (size_t)y*work.row_bytes,
        work.rgba->data[0] + (size_t)y*work.stride, (size_t)work.row_bytes);
    AVFrame *held = av_frame_clone(work.rgba); if (!held) fail("Hold live owner failed.");
    release(&work); apply(&work);
    if (held->buf[0]->buffer == work.rgba->buf[0]->buffer) fail("Live RGBA outputs alias.");
    check_frame(&work, held); check_frame(&work, work.rgba);
    held->data[0][0] ^= 255; check_frame(&work, work.rgba); av_frame_free(&held); release(&work);
    check_and_write(&work, argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"%s\",\"width\":%d,\"height\":%d,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"conversionThreads\":1,\"decoder\":\"mjpeg\",\"sampling\":\"%s\",\"chromaExpansionTimed\":%s,\"chromaVerticalFactor\":%d,\"grayscale\":%s,\"colorConversionTimed\":true,\"stateResetTimed\":true,\"pixelsDeterministic\":true,\"outputBytes\":%zu,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u,\"swscaleVersion\":%u}\n",
        argv[1],work.width, work.height, work.cpu, work.gray ? "gray" : work.subsampled==420 ? "420" : work.subsampled==422 ? "422" : "444", work.subsampled ? "true" : "false", work.subsampled==420 ? 2 : 1, work.gray ? "true" : "false", work.raw_bytes, work.completed, av_version_info(), avcodec_version(), swscale_version()); fflush(stdout);
    char line[128];
    while (fgets(line, sizeof(line), stdin)) {
        if (!strcmp(line, "quit\n")) break;
        if (!strcmp(line, "check\n")) {
            check_and_write(&work, argv[6]); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if (sscanf(line, "sample %" SCNu64 " %c", &count, &extra) != 1 || !count || count > UINT64_C(10000000)) fail("Invalid bounded sample request.");
        uint64_t completed = work.completed, start = now_ns();
        for (uint64_t i = 0; i < count; i++) { apply(&work); release(&work); }
        uint64_t elapsed = now_ns()-start;
        if (sched_getcpu() != work.cpu || affinity_cpu() != work.cpu || work.completed-completed != count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"outputBytes\":%zu,\"cpu\":%d}\n",
            count, elapsed, work.completed, work.observed, work.stride, work.raw_bytes, work.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_packet_free(&work.input); av_frame_free(&work.decoded); av_frame_free(&work.rgba);
    av_frame_free(&work.u); av_frame_free(&work.v); avcodec_free_context(&work.codec); sws_freeContext(work.conversion); sws_freeContext(work.chroma); free(work.encoded); free(work.expected); return 0;
}
