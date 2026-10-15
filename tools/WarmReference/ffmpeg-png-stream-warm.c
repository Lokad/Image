/* Original development-only public-API adapter. PNG signature/profile fields
 * follow https://www.w3.org/TR/png-3/. No upstream implementation is copied. */
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
#include <libavformat/avio.h>
#include <libavutil/avutil.h>
#include <libavutil/frame.h>
#include <libavutil/opt.h>
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
static uint8_t *read_file(const char *path, size_t *size)
{
    FILE *file = fopen(path, "rb"); if (!file) fail("Open input failed.");
    if (fseek(file, 0, SEEK_END)) fail("Input seek failed.");
    long length = ftell(file);
    if (length <= 0 || length > 512L * 1024 * 1024 || fseek(file, 0, SEEK_SET)) fail("Invalid bounded input size.");
    uint8_t *data = malloc((size_t)length); if (!data) fail("Input allocation failed.");
    if (fread(data, 1, (size_t)length, file) != (size_t)length || ferror(file) || fclose(file)) fail("Input read failed.");
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
    AVCodecContext *codec; AVPacket *packet; AVFrame *result;
    uint8_t *encoded, *original_encoded, *expected; size_t encoded_bytes, raw_bytes;
    int width, height, row_bytes, cpu; uint64_t completed, observed;
} Work;
static void apply(Work *work)
{
    MemoryInput input = { work->encoded, (int64_t)work->encoded_bytes, 0 };
    uint8_t *scratch = av_malloc(8192); if (!scratch) fail("Allocate stream scratch failed.");
    AVIOContext *io = avio_alloc_context(scratch, 8192, 0, &input, read_memory, NULL, seek_memory);
    if (!io) fail("Allocate fresh stream context failed.");
    int64_t length = avio_size(io);
    if (length <= 0 || length > INT32_MAX) fail("Stream length differs.");
    require(av_get_packet(io, work->packet, (int)length), "Read fresh owned input packet");
    uint8_t extra;
    int end = avio_read(io, &extra, 1);
    if (work->packet->size != length || input.position != length || end != AVERROR_EOF || !avio_feof(io)
        || !work->packet->buf || work->packet->data == work->encoded) fail("Stream acquisition differs.");
    av_freep(&io->buffer); avio_context_free(&io);
    work->packet->flags |= AV_PKT_FLAG_KEY; // Independent still image, never a PNG delta frame.
    require(avcodec_send_packet(work->codec, work->packet), "Submit acquired PNG");
    require(avcodec_receive_frame(work->codec, work->result), "Receive complete owned RGBA");
    if (work->result->width != work->width || work->result->height != work->height
        || work->result->format != AV_PIX_FMT_RGBA || !work->result->buf[0]
        || work->result->linesize[0] < work->row_bytes) fail("PNG output differs.");
    av_packet_unref(work->packet);
    work->observed += work->result->data[0][work->row_bytes / 2]; work->completed++;
}
static void validate(Work *work, const AVFrame *frame)
{
    for (int y = 0; y < work->height; y++)
        if (memcmp(frame->data[0] + (size_t)y * frame->linesize[0], work->expected + (size_t)y * work->row_bytes, (size_t)work->row_bytes))
            fail("Exact RGBA samples differ.");
}
static void check_and_write(Work *work, const char *path)
{
    apply(work); validate(work, work->result);
    if (memcmp(work->encoded, work->original_encoded, work->encoded_bytes)) fail("Encoded source changed.");
    FILE *output = fopen(path, "wb"); if (!output) fail("Open output failed.");
    for (int y = 0; y < work->height; y++)
        if (fwrite(work->result->data[0] + (size_t)y * work->result->linesize[0], 1, (size_t)work->row_bytes, output) != (size_t)work->row_bytes)
            fail("Write output failed.");
    if (fclose(output)) fail("Close output failed.");
    av_frame_unref(work->result);
}
int main(int argc, char **argv)
{
    if (argc != 8 || strcmp(argv[1], "png-stream-decode"))
        fail("Usage: ffmpeg-png-stream-warm png-stream-decode INPUT_PNG WIDTH HEIGHT 8 OUTPUT_RAW EXPECTED_RAW");
    av_log_set_level(AV_LOG_ERROR);
    Work work = {0}; work.cpu = affinity_cpu(); work.width = number(argv[3]); work.height = number(argv[4]);
    if (number(argv[5]) != 8) fail("Only the explicit RGBA8 stream case is supported.");
    work.row_bytes = work.width * 4; work.raw_bytes = (size_t)work.row_bytes * work.height;
    work.encoded = read_file(argv[2], &work.encoded_bytes); size_t expected_bytes;
    work.original_encoded = malloc(work.encoded_bytes); if (!work.original_encoded) fail("Allocate source witness failed.");
    memcpy(work.original_encoded, work.encoded, work.encoded_bytes);
    work.expected = read_file(argv[7], &expected_bytes);
    if (expected_bytes != work.raw_bytes || work.encoded_bytes < 33 || work.encoded[24] != 8 || work.encoded[25] != 6 || work.encoded[28])
        fail("Original PNG8 input/profile differs.");
    work.codec = avcodec_alloc_context3(avcodec_find_decoder(AV_CODEC_ID_PNG)); if (!work.codec) fail("Allocate PNG context failed.");
    work.codec->thread_count = 1; work.codec->thread_type = 0;
    work.codec->max_pixels = (int64_t)(work.width + 255) * (work.height + 64);
    require(avcodec_open2(work.codec, avcodec_find_decoder(AV_CODEC_ID_PNG), NULL), "Open PNG decoder");
    if (work.codec->thread_count != 1 || work.codec->thread_type || work.codec->active_thread_type) fail("Decoder threading differs.");
    work.packet = av_packet_alloc(); work.result = av_frame_alloc(); AVFrame *held = av_frame_alloc();
    if (!work.packet || !work.result || !held) fail("Allocate wrappers failed.");
    apply(&work); av_frame_move_ref(held, work.result); apply(&work);
    validate(&work, held); validate(&work, work.result);
    if (held->buf[0]->buffer == work.result->buf[0]->buffer || held->data[0] == work.result->data[0]) fail("Live outputs alias.");
    uint8_t saved = held->data[0][0]; held->data[0][0] ^= 255;
    validate(&work, work.result); held->data[0][0] = saved;
    av_frame_free(&held); av_frame_unref(work.result); check_and_write(&work, argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"png-stream-decode\",\"width\":%d,\"height\":%d,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"conversionThreads\":0,\"threadType\":0,\"activeThreadType\":0,\"independentLiveOutputs\":true,\"initialCompletedOperations\":%" PRIu64 ",\"freshSeekableStream\":true,\"inputCopyTimed\":true,\"inputLengthQueryTimed\":true,\"inputView\":false,\"eofChecked\":true,\"streamScratchBytes\":8192,\"inputBytes\":%zu,\"outputBytes\":%zu,\"ffmpegVersion\":\"%s\"}\n",
        work.width, work.height, work.cpu, work.completed, work.encoded_bytes, work.raw_bytes, av_version_info()); fflush(stdout);
    char line[128];
    while (fgets(line, sizeof(line), stdin)) {
        if (!strcmp(line, "quit\n")) break;
        if (!strcmp(line, "check\n")) {
            check_and_write(&work, argv[6]); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if (sscanf(line, "sample %" SCNu64 " %c", &count, &extra) != 1 || !count || count > 10000000) fail("Invalid sample count.");
        uint64_t before = work.completed, start = now_ns(); int stride = 0;
        for (uint64_t i = 0; i < count; i++) { apply(&work); stride = work.result->linesize[0]; av_frame_unref(work.result); }
        uint64_t elapsed = now_ns() - start;
        if (affinity_cpu() != work.cpu || sched_getcpu() != work.cpu || work.completed - before != count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"outputBytes\":%zu,\"cpu\":%d}\n", count, elapsed, work.completed, work.observed, stride, work.raw_bytes, work.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_frame_free(&work.result); av_packet_free(&work.packet); avcodec_free_context(&work.codec);
    free(work.encoded); free(work.original_encoded); free(work.expected); return 0;
}
