/* Original benchmark-only public FFmpeg PNG/AVIO adapter. PNG fields follow
 * https://www.w3.org/TR/png-3/. No upstream sources or tables are copied. */
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
#include <libavutil/mem.h>
#include <libavutil/opt.h>

static void fail(const char *message) { fprintf(stderr,"%s\n",message); exit(1); }
static void require(int code, const char *message)
{
    if (code<0) { char text[AV_ERROR_MAX_STRING_SIZE]; av_strerror(code,text,sizeof(text));
        fprintf(stderr,"%s: %s\n",message,text); exit(1); }
}
static int cpu(void)
{
    cpu_set_t set; if (sched_getaffinity(0,sizeof(set),&set) || CPU_COUNT(&set)!=1) fail("Use one CPU.");
    for (int i=0; i<CPU_SETSIZE; i++) if (CPU_ISSET(i,&set)) return i;
    fail("No CPU."); return -1;
}
static uint64_t clock_ns(clockid_t clock)
{
    struct timespec t; if (clock_gettime(clock,&t)) fail("Clock failed.");
    return (uint64_t)t.tv_sec*UINT64_C(1000000000)+(uint64_t)t.tv_nsec;
}
static uint32_t big32(const uint8_t *p)
{ return (uint32_t)p[0]<<24 | (uint32_t)p[1]<<16 | (uint32_t)p[2]<<8 | p[3]; }
typedef struct { uint8_t *bytes; size_t length, capacity; unsigned writes, growths; } Output;
static int write_output(void *opaque, const uint8_t *bytes, int count)
{
    Output *out=opaque;
    if (count<0 || (size_t)count>64U*1024*1024-out->length) return AVERROR(ENOMEM);
    size_t needed=out->length+(size_t)count;
    if (needed>out->capacity) {
        size_t next=out->capacity ? out->capacity*2 : 256;
        if (next<needed) next=needed;
        uint8_t *grown=av_realloc(out->bytes,next); if (!grown) return AVERROR(ENOMEM);
        out->bytes=grown; out->capacity=next; out->growths++;
    }
    memcpy(out->bytes+out->length,bytes,(size_t)count); out->length=needed; out->writes++; return count;
}
typedef struct {
    AVCodecContext *codec; AVFrame *source; AVPacket *packet;
    uint8_t *original, *golden; size_t golden_bytes, output_bytes, idat_bytes;
    int cpu; uint64_t completed, observed; unsigned writes, growths; size_t capacity;
} Work;
static Output apply(Work *work)
{
    work->source->pts=(int64_t)work->completed; work->source->pict_type=AV_PICTURE_TYPE_I;
    require(avcodec_send_frame(work->codec,work->source),"Submit PNG source");
    require(avcodec_receive_packet(work->codec,work->packet),"Receive complete owned PNG");
    if (!work->packet->buf || work->packet->size<33 || !(work->packet->flags&AV_PKT_FLAG_KEY)) fail("PNG staging ownership differs.");
    Output out={0}; uint8_t *scratch=av_malloc(8192); if (!scratch) fail("AVIO scratch allocation failed.");
    AVIOContext *io=avio_alloc_context(scratch,8192,1,&out,NULL,write_output,NULL);
    if (!io) fail("Fresh AVIO output allocation failed.");
    io->seekable=0;
    for (int offset=0; offset<work->packet->size;) {
        int count=work->packet->size-offset; if (count>65536) count=65536;
        avio_write(io,work->packet->data+offset,count); offset+=count;
    }
    avio_flush(io); require(io->error,"Deliver complete PNG");
    if (io->seek || io->seekable || !out.writes || !out.growths || out.length!=(size_t)work->packet->size
        || out.bytes==work->packet->data) fail("Nonseekable output ownership/delivery differs.");
    work->writes=out.writes; work->growths=out.growths; work->capacity=out.capacity; work->output_bytes=out.length;
    av_freep(&io->buffer); avio_context_free(&io); av_packet_unref(work->packet);
    work->observed+=out.bytes[out.length/2]; work->completed++;
    return out; // Caller storage survives staging and AVIO release.
}
static size_t profile(const Output *out)
{
    static const uint8_t signature[]={137,80,78,71,13,10,26,10};
    if (out->length<33 || memcmp(out->bytes,signature,8) || big32(out->bytes+8)!=13
        || memcmp(out->bytes+12,"IHDR",4) || big32(out->bytes+16)!=513 || big32(out->bytes+20)!=259
        || out->bytes[24]!=8 || out->bytes[25]!=6 || out->bytes[26] || out->bytes[27] || out->bytes[28]) fail("Fixed PNG profile differs.");
    size_t position=8,idat=0;
    while (position<out->length) {
        if (out->length-position<12) fail("Truncated PNG chunk.");
        uint32_t length=big32(out->bytes+position);
        if (length>out->length-position-12) fail("PNG chunk length differs.");
        if (!memcmp(out->bytes+position+4,"IDAT",4)) idat+=length;
        position+=(size_t)length+12;
    }
    if (!idat || position!=out->length) fail("PNG IDAT/end differs.");
    return idat;
}
static void source_unchanged(Work *work)
{
    for (int y=0; y<259; y++)
        if (memcmp(work->source->data[0]+(size_t)y*work->source->linesize[0],work->original+(size_t)y*513*4,513*4)) fail("Borrowed source pixels changed.");
}
static void check_write(Work *work, const char *path)
{
    Output out=apply(work); source_unchanged(work); work->idat_bytes=profile(&out);
    if (out.length!=work->golden_bytes || memcmp(out.bytes,work->golden,out.length)) fail("Complete output is nondeterministic.");
    FILE *file=fopen(path,"wb");
    if (!file || fwrite(out.bytes,1,out.length,file)!=out.length || fclose(file)) fail("Write check PNG failed.");
    av_free(out.bytes);
}
int main(int argc, char **argv)
{
    if (argc!=7 || strcmp(argv[1],"png-nonseekable-encode") || strcmp(argv[3],"513") || strcmp(argv[4],"259") || strcmp(argv[5],"8"))
        fail("Use png-nonseekable-encode INPUT_RGBA 513 259 8 OUTPUT_PNG.");
    int scalar=getenv("LOKAD_IMAGE_SCALAR_REFERENCE") && !strcmp(getenv("LOKAD_IMAGE_SCALAR_REFERENCE"),"1");
    if (scalar) av_force_cpu_flags(0);
    av_log_set_level(AV_LOG_ERROR); Work work={0}; work.cpu=cpu();
    work.original=av_malloc(513U*259*4); FILE *file=fopen(argv[2],"rb");
    if (!work.original || !file || fread(work.original,1,513U*259*4,file)!=513U*259*4 || fgetc(file)!=EOF || fclose(file)) fail("Original raster size/read differs.");
    const AVCodec *codec=avcodec_find_encoder(AV_CODEC_ID_PNG); work.codec=avcodec_alloc_context3(codec);
    if (!codec || !work.codec) fail("Public PNG encoder unavailable.");
    work.codec->width=513; work.codec->height=259; work.codec->pix_fmt=AV_PIX_FMT_RGBA;
    work.codec->time_base=(AVRational){1,1}; work.codec->gop_size=1; work.codec->compression_level=6;
    work.codec->thread_count=1; work.codec->thread_type=0;
    require(av_opt_set(work.codec->priv_data,"pred","none",0),"Select PNG filter None");
    require(avcodec_open2(work.codec,codec,NULL),"Open PNG encoder");
    if (work.codec->thread_count!=1 || work.codec->thread_type || work.codec->active_thread_type) fail("Codec threading differs.");
    work.source=av_frame_alloc(); work.packet=av_packet_alloc(); if (!work.source || !work.packet) fail("Frame/packet wrapper allocation failed.");
    work.source->width=513; work.source->height=259; work.source->format=AV_PIX_FMT_RGBA;
    require(av_frame_get_buffer(work.source,32),"Prepare resident borrowed pixels");
    for (int y=0; y<259; y++) memcpy(work.source->data[0]+(size_t)y*work.source->linesize[0],work.original+(size_t)y*513*4,513*4);
    Output first=apply(&work),second=apply(&work);
    if (first.bytes==second.bytes || first.length!=second.length || memcmp(first.bytes,second.bytes,first.length)) fail("Live outputs alias/differ.");
    work.golden_bytes=second.length; work.golden=av_memdup(second.bytes,second.length); if (!work.golden) fail("Golden allocation failed.");
    first.bytes[0]^=255; if (memcmp(second.bytes,work.golden,second.length)) fail("Peer output changed.");
    av_free(first.bytes); if (memcmp(second.bytes,work.golden,second.length)) fail("Surviving output changed.");
    av_free(second.bytes); check_write(&work,argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"png-nonseekable-encode\",\"width\":513,\"height\":259,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"conversionThreads\":0,\"threadType\":0,\"activeThreadType\":0,\"compressionLevel\":6,\"filterNone\":true,\"outputBytes\":%zu,\"idatBytes\":%zu,\"initialCompletedOperations\":%" PRIu64 ",\"freshNonSeekableOutput\":true,\"seekCallback\":false,\"lengthQueries\":0,\"outputView\":false,\"outputGrowthTimed\":true,\"outputCopyTimed\":true,\"encodedStagingTimed\":true,\"internalAvioFlushTimed\":true,\"streamAcquisitionAndReleaseTimed\":true,\"residentCallbacksImmediate\":true,\"callerOutputLeftOpen\":true,\"independentLiveOutputs\":true,\"independentOutputFromStaging\":true,\"writes\":%u,\"growths\":%u,\"outputCapacity\":%zu,\"scalarMode\":%s,\"ffmpegVersion\":\"%s\"}\n",
        work.cpu,work.output_bytes,work.idat_bytes,work.completed,work.writes,work.growths,work.capacity,scalar?"true":"false",av_version_info()); fflush(stdout);
    char line[128];
    while (fgets(line,sizeof(line),stdin)) {
        if (!strcmp(line,"quit\n")) break;
        if (!strcmp(line,"check\n")) { check_write(&work,argv[6]); puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue; }
        uint64_t count; char extra;
        if (sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1 || !count || count>10000000) fail("Invalid sample count.");
        uint64_t completed=work.completed,start=clock_ns(CLOCK_MONOTONIC),cpu_start=clock_ns(CLOCK_PROCESS_CPUTIME_ID);
        for (uint64_t i=0; i<count; i++) { Output out=apply(&work); av_free(out.bytes); }
        uint64_t cpu_elapsed=clock_ns(CLOCK_PROCESS_CPUTIME_ID)-cpu_start,elapsed=clock_ns(CLOCK_MONOTONIC)-start;
        if (cpu()!=work.cpu || sched_getcpu()!=work.cpu || work.completed-completed!=count) fail("CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"cpuElapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"cpu\":%d,\"outputBytes\":%zu}\n",
            count,elapsed,cpu_elapsed,work.completed,work.observed,work.cpu,work.output_bytes); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    av_packet_free(&work.packet); av_frame_free(&work.source); avcodec_free_context(&work.codec); av_free(work.original); av_free(work.golden); return 0;
}
