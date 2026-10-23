/* Original development-only VP8 quantization-kernel observer. Matrices and
 * coefficient blocks are supplied original/previously qualified test inputs.
 * Reference headers and static library remain ignored external tool inputs. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "src/enc/vp8i_enc.h"
#include "src/dsp/cpu.h"
extern VP8CPUInfo VP8GetCPUInfo;
static void fail(const char *message) { fprintf(stderr,"%s\n",message); _Exit(1); }
static uint32_t word(void) {
    uint8_t b[4]; if (fread(b,1,4,stdin)!=4) fail("Truncated kernel input.");
    return (uint32_t)b[0]|(uint32_t)b[1]<<8|(uint32_t)b[2]<<16|(uint32_t)b[3]<<24;
}
static int16_t coefficient(void) {
    uint8_t b[2]; if (fread(b,1,2,stdin)!=2) fail("Truncated coefficients.");
    int value=b[0]|b[1]<<8; if (value>=32768) value-=65536;
    if (value < -16320 || value > 16320) fail("Coefficient outside transform domain.");
    return (int16_t)value;
}
static void emit(const int16_t *values) {
    uint8_t b[32];
    for (int p=0;p<16;p++) { b[2*p]=(uint8_t)values[p];b[2*p+1]=(uint8_t)((uint16_t)values[p]>>8); }
    if (fwrite(b,1,sizeof(b),stdout)!=sizeof(b)) fail("Write failed.");
}
int main(int argc,char **argv) {
    const int scalar=argc==2 && !strcmp(argv[1],"scalar");
    if (argc!=1 && !scalar) fail("Usage: quantization-observer [scalar]");
    char magic[4]; if (fread(magic,1,4,stdin)!=4 || memcmp(magic,"LQ73",4)) fail("Unknown kernel stream.");
    const uint32_t groups=word(); if (!groups || groups>8) fail("Too many kernel groups.");
    if (scalar) VP8GetCPUInfo=NULL;
    VP8EncDspInit();
    printf("{\"scalar\":%s,\"groups\":%u,\"bytesPerBlock\":65,\"coefficientBound\":16320}\n",scalar?"true":"false",groups);
    for (uint32_t group=0;group<groups;group++) {
        const uint32_t walsh=word(), blocks=word();
        if (walsh>1 || !blocks || blocks>20000) fail("Invalid kernel group.");
        _Alignas(32) VP8Matrix matrix;
        for (int p=0;p<16;p++) { uint32_t v=word();if (!v || v>440) fail("Invalid step.");matrix.q[p]=(uint16_t)v; }
        for (int p=0;p<16;p++) { uint32_t v=word();if (!v || v>32768) fail("Invalid reciprocal.");matrix.iq[p]=(uint16_t)v; }
        for (int p=0;p<16;p++) { uint32_t v=word();if (v>131071) fail("Invalid bias.");matrix.bias[p]=v; }
        for (int p=0;p<16;p++) { uint32_t v=word();if (v>16320) fail("Invalid threshold.");matrix.zthresh[p]=v; }
        for (int p=0;p<16;p++) { uint32_t v=word();if (v>90) fail("Invalid sharpening.");matrix.sharpen[p]=(uint16_t)v; }
        for (uint32_t block=0;block<blocks;block++) {
            _Alignas(32) int16_t input[16],output[16];
            for (int p=0;p<16;p++) input[p]=coefficient();
            const int nz=walsh?VP8EncQuantizeBlockWHT(input,output,&matrix):VP8EncQuantizeBlock(input,output,&matrix);
            if (fputc(nz!=0,stdout)==EOF) fail("Write failed.");
            emit(input);emit(output);
        }
    }
    if (fgetc(stdin)!=EOF) fail("Trailing input.");
    return 0;
}
