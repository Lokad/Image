/* Original development-only public-API adapter. RIFF chunk framing follows
 * RFC 9649. No upstream implementation, tables or test material copied. */
#define _GNU_SOURCE
#include <errno.h>
#include <inttypes.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <webp/encode.h>

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
static int number(const char *text)
{
    char *end; errno = 0; long value = strtol(text, &end, 10);
    if (errno || !*text || *end || value < 1 || value > 32768) fail("Invalid bounded integer.");
    return (int)value;
}
static int affinity_cpu(void)
{
    cpu_set_t mask;
    if (sched_getaffinity(0, sizeof(mask), &mask) || CPU_COUNT(&mask) != 1) fail("Exactly one CPU required.");
    for (int cpu = 0; cpu < CPU_SETSIZE; cpu++) if (CPU_ISSET(cpu, &mask)) return cpu;
    fail("No affinity CPU."); return -1;
}
static uint64_t now_ns(void)
{
    struct timespec value; if (clock_gettime(CLOCK_MONOTONIC, &value)) fail("Clock failed.");
    return (uint64_t)value.tv_sec * UINT64_C(1000000000) + (uint64_t)value.tv_nsec;
}
static uint8_t *read_file(const char *path, size_t size)
{
    FILE *file = fopen(path, "rb"); if (!file) fail("Open raster failed.");
    uint8_t *data = malloc(size); if (!data) fail("Allocate raster failed.");
    if (fread(data, 1, size, file) != size || fgetc(file) != EOF || ferror(file) || fclose(file)) fail("Raster size/read differs.");
    return data;
}
static uint32_t little(const uint8_t *p, int count)
{
    uint32_t value = 0; for (int i = 0; i < count; i++) value |= (uint32_t)p[i] << (8*i); return value;
}
typedef struct {
    WebPConfig config;
    WebPPicture picture;
    WebPMemoryWriter packet;
    uint8_t *bgra, *source_copy;
    uint8_t *original, *expected, *normalized_expected;
    size_t raw_bytes;
    int width, height, depth, cpu, quality, output_bytes, expected_bytes;
    uint64_t completed, observed;
} Work;
static void complete_webp(const Work *work, const WebPMemoryWriter *packet)
{
    const uint8_t *p = packet->mem; size_t size = (size_t)packet->size;
    if (!packet->mem || packet->size < 12 || memcmp(p,"RIFF",4) || memcmp(p+8,"WEBP",4)
        || (uint64_t)little(p+4,4)+8 != size) fail("Independent complete WebP missing.");
    size_t offset = 12; int lossless = 0, extended = 0;
    while (offset < size) {
        if (size-offset < 8) fail("Truncated WebP chunk.");
        uint32_t length = little(p+offset+4,4);
        if ((uint64_t)length+(length&1)+8 > size-offset) fail("Invalid WebP chunk size.");
        const uint8_t *payload = p+offset+8;
        if (!memcmp(p+offset,"VP8X",4)) {
            if (extended++ || offset != 12 || length != 10 || payload[0] != 16
                || little(payload+4,3)+1 != (unsigned)work->width
                || little(payload+7,3)+1 != (unsigned)work->height) fail("WebP extended geometry/flags differ.");
        } else if (!memcmp(p+offset,"VP8L",4)) {
            if (lossless++ || length < 5 || payload[0] != 0x2f) fail("Missing lossless signature.");
            uint32_t header = little(payload+1,4);
            if ((header&0x3fff)+1 != (unsigned)work->width
                || ((header>>14)&0x3fff)+1 != (unsigned)work->height
                || (header>>29)) fail("Lossless geometry/version differs.");
        } else fail("Unexpected lossy/animation/metadata chunk.");
        if ((length&1) && payload[length] != 0) fail("Invalid RIFF padding.");
        offset += 8+(size_t)length+(length&1);
    }
    if (lossless != 1) fail("Expected one still VP8L payload.");
}
static void apply(Work *work)
{
    /* Public libwebp API, the encoder backend used by FFmpeg. exact=1 preserves
     * hidden RGB. Repack and import every source pixel inside each operation. */
    if (work->depth == 16) {
        const int channels[4] = {2,1,0,3};
        size_t pixels = (size_t)work->width * work->height;
        for (size_t i = 0; i < pixels; i++)
            for (int c = 0; c < 4; c++) {
                unsigned value = little(work->original + i*8 + channels[c]*2, 2);
                work->bgra[i*4+c] = (uint8_t)((value+128)/257);
            }
    } else
    for (size_t i = 0; i < work->raw_bytes; i += 4) {
        work->bgra[i] = work->original[i+2]; work->bgra[i+1] = work->original[i+1];
        work->bgra[i+2] = work->original[i]; work->bgra[i+3] = work->original[i+3];
    }
    WebPMemoryWriterInit(&work->packet);
    if (!WebPPictureImportBGRA(&work->picture, work->bgra, work->width*4)) fail("Import source failed.");
    if (!WebPEncode(&work->config, &work->picture)) fail("Exact lossless encode failed.");
    WebPPictureFree(&work->picture);
    complete_webp(work,&work->packet);
    if (work->expected_bytes && work->packet.size != (size_t)work->expected_bytes) fail("Repeated WebP size differs.");
    if (work->packet.size > INT32_MAX) fail("Packet too large.");
    work->output_bytes = (int)work->packet.size;
    work->observed += work->packet.mem[work->packet.size/2]; work->completed++;
}
static void check_source(const Work *work)
{
    if (memcmp(work->original,work->source_copy,work->raw_bytes)) fail("Source changed.");
    if (work->normalized_expected) {
        const int channels[4] = {2,1,0,3};
        size_t pixels = (size_t)work->width * work->height;
        for (size_t i = 0; i < pixels; i++)
            for (int c = 0; c < 4; c++)
                if (work->bgra[i*4+c] != work->normalized_expected[i*4+channels[c]])
                    fail("Full-source normalized reference differs.");
    }
}
static void check_packet(const Work *work, const WebPMemoryWriter *packet)
{
    if (!packet->mem || packet->size != (size_t)work->expected_bytes
        || memcmp(packet->mem, work->expected, (size_t)packet->size)) fail("Repeated WebP bitstream differs.");
}
static void write_packet(const Work *work, const char *path)
{
    FILE *file = fopen(path, "wb"); if (!file) fail("Open result failed.");
    if (fwrite(work->packet.mem, 1, (size_t)work->packet.size, file) != (size_t)work->packet.size || fclose(file)) fail("Write result failed.");
}
int main(int argc, char **argv)
{
    if ((argc != 8 && argc != 9) || strcmp(argv[1], "webp-lossless-encode")) fail("Usage: libwebp-lossless-warm webp-lossless-encode INPUT WIDTH HEIGHT 8|16 OUTPUT QUALITY [NORMALIZED-RGBA8 when depth16]");
    Work work = {0}; work.cpu = affinity_cpu();
    work.width = number(argv[3]); work.height = number(argv[4]); work.quality = number(argv[7]); work.depth = number(argv[5]);
    if ((work.depth != 8 && work.depth != 16) || work.quality > 100 || argc != 8+(work.depth==16)) fail("WebP requires depth 8/16, the matching reference for depth16 and quality 1..100.");
    work.raw_bytes = (size_t)work.width * work.height * 4 * (work.depth/8);
    if (work.raw_bytes > 512U*1024U*1024U) fail("Reference raster exceeds 512 MiB.");
    work.original = read_file(argv[2], work.raw_bytes);
    if (work.depth == 16) work.normalized_expected = read_file(argv[8], (size_t)work.width * work.height * 4);
    work.source_copy = malloc(work.raw_bytes); work.bgra = malloc((size_t)work.width * work.height * 4);
    if (!work.source_copy || !work.bgra) fail("Allocate source/scratch failed.");
    memcpy(work.source_copy,work.original,work.raw_bytes);
    if (!WebPConfigInit(&work.config) || !WebPPictureInit(&work.picture)) fail("Initialize public libwebp API failed.");
    work.config.lossless = 1; work.config.method = 4; work.config.quality = (float)work.quality;
    work.config.exact = 1; work.config.thread_level = 0;
    if (!WebPValidateConfig(&work.config)) fail("Invalid exact configuration.");
    work.picture.width = work.width; work.picture.height = work.height; work.picture.use_argb = 1;
    work.picture.writer = WebPMemoryWrite; work.picture.custom_ptr = &work.packet;
    apply(&work); work.expected_bytes = work.output_bytes;
    work.expected = malloc((size_t)work.output_bytes); if (!work.expected) fail("Allocate check bytes failed.");
    memcpy(work.expected, work.packet.mem, (size_t)work.output_bytes);
    WebPMemoryWriter held = work.packet;
    WebPMemoryWriterInit(&work.packet); apply(&work);
    if (held.mem == work.packet.mem) fail("Live packets alias.");
    check_packet(&work,&held); check_packet(&work,&work.packet);
    WebPMemoryWriterClear(&held); WebPMemoryWriterClear(&work.packet);
    apply(&work); check_packet(&work,&work.packet); check_source(&work); write_packet(&work,argv[6]); WebPMemoryWriterClear(&work.packet);
    printf("{\"schemaVersion\":1,\"state\":\"ready\",\"operation\":\"webp-lossless-encode\",\"width\":%d,\"height\":%d,\"depth\":%d,\"cpu\":%d,\"affinityCount\":1,\"codecThreads\":1,\"threadType\":0,\"activeThreadType\":0,\"conversionThreads\":1,\"quality\":%d,\"effort\":4,\"lossless\":true,\"exactRgb\":true,\"baselineBackend\":\"libwebp exact=1 (FFmpeg encoder backend)\",\"inputFormat\":\"BGRA\",\"repackingTimed\":true,\"fullSourceNormalizationTimed\":%s,\"normalizedReferenceExact\":%s,\"inputImportAllocationTimed\":true,\"outputBytes\":%d,\"initialCompletedOperations\":%" PRIu64 ",\"independentLiveOutputs\":true,\"bitstreamDeterministic\":true,\"libwebpEncoderVersion\":%d}\n",
        work.width,work.height,work.depth,work.cpu,work.quality,work.depth == 16 ? "true" : "false",work.depth == 16 ? "true" : "false",work.output_bytes,work.completed,WebPGetEncoderVersion()); fflush(stdout);
    char line[128];
    while (fgets(line, sizeof(line), stdin)) {
        if (!strcmp(line, "quit\n")) break;
        if (!strcmp(line, "check\n")) {
            apply(&work); check_packet(&work, &work.packet); check_source(&work); write_packet(&work, argv[6]); WebPMemoryWriterClear(&work.packet);
            puts("{\"state\":\"checked\",\"exact\":true,\"sourceUnchanged\":true}"); fflush(stdout); continue;
        }
        uint64_t count; char extra;
        if (sscanf(line, "sample %" SCNu64 " %c", &count, &extra) != 1 || !count || count > UINT64_C(10000000)) fail("Invalid bounded sample request.");
        uint64_t completed = work.completed, start = now_ns();
        for (uint64_t i = 0; i < count; i++) { apply(&work); WebPMemoryWriterClear(&work.packet); }
        uint64_t elapsed = now_ns()-start;
        if (sched_getcpu() != work.cpu || affinity_cpu() != work.cpu || work.completed-completed != count) fail("Sample CPU/count differs.");
        printf("{\"state\":\"sample\",\"operations\":%" PRIu64 ",\"elapsedNanoseconds\":%" PRIu64 ",\"completedOperations\":%" PRIu64 ",\"observed\":%" PRIu64 ",\"outputStride\":0,\"outputBytes\":%d,\"cpu\":%d}\n",
            count, elapsed, work.completed, work.observed, work.output_bytes, work.cpu); fflush(stdout);
    }
    if (ferror(stdin)) fail("Protocol read failed.");
    WebPMemoryWriterClear(&work.packet); WebPPictureFree(&work.picture);
    free(work.original); free(work.source_copy); free(work.bgra); free(work.expected); free(work.normalized_expected); return 0;
}
