/* Original development-only public-API candidate. T.81 baseline grayscale;
 * Lokad.Image's mathematical quantizers and SPEC alpha policy. Unscored. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <math.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <jpeglib.h>
#define VERSION_LITERAL_INNER(x) #x
#define VERSION_LITERAL(x) VERSION_LITERAL_INNER(x)

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
static int number(const char *text) {
    char *end; errno=0; long value=strtol(text,&end,10);
    if(errno || !*text || *end || value<1 || value>32768) fail("Invalid bounded integer.");
    return (int)value;
}
static int affinity_cpu(void) {
    cpu_set_t mask;
    if(sched_getaffinity(0,sizeof(mask),&mask) || CPU_COUNT(&mask)!=1) fail("Exactly one CPU required.");
    for(int cpu=0;cpu<CPU_SETSIZE;cpu++) if(CPU_ISSET(cpu,&mask)) return cpu;
    fail("No affinity CPU."); return -1;
}
static uint64_t now_ns(void) {
    struct timespec value; if(clock_gettime(CLOCK_MONOTONIC,&value)) fail("Clock failed.");
    return (uint64_t)value.tv_sec*UINT64_C(1000000000)+(uint64_t)value.tv_nsec;
}
static uint8_t *read_file(const char *path,size_t size) {
    FILE *file=fopen(path,"rb"); if(!file) fail("Open input failed.");
    uint8_t *bytes=malloc(size); if(!bytes) fail("Allocate input failed.");
    if(fread(bytes,1,size,file)!=size || fgetc(file)!=EOF || ferror(file) || fclose(file)) fail("Input length/read differs.");
    return bytes;
}
typedef struct {
    struct jpeg_compress_struct codec;
    struct jpeg_error_mgr errors;
    uint8_t *rgba,*original,*gray,*reference,*expected,*packet;
    unsigned long packet_bytes,expected_bytes;
    size_t pixels;
    int width,height,quality,cpu;
    uint64_t completed,observed;
} Work;
static void release_packet(Work *work) {
    free(work->packet); work->packet=NULL; work->packet_bytes=0;
}
static void apply(Work *work) {
    if(work->packet) fail("Previous output must be released or independently held.");
    for(size_t i=0;i<work->pixels;i++) {
        unsigned int a=work->rgba[4*i+3];
        unsigned int r=(work->rgba[4*i]*a+32*(255-a)+127)/255;
        unsigned int g=(work->rgba[4*i+1]*a+64*(255-a)+127)/255;
        unsigned int b=(work->rgba[4*i+2]*a+96*(255-a)+127)/255;
        work->gray[i]=(uint8_t)floor(((0.299*r+0.587*g)+0.114*b)+0.5);
    }
    /* Context/gray scratch are warm; conversion, histogram, transform, entropy
       coding and a fresh independently owned packet execute on every call. */
    jpeg_mem_dest(&work->codec,&work->packet,&work->packet_bytes);
    jpeg_start_compress(&work->codec,TRUE);
    while(work->codec.next_scanline<work->codec.image_height) {
        JSAMPROW row=work->gray+(size_t)work->codec.next_scanline*work->width;
        if(jpeg_write_scanlines(&work->codec,&row,1)!=1) fail("Scanline was not consumed.");
    }
    jpeg_finish_compress(&work->codec);
    if(!work->packet || !work->packet_bytes) fail("No encoded output.");
    work->completed++; work->observed^=work->packet_bytes;
}
static void check(Work *work,const uint8_t *packet,unsigned long bytes) {
    if(bytes!=work->expected_bytes || memcmp(packet,work->expected,bytes)) fail("Packet is not deterministic.");
    if(memcmp(work->gray,work->reference,work->pixels)) fail("Flattened grayscale reference differs.");
    if(memcmp(work->rgba,work->original,work->pixels*4)) fail("Source changed.");
}
static void write_packet(Work *work,const char *path) {
    FILE *file=fopen(path,"wb"); if(!file) fail("Open output failed.");
    if(fwrite(work->packet,1,work->packet_bytes,file)!=work->packet_bytes || fclose(file)) fail("Write output failed.");
}
int main(int argc,char **argv) {
    if(argc!=9 || strcmp(argv[1],"jpeg-gray-encode"))
        fail("Usage: libjpeg-gray-warm jpeg-gray-encode INPUT WIDTH HEIGHT 8 OUTPUT QUALITY REFERENCE-GRAY");
    Work work={0}; work.cpu=affinity_cpu(); work.width=number(argv[3]); work.height=number(argv[4]);
    work.quality=number(argv[7]);
    if(strcmp(argv[5],"8") || work.quality>100) fail("Candidate requires byte input and quality1..100.");
    work.pixels=(size_t)work.width*work.height;
    if(work.pixels>512U*1024U*1024U/4) fail("Reference raster exceeds512MiB.");
    work.rgba=read_file(argv[2],work.pixels*4); work.original=malloc(work.pixels*4);
    work.gray=malloc(work.pixels); work.reference=read_file(argv[8],work.pixels);
    if(!work.original || !work.gray) fail("Allocate resident workspace failed.");
    memcpy(work.original,work.rgba,work.pixels*4);
    work.codec.err=jpeg_std_error(&work.errors); jpeg_create_compress(&work.codec);
    work.codec.image_width=work.width; work.codec.image_height=work.height;
    work.codec.input_components=1; work.codec.in_color_space=JCS_GRAYSCALE;
    jpeg_set_defaults(&work.codec);
    unsigned int quantizers[DCTSIZE2]; unsigned int scale=work.quality<50?5000/work.quality:200-2*work.quality;
    for(unsigned int v=0;v<8;v++) for(unsigned int u=0;u<8;u++) {
        unsigned int q=((1+2*(u+v)+u*v)*scale+50)/100;
        quantizers[8*v+u]=q<1?1:q>255?255:q;
    }
    jpeg_add_quant_table(&work.codec,0,quantizers,100,TRUE);
    work.codec.optimize_coding=TRUE; work.codec.dct_method=JDCT_ISLOW;
    apply(&work); work.expected_bytes=work.packet_bytes; work.expected=malloc(work.expected_bytes);
    if(!work.expected) fail("Allocate validation packet failed.");
    memcpy(work.expected,work.packet,work.expected_bytes);
    uint8_t *held=work.packet; unsigned long held_bytes=work.packet_bytes;
    work.packet=NULL; work.packet_bytes=0; apply(&work);
    if(held==work.packet) fail("Live outputs alias.");
    check(&work,held,held_bytes); check(&work,work.packet,work.packet_bytes); free(held); release_packet(&work);
    apply(&work); check(&work,work.packet,work.packet_bytes); write_packet(&work,argv[6]); release_packet(&work);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"jpeg-gray-encode\",\"baselineAccepted\":false,\"encoder\":\"standalone libjpeg-turbo\",\"version\":\"%s\",\"width\":%d,\"height\":%d,\"depth\":8,\"components\":1,\"sampling\":\"gray\",\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"conversionThreads\":1,\"quality\":%d,\"ownedMathematicalQuantizers\":true,\"dctMethod\":\"ISLOW\",\"optimizeHuffman\":true,\"flatteningTimed\":true,\"colorConversionTimed\":true,\"normalizedReferenceExact\":true,\"outputAcquisitionAndReleaseTimed\":true,\"independentLiveOutputs\":true,\"bitstreamDeterministic\":true,\"initialCompletedOperations\":%" PRIu64 ",\"outputBytes\":%lu}\n",VERSION_LITERAL(LIBJPEG_TURBO_VERSION),work.width,work.height,work.cpu,work.quality,work.completed,work.expected_bytes); fflush(stdout);
    char line[128];
    while(fgets(line,sizeof(line),stdin)) {
        if(!strcmp(line,"quit\n")) break;
        if(!strcmp(line,"check\n")) {
            apply(&work); check(&work,work.packet,work.packet_bytes); write_packet(&work,argv[6]); release_packet(&work);
            puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if(sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1 || !count || count>UINT64_C(10000000)) fail("Invalid bounded sample request.");
        uint64_t completed=work.completed,start=now_ns();
        for(uint64_t i=0;i<count;i++) { apply(&work); release_packet(&work); }
        uint64_t elapsed=now_ns()-start;
        if(sched_getcpu()!=work.cpu || affinity_cpu()!=work.cpu || work.completed-completed!=count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"outputBytes\":%lu,\"cpu\":%d}\n",count,elapsed,work.completed,work.observed,work.expected_bytes,work.cpu); fflush(stdout);
    }
    if(ferror(stdin)) fail("Protocol read failed.");
    jpeg_destroy_compress(&work.codec); free(work.rgba); free(work.original); free(work.gray); free(work.reference); free(work.expected);
    return 0;
}
