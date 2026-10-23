/* Original development-only adapter to the pinned backend's transform entry.
 * No upstream implementation, constants table or test corpus is incorporated.
 * The inspected internal ABI has a 32-byte row stride and sixteen int16 outputs.
 * Output is original signed-boundary fragments, then all padded Y/U/V blocks
 * of one supplied original raster against neutral128 prediction, little endian.
 */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <webp/encode.h>

extern void VP8EncDspInit(void);
extern void (*VP8FTransform)(const uint8_t *, const uint8_t *, int16_t *);
extern int (*VP8GetCPUInfo)(int);

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
static int dimension(const char *text) {
    char *end; long value = strtol(text, &end, 10);
    if (!*text || *end || value < 1 || value > 16383) fail("Invalid dimension.");
    return (int)value;
}
static void transform(FILE *output, const uint8_t *source, const uint8_t *prediction) {
    int16_t coefficients[16]; uint8_t bytes[32];
    VP8FTransform(source, prediction, coefficients);
    for (int i = 0; i < 16; i++) {
        const uint16_t value = (uint16_t)coefficients[i];
        bytes[2*i] = (uint8_t)value; bytes[2*i+1] = (uint8_t)(value >> 8);
    }
    if (fwrite(bytes, 1, sizeof(bytes), output) != sizeof(bytes)) fail("Write coefficients failed.");
}
int main(int argc, char **argv) {
    if (argc != 4 && argc != 5) fail("Usage: vp8-transform-observer WIDTH HEIGHT OUTPUT [scalar] < LOGICAL-YUV");
    const int scalar = argc == 5;
    if (scalar && strcmp(argv[4], "scalar")) fail("Unknown CPU selection.");
    const int width = dimension(argv[1]), height = dimension(argv[2]);
    const int uv_width = (width+1)/2, uv_height = (height+1)/2;
    const size_t length = (size_t)width*height + 2U*(size_t)uv_width*uv_height;
    if (length > 16U*1024U*1024U) fail("Observer input exceeds16MiB.");
    uint8_t *raster = malloc(length); if (!raster) fail("Allocate input failed.");
    if (fread(raster, 1, length, stdin) != length || fgetc(stdin) != EOF) fail("Input size differs.");
    if (scalar) VP8GetCPUInfo = NULL;
    VP8EncDspInit(); if (!VP8FTransform) fail("Transform entry missing.");
    FILE *output = fopen(argv[3], "wb"); if (!output) fail("Open output failed.");
    uint8_t source[128], prediction[128];
    for (int probe = 0; probe < 36; probe++) {
        memset(source, 128, sizeof(source)); memset(prediction, 128, sizeof(prediction));
        for (int p = 0; p < 16; p++) {
            int residual = probe == 1 ? 255 : probe == 2 ? -255 : probe == 3 ? ((p/4+p%4)%2 ? -255 : 255) : 0;
            if (probe >= 4 && p == (probe-4)/2) residual = (probe-4)%2 ? -255 : 255;
            if (residual) { const int pos = (p/4)*32+p%4; source[pos] = residual > 0 ? 255 : 0; prediction[pos] = residual > 0 ? 0 : 255; }
        }
        transform(output, source, prediction);
    }
    unsigned blocks = 0; size_t offset = 0;
    for (int plane = 0; plane < 3; plane++) {
        const int w = plane ? uv_width : width, h = plane ? uv_height : height;
        const int padded_w = ((width+15)/16*16) / (plane ? 2 : 1);
        const int padded_h = ((height+15)/16*16) / (plane ? 2 : 1);
        memset(prediction, 128, sizeof(prediction));
        for (int y = 0; y < padded_h; y += 4) for (int x = 0; x < padded_w; x += 4) {
            for (int dy = 0; dy < 4; dy++) for (int dx = 0; dx < 4; dx++) {
                const int sx = x+dx < w ? x+dx : w-1, sy = y+dy < h ? y+dy : h-1;
                source[dy*32+dx] = raster[offset+(size_t)sy*w+sx];
            }
            transform(output, source, prediction); blocks++;
        }
        offset += (size_t)w*h;
    }
    if (fclose(output)) fail("Close output failed.");
    printf("{\"libwebpVersion\":%d,\"scalar\":%s,\"signedProbes\":36,\"rasterBlocks\":%u,\"width\":%d,\"height\":%d,\"prediction\":128}\n",
        WebPGetEncoderVersion(), scalar ? "true" : "false", blocks, width, height);
    free(raster); return 0;
}
