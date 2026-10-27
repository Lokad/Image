/* Original development-only bounded header adapter to public FFmpeg AVIO and
 * owned descriptor APIs. PNG3 framing and CIPA EXIF IFD0; no PNG CRC or pixel
 * decoding. No upstream implementation/tables/corpus copied. The public PNG
 * packet parser is deliberately excluded: incremental use buffers whole packets.
 * This reference must be labeled as an original bounded header adapter. */
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
#include <libavutil/mem.h>

static void fail(const char *message) { fprintf(stderr,"%s\n",message); exit(1); }
static uint32_t be32(const uint8_t *p) { return (uint32_t)p[0]<<24|(uint32_t)p[1]<<16|(uint32_t)p[2]<<8|p[3]; }
static unsigned u16(const uint8_t *p,int le) { return le ? p[0]|(unsigned)p[1]<<8 : (unsigned)p[0]<<8|p[1]; }
static uint32_t u32(const uint8_t *p,int le) { return le ? (uint32_t)p[3]<<24|(uint32_t)p[2]<<16|(uint32_t)p[1]<<8|p[0] : be32(p); }
static int cpu(void)
{
    cpu_set_t mask; if (sched_getaffinity(0,sizeof(mask),&mask)||CPU_COUNT(&mask)!=1) fail("Exactly one CPU required.");
    for (int c=0;c<CPU_SETSIZE;c++) if (CPU_ISSET(c,&mask)) return c;
    fail("No CPU."); return -1;
}
static uint64_t now(void)
{
    struct timespec t; if (clock_gettime(CLOCK_MONOTONIC,&t)) fail("Clock failed.");
    return (uint64_t)t.tv_sec*UINT64_C(1000000000)+t.tv_nsec;
}
typedef struct { const uint8_t *data; size_t size,position,read_bytes; unsigned reads,eof_reads,seeks,length_queries; } Source;
static int read_source(void *opaque,uint8_t *bytes,int count)
{
    Source *s=opaque; s->reads++; size_t left=s->size-s->position;
    if (!left) { s->eof_reads++; return AVERROR_EOF; }
    if ((size_t)count>left) count=(int)left;
    memcpy(bytes,s->data+s->position,(size_t)count); s->position+=(size_t)count; s->read_bytes+=(size_t)count; return count;
}
static int64_t seek_source(void *opaque,int64_t offset,int whence)
{
    Source *s=opaque;
    if (whence==AVSEEK_SIZE) { s->length_queries++; return (int64_t)s->size; }
    whence&=~AVSEEK_FORCE;
    int64_t origin=whence==SEEK_SET ? 0 : whence==SEEK_CUR ? (int64_t)s->position : whence==SEEK_END ? (int64_t)s->size : -1;
    if (origin<0||offset < -origin||offset>(int64_t)s->size-origin) return AVERROR(EINVAL);
    s->seeks++; s->position=(size_t)(origin+offset); return (int64_t)s->position;
}
typedef struct { AVIOContext *io; uint8_t header[32]; int64_t length; } Reader;
static int64_t position(Reader *r) { return avio_tell(r->io); }
static void read_header(Reader *r,int count)
{
    if (count<0||count>32||avio_read(r->io,r->header,count)!=count) fail("Truncated header.");
}
static void skip(Reader *r,int64_t count)
{
    int64_t start=position(r);
    if (start<0||start>r->length||count<0||count>r->length-start) fail("Truncated skipped payload.");
    if (count&&avio_skip(r->io,count)!=start+count) fail("Invalid skip result.");
}
static int exif(Reader *r,uint32_t size)
{
    int64_t end=position(r)+size; uint8_t tiff[8];
    if (size<8) fail("Truncated EXIF.");
    read_header(r,8); memcpy(tiff,r->header,8);
    if (!memcmp(tiff,"Exif\0\0",6)) {
        if (size<14) fail("Truncated prefixed TIFF.");
        tiff[0]=tiff[6]; tiff[1]=tiff[7]; read_header(r,6); memcpy(tiff+2,r->header,6);
    }
    int le=!memcmp(tiff,"II",2);
    if ((!le&&memcmp(tiff,"MM",2))||u16(tiff+2,le)!=42) fail("Invalid TIFF header.");
    uint32_t offset=u32(tiff+4,le);
    if (offset<8||(int64_t)offset-8>end-position(r)-2) fail("Invalid IFD offset.");
    skip(r,offset-8); read_header(r,2); unsigned count=u16(r->header,le);
    if ((int64_t)count*12+4>end-position(r)) fail("Truncated IFD.");
    int orientation=1,found=0;
    for (unsigned i=0;i<count;i++) {
        read_header(r,12); if (u16(r->header,le)!=0x0112) continue;
        if (found||u16(r->header+2,le)!=3||u32(r->header+4,le)!=1) fail("Invalid orientation field.");
        orientation=(int)u16(r->header+8,le); found=1;
        if (orientation<1||orientation>8) fail("Invalid orientation value.");
    }
    skip(r,end-position(r)); return orientation;
}
typedef struct { AVCodecParameters *parameters; int alpha,orientation,color,alpha_rep,components; } Descriptor;
typedef struct { uint8_t *input,*original; size_t size,last_read_bytes; unsigned last_reads,last_eof_reads,last_seeks,last_length_queries; int cpu; uint64_t completed,observed; } Work;
static Descriptor inspect(Work *w)
{
    Source source={.data=w->input,.size=w->size}; uint8_t *scratch=av_malloc(8192);
    if (!scratch) fail("Scratch allocation failed.");
    AVIOContext *io=avio_alloc_context(scratch,8192,0,&source,read_source,NULL,seek_source);
    if (!io) fail("AVIO allocation failed.");
    io->seekable=AVIO_SEEKABLE_NORMAL;
    Reader r={.io=io,.length=avio_size(io)};
    if (r.length<8||r.length>64L*1024*1024) fail("Input limit exceeded.");
    read_header(&r,8); if (memcmp(r.header,"\x89PNG\r\n\x1a\n",8)) fail("Invalid PNG signature.");
    unsigned width=0,height=0,depth=0,color=0,palette=0,items=0;
    int header=0,data=0,closed=0,transparent=0,has_exif=0,orientation=1;
    uint64_t compressed=0,metadata=0;
    for (;;) {
        if (++items>100000) fail("Container item limit exceeded.");
        read_header(&r,8); uint32_t length=be32(r.header); uint8_t type[4]; memcpy(type,r.header+4,4);
        for (int c=0;c<4;c++) if (!((type[c]>='A'&&type[c]<='Z')||(type[c]>='a'&&type[c]<='z'))) fail("Invalid chunk name.");
        int64_t end=position(&r)+length;
        if (length>INT32_MAX||end+4>r.length) fail("Invalid chunk length.");
        if (!header&&memcmp(type,"IHDR",4)) fail("IHDR must be first.");
        if (data&&memcmp(type,"IDAT",4)) closed=1;
        if (!memcmp(type,"IHDR",4)) {
            if (header||length!=13) fail("Invalid IHDR.");
            read_header(&r,13); width=be32(r.header); height=be32(r.header+4); depth=r.header[8]; color=r.header[9];
            int valid=color==0 ? depth==1||depth==2||depth==4||depth==8||depth==16 : color==3 ? depth==1||depth==2||depth==4||depth==8 : (color==2||color==4||color==6)&&(depth==8||depth==16);
            if (!valid||r.header[10]||r.header[11]||r.header[12]>1) fail("Invalid coding profile.");
            if (!width||!height||width>32768||height>32768||(uint64_t)width*height>32000000) fail("Dimension/pixel limit exceeded.");
            header=1;
        } else if (!memcmp(type,"PLTE",4)) {
            if (palette||data||transparent||color==0||color==4||length<3||length>768||length%3||(color==3&&length/3>(1U<<depth))) fail("Invalid palette.");
            palette=length;
        } else if (!memcmp(type,"tRNS",4)) {
            if (transparent||data||(color==3&&(!palette||!length||length>palette/3))||(color==0&&length!=2)||(color==2&&length!=6)||color==4||color==6) fail("Invalid transparency.");
            transparent=1;
            if (color==0||color==2) {
                read_header(&r,(int)length);
                for (unsigned c=0;c<length;c+=2) if (u16(r.header+c,0)>((1U<<depth)-1)) fail("Invalid transparency sample.");
            }
        } else if (!memcmp(type,"IDAT",4)) {
            if (closed||(color==3&&!palette)) fail("Invalid IDAT ordering.");
            data=1; compressed+=length;
        } else if (!memcmp(type,"IEND",4)) {
            if (length||!data||!compressed) fail("Invalid IEND.");
            skip(&r,4); uint8_t tail;
            if (avio_read(io,&tail,1)!=AVERROR_EOF||position(&r)!=r.length||!source.eof_reads||source.position!=source.size) fail("Trailing input or missing EOF.");
            break;
        } else {
            if (!memcmp(type,"acTL",4)||!memcmp(type,"fcTL",4)||!memcmp(type,"fdAT",4)) fail("Animation unsupported.");
            if (!(type[0]&32)) fail("Unknown critical chunk.");
            if (length>4U*1024*1024-metadata) fail("Metadata limit exceeded.");
            metadata+=length;
            if (!memcmp(type,"eXIf",4)) {
                if (has_exif||data) fail("Duplicate/misplaced EXIF.");
                orientation=exif(&r,length); has_exif=1;
            }
        }
        skip(&r,end-position(&r)); skip(&r,4); /* CRC intentionally opaque. */
    }
    Descriptor d={.parameters=avcodec_parameters_alloc(),.alpha=color==4||color==6||transparent,.orientation=orientation,
        .color=color==3?3:color==0||color==4?1:2,.alpha_rep=color==4||color==6?2:transparent?color==3?3:4:1,
        .components=color==0||color==3?1:color==2?3:color==4?2:4};
    if (!d.parameters) fail("Descriptor allocation failed.");
    d.parameters->codec_type=AVMEDIA_TYPE_VIDEO; d.parameters->codec_id=AV_CODEC_ID_PNG;
    d.parameters->width=(int)width; d.parameters->height=(int)height; d.parameters->bits_per_raw_sample=(int)depth;
    w->last_reads=source.reads; w->last_eof_reads=source.eof_reads; w->last_read_bytes=source.read_bytes;
    w->last_seeks=source.seeks; w->last_length_queries=source.length_queries;
    av_freep(&io->buffer); avio_context_free(&io); w->completed++; w->observed+=width+height+depth+d.alpha+d.orientation;
    return d;
}
static void release(Descriptor *d) { avcodec_parameters_free(&d->parameters); }
static void bytes(const Descriptor *d,uint8_t output[36])
{
    uint32_t values[]={0,(uint32_t)d->parameters->width,(uint32_t)d->parameters->height,(uint32_t)d->parameters->bits_per_raw_sample,
        (uint32_t)d->alpha,(uint32_t)d->orientation,(uint32_t)d->color,(uint32_t)d->alpha_rep,(uint32_t)d->components};
    for (int i=0;i<9;i++) for (int c=0;c<4;c++) output[i*4+c]=(uint8_t)(values[i]>>(8*c));
}
static void check(Work *w,const char *path)
{
    Descriptor d=inspect(w); uint8_t output[36]; bytes(&d,output); release(&d);
    if (memcmp(w->input,w->original,w->size)) fail("Source changed.");
    FILE *f=fopen(path,"wb"); if (!f||fwrite(output,1,36,f)!=36||fclose(f)) fail("Write descriptor failed.");
}
int main(int argc,char **argv)
{
    if ((argc!=4&&argc!=5)||strcmp(argv[1],"png-header-inspect")) fail("Use png-header-inspect INPUT OUTPUT [scalar].");
    int scalar=argc==5;
    if (scalar) { if (strcmp(argv[4],"scalar")) fail("Unknown mode."); av_force_cpu_flags(0); }
    Work w={0}; w.cpu=cpu(); FILE *f=fopen(argv[2],"rb");
    if (!f||fseek(f,0,SEEK_END)) fail("Open input failed.");
    long length=ftell(f); if (length<1||length>64L*1024*1024||fseek(f,0,SEEK_SET)) fail("Input limit exceeded.");
    w.size=(size_t)length; w.input=av_malloc(w.size); w.original=av_malloc(w.size);
    if (!w.input||!w.original||fread(w.input,1,w.size,f)!=w.size||fclose(f)) fail("Read input failed.");
    memcpy(w.original,w.input,w.size);
    Descriptor first=inspect(&w),second=inspect(&w); uint8_t a[36],b[36]; bytes(&first,a); bytes(&second,b);
    if (first.parameters==second.parameters||memcmp(a,b,36)) fail("Live descriptors alias/differ.");
    first.parameters->width^=1; release(&first); bytes(&second,b); if (memcmp(a,b,36)) fail("Surviving descriptor changed.");
    unsigned width=second.parameters->width,height=second.parameters->height; int depth=second.parameters->bits_per_raw_sample,alpha=second.alpha,orientation=second.orientation;
    release(&second); check(&w,argv[3]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"png-header-inspect\",\"backend\":\"FFmpeg AVIO/owned descriptor APIs with original bounded header/EXIF adapter\",\"width\":%u,\"height\":%u,\"depth\":%d,\"hasAlpha\":%s,\"orientation\":%d,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"conversionThreads\":0,\"threadType\":0,\"activeThreadType\":0,\"outputBytes\":36,\"typedSourceMetadata\":true,\"fuelAccounting\":false,\"inputBytes\":%zu,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"freshSeekableStream\":true,\"inputView\":false,\"wholeInputCopy\":false,\"scratchBytes\":8192,\"headerBytes\":32,\"eofChecked\":true,\"callerSourceLeftOpen\":true,\"streamAcquisitionAndReleaseTimed\":true,\"publicParserTimed\":false,\"fullCrcTimed\":false,\"structureMetadataTimed\":true,\"descriptorAcquisitionAndReleaseTimed\":true,\"pixelDecode\":false,\"opaqueSeekEnabled\":true,\"readBytes\":%zu,\"reads\":%u,\"eofReads\":%u,\"seekCalls\":%u,\"lengthQueries\":%u,\"scalarMode\":%s,\"ffmpegVersion\":\"%s\"}\n",width,height,depth,alpha?"true":"false",orientation,w.cpu,w.size,w.completed,w.last_read_bytes,w.last_reads,w.last_eof_reads,w.last_seeks,w.last_length_queries,scalar?"true":"false",av_version_info());
    fflush(stdout); char line[128];
    while (fgets(line,sizeof(line),stdin)) {
        if (!strcmp(line,"quit\n")) break;
        if (!strcmp(line,"check\n")) { check(&w,argv[3]); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue; }
        uint64_t count; char extra;
        if (sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1||!count||count>10000000) fail("Invalid sample count.");
        uint64_t completed=w.completed,start=now();
        for (uint64_t i=0;i<count;i++) { Descriptor d=inspect(&w); release(&d); }
        uint64_t elapsed=now()-start;
        if (cpu()!=w.cpu||sched_getcpu()!=w.cpu||w.completed-completed!=count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"outputBytes\":36,\"cpu\":%d}\n",count,elapsed,w.completed,w.observed,w.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_free(w.input); av_free(w.original); return 0;
}
