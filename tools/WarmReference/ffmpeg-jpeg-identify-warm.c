/* Original benchmark-only FFmpeg MJPEG metadata adapter.
 * JPEG marker/SOF/SOS structure: ITU-T T.81, annex B.
 * TIFF IFD0 orientation: https://www.cipa.jp/std/documents/e/DC-008-Translation-2023-E.pdf
 * Complete original structural/EXIF validation and a timed canonical SOF2
 * metadata-header projection align discard-all extraction with Identify, which
 * does not validate quantizers/sampling/entropy. No upstream source is copied. */
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
#include <libavutil/cpu.h>
#include <libavutil/mem.h>

typedef struct {
    AVCodecParameters *parameters;
    int alpha, orientation;
} Descriptor;
typedef struct {
    AVCodecContext *context;
    AVPacket *projection;
    AVFrame *frame;
    unsigned pixel_requests;
    uint8_t *input, *original;
    size_t size;
    int cpu;
    uint64_t completed, observed;
} Work;

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
static uint32_t be32(const uint8_t *p)
{ return (uint32_t)p[0]<<24 | (uint32_t)p[1]<<16 | (uint32_t)p[2]<<8 | p[3]; }
static unsigned read16(const uint8_t *p, int little)
{ return little ? (unsigned)p[1]<<8 | p[0] : (unsigned)p[0]<<8 | p[1]; }
static uint32_t read32(const uint8_t *p, int little)
{ return little ? (uint32_t)p[3]<<24 | (uint32_t)p[2]<<16 | (uint32_t)p[1]<<8 | p[0] : be32(p); }
static uint64_t clock_ns(clockid_t clock)
{
    struct timespec t;
    if (clock_gettime(clock, &t)) fail("Clock failed.");
    return (uint64_t)t.tv_sec*UINT64_C(1000000000)+(uint64_t)t.tv_nsec;
}
static int cpu(void)
{
    cpu_set_t set;
    if (sched_getaffinity(0, sizeof(set), &set) || CPU_COUNT(&set)!=1) fail("Use exactly one CPU.");
    for (int i=0; i<CPU_SETSIZE; i++) if (CPU_ISSET(i, &set)) return i;
    fail("Empty CPU affinity."); return -1;
}
static uint8_t *input_file(const char *path, size_t *size)
{
    FILE *f=fopen(path,"rb");
    if (!f || fseek(f,0,SEEK_END)) fail("Open/seek input failed.");
    long n=ftell(f);
    if (n<1 || n>64L*1024*1024 || fseek(f,0,SEEK_SET)) fail("Input limit exceeded.");
    uint8_t *b=av_mallocz((size_t)n+AV_INPUT_BUFFER_PADDING_SIZE);
    if (!b || fread(b,1,(size_t)n,f)!=(size_t)n || ferror(f) || fclose(f)) fail("Read input failed.");
    *size=(size_t)n; return b;
}
static void add_work(uint64_t *work, uint64_t amount)
{
    if (amount>UINT64_C(2000000000)-*work) fail("Work limit exceeded.");
    *work+=amount;
}
static int orientation(const uint8_t *b, size_t n, uint64_t *work)
{
    if (n>=6 && !memcmp(b,"Exif\0\0",6)) { b+=6; n-=6; }
    if (n<8) fail("Truncated EXIF.");
    int little=!memcmp(b,"II",2);
    if ((!little && memcmp(b,"MM",2)) || read16(b+2,little)!=42) fail("Invalid TIFF header.");
    uint32_t offset=read32(b+4,little);
    if (offset<8 || offset>n-2) fail("Invalid IFD offset.");
    const uint8_t *directory=b+offset;
    unsigned count=read16(directory,little);
    if ((uint64_t)count*12+6>n-offset) fail("Truncated IFD.");
    int found=0, result=1;
    for (unsigned i=0; i<count; i++) {
        add_work(work,1);
        const uint8_t *entry=directory+2+12*i;
        if (read16(entry,little)!=0x0112) continue;
        if (found || read16(entry+2,little)!=3 || read32(entry+4,little)!=1) fail("Invalid orientation field.");
        result=(int)read16(entry+8,little);
        if (result<1 || result>8) fail("Invalid orientation value.");
        found=1;
    }
    return result;
}
static int reject_pixels(AVCodecContext *context, AVFrame *frame, int flags)
{
    (void)frame; (void)flags;
    Work *w=context->opaque; w->pixel_requests++;
    return AVERROR(EINVAL);
}
static Descriptor identify(Work *w)
{
    if (w->size>64U*1024*1024 || w->size<2 || w->input[0]!=255 || w->input[1]!=0xd8)
        fail("Invalid JPEG signature/input limit.");
    size_t position=2, metadata=0;
    unsigned items=0, width=0, height=0;
    int frame=0, scan=0, exif=0, scan_seen=0, ended=0, orient=1;
    uint64_t work=0;
    while (position<w->size) {
        if (scan && w->input[position]!=255) {
            const uint8_t *next=memchr(w->input+position,255,w->size-position);
            size_t count=next ? (size_t)(next-w->input)-position : w->size-position;
            add_work(&work,count); position+=count;
            if (position==w->size) break;
        }
        add_work(&work,1);
        if (w->input[position++]!=255) fail("Invalid JPEG marker.");
        while (position<w->size && w->input[position]==255) { position++; add_work(&work,1); }
        if (position==w->size) break;
        unsigned marker=w->input[position++];
        if (scan && !marker) continue;
        if (++items>100000) fail("JPEG marker count limit exceeded.");
        if (marker>=0xd0 && marker<=0xd7) {
            if (!scan) fail("Restart marker outside scan.");
            continue;
        }
        scan=0;
        if (marker==0xd9) {
            if (!frame || !scan_seen || position!=w->size) fail("Invalid JPEG EOI/trailing input.");
            ended=1; break;
        }
        if (!marker || marker==0xd8 || marker==1 || w->size-position<2) fail("Invalid JPEG marker sequence.");
        unsigned length=read16(w->input+position,0);
        if (length<2 || length>w->size-position) fail("Truncated JPEG segment.");
        const uint8_t *payload=w->input+position+2; unsigned payload_size=length-2;
        position+=length; add_work(&work,length);
        if (marker>=0xc0 && marker<=0xcf && marker!=0xc4 && marker!=0xc8 && marker!=0xcc) {
            if (marker!=0xc0 && marker!=0xc2) fail("Unsupported JPEG coding profile.");
            if (frame || payload_size<6) fail("Invalid/duplicate JPEG frame header.");
            if (payload[0]!=8) fail("Unsupported JPEG precision.");
            height=read16(payload+1,0); width=read16(payload+3,0); unsigned components=payload[5];
            if (components!=1 && components!=3) fail("Unsupported JPEG component layout.");
            if (payload_size!=6+3*components || !width || !height) fail("Invalid JPEG frame dimensions/components.");
            if (width>32768 || height>32768 || (uint64_t)width*height>32000000) fail("JPEG dimension/pixel limit exceeded.");
            frame=1;
        } else if (marker==0xda) {
            if (!frame || payload_size<4 || !payload[0] || payload_size!=4+2U*payload[0]) fail("Invalid JPEG scan header.");
            scan=1; scan_seen=1;
        } else if ((marker>=0xe0 && marker<=0xef) || marker==0xfe) {
            if (payload_size>4U*1024*1024-metadata) fail("JPEG metadata limit exceeded.");
            metadata+=payload_size;
            if (marker==0xe1 && payload_size>=6 && !memcmp(payload,"Exif\0\0",6)) {
                if (exif) fail("Duplicate JPEG EXIF.");
                orient=orientation(payload,payload_size,&work); exif=1;
            }
        }
    }
    if (!ended) fail("JPEG is missing EOI.");
    // ImageInfo does not expose sampling, quantizers, entropy or coding mode.
    // A canonical SOF2 header preserves its exact validated dimensions/precision
    // while avoiding the decoder's stronger DQT and compressed-size checks.
    // Construction, packet ownership, flush/extraction and descriptor are timed.
    avcodec_flush_buffers(w->context);
    if (av_new_packet(w->projection,17)<0) fail("Canonical header allocation failed.");
    uint8_t *h=w->projection->data;
    h[0]=255;h[1]=0xd8;h[2]=255;h[3]=0xc2;h[4]=0;h[5]=11;h[6]=8;
    h[7]=(uint8_t)(height>>8);h[8]=(uint8_t)height;h[9]=(uint8_t)(width>>8);h[10]=(uint8_t)width;
    h[11]=1;h[12]=1;h[13]=0x11;h[14]=0;h[15]=255;h[16]=0xd9;
    if (avcodec_send_packet(w->context,w->projection)<0) fail("Canonical header submission failed.");
    if (avcodec_receive_frame(w->context,w->frame)!=AVERROR(EAGAIN) || w->pixel_requests)
        fail("Metadata extraction requested pixel decoding.");
    av_packet_unref(w->projection);
    Descriptor result={0}; result.orientation=orient;
    result.parameters=avcodec_parameters_alloc();
    if (!result.parameters || avcodec_parameters_from_context(result.parameters,w->context)<0) fail("Owned descriptor extraction failed.");
    if (result.parameters->codec_id!=AV_CODEC_ID_MJPEG || result.parameters->width!=(int)width
        || result.parameters->height!=(int)height || result.parameters->bits_per_raw_sample!=8)
        fail("Native extracted metadata differs from complete source validation.");
    w->completed++; w->observed+=(uint64_t)width+height+8+orient;
    return result;
}
static void release(Descriptor *d)
{ avcodec_parameters_free(&d->parameters); }
static void descriptor_bytes(const Descriptor *d, uint8_t output[24])
{
    uint32_t fields[]={1,(uint32_t)d->parameters->width,(uint32_t)d->parameters->height,
        (uint32_t)d->parameters->bits_per_raw_sample,(uint32_t)d->alpha,(uint32_t)d->orientation};
    for (unsigned i=0; i<6; i++)
        for (unsigned b=0; b<4; b++) output[4*i+b]=(uint8_t)(fields[i]>>(8*b));
}
static void check_write(Work *w, const char *path)
{
    Descriptor d=identify(w); uint8_t bytes[24]; descriptor_bytes(&d,bytes); release(&d);
    if (memcmp(w->input,w->original,w->size)) fail("Source changed.");
    FILE *f=fopen(path,"wb");
    if (!f || fwrite(bytes,1,sizeof(bytes),f)!=sizeof(bytes) || fclose(f)) fail("Write descriptor failed.");
}
int main(int argc, char **argv)
{
    if (argc!=4 || strcmp(argv[1],"jpeg-identify")) fail("Use jpeg-identify input.jpg output.info.");
    Work w={0}; w.cpu=cpu(); w.input=input_file(argv[2],&w.size);
    w.original=av_memdup(w.input,w.size); if (!w.original) fail("Original input allocation failed.");
    const AVCodec *codec=avcodec_find_decoder(AV_CODEC_ID_MJPEG);
    w.context=avcodec_alloc_context3(codec); w.projection=av_packet_alloc(); w.frame=av_frame_alloc();
    if (!codec || !w.context || !w.projection || !w.frame) fail("Pinned public MJPEG APIs unavailable.");
    w.context->thread_count=1; w.context->thread_type=0; w.context->skip_frame=AVDISCARD_ALL;
    w.context->opaque=&w; w.context->get_buffer2=reject_pixels; w.context->max_pixels=32000000;
    if (avcodec_open2(w.context,codec,NULL)<0 || w.context->active_thread_type) fail("Open serial metadata decoder failed.");
    Descriptor first=identify(&w), second=identify(&w);
    uint8_t a[24], b[24]; descriptor_bytes(&first,a); descriptor_bytes(&second,b);
    if (first.parameters==second.parameters || memcmp(a,b,sizeof(a))) fail("Descriptors alias or differ.");
    first.parameters->width^=1; descriptor_bytes(&second,b);
    if (memcmp(a,b,sizeof(a))) fail("Live descriptor changed with its peer.");
    unsigned width=(unsigned)second.parameters->width, height=(unsigned)second.parameters->height;
    int depth=second.parameters->bits_per_raw_sample, orient=second.orientation;
    release(&first); release(&second); check_write(&w,argv[3]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"jpeg-identify\",\"backend\":\"FFmpeg MJPEG discard-all metadata extraction with original full structural/EXIF scan and timed canonical SOF2 projection\",\"width\":%u,\"height\":%u,\"depth\":%d,\"hasAlpha\":false,\"orientation\":%d,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"outputBytes\":24,\"inputBytes\":%zu,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"fullStructureTimed\":true,\"structureMetadataTimed\":true,\"canonicalHeaderProjectionTimed\":true,\"canonicalHeaderBytes\":17,\"canonicalHeaderProfile\":\"SOF2 grayscale8 metadata only\",\"discardAllHeaderExtractionTimed\":true,\"decoderStateResetTimed\":true,\"descriptorAcquisitionAndReleaseTimed\":true,\"pixelDecode\":false,\"pixelBufferRequests\":%u,\"scalarMode\":false,\"cpuFlags\":%d,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u,\"avutilVersion\":%u}\n",
        width,height,depth,orient,w.cpu,w.size,w.completed,w.pixel_requests,av_get_cpu_flags(),av_version_info(),avcodec_version(),avutil_version());
    fflush(stdout);
    char line[128];
    while (fgets(line,sizeof(line),stdin)) {
        if (!strcmp(line,"quit\n")) break;
        if (!strcmp(line,"check\n")) {
            check_write(&w,argv[3]); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if (sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1 || !count || count>10000000) fail("Invalid sample request.");
        uint64_t completed=w.completed, cpu_start=clock_ns(CLOCK_PROCESS_CPUTIME_ID), start=clock_ns(CLOCK_MONOTONIC);
        for (uint64_t i=0; i<count; i++) { Descriptor d=identify(&w); release(&d); }
        uint64_t elapsed=clock_ns(CLOCK_MONOTONIC)-start, cpu_elapsed=clock_ns(CLOCK_PROCESS_CPUTIME_ID)-cpu_start;
        if (cpu()!=w.cpu || sched_getcpu()!=w.cpu || w.completed-completed!=count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"cpuElapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"outputBytes\":24,\"cpu\":%d}\n",count,elapsed,cpu_elapsed,w.completed,w.observed,w.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_packet_free(&w.projection); av_frame_free(&w.frame); avcodec_free_context(&w.context); av_free(w.input); av_free(w.original);
    return 0;
}
