/* Original benchmark-only WebP container/EXIF adapter to public FFmpeg APIs.
 * RIFF/VP8X/VP8L/VP8 container fields: RFC 9649 and RFC 6386.
 * TIFF IFD0 orientation: CIPA DC-008-Translation-2023-E.
 * Public WebP parser frames the complete original packet; its required buffered
 * EOF flush is timed. Original structural/metadata validation adds the still
 * Identify contract. No decoder, pixel conversion or upstream source is used. */
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
    AVCodecParserContext *parser;
    AVCodecContext *context;
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
static uint32_t le24(const uint8_t *p)
{ return (uint32_t)p[0] | (uint32_t)p[1]<<8 | (uint32_t)p[2]<<16; }
static void check_dimensions(unsigned width, unsigned height)
{
    if (!width || !height || width>32768 || height>32768 || (uint64_t)width*height>32000000)
        fail("Dimension/pixel limit exceeded.");
}
static Descriptor identify(Work *w)
{
    if (w->size>64U*1024*1024 || w->size<12 || memcmp(w->input,"RIFF",4)
        || memcmp(w->input+8,"WEBP",4) || (uint64_t)read32(w->input+4,1)+8!=w->size)
        fail("Invalid WebP RIFF header/input limit.");
    size_t position=12, metadata=0;
    unsigned items=0,width=0,height=0,canvas_w=0,canvas_h=0;
    int extended=0,pixels=0,alpha=0,exif=0,alpha_chunk=0,icc=0,xmp=0,lossless=0,flags=0;
    uint64_t work=0; Descriptor result={0}; result.orientation=1;
    while (position<w->size) {
        if (++items>100000) fail("Container item limit exceeded.");
        if (w->size-position<8) fail("Truncated WebP chunk.");
        const uint8_t *type=w->input+position,*payload=type+8;
        uint32_t size=read32(type+4,1); uint64_t padded=(uint64_t)size+(size&1);
        if (padded>w->size-position-8) fail("Invalid WebP chunk length.");
        if ((size&1) && payload[size]) fail("Invalid WebP padding.");
        add_work(&work,(uint64_t)size+8);position+=(size_t)padded+8;
        if (!memcmp(type,"VP8X",4)) {
            if (items!=1 || size<10) fail("Invalid VP8X.");
            if (payload[0]&2) fail("Animation is outside the still contract.");
            canvas_w=le24(payload+4)+1;canvas_h=le24(payload+7)+1;
            if ((uint64_t)canvas_w*canvas_h>UINT32_MAX) fail("Canvas pixel count overflow.");
            check_dimensions(canvas_w,canvas_h);
            flags=payload[0];extended=1;alpha=!!(flags&16);
        } else if (!memcmp(type,"ANIM",4) || !memcmp(type,"ANMF",4)) {
            fail("Animation is outside the still contract.");
        } else if (!memcmp(type,"VP8L",4)) {
            if (pixels || alpha_chunk || size<5 || payload[0]!=0x2f) fail("Invalid VP8L header.");
            uint32_t bits=read32(payload+1,1);
            if (bits>>29) fail("Unsupported VP8L version.");
            width=(bits&0x3fff)+1;height=((bits>>14)&0x3fff)+1;
            alpha|=!!(bits&(1U<<28));pixels=1;lossless=1;
        } else if (!memcmp(type,"VP8 ",4)) {
            if (pixels || size<10 || (payload[0]&1) || payload[3]!=0x9d || payload[4]!=1 || payload[5]!=0x2a)
                fail("Invalid VP8 keyframe header.");
            width=read16(payload+6,1)&0x3fff;height=read16(payload+8,1)&0x3fff;
            if (!width || !height) fail("Invalid VP8 dimensions.");
            pixels=1;
        } else if (!memcmp(type,"ALPH",4)) {
            if (!extended || pixels || alpha_chunk || !size || !alpha) fail("Invalid alpha chunk.");
            alpha_chunk=1;
        } else {
            if (size>4U*1024*1024-metadata) fail("Metadata limit exceeded.");
            metadata+=size;
            if (!memcmp(type,"EXIF",4)) {
                if (!extended || exif || !(flags&8)) fail("Invalid EXIF chunk.");
                result.orientation=orientation(payload,size,&work);exif=1;
            } else if (!memcmp(type,"ICCP",4)) {
                if (!extended || icc || pixels || !(flags&32)) fail("Invalid ICC chunk.");
                icc=1;
            } else if (!memcmp(type,"XMP ",4)) {
                if (!extended || xmp || !(flags&4)) fail("Invalid XMP chunk.");
                xmp=1;
            }
        }
    }
    if (!pixels || (extended && (width!=canvas_w || height!=canvas_h))) fail("Invalid canvas geometry.");
    if (extended && ( !!(flags&8)!=exif || !!(flags&32)!=icc || !!(flags&4)!=xmp || (!lossless && alpha!=alpha_chunk)))
        fail("WebP feature flags disagree with chunks.");
    check_dimensions(width,height);
    uint8_t *packet=NULL;int packet_size=0;
    int consumed=av_parser_parse2(w->parser,w->context,&packet,&packet_size,w->input,(int)w->size,
        AV_NOPTS_VALUE,AV_NOPTS_VALUE,0);
    if (consumed!=(int)w->size || packet_size) fail("Public WebP parser input framing differs.");
    consumed=av_parser_parse2(w->parser,w->context,&packet,&packet_size,NULL,0,AV_NOPTS_VALUE,AV_NOPTS_VALUE,0);
    if (consumed || packet_size!=(int)w->size || !packet) fail("Public WebP parser EOF framing differs.");
    result.parameters=avcodec_parameters_alloc();
    if (!result.parameters) fail("Descriptor allocation failed.");
    result.parameters->codec_type=AVMEDIA_TYPE_VIDEO;result.parameters->codec_id=AV_CODEC_ID_WEBP;
    result.parameters->width=(int)width;result.parameters->height=(int)height;result.parameters->bits_per_raw_sample=8;
    result.alpha=alpha;w->completed++;w->observed+=(uint64_t)width+height+8+alpha+result.orientation;
    return result;
}
static void release(Descriptor *d)
{ avcodec_parameters_free(&d->parameters); }
static void descriptor_bytes(const Descriptor *d, uint8_t output[24])
{
    uint32_t fields[]={2,(uint32_t)d->parameters->width,(uint32_t)d->parameters->height,
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
    if (argc!=4 || strcmp(argv[1],"webp-identify")) fail("Use webp-identify input.webp output.info.");
    Work w={0}; w.cpu=cpu(); w.input=input_file(argv[2],&w.size);
    w.original=av_memdup(w.input,w.size); if (!w.original) fail("Original input allocation failed.");
    w.parser=av_parser_init(AV_CODEC_ID_WEBP); w.context=avcodec_alloc_context3(NULL);
    if (!w.parser || !w.context) fail("Pinned public WebP parser APIs unavailable.");
    w.context->thread_count=1; w.context->thread_type=0;
    Descriptor first=identify(&w), second=identify(&w);
    uint8_t a[24], b[24]; descriptor_bytes(&first,a); descriptor_bytes(&second,b);
    if (first.parameters==second.parameters || memcmp(a,b,sizeof(a))) fail("Descriptors alias or differ.");
    first.parameters->width^=1; descriptor_bytes(&second,b);
    if (memcmp(a,b,sizeof(a))) fail("Live descriptor changed with its peer.");
    unsigned width=(unsigned)second.parameters->width, height=(unsigned)second.parameters->height;
    int depth=second.parameters->bits_per_raw_sample, alpha=second.alpha, orient=second.orientation;
    release(&first); release(&second); check_write(&w,argv[3]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"webp-identify\",\"backend\":\"FFmpeg public WebP parser with original complete RIFF/EXIF compatibility adapter\",\"width\":%u,\"height\":%u,\"depth\":%d,\"hasAlpha\":%s,\"orientation\":%d,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"outputBytes\":24,\"inputBytes\":%zu,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"publicParserTimed\":true,\"fullStructureTimed\":true,\"parserEofFlushTimed\":true,\"parserRequiredInputBufferingTimed\":true,\"structureMetadataTimed\":true,\"descriptorAcquisitionAndReleaseTimed\":true,\"pixelDecode\":false,\"scalarMode\":false,\"cpuFlags\":%d,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u,\"avutilVersion\":%u}\n",
        width,height,depth,alpha?"true":"false",orient,w.cpu,w.size,w.completed,av_get_cpu_flags(),av_version_info(),avcodec_version(),avutil_version());
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
    av_parser_close(w.parser); avcodec_free_context(&w.context); av_free(w.input); av_free(w.original);
    return 0;
}
