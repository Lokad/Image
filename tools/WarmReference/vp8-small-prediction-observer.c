/* Original development-only observer of native4x4 prediction on supplied
 * neighbor vectors. Cache addresses follow its two-row strip geometry. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "src/dsp/dsp.h"
#include "src/dsp/cpu.h"
extern VP8CPUInfo VP8GetCPUInfo;
static void fail(const char *message) { fprintf(stderr,"%s\n",message);_Exit(1); }
int main(int argc,char **argv) {
    const int scalar=argc==2 && !strcmp(argv[1],"scalar");
    if (argc!=1 && !scalar) fail("Usage: small-prediction-observer [scalar]");
    uint8_t header[8];if (fread(header,1,8,stdin)!=8 || memcmp(header,"IP75",4)) fail("Invalid predictor stream.");
    uint32_t count=(uint32_t)header[4]|(uint32_t)header[5]<<8|(uint32_t)header[6]<<16|(uint32_t)header[7]<<24;
    if (!count || count>64) fail("Invalid predictor count.");
    if (scalar) VP8GetCPUInfo=NULL;
    VP8EncDspInit();printf("{\"scalar\":%s,\"contexts\":%u,\"modes\":10,\"bytesPerContext\":160}\n",scalar?"true":"false",count);
    for (uint32_t context=0;context<count;context++) {
        _Alignas(32) uint8_t neighbors[64],cache[2048];uint8_t original[13];
        memset(neighbors,0x4d,sizeof(neighbors));
        if (fread(neighbors+27,1,13,stdin)!=13) fail("Truncated predictor context.");
        memcpy(original,neighbors+27,13);memset(cache,0xa7,sizeof(cache));
        VP8EncPredLuma4(cache,neighbors+32);
        if (memcmp(original,neighbors+27,13)) fail("Native predictor mutated neighbors.");
        for (int mode=0;mode<10;mode++) {
            const int offset=(48+4*(mode/8))*32+4*(mode%8);
            for (int y=0;y<4;y++) if (fwrite(cache+offset+y*32,1,4,stdout)!=4) fail("Write failed.");
        }
    }
    if (fgetc(stdin)!=EOF) fail("Trailing predictor input.");
    return 0;
}
