/* Original development-only transform probe for the pinned SIMD grayscale
 * backend. No upstream implementation/header/table is incorporated. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

/* These frozen backend symbols use sixteen-bit centered8x8 blocks. */
extern int jsimd_can_fdct_islow(void);
extern void jsimd_fdct_islow(int16_t *block);
extern void jpeg_fdct_islow(int16_t *block);

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }

int main(int argc, char **argv)
{
    enum { width = 513, height = 259, pixels = width * height };
    if (argc != 3) fail("Usage: libjpeg-gray-fdct FROZEN-GRAY OUTPUT");
    FILE *input = fopen(argv[1], "rb"); if (!input) fail("Open gray source failed.");
    uint8_t *gray = malloc(pixels); if (!gray) fail("Allocate bounded gray source failed.");
    if (fread(gray, 1, pixels, input) != pixels || fgetc(input) != EOF || ferror(input) || fclose(input))
        fail("Frozen source length/read differs.");
    if (!jsimd_can_fdct_islow()) fail("Pinned normal SIMD transform is unavailable.");
    FILE *output = fopen(argv[2], "wb"); if (!output) fail("Open transform output failed.");
    unsigned blocks = 0;
    for (int top = 0; top < height; top += 8)
        for (int left = 0; left < width; left += 8) {
            _Alignas(32) int16_t automatic[64], scalar[64];
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++) {
                    int sy = top + y < height ? top + y : height - 1;
                    int sx = left + x < width ? left + x : width - 1;
                    automatic[y * 8 + x] = (int16_t)(gray[sy * width + sx] - 128);
                }
            memcpy(scalar, automatic, sizeof(scalar));
            jsimd_fdct_islow(automatic); jpeg_fdct_islow(scalar);
            if (memcmp(automatic, scalar, sizeof(scalar))) fail("Native scalar/automatic transforms differ.");
            for (int k = 0; k < 64; k++) {
                uint16_t value = (uint16_t)automatic[k];
                if (fputc(value & 255, output) == EOF || fputc(value >> 8, output) == EOF)
                    fail("Transform write failed.");
            }
            blocks++;
        }
    if (fclose(output)) fail("Transform close failed.");
    free(gray);
    printf("{\"blocks\":%u,\"values\":%u,\"nativeSimdAvailable\":true,\"nativeScalarEqualsAutomatic\":true}\n",
           blocks, blocks * 64);
    return 0;
}
