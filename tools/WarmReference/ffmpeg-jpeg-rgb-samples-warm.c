/* Original benchmark-only public-API adapter. JPEG framing follows ITU T.81.
 * Resident JPEG -> fresh caller-owned packed RGB24, without an RGBA frame. */
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
#include <libavutil/opt.h>
#include <libswscale/swscale.h>

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
    AVFrame *decoded,*u,*v;
    struct SwsContext *conversion,*chroma;
    AVPacket *input;
    uint8_t *source,*output,*expected;
    size_t encoded_bytes,raw_bytes;
    int width,height,cpu;
    uint64_t completed,observed;
} Work;
static struct SwsContext *conversion(int sw,int sh,int dw,int dh,int source,int target,int flags)
{
    struct SwsContext *c=sws_alloc_context(); if (!c) fail("Allocate conversion failed.");
    require(av_opt_set_int(c,"srcw",sw,0),"Set source width");
    require(av_opt_set_int(c,"srch",sh,0),"Set source height");
    require(av_opt_set_int(c,"dstw",dw,0),"Set output width");
    require(av_opt_set_int(c,"dsth",dh,0),"Set output height");
    require(av_opt_set_int(c,"src_format",source,0),"Set source format");
    require(av_opt_set_int(c,"dst_format",target,0),"Set output format");
    require(av_opt_set_int(c,"sws_flags",flags,0),"Set conversion flags");
    require(av_opt_set_int(c,"src_h_chr_pos",0,0),"Set chroma horizontal phase");
    require(av_opt_set_int(c,"src_v_chr_pos",0,0),"Set chroma vertical phase");
    require(av_opt_set_int(c,"threads",1,0),"Set conversion threads");
    require(sws_init_context(c,NULL,NULL),"Initialize conversion");
    int64_t threads; require(av_opt_get_int(c,"threads",0,&threads),"Read conversion threads");
    if (threads!=1) fail("Conversion threading differs.");
    return c;
}
static void initialize_conversion(Work *w)
{
    w->conversion=conversion(w->width,w->height,w->width,w->height,AV_PIX_FMT_YUVJ444P,
        AV_PIX_FMT_RGB24,SWS_BILINEAR|SWS_ACCURATE_RND|SWS_FULL_CHR_H_INT);
    const int *matrix=sws_getCoefficients(SWS_CS_ITU601);
    require(sws_setColorspaceDetails(w->conversion,matrix,1,matrix,1,0,1<<16,1<<16),"Set full-range BT.601");
    int cw=(w->width+1)/2,ch=(w->height+1)/2;
    w->chroma=conversion(cw,ch,cw*2,ch*2,AV_PIX_FMT_GRAY8,AV_PIX_FMT_GRAY8,SWS_BILINEAR|SWS_ACCURATE_RND);
}
static void release(Work *w) { free(w->output); w->output=NULL; }
static void apply(Work *w)
{
    /* Match new byte[] acquisition, including zero initialization, inside the
     * operation. Decoder planes and the independently surviving caller output
     * are separate owners; centered chroma expansion and BT.601 packing execute every operation.
     * The caller allocation includes FFmpeg output overrun padding after the
     * packed payload; it is not a second raster or an exposed row gap. */
    w->output=calloc(w->raw_bytes+AV_INPUT_BUFFER_PADDING_SIZE,1); if (!w->output) fail("Allocate caller output failed.");
    avcodec_flush_buffers(w->codec);
    require(avcodec_send_packet(w->codec,w->input),"Submit complete JPEG");
    require(avcodec_receive_frame(w->codec,w->decoded),"Receive JPEG samples");
    AVFrame *d=w->decoded;
    if (d->width!=w->width || d->height!=w->height || d->format!=AV_PIX_FMT_YUVJ420P
        || !d->buf[0] || d->linesize[0]<w->width
        || d->color_range!=AVCOL_RANGE_JPEG)
        fail("Expected owned full-range YUVJ420P decoder planes.");
    int cw=(w->width+1)/2,ch=(w->height+1)/2;
    w->u->width=w->v->width=cw*2; w->u->height=w->v->height=ch*2;
    w->u->format=w->v->format=AV_PIX_FMT_GRAY8;
    require(av_frame_get_buffer(w->u,32),"Allocate expanded U");
    require(av_frame_get_buffer(w->v,32),"Allocate expanded V");
    const uint8_t *up[4]={d->data[1],NULL,NULL,NULL},*vp[4]={d->data[2],NULL,NULL,NULL};
    int us[4]={d->linesize[1],0,0,0},vs[4]={d->linesize[2],0,0,0};
    if (sws_scale(w->chroma,up,us,0,ch,w->u->data,w->u->linesize)!=ch*2
        || sws_scale(w->chroma,vp,vs,0,ch,w->v->data,w->v->linesize)!=ch*2)
        fail("Centered chroma expansion incomplete.");
    const uint8_t *planes[4]={d->data[0],w->u->data[0],w->v->data[0],NULL};
    int strides[4]={d->linesize[0],w->u->linesize[0],w->v->linesize[0],0};
    uint8_t *output[4]={w->output,NULL,NULL,NULL}; int output_stride[4]={w->width*3,0,0,0};
    if (sws_scale(w->conversion,planes,strides,0,w->height,output,output_stride)!=w->height)
        fail("BT.601 RGB24 conversion/packing incomplete.");
    av_frame_unref(w->u); av_frame_unref(w->v);
    av_frame_unref(d);
    w->observed+=w->output[w->raw_bytes/2]; w->completed++;
}
static void check_pixels(const Work *w,const uint8_t *pixels)
{
    if (!pixels || memcmp(pixels,w->expected,w->raw_bytes)) fail("Repeated rgb samples differ.");
}
static void check_and_write(Work *w,const char *path)
{
    apply(w); check_pixels(w,w->output);
    if (w->input->size!=(int)w->encoded_bytes || memcmp(w->input->data,w->source,w->encoded_bytes))
        fail("Resident JPEG source changed.");
    FILE *file=fopen(path,"wb"); if (!file) fail("Open rgb output failed.");
    if (fwrite(w->output,1,w->raw_bytes,file)!=w->raw_bytes || fclose(file)) fail("Write rgb output failed.");
    release(w);
}
int main(int argc,char **argv)
{
    if (argc!=4 || strcmp(argv[1],"jpeg-rgb-samples-decode"))
        fail("Usage: ffmpeg-jpeg-rgb-samples-warm jpeg-rgb-samples-decode INPUT OUTPUT");
    Work w={0}; w.cpu=affinity_cpu(); w.width=513; w.height=259; w.raw_bytes=(size_t)w.width*w.height*3;
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
    w.codec=avcodec_alloc_context3(codec); w.input=av_packet_alloc(); w.decoded=av_frame_alloc(); w.u=av_frame_alloc(); w.v=av_frame_alloc();
    if (!w.codec || !w.input || !w.decoded || !w.u || !w.v) fail("Allocate codec wrappers failed.");
    w.codec->thread_count=1; w.codec->thread_type=0; w.codec->max_pixels=(int64_t)(w.width+255)*(w.height+64);
    require(avcodec_open2(w.codec,codec,NULL),"Open decoder");
    if (w.codec->thread_count!=1 || w.codec->thread_type!=0 || w.codec->active_thread_type!=0) fail("Decoder threading differs.");
    require(av_new_packet(w.input,(int)w.encoded_bytes),"Allocate padded resident packet");
    memcpy(w.input->data,w.source,w.encoded_bytes); w.input->flags|=AV_PKT_FLAG_KEY;
    initialize_conversion(&w);
    apply(&w); w.expected=malloc(w.raw_bytes); if (!w.expected) fail("Allocate untimed validation pixels failed.");
    memcpy(w.expected,w.output,w.raw_bytes);
    uint8_t *held=w.output; w.output=NULL; apply(&w);
    if (held==w.output) fail("Live caller outputs alias.");
    check_pixels(&w,held); check_pixels(&w,w.output); held[0]^=255;
    check_pixels(&w,w.output); free(held); check_pixels(&w,w.output); release(&w);
    check_and_write(&w,argv[3]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"jpeg-rgb-samples-decode\",\"width\":513,\"height\":259,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"decoder\":\"mjpeg\",\"decodedPixelFormat\":\"yuvj420p\",\"outputBytes\":%zu,\"outputStride\":1539,\"initialCompletedOperations\":%" PRIu64 ",\"freshZeroedCallerOutputTimed\":true,\"packingTimed\":true,\"chromaExpansionTimed\":true,\"colorConversionTimed\":true,\"stateResetTimed\":true,\"decoderPlanesReleaseTimed\":true,\"outputAcquisitionAndReleaseTimed\":true,\"independentLiveOutputs\":true,\"pixelsDeterministic\":true,\"rgbaIntermediate\":false,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u}\n",
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
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":1539,\"outputBytes\":%zu,\"cpu\":%d}\n",
            count,elapsed,w.completed,w.observed,w.raw_bytes,w.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    release(&w); av_packet_free(&w.input); av_frame_free(&w.decoded); av_frame_free(&w.u); av_frame_free(&w.v); avcodec_free_context(&w.codec);
    sws_freeContext(w.conversion); sws_freeContext(w.chroma);
    free(w.source); free(w.expected); return 0;
}
