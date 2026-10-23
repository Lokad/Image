/* Original development-only driver. An ignored reference translation unit
 * exposes its existing static quantizer without changing its body or tables.
 * The ordinary complete encoder packets gate that instrumented producer.
 * This file contains only our fragments, observation and public API driver. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "src/enc/vp8i_enc.h"
#include "src/enc/cost_enc.h"
#include "src/dsp/dsp.h"

extern int LokadReferenceTrellis(const VP8Encoder*,int16_t[16],int16_t[16],int,int,const VP8Matrix*,int);
extern int __real_VP8Decimate(VP8EncIterator*,VP8ModeScore*,VP8RDLevel);
static int observed=0,method=0;
static void fail(const char *s){fprintf(stderr,"%s\n",s);exit(1);}
static void bytes(const void *p,size_t n){if(fwrite(p,1,n,stdout)!=n)fail("Output failure.");}
static void word(uint32_t v){for(int p=0;p<4;p++)if(fputc((v>>(8*p))&255,stdout)==EOF)fail("Output failure.");}
static void shorts(const int16_t *p){for(int i=0;i<16;i++){unsigned v=(uint16_t)p[i];if(fputc(v&255,stdout)==EOF||fputc(v>>8,stdout)==EOF)fail("Output failure.");}}

static void fragments(const VP8Encoder *enc){
 VP8Encoder *probe=calloc(1,sizeof(*probe));if(!probe)fail("Probe allocation failure.");
 const VP8SegmentInfo *segment=enc->dqm;
 for(int profile=0;profile<2;profile++){
  probe->proba=enc->proba;
  if(profile)for(int p=0;p<1056;p++)((uint8_t*)probe->proba.coeffs)[p]=(uint8_t)((37*p+17*(p/11)+p/33)&255);
  probe->proba.dirty=1;VP8CalculateLevelCosts(&probe->proba);
  for(int kind=0;kind<2;kind++){
   const int type=kind?3:0,lambda=kind?segment->lambda_trellis_i4:segment->lambda_trellis_i16;
   bytes("TR88",4);word((unsigned)profile);word((unsigned)type);word((unsigned)lambda);
   const VP8Matrix *matrix=&segment->y1;
   for(int p=0;p<16;p++)word(matrix->q[p]);
   for(int p=0;p<16;p++)word(matrix->iq[p]);
   for(int p=0;p<16;p++)word(matrix->bias[p]);
   for(int p=0;p<16;p++)word(matrix->zthresh[p]);
   for(int p=0;p<16;p++)word((unsigned)matrix->sharpen[p]);
   bytes(probe->proba.coeffs,1056);
   for(int fragment=0;fragment<36;fragment++){
    uint8_t source[16*BPS]={0},prediction[16*BPS]={0};
    int16_t original[16];
    for(int p=0;p<16;p++){
     int v=0;
     if(fragment==1)v=255;else if(fragment==2)v=-255;
     else if(fragment==3)v=((p/4+p%4)&1)?-255:255;
     else if(fragment>=4&&p==(fragment-4)/2)v=((fragment-4)&1)?-255:255;
     source[(p/4)*BPS+p%4]=(uint8_t)(v>0?v:0);
     prediction[(p/4)*BPS+p%4]=(uint8_t)(v<0?-v:0);
    }
    VP8FTransform(source,prediction,original);
    for(int context=0;context<3;context++){
     int16_t input[16],output[16];memcpy(input,original,sizeof(input));
     for(int p=0;p<16;p++)output[p]=77;
     const int nz=LokadReferenceTrellis(probe,input,output,context,type,matrix,lambda);
     if(fputc(nz,stdout)==EOF)fail("Output failure.");
     shorts(input);shorts(output);
    }
   }
  }
 }
 free(probe);
}

int __wrap_VP8Decimate(VP8EncIterator *it,VP8ModeScore *rd,VP8RDLevel level){
 if(!observed&&method==5)fragments(it->enc);
 observed++;return __real_VP8Decimate(it,rd,level);
}

int main(int argc,char **argv){
 if(argc!=2)fail("Usage: vp8-trellis-observer METHOD5..6");
 char *end;long parsed=strtol(argv[1],&end,10);if(!*argv[1]||*end||parsed<5||parsed>6)fail("Invalid method.");method=(int)parsed;
 const int width=513,height=259;const size_t length=(size_t)width*height*4;
 uint8_t *source=malloc(length);if(!source)fail("Allocation failure.");
 if(fread(source,1,length,stdin)!=length||fgetc(stdin)!=EOF)fail("Invalid source size.");
 WebPConfig config;WebPPicture picture;WebPMemoryWriter writer;
 if(!WebPConfigInit(&config)||!WebPPictureInit(&picture))fail("Initialization failure.");
 config.quality=75;config.method=method;config.thread_level=0;config.lossless=0;
 if(!WebPValidateConfig(&config))fail("Invalid configuration.");
 WebPMemoryWriterInit(&writer);picture.use_argb=1;picture.width=width;picture.height=height;picture.writer=WebPMemoryWrite;picture.custom_ptr=&writer;
 if(!WebPPictureImportRGBA(&picture,source,width*4))fail("Import failed.");
 bytes("TB88",4);if(!WebPEncode(&config,&picture))fail("Encoding failure.");
 if(observed!=561)fail("Unexpected decision count.");
 bytes("PKT8",4);word((unsigned)observed);word((unsigned)writer.size);bytes(writer.mem,writer.size);
 WebPMemoryWriterClear(&writer);WebPPictureFree(&picture);free(source);return 0;
}
