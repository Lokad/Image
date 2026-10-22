/* Development-only probe for the frozen FFmpeg forward-transform stage.
 * Own ABI declaration for the pinned, two-function dispatch record. No upstream
 * implementation/header/table is copied. This never ships with Lokad.Image. */
#include <libavcodec/avcodec.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

typedef void (*Transform)(int16_t *);
typedef struct { Transform normal, field; } TransformDispatch;
extern void ff_fdctdsp_init(TransformDispatch *, AVCodecContext *);
extern void ff_fdct_sse2(int16_t *);

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }

int main(int argc, char **argv)
{
    if (argc != 3) fail("Usage: ffmpeg-jpeg-fdct PLANAR-YUV444 OUTPUT");
    enum { width = 513, height = 259, plane = width * height };
    FILE *input = fopen(argv[1], "rb");
    if (!input) fail("Open input failed.");
    uint8_t *pixels = malloc(plane * 3);
    if (!pixels || fread(pixels, 1, plane * 3, input) != plane * 3
        || fgetc(input) != EOF || ferror(input) || fclose(input)) fail("Bounded input read failed.");
    AVCodecContext context = {0}; /* Same AUTO algorithm and 8-bit defaults. */
    TransformDispatch dispatch = {0};
    ff_fdctdsp_init(&dispatch, &context);
    if (dispatch.normal != ff_fdct_sse2) fail("Pinned AUTO dispatch is not the expected SSE2 transform.");
    FILE *output = fopen(argv[2], "wb");
    if (!output) fail("Open output failed.");
    int blocks = 0;
    for (int c = 0; c < 3; c++)
        for (int top = 0; top < height; top += 8)
            for (int left = 0; left < width; left += 8) {
                _Alignas(16) int16_t block[64];
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++) {
                        int sx = left + x < width ? left + x : width - 1;
                        int sy = top + y < height ? top + y : height - 1;
                        block[y * 8 + x] = pixels[c * plane + sy * width + sx];
                    }
                dispatch.normal(block);
                for (int i = 0; i < 64; i++) {
                    uint16_t value = (uint16_t)block[i];
                    if (fputc(value & 255, output) == EOF || fputc(value >> 8, output) == EOF)
                        fail("Output write failed.");
                }
                blocks++;
            }
    if (fclose(output)) fail("Output close failed.");
    free(pixels);
    printf("{\"dispatch\":\"ff_fdct_sse2\",\"blocks\":%d,\"values\":%d,\"padding\":\"replicate-last-sample\"}\n", blocks, blocks * 64);
    return 0;
}
