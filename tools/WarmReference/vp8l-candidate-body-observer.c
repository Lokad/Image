/* Original development-only observations; hooks receive scalar state and
 * selected palette values from an ignored instrumented reference checkout. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include "webp/encode.h"
#include "src/utils/bit_writer_utils.h"
static unsigned analysis;
static void fail(const char *s) { fprintf(stderr, "%s\n", s); exit(1); }
static void bytes(const void *p, size_t n) { if (fwrite(p, 1, n, stdout) != n) fail("Write failed."); }
static void word(uint32_t v) { for (int i = 0; i < 4; i++) if (fputc((v >> (8 * i)) & 255, stdout) == EOF) fail("Write failed."); }
void LokadBeginStream(void) { analysis++; }
void LokadCandidateBody(int idx, int entropy, int sort, const void *source) {
  const VP8LBitWriter *bw = (const VP8LBitWriter*)source;
  VP8LBitWriter clone;
  if (!VP8LBitWriterInit(&clone, 0) || !VP8LBitWriterClone(bw, &clone)) fail("Clone failed.");
  const uint8_t *body = VP8LBitWriterFinish(&clone);
  const size_t size = VP8LBitWriterNumBytes(&clone);
  if (clone.error || size > 1000000) fail("Snapshot failed.");
  bytes("BD94", 4); word(analysis - 1); word((unsigned)idx); word((unsigned)entropy); word((unsigned)sort); word((unsigned)size);
  bytes(body, size); VP8LBitWriterWipeOut(&clone);
}
int main(void) {
  const int width = 513, height = 259; const size_t length = (size_t)width * height * 4;
  uint8_t *source = malloc(length); if (source == NULL) fail("Allocation failed.");
  if (fread(source, 1, length, stdin) != length || fgetc(stdin) != EOF) fail("Wrong input size.");
  WebPConfig config; WebPPicture picture; WebPMemoryWriter writer;
  if (!WebPConfigInit(&config) || !WebPPictureInit(&picture)) fail("Init failed.");
  config.quality = 75; config.method = 6; config.thread_level = 0;
  if (!WebPValidateConfig(&config)) fail("Config failed.");
  WebPMemoryWriterInit(&writer); picture.use_argb = 1; picture.width = width; picture.height = height;
  picture.writer = WebPMemoryWrite; picture.custom_ptr = &writer;
  if (!WebPPictureImportRGBA(&picture, source, width * 4)) fail("Import failed.");
  bytes("BC94", 4); if (!WebPEncode(&config, &picture)) fail("Encode failed.");
  if (writer.size != 85796) fail("Wrong packet length.");
  bytes("PKT4", 4); word((unsigned)writer.size); bytes(writer.mem, writer.size);
  WebPMemoryWriterClear(&writer); WebPPictureFree(&picture); free(source); return 0;
}
