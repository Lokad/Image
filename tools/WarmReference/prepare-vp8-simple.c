/* Original fixture author through the public libwebp API. Not a timed worker.
 * Encoded structure is independently inspected per RFC 9649 / RFC 6386. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <webp/encode.h>

static void fail(const char *message) { fprintf(stderr, "%s\n", message); exit(1); }
int main(int argc, char **argv)
{
    enum { width = 513, height = 259, bytes = width * height * 4 };
    if (argc != 3) fail("Usage: prepare-vp8-simple OPAQUE_RGBA8 OUTPUT_WEBP");
    uint8_t *rgba = malloc(bytes); if (!rgba) fail("Allocate raster failed.");
    FILE *input = fopen(argv[1], "rb");
    if (!input || fread(rgba, 1, bytes, input) != bytes || fgetc(input) != EOF
        || ferror(input) || fclose(input)) fail("Original raster size/read differs.");
    for (int i = 3; i < bytes; i += 4) if (rgba[i] != 255) fail("Opaque projection required.");
    WebPConfig config; WebPPicture picture; WebPMemoryWriter output;
    if (!WebPConfigInit(&config) || !WebPPictureInit(&picture)) fail("Backend ABI differs.");
    config.lossless = 0; config.quality = 75; config.method = 4; config.thread_level = 0;
    config.filter_type = 0; config.filter_strength = 60; config.filter_sharpness = 0;
    config.autofilter = 0; config.segments = 1;
    if (!WebPValidateConfig(&config)) fail("Fixed encoder configuration differs.");
    picture.width = width; picture.height = height; picture.use_argb = 0;
    WebPMemoryWriterInit(&output); picture.writer = WebPMemoryWrite; picture.custom_ptr = &output;
    if (!WebPPictureImportRGBA(&picture, rgba, width * 4) || !WebPEncode(&config, &picture))
        fail("Encode original simple-filter fixture failed.");
    FILE *file = fopen(argv[2], "wb");
    if (!file || !output.size || fwrite(output.mem, 1, output.size, file) != output.size || fclose(file))
        fail("Write complete fixture failed.");
    printf("{\"backendVersion\":%d,\"filterType\":0,\"filterStrength\":60,\"quality\":75,\"method\":4,\"threads\":1,\"segments\":1,\"outputBytes\":%zu}\n",
        WebPGetEncoderVersion(), output.size);
    WebPMemoryWriterClear(&output); WebPPictureFree(&picture); free(rgba); return 0;
}
