/* Original development-only link observer. It reads the completed native source
 * analysis before RDO; the existing public-API driver emits the final packet. */
#include <stdint.h>
#include <stdio.h>
#include "src/enc/vp8i_enc.h"

extern int __real_VP8EncAnalyze(VP8Encoder*);
extern int __real_VP8IteratorNext(VP8EncIterator*);
static int observing;
static unsigned histogram[256], count;
int __wrap_VP8IteratorNext(VP8EncIterator* it) {
    if (observing) {
        const VP8MBInfo* mb = it->mb;
        printf("%02x%02x%02x%02x", mb->alpha, mb->type, it->preds[0], mb->uv_mode);
        histogram[mb->alpha]++; count++;
    }
    return __real_VP8IteratorNext(it);
}
int __wrap_VP8EncAnalyze(VP8Encoder* enc) {
    observing = 1; count = 0;
    for (int i = 0; i < 256; i++) histogram[i] = 0;
    printf("{\"recordsHex\":\"");
    const int ok = __real_VP8EncAnalyze(enc);
    observing = 0;
    if (!ok) return ok;
    printf("\",\"analysisWidth\":%d,\"analysisHeight\":%d,\"records\":%u,\"meanAlpha\":%d,\"meanChroma\":%d,\"histogram\":[",
           enc->mb_w, enc->mb_h, count, enc->alpha, enc->uv_alpha);
    for (int i = 0; i < 256; i++) printf("%s%u", i ? "," : "", histogram[i]);
    printf("]}\n");
    return ok;
}
