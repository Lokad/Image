/* Original development-only public-API adapter. VP8L framing follows the public WebP container/lossless specifications.
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
#include <libavformat/avformat.h>
#include <libavutil/avutil.h>
#include <libavutil/frame.h>
#include <libavutil/mem.h>
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
static uint8_t *read_file(const char *path, size_t *size)
{
    FILE *file = fopen(path, "rb"); if (!file) fail("Open VP8L failed.");
    if (fseek(file, 0, SEEK_END)) fail("VP8L seek failed.");
    long length = ftell(file);
    if (length < 25 || length > 512L*1024*1024 || fseek(file, 0, SEEK_SET)) fail("Invalid bounded VP8L size.");
    uint8_t *data = malloc((size_t)length); if (!data) fail("Allocate VP8L failed.");
    if (fread(data, 1, (size_t)length, file) != (size_t)length || ferror(file) || fclose(file)) fail("VP8L read failed.");
    if (memcmp(data, "RIFF", 4) || memcmp(data+8, "WEBPVP8L", 8)
        || little32(data+4) != (uint32_t)length-8) fail("Complete simple VP8L RIFF required.");
    uint32_t payload = little32(data+16);
    if (payload < 5 || (size_t)payload+20+(payload&1) != (size_t)length
        || data[20] != 0x2f || data[24] >> 5) fail("Complete version-zero VP8L chunk required.");
    *size = (size_t)length; return data;
}
typedef struct { const uint8_t *data; int64_t length, position; } MemoryInput;
static int read_memory(void *opaque, uint8_t *buffer, int count)
{
    MemoryInput *input = opaque;
    if (input->position == input->length) return AVERROR_EOF;
    int64_t remaining = input->length - input->position;
    if (count > 8192) count = 8192;
    if (count > remaining) count = (int)remaining;
    memcpy(buffer, input->data + input->position, (size_t)count); input->position += count;
    return count;
}
static int64_t seek_memory(void *opaque, int64_t offset, int whence)
{
    MemoryInput *input = opaque;
    if (whence == AVSEEK_SIZE) return input->length;
    whence &= ~AVSEEK_FORCE;
    int64_t origin = whence == SEEK_SET ? 0 : whence == SEEK_CUR ? input->position : whence == SEEK_END ? input->length : -1;
    if (origin < 0 || offset < -origin || offset > input->length - origin) return AVERROR(EINVAL);
    input->position = origin + offset; return input->position;
}
typedef struct {
    AVCodecContext *codec;
    struct SwsContext *conversion;
    AVFrame *decoded, *rgba;
    AVPacket *input;
    uint8_t *encoded, *expected;
    size_t encoded_bytes, raw_bytes;
    int width, height, cpu, row_bytes, stride, stream;
    uint64_t completed, observed;
} Work;
static void initialize_conversion(Work *work)
{
    work->conversion = sws_alloc_context(); if (!work->conversion) fail("Allocate conversion failed.");
    require(av_opt_set_int(work->conversion, "srcw", work->width, 0), "Set source width");
    require(av_opt_set_int(work->conversion, "srch", work->height, 0), "Set source height");
    require(av_opt_set_int(work->conversion, "dstw", work->width, 0), "Set target width");
    require(av_opt_set_int(work->conversion, "dsth", work->height, 0), "Set target height");
    require(av_opt_set_int(work->conversion, "src_format", AV_PIX_FMT_ARGB, 0), "Set source format");
    require(av_opt_set_int(work->conversion, "dst_format", AV_PIX_FMT_RGBA, 0), "Set target format");
    require(av_opt_set_int(work->conversion, "sws_flags", SWS_POINT|SWS_BITEXACT, 0), "Set conversion flags");
    require(av_opt_set_int(work->conversion, "threads", 1, 0), "Set conversion threads");
    require(sws_init_context(work->conversion, NULL, NULL), "Initialize conversion");
    int64_t threads; require(av_opt_get_int(work->conversion, "threads", 0, &threads), "Read conversion threads");
    if (threads != 1) fail("Conversion threading differs.");
}
static void release(Work *work)
{
    av_frame_unref(work->decoded); av_frame_unref(work->rgba);
    if (work->stream) av_packet_unref(work->input);
}
static void apply(Work *work)
{
    if (work->stream) {
        MemoryInput input = { work->encoded, (int64_t)work->encoded_bytes, 0 };
        uint8_t *scratch = av_malloc(8192); if (!scratch) fail("Allocate stream scratch failed.");
        AVIOContext *io = avio_alloc_context(scratch, 8192, 0, &input, read_memory, NULL, seek_memory);
        if (!io) fail("Allocate fresh stream context failed.");
        int64_t length = avio_size(io);
        if (length != (int64_t)work->encoded_bytes || length > INT32_MAX) fail("Stream length differs.");
        require(av_get_packet(io, work->input, (int)length), "Read fresh owned input packet");
        uint8_t extra; int end = avio_read(io, &extra, 1);
        if (work->input->size != length || input.position != length || end != AVERROR_EOF || !avio_feof(io)
            || !work->input->buf || work->input->data == work->encoded) fail("Stream acquisition differs.");
        av_freep(&io->buffer); avio_context_free(&io);
        work->input->flags |= AV_PKT_FLAG_KEY;
    }
    // Reestablish independent still-image decoder state on every operation.
    avcodec_flush_buffers(work->codec);
    require(avcodec_send_packet(work->codec, work->input), "Submit complete VP8L");
    require(avcodec_receive_frame(work->codec, work->decoded), "Receive VP8L pixels");
    AVFrame *decoded = work->decoded;
    if (decoded->width != work->width || decoded->height != work->height || decoded->format != AV_PIX_FMT_ARGB
        || !decoded->buf[0]) fail("Expected owned 8-bit ARGB VP8L pixels.");
    work->rgba->width = work->width; work->rgba->height = work->height; work->rgba->format = AV_PIX_FMT_RGBA;
    require(av_frame_get_buffer(work->rgba, 32), "Allocate independent RGBA output");
    if (sws_scale(work->conversion, (const uint8_t *const *)decoded->data, decoded->linesize,
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
    if (argc != 7 || (strcmp(argv[1], "webp-decode") && strcmp(argv[1], "webp-stream-decode"))) fail("Usage: ffmpeg-webp-decode-warm webp[-stream]-decode INPUT WIDTH HEIGHT 8 OUTPUT");
    Work work = {0}; work.cpu = affinity_cpu(); work.width = number(argv[3]); work.height = number(argv[4]);
    work.stream = !strcmp(argv[1], "webp-stream-decode");
    if (number(argv[5]) != 8) fail("VP8L decode reference requires depth 8.");
    work.row_bytes = work.width*4; work.raw_bytes = (size_t)work.row_bytes*work.height;
    if (work.raw_bytes > 512U*1024U*1024U) fail("Reference output exceeds 512 MiB.");
    av_log_set_level(AV_LOG_ERROR); work.encoded = read_file(argv[2], &work.encoded_bytes);
    const AVCodec *codec = avcodec_find_decoder_by_name("webp");
    if (!codec || codec->id != AV_CODEC_ID_WEBP) fail("Pinned software VP8L decoder missing.");
    work.codec = avcodec_alloc_context3(codec); if (!work.codec) fail("Allocate decoder failed.");
    work.codec->thread_count = 1; work.codec->thread_type = 0;
    work.codec->max_pixels = (int64_t)(work.width+255)*(work.height+64);
    require(avcodec_open2(work.codec, codec, NULL), "Open VP8L decoder");
    if (work.codec->thread_count != 1 || work.codec->thread_type != 0 || work.codec->active_thread_type != 0) fail("Codec threading differs.");
    work.input = av_packet_alloc(); work.decoded = av_frame_alloc(); work.rgba = av_frame_alloc();
    if (!work.input || !work.decoded || !work.rgba) fail("Allocate packet/frame wrappers failed.");
    if (!work.stream) {
        require(av_new_packet(work.input, (int)work.encoded_bytes), "Allocate padded resident input");
        memcpy(work.input->data, work.encoded, work.encoded_bytes); work.input->flags |= AV_PKT_FLAG_KEY;
    }
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
    const char *fresh = work.stream ? "true" : "false";
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"%s\",\"width\":%d,\"height\":%d,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"conversionThreads\":1,\"decoder\":\"webp\",\"inputFormat\":\"ARGB\",\"lossless\":true,\"colorConversionTimed\":true,\"stateResetTimed\":true,\"pixelsDeterministic\":true,\"outputBytes\":%zu,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u,\"swscaleVersion\":%u,\"freshSeekableStream\":%s,\"inputCopyTimed\":%s,\"inputLengthQueryTimed\":%s,\"inputView\":%s,\"eofChecked\":%s}\n",
        argv[1], work.width, work.height, work.cpu, work.raw_bytes, work.completed, av_version_info(), avcodec_version(), swscale_version(), fresh, fresh, fresh, work.stream ? "false" : "true", fresh); fflush(stdout);
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
    avcodec_free_context(&work.codec); sws_freeContext(work.conversion); free(work.encoded); free(work.expected); return 0;
}
