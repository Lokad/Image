/* Original development-only public-API observer. No encoder implementation or
 * upstream tables are included. This observes the pinned FFmpeg backend's
 * ordinary, undithered ARGB-to-YUVA stage, before transparent-area cleanup. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <webp/encode.h>

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
static int number(const char *text) {
    char *end; long n = strtol(text, &end, 10);
    if (!*text || *end || n < 1 || n > 16383) fail("Invalid bounded integer.");
    return (int)n;
}
static void plane(FILE *output, const uint8_t *data, int stride, int width, int height) {
    for (int y = 0; y < height; y++)
        if (fwrite(data + (size_t)y * stride, 1, width, output) != (size_t)width) fail("Write plane failed.");
}
int main(int argc, char **argv) {
    if (argc != 5 && argc != 6) fail("Usage: vp8-input-observer WIDTH HEIGHT 8|16 OUTPUT [cleanup] < RGBA");
    const int cleanup = argc == 6;
    if (cleanup && strcmp(argv[5], "cleanup")) fail("Unknown stage.");
    const int width = number(argv[1]), height = number(argv[2]), depth = number(argv[3]);
    if (depth != 8 && depth != 16) fail("Invalid sample depth.");
    const size_t samples = (size_t)width * height * 4, bytes = samples * (depth / 8);
    if (bytes > 16U * 1024U * 1024U) fail("Observer input exceeds 16 MiB.");
    uint8_t *source = malloc(bytes), *rgba = malloc(samples);
    if (!source || !rgba) fail("Allocation failed.");
    if (fread(source, 1, bytes, stdin) != bytes || fgetc(stdin) != EOF) fail("Input size differs.");
    for (size_t i = 0; i < samples; i++)
        rgba[i] = depth == 8 ? source[i] : (uint8_t)(((unsigned)source[2*i] + ((unsigned)source[2*i+1] << 8) + 128) / 257);
    WebPPicture picture;
    if (!WebPPictureInit(&picture)) fail("Picture init failed.");
    picture.use_argb = 1; picture.width = width; picture.height = height;
    if (!WebPPictureImportRGBA(&picture, rgba, width * 4)) fail("RGBA import failed.");
    if (!WebPPictureARGBToYUVA(&picture, WEBP_YUV420)) fail("YUVA conversion failed.");
    if (cleanup) WebPCleanupTransparentArea(&picture);
    FILE *output = fopen(argv[4], "wb"); if (!output) fail("Open output failed.");
    const int has_alpha = (picture.colorspace & WEBP_CSP_ALPHA_BIT) != 0;
    plane(output, picture.y, picture.y_stride, width, height);
    plane(output, picture.u, picture.uv_stride, (width+1)/2, (height+1)/2);
    plane(output, picture.v, picture.uv_stride, (width+1)/2, (height+1)/2);
    if (has_alpha) {
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            if (picture.a[y * picture.a_stride+x] != rgba[((size_t)y*width+x)*4+3]) fail("Alpha differs.");
        plane(output, picture.a, picture.a_stride, width, height);
    }
    if (fclose(output)) fail("Close output failed.");
    printf("{\"width\":%d,\"height\":%d,\"depth\":%d,\"hasAlpha\":%s,\"libwebpVersion\":%d,\"cleanup\":%s}\n",
        width, height, depth, has_alpha ? "true" : "false", WebPGetEncoderVersion(), cleanup ? "true" : "false");
    WebPPictureFree(&picture); free(rgba); free(source); return 0;
}
