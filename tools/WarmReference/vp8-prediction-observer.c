/* Original development-only probe of pinned native large-block prediction.
 * Border samples come from the supplied original logicalYUV raster, with edge
 * replication. Cache offsets are derived from its four-tile geometry; no upstream
 * implementation, lookup table or test corpus is incorporated. JSON then pixels. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

extern void VP8EncDspInit(void);
extern void (*VP8EncPredLuma16)(uint8_t *, const uint8_t *, const uint8_t *);
extern void (*VP8EncPredChroma8)(uint8_t *, const uint8_t *, const uint8_t *);
extern int (*VP8GetCPUInfo)(int);
static void fail(const char *s) { fprintf(stderr, "%s\n", s); exit(1); }
static int dimension(const char *s) {
    char *end; long n = strtol(s, &end, 10);
    if (!*s || *end || n < 1 || n > 16383) fail("Invalid dimension.");
    return (int)n;
}
static uint8_t sample(const uint8_t *plane, int width, int height, int x, int y) {
    if (x >= width) x = width-1;
    if (y >= height) y = height-1;
    return plane[(size_t)y*width+x];
}
static void border(const uint8_t *plane, int width, int height, int x, int y, int size, uint8_t *left, uint8_t *top) {
    left[-1] = x && y ? sample(plane,width,height,x-1,y-1) : y ? 129 : 127;
    for (int p = 0; p < size; p++) {
        left[p] = x ? sample(plane,width,height,x-1,y+p) : 129;
        top[p] = y ? sample(plane,width,height,x+p,y-1) : 127;
    }
}
static void pixels(const uint8_t *cache, int offset, int width, int height) {
    for (int y = 0; y < height; y++)
        if (fwrite(cache+offset+y*32,1,width,stdout)!=(size_t)width) fail("Write predicted pixels failed.");
}
int main(int argc, char **argv) {
    if (argc != 3 && argc != 4) fail("Usage: vp8-prediction-observer WIDTH HEIGHT [scalar] < ORIGINAL-LOGICAL-YUV");
    const int scalar = argc == 4; if (scalar && strcmp(argv[3],"scalar")) fail("Unknown CPU selection.");
    const int width = dimension(argv[1]), height = dimension(argv[2]), uw = (width+1)/2, uh = (height+1)/2;
    const size_t length = (size_t)width*height+2U*(size_t)uw*uh;
    if (length > 16U*1024U*1024U) fail("Observer input exceeds16MiB.");
    uint8_t *source = malloc(length); if (!source) fail("Allocation failed.");
    if (fread(source,1,length,stdin)!=length || fgetc(stdin)!=EOF) fail("Input size differs.");
    if (scalar) VP8GetCPUInfo = NULL;
    VP8EncDspInit(); if (!VP8EncPredLuma16 || !VP8EncPredChroma8) fail("Prediction entry missing.");
    printf("{\"scalar\":%s,\"width\":%d,\"height\":%d,\"macroblocks\":%d,\"modes\":4,\"bytesPerMacroblock\":1536}\n",scalar ? "true" : "false",width,height,((width+15)/16)*((height+15)/16));
    _Alignas(32) uint8_t cache[1536], yleft[64], cleft[64], ytop[32], ctop[32];
    for (int my = 0; my < (height+15)/16; my++) for (int mx = 0; mx < (width+15)/16; mx++) {
        memset(yleft,129,sizeof(yleft)); memset(cleft,129,sizeof(cleft));
        memset(ytop,127,sizeof(ytop)); memset(ctop,127,sizeof(ctop));
        border(source,width,height,mx*16,my*16,16,yleft+32,ytop);
        border(source+(size_t)width*height,uw,uh,mx*8,my*8,8,cleft+32,ctop);
        border(source+(size_t)width*height+(size_t)uw*uh,uw,uh,mx*8,my*8,8,cleft+48,ctop+8);
        VP8EncPredLuma16(cache,mx ? yleft+32 : NULL,my ? ytop : NULL);
        VP8EncPredChroma8(cache,mx ? cleft+32 : NULL,my ? ctop : NULL);
        for (int mode = 0; mode < 4; mode++) {
            const int column = (mode&1)*16, row = mode/2;
            pixels(cache,row*16*32+column,16,16);
            pixels(cache,32*32+row*8*32+column,16,8);
        }
    }
    free(source); return 0;
}
