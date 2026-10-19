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
#include <libavformat/avio.h>
#include <libavutil/cpu.h>
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
typedef struct { uint8_t *bytes; size_t length, capacity; unsigned writes, growths; } Output;
static int write_output(void *opaque, const uint8_t *bytes, int count)
{
    Output *out=opaque;
    if (count<0 || (size_t)count>64U*1024*1024-out->length) return AVERROR(ENOMEM);
    size_t needed=out->length+(size_t)count;
    if (needed>out->capacity) {
        size_t next=out->capacity ? out->capacity*2 : 256;
        if (next<needed) next=needed;
        uint8_t *grown=av_realloc(out->bytes,next); if (!grown) return AVERROR(ENOMEM);
        out->bytes=grown; out->capacity=next; out->growths++;
    }
    memcpy(out->bytes+out->length,bytes,(size_t)count); out->length=needed; out->writes++; return count;
}
typedef struct {
    AVCodecContext *codec;
    struct SwsContext *conversion;
    AVFrame *rgba, *flattened, *yuv;
    AVPacket *packet;
    uint8_t *original, *expected, *normalized_expected;
    size_t raw_bytes;
    int width, height, depth, cpu, quality, output_bytes, expected_bytes;
    uint64_t completed, observed;
    unsigned writes,growths; size_t capacity;
} Work;
static Output apply(Work *work)
{
    /* Exact single-round flattening/normalization to RGB 0x204060. Scratch frames and
     * contexts are reused; both transforms execute again for every operation. */
    const int background[3] = {32,64,96};
    if (work->depth == 16) {
        const uint64_t maximum = 65535, denominator = maximum * maximum;
        for (int y = 0; y < work->height; y++) {
            const uint8_t *source = work->rgba->data[0] + (size_t)y * work->rgba->linesize[0];
            uint8_t *target = work->flattened->data[0] + (size_t)y * work->flattened->linesize[0];
            for (int x = 0; x < work->width; x++) {
                const uint8_t *pixel = source + (size_t)x * 8;
                uint64_t alpha = (uint64_t)pixel[6] | (uint64_t)pixel[7] << 8;
                for (int c = 0; c < 3; c++) {
                    uint64_t value = (uint64_t)pixel[c*2] | (uint64_t)pixel[c*2+1] << 8;
                    uint64_t numerator = value * alpha * 255 + (uint64_t)background[c] * (maximum-alpha) * maximum;
                    target[x*4+c] = (uint8_t)((numerator + denominator/2) / denominator);
                }
                target[x*4+3] = 255;
            }
        }
    } else
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
    Output out={0}; uint8_t *scratch=av_malloc(8192); if (!scratch) fail("AVIO scratch allocation failed.");
    AVIOContext *io=avio_alloc_context(scratch,8192,1,&out,NULL,write_output,NULL);
    if (!io) fail("Fresh AVIO output allocation failed.");
    io->seekable=0;
    for (int offset=0; offset<packet->size;) {
        int count=packet->size-offset; if (count>65536) count=65536;
        avio_write(io,packet->data+offset,count); offset+=count;
    }
    avio_flush(io); require(io->error,"Deliver complete JPEG");
    if (io->seek || io->seekable || !out.writes || !out.growths || out.length!=(size_t)packet->size || out.bytes==packet->data)
        fail("Nonseekable output ownership/delivery differs.");
    work->writes=out.writes; work->growths=out.growths; work->capacity=out.capacity;
    av_freep(&io->buffer); avio_context_free(&io); av_packet_unref(packet);
    work->output_bytes=(int)out.length; work->observed+=out.bytes[out.length/2]; work->completed++;
    return out;
}
static void check_source(const Work *work)
{
    for (int y = 0; y < work->height; y++)
        if (memcmp(work->rgba->data[0] + (size_t)y * work->rgba->linesize[0],
                   work->original + (size_t)y * work->width*4*(work->depth/8), (size_t)work->width*4*(work->depth/8))) fail("Source changed.");
    if (work->normalized_expected)
        for (int y = 0; y < work->height; y++)
            if (memcmp(work->flattened->data[0] + (size_t)y * work->flattened->linesize[0],
                       work->normalized_expected + (size_t)y * work->width*4, (size_t)work->width*4)) fail("Full-source normalized reference differs.");
}
static void check_write(Work *work, const char *path)
{
    Output out=apply(work); check_source(work);
    if (out.length!=(size_t)work->expected_bytes || memcmp(out.bytes,work->expected,out.length)) fail("Repeated JPEG bytes differ.");
    FILE *file=fopen(path,"wb");
    if (!file || fwrite(out.bytes,1,out.length,file)!=out.length || fclose(file)) fail("Write JPEG failed.");
    av_free(out.bytes);
}
int main(int argc, char **argv)
{
    if (argc!=9 || strcmp(argv[1],"jpeg-nonseekable-encode")) fail("Use jpeg-nonseekable-encode INPUT 513 259 8 OUTPUT 5 NORMALIZED_RGBA.");
    Work work={0}; work.cpu=affinity_cpu(); work.width=number(argv[3]); work.height=number(argv[4]); work.depth=number(argv[5]); work.quality=number(argv[7]);
    if (work.width!=513 || work.height!=259 || work.depth!=8 || work.quality!=5) fail("Only the frozen RGBA8/444/qscale5 anchor is timed.");
    const char *sampling="444"; enum AVPixelFormat yuv_format=AV_PIX_FMT_YUVJ444P;
    work.raw_bytes = (size_t)work.width * work.height * 4 * (work.depth/8);
    if (work.raw_bytes > 512U*1024U*1024U) fail("Reference raster exceeds 512 MiB.");
    av_log_set_level(AV_LOG_ERROR); work.original = read_file(argv[2], work.raw_bytes);
    work.normalized_expected = read_file(argv[8], (size_t)work.width * work.height * 4);
    work.rgba = frame(work.width, work.height, work.depth == 16 ? AV_PIX_FMT_RGBA64LE : AV_PIX_FMT_RGBA);
    work.flattened = frame(work.width, work.height, AV_PIX_FMT_RGBA);
    work.yuv = frame(work.width, work.height, yuv_format);
    for (int y = 0; y < work.height; y++) memcpy(work.rgba->data[0] + (size_t)y*work.rgba->linesize[0],
        work.original + (size_t)y*work.width*4*(work.depth/8), (size_t)work.width*4*(work.depth/8));
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

    Output first=apply(&work); work.expected_bytes=work.output_bytes; work.expected=av_memdup(first.bytes,first.length);
    if (!work.expected) fail("Expected packet allocation failed.");
    Output second=apply(&work);
    if (first.bytes==second.bytes || first.length!=second.length || memcmp(first.bytes,second.bytes,first.length)) fail("Live caller outputs alias/differ.");
    first.bytes[0]^=255; if (memcmp(second.bytes,work.expected,second.length)) fail("Peer output changed.");
    av_free(first.bytes); if (memcmp(second.bytes,work.expected,second.length)) fail("Surviving output changed.");
    av_free(second.bytes); check_write(&work,argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"jpeg-nonseekable-encode\",\"width\":513,\"height\":259,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"conversionThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"nativeQuality\":5,\"sampling\":\"%s\",\"backgroundRgb\":2113632,\"normalizationTimed\":true,\"colorConversionTimed\":true,\"outputBytes\":%d,\"initialCompletedOperations\":%" PRIu64 ",\"freshNonSeekableOutput\":true,\"seekCallback\":false,\"lengthQueries\":0,\"outputView\":false,\"outputGrowthTimed\":true,\"outputCopyTimed\":true,\"encodedStagingTimed\":true,\"internalAvioFlushTimed\":true,\"streamAcquisitionAndReleaseTimed\":true,\"residentCallbacksImmediate\":true,\"callerOutputLeftOpen\":true,\"independentLiveOutputs\":true,\"independentOutputFromStaging\":true,\"writes\":%u,\"growths\":%u,\"outputCapacity\":%zu,\"ffmpegVersion\":\"%s\"}\n",
        work.cpu,sampling,work.output_bytes,work.completed,work.writes,work.growths,work.capacity,av_version_info()); fflush(stdout);
    char line[128];
    while (fgets(line,sizeof(line),stdin)) {
        if (!strcmp(line,"quit\n")) break;
        if (!strcmp(line,"check\n")) { check_write(&work,argv[6]); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue; }
        uint64_t count; char extra;
        if (sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1 || !count || count>10000000) fail("Invalid sample count.");
        uint64_t before=work.completed,start=now_ns();
        for (uint64_t i=0; i<count; i++) { Output out=apply(&work); av_free(out.bytes); }
        uint64_t elapsed=now_ns()-start;
        if (affinity_cpu()!=work.cpu || sched_getcpu()!=work.cpu || work.completed-before!=count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"cpu\":%d,\"outputBytes\":%d}\n",count,elapsed,work.completed,work.observed,work.cpu,work.output_bytes); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_packet_free(&work.packet); av_frame_free(&work.rgba); av_frame_free(&work.flattened); av_frame_free(&work.yuv); sws_freeContext(work.conversion);
    avcodec_free_context(&work.codec); av_free(work.original); av_free(work.expected); av_free(work.normalized_expected); return 0;
}
