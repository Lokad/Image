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
#include <libavfilter/avfilter.h>
#include <libavfilter/buffersink.h>
#include <libavfilter/buffersrc.h>
#include <libavutil/avutil.h>
#include <libavutil/frame.h>
#include <libavutil/opt.h>
#include <libavutil/pixfmt.h>
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
static AVCodecContext *context(int encode, int width, int height, int depth, int encode_color, int compression_level)
{
    const AVCodec *codec = encode ? avcodec_find_encoder(AV_CODEC_ID_PNG) : avcodec_find_decoder(AV_CODEC_ID_PNG);
    if (!codec) fail("Pinned PNG codec missing.");
    AVCodecContext *value = avcodec_alloc_context3(codec); if (!value) fail("Allocate codec context failed.");
    value->thread_count = 1; value->thread_type = 0;
    // FFmpeg's pixel guard also sees internal padded frame dimensions. The
    // externally coded geometry is checked exactly before any decoding below.
    value->max_pixels = (int64_t)(width + 255) * (height + 64);
    if (encode) {
        value->width = width; value->height = height; value->time_base = (AVRational){1,1};
        value->pix_fmt = encode_color == 0 ? (depth == 8 ? AV_PIX_FMT_GRAY8 : AV_PIX_FMT_GRAY16BE)
            : encode_color == 4 ? (depth == 8 ? AV_PIX_FMT_YA8 : AV_PIX_FMT_YA16BE)
            : encode_color == 2 ? (depth == 8 ? AV_PIX_FMT_RGB24 : AV_PIX_FMT_RGB48BE)
            : depth == 8 ? AV_PIX_FMT_RGBA : AV_PIX_FMT_RGBA64BE;
        value->gop_size = 1;
        value->compression_level = compression_level;
        require(av_opt_set(value->priv_data, "pred", "none", 0), "Set PNG filter None");
    }
    require(avcodec_open2(value, codec, NULL), "Open PNG codec");
    if (value->thread_count != 1 || value->thread_type != 0 || value->active_thread_type != 0)
        fail("PNG codec threading differs.");
    return value;
}
static AVFrame *frame(int width, int height, enum AVPixelFormat format)
{
    AVFrame *value = av_frame_alloc(); if (!value) fail("Allocate frame wrapper failed.");
    value->width = width; value->height = height; value->format = format;
    require(av_frame_get_buffer(value, 32), "Allocate frame pixels"); return value;
}
static uint32_t big32(const uint8_t *value)
{
    return (uint32_t)value[0] << 24 | (uint32_t)value[1] << 16 | (uint32_t)value[2] << 8 | value[3];
}
static void check_profile(const uint8_t *data, size_t size, int width, int height, int depth, int interlace, int color)
{
    static const uint8_t signature[] = {137,80,78,71,13,10,26,10};
    if (size < 33 || memcmp(data, signature, 8) || big32(data+8) != 13 || memcmp(data+12, "IHDR", 4)
        || big32(data+16) != (uint32_t)width || big32(data+20) != (uint32_t)height
        || data[24] != depth || data[25] != color || data[26] || data[27] || data[28] != interlace) fail("PNG input/profile differs.");
}
static void swap_samples(const AVFrame *input, AVFrame *output, int width, int height)
{
    for (int y = 0; y < height; y++) {
        const uint8_t *source = input->data[0] + (size_t)y * input->linesize[0];
        uint8_t *target = output->data[0] + (size_t)y * output->linesize[0];
        for (int i = 0; i < width * 8; i += 2) { target[i] = source[i+1]; target[i+1] = source[i]; }
    }
}
static void pack_gray_alpha(const AVFrame *input, AVFrame *output, int width, int height, int depth)
{
    int sample_bytes = depth / 8;
    for (int y = 0; y < height; y++) {
        const uint8_t *source = input->data[0] + (size_t)y * input->linesize[0];
        uint8_t *target = output->data[0] + (size_t)y * output->linesize[0];
        for (int x = 0; x < width; x++)
            for (int i = 0; i < sample_bytes; i++) {
                int source_byte = sample_bytes == 2 ? i ^ 1 : i;
                target[(x*2)*sample_bytes+i] = source[(x*4)*sample_bytes+source_byte];
                target[(x*2+1)*sample_bytes+i] = source[(x*4+3)*sample_bytes+source_byte];
            }
    }
}
typedef struct {
    AVCodecContext *codec;
    AVFrame *source, *prepared, *decoded, *converted;
    AVPacket *input, *packet;
    struct SwsContext *conversion;
    AVFilterGraph *gray_graph;
    AVFilterContext *gray_input, *gray_output;
    uint8_t *original, *encoded;
    size_t raw_bytes, encoded_bytes;
    int encode, encode_stored, encode_rgb, encode_gray, encode_gray_alpha, width, height, depth, row_bytes, cpu, stride, output_bytes, rgb, gray, gray_alpha, palette, mono, coded_depth, gray_transparency, rgb_transparency;
    uint64_t completed, observed;
} Work;
static void gray16_graph(Work *work)
{
    work->gray_graph = avfilter_graph_alloc(); if (!work->gray_graph) fail("Allocate gray16 filter graph failed.");
    work->gray_graph->nb_threads = 1; work->gray_graph->thread_type = 0;
    char arguments[256];
    snprintf(arguments, sizeof(arguments), "video_size=%dx%d:pix_fmt=%d:time_base=1/1:pixel_aspect=1/1",
        work->width, work->height, AV_PIX_FMT_RGBA64LE);
    AVFilterContext *extract, *endian, *format;
    require(avfilter_graph_create_filter(&work->gray_input, avfilter_get_by_name("buffer"), "input", arguments, NULL, work->gray_graph), "Create gray16 source");
    require(avfilter_graph_create_filter(&extract, avfilter_get_by_name("extractplanes"), "gray16_red", "planes=r", NULL, work->gray_graph), "Create gray16 extraction");
    require(avfilter_graph_create_filter(&endian, avfilter_get_by_name("scale"), "gray16_endian", "w=iw:h=ih:flags=neighbor+bitexact:threads=1", NULL, work->gray_graph), "Create gray16 endian conversion");
    require(avfilter_graph_create_filter(&format, avfilter_get_by_name("format"), "gray16_format", "pix_fmts=gray16be", NULL, work->gray_graph), "Create gray16 format");
    require(avfilter_graph_create_filter(&work->gray_output, avfilter_get_by_name("buffersink"), "output", NULL, NULL, work->gray_graph), "Create gray16 sink");
    require(avfilter_link(work->gray_input, 0, extract, 0), "Link gray16 extraction");
    require(avfilter_link(extract, 0, endian, 0), "Link gray16 endian conversion");
    require(avfilter_link(endian, 0, format, 0), "Link gray16 format");
    require(avfilter_link(format, 0, work->gray_output, 0), "Link gray16 sink");
    require(avfilter_graph_config(work->gray_graph, NULL), "Configure gray16 graph");
    int64_t threads; require(av_opt_get_int(endian->priv, "threads", AV_OPT_SEARCH_CHILDREN, &threads), "Read gray16 conversion threads");
    if (threads != 1 || work->gray_graph->nb_threads != 1 || work->gray_graph->thread_type != 0) fail("Gray16 graph threading differs.");
}
static AVFrame *result_frame(Work *work) { return work->depth == 16 || work->gray || work->gray_alpha || (work->rgb && !work->rgb_transparency) || work->palette ? work->converted : work->decoded; }
static void release(Work *work)
{
    if (work->encode) {
        av_packet_unref(work->packet);
        if (work->gray_graph) av_frame_unref(work->prepared);
    }
    else { av_frame_unref(work->decoded); av_frame_unref(work->converted); }
}
static void check_frame(const AVFrame *value, const Work *work, int big_endian)
{
    int expected_format = work->depth == 8 ? AV_PIX_FMT_RGBA : big_endian ? AV_PIX_FMT_RGBA64BE : AV_PIX_FMT_RGBA64LE;
    if (value->width != work->width || value->height != work->height || value->format != expected_format
        || !value->buf[0] || value->linesize[0] < work->row_bytes) fail("PNG result geometry/precision/ownership differs.");
    for (int y = 0; y < work->height; y++) {
        const uint8_t *row = value->data[0] + (size_t)y * value->linesize[0];
        const uint8_t *expected = work->original + (size_t)y * work->row_bytes;
        if (big_endian && work->depth == 16) {
            for (int i = 0; i < work->row_bytes; i += 2)
                if (row[i] != expected[i+1] || row[i+1] != expected[i]) fail("PNG 16-bit samples differ.");
        } else if (memcmp(row, expected, (size_t)work->row_bytes)) fail("PNG samples differ.");
    }
}
static void apply(Work *work)
{
    if (work->encode) {
        AVFrame *input = work->source;
        if (work->gray_graph) {
            work->source->pts = (int64_t)work->completed;
            require(av_buffersrc_add_frame_flags(work->gray_input, work->source, AV_BUFFERSRC_FLAG_KEEP_REF), "Submit gray16 source");
            require(av_buffersink_get_frame(work->gray_output, work->prepared), "Receive owned gray16 pixels");
            if (work->prepared->format != AV_PIX_FMT_GRAY16BE || work->prepared->width != work->width
                || work->prepared->height != work->height || !work->prepared->buf[0]
                || work->prepared->linesize[0] < work->width * 2) fail("Gray16 converted frame differs.");
            input = work->prepared;
        } else if (work->encode_gray_alpha) {
            pack_gray_alpha(work->source, work->prepared, work->width, work->height, work->depth);
            input = work->prepared;
        } else if (work->encode_rgb || work->encode_gray) {
            int rows = sws_scale(work->conversion, (const uint8_t * const *)work->source->data, work->source->linesize,
                0, work->height, work->prepared->data, work->prepared->linesize);
            if (rows != work->height) fail("PNG encode conversion row count differs.");
            input = work->prepared;
        }
        if (work->depth == 16 && !work->encode_rgb && !work->encode_gray && !work->encode_gray_alpha) { swap_samples(work->source, work->prepared, work->width, work->height); input = work->prepared; }
        input->pts = (int64_t)work->completed;
        input->pict_type = AV_PICTURE_TYPE_I;
        require(avcodec_send_frame(work->codec, input), "Submit PNG source");
        require(avcodec_receive_packet(work->codec, work->packet), "Receive complete PNG");
        if (!work->packet->buf || work->packet->size <= 33 || !(work->packet->flags & AV_PKT_FLAG_KEY))
            fail("PNG packet is not an owned independent still image.");
        work->output_bytes = work->packet->size; work->stride = 0;
        work->observed += work->packet->data[work->packet->size / 2];
    } else {
        require(avcodec_send_packet(work->codec, work->input), "Submit complete PNG");
        require(avcodec_receive_frame(work->codec, work->decoded), "Receive PNG pixels");
        int format = work->mono ? AV_PIX_FMT_MONOBLACK : work->palette ? AV_PIX_FMT_PAL8 : (work->gray_alpha || work->gray_transparency) ? (work->depth == 8 ? AV_PIX_FMT_YA8 : AV_PIX_FMT_YA16BE) : work->gray ? (work->depth == 8 ? AV_PIX_FMT_GRAY8 : AV_PIX_FMT_GRAY16BE) : work->rgb_transparency ? (work->depth == 8 ? AV_PIX_FMT_RGBA : AV_PIX_FMT_RGBA64BE) : work->rgb ? (work->depth == 8 ? AV_PIX_FMT_RGB24 : AV_PIX_FMT_RGB48BE) : work->depth == 8 ? AV_PIX_FMT_RGBA : AV_PIX_FMT_RGBA64BE;
        if (work->decoded->format != format || work->decoded->width != work->width || work->decoded->height != work->height
            || !work->decoded->buf[0] || work->decoded->linesize[0] < (work->mono ? (work->width + 7) / 8 : work->width * (work->palette ? 1 : (work->gray_alpha || work->gray_transparency) ? work->depth / 8 * 2 : work->gray ? work->depth / 8 : work->rgb && !work->rgb_transparency ? work->depth / 8 * 3 : work->depth / 8 * 4))) fail("Decoded PNG format differs.");
        if (work->palette && (!work->decoded->data[1] || !work->decoded->buf[0])) fail("Decoded PNG palette ownership differs.");
        if (work->depth == 16 || work->gray || work->gray_alpha || (work->rgb && !work->rgb_transparency) || work->palette) {
            work->converted->width = work->width; work->converted->height = work->height;
            work->converted->format = work->depth == 8 ? AV_PIX_FMT_RGBA : AV_PIX_FMT_RGBA64LE;
            require(av_frame_get_buffer(work->converted, 32), "Allocate owned native-endian output");
            if ((work->rgb && !work->rgb_transparency) || work->gray || work->gray_alpha || work->palette) {
                int rows = sws_scale(work->conversion, (const uint8_t * const *)work->decoded->data, work->decoded->linesize,
                    0, work->height, work->converted->data, work->converted->linesize);
                if (rows != work->height) fail("PNG conversion row count differs.");
            } else swap_samples(work->decoded, work->converted, work->width, work->height);
        }
        AVFrame *output = result_frame(work);
        work->output_bytes = (int)work->raw_bytes; work->stride = output->linesize[0];
        work->observed += output->data[0][work->row_bytes / 2];
    }
    work->completed++;
}
static void check_packet(Work *work, const AVPacket *packet)
{
    check_profile(packet->data, (size_t)packet->size, work->width, work->height, work->depth, 0, work->encode_gray_alpha ? 4 : work->encode_gray ? 0 : work->encode_rgb ? 2 : 6);
    AVCodecContext *decoder = context(0, work->width, work->height, work->depth, 0, 6);
    AVFrame *decoded = av_frame_alloc(); if (!decoded) fail("Check frame allocation failed.");
    require(avcodec_send_packet(decoder, packet), "Check encoded PNG");
    require(avcodec_receive_frame(decoder, decoded), "Check PNG roundtrip");
    if (work->encode_gray_alpha) {
        int sample_bytes = work->depth / 8;
        int format = work->depth == 8 ? AV_PIX_FMT_YA8 : AV_PIX_FMT_YA16BE;
        if (decoded->width != work->width || decoded->height != work->height || decoded->format != format
            || !decoded->buf[0] || decoded->linesize[0] < work->width * 2 * sample_bytes) fail("Gray-alpha PNG roundtrip geometry/ownership differs.");
        for (int y = 0; y < work->height; y++) {
            const uint8_t *row = decoded->data[0] + (size_t)y * decoded->linesize[0];
            const uint8_t *expected = work->original + (size_t)y * work->row_bytes;
            for (int x = 0; x < work->width; x++)
                for (int i = 0; i < sample_bytes; i++) {
                    int source_byte = sample_bytes == 2 ? i ^ 1 : i;
                    if (row[(x*2)*sample_bytes+i] != expected[(x*4)*sample_bytes+source_byte]
                        || row[(x*2+1)*sample_bytes+i] != expected[(x*4+3)*sample_bytes+source_byte]) fail("Gray-alpha PNG roundtrip samples differ.");
                }
        }
    } else if (work->encode_gray) {
        int sample_bytes = work->depth / 8;
        int format = work->depth == 8 ? AV_PIX_FMT_GRAY8 : AV_PIX_FMT_GRAY16BE;
        if (decoded->width != work->width || decoded->height != work->height || decoded->format != format
            || !decoded->buf[0] || decoded->linesize[0] < work->width * sample_bytes) fail("Gray PNG roundtrip geometry/ownership differs.");
        for (int y = 0; y < work->height; y++) {
            const uint8_t *row = decoded->data[0] + (size_t)y * decoded->linesize[0];
            const uint8_t *expected = work->original + (size_t)y * work->row_bytes;
            for (int x = 0; x < work->width; x++) {
                for (int i = 0; i < sample_bytes; i++)
                    if (row[x * sample_bytes + i] != expected[x * 4 * sample_bytes + (sample_bytes == 2 ? i ^ 1 : i)]
                        || expected[(x * 4 + 3) * sample_bytes + i] != 255) fail("Gray PNG roundtrip samples differ.");
            }
        }
    } else if (work->encode_rgb) {
        int sample_bytes = work->depth / 8;
        int format = work->depth == 8 ? AV_PIX_FMT_RGB24 : AV_PIX_FMT_RGB48BE;
        if (decoded->width != work->width || decoded->height != work->height || decoded->format != format
            || !decoded->buf[0] || decoded->linesize[0] < work->width * 3 * sample_bytes) fail("RGB PNG roundtrip geometry/ownership differs.");
        for (int y = 0; y < work->height; y++) {
            const uint8_t *row = decoded->data[0] + (size_t)y * decoded->linesize[0];
            const uint8_t *expected = work->original + (size_t)y * work->row_bytes;
            for (int x = 0; x < work->width; x++) {
                for (int i = 0; i < 3 * sample_bytes; i++)
                    if (row[x * 3 * sample_bytes + i] != expected[x * 4 * sample_bytes + (sample_bytes == 2 ? i ^ 1 : i)])
                        fail("RGB PNG roundtrip samples differ.");
                for (int i = 0; i < sample_bytes; i++)
                    if (expected[(x * 4 + 3) * sample_bytes + i] != 255) fail("RGB PNG roundtrip alpha differs.");
            }
        }
    } else check_frame(decoded, work, 1);
    av_frame_free(&decoded); avcodec_free_context(&decoder);
}
static void check_source(Work *work)
{
    if (work->encode) check_frame(work->source, work, 0);
    else if (work->input->size != (int)work->encoded_bytes || memcmp(work->input->data, work->encoded, work->encoded_bytes))
        fail("Encoded source changed.");
}
static void check_independent_owners(Work *work)
{
    apply(work);
    AVPacket *held_packet = work->encode ? av_packet_clone(work->packet) : NULL;
    AVFrame *held_frame = work->encode ? NULL : av_frame_clone(result_frame(work));
    if (work->encode ? !held_packet : !held_frame) fail("Clone live owner failed.");
    release(work); apply(work);
    if (work->encode) {
        if (held_packet->buf->buffer == work->packet->buf->buffer) fail("Live packets share output storage.");
        check_packet(work, held_packet); check_packet(work, work->packet); av_packet_free(&held_packet);
    } else {
        if (held_frame->buf[0]->buffer == result_frame(work)->buf[0]->buffer) fail("Live rasters share output storage.");
        check_frame(held_frame, work, 0); check_frame(result_frame(work), work, 0); av_frame_free(&held_frame);
    }
    release(work); check_source(work);
}
static void check_and_write(Work *work, const char *path)
{
    apply(work); check_source(work);
    FILE *output = fopen(path, "wb"); if (!output) fail("Open result failed.");
    if (work->encode) {
        check_packet(work, work->packet);
        if (fwrite(work->packet->data, 1, (size_t)work->packet->size, output) != (size_t)work->packet->size) fail("Write PNG failed.");
    } else {
        AVFrame *value = result_frame(work); check_frame(value, work, 0);
        for (int y = 0; y < work->height; y++)
            if (fwrite(value->data[0] + (size_t)y * value->linesize[0], 1, (size_t)work->row_bytes, output) != (size_t)work->row_bytes)
                fail("Write RGBA failed.");
    }
    if (fclose(output)) fail("Close result failed.");
    release(work);
}
int main(int argc, char **argv)
{
    if (argc != 7 || strcmp(argv[1], "png-fast16-encode")) fail("Usage: ffmpeg-png-fast16-warm png-fast16-encode INPUT WIDTH HEIGHT 8|16 OUTPUT");
    Work work = {0}; work.encode_rgb = !strcmp(argv[1], "png-rgb-encode"); work.encode_gray = !strcmp(argv[1], "png-gray-encode");
    work.encode_gray_alpha = !strcmp(argv[1], "png-gray-alpha-encode");
    work.encode_stored = !strcmp(argv[1], "png-store-encode");
    work.encode = !strcmp(argv[1], "png-fast16-encode") || work.encode_stored || work.encode_rgb || work.encode_gray || work.encode_gray_alpha; work.cpu = affinity_cpu();
    int interlace = !strcmp(argv[1], "png-adam7-decode");
    if (argc != (work.encode ? 7 : 8)) fail("PNG argument count differs.");
    uint16_t endian = 1; if (*(uint8_t *)&endian != 1) fail("This reference requires a little-endian host.");
    av_log_set_level(AV_LOG_ERROR);
    work.width = number(argv[3]); work.height = number(argv[4]); work.depth = number(argv[5]);
    if (work.depth != 8 && work.depth != 16) fail("Fastest PNG reference requires RGBA8/16 input.");
    if (work.encode_stored && work.depth != 8) fail("Stored PNG encode reference requires RGBA8 input.");
    work.row_bytes = work.width * work.depth / 8 * 4; work.raw_bytes = (size_t)work.row_bytes * work.height;
    if (work.raw_bytes > 512U * 1024U * 1024U) fail("Reference raster exceeds 512 MiB.");
    size_t original_bytes;
    work.original = read_file(work.encode ? argv[2] : argv[7], &original_bytes);
    if (original_bytes != work.raw_bytes) fail("Original raster size differs.");
    work.codec = context(work.encode, work.width, work.height, work.depth, work.encode_gray_alpha ? 4 : work.encode_gray ? 0 : work.encode_rgb ? 2 : 6, work.encode_stored ? 0 : 1);
    work.packet = av_packet_alloc(); work.input = av_packet_alloc();
    work.decoded = av_frame_alloc(); work.converted = av_frame_alloc();
    if (!work.packet || !work.input || !work.decoded || !work.converted) fail("Allocate wrappers failed.");
    if (work.encode) {
        work.source = frame(work.width, work.height, work.depth == 8 ? AV_PIX_FMT_RGBA : AV_PIX_FMT_RGBA64LE);
        for (int y = 0; y < work.height; y++) memcpy(work.source->data[0] + (size_t)y * work.source->linesize[0],
            work.original + (size_t)y * work.row_bytes, (size_t)work.row_bytes);
        if (work.depth == 16 && !work.encode_rgb && !work.encode_gray && !work.encode_gray_alpha) work.prepared = frame(work.width, work.height, AV_PIX_FMT_RGBA64BE);
        if (work.encode_rgb || work.encode_gray || work.encode_gray_alpha) {
            int sample_bytes = work.depth / 8;
            if (!work.encode_gray_alpha)
                for (size_t i = 3U * sample_bytes; i < work.raw_bytes; i += 4U * sample_bytes)
                    for (int j = 0; j < sample_bytes; j++)
                        if (work.original[i+j] != 255) fail("RGB encode input requires opaque pixels.");
            if (work.encode_gray || work.encode_gray_alpha)
                for (size_t i = 0; i < work.raw_bytes; i += 4U * sample_bytes)
                    if (memcmp(work.original+i, work.original+i+sample_bytes, (size_t)sample_bytes)
                        || memcmp(work.original+i, work.original+i+2*sample_bytes, (size_t)sample_bytes))
                        fail("Gray encode input requires equal RGB samples.");
            enum AVPixelFormat output_format = work.encode_gray_alpha ? (work.depth == 8 ? AV_PIX_FMT_YA8 : AV_PIX_FMT_YA16BE) : work.encode_gray ? (work.depth == 8 ? AV_PIX_FMT_GRAY8 : AV_PIX_FMT_GRAY16BE)
                : work.depth == 8 ? AV_PIX_FMT_RGB24 : AV_PIX_FMT_RGB48BE;
            if (work.encode_gray_alpha) {
                work.prepared = frame(work.width, work.height, output_format);
            } else if (work.encode_gray && work.depth == 16) {
                work.prepared = av_frame_alloc(); if (!work.prepared) fail("Allocate gray16 frame wrapper failed.");
                gray16_graph(&work);
            } else {
                work.prepared = frame(work.width, work.height, output_format);
                work.conversion = sws_alloc_context(); if (!work.conversion) fail("Allocate RGB encode conversion failed.");
                require(av_opt_set_int(work.conversion, "srcw", work.width, 0), "RGB encode source width");
                require(av_opt_set_int(work.conversion, "srch", work.height, 0), "RGB encode source height");
                require(av_opt_set_int(work.conversion, "dstw", work.width, 0), "RGB encode output width");
                require(av_opt_set_int(work.conversion, "dsth", work.height, 0), "RGB encode output height");
                require(av_opt_set_int(work.conversion, "src_format", work.depth == 8 ? AV_PIX_FMT_RGBA : AV_PIX_FMT_RGBA64LE, 0), "RGB encode source format");
                require(av_opt_set_int(work.conversion, "dst_format", output_format, 0), "PNG encode output format");
                if (work.encode_gray || work.encode_gray_alpha) {
                    require(av_opt_set_int(work.conversion, "src_range", 1, 0), "Gray encode source range");
                    require(av_opt_set_int(work.conversion, "dst_range", 1, 0), "Gray encode output range");
                }
                require(av_opt_set_int(work.conversion, "sws_flags", SWS_POINT | SWS_BITEXACT, 0), "RGB encode conversion policy");
                require(av_opt_set_int(work.conversion, "threads", 1, 0), "RGB encode conversion thread count");
                require(sws_init_context(work.conversion, NULL, NULL), "Initialize RGB encode conversion");
                int64_t threads; require(av_opt_get_int(work.conversion, "threads", 0, &threads), "Read RGB encode conversion threads");
                if (threads != 1) fail("RGB encode conversion threading differs.");
            }
        }
    } else {
        work.encoded = read_file(argv[2], &work.encoded_bytes);
        work.rgb = work.encoded_bytes >= 33 && work.encoded[25] == 2;
        work.gray = work.encoded_bytes >= 33 && work.encoded[25] == 0;
        work.gray_alpha = work.encoded_bytes >= 33 && work.encoded[25] == 4;
        work.palette = work.encoded_bytes >= 33 && work.encoded[25] == 3;
        work.coded_depth = work.encoded_bytes >= 33 ? work.encoded[24] : 0;
        if (work.gray) {
            for (size_t offset = 8; offset < work.encoded_bytes;) {
                if (work.encoded_bytes-offset < 12) fail("Truncated PNG chunk.");
                uint32_t length = big32(work.encoded+offset);
                if ((uint64_t)length+12 > work.encoded_bytes-offset) fail("PNG chunk exceeds input.");
                if (!memcmp(work.encoded+offset+4,"tRNS",4)) {
                    if (length != 2 || work.gray_transparency || work.coded_depth < 8) fail("Gray tRNS worker requires one two-byte key and gray8/16.");
                    work.gray_transparency = 1;
                }
                offset += (size_t)length+12;
            }
        }
        if (work.rgb) {
            for (size_t offset = 8; offset < work.encoded_bytes;) {
                if (work.encoded_bytes-offset < 12) fail("Truncated PNG chunk.");
                uint32_t length = big32(work.encoded+offset);
                if ((uint64_t)length+12 > work.encoded_bytes-offset) fail("PNG chunk exceeds input.");
                if (!memcmp(work.encoded+offset+4,"tRNS",4)) {
                    if (length != 6 || work.rgb_transparency || work.coded_depth != work.depth) fail("RGB tRNS worker requires one six-byte key and RGB8/16.");
                    work.rgb_transparency = 1;
                }
                offset += (size_t)length+12;
            }
        }
        work.mono = work.gray && work.coded_depth == 1;
        if (work.gray && work.coded_depth < 8 && (work.depth != 8 || (work.coded_depth != 1 && work.coded_depth != 2 && work.coded_depth != 4))) fail("Packed gray requires gray1/2/4 to RGBA8.");
        if (work.gray && work.coded_depth >= 8 && work.coded_depth != work.depth) fail("Gray coded/output precision differs.");
        if (work.mono && work.depth != 8) fail("Packed gray1 requires RGBA8 output.");
        if (work.rgb && interlace) fail("RGB decode requires noninterlaced RGB8/16.");
        if (work.gray && interlace) fail("Gray decode requires noninterlaced gray8/16.");
        if (work.gray_alpha && interlace) fail("Gray-alpha decode requires noninterlaced gray-alpha8/16.");
        if (work.palette && (work.depth != 8 || (work.coded_depth != 1 && work.coded_depth != 2 && work.coded_depth != 4 && work.coded_depth != 8) || interlace)) fail("Palette decode requires noninterlaced palette1/2/4/8 to RGBA8.");
        check_profile(work.encoded, work.encoded_bytes, work.width, work.height, work.gray || work.palette ? work.coded_depth : work.depth, interlace, work.palette ? 3 : work.gray_alpha ? 4 : work.gray ? 0 : work.rgb ? 2 : 6);
        if ((work.rgb && !work.rgb_transparency) || work.gray || work.gray_alpha || work.palette) {
            work.conversion = sws_alloc_context(); if (!work.conversion) fail("Allocate RGB conversion failed.");
            require(av_opt_set_int(work.conversion, "srcw", work.width, 0), "Source width");
            require(av_opt_set_int(work.conversion, "srch", work.height, 0), "Source height");
            require(av_opt_set_int(work.conversion, "dstw", work.width, 0), "Output width");
            require(av_opt_set_int(work.conversion, "dsth", work.height, 0), "Output height");
            require(av_opt_set_int(work.conversion, "src_format", work.mono ? AV_PIX_FMT_MONOBLACK : work.palette ? AV_PIX_FMT_PAL8 : (work.gray_alpha || work.gray_transparency) ? (work.depth == 8 ? AV_PIX_FMT_YA8 : AV_PIX_FMT_YA16BE) : work.gray ? (work.depth == 8 ? AV_PIX_FMT_GRAY8 : AV_PIX_FMT_GRAY16BE) : work.depth == 8 ? AV_PIX_FMT_RGB24 : AV_PIX_FMT_RGB48BE, 0), "Source format");
            require(av_opt_set_int(work.conversion, "dst_format", work.depth == 8 ? AV_PIX_FMT_RGBA : AV_PIX_FMT_RGBA64LE, 0), "Output format");
            require(av_opt_set_int(work.conversion, "sws_flags", SWS_POINT | SWS_BITEXACT, 0), "Conversion policy");
            require(av_opt_set_int(work.conversion, "threads", 1, 0), "Conversion thread count");
            require(sws_init_context(work.conversion, NULL, NULL), "Initialize RGB conversion");
            int64_t threads; require(av_opt_get_int(work.conversion, "threads", 0, &threads), "Read conversion threads");
            if (threads != 1) fail("Conversion threading differs.");
        }
        require(av_new_packet(work.input, (int)work.encoded_bytes), "Allocate encoded input with decoder padding");
        work.input->flags |= AV_PKT_FLAG_KEY;
        memcpy(work.input->data, work.encoded, work.encoded_bytes);
    }
    check_independent_owners(&work); check_and_write(&work, argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"%s\",\"width\":%d,\"height\":%d,\"depth\":%d,\"codedDepth\":%d,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"compressionLevel\":%d,\"filter\":%d,\"colorType\":%d,\"conversionThreads\":%d,\"rgbExpansionTimed\":%s,\"rgbTransparencyExpansionTimed\":%s,\"rgbPackingTimed\":%s,\"grayExpansionTimed\":%s,\"grayTransparencyExpansionTimed\":%s,\"grayPackingTimed\":%s,\"grayPlaneExtractionTimed\":%s,\"grayFilterOutputAllocationTimed\":%s,\"grayAlphaExpansionTimed\":%s,\"grayAlphaPackingTimed\":%s,\"grayAlphaStoragePackingTimed\":%s,\"paletteExpansionTimed\":%s,\"outputBytes\":%d,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u,\"avutilVersion\":%u}\n",
        argv[1], work.width, work.height, work.depth, work.encode ? work.depth : work.coded_depth, work.cpu, work.encode ? (work.encode_stored ? 0 : 1) : -1, work.encode ? 0 : -1,
        work.encode_gray_alpha ? 4 : work.encode_gray ? 0 : work.encode_rgb ? 2 : work.palette ? 3 : work.gray_alpha ? 4 : work.gray ? 0 : work.rgb ? 2 : 6, work.encode_gray || work.encode_rgb || (work.rgb && !work.rgb_transparency) || work.gray || work.gray_alpha || work.palette ? 1 : 0, work.rgb && !work.rgb_transparency ? "true" : "false", work.rgb_transparency ? "true" : "false", work.encode_rgb ? "true" : "false", work.gray ? "true" : "false", work.gray_transparency ? "true" : "false", work.encode_gray ? "true" : "false", work.gray_graph ? "true" : "false", work.gray_graph ? "true" : "false", work.gray_alpha ? "true" : "false", work.encode_gray_alpha ? "true" : "false", work.encode_gray_alpha ? "true" : "false", work.palette ? "true" : "false", work.output_bytes, work.completed, av_version_info(), avcodec_version(), avutil_version()); fflush(stdout);
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
        uint64_t elapsed = now_ns() - start;
        if (sched_getcpu() != work.cpu || affinity_cpu() != work.cpu || work.completed - completed != count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"outputBytes\":%d,\"cpu\":%d}\n",
            count, elapsed, work.completed, work.observed, work.stride, work.output_bytes, work.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_frame_free(&work.source); av_frame_free(&work.prepared); av_frame_free(&work.decoded); av_frame_free(&work.converted);
    av_packet_free(&work.input); av_packet_free(&work.packet); avcodec_free_context(&work.codec);
    sws_freeContext(work.conversion); avfilter_graph_free(&work.gray_graph); free(work.original); free(work.encoded); return 0;
}
