/* Original benchmark-only public-API adapter. JPEG framing follows ITU T.81.
 * Resident H4V2 JPEG -> fresh independently owned RGBA8. Full-range centered
 * original quarter-width samples are recovered from even decoder positions.
 * Centered half-height interpolation precedes 516-column grid conversion and
 * in-place output compaction to the original odd width. No packet changes. */
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
    AVFrame *decoded;
    struct SwsContext *conversion;
    AVPacket *input;
    uint8_t *source,*output,*expected,*vertical;
    size_t encoded_bytes,raw_bytes;
    int width,height,cpu;
    uint64_t completed,observed;
} Work;
static void initialize_conversion(Work *w)
{
    w->conversion=sws_alloc_context(); if (!w->conversion) fail("Allocate conversion failed.");
    require(av_opt_set_int(w->conversion,"srcw",(w->width+3)&~3,0),"Set source width");
    require(av_opt_set_int(w->conversion,"srch",w->height,0),"Set source height");
    require(av_opt_set_int(w->conversion,"dstw",(w->width+3)&~3,0),"Set output width");
    require(av_opt_set_int(w->conversion,"dsth",w->height,0),"Set output height");
    require(av_opt_set_int(w->conversion,"src_format",AV_PIX_FMT_YUVJ411P,0),"Set decoded format");
    require(av_opt_set_int(w->conversion,"dst_format",AV_PIX_FMT_RGBA,0),"Set owned output format");
    require(av_opt_set_int(w->conversion,"sws_flags",SWS_BILINEAR|SWS_ACCURATE_RND|SWS_FULL_CHR_H_INT|SWS_BITEXACT,0),"Set packing flags");
    require(av_opt_set_int(w->conversion,"threads",1,0),"Set conversion threads");
    require(av_opt_set_int(w->conversion,"src_h_chr_pos",384,0),"Centered quarter-width chroma");
    require(av_opt_set_int(w->conversion,"src_v_chr_pos",0,0),"Full-height chroma");
    require(sws_init_context(w->conversion,NULL,NULL),"Initialize packing");
    const int *matrix=sws_getCoefficients(SWS_CS_ITU601);
    require(sws_setColorspaceDetails(w->conversion,matrix,1,matrix,1,0,1<<16,1<<16),"Set full-range BT.601 conversion");
    int64_t threads;require(av_opt_get_int(w->conversion,"threads",0,&threads),"Read conversion threads");
    if (threads!=1) fail("Conversion threading differs.");
}
static void release(Work *w) { free(w->output); w->output=NULL; }
static void apply(Work *w)
{
    /* Include fresh zeroed owned output capacity, all component decode,
     * exact-grid conversion, in-place row compaction and plane release.
     * Reuse only codec/conversion/packet/empty wrapper; no decoded cache. */
    int grid=(w->width+3)&~3;
    w->output=calloc((size_t)grid*w->height*4+AV_INPUT_BUFFER_PADDING_SIZE,1);
    if (!w->output) fail("Allocate owned output failed.");
    avcodec_flush_buffers(w->codec);
    require(avcodec_send_packet(w->codec,w->input),"Submit complete JPEG");
    require(avcodec_receive_frame(w->codec,w->decoded),"Receive JPEG samples");
    AVFrame *d=w->decoded;
    if (d->width!=w->width || d->height!=w->height || d->format!=AV_PIX_FMT_YUVJ420P
        || !d->buf[0] || !d->data[1] || !d->data[2] || d->linesize[0]<grid
        || d->linesize[1]<(w->width+1)/2 || d->linesize[2]<(w->width+1)/2
        || (d->color_range!=AVCOL_RANGE_JPEG && d->color_range!=AVCOL_RANGE_UNSPECIFIED))
        fail("Expected owned full-range H4V2 decoder planes.");
    // The final chroma sample is real (ceil(width/4)); keep it. Extend only
    // owned decoder luma padding, never the packet or valid component samples.
    for (int y=0;y<w->height;y++)
        memset(d->data[0]+y*d->linesize[0]+w->width,d->data[0][y*d->linesize[0]+w->width-1],grid-w->width);
    // Software MJPEG preserves every original quarter-width chroma sample
    // at even positions in its expanded 420 planes. Recover and vertically
    // interpolate in one pass, with no separate recovered-plane copy.
    int cw=(w->width+3)/4,ch=(w->height+1)/2;
    size_t plane_bytes=(size_t)cw*w->height;
    w->vertical=malloc(plane_bytes*2); if (!w->vertical) fail("Allocate vertical chroma failed.");
    for (int c=0;c<2;c++)
    for (int y=0;y<w->height;y++) {
        int top=(y+1)/2-1,bottom=top+1,tw=(y&1)?3:1;
        if (top<0) top=0;
        if (bottom>=ch) bottom=ch-1;
        const uint8_t *a=d->data[c+1]+top*d->linesize[c+1];
        const uint8_t *b=d->data[c+1]+bottom*d->linesize[c+1];
        uint8_t *out=w->vertical+c*plane_bytes+(size_t)y*cw;
        for (int x=0;x<cw;x++) out[x]=(a[x*2]*tw+b[x*2]*(4-tw)+2)>>2;
    }
    const uint8_t *planes[4]={d->data[0],w->vertical,w->vertical+plane_bytes,NULL};
    int strides[4]={d->linesize[0],cw,cw,0};
    uint8_t *output[4]={w->output,NULL,NULL,NULL}; int output_stride[4]={grid*4,0,0,0};
    if (sws_scale(w->conversion,planes,strides,0,w->height,output,output_stride)!=w->height)
        fail("Quarter-width centered RGBA conversion incomplete.");
    for (int y=0;y<w->height;y++)
        memmove(w->output+(size_t)y*w->width*4,w->output+(size_t)y*grid*4,(size_t)w->width*4);
    free(w->vertical); w->vertical=NULL;
    av_frame_unref(d);
    w->observed+=w->output[w->raw_bytes/2]; w->completed++;
}
static void check_pixels(const Work *w,const uint8_t *pixels)
{
    if (!pixels || memcmp(pixels,w->expected,w->raw_bytes)) fail("Repeated H4V2 samples differ.");
    for (size_t i=3;i<w->raw_bytes;i+=4) if (pixels[i]!=255) fail("Opaque alpha differs.");
}
static void check_and_write(Work *w,const char *path)
{
    apply(w); check_pixels(w,w->output);
    if (w->input->size!=(int)w->encoded_bytes || memcmp(w->input->data,w->source,w->encoded_bytes))
        fail("Resident JPEG source changed.");
    FILE *file=fopen(path,"wb"); if (!file) fail("Open H4V2 output failed.");
    if (fwrite(w->output,1,w->raw_bytes,file)!=w->raw_bytes || fclose(file)) fail("Write H4V2 output failed.");
    release(w);
}
int main(int argc,char **argv)
{
    if (argc!=4 || strcmp(argv[1],"jpeg-410-decode"))
        fail("Usage: ffmpeg-jpeg-410-warm jpeg-410-decode INPUT OUTPUT");
    Work w={0}; w.cpu=affinity_cpu(); w.width=513; w.height=259; w.raw_bytes=(size_t)w.width*w.height*4;
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
    initialize_conversion(&w);
    apply(&w); w.expected=malloc(w.raw_bytes); if (!w.expected) fail("Allocate untimed validation pixels failed.");
    memcpy(w.expected,w.output,w.raw_bytes);
    uint8_t *held=w.output; w.output=NULL; apply(&w);
    if (held==w.output) fail("Live caller outputs alias.");
    check_pixels(&w,held); check_pixels(&w,w.output); held[0]^=255;
    check_pixels(&w,w.output); free(held); check_pixels(&w,w.output); release(&w);
    check_and_write(&w,argv[3]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"jpeg-410-decode\",\"width\":513,\"height\":259,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"decoder\":\"mjpeg\",\"decodedPixelFormat\":\"yuvj420p\",\"outputBytes\":%zu,\"outputStride\":2052,\"initialCompletedOperations\":%" PRIu64 ",\"freshZeroedCallerOutputTimed\":true,\"packingTimed\":true,\"horizontalSampling\":4,\"verticalSampling\":2,\"centeredGridWidth\":516,\"inPlaceCompactionTimed\":true,\"quarterSampleRecoveryTimed\":true,\"verticalInterpolationTimed\":true,\"chromaStagingAcquisitionAndReleaseTimed\":true,\"nativeVerticalRounding\":\"ties-up to8bit\",\"explicitBt601FullRange\":true,\"opaqueAlpha\":true,\"conversionThreads\":1,\"stateResetTimed\":true,\"decoderPlanesReleaseTimed\":true,\"outputAcquisitionAndReleaseTimed\":true,\"independentLiveOutputs\":true,\"pixelsDeterministic\":true,\"rgbaIntermediate\":false,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u}\n",
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
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":2052,\"outputBytes\":%zu,\"cpu\":%d}\n",
            count,elapsed,w.completed,w.observed,w.raw_bytes,w.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    release(&w); av_packet_free(&w.input); av_frame_free(&w.decoded); avcodec_free_context(&w.codec); sws_freeContext(w.conversion);
    free(w.source); free(w.expected); return 0;
}
