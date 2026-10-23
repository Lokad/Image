/* Original development-only public-API observer of FFmpeg's encoder backend.
 * Emits one JSON line followed by its complete encoded packet. Statistics are
 * trusted only after the caller verifies a frozen FFmpeg packet hash. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <webp/encode.h>

static void fail(const char *s) { fprintf(stderr, "%s\n", s); exit(1); }
static int setting(const char *s, int minimum, int maximum) {
    char *end; long n = strtol(s, &end, 10);
    if (!*s || *end || n < minimum || n > maximum) fail("Invalid setting.");
    return (int)n;
}
static void integers(const char *name, const int *values, int count) {
    printf("\"%s\":[", name);
    for (int p = 0; p < count; p++) printf("%s%d", p ? "," : "", values[p]);
    printf("]");
}
int main(int argc, char **argv) {
    if (argc != 3 && argc != 5) fail("Usage: libwebp-vp8-decisions-observer WIDTH HEIGHT [QUALITY METHOD] < ORIGINAL-RGBA8");
    const int width = setting(argv[1], 1, 16383), height = setting(argv[2], 1, 16383);
    const int quality = argc == 5 ? setting(argv[3], 1, 100) : 75;
    const int method = argc == 5 ? setting(argv[4], 0, 6) : 4;
    const size_t bytes = (size_t)width*height*4, macroblocks = (size_t)((width+15)/16)*((height+15)/16);
    if (bytes > 16U*1024U*1024U) fail("Observer input exceeds16MiB.");
    uint8_t *source = malloc(bytes), *segments = calloc(macroblocks, 1);
    if (!source || !segments) fail("Allocation failed.");
    if (fread(source, 1, bytes, stdin) != bytes || fgetc(stdin) != EOF) fail("Input size differs.");
    WebPConfig config; WebPPicture picture; WebPMemoryWriter writer; WebPAuxStats stats;
    if (!WebPConfigInit(&config) || !WebPPictureInit(&picture)) fail("Initialization failed.");
    config.lossless = 0; config.quality = (float)quality; config.method = method; config.thread_level = 0;
    if (!WebPValidateConfig(&config)) fail("Invalid configuration.");
    WebPMemoryWriterInit(&writer); memset(&stats, 0, sizeof(stats));
    picture.use_argb = 1; picture.width = width; picture.height = height;
    picture.writer = WebPMemoryWrite; picture.custom_ptr = &writer;
    picture.stats = &stats; picture.extra_info_type = 2; picture.extra_info = segments;
    if (!WebPPictureImportRGBA(&picture, source, width*4)) fail("RGBA import failed.");
    if (!WebPEncode(&config, &picture)) fail("Native encoding failed.");
    if (writer.size < 20 || memcmp(writer.mem, "RIFF", 4) || memcmp(writer.mem+8, "WEBP", 4)) fail("Complete WebP missing.");
    printf("{\"libwebpVersion\":%d,\"packetBytes\":%zu,\"macroblocks\":%zu,\"extraInfoType\":2,", WebPGetEncoderVersion(), writer.size, macroblocks);
    printf("\"config\":{\"quality\":%d,\"method\":%d,\"threadLevel\":0,\"segments\":%d,\"snsStrength\":%d,\"filterStrength\":%d,\"filterSharpness\":%d,\"filterType\":%d,\"autoFilter\":%d,\"pass\":%d,\"preprocessing\":%d,\"partitions\":%d,\"partitionLimit\":%d,\"alphaCompression\":%d,\"alphaFiltering\":%d,\"alphaQuality\":%d,\"exact\":%d,\"lowMemory\":%d,\"sharpYuv\":%d},",
        quality, method, config.segments, config.sns_strength, config.filter_strength, config.filter_sharpness, config.filter_type, config.autofilter, config.pass, config.preprocessing, config.partitions, config.partition_limit, config.alpha_compression, config.alpha_filtering, config.alpha_quality, config.exact, config.low_memory, config.use_sharp_yuv);
    integers("segmentQuantizers", stats.segment_quant, 4); printf(",");
    integers("segmentFilterLevels", stats.segment_level, 4); printf(",");
    integers("segmentSizes", stats.segment_size, 4); printf(",");
    integers("blockCounts", stats.block_count, 3);
    printf(",\"alphaDataBytes\":%d,\"segmentMapHex\":\"", stats.alpha_data_size);
    for (size_t p = 0; p < macroblocks; p++) printf("%02x", segments[p]);
    printf("\"}\n");
    if (fwrite(writer.mem, 1, writer.size, stdout) != writer.size) fail("Write packet failed.");
    WebPMemoryWriterClear(&writer); WebPPictureFree(&picture); free(segments); free(source);
    return 0;
}
