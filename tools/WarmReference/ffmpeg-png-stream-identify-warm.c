/* Original fresh-Stream benchmark adapter to public FFmpeg AVIO/parser/CRC APIs.
 * PNG structure/CRC: https://www.w3.org/TR/png-3/
 * TIFF IFD0 orientation: https://www.cipa.jp/std/documents/e/DC-008-Translation-2023-E.pdf
 * The parser frames packets; the timed compatibility scan supplies the stricter
 * still-container validation and metadata required by Lokad.Image Identify.
 * No decoder, inflater, image conversion or upstream implementation is used. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <libavformat/avio.h>
#include <libavcodec/avcodec.h>
#include <libavutil/avutil.h>
#include <libavutil/cpu.h>
#include <libavutil/crc.h>
#include <libavutil/mem.h>

typedef struct {
    AVCodecParameters *parameters;
    int alpha, orientation, color, alpha_rep, components;
    uint8_t *palette, *transparency;
} Descriptor;
typedef struct {
    AVCodecParserContext *parser;
    AVCodecContext *context;
    const AVCRC *crc;
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
static int orientation(const uint8_t *b, size_t n)
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
        const uint8_t *entry=directory+2+12*i;
        if (read16(entry,little)!=0x0112) continue;
        if (found || read16(entry+2,little)!=3 || read32(entry+4,little)!=1) fail("Invalid orientation field.");
        result=(int)read16(entry+8,little);
        if (result<1 || result>8) fail("Invalid orientation value.");
        found=1;
    }
    return result;
}
static Descriptor identify_resident(Work *w)
{
    static const uint8_t signature[]={137,80,78,71,13,10,26,10};
    if (w->size>64U*1024*1024 || w->size<8 || memcmp(w->input,signature,8)) fail("Invalid PNG signature/input limit.");
    size_t position=8, palette_length=0, metadata=0;
    uint64_t compressed=0;
    unsigned items=0, width=0, height=0, depth=0, color=0;
    int header=0, palette=0, transparency=0, exif=0, data=0, closed=0, ended=0;
    Descriptor result={0}; result.orientation=1;
    while (position<w->size) {
        if (++items>100000) fail("Container item limit exceeded.");
        if (w->size-position<12) fail("Truncated PNG chunk.");
        uint32_t length=be32(w->input+position);
        if (length>INT32_MAX || length>w->size-position-12) fail("Invalid PNG chunk length.");
        const uint8_t *type=w->input+position+4, *payload=type+4;
        for (unsigned i=0; i<4; i++)
            if (!((type[i]>='A' && type[i]<='Z') || (type[i]>='a' && type[i]<='z'))) fail("Invalid PNG chunk name.");
        if (~av_crc(w->crc,UINT32_MAX,type,(size_t)length+4)!=be32(payload+length)) fail("PNG CRC mismatch.");
        position+=(size_t)length+12;
        if (!header && memcmp(type,"IHDR",4)) fail("PNG must start with IHDR.");
        if (data && memcmp(type,"IDAT",4)) closed=1;
        if (!memcmp(type,"IHDR",4)) {
            if (header || length!=13) fail("Invalid/duplicate IHDR.");
            width=be32(payload); height=be32(payload+4); depth=payload[8]; color=payload[9];
            int valid=color==0 ? (depth==1 || depth==2 || depth==4 || depth==8 || depth==16)
                : color==3 ? (depth==1 || depth==2 || depth==4 || depth==8)
                : (color==2 || color==4 || color==6) && (depth==8 || depth==16);
            if (!valid || payload[10] || payload[11] || payload[12]>1) fail("Invalid PNG coding fields.");
            if (!width || !height || width>32768 || height>32768 || (uint64_t)width*height>32000000) fail("Pixel/dimension limit exceeded.");
            header=1;
        } else if (!memcmp(type,"PLTE",4)) {
            if (palette || data || transparency || color==0 || color==4 || length<3 || length>768 || length%3
                || (color==3 && length/3>(1U<<depth))) fail("Invalid PNG palette.");
            result.palette=av_memdup(payload,length);
            if (!result.palette) fail("Palette allocation failed.");
            palette_length=length; palette=1;
        } else if (!memcmp(type,"tRNS",4)) {
            if (transparency || data || (color==3 && (!palette || !length || length>palette_length/3))
                || (color==0 && length!=2) || (color==2 && length!=6) || color==4 || color==6) fail("Invalid PNG transparency.");
            if (color==0 || color==2)
                for (unsigned i=0; i<length; i+=2)
                    if (read16(payload+i,0)>((1U<<depth)-1)) fail("Invalid transparency sample.");
            result.transparency=av_memdup(payload,length);
            if (!result.transparency) fail("Transparency allocation failed.");
            transparency=1;
        } else if (!memcmp(type,"IDAT",4)) {
            if (closed || (color==3 && !palette)) fail("Invalid PNG IDAT ordering.");
            data=1; compressed+=length;
            if (compressed>INT32_MAX) fail("Compressed length overflow.");
        } else if (!memcmp(type,"IEND",4)) {
            if (length || !data || !compressed || position!=w->size) fail("Invalid PNG end/trailing input.");
            ended=1; break;
        } else {
            if (!memcmp(type,"acTL",4) || !memcmp(type,"fcTL",4) || !memcmp(type,"fdAT",4)) fail("Animation is outside the still contract.");
            if (!(type[0]&32)) fail("Unknown critical PNG chunk.");
            if (length>4U*1024*1024-metadata) fail("Metadata limit exceeded.");
            metadata+=length;
            if (!memcmp(type,"eXIf",4)) {
                if (exif || data) fail("Duplicate/misplaced EXIF.");
                result.orientation=orientation(payload,length); exif=1;
            }
        }
    }
    if (!ended) fail("PNG is missing IEND.");
    uint8_t *packet=NULL; int packet_size=0;
    int consumed=av_parser_parse2(w->parser,w->context,&packet,&packet_size,w->input,(int)w->size,
        AV_NOPTS_VALUE,AV_NOPTS_VALUE,0);
    if (consumed!=(int)w->size || packet_size!=(int)w->size || packet!=w->input)
        fail("Public PNG parser framing differs.");
    result.parameters=avcodec_parameters_alloc();
    if (!result.parameters) fail("Descriptor allocation failed.");
    result.parameters->codec_type=AVMEDIA_TYPE_VIDEO;
    result.parameters->codec_id=AV_CODEC_ID_PNG;
    result.parameters->width=(int)width; result.parameters->height=(int)height;
    result.parameters->bits_per_raw_sample=(int)depth;
    result.alpha=color==4 || color==6 || transparency;
    result.color=color==0 || color==4 ? 1 : color==3 ? 3 : 2;
    result.alpha_rep=color==4 || color==6 ? 2 : transparency ? (color==3 ? 3 : 4) : 1;
    result.components=color==0 || color==3 ? 1 : color==2 ? 3 : color==4 ? 2 : 4;
    w->completed++;
    w->observed+=(uint64_t)width+height+depth+result.alpha+result.orientation+result.color+result.alpha_rep+result.components;
    return result;
}
typedef struct { const uint8_t *data; size_t size, position; unsigned reads, eof_reads, size_queries; } Source;
static int source_read(void *opaque, uint8_t *output, int count)
{
    Source *s=opaque;s->reads++;
    size_t available=s->size-s->position;
    if (!available) { s->eof_reads++;return AVERROR_EOF; }
    if ((size_t)count>available) count=(int)available;
    memcpy(output,s->data+s->position,(size_t)count);s->position+=(size_t)count;return count;
}
static int64_t source_seek(void *opaque, int64_t offset, int whence)
{
    Source *s=opaque;
    if (whence==AVSEEK_SIZE) { s->size_queries++;return (int64_t)s->size; }
    whence&=~AVSEEK_FORCE;
    int64_t origin=whence==SEEK_SET ? 0 : whence==SEEK_CUR ? (int64_t)s->position : whence==SEEK_END ? (int64_t)s->size : -1;
    if (origin<0 || offset < -origin || offset>(int64_t)s->size-origin) return AVERROR(EINVAL);
    s->position=(size_t)(origin+offset);return (int64_t)s->position;
}
static Descriptor identify(Work *w)
{
    Source source={.data=w->input,.size=w->size};
    uint8_t *scratch=av_malloc(8192);
    if (!scratch) fail("AVIO scratch acquisition failed.");
    AVIOContext *io=avio_alloc_context(scratch,8192,0,&source,source_read,NULL,source_seek);
    if (!io) fail("Fresh AVIO acquisition failed.");
    io->seekable=AVIO_SEEKABLE_NORMAL;
    int64_t length=avio_size(io);
    if (length<1 || length>64L*1024*1024) fail("Stream input limit exceeded.");
    uint8_t *owned=av_mallocz((size_t)length+AV_INPUT_BUFFER_PADDING_SIZE);
    if (!owned) fail("Fresh input acquisition failed.");
    if (avio_read(io,owned,(int)length)!=length) fail("Incomplete input read.");
    uint8_t tail;
    if (avio_read(io,&tail,1)!=AVERROR_EOF || !avio_feof(io) || source.position!=source.size
        || !source.eof_reads || !source.size_queries) fail("Stream EOF/length/source boundary differs.");
    Work current=*w;current.input=owned;current.size=(size_t)length;
    Descriptor result=identify_resident(&current);
    w->completed=current.completed;w->observed=current.observed;
    av_free(owned);av_freep(&io->buffer);avio_context_free(&io);
    return result;
}
static void release(Descriptor *d)
{ avcodec_parameters_free(&d->parameters); av_freep(&d->palette); av_freep(&d->transparency); }
static void descriptor_bytes(const Descriptor *d, uint8_t output[36])
{
    uint32_t fields[]={0,(uint32_t)d->parameters->width,(uint32_t)d->parameters->height,
        (uint32_t)d->parameters->bits_per_raw_sample,(uint32_t)d->alpha,(uint32_t)d->orientation,(uint32_t)d->color,(uint32_t)d->alpha_rep,(uint32_t)d->components};
    for (unsigned i=0; i<9; i++)
        for (unsigned b=0; b<4; b++) output[4*i+b]=(uint8_t)(fields[i]>>(8*b));
}
static void check_write(Work *w, const char *path)
{
    Descriptor d=identify(w); uint8_t bytes[36]; descriptor_bytes(&d,bytes); release(&d);
    if (memcmp(w->input,w->original,w->size)) fail("Source changed.");
    FILE *f=fopen(path,"wb");
    if (!f || fwrite(bytes,1,sizeof(bytes),f)!=sizeof(bytes) || fclose(f)) fail("Write descriptor failed.");
}
int main(int argc, char **argv)
{
    if (argc!=4 || strcmp(argv[1],"png-stream-identify")) fail("Use png-stream-identify input.png output.info.");
    Work w={0}; w.cpu=cpu(); w.input=input_file(argv[2],&w.size);
    w.original=av_memdup(w.input,w.size); if (!w.original) fail("Original input allocation failed.");
    w.crc=av_crc_get_table(AV_CRC_32_IEEE_LE);
    w.parser=av_parser_init(AV_CODEC_ID_PNG); w.context=avcodec_alloc_context3(NULL);
    if (!w.crc || !w.parser || !w.context) fail("Pinned public parser/CRC APIs unavailable.");
    w.context->thread_count=1; w.context->thread_type=0;
    Descriptor first=identify(&w), second=identify(&w);
    uint8_t a[36], b[36]; descriptor_bytes(&first,a); descriptor_bytes(&second,b);
    if (first.parameters==second.parameters || memcmp(a,b,sizeof(a))) fail("Descriptors alias or differ.");
    first.parameters->width^=1; descriptor_bytes(&second,b);
    if (memcmp(a,b,sizeof(a))) fail("Live descriptor changed with its peer.");
    unsigned width=(unsigned)second.parameters->width, height=(unsigned)second.parameters->height;
    int depth=second.parameters->bits_per_raw_sample, alpha=second.alpha, orient=second.orientation;
    int source_color=second.color, source_alpha=second.alpha_rep, components=second.components;
    release(&first); release(&second); check_write(&w,argv[3]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"png-stream-identify\",\"backend\":\"FFmpeg fresh AVIO plus public PNG parser and CRC with original structural/metadata compatibility adapter\",\"width\":%u,\"height\":%u,\"depth\":%d,\"hasAlpha\":%s,\"orientation\":%d,\"sourceColor\":%d,\"sourceAlpha\":%d,\"sourceComponentCount\":%d,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"outputBytes\":36,\"inputBytes\":%zu,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"freshSeekableStream\":true,\"inputView\":false,\"inputCopyTimed\":true,\"inputLengthQueryTimed\":true,\"eofChecked\":true,\"callerSourceLeftOpen\":true,\"streamAcquisitionAndReleaseTimed\":true,\"publicParserTimed\":true,\"fullCrcTimed\":true,\"structureMetadataTimed\":true,\"descriptorAcquisitionAndReleaseTimed\":true,\"pixelDecode\":false,\"scalarMode\":false,\"cpuFlags\":%d,\"ffmpegVersion\":\"%s\",\"avcodecVersion\":%u,\"avutilVersion\":%u}\n",
        width,height,depth,alpha?"true":"false",orient,source_color,source_alpha,components,w.cpu,w.size,w.completed,av_get_cpu_flags(),av_version_info(),avcodec_version(),avutil_version());
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
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"cpuElapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"outputBytes\":36,\"cpu\":%d}\n",count,elapsed,cpu_elapsed,w.completed,w.observed,w.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_parser_close(w.parser); avcodec_free_context(&w.context); av_free(w.input); av_free(w.original);
    return 0;
}
