/* Original benchmark-only nonseekable PNG decode adapter. PNG structure/CRC:
 * https://www.w3.org/TR/png-3/; EXIF orientation:
 * https://www.cipa.jp/std/documents/e/DC-008-Translation-2023-E.pdf
 * The container scan reuses Lokad.Image's original Identify reference adapter;
 * public FFmpeg decodes pixels. No upstream source or tables are copied. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <sched.h>
#include <stdarg.h>
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

typedef struct { int width, height, depth, alpha, orientation; uint8_t *palette, *transparency; } Descriptor;

// The public PNG decoder can return a partial frame while logging an error.
// Honor its error severity without parsing diagnostic text. One codec thread.
static unsigned decode_errors;
static void decode_log(void *context, int level, const char *format, va_list arguments)
{
    if (level<=AV_LOG_ERROR) decode_errors++;
    av_log_default_callback(context,level,format,arguments);
}

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
static Descriptor validate_container(const uint8_t *input, size_t size, const AVCRC *crc)
{
    static const uint8_t signature[]={137,80,78,71,13,10,26,10};
    if (size>64U*1024*1024 || size<8 || memcmp(input,signature,8)) fail("Invalid PNG signature/input limit.");
    size_t position=8, palette_length=0, metadata=0;
    uint64_t work=0, compressed=0;
    unsigned items=0, width=0, height=0, depth=0, color=0;
    int header=0, palette=0, transparency=0, exif=0, data=0, closed=0, ended=0;
    Descriptor result={0}; result.orientation=1;
    while (position<size) {
        if (++items>100000) fail("Container item limit exceeded.");
        if (size-position<12) fail("Truncated PNG chunk.");
        uint32_t length=be32(input+position);
        if (length>INT32_MAX || length>size-position-12) fail("Invalid PNG chunk length.");
        const uint8_t *type=input+position+4, *payload=type+4;
        for (unsigned i=0; i<4; i++)
            if (!((type[i]>='A' && type[i]<='Z') || (type[i]>='a' && type[i]<='z'))) fail("Invalid PNG chunk name.");
        add_work(&work,(uint64_t)length+12);
        if (~av_crc(crc,UINT32_MAX,type,(size_t)length+4)!=be32(payload+length)) fail("PNG CRC mismatch.");
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
            if (length || !data || !compressed || position!=size) fail("Invalid PNG end/trailing input.");
            ended=1; break;
        } else {
            if (!memcmp(type,"acTL",4) || !memcmp(type,"fcTL",4) || !memcmp(type,"fdAT",4)) fail("Animation is outside the still contract.");
            if (!(type[0]&32)) fail("Unknown critical PNG chunk.");
            if (length>4U*1024*1024-metadata) fail("Metadata limit exceeded.");
            metadata+=length;
            if (!memcmp(type,"eXIf",4)) {
                if (exif || data) fail("Duplicate/misplaced EXIF.");
                result.orientation=orientation(payload,length,&work); exif=1;
            }
        }
    }
    if (!ended) fail("PNG is missing IEND.");
    result.width=(int)width; result.height=(int)height; result.depth=(int)depth;
    result.alpha=color==4 || color==6 || transparency;
    return result;
}

typedef struct { const uint8_t *data; size_t size, position; unsigned reads, eof; } Source;
static int source_read(void *opaque, uint8_t *output, int count)
{
    Source *source=opaque; source->reads++;
    size_t available=source->size-source->position;
    if (!available) { source->eof++; return AVERROR_EOF; }
    if (count>8192) count=8192;
    if ((size_t)count>available) count=(int)available;
    memcpy(output,source->data+source->position,(size_t)count); source->position+=(size_t)count;
    return count;
}
typedef struct {
    AVCodecContext *codec; AVPacket *packet; AVFrame *result; const AVCRC *crc;
    uint8_t *encoded, *original, *expected; size_t encoded_bytes, raw_bytes;
    int width, height, row_bytes, orientation, cpu; uint64_t completed, observed;
    unsigned reads, eof, growths; size_t capacity;
} Work;
static void require(int code, const char *message)
{
    if (code<0) { char error[AV_ERROR_MAX_STRING_SIZE]; av_strerror(code,error,sizeof(error));
        fprintf(stderr,"%s: %s\n",message,error); exit(1); }
}
static void apply(Work *work)
{
    Source source={work->encoded,work->encoded_bytes,0,0,0};
    uint8_t *scratch=av_malloc(8192), *block=av_malloc(8192), *owned=NULL;
    if (!scratch || !block) fail("Read scratch allocation failed.");
    AVIOContext *io=avio_alloc_context(scratch,8192,0,&source,source_read,NULL,NULL);
    if (!io) fail("Fresh AVIO allocation failed.");
    io->seekable=0;
    size_t length=0,capacity=0; unsigned growths=0; int count;
    while ((count=avio_read(io,block,8192))>0) {
        if ((size_t)count>64U*1024*1024-length) fail("Input limit exceeded.");
        size_t needed=length+(size_t)count;
        if (needed>capacity) {
            size_t next=capacity ? capacity*2 : 8192;
            if (next<needed) next=needed;
            if (next>64U*1024*1024) next=64U*1024*1024;
            uint8_t *grown=av_realloc(owned,next+AV_INPUT_BUFFER_PADDING_SIZE);
            if (!grown) fail("Private input growth failed.");
            owned=grown; capacity=next; growths++;
        }
        memcpy(owned+length,block,(size_t)count); length+=(size_t)count;
    }
    if (count!=AVERROR_EOF || !avio_feof(io) || source.position!=source.size || !source.eof
        || io->seekable || io->seek || !length || !growths) fail("Nonseekable EOF boundary differs.");
    memset(owned+length,0,AV_INPUT_BUFFER_PADDING_SIZE);
    work->reads=source.reads; work->eof=source.eof; work->growths=growths; work->capacity=capacity;
    av_free(block); av_freep(&io->buffer); avio_context_free(&io);
    require(av_packet_from_data(work->packet,owned,(int)length),"Own acquired packet without a final copy");
    if (!work->packet->buf || work->packet->data==work->encoded) fail("Encoded packet ownership differs.");
    Descriptor header=validate_container(work->packet->data,(size_t)work->packet->size,work->crc);
    work->packet->flags |= AV_PKT_FLAG_KEY;
    decode_errors=0;
    require(avcodec_send_packet(work->codec,work->packet),"Submit acquired PNG");
    require(avcodec_receive_frame(work->codec,work->result),"Receive owned RGBA");
    if (decode_errors) fail("PNG decoder reported an incomplete or invalid raster.");
    if (header.depth!=8 || work->result->width!=header.width || work->result->height!=header.height
        || work->result->format!=AV_PIX_FMT_RGBA || !work->result->buf[0]
        || work->result->linesize[0]<header.width*4) fail("Decoded PNG geometry/depth/ownership differs.");
    work->width=header.width; work->height=header.height; work->row_bytes=header.width*4;
    work->raw_bytes=(size_t)work->row_bytes*header.height; work->orientation=header.orientation;
    av_free(header.palette); av_free(header.transparency); av_packet_unref(work->packet);
    work->observed+=work->result->data[0][work->row_bytes/2]; work->completed++;
}
static void validate(Work *work, const AVFrame *frame)
{
    for (int y=0; y<work->height; y++)
        if (memcmp(frame->data[0]+(size_t)y*frame->linesize[0],work->expected+(size_t)y*work->row_bytes,(size_t)work->row_bytes))
            fail("Exact RGBA samples differ.");
}
static void write_result(Work *work, const char *path)
{
    FILE *output=fopen(path,"wb"); if (!output) fail("Open raw output failed.");
    for (int y=0; y<work->height; y++)
        if (fwrite(work->result->data[0]+(size_t)y*work->result->linesize[0],1,(size_t)work->row_bytes,output)!=(size_t)work->row_bytes)
            fail("Write raw output failed.");
    if (fclose(output)) fail("Close raw output failed.");
}
static void check_write(Work *work, const char *path)
{
    apply(work); validate(work,work->result);
    if (memcmp(work->encoded,work->original,work->encoded_bytes)) fail("Encoded input changed.");
    write_result(work,path); av_frame_unref(work->result);
}
int main(int argc, char **argv)
{
    int probe=argc==4 && !strcmp(argv[1],"--probe");
    if (!probe && (argc!=8 || strcmp(argv[1],"png-nonseekable-decode")))
        fail("Use png-nonseekable-decode INPUT 513 259 8 OUTPUT EXPECTED, or --probe INPUT OUTPUT.");
    int scalar=getenv("LOKAD_IMAGE_SCALAR_REFERENCE") && !strcmp(getenv("LOKAD_IMAGE_SCALAR_REFERENCE"),"1");
    if (scalar) av_force_cpu_flags(0);
    av_log_set_level(AV_LOG_ERROR);
    av_log_set_callback(decode_log);
    Work work={0}; work.cpu=cpu(); work.encoded=input_file(argv[2],&work.encoded_bytes);
    work.original=av_memdup(work.encoded,work.encoded_bytes); if (!work.original) fail("Source witness allocation failed.");
    work.crc=av_crc_get_table(AV_CRC_32_IEEE_LE);
    work.codec=avcodec_alloc_context3(avcodec_find_decoder(AV_CODEC_ID_PNG));
    if (!work.codec || !work.crc) fail("PNG public APIs unavailable.");
    work.codec->thread_count=1; work.codec->thread_type=0; work.codec->max_pixels=32000000;
    work.codec->err_recognition=AV_EF_EXPLODE;
    require(avcodec_open2(work.codec,avcodec_find_decoder(AV_CODEC_ID_PNG),NULL),"Open PNG decoder");
    if (work.codec->thread_count!=1 || work.codec->thread_type || work.codec->active_thread_type) fail("Decoder threading differs.");
    work.packet=av_packet_alloc(); work.result=av_frame_alloc();
    if (!work.packet || !work.result) fail("Packet/frame wrapper allocation failed.");
    if (probe) {
        apply(&work); write_result(&work,argv[3]);
        printf("{\"accepted\":true,\"width\":%d,\"height\":%d,\"depth\":8,\"orientation\":%d,\"outputBytes\":%zu,\"scalarMode\":%s,\"reads\":%u,\"eofReads\":%u,\"growths\":%u,\"privateCapacity\":%zu,\"seekCallback\":false,\"lengthQueries\":0,\"sourceUnchanged\":%s}\n",
            work.width,work.height,work.orientation,work.raw_bytes,scalar?"true":"false",work.reads,work.eof,work.growths,work.capacity,
            memcmp(work.encoded,work.original,work.encoded_bytes)?"false":"true");
    } else {
        if (strcmp(argv[3],"513") || strcmp(argv[4],"259") || strcmp(argv[5],"8")) fail("Only the fixed original RGBA8 representative is timed.");
        size_t expected_bytes; work.expected=input_file(argv[7],&expected_bytes);
        if (expected_bytes!=513U*259*4) fail("Expected raster length differs.");
        AVFrame *held=av_frame_alloc(); if (!held) fail("Held frame allocation failed.");
        apply(&work);
        if (work.width!=513 || work.height!=259 || work.orientation!=1) fail("Fixed timed descriptor differs.");
        av_frame_move_ref(held,work.result); apply(&work); validate(&work,held); validate(&work,work.result);
        if (held->buf[0]->buffer==work.result->buf[0]->buffer || held->data[0]==work.result->data[0]) fail("Live outputs alias.");
        uint8_t saved=held->data[0][0]; held->data[0][0]^=255; validate(&work,work.result); held->data[0][0]=saved;
        av_frame_free(&held); av_frame_unref(work.result); check_write(&work,argv[6]);
        printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"png-nonseekable-decode\",\"width\":513,\"height\":259,\"depth\":8,\"orientation\":1,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"conversionThreads\":0,\"threadType\":0,\"activeThreadType\":0,\"independentLiveOutputs\":true,\"initialCompletedOperations\":%" PRIu64 ",\"freshSeekableStream\":false,\"freshNonSeekableStream\":true,\"inputCopyTimed\":true,\"inputLengthQueryTimed\":false,\"lengthQueries\":0,\"seekCallback\":false,\"inputView\":false,\"eofChecked\":true,\"privateInputGrowthTimed\":true,\"noFinalInputCopy\":true,\"residentCallbacksImmediate\":true,\"streamAcquisitionAndReleaseTimed\":true,\"fullStructureAndCrcTimed\":true,\"pixelDecode\":true,\"scalarMode\":%s,\"inputBytes\":%zu,\"outputBytes\":%zu,\"reads\":%u,\"eofReads\":%u,\"growths\":%u,\"privateCapacity\":%zu,\"backend\":\"FFmpeg nonseekable AVIO and PNG decoder with original structural/CRC compatibility adapter\",\"ffmpegVersion\":\"%s\"}\n",
            work.cpu,work.completed,scalar?"true":"false",work.encoded_bytes,work.raw_bytes,work.reads,work.eof,work.growths,work.capacity,av_version_info()); fflush(stdout);
        char line[128];
        while (fgets(line,sizeof(line),stdin)) {
            if (!strcmp(line,"quit\n")) break;
            if (!strcmp(line,"check\n")) {
                check_write(&work,argv[6]); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue;
            }
            uint64_t count; char extra;
            if (sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1 || !count || count>10000000) fail("Invalid sample count.");
            uint64_t completed=work.completed, cpu_start=clock_ns(CLOCK_PROCESS_CPUTIME_ID), start=clock_ns(CLOCK_MONOTONIC); int stride=0;
            for (uint64_t i=0; i<count; i++) { apply(&work); stride=work.result->linesize[0]; av_frame_unref(work.result); }
            uint64_t elapsed=clock_ns(CLOCK_MONOTONIC)-start, cpu_elapsed=clock_ns(CLOCK_PROCESS_CPUTIME_ID)-cpu_start;
            if (cpu()!=work.cpu || sched_getcpu()!=work.cpu || work.completed-completed!=count) fail("Sample CPU/count differs.");
            printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"cpuElapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"cpu\":%d,\"outputBytes\":%zu}\n",
                count,elapsed,cpu_elapsed,work.completed,work.observed,stride,work.cpu,work.raw_bytes); fflush(stdout);
        }
        if (ferror(stdin)) fail("Protocol read failed.");
    }
    av_frame_free(&work.result); av_packet_free(&work.packet); avcodec_free_context(&work.codec);
    av_free(work.encoded); av_free(work.original); av_free(work.expected); return 0;
}
