/* Original benchmark-only public-API adapter. JPEG framing follows ITU T.81.
 * Resident JPEG -> fresh caller-owned packed GRAY8, without an RGBA frame. */
#define _GNU_SOURCE
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <libavcodec/avcodec.h>
#include <libavutil/avutil.h>

static void fail(const char *message) { fprintf(stderr,"%s\n",message); exit(1); }
static void require(int code,const char *message)
{
    if (code<0) { char error[AV_ERROR_MAX_STRING_SIZE]; av_strerror(code,error,sizeof(error));
        fprintf(stderr,"%s: %s\n",message,error); exit(1); }
}
static int affinity_cpu(void)
{
    cpu_set_t mask;
    if (sched_getaffinity(0,sizeof(mask),&mask) || CPU_COUNT(&mask)!=1) fail("Exactly one CPU required.");
    for (int cpu=0;cpu<CPU_SETSIZE;cpu++) if (CPU_ISSET(cpu,&mask)) return cpu;
    fail("No affinity CPU."); return -1;
}
static uint64_t now_ns(void)
{
    struct timespec t; if (clock_gettime(CLOCK_MONOTONIC,&t)) fail("Clock failed.");
    return (uint64_t)t.tv_sec*UINT64_C(1000000000)+(uint64_t)t.tv_nsec;
}
typedef struct {
    AVCodecContext *codec;
    AVFrame *decoded;
    AVPacket *input;
    uint8_t *source,*output,*expected;
    size_t encoded_bytes,raw_bytes;
    int width,height,cpu;
    uint64_t completed,observed;
} Work;
static void release(Work *w) { free(w->output); w->output=NULL; }
static void apply(Work *w)
{
    /* Match new byte[] acquisition, including zero initialization, inside the
     * operation. Decoder planes and the independently surviving caller output
     * are separate owners; row packing/copying is never omitted. */
    w->output=calloc(w->raw_bytes,1); if (!w->output) fail("Allocate caller output failed.");
    avcodec_flush_buffers(w->codec);
    require(avcodec_send_packet(w->codec,w->input),"Submit complete JPEG");
    require(avcodec_receive_frame(w->codec,w->decoded),"Receive JPEG samples");
    AVFrame *d=w->decoded;
    if (d->width!=w->width || d->height!=w->height || d->format!=AV_PIX_FMT_GRAY8
        || !d->buf[0] || d->linesize[0]<w->width
        || (d->color_range!=AVCOL_RANGE_JPEG && d->color_range!=AVCOL_RANGE_UNSPECIFIED))
        fail("Expected owned full-range grayscale decoder planes.");
    for (int y=0;y<w->height;y++)
        memcpy(w->output+(size_t)y*w->width,d->data[0]+(size_t)y*d->linesize[0],(size_t)w->width);
    av_frame_unref(d);
    w->observed+=w->output[w->raw_bytes/2]; w->completed++;
}
static void check_pixels(const Work *w,const uint8_t *pixels)
{
    if (!pixels || memcmp(pixels,w->expected,w->raw_bytes)) fail("Repeated gray samples differ.");
}
static void check_and_write(Work *w,const char *path)
{
    apply(w); check_pixels(w,w->output);
    if (w->input->size!=(int)w->encoded_bytes || memcmp(w->input->data,w->source,w->encoded_bytes))
        fail("Resident JPEG source changed.");
    FILE *file=fopen(path,"wb"); if (!file) fail("Open gray output failed.");
    if (fwrite(w->output,1,w->raw_bytes,file)!=w->raw_bytes || fclose(file)) fail("Write gray output failed.");
    release(w);
}
int main(int argc,char **argv)
{
    if (argc!=4 || strcmp(argv[1],"jpeg-gray-samples-decode"))
        fail("Usage: ffmpeg-jpeg-gray-samples-warm jpeg-gray-samples-decode INPUT OUTPUT");
    Work w={0}; w.cpu=affinity_cpu(); w.width=513; w.height=259; w.raw_bytes=(size_t)w.width*w.height;
    FILE *file=fopen(argv[2],"rb"); if (!file) fail("Open JPEG failed.");
    if (fseek(file,0,SEEK_END)) fail("JPEG seek failed.");
    long length=ftell(file);
    if (length<4 || length>512L*1024*1024 || fseek(file,0,SEEK_SET)) fail("Invalid bounded JPEG length.");
    w.encoded_bytes=(size_t)length; w.source=malloc(w.encoded_bytes); if (!w.source) fail("Allocate source failed.");
    if (fread(w.source,1,w.encoded_bytes,file)!=w.encoded_bytes || ferror(file) || fclose(file)) fail("Read JPEG failed.");
    if (w.source[0]!=255 || w.source[1]!=216 || w.source[length-2]!=255 || w.source[length-1]!=217) fail("Complete JPEG required.");
    av_log_set_level(AV_LOG_ERROR);
    const AVCodec *codec=avcodec_find_decoder_by_name("mjpeg");
    if (!codec || codec->id!=AV_CODEC_ID_MJPEG) fail("Software MJPEG decoder missing.");
    w.codec=avcodec_alloc_context3(codec); w.input=av_packet_alloc(); w.decoded=av_frame_alloc();
    if (!w.codec || !w.input || !w.decoded) fail("Allocate codec wrappers failed.");
    w.codec->thread_count=1; w.codec->thread_type=0; w.codec->max_pixels=(int64_t)(w.width+255)*(w.height+64);
    require(avcodec_open2(w.codec,codec,NULL),"Open decoder");
    if (w.codec->thread_count!=1 || w.codec->thread_type!=0 || w.codec->active_thread_type!=0) fail("Decoder threading differs.");
    require(av_new_packet(w.input,(int)w.encoded_bytes),"Allocate padded resident packet");
    memcpy(w.input->data,w.source,w.encoded_bytes); w.input->flags|=AV_PKT_FLAG_KEY;
    apply(&w); w.expected=malloc(w.raw_bytes); if (!w.expected) fail("Allocate untimed validation pixels failed.");
    memcpy(w.expected,w.output,w.raw_bytes);
    uint8_t *held=w.output; w.output=NULL; apply(&w);
    if (held==w.output) fail("Live caller outputs alias.");
    check_pixels(&w,held); check_pixels(&w,w.output); held[0]^=255;
    check_pixels(&w,w.output); free(held); check_pixels(&w,w.output); release(&w);
    check_and_write(&w,argv[3]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"jpeg-gray-samples-decode\",\"width\":513,\"height\":259,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"decoder\":\"mjpeg\",\"decodedPixelFormat\":\"gray8\",\"outputBytes\":%zu,\"outputStride\":513,\"initialCompletedOperations\":%" PRIu64 ",\"freshZeroedCallerOutputTimed\":true,\"packingTimed\":true,\"stateResetTimed\":true,\"decoderPlanesReleaseTimed\":true,\"outputAcquisitionAndReleaseTimed\":true,\"independentLiveOutputs\":true,\"pixelsDeterministic\":true,\"rgbaIntermediate\":false,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u}\n",
        w.cpu,w.raw_bytes,w.completed,av_version_info(),avcodec_version()); fflush(stdout);
    char line[128];
    while (fgets(line,sizeof(line),stdin)) {
        if (!strcmp(line,"quit\n")) break;
        if (!strcmp(line,"check\n")) {
            check_and_write(&w,argv[3]); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if (sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1 || !count || count>UINT64_C(10000000)) fail("Invalid sample request.");
        uint64_t start=now_ns(),completed=w.completed;
        for (uint64_t i=0;i<count;i++) { apply(&w); release(&w); }
        uint64_t elapsed=now_ns()-start;
        if (sched_getcpu()!=w.cpu || affinity_cpu()!=w.cpu || w.completed-completed!=count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":513,\"outputBytes\":%zu,\"cpu\":%d}\n",
            count,elapsed,w.completed,w.observed,w.raw_bytes,w.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    release(&w); av_packet_free(&w.input); av_frame_free(&w.decoded); avcodec_free_context(&w.codec);
    free(w.source); free(w.expected); return 0;
}
