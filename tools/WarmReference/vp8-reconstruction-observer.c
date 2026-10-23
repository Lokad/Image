/* Original development-only native inverse/reconstruction observer. Supplied
 * coefficients come from retained original-control transform/quantization data.
 * Reference headers/library remain ignored external tool inputs. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "src/dsp/dsp.h"
#include "src/dsp/cpu.h"
extern VP8CPUInfo VP8GetCPUInfo;
static void fail(const char *message) { fprintf(stderr,"%s\n",message);_Exit(1); }
static uint32_t word(void) {
    uint8_t b[4];if (fread(b,1,4,stdin)!=4) fail("Truncated group.");
    return (uint32_t)b[0]|(uint32_t)b[1]<<8|(uint32_t)b[2]<<16|(uint32_t)b[3]<<24;
}
static int16_t coefficient(int bound) {
    uint8_t b[2];if (fread(b,1,2,stdin)!=2) fail("Truncated coefficients.");
    int v=b[0]|b[1]<<8;if (v>=32768) v-=65536;
    if (v < -bound || v > bound) fail("Coefficient outside source-transform domain.");
    return (int16_t)v;
}
int main(int argc,char **argv) {
    const int scalar=argc==2 && !strcmp(argv[1],"scalar");
    if (argc!=1 && !scalar) fail("Usage: reconstruction-observer [scalar]");
    char magic[4];if (fread(magic,1,4,stdin)!=4 || memcmp(magic,"IR74",4)) fail("Unknown inverse stream.");
    uint32_t groups=word();if (!groups || groups>8) fail("Invalid group count.");
    if (scalar) VP8GetCPUInfo=NULL;
    VP8EncDspInit();VP8DspInit();
    printf("{\"scalar\":%s,\"groups\":%u,\"pixelStride\":32}\n",scalar?"true":"false",groups);
    for (uint32_t group=0;group<groups;group++) {
        uint32_t kind=word(),blocks=word();if (kind>2 || !blocks || blocks>20000) fail("Invalid inverse group.");
        for (uint32_t block=0;block<blocks;block++) {
            const int count=kind==2?32:16;
            _Alignas(32) int16_t input[32],original[32],dc[256];
            for (int p=0;p<count;p++) input[p]=coefficient(kind==1?16320:4096);
            memcpy(original,input,(size_t)count*sizeof(input[0]));
            if (kind==1) {
                for (int p=0;p<256;p++) dc[p]=0x5a5a;
                VP8TransformWHT(input,dc);
                uint8_t bytes[32];
                for (int p=0;p<16;p++) { bytes[2*p]=(uint8_t)dc[p*16];bytes[2*p+1]=(uint8_t)((uint16_t)dc[p*16]>>8); }
                for (int p=0;p<256;p++) if (p%16 && dc[p]!=0x5a5a) fail("Native WHT overwrote AC slots.");
                if (fwrite(bytes,1,sizeof(bytes),stdout)!=sizeof(bytes)) fail("Write DC failed.");
            } else {
                _Alignas(32) uint8_t prediction[128],output[128];const int width=kind==2?8:4;
                for (int y=0;y<4;y++) for (int x=0;x<32;x++) prediction[y*32+x]=(uint8_t)(29*block+73*x+41*y);
                memset(output,0xa7,sizeof(output));
                VP8ITransform(prediction,input,output,kind==2);
                for (int y=0;y<4;y++) {
                    if (fwrite(output+y*32,1,(size_t)width,stdout)!=(size_t)width) fail("Write pixels failed.");
                    for (int x=width;x<32;x++) if (output[y*32+x]!=0xa7) fail("Native inverse overwrote row padding.");
                    for (int x=0;x<32;x++) if (prediction[y*32+x]!=(uint8_t)(29*block+73*x+41*y)) fail("Native prediction mutated.");
                }
            }
            if (memcmp(input,original,(size_t)count*sizeof(input[0]))) fail("Native coefficients mutated.");
        }
    }
    if (fgetc(stdin)!=EOF) fail("Trailing inverse input.");
    return 0;
}
