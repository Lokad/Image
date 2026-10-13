/* Original development-only public-API adapter. VP8 framing follows the public WebP container specification and RFC 6386.
 * https://developers.google.com/speed/webp/docs/riff_container
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
static uint32_t little32(const uint8_t *data)
{
    return (uint32_t)data[0] | (uint32_t)data[1]<<8 | (uint32_t)data[2]<<16 | (uint32_t)data[3]<<24;
}
static uint8_t *read_file(const char *path, size_t *size, int *has_alpha)
{
    FILE *file = fopen(path, "rb"); if (!file) fail("Open VP8 failed.");
    if (fseek(file, 0, SEEK_END)) fail("VP8 seek failed.");
    long length = ftell(file);
    if (length < 25 || length > 512L*1024*1024 || fseek(file, 0, SEEK_SET)) fail("Invalid bounded VP8 size.");
    uint8_t *data = malloc((size_t)length); if (!data) fail("Allocate VP8 failed.");
    if (fread(data, 1, (size_t)length, file) != (size_t)length || ferror(file) || fclose(file)) fail("VP8 read failed.");
    if (memcmp(data, "RIFF", 4) || memcmp(data+8, "WEBP", 4)
        || little32(data+4) != (uint32_t)length-8) fail("Complete VP8 RIFF required.");
    size_t offset = 12; int extended = 0, alpha = 0, lossy = 0; *has_alpha = 0;
    while (offset < (size_t)length) {
        if ((size_t)length-offset < 8) fail("Truncated VP8 chunk.");
        uint32_t count = little32(data+offset+4); size_t bytes = (size_t)count+(count&1);
        if (bytes > (size_t)length-offset-8) fail("VP8 chunk exceeds RIFF.");
        const uint8_t *payload = data+offset+8;
        if (!memcmp(data+offset, "VP8X", 4)) {
            if (extended++ || offset != 12 || count != 10 || (payload[0] != 0 && payload[0] != 0x10)) fail("Still opaque/alpha VP8 extension required.");
            *has_alpha = !!(payload[0] & 0x10);
        } else if (!memcmp(data+offset, "ALPH", 4)) {
            if (!extended || !*has_alpha || alpha++ || lossy || count < 1) fail("One preceding alpha chunk required.");
        } else if (!memcmp(data+offset, "VP8 ", 4)) {
            if ((*has_alpha && !alpha) || lossy++ || count < 10 || (payload[0]&1)
                || memcmp(payload+3, "\x9d\x01\x2a", 3)) fail("One VP8 key frame required.");
        } else fail("Unexpected frozen VP8 profile chunk.");
        offset += 8+bytes;
    }
    if (lossy != 1 || alpha != *has_alpha || (*has_alpha && extended != 1)) fail("Complete opaque or VP8X/ALPH/VP8 profile required.");
    *size = (size_t)length; return data;
}
typedef struct {
    AVCodecContext *codec;
    struct SwsContext *chroma, *color, *precision;
    AVFrame *decoded, *u, *v, *rgb64, *rgba;
    AVPacket *input;
    uint8_t *encoded, *expected;
    size_t encoded_bytes, raw_bytes;
    int width, height, cpu, row_bytes, stride, has_alpha;
    uint64_t completed, observed;
} Work;
static struct SwsContext *conversion(int sw, int sh, int sf, int dw, int dh, int df, int flags, int color)
{
    struct SwsContext *value = sws_alloc_context(); if (!value) fail("Allocate conversion failed.");
    require(av_opt_set_int(value, "srcw", sw, 0), "Set source width");
    require(av_opt_set_int(value, "srch", sh, 0), "Set source height");
    require(av_opt_set_int(value, "dstw", dw, 0), "Set target width");
    require(av_opt_set_int(value, "dsth", dh, 0), "Set target height");
    require(av_opt_set_int(value, "src_format", sf, 0), "Set source format");
    require(av_opt_set_int(value, "dst_format", df, 0), "Set target format");
    require(av_opt_set_int(value, "sws_flags", flags, 0), "Set conversion flags");
    require(av_opt_set_int(value, "threads", 1, 0), "Set conversion threads");
    if (color) {
        require(av_opt_set_int(value, "src_h_chr_pos", 0, 0), "Set horizontal chroma position");
        require(av_opt_set_int(value, "src_v_chr_pos", 0, 0), "Set vertical chroma position");
    }
    if (sf == AV_PIX_FMT_RGBA64LE) require(av_opt_set(value, "sws_dither", "none", 0), "Disable precision dither");
    require(sws_init_context(value, NULL, NULL), "Initialize conversion");
    if (color) {
        const int *matrix = sws_getCoefficients(SWS_CS_ITU601);
        require(sws_setColorspaceDetails(value, matrix, 0, matrix, 1, 0, 1<<16, 1<<16), "Set limited BT601 conversion");
    }
    int64_t threads; require(av_opt_get_int(value, "threads", 0, &threads), "Read conversion threads");
    if (threads != 1) fail("Conversion threading differs.");
    return value;
}
static void initialize_conversion(Work *work)
{
    int cw = (work->width+1)/2, ch = (work->height+1)/2;
    work->chroma = conversion(cw,ch,AV_PIX_FMT_GRAY8,cw*2,ch*2,AV_PIX_FMT_GRAY8,SWS_BILINEAR|SWS_ACCURATE_RND,0);
    work->color = conversion(work->width,work->height,AV_PIX_FMT_YUV444P,work->width,work->height,
        AV_PIX_FMT_RGBA64LE,SWS_BILINEAR|SWS_ACCURATE_RND|SWS_FULL_CHR_H_INT,1);
    work->precision = conversion(work->width,work->height,AV_PIX_FMT_RGBA64LE,work->width,work->height,
        AV_PIX_FMT_RGBA,SWS_ACCURATE_RND|SWS_BITEXACT,0);
}
static void release(Work *work)
{
    av_frame_unref(work->decoded); av_frame_unref(work->u); av_frame_unref(work->v);
    av_frame_unref(work->rgb64); av_frame_unref(work->rgba);
}
static void allocate(AVFrame *value, int width, int height, int format)
{
    value->width=width; value->height=height; value->format=format;
    require(av_frame_get_buffer(value,32), "Allocate independent conversion/output frame");
}
static void apply(Work *work)
{
    avcodec_flush_buffers(work->codec);
    require(avcodec_send_packet(work->codec,work->input), "Submit complete VP8");
    require(avcodec_receive_frame(work->codec,work->decoded), "Receive VP8 pixels");
    AVFrame *decoded=work->decoded;
    if (decoded->width!=work->width || decoded->height!=work->height
        || decoded->format!=(work->has_alpha ? AV_PIX_FMT_YUVA420P : AV_PIX_FMT_YUV420P)
        || !decoded->buf[0] || (work->has_alpha && !decoded->data[3])) fail("Expected owned YUV(A)420P VP8 pixels.");
    int cw=(work->width+1)/2, ch=(work->height+1)/2;
    allocate(work->u,cw*2,ch*2,AV_PIX_FMT_GRAY8); allocate(work->v,cw*2,ch*2,AV_PIX_FMT_GRAY8);
    const uint8_t *u[4]={decoded->data[1],NULL,NULL,NULL}, *v[4]={decoded->data[2],NULL,NULL,NULL};
    int us[4]={decoded->linesize[1],0,0,0}, vs[4]={decoded->linesize[2],0,0,0};
    if (sws_scale(work->chroma,u,us,0,ch,work->u->data,work->u->linesize)!=ch*2
        || sws_scale(work->chroma,v,vs,0,ch,work->v->data,work->v->linesize)!=ch*2) fail("Chroma expansion incomplete.");
    const uint8_t *yuv[4]={decoded->data[0],work->u->data[0],work->v->data[0],NULL};
    int strides[4]={decoded->linesize[0],work->u->linesize[0],work->v->linesize[0],0};
    allocate(work->rgb64,work->width,work->height,AV_PIX_FMT_RGBA64LE);
    if (sws_scale(work->color,yuv,strides,0,work->height,work->rgb64->data,work->rgb64->linesize)!=work->height) fail("BT601 conversion incomplete.");
    allocate(work->rgba,work->width,work->height,AV_PIX_FMT_RGBA);
    if (sws_scale(work->precision,(const uint8_t *const *)work->rgb64->data,work->rgb64->linesize,
        0,work->height,work->rgba->data,work->rgba->linesize)!=work->height) fail("Precision reduction incomplete.");
    if (work->has_alpha) for (int y=0;y<work->height;y++) for (int x=0;x<work->width;x++)
        work->rgba->data[0][(size_t)y*work->rgba->linesize[0]+x*4+3]=decoded->data[3][(size_t)y*decoded->linesize[3]+x];
    work->stride=work->rgba->linesize[0]; work->observed+=work->rgba->data[0][work->row_bytes/2]; work->completed++;
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
    if (argc != 7 || strcmp(argv[1], "vp8-decode")) fail("Usage: ffmpeg-vp8-decode-warm vp8-decode INPUT WIDTH HEIGHT 8 OUTPUT");
    Work work = {0}; work.cpu = affinity_cpu(); work.width = number(argv[3]); work.height = number(argv[4]);
    if (number(argv[5]) != 8) fail("VP8 decode reference requires depth 8.");
    work.row_bytes = work.width*4; work.raw_bytes = (size_t)work.row_bytes*work.height;
    if (work.raw_bytes > 512U*1024U*1024U) fail("Reference output exceeds 512 MiB.");
    av_log_set_level(AV_LOG_ERROR); work.encoded = read_file(argv[2], &work.encoded_bytes, &work.has_alpha);
    const AVCodec *codec = avcodec_find_decoder_by_name("webp");
    if (!codec || codec->id != AV_CODEC_ID_WEBP) fail("Pinned software VP8 decoder missing.");
    work.codec = avcodec_alloc_context3(codec); if (!work.codec) fail("Allocate decoder failed.");
    work.codec->thread_count = 1; work.codec->thread_type = 0;
    work.codec->max_pixels = (int64_t)(work.width+255)*(work.height+64);
    require(avcodec_open2(work.codec, codec, NULL), "Open VP8 decoder");
    if (work.codec->thread_count != 1 || work.codec->thread_type != 0 || work.codec->active_thread_type != 0) fail("Codec threading differs.");
    work.input = av_packet_alloc(); work.decoded = av_frame_alloc(); work.rgba = av_frame_alloc();
    work.u=av_frame_alloc(); work.v=av_frame_alloc(); work.rgb64=av_frame_alloc();
    if (!work.input || !work.decoded || !work.rgba || !work.u || !work.v || !work.rgb64) fail("Allocate packet/frame wrappers failed.");
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
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"vp8-decode\",\"width\":%d,\"height\":%d,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"conversionThreads\":1,\"decoder\":\"webp\",\"inputFormat\":\"%s\",\"alphaMergeTimed\":%s,\"lossless\":false,\"colorConversionTimed\":true,\"stateResetTimed\":true,\"pixelsDeterministic\":true,\"outputBytes\":%zu,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u,\"swscaleVersion\":%u}\n",
        work.width, work.height, work.cpu, work.has_alpha ? "YUVA420P" : "YUV420P", work.has_alpha ? "true" : "false",
        work.raw_bytes, work.completed, av_version_info(), avcodec_version(), swscale_version()); fflush(stdout);
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
    avcodec_free_context(&work.codec); av_frame_free(&work.u); av_frame_free(&work.v); av_frame_free(&work.rgb64);
    sws_freeContext(work.chroma); sws_freeContext(work.color); sws_freeContext(work.precision); free(work.encoded); free(work.expected); return 0;
}
