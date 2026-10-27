/* Original development-only FFmpeg public-API adapter. PNG3, T.81, RFC9649
 * and CIPA TIFF/IFD0 define the independently written header grammar below.
 * Probe: fresh FFmpeg AVIO + bounded original header adapter, not pixel decode.
 * Validate: complete libavcodec natural-frame decode + source metadata adapter;
 * no swscale, caller raster or output pixel copy. No upstream code is included. */
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
#include <libavutil/avutil.h>
#include <libavutil/cpu.h>
#include <libavutil/error.h>
#include <libavutil/mem.h>

#define INPUT_LIMIT (64U*1024*1024)
#define METADATA_LIMIT (4U*1024*1024)
#define ITEM_LIMIT 100000U
static void fail(const char *message) { fprintf(stderr,"%s\n",message); exit(1); }
static void require(int code,const char *message)
{ if(code<0) { char error[AV_ERROR_MAX_STRING_SIZE];av_strerror(code,error,sizeof(error));fprintf(stderr,"%s: %s\n",message,error);exit(1); } }
static uint32_t be32(const uint8_t *p) { return (uint32_t)p[0]<<24|(uint32_t)p[1]<<16|(uint32_t)p[2]<<8|p[3]; }
static uint32_t le32(const uint8_t *p) { return (uint32_t)p[3]<<24|(uint32_t)p[2]<<16|(uint32_t)p[1]<<8|p[0]; }
static unsigned u16(const uint8_t *p,int little) { return little ? p[0]|(unsigned)p[1]<<8 : (unsigned)p[0]<<8|p[1]; }
static uint32_t u32(const uint8_t *p,int little) { return little ? le32(p) : be32(p); }
static unsigned le24(const uint8_t *p) { return p[0]|(unsigned)p[1]<<8|(unsigned)p[2]<<16; }
static int cpu(void)
{
    cpu_set_t set;if(sched_getaffinity(0,sizeof(set),&set)||CPU_COUNT(&set)!=1){ fail("Exactly one CPU required."); }
    for(int i=0;i<CPU_SETSIZE;i++)if(CPU_ISSET(i,&set)){ return i; }
    fail("Missing CPU.");return -1;
}
static uint64_t clock_ns(clockid_t clock)
{ struct timespec t;if(clock_gettime(clock,&t)){ fail("Clock failed."); }return (uint64_t)t.tv_sec*UINT64_C(1000000000)+t.tv_nsec; }
static void dimensions(unsigned w,unsigned h)
{ if(!w||!h||w>32768||h>32768||(uint64_t)w*h>32000000){ fail("Dimension/pixel limit."); } }

typedef struct { const uint8_t *data;size_t size,position,read_bytes,allowed;unsigned reads,seeks;int exact; } Source;
static int read_source(void *opaque,uint8_t *buffer,int count)
{
    Source *s=opaque;s->reads++;
    size_t left=s->size-s->position;if(!left){ return AVERROR_EOF; }
    if(count>8192){ count=8192; }
    if(s->exact&&s->allowed<(size_t)count){ count=(int)s->allowed; }
    if(count<=0){ fail("Unrequested prefix read."); }
    if((size_t)count>left){ count=(int)left; }
    if(s->exact&&(size_t)count>1024U*1024-s->read_bytes){ fail("Probe acquisition limit."); }
    memcpy(buffer,s->data+s->position,(size_t)count);s->position+=(size_t)count;s->read_bytes+=(size_t)count;
    if(s->exact){ s->allowed-=(size_t)count; }
    return count;
}
static int64_t seek_source(void *opaque,int64_t offset,int whence)
{
    Source *s=opaque;if(whence==AVSEEK_SIZE){ return (int64_t)s->size; }
    whence&=~AVSEEK_FORCE;
    int64_t origin=whence==SEEK_SET?0:whence==SEEK_CUR?(int64_t)s->position:whence==SEEK_END?(int64_t)s->size:-1;
    if(origin<0||offset < -origin||offset>(int64_t)s->size-origin){ return AVERROR(EINVAL); }
    s->position=(size_t)(origin+offset);s->seeks++;return (int64_t)s->position;
}
typedef struct { AVIOContext *io;Source *source;const uint8_t *span;size_t length,position;uint8_t h[32];unsigned items;size_t metadata; } Reader;
static AVIOContext *open_source(Source *source,int seekable)
{
    uint8_t *scratch=av_malloc(8192);if(!scratch){ fail("Input scratch allocation."); }
    AVIOContext *io=avio_alloc_context(scratch,8192,0,source,read_source,NULL,seekable?seek_source:NULL);
    if(!io){ fail("AVIO allocation."); }io->seekable=seekable?AVIO_SEEKABLE_NORMAL:0;io->direct=1;return io;
}
static void close_source(AVIOContext **io) { av_freep(&(*io)->buffer);avio_context_free(io); }
static void read_header(Reader *r,unsigned count)
{
    if(count>32||r->position>INPUT_LIMIT-count){ fail("Header/input limit."); }
    if(r->io) {
        r->source->allowed=count;
        if(avio_read(r->io,r->h,(int)count)!=(int)count){ fail("Truncated header."); }
    } else {
        if(count>r->length-r->position){ fail("Truncated header."); }
        memcpy(r->h,r->span+r->position,count);
    }
    r->position+=count;
}
static void skip(Reader *r,size_t count)
{
    if(count>INPUT_LIMIT-r->position){ fail("Input limit."); }
    if(r->io) {
        r->source->allowed=count;
        if(avio_skip(r->io,(int64_t)count)!=(int64_t)(r->position+count)){ fail("Skip failed."); }
    } else if(count>r->length-r->position){ fail("Truncated payload."); }
    r->position+=count;
}
static void item(Reader *r) { if(++r->items>ITEM_LIMIT){ fail("Container item limit."); } }
static void metadata(Reader *r,size_t count)
{ if(count>METADATA_LIMIT-r->metadata){ fail("Metadata limit."); }r->metadata+=count; }
static int exif(Reader *r,size_t count)
{
    size_t end=r->position+count;if(count<8){ fail("Truncated EXIF."); }
    read_header(r,8);uint8_t h[8];memcpy(h,r->h,8);
    if(!memcmp(h,"Exif\0\0",6)) {
        if(count<14){ fail("Truncated prefixed EXIF."); }h[0]=h[6];h[1]=h[7];read_header(r,6);memcpy(h+2,r->h,6);
    }
    int little=!memcmp(h,"II",2);
    if((!little&&memcmp(h,"MM",2))||u16(h+2,little)!=42){ fail("Invalid TIFF."); }
    uint32_t offset=u32(h+4,little);
    if(offset<8||offset-8U>end-r->position||end-r->position-(offset-8U)<2){ fail("Invalid IFD offset."); }
    skip(r,offset-8U);read_header(r,2);unsigned entries=u16(r->h,little);
    if((uint64_t)entries*12+4>end-r->position){ fail("Truncated IFD."); }
    int result=1,found=0;
    for(unsigned i=0;i<entries;i++) {
        read_header(r,12);if(u16(r->h,little)!=0x0112){ continue; }
        if(found||u16(r->h+2,little)!=3||u32(r->h+4,little)!=1){ fail("Invalid orientation tag."); }
        result=(int)u16(r->h+8,little);found=1;if(result<1||result>8){ fail("Invalid orientation."); }
    }
    skip(r,end-r->position);return result;
}
typedef struct { unsigned format,width,height,depth,color,alpha_rep,components;int alpha,orientation; } Info;
static Info png_info(Reader *r,int probe)
{
    read_header(r,6);static const uint8_t tail[]={78,71,13,10,26,10};
    if(memcmp(r->h,tail,6)){ fail("Invalid PNG signature."); }
    Info info={.format=0,.orientation=1};int color=-1,header=0,data=0,trns=0,exif_seen=0,ended=0;
    unsigned palette=0;int closed=0;
    do {
        item(r);read_header(r,8);uint32_t size=be32(r->h);uint8_t type[4];memcpy(type,r->h+4,4);
        if(size>INT32_MAX||size>INPUT_LIMIT-r->position-4){ fail("Invalid PNG chunk."); }
        size_t end=r->position+size;
        if(!header&&memcmp(type,"IHDR",4)){ fail("PNG missing IHDR."); }
        if(data&&memcmp(type,"IDAT",4)){ closed=1; }
        if(!memcmp(type,"IHDR",4)) {
            if(header||size!=13){ fail("Invalid IHDR."); }read_header(r,13);
            info.width=be32(r->h);info.height=be32(r->h+4);info.depth=r->h[8];color=r->h[9];
            int valid=color==0?(info.depth==1||info.depth==2||info.depth==4||info.depth==8||info.depth==16):
                color==3?(info.depth==1||info.depth==2||info.depth==4||info.depth==8):
                (color==2||color==4||color==6)&&(info.depth==8||info.depth==16);
            if(!valid||r->h[10]||r->h[11]||r->h[12]>1){ fail("Unsupported PNG coding."); }dimensions(info.width,info.height);header=1;
            if(probe){ return info; }
        } else if(!memcmp(type,"PLTE",4)) {
            if(palette||data||trns||color==0||color==4||size<3||size>768||size%3||(color==3&&size/3>(1U<<info.depth))){ fail("Invalid palette."); }palette=size;
        } else if(!memcmp(type,"tRNS",4)) {
            if(trns||data||(color==3&&(!palette||!size||size>palette/3))||(color==0&&size!=2)||(color==2&&size!=6)||color==4||color==6){ fail("Invalid transparency."); }trns=1;
        } else if(!memcmp(type,"IDAT",4)) {
            if(closed||(color==3&&!palette)){ fail("Invalid IDAT order."); }data=1;
        } else if(!memcmp(type,"IEND",4)) {
            if(size||!data){ fail("Invalid IEND."); }ended=1;
        } else if(!memcmp(type,"acTL",4)||!memcmp(type,"fcTL",4)||!memcmp(type,"fdAT",4)){ fail("Animation unsupported."); }
        else {
            if(!(type[0]&32)){ fail("Unknown critical PNG chunk."); }metadata(r,size);
            if(!memcmp(type,"eXIf",4)) { if(exif_seen||data){ fail("Duplicate/misplaced EXIF."); }info.orientation=exif(r,size);exif_seen=1; }
        }
        skip(r,end-r->position);skip(r,4);
    } while(!ended);
    if(r->position!=r->length){ fail("PNG trailing bytes."); }
    info.color=(color==0||color==4)?1:color==3?3:2;
    info.alpha=color==4||color==6||trns;
    info.alpha_rep=(color==4||color==6)?2:!trns?1:color==3?3:4;
    info.components=(color==0||color==3)?1:color==2?3:color==4?2:4;
    return info;
}
static int marker(Reader *r,int entropy)
{
    if(entropy&&!r->io) {
        const uint8_t *found=memchr(r->span+r->position,255,r->length-r->position);
        if(!found){ fail("JPEG missing EOI."); }r->position=(size_t)(found-r->span)+1;
    } else { read_header(r,1);if(r->h[0]!=255){ fail("Invalid JPEG marker."); } }
    do { read_header(r,1); } while(r->h[0]==255);
    return r->h[0];
}
static Info jpeg_info(Reader *r,int probe)
{
    Info info={.format=1,.depth=8,.alpha_rep=1,.orientation=1};
    int frame=0,scan=0,scan_seen=0,adobe=-1,jfif=0,exif_seen=0;uint32_t ids=0;
    for(;;) {
        int m=marker(r,scan);if(scan&&m==0){ continue; }item(r);
        if(m>=0xd0&&m<=0xd7) { if(!scan){ fail("Restart outside scan."); }continue; }scan=0;
        if(m==0xd9) { if(!frame||!scan_seen||r->position!=r->length){ fail("Invalid JPEG end."); }break; }
        if(!m||m==0xd8||m==1){ fail("Invalid JPEG marker sequence."); }
        read_header(r,2);unsigned size=u16(r->h,0);if(size<2){ fail("Invalid JPEG segment."); }size-=2;size_t end=r->position+size;
        if(end>INPUT_LIMIT){ fail("JPEG input limit."); }
        if(m>=0xc0&&m<=0xcf&&m!=0xc4&&m!=0xc8&&m!=0xcc) {
            if((m!=0xc0&&m!=0xc2)||frame||size<6){ fail("Unsupported JPEG frame."); }read_header(r,6);
            if(r->h[0]!=8){ fail("Unsupported JPEG precision."); }info.height=u16(r->h+1,0);info.width=u16(r->h+3,0);info.components=r->h[5];
            if((info.components!=1&&info.components!=3&&info.components!=4)||size!=6+info.components*3){ fail("Invalid JPEG components."); }
            dimensions(info.width,info.height);read_header(r,info.components*3);ids=0;
            for(unsigned i=0;i<info.components;i++)ids|=(uint32_t)r->h[i*3]<<(i*8);
            frame=1;if(probe){ return info; }
        } else if(m==0xda) { if(!frame||size<4){ fail("Invalid scan."); }scan=scan_seen=1; }
        else if((m>=0xe0&&m<=0xef)||m==0xfe) {
            metadata(r,size);
            if(m==0xe0&&size>=5) { read_header(r,5);jfif|=!memcmp(r->h,"JFIF\0",5); }
            else if(m==0xee&&size>=12) { read_header(r,12);if(!memcmp(r->h,"Adobe",5)){ adobe=adobe==-1||adobe==r->h[11]?r->h[11]:-2; } }
            else if(m==0xe1&&size>=6) {
                read_header(r,6);
                if(!memcmp(r->h,"Exif\0\0",6)) { if(exif_seen){ fail("Duplicate EXIF."); }info.orientation=exif(r,size-6);exif_seen=1; }
            }
        }
        skip(r,end-r->position);
    }
    if(info.components==1){ info.color=1; }
    else if(info.components==3){ info.color=adobe==0?2:adobe==1?4:adobe==-1?(jfif?4:ids==0x00424752?2:0):0; }
    else info.color=adobe==0?5:adobe==2?6:adobe==-1&&ids==0x4b594d43?5:0;
    return info;
}
static Info webp_info(Reader *r,int probe)
{
    read_header(r,10);if(memcmp(r->h,"FF",2)||memcmp(r->h+6,"WEBP",4)){ fail("Invalid RIFF."); }
    size_t end=(size_t)le32(r->h+2)+8;if(end<12||end>INPUT_LIMIT){ fail("Invalid RIFF extent."); }
    Info info={.format=2,.depth=8,.orientation=1};unsigned cw=0,ch=0,flags=0;int extended=0,pixels=0,lossless=0,alpha_chunk=0,exif_seen=0;
    while(r->position<end) {
        item(r);if(end-r->position<8){ fail("Truncated WebP chunk."); }read_header(r,8);
        uint8_t type[4];memcpy(type,r->h,4);uint32_t size=le32(r->h+4);size_t stop=r->position+size;
        if((uint64_t)size+(size&1)>end-r->position){ fail("Invalid WebP chunk extent."); }
        if(!memcmp(type,"VP8X",4)) {
            if(r->items!=1||size<10){ fail("Invalid VP8X."); }read_header(r,10);flags=r->h[0];cw=le24(r->h+4)+1;ch=le24(r->h+7)+1;
            dimensions(cw,ch);extended=1;info.alpha=!!(flags&16);
            if(probe) { info.width=cw;info.height=ch;return info; }
            if(flags&2){ fail("Animation unsupported."); }
        } else if(!memcmp(type,"ANIM",4)||!memcmp(type,"ANMF",4)){ fail("Animation unsupported."); }
        else if(!memcmp(type,"VP8L",4)) {
            if(pixels||alpha_chunk||size<5){ fail("Invalid VP8L."); }read_header(r,5);uint32_t bits=le32(r->h+1);
            if(r->h[0]!=0x2f||bits>>29){ fail("Invalid VP8L header."); }info.width=(bits&0x3fff)+1;info.height=((bits>>14)&0x3fff)+1;info.alpha|=!!(bits&(1U<<28));pixels=lossless=1;
        } else if(!memcmp(type,"VP8 ",4)) {
            if(pixels||size<10){ fail("Invalid VP8."); }read_header(r,10);
            if((r->h[0]&1)||r->h[3]!=0x9d||r->h[4]!=1||r->h[5]!=0x2a){ fail("Invalid VP8 keyframe."); }info.width=u16(r->h+6,1)&0x3fff;info.height=u16(r->h+8,1)&0x3fff;pixels=1;
        } else if(!memcmp(type,"ALPH",4)) {
            if(!extended||pixels||alpha_chunk||!size||!info.alpha){ fail("Invalid alpha chunk."); }alpha_chunk=1;
        } else {
            metadata(r,size);
            if(!memcmp(type,"EXIF",4)) { if(!extended||exif_seen||!(flags&8)){ fail("Invalid EXIF chunk."); }info.orientation=exif(r,size);exif_seen=1; }
        }
        if(pixels&&probe) { dimensions(info.width,info.height);return info; }
        skip(r,stop-r->position);if(size&1) { read_header(r,1);if(r->h[0]){ fail("Invalid WebP padding."); } }
    }
    if(!pixels||r->position!=r->length||(extended&&(info.width!=cw||info.height!=ch))){ fail("Invalid WebP geometry/end."); }
    dimensions(info.width,info.height);info.color=lossless?2:4;info.alpha_rep=lossless||alpha_chunk?2:1;info.components=lossless||alpha_chunk?4:3;
    return info;
}
static Info describe(Reader *r,int probe)
{
    read_header(r,2);
    if(r->h[0]==137&&r->h[1]==80){ return png_info(r,probe); }
    if(r->h[0]==255&&r->h[1]==216){ return jpeg_info(r,probe); }
    if(r->h[0]=='R'&&r->h[1]=='I'){ return webp_info(r,probe); }
    fail("Unsupported format.");return (Info){0};
}

typedef struct { AVCodecParameters *parameters;Info info; } Descriptor;
typedef struct {
    uint8_t *input,*original;size_t size,last_read,last_position;int cpu,probe,nonseek;
    AVCodecContext *codec;AVPacket *packet;AVFrame *frame;uint64_t completed,observed;
} Work;
static void borrowed_free(void *opaque,uint8_t *data);
static Descriptor operation(Work *w)
{
    uint8_t *acquired=NULL;size_t length=w->size;
    Source source={.data=w->input,.size=w->size,.exact=w->probe};AVIOContext *io=NULL;
    Reader r={.span=w->input,.length=w->size};
    if(w->probe) { io=open_source(&source,!w->nonseek);r.io=io;r.source=&source; }
    else if(w->nonseek) {
        io=open_source(&source,0);uint8_t scratch[8192];size_t used=0,capacity=0;
        for(;;) {
            int n=avio_read(io,scratch,sizeof(scratch));if(n==AVERROR_EOF||n==0){ break; }if(n<0){ fail("Input read failed."); }
            if((size_t)n>INPUT_LIMIT-used){ fail("Input limit."); }size_t needed=used+(size_t)n;
            if(needed>capacity) {
                size_t next=capacity?capacity*2:256;if(next<needed){ next=needed; }if(next>INPUT_LIMIT){ next=INPUT_LIMIT; }
                uint8_t *grown=av_malloc(next+AV_INPUT_BUFFER_PADDING_SIZE);if(!grown){ fail("Input growth failed."); }
                if(used){ memcpy(grown,acquired,used); }av_free(acquired);acquired=grown;capacity=next;
            }
            memcpy(acquired+used,scratch,(size_t)n);used+=(size_t)n;
        }
        memset(acquired+used,0,AV_INPUT_BUFFER_PADDING_SIZE);length=used;r.span=acquired;r.length=used;
    }
    Info info=describe(&r,w->probe);
    if(!w->probe) {
        AVPacket packet=*w->packet;
        if(acquired) {
            packet.data=acquired;packet.size=(int)length;
            packet.buf=av_buffer_create(acquired,length+AV_INPUT_BUFFER_PADDING_SIZE,borrowed_free,NULL,AV_BUFFER_FLAG_READONLY);
            if(!packet.buf){ fail("Acquired packet reference failed."); }
        }
        require(avcodec_send_packet(w->codec,&packet),"Submit complete image");
        require(avcodec_receive_frame(w->codec,w->frame),"Validate coded image");
        if(w->frame->width!=(int)info.width||w->frame->height!=(int)info.height){ fail("Native frame geometry differs."); }
        av_frame_unref(w->frame);avcodec_flush_buffers(w->codec);
        if(acquired){ av_buffer_unref(&packet.buf); }
    }
    Descriptor result={.parameters=avcodec_parameters_alloc(),.info=info};if(!result.parameters){ fail("Descriptor allocation failed."); }
    result.parameters->codec_type=AVMEDIA_TYPE_VIDEO;result.parameters->codec_id=info.format==0?AV_CODEC_ID_PNG:info.format==1?AV_CODEC_ID_MJPEG:AV_CODEC_ID_WEBP;
    result.parameters->width=(int)info.width;result.parameters->height=(int)info.height;result.parameters->bits_per_raw_sample=w->probe?0:(int)info.depth;
    w->last_read=w->probe||w->nonseek?source.read_bytes:w->size;w->last_position=w->probe?source.position:w->size;
    if(io){ close_source(&io); }av_free(acquired);
    w->completed++;w->observed+=info.width+info.height+(unsigned)info.orientation;return result;
}
static void release(Descriptor *d) { avcodec_parameters_free(&d->parameters); }
static void serialize(const Descriptor *d,int probe,uint8_t bytes[36])
{
    const Info *v=&d->info;uint32_t fields[]={v->format,(uint32_t)d->parameters->width,(uint32_t)d->parameters->height,
        probe?0:v->depth,probe?UINT32_MAX:(uint32_t)v->alpha,probe?0:(uint32_t)v->orientation,probe?0:v->color,probe?0:v->alpha_rep,probe?0:v->components};
    for(unsigned i=0;i<9;i++)for(unsigned j=0;j<4;j++)bytes[i*4+j]=(uint8_t)(fields[i]>>(j*8));
}
static void check(Work *w,const char *path)
{
    Descriptor d=operation(w);uint8_t bytes[36];serialize(&d,w->probe,bytes);release(&d);
    if(memcmp(w->input,w->original,w->size)){ fail("Input changed."); }FILE *file=fopen(path,"wb");if(!file||fwrite(bytes,1,36,file)!=36||fclose(file)){ fail("Descriptor write failed."); }
}
static void borrowed_free(void *opaque,uint8_t *data) { (void)opaque;(void)data; }
int main(int argc,char **argv)
{
    if(argc!=4){ fail("Use mode input output.info; modes probe-seekable/probe-nonseekable/validate-span/validate-nonseekable."); }
    Work w={0};w.cpu=cpu();w.probe=!strncmp(argv[1],"probe-",6);w.nonseek=strstr(argv[1],"nonseekable")!=NULL;
    if(strcmp(argv[1],"probe-seekable")&&strcmp(argv[1],"probe-nonseekable")&&strcmp(argv[1],"validate-span")&&strcmp(argv[1],"validate-nonseekable")){ fail("Unknown mode."); }
    FILE *file=fopen(argv[2],"rb");if(!file||fseek(file,0,SEEK_END)){ fail("Open input failed."); }long size=ftell(file);
    if(size<2||size>INPUT_LIMIT||fseek(file,0,SEEK_SET)){ fail("Input limit."); }w.size=(size_t)size;
    w.input=av_mallocz(w.size+AV_INPUT_BUFFER_PADDING_SIZE);if(!w.input||fread(w.input,1,w.size,file)!=w.size||fclose(file)){ fail("Read input failed."); }
    w.original=av_memdup(w.input,w.size);if(!w.original){ fail("Input snapshot failed."); }
    if(!w.probe) {
        enum AVCodecID id=w.input[0]==137?AV_CODEC_ID_PNG:w.input[0]==255?AV_CODEC_ID_MJPEG:AV_CODEC_ID_WEBP;
        const AVCodec *codec=avcodec_find_decoder(id);w.codec=avcodec_alloc_context3(codec);w.packet=av_packet_alloc();w.frame=av_frame_alloc();
        if(!codec||!w.codec||!w.packet||!w.frame){ fail("Codec allocation failed."); }
        w.codec->thread_count=1;w.codec->thread_type=0;w.codec->max_pixels=32000000;
        w.codec->err_recognition=AV_EF_CRCCHECK|AV_EF_BITSTREAM|AV_EF_BUFFER|AV_EF_EXPLODE;
        require(avcodec_open2(w.codec,codec,NULL),"Open codec");if(w.codec->active_thread_type){ fail("Codec threading differs."); }
        w.packet->data=w.input;w.packet->size=(int)w.size;
        w.packet->buf=av_buffer_create(w.input,w.size+AV_INPUT_BUFFER_PADDING_SIZE,borrowed_free,NULL,AV_BUFFER_FLAG_READONLY);if(!w.packet->buf){ fail("Borrowed packet failed."); }
    }
    Descriptor first=operation(&w),second=operation(&w);uint8_t a[36],b[36];serialize(&first,w.probe,a);serialize(&second,w.probe,b);
    if(first.parameters==second.parameters||memcmp(a,b,36)){ fail("Descriptor ownership differs."); }
    first.parameters->width^=1;release(&first);serialize(&second,w.probe,b);if(memcmp(a,b,36)){ fail("Surviving descriptor changed."); }
    Info expected=second.info;release(&second);check(&w,argv[3]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"%s\",\"backend\":\"%s\",\"format\":%u,\"width\":%u,\"height\":%u,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"inputBytes\":%zu,\"readBytes\":%zu,\"traversedBytes\":%zu,\"outputBytes\":36,\"initialCompletedOperations\":%" PRIu64 ",\"metadataOnly\":true,\"independentLiveOutputs\":true,\"sourceUnchanged\":true,\"pixelDecode\":%s,\"rgbaConversion\":false,\"callerPixelCopy\":false,\"freshStream\":%s,\"nonseekable\":%s,\"residentCallbacksImmediate\":true,\"fuelAccounting\":false,\"ffmpegVersion\":\"%s\",\"cpuFlags\":%d}\n",
        argv[1],w.probe?"FFmpeg AVIO and owned descriptor APIs with original bounded prefix-header adapter":"FFmpeg libavcodec native-frame coding validation with original source-metadata adapter",
        expected.format,expected.width,expected.height,w.cpu,w.size,w.last_read,w.last_position,w.completed,w.probe?"false":"true",w.probe||w.nonseek?"true":"false",w.nonseek?"true":"false",av_version_info(),av_get_cpu_flags());fflush(stdout);
    char line[128];
    while(fgets(line,sizeof(line),stdin)) {
        if(!strcmp(line,"quit\n")){ break; }
        if(!strcmp(line,"check\n")) { check(&w,argv[3]);puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}");fflush(stdout);continue; }
        uint64_t count;char extra;if(sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1||!count||count>10000000){ fail("Invalid sample count."); }
        uint64_t old=w.completed,cstart=clock_ns(CLOCK_PROCESS_CPUTIME_ID),start=clock_ns(CLOCK_MONOTONIC);
        for(uint64_t i=0;i<count;i++) { Descriptor d=operation(&w);release(&d); }
        uint64_t elapsed=clock_ns(CLOCK_MONOTONIC)-start,celapsed=clock_ns(CLOCK_PROCESS_CPUTIME_ID)-cstart;
        if(cpu()!=w.cpu||sched_getcpu()!=w.cpu||w.completed-old!=count){ fail("CPU/count changed."); }
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"cpuElapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"outputBytes\":36,\"cpu\":%d}\n",count,elapsed,celapsed,w.completed,w.observed,w.cpu);fflush(stdout);
    }
    if(ferror(stdin)){ fail("Protocol read failed."); }av_frame_free(&w.frame);av_packet_free(&w.packet);avcodec_free_context(&w.codec);av_free(w.input);av_free(w.original);return 0;
}
