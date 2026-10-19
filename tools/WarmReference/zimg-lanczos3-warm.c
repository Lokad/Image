/* Original development-only public-API adapter to FFmpeg's zscale backend.
 * Packed RGBA input -> clamped halo -> radius-three premultiplied float output
 * -> unrounded-alpha division -> owned packed RGBA. No production native input. */
#define _GNU_SOURCE
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <zimg.h>

enum { WIDTH=513, HEIGHT=259, HALO=8, PAD_WIDTH=WIDTH+2*HALO, PAD_HEIGHT=HEIGHT+2*HALO };
static void fail(const char *s) { fprintf(stderr,"%s\n",s);exit(1); }
static void check(int code) { if(code) {char s[1024];zimg_get_last_error(s,sizeof(s));fail(s);} }
static void *aligned(size_t n) {void *p=NULL;if(posix_memalign(&p,64,n?n:64))fail("Allocation failed");return p;}
static int cpu(void) {
    cpu_set_t mask;if(sched_getaffinity(0,sizeof(mask),&mask)||CPU_COUNT(&mask)!=1)fail("Exactly one affinity CPU required.");
    for(int i=0;i<CPU_SETSIZE;i++)if(CPU_ISSET(i,&mask))return i;
    fail("Missing affinity CPU.");return -1;
}
static uint64_t now_ns(void) {struct timespec t;if(clock_gettime(CLOCK_MONOTONIC,&t))fail("Clock failed.");return (uint64_t)t.tv_sec*UINT64_C(1000000000)+t.tv_nsec;}
static uint8_t *read_exact(const char *path,size_t bytes) {
    uint8_t *p=malloc(bytes);if(!p)fail("Input allocation failed.");FILE *f=fopen(path,"rb");
    if(!f||fread(p,1,bytes,f)!=bytes||fgetc(f)!=EOF||ferror(f)||fclose(f))fail("Input size/read differs.");
    return p;
}
typedef struct {
    zimg_filter_graph *graph;zimg_image_buffer_const input;uint8_t *raw,*backup,*expected,*storage;void *scratch;
    int depth,width,height,cpu,scalar,input_stride,output_stride;size_t input_bytes,output_bytes,scratch_bytes;
    uint64_t completed,observed;
} Work;
static unsigned get(const uint8_t *p,size_t i,int depth) {return depth==8?p[i]:((const uint16_t *)p)[i];}
static unsigned quantize(double v,int maximum) {return v>=maximum?(unsigned)maximum:v<=0?0:(unsigned)(v+.5);}
// Specialize once per row, as in the existing bicubic adapters. Per-sample
// precision dispatch must not make the reference's normalization needlessly slow.
#define DEINTERLEAVE(type) do { \
    const type *row=(const type *)w->raw+(size_t)sy*WIDTH*4; \
    type *r=(type *)(w->storage+(size_t)y*w->input_stride),*g=r+plane_bytes/sizeof(type),*b=g+plane_bytes/sizeof(type),*a=b+plane_bytes/sizeof(type); \
    for(int x=0;x<WIDTH;x++){r[x+HALO]=row[x*4];g[x+HALO]=row[x*4+1];b[x+HALO]=row[x*4+2];a[x+HALO]=row[x*4+3];} \
    for(int x=0;x<HALO;x++){r[x]=row[0];g[x]=row[1];b[x]=row[2];a[x]=row[3]; \
        r[WIDTH+HALO+x]=row[(WIDTH-1)*4];g[WIDTH+HALO+x]=row[(WIDTH-1)*4+1];b[WIDTH+HALO+x]=row[(WIDTH-1)*4+2];a[WIDTH+HALO+x]=row[(WIDTH-1)*4+3];} \
} while(0)
#define QUANTIZE(type) do { \
    type *row=(type *)packed+(size_t)y*w->width*4; \
    for(int x=0;x<w->width;x++){unsigned alpha=quantize((double)a[x]*maximum,maximum);row[x*4+3]=(type)alpha; \
        row[x*4]=(type)(alpha&&a[x]>0?quantize((double)r[x]/a[x]*maximum,maximum):0); \
        row[x*4+1]=(type)(alpha&&a[x]>0?quantize((double)g[x]/a[x]*maximum,maximum):0); \
        row[x*4+2]=(type)(alpha&&a[x]>0?quantize((double)b[x]/a[x]*maximum,maximum):0);} \
} while(0)
static uint8_t *apply(Work *w) {
    const size_t plane_bytes=(size_t)w->input_stride*PAD_HEIGHT;
    for(int y=0;y<PAD_HEIGHT;y++) {
        int sy=y-HALO;if(sy<0)sy=0;if(sy>=HEIGHT)sy=HEIGHT-1;
        if(w->depth==8)DEINTERLEAVE(uint8_t);else DEINTERLEAVE(uint16_t);
    }
    const size_t plane_output=(size_t)w->output_stride*w->height;
    uint8_t *planes=aligned(plane_output*4),*packed=malloc(w->output_bytes);if(!packed)fail("Output allocation failed.");
    zimg_image_buffer output={0};output.version=ZIMG_API_VERSION;
    for(int c=0;c<4;c++){output.plane[c].data=planes+c*plane_output;output.plane[c].stride=w->output_stride;output.plane[c].mask=ZIMG_BUFFER_MAX;}
    check(zimg_filter_graph_process(w->graph,&w->input,&output,w->scratch,NULL,NULL,NULL,NULL));
    const int maximum=w->depth==8?255:65535;
    for(int y=0;y<w->height;y++) {
        const float *r=(const float *)(planes+(size_t)y*w->output_stride),*g=r+plane_output/4,*b=g+plane_output/4,*a=b+plane_output/4;
        if(w->depth==8)QUANTIZE(uint8_t);else QUANTIZE(uint16_t);
    }
    free(planes);w->observed+=packed[0];w->completed++;return packed;
}
typedef struct {unsigned rgb_max,alpha_max;uint64_t rgb_square,alpha_square;} Difference;
static Difference verify(Work *w,const uint8_t *p) {
    Difference d={0};
    for(size_t i=0;i<(size_t)w->width*w->height*4;i+=4) {
        if(!get(p,i+3,w->depth)&&(get(p,i,w->depth)||get(p,i+1,w->depth)||get(p,i+2,w->depth)))fail("Hidden native RGB differs.");
        for(int c=0;c<4;c++) {
            int error=(int)get(p,i+c,w->depth)-(int)get(w->expected,i+c,w->depth);if(error<0)error=-error;
            if(c==3){if((unsigned)error>d.alpha_max)d.alpha_max=error;d.alpha_square+=(uint64_t)error*error;}
            else {if((unsigned)error>d.rgb_max)d.rgb_max=error;d.rgb_square+=(uint64_t)error*error;}
        }
    }
    if(memcmp(w->raw,w->backup,w->input_bytes))fail("Source changed.");
    return d;
}
static int passes(Work *w,Difference d) {
    return d.rgb_max<=1&&d.alpha_max<=1&&d.rgb_square*16<=(uint64_t)w->width*w->height*3&&d.alpha_square*16<=(uint64_t)w->width*w->height;
}
static Difference check_and_write(Work *w,const char *path) {
    uint8_t *p=apply(w);Difference d=verify(w,p);FILE *f=fopen(path,"wb");
    if(!f||fwrite(p,1,w->output_bytes,f)!=w->output_bytes||fclose(f))fail("Write output failed.");
    free(p);return d;
}
int main(int argc,char **argv) {
    if((argc!=10&&argc!=11)||strcmp(argv[1],"lanczos3-resize")||strcmp(argv[3],"513")||strcmp(argv[4],"259")
        ||(strcmp(argv[5],"8")&&strcmp(argv[5],"16"))
        ||!((!strcmp(argv[8],"256")&&!strcmp(argv[9],"129"))||(!strcmp(argv[8],"1027")&&!strcmp(argv[9],"519")))
        ||(argc==11&&strcmp(argv[10],"--scalar")))fail("Usage: zimg-lanczos3-warm lanczos3-resize INPUT 513 259 DEPTH OUTPUT EXPECTED TARGET_WIDTH TARGET_HEIGHT [--scalar]");
    Work w={0};w.cpu=cpu();w.scalar=argc==11;w.depth=atoi(argv[5]);w.width=atoi(argv[8]);w.height=atoi(argv[9]);
    w.input_stride=(PAD_WIDTH*(w.depth/8)+63)&~63;w.output_stride=(w.width*4+63)&~63;
    w.input_bytes=(size_t)WIDTH*HEIGHT*4*(w.depth/8);w.output_bytes=(size_t)w.width*w.height*4*(w.depth/8);
    w.raw=read_exact(argv[2],w.input_bytes);w.backup=malloc(w.input_bytes);if(!w.backup)fail("Backup allocation failed.");memcpy(w.backup,w.raw,w.input_bytes);
    w.expected=read_exact(argv[7],w.output_bytes);
    zimg_image_format src,dst;zimg_image_format_default(&src,ZIMG_API_VERSION);zimg_image_format_default(&dst,ZIMG_API_VERSION);
    src.width=PAD_WIDTH;src.height=PAD_HEIGHT;src.pixel_type=w.depth==8?ZIMG_PIXEL_BYTE:ZIMG_PIXEL_WORD;src.depth=w.depth;
    src.pixel_range=ZIMG_RANGE_FULL;src.color_family=ZIMG_COLOR_RGB;src.matrix_coefficients=ZIMG_MATRIX_RGB;src.transfer_characteristics=ZIMG_TRANSFER_LINEAR;src.alpha=ZIMG_ALPHA_STRAIGHT;
    src.active_region.left=HALO;src.active_region.top=HALO;src.active_region.width=WIDTH;src.active_region.height=HEIGHT;
    dst.width=w.width;dst.height=w.height;dst.pixel_type=ZIMG_PIXEL_FLOAT;dst.depth=32;dst.pixel_range=ZIMG_RANGE_FULL;
    dst.color_family=ZIMG_COLOR_RGB;dst.matrix_coefficients=ZIMG_MATRIX_RGB;dst.transfer_characteristics=ZIMG_TRANSFER_LINEAR;dst.alpha=ZIMG_ALPHA_PREMULTIPLIED;
    zimg_graph_builder_params params;zimg_graph_builder_params_default(&params,ZIMG_API_VERSION);
    params.resample_filter=ZIMG_RESIZE_LANCZOS;params.filter_param_a=3;params.dither_type=ZIMG_DITHER_NONE;if(w.scalar)params.cpu_type=ZIMG_CPU_NONE;
    w.graph=zimg_filter_graph_build(&src,&dst,&params);if(!w.graph){char s[1024];zimg_get_last_error(s,sizeof(s));fail(s);}
    w.input.version=ZIMG_API_VERSION;const size_t plane_bytes=(size_t)w.input_stride*PAD_HEIGHT;w.storage=aligned(plane_bytes*4);memset(w.storage,0,plane_bytes*4);
    for(int c=0;c<4;c++){w.input.plane[c].data=w.storage+c*plane_bytes;w.input.plane[c].stride=w.input_stride;w.input.plane[c].mask=ZIMG_BUFFER_MAX;}
    check(zimg_filter_graph_get_tmp_size(w.graph,&w.scratch_bytes));w.scratch=aligned(w.scratch_bytes);
    Difference d=check_and_write(&w,argv[6]);
    if(!passes(&w,d)) {
        fprintf(stderr,"{\"qualityPass\":false,\"rgbMaximum\":%u,\"alphaMaximum\":%u,\"rgbMeanSquared\":%.17g,\"alphaMeanSquared\":%.17g}\n",d.rgb_max,d.alpha_max,(double)d.rgb_square/(w.width*w.height*3),(double)d.alpha_square/(w.width*w.height));
        fail("Lanczos3 quality gate failed; no timing permitted.");
    }
    uint8_t *first=apply(&w),*second=apply(&w);if(!passes(&w,verify(&w,first))||!passes(&w,verify(&w,second))||first==second||memcmp(first,second,w.output_bytes))fail("Outputs alias or differ.");
    first[0]^=255;if(!passes(&w,verify(&w,second)))fail("Held output changed.");free(first);free(second);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"lanczos3-resize\",\"width\":513,\"height\":259,\"depth\":%d,\"cpu\":%d,\"affinityCount\":1,\"filterThreads\":1,\"threadType\":0,\"outputWidth\":%d,\"outputHeight\":%d,\"outputBytes\":%zu,\"independentLiveOutputs\":true,\"initialCompletedOperations\":%" PRIu64 ",\"backend\":\"zimg (FFmpeg zscale backend)\",\"scalar\":%s,\"radius\":3,\"haloPixels\":8,\"haloAndPackingTimed\":true,\"outputAllocationAndRepackingTimed\":true,\"premultipliedFloatOutput\":true,\"unpremultiplicationAndQuantizationTimed\":true,\"alphaTolerance1\":true,\"scratchBytes\":%zu}\n",w.depth,w.cpu,w.width,w.height,w.output_bytes,w.completed,w.scalar?"true":"false",w.scratch_bytes);fflush(stdout);
    char line[128];
    while(fgets(line,sizeof(line),stdin)) {
        if(!strcmp(line,"quit\n"))break;
        if(!strcmp(line,"check\n")) {
            d=check_and_write(&w,argv[6]);if(!passes(&w,d))fail("Final quality differs.");
            printf("{\"state\":\"checked\",\"matched\":true,\"sourceUnchanged\":true,\"rgbMaximum\":%u,\"alphaMaximum\":%u,\"rgbMeanSquared\":%.17g,\"alphaMeanSquared\":%.17g}\n",d.rgb_max,d.alpha_max,(double)d.rgb_square/(w.width*w.height*3),(double)d.alpha_square/(w.width*w.height));fflush(stdout);continue;
        }
        uint64_t count;char extra;if(sscanf(line,"sample %" SCNu64 " %c",&count,&extra)!=1||!count||count>10000000)fail("Invalid sample request.");
        uint64_t before=w.completed,start=now_ns();for(uint64_t i=0;i<count;i++)free(apply(&w));uint64_t elapsed=now_ns()-start;
        if(sched_getcpu()!=w.cpu||cpu()!=w.cpu||w.completed-before!=count)fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"outputBytes\":%zu,\"cpu\":%d}\n",count,elapsed,w.completed,w.observed,w.width*4*(w.depth/8),w.output_bytes,w.cpu);fflush(stdout);
    }
    if(ferror(stdin))fail("Protocol read failed.");
    free(w.raw);free(w.backup);free(w.expected);free(w.storage);free(w.scratch);zimg_filter_graph_free(w.graph);return 0;
}
