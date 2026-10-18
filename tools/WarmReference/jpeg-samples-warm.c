/* Original tooling-only raw-sample adapter. Reuses the pinned native libraries;
 * no native code or dependency enters the production package. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#ifdef GRAY_REFERENCE
#include <jpeglib.h>
#define CHANNELS 1
#define FORMAT "gray"
#else
#include <libavcodec/avcodec.h>
#include <libavutil/opt.h>
#include <libswscale/swscale.h>
#define CHANNELS 3
#define FORMAT "rgb"
#endif

static void fail(const char *s) { fprintf(stderr,"%s\n",s); exit(1); }
static int number(const char *s) {
    char *end; errno=0; long n=strtol(s,&end,10);
    if(errno || !*s || *end || n<1 || n>32768) fail("Invalid bounded dimension.");
    return (int)n;
}
static int affinity(void) {
    cpu_set_t set; if(sched_getaffinity(0,sizeof(set),&set) || CPU_COUNT(&set)!=1) fail("One CPU required.");
    for(int i=0;i<CPU_SETSIZE;i++) if(CPU_ISSET(i,&set)) return i;
    fail("No CPU."); return -1;
}
static uint64_t now(void) {
    struct timespec t; if(clock_gettime(CLOCK_MONOTONIC,&t)) fail("Clock failed.");
    return (uint64_t)t.tv_sec*UINT64_C(1000000000)+t.tv_nsec;
}
static uint8_t *read_file(const char *path,size_t *size,size_t expected) {
    FILE *f=fopen(path,"rb"); if(!f || fseek(f,0,SEEK_END)) fail("Open input failed.");
    long n=ftell(f); if(n<=0 || n>512*1024*1024 || fseek(f,0,SEEK_SET)) fail("Input size failed.");
    *size=(size_t)n; if(expected && *size!=expected) fail("Raw input size differs.");
    /* Packed rows are borrowed directly; tail padding permits native SIMD reads. */
    uint8_t *p=calloc(*size+64,1); if(!p || fread(p,1,*size,f)!=*size || fclose(f)) fail("Read failed.");
    return p;
}
typedef struct {
    uint8_t *input,*original,*expected,*bytes;
    size_t input_size,expected_size,output_size;
    int width,height,cpu;
    uint64_t completed,observed;
#ifdef GRAY_REFERENCE
    struct jpeg_compress_struct codec;
    struct jpeg_error_mgr errors;
    unsigned long packet_size;
#else
    AVCodecContext *codec;
    struct SwsContext *conversion;
    AVFrame *yuv;
    AVPacket *packet;
#endif
} Work;
#ifndef GRAY_REFERENCE
static void require(int n) { if(n<0) fail("FFmpeg operation failed."); }
#endif
static void apply(Work *w) {
#ifdef GRAY_REFERENCE
    if(w->bytes) fail("Unreleased output.");
    jpeg_mem_dest(&w->codec,&w->bytes,&w->packet_size);
    jpeg_start_compress(&w->codec,TRUE);
    while(w->codec.next_scanline<w->codec.image_height) {
        JSAMPROW row=w->input+(size_t)w->codec.next_scanline*w->width;
        if(jpeg_write_scanlines(&w->codec,&row,1)!=1) fail("Incomplete scanline.");
    }
    jpeg_finish_compress(&w->codec); w->output_size=w->packet_size;
#else
    const uint8_t *planes[4]={w->input,NULL,NULL,NULL}; int strides[4]={w->width*3,0,0,0};
    if(sws_scale(w->conversion,planes,strides,0,w->height,w->yuv->data,w->yuv->linesize)!=w->height) fail("Incomplete RGB conversion.");
    w->yuv->pts=(int64_t)w->completed; w->yuv->pict_type=AV_PICTURE_TYPE_I; w->yuv->quality=w->codec->global_quality;
    require(avcodec_send_frame(w->codec,w->yuv)); require(avcodec_receive_packet(w->codec,w->packet));
    if(!w->packet->buf) fail("Missing owned output.");
    w->bytes=w->packet->data; w->output_size=(size_t)w->packet->size;
#endif
    if(!w->bytes || w->output_size<4) fail("Missing output.");
    w->completed++; w->observed+=w->bytes[w->output_size/2];
}
static void release(Work *w) {
#ifdef GRAY_REFERENCE
    free(w->bytes); w->packet_size=0;
#else
    av_packet_unref(w->packet);
#endif
    w->bytes=NULL; w->output_size=0;
}
static void check(Work *w) {
    if(w->output_size!=w->expected_size || memcmp(w->bytes,w->expected,w->expected_size)) fail("Frozen reference packet differs.");
    if(memcmp(w->input,w->original,w->input_size)) fail("Caller input changed.");
}
static void write_packet(Work *w,const char *path) {
    FILE *f=fopen(path,"wb"); if(!f || fwrite(w->bytes,1,w->output_size,f)!=w->output_size || fclose(f)) fail("Write failed.");
}
int main(int argc,char **argv) {
    if(argc!=7 || strcmp(argv[1],"jpeg-samples-encode")) fail("Usage: WORKER jpeg-samples-encode INPUT WIDTH HEIGHT OUTPUT FROZEN-NATIVE-PACKET");
    Work w={0}; w.cpu=affinity(); w.width=number(argv[3]); w.height=number(argv[4]);
    w.input=read_file(argv[2],&w.input_size,(size_t)w.width*w.height*CHANNELS);
    w.original=malloc(w.input_size); if(!w.original) fail("Allocate check input failed.");
    memcpy(w.original,w.input,w.input_size); w.expected=read_file(argv[6],&w.expected_size,0);
#ifdef GRAY_REFERENCE
    w.codec.err=jpeg_std_error(&w.errors); jpeg_create_compress(&w.codec);
    w.codec.image_width=w.width; w.codec.image_height=w.height; w.codec.input_components=1; w.codec.in_color_space=JCS_GRAYSCALE;
    jpeg_set_defaults(&w.codec);
    /* Same owned mathematical quantizers as the already qualified gray anchor. */
    unsigned int quantizers[DCTSIZE2];
    for(unsigned int v=0;v<8;v++) for(unsigned int u=0;u<8;u++) {
        unsigned int q=((1+2*(u+v)+u*v)*62+50)/100; quantizers[v*8+u]=q<1?1:q>255?255:q;
    }
    jpeg_add_quant_table(&w.codec,0,quantizers,100,TRUE); w.codec.optimize_coding=TRUE; w.codec.dct_method=JDCT_ISLOW;
#else
    av_log_set_level(AV_LOG_ERROR); const AVCodec *encoder=avcodec_find_encoder(AV_CODEC_ID_MJPEG); if(!encoder) fail("Encoder missing.");
    w.codec=avcodec_alloc_context3(encoder); if(!w.codec) fail("Codec allocation failed.");
    w.codec->thread_count=1; w.codec->thread_type=0; w.codec->width=w.width; w.codec->height=w.height;
    w.codec->time_base=(AVRational){1,1}; w.codec->pix_fmt=AV_PIX_FMT_YUVJ420P; w.codec->color_range=AVCOL_RANGE_JPEG;
    w.codec->colorspace=AVCOL_SPC_SMPTE170M; w.codec->gop_size=1; w.codec->flags|=AV_CODEC_FLAG_QSCALE; w.codec->global_quality=5*FF_QP2LAMBDA;
    require(av_opt_set(w.codec->priv_data,"huffman","optimal",0)); require(avcodec_open2(w.codec,encoder,NULL));
    if(w.codec->thread_count!=1 || w.codec->thread_type || w.codec->active_thread_type) fail("Codec thread count differs.");
    w.yuv=av_frame_alloc(); w.packet=av_packet_alloc(); if(!w.yuv || !w.packet) fail("Frame/packet allocation failed.");
    w.yuv->width=w.width; w.yuv->height=w.height; w.yuv->format=AV_PIX_FMT_YUVJ420P; require(av_frame_get_buffer(w.yuv,32));
    w.conversion=sws_alloc_context(); if(!w.conversion) fail("Conversion allocation failed.");
    require(av_opt_set_int(w.conversion,"srcw",w.width,0)); require(av_opt_set_int(w.conversion,"srch",w.height,0));
    require(av_opt_set_int(w.conversion,"dstw",w.width,0)); require(av_opt_set_int(w.conversion,"dsth",w.height,0));
    require(av_opt_set_int(w.conversion,"src_format",AV_PIX_FMT_RGB24,0)); require(av_opt_set_int(w.conversion,"dst_format",AV_PIX_FMT_YUVJ420P,0));
    require(av_opt_set_int(w.conversion,"sws_flags",SWS_AREA|SWS_ACCURATE_RND|SWS_BITEXACT,0)); require(av_opt_set_int(w.conversion,"threads",1,0));
    require(sws_init_context(w.conversion,NULL,NULL)); const int *matrix=sws_getCoefficients(SWS_CS_ITU601);
    require(sws_setColorspaceDetails(w.conversion,matrix,1,matrix,1,0,1<<16,1<<16));
#endif
    apply(&w); check(&w);
#ifdef GRAY_REFERENCE
    uint8_t *held=w.bytes; w.bytes=NULL; w.packet_size=0; apply(&w); check(&w);
    if(held==w.bytes || memcmp(held,w.expected,w.expected_size)) fail("Independent outputs differ.");
    free(held);
#else
    AVPacket *held=av_packet_clone(w.packet); if(!held) fail("Retain output failed.");
    release(&w); apply(&w); check(&w);
    if(held->buf->buffer==w.packet->buf->buffer || held->size!=(int)w.expected_size || memcmp(held->data,w.expected,w.expected_size)) fail("Independent outputs differ.");
    av_packet_free(&held);
#endif
    write_packet(&w,argv[5]); release(&w);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"jpeg-samples-encode\",\"sampleFormat\":\"%s\",\"width\":%d,\"height\":%d,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"conversionThreads\":1,\"initialCompletedOperations\":%" PRIu64 ",\"outputBytes\":%zu,\"rawInputBorrowed\":true,\"normalizationTimed\":true,\"outputAcquisitionAndReleaseTimed\":true,\"independentLiveOutputs\":true,\"frozenPacketExact\":true}\n",FORMAT,w.width,w.height,w.cpu,w.completed,w.expected_size); fflush(stdout);
    char line[128];
    while(fgets(line,sizeof(line),stdin)) {
        if(!strcmp(line,"quit\n")) break;
        if(!strcmp(line,"check\n")) { apply(&w); check(&w); write_packet(&w,argv[5]); release(&w); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue; }
        uint64_t count; char extra;
        if(sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1 || !count || count>UINT64_C(10000000)) fail("Invalid bounded sample count.");
        uint64_t start=now(); for(uint64_t i=0;i<count;i++) { apply(&w); release(&w); } uint64_t elapsed=now()-start;
        if(sched_getcpu()!=w.cpu || affinity()!=w.cpu) fail("CPU changed.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"outputBytes\":%zu,\"cpu\":%d}\n",count,elapsed,w.completed,w.observed,w.expected_size,w.cpu); fflush(stdout);
    }
    if(ferror(stdin)) fail("Protocol read failed.");
#ifdef GRAY_REFERENCE
    jpeg_destroy_compress(&w.codec);
#else
    av_packet_free(&w.packet); av_frame_free(&w.yuv); avcodec_free_context(&w.codec); sws_freeContext(w.conversion);
#endif
    free(w.input); free(w.original); free(w.expected); return 0;
}
