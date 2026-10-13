/* Original development-only adapter around the public zimg API (FFmpeg's
 * zscale backend). No native assets or sources enter the production package. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <zimg.h>

enum { WIDTH=513, HEIGHT=259, OUTPUT_WIDTH=256, OUTPUT_HEIGHT=129, HALO=4,
       PADDED_WIDTH=WIDTH+2*HALO, PADDED_HEIGHT=HEIGHT+2*HALO,
       INPUT_STRIDE=(PADDED_WIDTH+63)&~63, OUTPUT_STRIDE=(OUTPUT_WIDTH+63)&~63,
       INPUT_BYTES=WIDTH*HEIGHT*4, OUTPUT_BYTES=OUTPUT_WIDTH*OUTPUT_HEIGHT*4 };
static void fail(const char *s) { fprintf(stderr,"%s\n",s);exit(1); }
static void check(int code) { if(code) {char s[1024];zimg_get_last_error(s,sizeof(s));fail(s);} }
static void *aligned(size_t n) {void *p=NULL;if(posix_memalign(&p,64,n?n:64))fail("Allocation failed");return p;}
static int affinity_cpu(void) {
    cpu_set_t mask;
    if(sched_getaffinity(0,sizeof(mask),&mask)||CPU_COUNT(&mask)!=1)fail("Exactly one affinity CPU required.");
    for(int cpu=0;cpu<CPU_SETSIZE;cpu++)if(CPU_ISSET(cpu,&mask))return cpu;
    fail("Missing affinity CPU.");return -1;
}
static uint64_t now_ns(void) {
    struct timespec t;if(clock_gettime(CLOCK_MONOTONIC,&t))fail("Monotonic clock failed.");
    return (uint64_t)t.tv_sec*UINT64_C(1000000000)+(uint64_t)t.tv_nsec;
}
static uint8_t *read_exact(const char *path,size_t bytes) {
    uint8_t *p=malloc(bytes);if(!p)fail("Input allocation failed.");FILE *f=fopen(path,"rb");
    if(!f||fread(p,1,bytes,f)!=bytes||fgetc(f)!=EOF||ferror(f))fail("Input size differs.");
    if(fclose(f))fail("Input close failed.");
    return p;
}
typedef struct {
    zimg_filter_graph *graph;
    zimg_image_buffer_const input;
    uint8_t *raw,*backup,*expected,*input_storage;
    void *scratch;
    int cpu,scalar;
    size_t scratch_bytes;
    uint64_t completed,observed;
} Work;

/* All packed source samples and halo cells are materialized on every call.
 * Reusable input/graph scratch is workspace, never a cached converted image. */
static uint8_t *apply(Work *w) {
    const size_t plane_bytes=(size_t)INPUT_STRIDE*PADDED_HEIGHT;
    for(int y=0;y<PADDED_HEIGHT;y++) {
        int sy=y-HALO;if(sy<0)sy=0;if(sy>=HEIGHT)sy=HEIGHT-1;
        const uint8_t *row=w->raw+(size_t)sy*WIDTH*4;
        uint8_t *r=w->input_storage+(size_t)y*INPUT_STRIDE;
        uint8_t *g=r+plane_bytes,*b=g+plane_bytes,*a=b+plane_bytes;
        for(int x=0;x<WIDTH;x++) {
            r[x+HALO]=row[x*4];g[x+HALO]=row[x*4+1];b[x+HALO]=row[x*4+2];a[x+HALO]=row[x*4+3];
        }
        for(int x=0;x<HALO;x++) {
            r[x]=row[0];g[x]=row[1];b[x]=row[2];a[x]=row[3];
            r[WIDTH+HALO+x]=row[(WIDTH-1)*4];g[WIDTH+HALO+x]=row[(WIDTH-1)*4+1];
            b[WIDTH+HALO+x]=row[(WIDTH-1)*4+2];a[WIDTH+HALO+x]=row[(WIDTH-1)*4+3];
        }
    }
    zimg_image_buffer output={0};output.version=ZIMG_API_VERSION;
    const size_t output_plane_bytes=(size_t)OUTPUT_STRIDE*OUTPUT_HEIGHT;
    uint8_t *planes=aligned(output_plane_bytes*4),*packed=malloc(OUTPUT_BYTES);
    if(!packed)fail("Output allocation failed.");
    for(int c=0;c<4;c++) {
        output.plane[c].data=planes+c*output_plane_bytes;
        output.plane[c].stride=OUTPUT_STRIDE;output.plane[c].mask=ZIMG_BUFFER_MAX;
    }
    check(zimg_filter_graph_process(w->graph,&w->input,&output,w->scratch,NULL,NULL,NULL,NULL));
    for(int y=0;y<OUTPUT_HEIGHT;y++) {
        const uint8_t *r=planes+(size_t)y*OUTPUT_STRIDE;
        const uint8_t *g=r+output_plane_bytes,*b=g+output_plane_bytes,*a=b+output_plane_bytes;
        uint8_t *row=packed+(size_t)y*OUTPUT_WIDTH*4;
        for(int x=0;x<OUTPUT_WIDTH;x++) {row[x*4]=r[x];row[x*4+1]=g[x];row[x*4+2]=b[x];row[x*4+3]=a[x];}
    }
    free(planes);w->observed+=packed[0];w->completed++;return packed;
}
typedef struct {int rgb_max,alpha_max;uint64_t rgb_square,alpha_square;} Difference;
static Difference verify(Work *w,const uint8_t *p) {
    Difference d={0};
    for(int i=0;i<OUTPUT_BYTES;i+=4) {
        if(!p[i+3]&&(p[i]||p[i+1]||p[i+2]))fail("Nonzero RGB under zero alpha.");
        for(int c=0;c<4;c++) {
            int error=(int)p[i+c]-w->expected[i+c];if(error<0)error=-error;
            if(c==3) {if(error>d.alpha_max)d.alpha_max=error;d.alpha_square+=(uint64_t)error*error;}
            else {if(error>d.rgb_max)d.rgb_max=error;d.rgb_square+=(uint64_t)error*error;}
        }
    }
    if(d.rgb_max>1||d.alpha_max>1||d.rgb_square*16>(uint64_t)OUTPUT_WIDTH*OUTPUT_HEIGHT*3
        ||d.alpha_square*16>(uint64_t)OUTPUT_WIDTH*OUTPUT_HEIGHT)fail("Bicubic comparison bounds exceeded.");
    if(memcmp(w->raw,w->backup,INPUT_BYTES))fail("Source changed.");
    return d;
}
static Difference check_and_write(Work *w,const char *path) {
    uint8_t *p=apply(w);Difference d=verify(w,p);FILE *f=fopen(path,"wb");
    if(!f||fwrite(p,1,OUTPUT_BYTES,f)!=OUTPUT_BYTES||fclose(f))fail("Write output failed.");
    free(p);return d;
}
int main(int argc,char **argv) {
    if((argc!=8&&argc!=9)||strcmp(argv[1],"bicubic-resize")||strcmp(argv[3],"513")
        ||strcmp(argv[4],"259")||strcmp(argv[5],"8")||(argc==9&&strcmp(argv[8],"--scalar")))
        fail("Usage: zimg-bicubic-warm bicubic-resize INPUT_RAW 513 259 8 OUTPUT_RAW EXPECTED_RAW [--scalar]");
    Work w={0};w.cpu=affinity_cpu();w.scalar=argc==9;
    w.raw=read_exact(argv[2],INPUT_BYTES);w.backup=malloc(INPUT_BYTES);if(!w.backup)fail("Backup allocation failed.");
    memcpy(w.backup,w.raw,INPUT_BYTES);w.expected=read_exact(argv[7],OUTPUT_BYTES);
    zimg_image_format src,dst;zimg_image_format_default(&src,ZIMG_API_VERSION);zimg_image_format_default(&dst,ZIMG_API_VERSION);
    src.width=PADDED_WIDTH;src.height=PADDED_HEIGHT;src.pixel_type=ZIMG_PIXEL_BYTE;src.depth=8;
    src.pixel_range=ZIMG_RANGE_FULL;src.color_family=ZIMG_COLOR_RGB;src.matrix_coefficients=ZIMG_MATRIX_RGB;
    src.transfer_characteristics=ZIMG_TRANSFER_LINEAR;src.alpha=ZIMG_ALPHA_STRAIGHT;
    src.active_region.left=HALO;src.active_region.top=HALO;src.active_region.width=WIDTH;src.active_region.height=HEIGHT;
    dst.width=OUTPUT_WIDTH;dst.height=OUTPUT_HEIGHT;dst.pixel_type=ZIMG_PIXEL_BYTE;dst.depth=8;
    dst.pixel_range=ZIMG_RANGE_FULL;dst.color_family=ZIMG_COLOR_RGB;dst.matrix_coefficients=ZIMG_MATRIX_RGB;
    dst.transfer_characteristics=ZIMG_TRANSFER_LINEAR;dst.alpha=ZIMG_ALPHA_STRAIGHT;
    zimg_graph_builder_params params;zimg_graph_builder_params_default(&params,ZIMG_API_VERSION);
    params.resample_filter=ZIMG_RESIZE_BICUBIC;params.filter_param_a=0;params.filter_param_b=.5;
    params.dither_type=ZIMG_DITHER_NONE;if(w.scalar)params.cpu_type=ZIMG_CPU_NONE;
    w.graph=zimg_filter_graph_build(&src,&dst,&params);
    if(!w.graph){char s[1024];zimg_get_last_error(s,sizeof(s));fail(s);}
    w.input.version=ZIMG_API_VERSION;const size_t plane_bytes=(size_t)INPUT_STRIDE*PADDED_HEIGHT;
    w.input_storage=aligned(plane_bytes*4);memset(w.input_storage,0,plane_bytes*4);
    for(int c=0;c<4;c++){w.input.plane[c].data=w.input_storage+c*plane_bytes;w.input.plane[c].stride=INPUT_STRIDE;w.input.plane[c].mask=ZIMG_BUFFER_MAX;}
    check(zimg_filter_graph_get_tmp_size(w.graph,&w.scratch_bytes));w.scratch=aligned(w.scratch_bytes);
    uint8_t *first=apply(&w),*second=apply(&w);verify(&w,first);verify(&w,second);
    if(first==second||memcmp(first,second,OUTPUT_BYTES))fail("Outputs alias or differ.");
    first[0]^=255;verify(&w,second);free(first);free(second);check_and_write(&w,argv[6]);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"bicubic-resize\",\"width\":513,\"height\":259,\"depth\":8,\"cpu\":%d,\"affinityCount\":1,\"filterThreads\":1,\"threadType\":0,\"outputWidth\":256,\"outputHeight\":129,\"independentLiveOutputs\":true,\"initialCompletedOperations\":%" PRIu64 ",\"inputStride\":2052,\"backend\":\"zimg (FFmpeg zscale backend)\",\"scalar\":%s,\"haloPixels\":4,\"haloAndPackingTimed\":true,\"outputAllocationAndRepackingTimed\":true,\"alphaTolerance1\":true,\"scratchBytes\":%zu,\"nativeOutputPixelAllocationBytes\":%d}\n",w.cpu,w.completed,w.scalar?"true":"false",w.scratch_bytes,OUTPUT_BYTES*2);fflush(stdout);
    char line[128];
    while(fgets(line,sizeof(line),stdin)) {
        if(!strcmp(line,"quit\n"))break;
        if(!strcmp(line,"check\n")) {
            Difference d=check_and_write(&w,argv[6]);
            printf("{\"state\":\"checked\",\"matched\":true,\"sourceUnchanged\":true,\"rgbMaximum\":%d,\"alphaMaximum\":%d,\"alphaMeanSquared\":%.17g}\n",d.rgb_max,d.alpha_max,(double)d.alpha_square/(OUTPUT_WIDTH*OUTPUT_HEIGHT));fflush(stdout);continue;
        }
        uint64_t count;char extra;
        if(sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1||!count||count>UINT64_C(10000000))fail("Invalid bounded sample request.");
        uint64_t before=w.completed,start=now_ns();for(uint64_t i=0;i<count;i++)free(apply(&w));uint64_t elapsed=now_ns()-start;
        if(sched_getcpu()!=w.cpu||affinity_cpu()!=w.cpu||w.completed-before!=count)fail("Sample CPU/work count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":1024,\"cpu\":%d}\n",count,elapsed,w.completed,w.observed,w.cpu);fflush(stdout);
    }
    if(ferror(stdin))fail("Protocol read failed.");
    free(w.scratch);free(w.input_storage);free(w.expected);free(w.backup);free(w.raw);zimg_filter_graph_free(w.graph);return 0;
}
