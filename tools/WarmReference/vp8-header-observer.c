/* Original development-only header-pass observations. Generates two declared
 * procedural controls in RAM; no encoded input or upstream corpus is used. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <webp/encode.h>

static void fail(const char *s) { fprintf(stderr, "%s\n", s); exit(1); }
static void bytes(const void *p, size_t n) { if (fwrite(p, 1, n, stdout) != n) fail("Write failed."); }
static void word(uint32_t v) { for (int i = 0; i < 4; i++) if (fputc((v >> (8 * i)) & 255, stdout) == EOF) fail("Write failed."); }
void LokadHeaderPass(int blocks, int limit, uint64_t cost, int tokens) {
  bytes("PASS", 4); word((uint32_t)blocks); word((uint32_t)limit);
  word((uint32_t)cost); word((uint32_t)(cost >> 32)); word((uint32_t)tokens);
}
int main(int argc, char **argv) {
  if (argc != 2 || (strcmp(argv[1], "anchor") && strcmp(argv[1], "large"))) fail("Expected anchor or large.");
  const int large = !strcmp(argv[1], "large");
  const int width = large ? 5632 : 513, height = large ? 5632 : 259;
  const size_t length = (size_t)width * height * 4;
  uint8_t *source = malloc(length); if (source == NULL) fail("Allocation failed.");
  for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) {
    const size_t at = ((size_t)y * width + x) * 4;
    for (uint32_t c = 0; c < 3; c++) {
      const uint32_t sample = ((uint32_t)x * 12347u + (uint32_t)y * 7919u + c * 23459u + (uint32_t)x * (uint32_t)y * 101u) & 65535u;
      source[at + c] = (uint8_t)((sample + 128u) / 257u);
    }
    source[at + 3] = 255;
  }
  // Source bytes are streamed to the observer caller for independent validation;
  // neither host needs a loose full-size raster on disk.
  bytes("HN97", 4); word((uint32_t)width); word((uint32_t)height); word((uint32_t)length); bytes(source, length);
  WebPConfig config; WebPPicture picture; WebPMemoryWriter writer; WebPAuxStats stats;
  if (!WebPConfigInit(&config) || !WebPPictureInit(&picture)) fail("Init failed.");
  config.quality = 75; config.method = 4; config.thread_level = 0;
  if (!WebPValidateConfig(&config)) fail("Invalid configuration.");
  memset(&stats, 0, sizeof(stats)); WebPMemoryWriterInit(&writer);
  picture.use_argb = 1; picture.width = width; picture.height = height;
  picture.stats = &stats; picture.writer = WebPMemoryWrite; picture.custom_ptr = &writer;
  if (!WebPPictureImportRGBA(&picture, source, width * 4) || !WebPEncode(&config, &picture)) fail("Native encoding failed.");
  if (writer.size > UINT32_MAX) fail("Packet length is not representable.");
  bytes("PKT7", 4); word((uint32_t)writer.size); bytes(writer.mem, writer.size);
  WebPMemoryWriterClear(&writer); WebPPictureFree(&picture); free(source); return 0;
}
