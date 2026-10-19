/* Original development-only direct RGB24/output adapter. JPEG framing follows
 * T.81 B.2. No RGBA frame, alpha flattening or copied upstream implementation. */
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
    uint8_t *data = calloc(size+AV_INPUT_BUFFER_PADDING_SIZE,1); if (!data) fail("Allocate raster failed.");
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
    AVCodecContext *codec; struct SwsContext *conversion; AVFrame *yuv; AVPacket *packet;
    uint8_t *input,*original,*expected; size_t input_bytes; int cpu;
    uint64_t completed,observed; unsigned writes,growths; size_t capacity;
} Work;
static Output apply(Work *w)
{
    const uint8_t *planes[4]={w->input,NULL,NULL,NULL}; int strides[4]={513*3,0,0,0};
    if (sws_scale(w->conversion,planes,strides,0,259,w->yuv->data,w->yuv->linesize)!=259) fail("RGB420 conversion incomplete.");
    w->yuv->pts=(int64_t)w->completed; w->yuv->pict_type=AV_PICTURE_TYPE_I; w->yuv->quality=w->codec->global_quality;
    require(avcodec_send_frame(w->codec,w->yuv),"Submit RGB420 source"); require(avcodec_receive_packet(w->codec,w->packet),"Receive complete JPEG");
    if (!w->packet->buf || !(w->packet->flags&AV_PKT_FLAG_KEY) || w->packet->size!=52093) fail("Frozen JPEG size/owner differs.");
    Output out={0}; uint8_t *scratch=av_malloc(8192); if (!scratch) fail("AVIO scratch allocation failed.");
    AVIOContext *io=avio_alloc_context(scratch,8192,1,&out,NULL,write_output,NULL); if (!io) fail("Fresh AVIO allocation failed.");
    io->seekable=0;
    for (int at=0; at<w->packet->size;) {
        int count=w->packet->size-at; if (count>65536) count=65536; avio_write(io,w->packet->data+at,count); at+=count;
    }
    avio_flush(io); require(io->error,"Deliver complete JPEG");
    if (io->seek || io->seekable || !out.writes || !out.growths || out.length!=52093 || out.bytes==w->packet->data) fail("Nonseekable output owner/delivery differs.");
    w->writes=out.writes; w->growths=out.growths; w->capacity=out.capacity;
    av_freep(&io->buffer); avio_context_free(&io); av_packet_unref(w->packet);
    w->observed+=out.bytes[out.length/2]; w->completed++; return out;
}
static void check_write(Work *w, const char *path)
{
    Output out=apply(w);
    if (out.length!=52093 || memcmp(out.bytes,w->expected,out.length) || memcmp(w->input,w->original,w->input_bytes)) fail("Frozen output/input differs.");
    FILE *file=fopen(path,"wb"); if (!file || fwrite(out.bytes,1,out.length,file)!=out.length || fclose(file)) fail("Write JPEG failed."); av_free(out.bytes);
}
int main(int argc, char **argv)
{
    if (argc!=7 || strcmp(argv[1],"jpeg-samples-nonseekable-encode") || number(argv[3])!=513 || number(argv[4])!=259)
        fail("Use jpeg-samples-nonseekable-encode RGB24 513 259 OUTPUT FROZEN_NATIVE.");
    Work w={0}; w.cpu=affinity_cpu(); w.input_bytes=513U*259*3; w.input=read_file(argv[2],w.input_bytes);
    w.original=av_memdup(w.input,w.input_bytes); w.expected=read_file(argv[6],52093); if (!w.original) fail("Source witness allocation failed.");
    av_log_set_level(AV_LOG_ERROR); const AVCodec *codec=avcodec_find_encoder(AV_CODEC_ID_MJPEG); if (!codec) fail("MJPEG encoder unavailable.");
    w.codec=avcodec_alloc_context3(codec); if (!w.codec) fail("Codec allocation failed.");
    w.codec->thread_count=1; w.codec->thread_type=0; w.codec->width=513; w.codec->height=259;
    w.codec->time_base=(AVRational){1,1}; w.codec->pix_fmt=AV_PIX_FMT_YUVJ420P; w.codec->color_range=AVCOL_RANGE_JPEG;
    w.codec->colorspace=AVCOL_SPC_SMPTE170M; w.codec->gop_size=1; w.codec->flags|=AV_CODEC_FLAG_QSCALE; w.codec->global_quality=5*FF_QP2LAMBDA;
    require(av_opt_set(w.codec->priv_data,"huffman","optimal",0),"Select optimal Huffman"); require(avcodec_open2(w.codec,codec,NULL),"Open encoder");
    if (w.codec->thread_count!=1 || w.codec->thread_type || w.codec->active_thread_type) fail("Codec threading differs.");
    w.yuv=frame(513,259,AV_PIX_FMT_YUVJ420P); w.packet=av_packet_alloc(); if (!w.packet) fail("Packet wrapper allocation failed.");
    w.conversion=sws_alloc_context(); if (!w.conversion) fail("Conversion allocation failed.");
    require(av_opt_set_int(w.conversion,"srcw",513,0),"Source width"); require(av_opt_set_int(w.conversion,"srch",259,0),"Source height");
    require(av_opt_set_int(w.conversion,"dstw",513,0),"Target width"); require(av_opt_set_int(w.conversion,"dsth",259,0),"Target height");
    require(av_opt_set_int(w.conversion,"src_format",AV_PIX_FMT_RGB24,0),"RGB24 source"); require(av_opt_set_int(w.conversion,"dst_format",AV_PIX_FMT_YUVJ420P,0),"YCbCr420 target");
    require(av_opt_set_int(w.conversion,"sws_flags",SWS_AREA|SWS_ACCURATE_RND|SWS_BITEXACT,0),"Conversion flags"); require(av_opt_set_int(w.conversion,"threads",1,0),"Conversion threads");
    require(sws_init_context(w.conversion,NULL,NULL),"Initialize conversion"); const int *matrix=sws_getCoefficients(SWS_CS_ITU601);
    require(sws_setColorspaceDetails(w.conversion,matrix,1,matrix,1,0,1<<16,1<<16),"Full-range BT601");
    int64_t threads; require(av_opt_get_int(w.conversion,"threads",0,&threads),"Read conversion threads"); if (threads!=1) fail("Conversion threading differs.");
    Output first=apply(&w),second=apply(&w);
    if (first.bytes==second.bytes || first.length!=second.length || memcmp(first.bytes,w.expected,first.length) || memcmp(second.bytes,w.expected,second.length)) fail("Independent outputs differ.");
    first.bytes[0]^=255; if (memcmp(second.bytes,w.expected,second.length)) fail("Peer output changed."); av_free(first.bytes);
    if (memcmp(second.bytes,w.expected,second.length)) fail("Surviving output changed.");
    av_free(second.bytes); check_write(&w,argv[5]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"jpeg-samples-nonseekable-encode\",\"width\":513,\"height\":259,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"conversionThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"nativeQuality\":5,\"sampling\":\"420\",\"rawInputChannels\":3,\"rawInputBorrowed\":true,\"rgbaIntermediate\":false,\"colorConversionTimed\":true,\"outputBytes\":52093,\"initialCompletedOperations\":%" PRIu64 ",\"freshNonSeekableOutput\":true,\"seekCallback\":false,\"lengthQueries\":0,\"outputView\":false,\"outputGrowthTimed\":true,\"outputCopyTimed\":true,\"encodedStagingTimed\":true,\"internalAvioFlushTimed\":true,\"streamAcquisitionAndReleaseTimed\":true,\"residentCallbacksImmediate\":true,\"callerOutputLeftOpen\":true,\"independentLiveOutputs\":true,\"independentOutputFromStaging\":true,\"writes\":%u,\"growths\":%u,\"outputCapacity\":%zu,\"ffmpegVersion\":\"%s\"}\n",
        w.cpu,w.completed,w.writes,w.growths,w.capacity,av_version_info()); fflush(stdout);
    char line[128];
    while (fgets(line,sizeof(line),stdin)) {
        if (!strcmp(line,"quit\n")) break;
        if (!strcmp(line,"check\n")) { check_write(&w,argv[5]); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue; }
        uint64_t count; char extra;
        if (sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1 || !count || count>10000000) fail("Invalid sample count.");
        uint64_t before=w.completed,start=now_ns(); for (uint64_t i=0; i<count; i++) { Output out=apply(&w); av_free(out.bytes); }
        uint64_t elapsed=now_ns()-start; if (affinity_cpu()!=w.cpu || sched_getcpu()!=w.cpu || w.completed-before!=count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"cpu\":%d,\"outputBytes\":52093}\n",count,elapsed,w.completed,w.observed,w.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_packet_free(&w.packet); av_frame_free(&w.yuv); avcodec_free_context(&w.codec); sws_freeContext(w.conversion);
    av_free(w.input); av_free(w.original); av_free(w.expected); return 0;
}
