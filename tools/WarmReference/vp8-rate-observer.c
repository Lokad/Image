/* Original development-only observer for generated VP8 rate costs. Reference
 * headers/archive are ignored tool inputs, never production build inputs. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "src/enc/cost_enc.h"
#include "src/dsp/cpu.h"
extern VP8CPUInfo VP8GetCPUInfo;
static void fail(const char *s) { fprintf(stderr,"%s\n",s); _Exit(1); }
static uint32_t read32(void) {
  uint8_t b[4]; if (fread(b,1,4,stdin)!=4) fail("Truncated input.");
  return (uint32_t)b[0]|(uint32_t)b[1]<<8|(uint32_t)b[2]<<16|(uint32_t)b[3]<<24;
}
static void emit(unsigned value,int bytes) {
  for (int p=0;p<bytes;p++) if (fputc((value>>(8*p))&255,stdout)==EOF) fail("Output failure.");
}
int main(int argc,char **argv) {
  const int scalar=argc==2 && !strcmp(argv[1],"scalar");
  if (argc!=1 && !scalar) fail("Usage: rate-observer [scalar]");
  char magic[4]; if (fread(magic,1,4,stdin)!=4 || memcmp(magic,"RC77",4)) fail("Unknown stream.");
  const uint32_t count=read32(); if (!count || count>30000) fail("Invalid block count.");
  if (scalar) VP8GetCPUInfo=NULL;
  VP8EncDspInit(); VP8EncDspCostInit();
  VP8EncProba *proba=calloc(2,sizeof(*proba)); if (!proba) fail("Allocation failure.");
  for (int profile=0;profile<2;profile++) {
    if (fread(proba[profile].coeffs,1,1056,stdin)!=1056) fail("Missing probability profile.");
    proba[profile].dirty=1; VP8CalculateLevelCosts(proba+profile);
  }
  printf("{\"scalar\":%s,\"blocks\":%u,\"profiles\":2,\"tableWords\":16752}\n",scalar?"true":"false",count);
  for (int p=0;p<256;p++) emit(VP8EntropyCost[p],2);
  for (int p=0;p<2048;p++) emit(VP8LevelFixedCosts[p],2);
  for (int p=0;p<4;p++) emit(VP8FixedCostsI16[p],2);
  for (int p=0;p<4;p++) emit(VP8FixedCostsUV[p],2);
  for (int top=0;top<10;top++) for (int left=0;left<10;left++) for (int mode=0;mode<10;mode++) emit(VP8FixedCostsI4[top][left][mode],2);
  for (int profile=0;profile<2;profile++) {
    for (int type=0;type<4;type++) for (int band=0;band<8;band++) for (int ctx=0;ctx<3;ctx++)
      for (int bit=0;bit<2;bit++) emit(VP8BitCost(bit,proba[profile].coeffs[type][band][ctx][0]),2);
    for (int type=0;type<4;type++) for (int band=0;band<8;band++) for (int ctx=0;ctx<3;ctx++)
      for (int level=0;level<68;level++) emit(proba[profile].level_cost[type][band][ctx][level],2);
  }
  for (uint32_t block=0;block<count;block++) {
    const uint32_t type=read32(),first=read32();
    if (type>3 || first>1) fail("Invalid residual type.");
    _Alignas(32)int16_t levels[16];
    for (int p=0;p<16;p++) { int v=(int)read32(); if (v < -2047 || v > 2047) fail("Invalid level."); levels[p]=(int16_t)v; }
    if (first && levels[0]) fail("AC-only residual has DC.");
    for (int profile=0;profile<2;profile++) {
      VP8Residual res; memset(&res,0,sizeof(res)); res.first=(int)first; res.coeff_type=(int)type;
      res.prob=proba[profile].coeffs[type]; res.costs=proba[profile].remapped_costs[type];
      VP8SetResidualCoeffs(levels,&res);
      for (int ctx=0;ctx<3;ctx++) emit((unsigned)VP8GetResidualCost(ctx,&res),4);
    }
  }
  if (fgetc(stdin)!=EOF) fail("Trailing input.");
  free(proba); return 0;
}
