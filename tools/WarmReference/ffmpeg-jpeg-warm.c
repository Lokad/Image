/* Original development-only public-API adapter. JPEG framing follows T.81 B.2;
 * alpha flattening follows Lokad.Image SPEC.md. No upstream implementation copied. */
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
static uint8_t *read_file(const char *path, size_t size)
{
    FILE *file = fopen(path, "rb"); if (!file) fail("Open raster failed.");
    uint8_t *data = malloc(size); if (!data) fail("Allocate raster failed.");
    if (fread(data, 1, size, file) != size || fgetc(file) != EOF || ferror(file) || fclose(file)) fail("Raster size/read differs.");
    return data;
}
static AVFrame *frame(int width, int height, enum AVPixelFormat format)
{
    AVFrame *value = av_frame_alloc(); if (!value) fail("Allocate frame failed.");
    value->width = width; value->height = height; value->format = format;
    require(av_frame_get_buffer(value, 32), "Allocate frame storage"); return value;
}
typedef struct {
    AVCodecContext *codec;
    struct SwsContext *conversion;
    AVFrame *rgba, *flattened, *yuv;
    AVPacket *packet;
    uint8_t *original, *expected;
    size_t raw_bytes;
    int width, height, cpu, quality, output_bytes, expected_bytes;
    uint64_t completed, observed;
} Work;
static void apply(Work *work)
{
    /* Exact 8-bit single-round flattening to RGB 0x204060. Scratch frames and
     * contexts are reused; both transforms execute again for every operation. */
    const int background[3] = {32,64,96};
    for (int y = 0; y < work->height; y++) {
        const uint8_t *source = work->rgba->data[0] + (size_t)y * work->rgba->linesize[0];
        uint8_t *target = work->flattened->data[0] + (size_t)y * work->flattened->linesize[0];
        for (int x = 0; x < work->width; x++) {
            int alpha = source[x*4+3];
            for (int c = 0; c < 3; c++) target[x*4+c] = (uint8_t)((source[x*4+c]*alpha + background[c]*(255-alpha) + 127)/255);
            target[x*4+3] = 255;
        }
    }
    if (sws_scale(work->conversion, (const uint8_t *const *)work->flattened->data, work->flattened->linesize,
                  0, work->height, work->yuv->data, work->yuv->linesize) != work->height) fail("Color conversion incomplete.");
    work->yuv->pts = (int64_t)work->completed; work->yuv->pict_type = AV_PICTURE_TYPE_I;
    work->yuv->quality = work->codec->global_quality;
    require(avcodec_send_frame(work->codec, work->yuv), "Submit JPEG source");
    require(avcodec_receive_packet(work->codec, work->packet), "Receive JPEG");
    AVPacket *packet = work->packet;
    if (!packet->buf || packet->size < 4 || !(packet->flags & AV_PKT_FLAG_KEY)
        || packet->data[0] != 255 || packet->data[1] != 216
        || packet->data[packet->size-2] != 255 || packet->data[packet->size-1] != 217) fail("Independent complete JPEG missing.");
    if (work->expected_bytes && packet->size != work->expected_bytes) fail("Repeated JPEG size differs.");
    work->output_bytes = packet->size; work->observed += packet->data[packet->size/2]; work->completed++;
}
static void check_source(const Work *work)
{
    for (int y = 0; y < work->height; y++)
        if (memcmp(work->rgba->data[0] + (size_t)y * work->rgba->linesize[0],
                   work->original + (size_t)y * work->width*4, (size_t)work->width*4)) fail("Source changed.");
}
static void check_packet(const Work *work, const AVPacket *packet)
{
    if (!packet->buf || packet->size != work->expected_bytes
        || memcmp(packet->data, work->expected, (size_t)packet->size)) fail("Repeated JPEG bitstream differs.");
}
static void write_packet(const Work *work, const char *path)
{
    FILE *file = fopen(path, "wb"); if (!file) fail("Open result failed.");
    if (fwrite(work->packet->data, 1, (size_t)work->packet->size, file) != (size_t)work->packet->size || fclose(file)) fail("Write result failed.");
}
int main(int argc, char **argv)
{
    if ((argc != 8 && argc != 9) || strcmp(argv[1], "jpeg-encode") || (argc == 9 && strcmp(argv[8], "420") && strcmp(argv[8], "422")))
        fail("Usage: ffmpeg-jpeg-warm jpeg-encode INPUT WIDTH HEIGHT 8 OUTPUT QSCALE [420|422]");
    const char *sampling = argc == 9 ? argv[8] : "444";
    enum AVPixelFormat yuv_format = !strcmp(sampling, "422") ? AV_PIX_FMT_YUVJ422P : !strcmp(sampling, "420") ? AV_PIX_FMT_YUVJ420P : AV_PIX_FMT_YUVJ444P;
    Work work = {0}; work.cpu = affinity_cpu();
    work.width = number(argv[3]); work.height = number(argv[4]); work.quality = number(argv[7]);
    if (number(argv[5]) != 8 || work.quality > 31) fail("JPEG requires depth 8 and qscale 1..31.");
    work.raw_bytes = (size_t)work.width * work.height * 4;
    if (work.raw_bytes > 512U*1024U*1024U) fail("Reference raster exceeds 512 MiB.");
    av_log_set_level(AV_LOG_ERROR); work.original = read_file(argv[2], work.raw_bytes);
    work.rgba = frame(work.width, work.height, AV_PIX_FMT_RGBA);
    work.flattened = frame(work.width, work.height, AV_PIX_FMT_RGBA);
    work.yuv = frame(work.width, work.height, yuv_format);
    for (int y = 0; y < work.height; y++) memcpy(work.rgba->data[0] + (size_t)y*work.rgba->linesize[0],
        work.original + (size_t)y*work.width*4, (size_t)work.width*4);
    work.conversion = sws_alloc_context(); if (!work.conversion) fail("Allocate conversion failed.");
    require(av_opt_set_int(work.conversion, "srcw", work.width, 0), "Set source width");
    require(av_opt_set_int(work.conversion, "srch", work.height, 0), "Set source height");
    require(av_opt_set_int(work.conversion, "dstw", work.width, 0), "Set target width");
    require(av_opt_set_int(work.conversion, "dsth", work.height, 0), "Set target height");
    require(av_opt_set_int(work.conversion, "src_format", AV_PIX_FMT_RGBA, 0), "Set source format");
    require(av_opt_set_int(work.conversion, "dst_format", yuv_format, 0), "Set target format");
    require(av_opt_set_int(work.conversion, "sws_flags", SWS_AREA|SWS_ACCURATE_RND|SWS_BITEXACT, 0), "Set conversion flags");
    require(av_opt_set_int(work.conversion, "threads", 1, 0), "Set conversion threads");
    require(sws_init_context(work.conversion, NULL, NULL), "Initialize conversion");
    const int *matrix = sws_getCoefficients(SWS_CS_ITU601);
    require(sws_setColorspaceDetails(work.conversion, matrix, 1, matrix, 1, 0, 1<<16, 1<<16), "Set full-range BT.601");
    int64_t conversion_threads;
    require(av_opt_get_int(work.conversion, "threads", 0, &conversion_threads), "Read conversion threads");
    if (conversion_threads != 1) fail("Conversion threading differs.");
    const AVCodec *codec = avcodec_find_encoder(AV_CODEC_ID_MJPEG); if (!codec) fail("Pinned JPEG encoder missing.");
    work.codec = avcodec_alloc_context3(codec); if (!work.codec) fail("Allocate codec failed.");
    work.codec->thread_count = 1; work.codec->thread_type = 0;
    work.codec->width = work.width; work.codec->height = work.height; work.codec->time_base = (AVRational){1,1};
    work.codec->pix_fmt = yuv_format; work.codec->color_range = AVCOL_RANGE_JPEG; work.codec->colorspace = AVCOL_SPC_SMPTE170M;
    work.codec->gop_size = 1; work.codec->flags |= AV_CODEC_FLAG_QSCALE; work.codec->global_quality = work.quality*FF_QP2LAMBDA;
    require(av_opt_set(work.codec->priv_data, "huffman", "optimal", 0), "Set Huffman effort");
    require(avcodec_open2(work.codec, codec, NULL), "Open JPEG encoder");
    if (work.codec->thread_count != 1 || work.codec->thread_type != 0 || work.codec->active_thread_type != 0) fail("Codec threading differs.");
    work.packet = av_packet_alloc(); if (!work.packet) fail("Allocate packet wrapper failed.");
    apply(&work); work.expected_bytes = work.output_bytes;
    work.expected = malloc((size_t)work.output_bytes); if (!work.expected) fail("Allocate check bytes failed.");
    memcpy(work.expected, work.packet->data, (size_t)work.output_bytes);
    AVPacket *held = av_packet_clone(work.packet); if (!held) fail("Hold independent owner failed.");
    av_packet_unref(work.packet); apply(&work);
    if (held->buf->buffer == work.packet->buf->buffer) fail("Live packets alias.");
    check_packet(&work, held); check_packet(&work, work.packet); av_packet_free(&held); av_packet_unref(work.packet);
    apply(&work); check_packet(&work, work.packet); check_source(&work); write_packet(&work, argv[6]); av_packet_unref(work.packet);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"jpeg-encode\",\"width\":%d,\"height\":%d,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"conversionThreads\":1,\"qscale\":%d,\"backgroundRgb\":2113632,\"sampling\":\"%s\",\"colorConversionTimed\":true,\"flatteningTimed\":true,\"outputBytes\":%d,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"bitstreamDeterministic\":true,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u,\"swscaleVersion\":%u}\n",
        work.width, work.height, work.cpu, work.quality, sampling, work.output_bytes, work.completed, av_version_info(), avcodec_version(), swscale_version()); fflush(stdout);
    char line[128];
    while (fgets(line, sizeof(line), stdin)) {
        if (!strcmp(line, "quit\n")) break;
        if (!strcmp(line, "check\n")) {
            apply(&work); check_packet(&work, work.packet); check_source(&work); write_packet(&work, argv[6]); av_packet_unref(work.packet);
            puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if (sscanf(line, "sample %" SCNu64 " %c", &count, &extra) != 1 || !count || count > UINT64_C(10000000)) fail("Invalid bounded sample request.");
        uint64_t completed = work.completed, start = now_ns();
        for (uint64_t i = 0; i < count; i++) { apply(&work); av_packet_unref(work.packet); }
        uint64_t elapsed = now_ns()-start;
        if (sched_getcpu() != work.cpu || affinity_cpu() != work.cpu || work.completed-completed != count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"outputBytes\":%d,\"cpu\":%d}\n",
            count, elapsed, work.completed, work.observed, work.output_bytes, work.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_packet_free(&work.packet); av_frame_free(&work.rgba); av_frame_free(&work.flattened); av_frame_free(&work.yuv);
    avcodec_free_context(&work.codec); sws_freeContext(work.conversion); free(work.original); free(work.expected); return 0;
}
