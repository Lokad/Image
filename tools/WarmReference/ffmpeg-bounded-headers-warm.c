/* Original benchmark-only bounded JPEG/WebP header adapter to public FFmpeg
 * AVIO/owned descriptor APIs. Not public packet-parser performance.
 * T.81 annex B; RFC 9649 RIFF/VP8X/VP8L/VP8; CIPA DC-008 TIFF IFD0.
 * No upstream implementation, tables or corpus. No pixel/entropy decoding. */
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
#include <libavutil/mem.h>

static void fail(const char *s) { fprintf(stderr,"%s\n",s);
    exit(1);
}
static uint32_t be32(const uint8_t *p) { return (uint32_t)p[0]<<24|(uint32_t)p[1]<<16|(uint32_t)p[2]<<8|p[3];
}
static unsigned u16(const uint8_t *p,int le) { return le?p[0]|(unsigned)p[1]<<8:(unsigned)p[0]<<8|p[1];
}
static uint32_t u32(const uint8_t *p,int le) { return le?(uint32_t)p[3]<<24|(uint32_t)p[2]<<16|(uint32_t)p[1]<<8|p[0]:be32(p);
}
static unsigned u24(const uint8_t *p) { return p[0]|(unsigned)p[1]<<8|(unsigned)p[2]<<16;
}
static int cpu(void)
{
    cpu_set_t mask;
    if(sched_getaffinity(0,sizeof(mask),&mask)||CPU_COUNT(&mask)!=1) fail("Exactly one CPU required.");
    for(int c=0;c<CPU_SETSIZE;c++) if(CPU_ISSET(c,&mask)) return c;
    fail("No CPU.");
    return -1;
}
static uint64_t now(void)
{
    struct timespec t;
    if(clock_gettime(CLOCK_MONOTONIC,&t)) fail("Clock failed.");
    return (uint64_t)t.tv_sec*UINT64_C(1000000000)+(uint64_t)t.tv_nsec;
}
typedef struct { const uint8_t *data;
    size_t size,position,bytes;
    unsigned reads,eof,seeks,lengths;
} Source;
static int read_source(void *opaque,uint8_t *bytes,int count)
{
    Source *s=opaque;
    s->reads++;
    size_t left=s->size-s->position;
    if(!left) { s->eof++;
        return AVERROR_EOF;
    }
    if((size_t)count>left) count=(int)left;
    memcpy(bytes,s->data+s->position,(size_t)count);
    s->position+=(size_t)count;
    s->bytes+=(size_t)count;
    return count;
}
static int64_t seek_source(void *opaque,int64_t offset,int whence)
{
    Source *s=opaque;
    if(whence==AVSEEK_SIZE) { s->lengths++;
        return (int64_t)s->size;
    }
    whence&=~AVSEEK_FORCE;
    int64_t start=whence==SEEK_SET?0:whence==SEEK_CUR?(int64_t)s->position:whence==SEEK_END?(int64_t)s->size:-1;
    if(start<0||offset < -start||offset>(int64_t)s->size-start) return AVERROR(EINVAL);
    s->position=(size_t)(start+offset);
    s->seeks++;
    return (int64_t)s->position;
}
/* Independent bounded staging permits public AVIO calls and libc memchr over
 * acquired bytes, without borrowing an encoded-input view or packet buffering. */
typedef struct { AVIOContext *io;
    uint8_t buffer[8192],header[32];
    int first,last;
    int64_t position,length;
} Reader;
static int fill(Reader *r)
{
    if(r->first<r->last) return 1;
    int n=avio_read_partial(r->io,r->buffer,sizeof(r->buffer));
    if(n==AVERROR_EOF) { r->first=r->last=0;
        return 0;
    }
    if(n<=0) fail("Input read failed.");
    r->first=0;
    r->last=n;
    return 1;
}
static int byte(Reader *r)
{
    if(!fill(r)) return -1;
    r->position++;
    return r->buffer[r->first++];
}
static void read_header(Reader *r,int count)
{
    if(count<0||count>32) fail("Header request out of bounds.");
    for(int copied=0;copied<count;) {
        if(!fill(r)) fail("Truncated header.");
        int n=r->last-r->first;
        if(n>count-copied) n=count-copied;
        memcpy(r->header+copied,r->buffer+r->first,(size_t)n);
        r->first+=n;
        r->position+=n;
        copied+=n;
    }
}
static void skip_to(Reader *r,int64_t end)
{
    if(end<r->position||end>r->length) fail("Truncated skipped payload.");
    int64_t count=end-r->position;
    if(count<=r->last-r->first) r->first+=(int)count;
    else { if(avio_seek(r->io,end,SEEK_SET)!=end) fail("Seek failed.");
        r->first=r->last=0;
    }
    r->position=end;
}
static void end_input(Reader *r)
{
    if(r->position!=r->length||byte(r)!=-1) fail("Trailing input or missing EOF.");
}
static void dimensions(unsigned width,unsigned height)
{
    if(!width||!height||width>32768||height>32768||(uint64_t)width*height>32000000) fail("Dimension limit exceeded.");
}
static int exif(Reader *r,uint32_t size,int prefix)
{
    int64_t end=r->position+size;
    uint8_t tiff[8];
    if(size<8||end>r->length) fail("Truncated EXIF.");
    read_header(r,8);
    memcpy(tiff,r->header,8);
    if(prefix&&!memcmp(tiff,"Exif\0\0",6)) {
        if(size<14) fail("Truncated prefixed EXIF.");
        tiff[0]=tiff[6];
        tiff[1]=tiff[7];
        read_header(r,6);
        memcpy(tiff+2,r->header,6);
    }
    int le=!memcmp(tiff,"II",2);
    if((!le&&memcmp(tiff,"MM",2))||u16(tiff+2,le)!=42) fail("Invalid TIFF header.");
    uint32_t offset=u32(tiff+4,le);
    if(offset<8||(int64_t)offset-8>end-r->position-2) fail("Invalid IFD offset.");
    skip_to(r,r->position+offset-8);
    read_header(r,2);
    unsigned count=u16(r->header,le);
    if((int64_t)count*12+4>end-r->position) fail("Truncated IFD.");
    int orientation=1,found=0;
    for(unsigned i=0;i<count;i++) {
        read_header(r,12);
        if(u16(r->header,le)!=0x0112) continue;
        if(found||u16(r->header+2,le)!=3||u32(r->header+4,le)!=1) fail("Invalid orientation field.");
        orientation=(int)u16(r->header+8,le);
        found=1;
        if(orientation<1||orientation>8) fail("Invalid orientation value.");
    }
    skip_to(r,end);
    return orientation;
}
typedef struct { AVCodecParameters *parameters;
    unsigned format,alpha,orientation,color,alpha_rep,components;
} Descriptor;
static int marker(Reader *r,int entropy)
{
    if(entropy) {
        for(;;) {
            if(!fill(r)) fail("JPEG missing EOI.");
            const uint8_t *at=memchr(r->buffer+r->first,255,(size_t)(r->last-r->first));
            int n=at?(int)(at-r->buffer)-r->first+1:r->last-r->first;
            r->first+=n;
            r->position+=n;
            if(at) break;
        }
    } else if(byte(r)!=255) fail("Invalid JPEG marker prefix.");
    int value;
    do { value=byte(r);
    } while(value==255);
    if(value<0) fail("Truncated JPEG marker.");
    return value;
}
static Descriptor jpeg(Reader *r)
{
    read_header(r,2);
    if(r->header[0]!=255||r->header[1]!=216) fail("Invalid JPEG SOI.");
    unsigned width=0,height=0,components=0,items=0;
    uint32_t identifiers=0;
    uint64_t metadata=0;
    int frame=0,scan=0,seen=0,has_exif=0,orientation=1,adobe=-1,jfif=0;
    for(;;) {
        int m=marker(r,scan);
        if(scan&&!m) continue;
        if(++items>100000) fail("Container limit exceeded.");
        if(m>=0xd0&&m<=0xd7) { if(!scan) fail("Restart outside scan.");
            continue;
        }
        scan=0;
        if(m==0xd9) { if(!frame||!seen) fail("Invalid JPEG end.");
            end_input(r);
            break;
        }
        if(!m||m==0xd8||m==1) fail("Invalid JPEG marker sequence.");
        read_header(r,2);
        unsigned length=u16(r->header,0);
        if(length<2) fail("Invalid JPEG segment length.");
        unsigned size=length-2;
        int64_t end=r->position+size;
        if(end>r->length) fail("Truncated JPEG segment.");
        if(m>=0xc0&&m<=0xcf&&m!=0xc4&&m!=0xc8&&m!=0xcc) {
            if((m!=0xc0&&m!=0xc2)||frame||size<6) fail("Unsupported/invalid JPEG frame.");
            read_header(r,6);
            if(r->header[0]!=8) fail("Unsupported JPEG depth.");
            height=u16(r->header+1,0);
            width=u16(r->header+3,0);
            components=r->header[5];
            if((components!=1&&components!=3&&components!=4)||size!=6+3*components) fail("Invalid JPEG components.");
            dimensions(width,height);
            read_header(r,(int)components*3);
            for(unsigned i=0;i<components;i++) identifiers|=(uint32_t)r->header[i*3]<<(i*8);
            frame=1;
        } else if(m==0xda) {
            if(!frame||size<4) fail("Invalid JPEG scan header.");
            read_header(r,1);
            unsigned count=r->header[0];
            if(!count||count>4||size!=4+2*count) fail("Invalid JPEG scan components.");
            scan=seen=1;
        } else if((m>=0xe0&&m<=0xef)||m==0xfe) {
            metadata+=size;
            if(metadata>4*1024*1024) fail("Metadata limit exceeded.");
            if(m==0xe0&&size>=5) { read_header(r,5);
                if(!memcmp(r->header,"JFIF\0",5)) jfif=1;
            }
            else if(m==0xee&&size>=12) {
                read_header(r,12);
                if(!memcmp(r->header,"Adobe",5)) adobe=adobe==-1||adobe==r->header[11]?r->header[11]:-2;
            } else if(m==0xe1&&size>=6) {
                read_header(r,6);
                if(!memcmp(r->header,"Exif\0\0",6)) { if(has_exif) fail("Duplicate JPEG EXIF.");
                    orientation=exif(r,size-6,0);
                    has_exif=1;
                }
            }
        }
        skip_to(r,end);
    }
    unsigned color=components==1?1:components==3&&adobe==0?2:components==3&&adobe==1?4:
    components==4&&adobe==0?5:components==4&&adobe==2?6:components==3&&adobe==-1&&jfif?4:
    components==3&&adobe==-1&&identifiers==0x00424752?2:components==4&&adobe==-1&&identifiers==0x4b594d43?5:0;
    Descriptor d={.parameters=avcodec_parameters_alloc(),.format=1,.orientation=(unsigned)orientation,.color=color,.alpha_rep=1,.components=components};
    if(!d.parameters) fail("Descriptor allocation failed.");
    d.parameters->codec_type=AVMEDIA_TYPE_VIDEO;
    d.parameters->codec_id=AV_CODEC_ID_MJPEG;
    d.parameters->width=(int)width;
    d.parameters->height=(int)height;
    d.parameters->bits_per_raw_sample=8;
    return d;
}
static Descriptor webp(Reader *r)
{
    read_header(r,12);
    if(memcmp(r->header,"RIFF",4)||memcmp(r->header+8,"WEBP",4)) fail("Invalid WebP signature.");
    int64_t end=(int64_t)u32(r->header+4,1)+8;
    if(end<12||end!=r->length) fail("Invalid RIFF length.");
    unsigned width=0,height=0,cw=0,ch=0,items=0;
    uint64_t metadata=0;
    int extended=0,pixels=0,alpha=0,alpha_chunk=0,lossless=0,has_exif=0,icc=0,xmp=0,orientation=1;
    uint8_t flags=0;
    while(r->position<end) {
        if(++items>100000||end-r->position<8) fail("Invalid WebP chunk framing.");
        read_header(r,8);
        uint8_t type[4];
        memcpy(type,r->header,4);
        uint32_t size=u32(r->header+4,1);
        int64_t payload_end=r->position+size;
        if((uint64_t)size+(size&1)>(uint64_t)(end-r->position)) fail("Invalid WebP chunk length.");
        if(!memcmp(type,"VP8X",4)) {
            if(items!=1||size<10) fail("Invalid VP8X.");
            read_header(r,10);
            flags=r->header[0];
            if(flags&2) fail("Animation unsupported.");
            cw=u24(r->header+4)+1;
            ch=u24(r->header+7)+1;
            dimensions(cw,ch);
            extended=1;
            alpha=(flags&16)!=0;
        } else if(!memcmp(type,"ANIM",4)||!memcmp(type,"ANMF",4)) fail("Animation unsupported.");
        else if(!memcmp(type,"VP8L",4)) {
            if(pixels||alpha_chunk||size<5) fail("Invalid VP8L.");
            read_header(r,5);
            if(r->header[0]!=0x2f) fail("Invalid VP8L signature.");
            uint32_t bits=u32(r->header+1,1);
            if(bits>>29) fail("Unsupported VP8L version.");
            width=(bits&0x3fff)+1;
            height=((bits>>14)&0x3fff)+1;
            alpha|=(bits&(1U<<28))!=0;
            pixels=lossless=1;
        } else if(!memcmp(type,"VP8 ",4)) {
            if(pixels||size<10) fail("Invalid VP8.");
            read_header(r,10);
            if((r->header[0]&1)||memcmp(r->header+3,"\x9d\x01\x2a",3)) fail("Invalid VP8 keyframe.");
            width=u16(r->header+6,1)&0x3fff;
            height=u16(r->header+8,1)&0x3fff;
            if(!width||!height) fail("Invalid VP8 dimensions.");
            pixels=1;
        } else if(!memcmp(type,"ALPH",4)) {
            if(!extended||pixels||alpha_chunk||!size||!alpha) fail("Invalid ALPH.");
            alpha_chunk=1;
        } else {
            metadata+=size;
            if(metadata>4*1024*1024) fail("Metadata limit exceeded.");
            if(!memcmp(type,"EXIF",4)) {
                if(!extended||has_exif||!(flags&8)) fail("Invalid WebP EXIF.");
                orientation=exif(r,size,1);
                has_exif=1;
            } else if(!memcmp(type,"ICCP",4)) {
                if(!extended||icc||pixels||!(flags&32)) fail("Invalid ICC chunk.");
                icc=1;
            } else if(!memcmp(type,"XMP ",4)) {
                if(!extended||xmp||!(flags&4)) fail("Invalid XMP chunk.");
                xmp=1;
            }
        }
        skip_to(r,payload_end);
        if((size&1)&&byte(r)!=0) fail("Invalid WebP padding.");
    }
    if(!pixels||(extended&&(width!=cw||height!=ch))) fail("Invalid WebP geometry.");
    if(extended&&(((flags&8)!=0)!=has_exif||((flags&32)!=0)!=icc||((flags&4)!=0)!=xmp||(!lossless&&alpha!=alpha_chunk))) fail("WebP flags disagree.");
    dimensions(width,height);
    end_input(r);
    Descriptor d={.parameters=avcodec_parameters_alloc(),.format=2,.alpha=(unsigned)alpha,.orientation=(unsigned)orientation,.color=lossless?2:4,.alpha_rep=lossless||alpha_chunk?2:1,.components=lossless||alpha_chunk?4:3};
    if(!d.parameters) fail("Descriptor allocation failed.");
    d.parameters->codec_type=AVMEDIA_TYPE_VIDEO;
    d.parameters->codec_id=AV_CODEC_ID_WEBP;
    d.parameters->width=(int)width;
    d.parameters->height=(int)height;
    d.parameters->bits_per_raw_sample=8;
    return d;
}
typedef struct { uint8_t *input,*original;
    size_t size;
    int format,cpu;
    uint64_t completed,observed;
    Source last;
} Work;
static Descriptor inspect(Work *w)
{
    Source source={.data=w->input,.size=w->size};
    uint8_t *scratch=av_malloc(8192);
    if(!scratch) fail("Scratch allocation failed.");
    AVIOContext *io=avio_alloc_context(scratch,8192,0,&source,read_source,NULL,seek_source);
    if(!io) fail("AVIO allocation failed.");
    io->seekable=AVIO_SEEKABLE_NORMAL;
    Reader r={.io=io,.length=avio_size(io)};
    if(r.length<1||r.length>64L*1024*1024) fail("Input limit exceeded.");
    Descriptor d=w->format==1?jpeg(&r):webp(&r);
    if(!source.eof||source.position!=source.size) fail("EOF acquisition missing.");
    w->last=source;
    av_freep(&io->buffer);
    avio_context_free(&io);
    w->completed++;
    w->observed+=(unsigned)d.parameters->width+(unsigned)d.parameters->height+d.orientation+d.color;
    return d;
}
static void release(Descriptor *d) { avcodec_parameters_free(&d->parameters);
}
static void bytes(const Descriptor *d,uint8_t output[36])
{
    uint32_t v[]={d->format,(uint32_t)d->parameters->width,(uint32_t)d->parameters->height,8,d->alpha,d->orientation,d->color,d->alpha_rep,d->components};
    for(int i=0;i<9;i++) for(int c=0;c<4;c++) output[i*4+c]=(uint8_t)(v[i]>>(c*8));
}
static void check(Work *w,const char *path)
{
    Descriptor d=inspect(w);
    uint8_t output[36];
    bytes(&d,output);
    release(&d);
    if(memcmp(w->input,w->original,w->size)) fail("Borrowed input changed.");
    FILE *f=fopen(path,"wb");
    if(!f||fwrite(output,1,36,f)!=36||fclose(f)) fail("Write descriptor failed.");
}
int main(int argc,char **argv)
{
    if(argc!=4||(strcmp(argv[1],"jpeg")&&strcmp(argv[1],"webp"))) fail("Use jpeg|webp INPUT OUTPUT.");
    Work w={.format=!strcmp(argv[1],"jpeg")?1:2};
    w.cpu=cpu();
    FILE *f=fopen(argv[2],"rb");
    if(!f||fseek(f,0,SEEK_END)) fail("Open input failed.");
    long size=ftell(f);
    if(size<1||size>64L*1024*1024||fseek(f,0,SEEK_SET)) fail("Input limit exceeded.");
    w.size=(size_t)size;
    w.input=av_malloc(w.size);
    w.original=av_malloc(w.size);
    if(!w.input||!w.original||fread(w.input,1,w.size,f)!=w.size||fclose(f)) fail("Read input failed.");
    memcpy(w.original,w.input,w.size);
    Descriptor first=inspect(&w),second=inspect(&w);
    uint8_t a[36],b[36];
    bytes(&first,a);
    bytes(&second,b);
    if(first.parameters==second.parameters||memcmp(a,b,36)) fail("Independent descriptor check failed.");
    first.parameters->width^=1;
    release(&first);
    bytes(&second,a);
    release(&second);
    if(memcmp(a,b,36)) fail("Surviving descriptor changed.");
    check(&w,argv[3]);
    printf("{\"state\":\"ready\",\"format\":%d,\"backend\":\"FFmpeg AVIO/owned descriptor APIs plus original bounded structural/EXIF adapter;not public packet-parser performance\",\"cpu\":%d,\"inputBytes\":%zu,\"independentLiveOutputs\":true,\"freshSeekableStream\":true,\"inputView\":false,\"wholeInputCopy\":false,\"avioScratchBytes\":8192,\"adapterBufferBytes\":8192,\"headerBytes\":32,\"eofChecked\":true,\"pixelDecode\":false,\"sourceAcquisitionAndReleaseTimed\":true,\"descriptorAcquisitionAndReleaseTimed\":true,\"reads\":%u,\"readBytes\":%zu,\"seeks\":%u,\"ffmpegVersion\":\"%s\"}\n",w.format,w.cpu,w.size,w.last.reads,w.last.bytes,w.last.seeks,av_version_info());
    fflush(stdout);
    char line[128];
    while(fgets(line,sizeof(line),stdin)) {
        if(!strcmp(line,"quit\n")) break;
        if(!strcmp(line,"check\n")) { check(&w,argv[3]);
            puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}");
            fflush(stdout);
            continue;
        }
        uint64_t count;
        char extra;
        if(sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1||!count||count>10000000) fail("Invalid sample request.");
        uint64_t start=now();
        for(uint64_t i=0;i<count;i++) { Descriptor d=inspect(&w);
            release(&d);
        }
        uint64_t elapsed=now()-start;
        if(cpu()!=w.cpu||sched_getcpu()!=w.cpu) fail("CPU changed.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"cpu\":%d,\"completed\":%" PRIu64 ",\"observed\":%" PRIu64 "}\n",count,elapsed,w.cpu,w.completed,w.observed);
        fflush(stdout);
    }
    if(ferror(stdin)) fail("Protocol read failed.");
    av_free(w.input);
    av_free(w.original);
    return 0;
}
