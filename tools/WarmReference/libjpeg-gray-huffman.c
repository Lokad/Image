/* Original bounded table probe for the approved grayscale reference. Only
 * public data structures and one frozen backend entry point are used. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <jpeglib.h>

extern void jpeg_gen_optimal_table(j_compress_ptr codec, JHUFF_TBL *table, long *frequencies);
static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }

int main(int argc, char **argv)
{
    if (argc != 3) fail("Usage: libjpeg-gray-huffman HISTOGRAMS OUTPUT");
    FILE *input = fopen(argv[1], "rb"), *output = fopen(argv[2], "wb");
    if (!input || !output) fail("Open table probe input/output failed.");
    struct jpeg_compress_struct codec;
    struct jpeg_error_mgr errors;
    codec.err = jpeg_std_error(&errors); jpeg_create_compress(&codec);
    JHUFF_TBL *table = jpeg_alloc_huff_table((j_common_ptr)&codec);
    uint8_t bytes[1024]; size_t size; int cases = 0;
    while ((size = fread(bytes, 1, sizeof(bytes), input)) != 0) {
        if (size != sizeof(bytes) || ++cases > 34) fail("Frozen bounded histogram batch required.");
        long frequencies[257] = {0}; int active = 0;
        for (int i = 0; i < 256; i++) {
            uint32_t value = bytes[i * 4] | (uint32_t)bytes[i * 4 + 1] << 8
                | (uint32_t)bytes[i * 4 + 2] << 16 | (uint32_t)bytes[i * 4 + 3] << 24;
            if (value > 1000000) fail("Histogram exceeds the probe bound.");
            frequencies[i] = (long)value; active += value != 0;
        }
        if (!active) fail("Nonempty histogram required.");
        jpeg_gen_optimal_table(&codec, table, frequencies);
        int count = 0; for (int length = 1; length <= 16; length++) count += table->bits[length];
        if (count != active || fwrite(table->bits + 1, 1, 16, output) != 16
            || fputc(count & 255, output) == EOF || fputc(count >> 8, output) == EOF
            || fwrite(table->huffval, 1, (size_t)count, output) != (size_t)count)
            fail("Canonical table output differs or failed.");
    }
    if (ferror(input) || fclose(input) || fclose(output) || cases != 34) fail("Histogram batch incomplete.");
    jpeg_destroy_compress(&codec);
    printf("{\"cases\":%d,\"maximumCodeLength\":16}\n", cases);
    return 0;
}
