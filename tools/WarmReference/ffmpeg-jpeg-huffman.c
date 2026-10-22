/* Original bounded table probe for the pinned FFmpeg archive. Native tooling
 * only: our own ABI declaration accepts the 256-count record. No implementation
 * source/header/table is imported. Inputs are original deterministic histograms. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

extern void ff_mjpeg_encode_huffman_close(void *, uint8_t *, uint8_t *, int);
static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
int main(int argc, char **argv)
{
    if (argc != 3) fail("Usage: ffmpeg-jpeg-huffman HISTOGRAMS OUTPUT");
    FILE *input = fopen(argv[1], "rb"), *output = fopen(argv[2], "wb");
    if (!input || !output) fail("Open failed.");
    uint8_t bytes[1024]; int cases = 0;
    size_t size;
    while ((size = fread(bytes, 1, sizeof(bytes), input)) != 0) {
        if (size != sizeof(bytes) || ++cases > 32) fail("Exactly bounded histograms required.");
        int counts[256], active = 0;
        for (int i = 0; i < 256; i++) {
            uint32_t value = bytes[i*4] | (uint32_t)bytes[i*4+1] << 8
                | (uint32_t)bytes[i*4+2] << 16 | (uint32_t)bytes[i*4+3] << 24;
            if (value > 1000000) fail("Histogram count exceeds probe bound.");
            counts[i] = (int)value; active += value != 0;
        }
        if (!active) fail("Nonempty histogram required.");
        uint8_t bits[17] = {0}, symbols[256] = {0};
        ff_mjpeg_encode_huffman_close(counts, bits, symbols, 256);
        int total = 0;
        for (int i = 1; i <= 16; i++) total += bits[i];
        if (total != active || fwrite(bits+1, 1, 16, output) != 16
            || fputc(total & 255, output) == EOF || fputc(total >> 8, output) == EOF
            || fwrite(symbols, 1, (size_t)total, output) != (size_t)total) fail("Table output failed.");
    }
    if (ferror(input) || fclose(input) || fclose(output) || cases != 32) fail("Histogram batch incomplete.");
    printf("{\"exactTableCases\":%d,\"maximumLength\":16}\n", cases);
    return 0;
}
