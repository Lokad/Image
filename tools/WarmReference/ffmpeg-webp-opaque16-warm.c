/* Opaque-only original development adapter; existing byte/alpha workers are preserved.
 * Original development-only public-API adapter. RIFF chunk framing follows
 * RFC 9649. No upstream implementation, tables or test material copied. */
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
static uint32_t little(const uint8_t *p, int count)
{
    uint32_t value = 0; for (int i = 0; i < count; i++) value |= (uint32_t)p[i] << (8*i); return value;
}
typedef struct {
    AVCodecContext *codec;
    AVFrame *rgba, *bgra;
    AVPacket *packet;
    uint8_t *original, *expected, *normalized_expected;
    size_t raw_bytes;
    int width, height, depth, cpu, quality, output_bytes, expected_bytes;
    uint64_t completed, observed;
} Work;
static void complete_webp(const Work *work, const AVPacket *packet)
{
    const uint8_t *p = packet->data; size_t size = (size_t)packet->size;
    if (!packet->buf || packet->size < 12 || memcmp(p,"RIFF",4) || memcmp(p+8,"WEBP",4)
        || (uint64_t)little(p+4,4)+8 != size) fail("Independent complete WebP missing.");
    size_t offset = 12; int vp8 = 0, alpha = 0, extended = 0;
    while (offset < size) {
        if (size-offset < 8) fail("Truncated WebP chunk.");
        uint32_t length = little(p+offset+4,4);
        if ((uint64_t)length+(length&1)+8 > size-offset) fail("Invalid WebP chunk size.");
        if (!memcmp(p+offset,"VP8X",4)) {
            if (extended++ || offset != 12 || length != 10 || !(p[offset+8]&16) || (p[offset+8]&2)
                || little(p+offset+12,3)+1 != (unsigned)work->width
                || little(p+offset+15,3)+1 != (unsigned)work->height) fail("WebP extended geometry/alpha differs.");
        } else if (!memcmp(p+offset,"ALPH",4)) alpha++;
        else if (!memcmp(p+offset,"VP8 ",4)) vp8++;
        else fail("Unexpected lossless/animation/metadata chunk.");
        offset += 8+(size_t)length+(length&1);
    }
    if (vp8 != 1 || alpha != 0 || extended != 0) fail("Expected one opaque still VP8 without alpha chunks.");
}
static void apply(Work *work)
{
    /* Resident original RGBA is repacked on every operation. The scratch BGRA
     * frame/context/wrapper is reused; no converted pixels or bytes are cached. */
    if (work->depth == 16) {
        const int channels[4] = {2,1,0,3};
        for (int y = 0; y < work->height; y++) {
            const uint8_t *source = work->rgba->data[0] + (size_t)y * work->rgba->linesize[0];
            uint8_t *target = work->bgra->data[0] + (size_t)y * work->bgra->linesize[0];
            for (int x = 0; x < work->width; x++)
                for (int c = 0; c < 4; c++) {
                    const uint8_t *word = source + (size_t)x*8 + channels[c]*2;
                    unsigned value = (unsigned)word[0] | (unsigned)word[1] << 8;
                    target[x*4+c] = (uint8_t)((value+128)/257);
                }
        }
    } else
    for (int y = 0; y < work->height; y++) {
        const uint8_t *source = work->rgba->data[0] + (size_t)y * work->rgba->linesize[0];
        uint8_t *target = work->bgra->data[0] + (size_t)y * work->bgra->linesize[0];
        for (int x = 0; x < work->width; x++) {
            target[x*4] = source[x*4+2]; target[x*4+1] = source[x*4+1];
            target[x*4+2] = source[x*4]; target[x*4+3] = source[x*4+3];
        }
    }
    work->bgra->pts = (int64_t)work->completed; work->bgra->pict_type = AV_PICTURE_TYPE_I;
    require(avcodec_send_frame(work->codec, work->bgra), "Submit WebP source");
    require(avcodec_receive_packet(work->codec, work->packet), "Receive WebP");
    complete_webp(work,work->packet);
    if (work->expected_bytes && work->packet->size != work->expected_bytes) fail("Repeated WebP size differs.");
    work->output_bytes = work->packet->size; work->observed += work->packet->data[work->packet->size/2]; work->completed++;
}
static void check_source(const Work *work)
{
    for (int y = 0; y < work->height; y++)
        if (memcmp(work->rgba->data[0] + (size_t)y * work->rgba->linesize[0],
                   work->original + (size_t)y * work->width*4*(work->depth/8), (size_t)work->width*4*(work->depth/8))) fail("Source changed.");
    if (work->normalized_expected) {
        const int channels[4] = {2,1,0,3};
        for (int y = 0; y < work->height; y++)
            for (int x = 0; x < work->width; x++)
                for (int c = 0; c < 4; c++)
                    if (work->bgra->data[0][(size_t)y*work->bgra->linesize[0]+x*4+c]
                        != work->normalized_expected[((size_t)y*work->width+x)*4+channels[c]])
                        fail("Full-source normalized reference differs.");
    }
}
static void check_packet(const Work *work, const AVPacket *packet)
{
    if (!packet->buf || packet->size != work->expected_bytes
        || memcmp(packet->data, work->expected, (size_t)packet->size)) fail("Repeated WebP bitstream differs.");
}
static void write_packet(const Work *work, const char *path)
{
    FILE *file = fopen(path, "wb"); if (!file) fail("Open result failed.");
    if (fwrite(work->packet->data, 1, (size_t)work->packet->size, file) != (size_t)work->packet->size || fclose(file)) fail("Write result failed.");
}
int main(int argc, char **argv)
{
    if ((argc != 8 && argc != 9) || strcmp(argv[1], "webp-opaque-encode")) fail("Usage: ffmpeg-webp-warm webp-opaque-encode INPUT WIDTH HEIGHT 16 OUTPUT QUALITY NORMALIZED-RGBA8");
    Work work = {0}; work.cpu = affinity_cpu();
    work.width = number(argv[3]); work.height = number(argv[4]); work.quality = number(argv[7]); work.depth = number(argv[5]);
    if (work.depth != 16 || work.quality > 100 || argc != 8+(work.depth==16)) fail("Opaque word WebP requires depth16, the matching reference for depth16 and quality 1..100.");
    work.raw_bytes = (size_t)work.width * work.height * 4 * (work.depth/8);
    if (work.raw_bytes > 512U*1024U*1024U) fail("Reference raster exceeds 512 MiB.");
    av_log_set_level(AV_LOG_ERROR); work.original = read_file(argv[2], work.raw_bytes);
    if (work.depth == 16) work.normalized_expected = read_file(argv[8], (size_t)work.width * work.height * 4);
    work.rgba = frame(work.width, work.height, work.depth == 16 ? AV_PIX_FMT_RGBA64LE : AV_PIX_FMT_RGBA); work.bgra = frame(work.width, work.height, AV_PIX_FMT_BGRA);
    for (int y = 0; y < work.height; y++) memcpy(work.rgba->data[0] + (size_t)y*work.rgba->linesize[0],
        work.original + (size_t)y*work.width*4*(work.depth/8), (size_t)work.width*4*(work.depth/8));
    const AVCodec *codec = avcodec_find_encoder_by_name("libwebp"); if (!codec) fail("Pinned still WebP encoder missing.");
    work.codec = avcodec_alloc_context3(codec); if (!work.codec) fail("Allocate codec failed.");
    work.codec->thread_count = 1; work.codec->thread_type = 0; work.codec->compression_level = 4;
    work.codec->width = work.width; work.codec->height = work.height; work.codec->time_base = (AVRational){1,1};
    work.codec->pix_fmt = AV_PIX_FMT_BGRA; work.codec->gop_size = 1; work.codec->global_quality = -1;
    require(av_opt_set_int(work.codec->priv_data, "lossless", 0, 0), "Set lossy WebP");
    require(av_opt_set_int(work.codec->priv_data, "quality", work.quality, 0), "Set WebP quality");
    require(av_opt_set(work.codec->priv_data, "preset", "none", 0), "Set explicit WebP preset");
    require(avcodec_open2(work.codec, codec, NULL), "Open WebP encoder");
    if (work.codec->thread_count != 1 || work.codec->thread_type != 0 || work.codec->active_thread_type != 0
        || work.codec->compression_level != 4 || work.codec->global_quality != -1) fail("Codec threading/effort differs.");
    work.packet = av_packet_alloc(); if (!work.packet) fail("Allocate packet wrapper failed.");
    apply(&work); work.expected_bytes = work.output_bytes;
    work.expected = malloc((size_t)work.output_bytes); if (!work.expected) fail("Allocate check bytes failed.");
    memcpy(work.expected, work.packet->data, (size_t)work.output_bytes);
    AVPacket *held = av_packet_clone(work.packet); if (!held) fail("Hold independent owner failed.");
    av_packet_unref(work.packet); apply(&work);
    if (held->buf->buffer == work.packet->buf->buffer) fail("Live packets alias.");
    check_packet(&work, held); check_packet(&work, work.packet); av_packet_free(&held); av_packet_unref(work.packet);
    apply(&work); check_packet(&work, work.packet); check_source(&work); write_packet(&work, argv[6]); av_packet_unref(work.packet);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"webp-opaque-encode\",\"width\":%d,\"height\":%d,\"depth\":%d,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"conversionThreads\":1,\"quality\":%d,\"effort\":4,\"lossless\":false,\"inputFormat\":\"BGRA\",\"repackingTimed\":true,\"fullSourceNormalizationTimed\":%s,\"normalizedReferenceExact\":%s,\"outputBytes\":%d,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"bitstreamDeterministic\":true,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u}\n",
        work.width, work.height, work.depth, work.cpu, work.quality, work.depth == 16 ? "true" : "false", work.depth == 16 ? "true" : "false", work.output_bytes, work.completed, av_version_info(), avcodec_version()); fflush(stdout);
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
    av_packet_free(&work.packet); av_frame_free(&work.rgba); av_frame_free(&work.bgra);
    avcodec_free_context(&work.codec); free(work.original); free(work.expected); free(work.normalized_expected); return 0;
}
