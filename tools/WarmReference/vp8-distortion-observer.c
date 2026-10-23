/* Original development-only native metric observer. Supplied byte blocks and
 * analytically generated weights are test inputs, not production dependencies. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "src/dsp/dsp.h"
#include "src/dsp/cpu.h"
extern VP8CPUInfo VP8GetCPUInfo;
static void fail(const char *s) { fprintf(stderr,"%s\n",s);_Exit(1); }
static void emit(int value) {
    if (value<0) fail("Unexpected negative distortion.");
    uint8_t b[4];for(int p=0;p<4;p++)b[p]=(uint8_t)((uint32_t)value>>(8*p));
    if(fwrite(b,1,4,stdout)!=4)fail("Write failed.");
}
int main(int argc,char **argv) {
    int scalar=argc==2 && !strcmp(argv[1],"scalar");if(argc!=1 && !scalar)fail("Usage: distortion-observer [scalar]");
    uint8_t header[8];if(fread(header,1,8,stdin)!=8 || memcmp(header,"DS76",4))fail("Invalid metric stream.");
    uint32_t count=(uint32_t)header[4]|(uint32_t)header[5]<<8|(uint32_t)header[6]<<16|(uint32_t)header[7]<<24;
    if(!count || count>600)fail("Invalid metric count.");
    _Alignas(32)uint16_t weights[16];int sum=0;
    for(int p=0;p<16;p++){uint8_t b[2];if(fread(b,1,2,stdin)!=2)fail("Missing weight.");weights[p]=(uint16_t)(b[0]|b[1]<<8);if(weights[p]>256)fail("Invalid weight.");sum+=weights[p];}
    if(sum!=256)fail("Metric weights must sum256.");
    if(scalar)VP8GetCPUInfo=NULL;
    VP8EncDspInit();printf("{\"scalar\":%s,\"pairs\":%u,\"integersPerPair\":35}\n",scalar?"true":"false",count);
    for(uint32_t pair=0;pair<count;pair++) {
        _Alignas(32)uint8_t source[512],prediction[512],original[1024];
        if(fread(source,1,512,stdin)!=512 || fread(prediction,1,512,stdin)!=512)fail("Truncated metric pair.");
        memcpy(original,source,512);memcpy(original+512,prediction,512);
        emit(VP8SSE16x16(source,prediction));emit(VP8SSE16x8(source+16,prediction+16));emit(VP8TDisto16x16(source,prediction,weights));
        for(int y=0;y<16;y+=4)for(int x=0;x<16;x+=4){emit(VP8SSE4x4(source+y*32+x,prediction+y*32+x));emit(VP8TDisto4x4(source+y*32+x,prediction+y*32+x,weights));}
        if(memcmp(original,source,512)||memcmp(original+512,prediction,512))fail("Native metric mutated input.");
    }
    if(fgetc(stdin)!=EOF)fail("Trailing metric input.");
    return 0;
}
