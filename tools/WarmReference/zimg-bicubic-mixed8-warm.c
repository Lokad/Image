/* Original development-only adapter around the public zimg API (FFmpeg's
 * zscale backend). No native assets or sources enter the production package. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <math.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <zimg.h>

#ifndef LOKAD_IMAGE_SAMPLE_DEPTH
#define LOKAD_IMAGE_SAMPLE_DEPTH 8
#endif
#if LOKAD_IMAGE_SAMPLE_DEPTH == 8
typedef uint8_t Sample;
#define SAMPLE_MAXIMUM 255
#define SAMPLE_PIXEL_TYPE ZIMG_PIXEL_BYTE
#define DEPTH_ARG "8"
#define WORKER_NAME "zimg-bicubic-mixed8-warm"
#elif LOKAD_IMAGE_SAMPLE_DEPTH == 16
typedef uint16_t Sample;
#define SAMPLE_MAXIMUM 65535
#define SAMPLE_PIXEL_TYPE ZIMG_PIXEL_WORD
#define DEPTH_ARG "16"
#define WORKER_NAME "zimg-bicubic-mixed16-warm"
#else
#error Unsupported sample depth
#endif

#ifndef LOKAD_IMAGE_REVERSE_MIXED
#define LOKAD_IMAGE_REVERSE_MIXED 0
#endif
#ifndef LOKAD_IMAGE_WIDE_DOWNSAMPLE
#define LOKAD_IMAGE_WIDE_DOWNSAMPLE 0
#endif
#ifndef LOKAD_IMAGE_HORIZONTAL_ONLY
#define LOKAD_IMAGE_HORIZONTAL_ONLY 0
#endif
#ifndef LOKAD_IMAGE_VERTICAL_ONLY
#define LOKAD_IMAGE_VERTICAL_ONLY 0
#endif
#if LOKAD_IMAGE_VERTICAL_ONLY
#undef WORKER_NAME
#define TARGET_WIDTH 513
#define WIDTH_ARG "513"
#if LOKAD_IMAGE_VERTICAL_ONLY == 2
#define WORKER_NAME "zimg-bicubic-vertical-up" DEPTH_ARG "-warm"
#define TARGET_HEIGHT 519
#define HEIGHT_ARG "519"
#else
#define WORKER_NAME "zimg-bicubic-vertical" DEPTH_ARG "-warm"
#define TARGET_HEIGHT 129
#define HEIGHT_ARG "129"
#endif
#elif LOKAD_IMAGE_HORIZONTAL_ONLY
#undef WORKER_NAME
#if LOKAD_IMAGE_HORIZONTAL_ONLY == 2
#define WORKER_NAME "zimg-bicubic-horizontal-up" DEPTH_ARG "-warm"
#define TARGET_WIDTH 1027
#define WIDTH_ARG "1027"
#else
#if LOKAD_IMAGE_SAMPLE_DEPTH == 16
#define WORKER_NAME "zimg-bicubic-horizontal16-warm"
#else
#define WORKER_NAME "zimg-bicubic-horizontal8-warm"
#endif
#define TARGET_WIDTH 256
#define WIDTH_ARG "256"
#endif
#define TARGET_HEIGHT 259
#define HEIGHT_ARG "259"
#elif LOKAD_IMAGE_WIDE_DOWNSAMPLE
#undef WORKER_NAME
#if LOKAD_IMAGE_WIDE_DOWNSAMPLE == 3
#define WORKER_NAME "zimg-bicubic-down32x16-" DEPTH_ARG "-warm"
#define TARGET_WIDTH 32
#define TARGET_HEIGHT 16
#define WIDTH_ARG "32"
#define HEIGHT_ARG "16"
#define TARGET_HALO 32
#elif LOKAD_IMAGE_WIDE_DOWNSAMPLE == 2
#define WORKER_NAME "zimg-bicubic-down64x32-" DEPTH_ARG "-warm"
#define TARGET_WIDTH 64
#define TARGET_HEIGHT 32
#define WIDTH_ARG "64"
#define HEIGHT_ARG "32"
#define TARGET_HALO 16
#else
#if LOKAD_IMAGE_SAMPLE_DEPTH == 16
#define WORKER_NAME "zimg-bicubic-wide16-warm"
#else
#define WORKER_NAME "zimg-bicubic-wide8-warm"
#endif
#define TARGET_WIDTH 128
#define TARGET_HEIGHT 64
#define WIDTH_ARG "128"
#define HEIGHT_ARG "64"
#define TARGET_HALO 8
#endif
#elif LOKAD_IMAGE_REVERSE_MIXED
#undef WORKER_NAME
#if LOKAD_IMAGE_SAMPLE_DEPTH == 16
#define WORKER_NAME "zimg-bicubic-cross16-warm"
#else
#define WORKER_NAME "zimg-bicubic-cross8-warm"
#endif
#define TARGET_WIDTH 256
#define TARGET_HEIGHT 519
#define WIDTH_ARG "256"
#define HEIGHT_ARG "519"
#else
#define TARGET_WIDTH 1027
#define TARGET_HEIGHT 129
#define WIDTH_ARG "1027"
#define HEIGHT_ARG "129"
#endif

#ifndef TARGET_HALO
#define TARGET_HALO 4
#endif

#if LOKAD_IMAGE_VERTICAL_ONLY == 2 && LOKAD_IMAGE_SAMPLE_DEPTH == 16
#define LOW_ALPHA_RGB_ALLOWED "true"
#define LOW_ALPHA_RGB_REFERENCE_MAXIMUM 16
#define LOW_ALPHA_RGB_ERROR_MAXIMUM 2
#else
#define LOW_ALPHA_RGB_ALLOWED "false"
#define LOW_ALPHA_RGB_REFERENCE_MAXIMUM 0
#define LOW_ALPHA_RGB_ERROR_MAXIMUM 1
#endif

enum { WIDTH=513, HEIGHT=259, OUTPUT_WIDTH=TARGET_WIDTH, OUTPUT_HEIGHT=TARGET_HEIGHT, HALO=TARGET_HALO,
       PADDED_WIDTH=WIDTH+2*HALO, PADDED_HEIGHT=HEIGHT+2*HALO,
       SAMPLE_BYTES=sizeof(Sample),
       INPUT_STRIDE=(PADDED_WIDTH*SAMPLE_BYTES+63)&~63, OUTPUT_STRIDE=(OUTPUT_WIDTH*4+63)&~63,
       INPUT_PIXEL_STRIDE=WIDTH*4*SAMPLE_BYTES, OUTPUT_PIXEL_STRIDE=OUTPUT_WIDTH*4*SAMPLE_BYTES,
       INPUT_BYTES=WIDTH*HEIGHT*4*SAMPLE_BYTES, OUTPUT_BYTES=OUTPUT_WIDTH*OUTPUT_HEIGHT*4*SAMPLE_BYTES };
static void fail(const char *s) { fprintf(stderr,"%s\n",s);exit(1); }
static void check(int code) { if(code) {char s[1024];zimg_get_last_error(s,sizeof(s));fail(s);} }
/* All graph samples are finite. Clamp first, then round nonnegative samples
 * away from zero, avoiding general-purpose libm calls per packed sample. */
static Sample quantize(double value) {
    if(value>=SAMPLE_MAXIMUM)return SAMPLE_MAXIMUM;
    if(value<=0)return 0;
    return (Sample)(value+.5);
}
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
        const Sample *row=(const Sample *)w->raw+(size_t)sy*WIDTH*4;
        Sample *r=(Sample *)(w->input_storage+(size_t)y*INPUT_STRIDE);
        Sample *g=r+plane_bytes/SAMPLE_BYTES,*b=g+plane_bytes/SAMPLE_BYTES,*a=b+plane_bytes/SAMPLE_BYTES;
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
        const float *r=(const float *)(planes+(size_t)y*OUTPUT_STRIDE);
        const float *g=r+output_plane_bytes/4,*b=g+output_plane_bytes/4,*a=b+output_plane_bytes/4;
        Sample *row=(Sample *)packed+(size_t)y*OUTPUT_WIDTH*4;
        for(int x=0;x<OUTPUT_WIDTH;x++) {
            int alpha=quantize((double)a[x]*SAMPLE_MAXIMUM);
            row[x*4+3]=(Sample)alpha;
            row[x*4]=alpha&&a[x]>0?quantize((double)r[x]/a[x]*SAMPLE_MAXIMUM):0;
            row[x*4+1]=alpha&&a[x]>0?quantize((double)g[x]/a[x]*SAMPLE_MAXIMUM):0;
            row[x*4+2]=alpha&&a[x]>0?quantize((double)b[x]/a[x]*SAMPLE_MAXIMUM):0;
        }
    }
    free(planes);w->observed+=packed[0];w->completed++;return packed;
}
typedef struct {int rgb_max,alpha_max;uint64_t rgb_square,alpha_square;} Difference;
static Difference verify(Work *w,const uint8_t *p) {
    Difference d={0};
    const Sample *samples=(const Sample *)p,*expected=(const Sample *)w->expected;
    for(int i=0;i<OUTPUT_BYTES/SAMPLE_BYTES;i+=4) {
        if(!samples[i+3]&&(samples[i]||samples[i+1]||samples[i+2]))fail("Nonzero RGB under zero alpha.");

        for(int c=0;c<4;c++) {
            int error=(int)samples[i+c]-expected[i+c];if(error<0)error=-error;
            if(c==3) {if(error>d.alpha_max)d.alpha_max=error;d.alpha_square+=(uint64_t)error*error;}
            else {
                int allowed=expected[i+3]<=LOW_ALPHA_RGB_REFERENCE_MAXIMUM?LOW_ALPHA_RGB_ERROR_MAXIMUM:1;
                if(error>allowed)fail("Bicubic RGB comparison bound exceeded.");
                if(error>d.rgb_max)d.rgb_max=error;
                d.rgb_square+=(uint64_t)error*error;
            }
        }
    }
    if(d.rgb_max>LOW_ALPHA_RGB_ERROR_MAXIMUM||d.alpha_max>1||d.rgb_square*16>(uint64_t)OUTPUT_WIDTH*OUTPUT_HEIGHT*3
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
    if((argc!=10&&argc!=11)||strcmp(argv[1],"bicubic-resize")||strcmp(argv[3],"513")
        ||strcmp(argv[4],"259")||strcmp(argv[5],DEPTH_ARG)||strcmp(argv[8],WIDTH_ARG)||strcmp(argv[9],HEIGHT_ARG)||(argc==11&&strcmp(argv[10],"--scalar")))
        fail("Usage: " WORKER_NAME " bicubic-resize INPUT_RAW 513 259 " DEPTH_ARG " OUTPUT_RAW EXPECTED_RAW " WIDTH_ARG " " HEIGHT_ARG " [--scalar]");
    Work w={0};w.cpu=affinity_cpu();w.scalar=argc==11;
    w.raw=read_exact(argv[2],INPUT_BYTES);w.backup=malloc(INPUT_BYTES);if(!w.backup)fail("Backup allocation failed.");
    memcpy(w.backup,w.raw,INPUT_BYTES);w.expected=read_exact(argv[7],OUTPUT_BYTES);
    zimg_image_format src,dst;zimg_image_format_default(&src,ZIMG_API_VERSION);zimg_image_format_default(&dst,ZIMG_API_VERSION);
    src.width=PADDED_WIDTH;src.height=PADDED_HEIGHT;src.pixel_type=SAMPLE_PIXEL_TYPE;src.depth=LOKAD_IMAGE_SAMPLE_DEPTH;
    src.pixel_range=ZIMG_RANGE_FULL;src.color_family=ZIMG_COLOR_RGB;src.matrix_coefficients=ZIMG_MATRIX_RGB;
    src.transfer_characteristics=ZIMG_TRANSFER_LINEAR;src.alpha=ZIMG_ALPHA_STRAIGHT;
    src.active_region.left=HALO;src.active_region.top=HALO;src.active_region.width=WIDTH;src.active_region.height=HEIGHT;
    dst.width=OUTPUT_WIDTH;dst.height=OUTPUT_HEIGHT;dst.pixel_type=ZIMG_PIXEL_FLOAT;dst.depth=32;
    dst.pixel_range=ZIMG_RANGE_FULL;dst.color_family=ZIMG_COLOR_RGB;dst.matrix_coefficients=ZIMG_MATRIX_RGB;
    dst.transfer_characteristics=ZIMG_TRANSFER_LINEAR;dst.alpha=ZIMG_ALPHA_PREMULTIPLIED;
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
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"bicubic-resize\",\"width\":513,\"height\":259,\"depth\":%d,\"cpu\":%d,\"affinityCount\":1,\"filterThreads\":1,\"threadType\":0,\"outputWidth\":%d,\"outputHeight\":%d,\"independentLiveOutputs\":true,\"initialCompletedOperations\":%" PRIu64 ",\"inputStride\":%d,\"backend\":\"zimg (FFmpeg zscale backend)\",\"scalar\":%s,\"haloPixels\":%d,\"haloAndPackingTimed\":true,\"outputAllocationAndRepackingTimed\":true,\"alphaTolerance1\":true,\"premultipliedFloatOutput\":true,\"unpremultiplicationAndQuantizationTimed\":true,\"lowAlphaRgbAllowance\":%s,\"lowAlphaRgbReferenceMaximum\":%d,\"lowAlphaRgbErrorMaximum\":%d,\"scratchBytes\":%zu,\"nativeOutputPixelAllocationBytes\":%d}\n",LOKAD_IMAGE_SAMPLE_DEPTH,w.cpu,OUTPUT_WIDTH,OUTPUT_HEIGHT,w.completed,INPUT_PIXEL_STRIDE,w.scalar?"true":"false",HALO,LOW_ALPHA_RGB_ALLOWED,LOW_ALPHA_RGB_REFERENCE_MAXIMUM,LOW_ALPHA_RGB_ERROR_MAXIMUM,w.scratch_bytes,OUTPUT_STRIDE*OUTPUT_HEIGHT*4+OUTPUT_BYTES);fflush(stdout);
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
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":%d,\"cpu\":%d}\n",count,elapsed,w.completed,w.observed,OUTPUT_PIXEL_STRIDE,w.cpu);fflush(stdout);
    }
    if(ferror(stdin))fail("Protocol read failed.");
    free(w.scratch);free(w.input_storage);free(w.expected);free(w.backup);free(w.raw);zimg_filter_graph_free(w.graph);return 0;
}
