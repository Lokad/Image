/* Original development-only probe of the pinned backend's Y2 transform entry.
 * Input is our E66 DCT stream:36 original fragments, then padded Y/U/V blocks.
 * No upstream implementation, tables or corpus is incorporated. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <webp/encode.h>

extern void VP8EncDspInit(void);
extern void (*VP8FTransformWHT)(const int16_t *, int16_t *);
extern int (*VP8GetCPUInfo)(int);
static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
static int dimension(const char *s) {
    char *end; long n = strtol(s, &end, 10);
    if (!*s || *end || n < 1 || n > 16383) fail("Invalid dimension.");
    return (int)n;
}
static void transform(FILE *file, const int16_t *input) {
    int16_t result[16]; uint8_t bytes[32]; VP8FTransformWHT(input, result);
    for (int p = 0; p < 16; p++) { const uint16_t v = (uint16_t)result[p]; bytes[2*p] = (uint8_t)v; bytes[2*p+1] = (uint8_t)(v >> 8); }
    if (fwrite(bytes, 1, sizeof(bytes), file) != sizeof(bytes)) fail("Write Walsh coefficients failed.");
}
int main(int argc, char **argv) {
    if (argc != 4 && argc != 5) fail("Usage: vp8-walsh-observer WIDTH HEIGHT OUTPUT [scalar] < E66-DCT-STREAM");
    const int scalar = argc == 5; if (scalar && strcmp(argv[4], "scalar")) fail("Unknown CPU selection.");
    const int width = dimension(argv[1]), height = dimension(argv[2]);
    const int stride = (width+15)/16*16, padded_height = (height+15)/16*16;
    const size_t blocks = (size_t)stride*padded_height/16*3/2, bytes = (36+blocks)*32;
    if (bytes > 16U*1024U*1024U) fail("Observer input exceeds16MiB.");
    uint8_t *source = malloc(bytes); if (!source) fail("Allocation failed.");
    if (fread(source, 1, bytes, stdin) != bytes || fgetc(stdin) != EOF) fail("Input size differs.");
    if (scalar) VP8GetCPUInfo = NULL;
    VP8EncDspInit(); if (!VP8FTransformWHT) fail("Walsh entry missing.");
    FILE *file = fopen(argv[3], "wb"); if (!file) fail("Open output failed.");
    int16_t input[256];
    for (int probe = 0; probe < 36; probe++) {
        memset(input, 0, sizeof(input));
        for (int p = 0; p < 16; p++) {
            int v = probe == 1 ? 2040 : probe == 2 ? -2040 : probe == 3 ? ((p/4+p%4)%2 ? -2040 : 2040) : 0;
            if (probe >= 4 && p == (probe-4)/2) v = (probe-4)%2 ? -1 : 1;
            input[p*16] = (int16_t)v;
        }
        transform(file, input);
    }
    unsigned macroblocks = 0;
    for (int my = 0; my < padded_height/16; my++) for (int mx = 0; mx < stride/16; mx++) {
        memset(input, 0, sizeof(input));
        for (int dy = 0; dy < 4; dy++) for (int dx = 0; dx < 4; dx++) {
            const size_t block = (size_t)(my*4+dy)*(stride/4)+mx*4+dx, offset = (36+block)*32;
            int value = source[offset] | (int)source[offset+1] << 8; if (value >= 32768) value -= 65536;
            input[(dy*4+dx)*16] = (int16_t)value;
        }
        transform(file, input); macroblocks++;
    }
    if (fclose(file)) fail("Close output failed.");
    free(source);
    printf("{\"libwebpVersion\":%d,\"scalar\":%s,\"signedProbes\":36,\"macroblocks\":%u}\n", WebPGetEncoderVersion(), scalar ? "true" : "false", macroblocks);
    return 0;
}
