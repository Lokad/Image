/* Original development-only observer of the pinned native histogram entries.
 * Internal ABI consists of two ints; native transform layout has32-byte rows.
 * No upstream implementation/table/corpus is incorporated. Output:10 original
 * population probes then luma/UV summaries per padded original macroblock. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

typedef struct { int peak; int last; } HistogramShape;
extern void VP8EncDspInit(void);
extern void VP8SetHistogramData(const int *, HistogramShape *);
extern void (*VP8CollectHistogram)(const uint8_t *, const uint8_t *, int, int, HistogramShape *);
extern int (*VP8GetCPUInfo)(int);

static void fail(const char *s) { fprintf(stderr, "%s\n", s); exit(1); }
static int dimension(const char *s) {
    char *end; long n = strtol(s, &end, 10);
    if (!*s || *end || n < 1 || n > 16383) fail("Invalid dimension.");
    return (int)n;
}
static void shape(FILE *file, const HistogramShape *h) {
    const int values[2] = { h->peak, h->last }; uint8_t bytes[8];
    for (int p = 0; p < 2; p++) for (int b = 0; b < 4; b++) bytes[4*p+b] = (uint8_t)((uint32_t)values[p] >> (8*b));
    if (fwrite(bytes, 1, sizeof(bytes), file) != sizeof(bytes)) fail("Write shape failed.");
}
int main(int argc, char **argv) {
    if (argc != 4 && argc != 5) fail("Usage: vp8-histogram-observer WIDTH HEIGHT OUTPUT [scalar] < ORIGINAL-LOGICAL-YUV");
    const int scalar = argc == 5; if (scalar && strcmp(argv[4], "scalar")) fail("Unknown CPU selection.");
    const int width = dimension(argv[1]), height = dimension(argv[2]), uw = (width+1)/2, uh = (height+1)/2;
    const size_t length = (size_t)width*height+2U*(size_t)uw*uh;
    if (length > 16U*1024U*1024U) fail("Observer input exceeds16MiB.");
    uint8_t *source = malloc(length); if (!source) fail("Allocation failed.");
    if (fread(source, 1, length, stdin) != length || fgetc(stdin) != EOF) fail("Input size differs.");
    if (scalar) VP8GetCPUInfo = NULL;
    VP8EncDspInit(); if (!VP8CollectHistogram) fail("Histogram entry missing.");
    FILE *file = fopen(argv[3], "wb"); if (!file) fail("Open output failed.");
    for (int probe = 0; probe < 10; probe++) {
        int counts[32] = { 0 }; HistogramShape h;
        switch (probe) {
            case 1: counts[0] = 256; break;
            case 2: for (int p = 0; p < 32; p++) counts[p] = 1; break;
            case 3: counts[31] = 2; break;
            case 4: counts[0] = 2; counts[31] = 1; break;
            case 5: counts[0] = counts[31] = 9; break;
            case 6: counts[2] = 1; break;
            case 7: counts[7] = counts[29] = 3; break;
            case 8: counts[1] = 200; counts[31] = 1; break;
            case 9: counts[0] = 1; break;
        }
        VP8SetHistogramData(counts, &h); shape(file, &h);
    }
    uint8_t block[512], prediction[512]; memset(prediction, 128, sizeof(prediction));
    unsigned macroblocks = 0;
    for (int my = 0; my < (height+15)/16; my++) for (int mx = 0; mx < (width+15)/16; mx++) {
        memset(block, 128, sizeof(block));
        for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) {
            const int sx = mx*16+x < width ? mx*16+x : width-1, sy = my*16+y < height ? my*16+y : height-1;
            block[y*32+x] = source[(size_t)sy*width+sx];
        }
        for (int plane = 0; plane < 2; plane++) for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) {
            const int sx = mx*8+x < uw ? mx*8+x : uw-1, sy = my*8+y < uh ? my*8+y : uh-1;
            block[y*32+16+plane*8+x] = source[(size_t)width*height+(size_t)plane*uw*uh+(size_t)sy*uw+sx];
        }
        HistogramShape h;
        VP8CollectHistogram(block, prediction, 0, 16, &h); shape(file, &h);
        VP8CollectHistogram(block+16, prediction+16, 16, 24, &h); shape(file, &h);
        macroblocks++;
    }
    if (fclose(file)) fail("Close output failed.");
    free(source);
    printf("{\"scalar\":%s,\"populationProbes\":10,\"macroblocks\":%u,\"summaryValues\":2,\"prediction\":128}\n", scalar ? "true" : "false", macroblocks);
    return 0;
}
