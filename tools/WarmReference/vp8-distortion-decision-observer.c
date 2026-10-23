/* Original development-only observer of the pinned distortion-only decision.
 * Reads native inputs/results; no upstream implementation or table is copied.
 * MB83 adds source type/modes and records actual selected modes, including both
 * the statistics and final pass. The public driver still uses one codec CPU. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "src/enc/vp8i_enc.h"
#include "src/dsp/cpu.h"
extern VP8CPUInfo VP8GetCPUInfo;
extern int __real_VP8Decimate(VP8EncIterator*,VP8ModeScore*,VP8RDLevel);
static unsigned calls=0,profiles=0;
static uint8_t previous[1056];
static void fail(const char *s){fprintf(stderr,"%s\n",s);_Exit(1);}
static void bytes(const void *p,size_t n){if(fwrite(p,1,n,stdout)!=n)fail("Output failure.");}
static void word(uint32_t v){for(int p=0;p<4;p++)if(fputc((v>>(8*p))&255,stdout)==EOF)fail("Output failure.");}
static void wide(int64_t v){word((uint32_t)v);word((uint32_t)((uint64_t)v>>32));}
static void pixels(const uint8_t *p){for(int y=0;y<16;y++)bytes(p+Y_OFF_ENC+y*BPS,16);for(int y=0;y<8;y++)bytes(p+U_OFF_ENC+y*BPS,16);}
static void coefficients(const int16_t *p,int count){for(int i=0;i<count;i++){unsigned v=(uint16_t)p[i];if(fputc(v&255,stdout)==EOF||fputc(v>>8,stdout)==EOF)fail("Output failure.");}}
static void parameters(const VP8Encoder *enc){
 bytes("PAR8",4);word((unsigned)enc->mb_w);word((unsigned)enc->mb_h);word((unsigned)enc->max_i4_header_bits);
 for(int s=0;s<4;s++){
  const VP8SegmentInfo *p=enc->dqm+s;word((unsigned)p->quant);word((unsigned)p->fstrength);
  word((unsigned)p->lambda_i4);word((unsigned)p->lambda_i16);word((unsigned)p->lambda_uv);word((unsigned)p->lambda_mode);word((unsigned)p->tlambda);
  word((unsigned)p->min_disto);wide(p->i4_penalty);
  for(int plane=0;plane<3;plane++){
   const VP8Matrix *m=plane==0?&p->y1:plane==1?&p->y2:&p->uv;
   for(int i=0;i<16;i++)word(m->q[i]);
   for(int i=0;i<16;i++)word(m->iq[i]);
   for(int i=0;i<16;i++)word(m->bias[i]);
   for(int i=0;i<16;i++)word(m->zthresh[i]);
   for(int i=0;i<16;i++)word((unsigned)m->sharpen[i]);
  }
 }
}
int __wrap_VP8Decimate(VP8EncIterator *it,VP8ModeScore *rd,VP8RDLevel level){
 if(!calls)parameters(it->enc);
 if(!profiles||memcmp(previous,it->enc->proba.coeffs,1056)){
  bytes("PRB8",4);word(calls);bytes(it->enc->proba.coeffs,1056);memcpy(previous,it->enc->proba.coeffs,1056);profiles++;
 }
 bytes("BEF8",4);word(calls);word((unsigned)it->x);word((unsigned)it->y);word((unsigned)level);word(it->mb->segment);word(it->mb->type);word(it->preds[0]);word(it->mb->uv_mode);
 pixels(it->yuv_in);bytes(it->y_left-1,17);bytes(it->u_left-1,9);bytes(it->v_left-1,9);bytes(it->y_top,16);bytes(it->uv_top,16);
 for(int x=0;x<4;x++)if(fputc(it->x+1<it->enc->mb_w?it->y_top[16+x]:it->y_top[15],stdout)==EOF)fail("Output failure.");
 for(int x=0;x<4;x++)if(fputc(it->preds[-it->enc->preds_w+x],stdout)==EOF)fail("Output failure.");
 for(int y=0;y<4;y++)if(fputc(it->preds[y*it->enc->preds_w-1],stdout)==EOF)fail("Output failure.");
 word(it->nz[0]);word(it->nz[-1]);word((unsigned)it->left_nz[8]);word(it->top_derr!=NULL);
 if(it->top_derr){bytes(it->top_derr[it->x],4);bytes(it->left_derr,4);}
 const int skipped=__real_VP8Decimate(it,rd,level);
 bytes("AFT8",4);word(calls);word((unsigned)skipped);word(it->mb->type);word(it->mb->type==1?it->preds[0]:0);word(it->mb->uv_mode);word(rd->nz);
 wide(rd->D);wide(rd->SD);wide(rd->H);wide(rd->R);wide(rd->score);
 if(it->mb->type==0)bytes(rd->modes_i4,16);else{const uint8_t zero[16]={0};bytes(zero,16);}
 if(it->mb->type==1)coefficients(rd->y_dc_levels,16);else{const int16_t zero[16]={0};coefficients(zero,16);}
 coefficients(rd->y_ac_levels[0],256);coefficients(rd->uv_levels[0],128);pixels(it->yuv_out);
 if(it->top_derr){bytes(rd->derr,6);bytes(it->top_derr[it->x],4);bytes(it->left_derr,4);}
 calls++;return skipped;
}
int main(int argc,char **argv){
 if(argc!=2 && argc!=3)fail("Usage: distortion-decision-observer METHOD0..2 [scalar]");
 char *end;const long method=strtol(argv[1],&end,10);if(!*argv[1]||*end||method<0||method>2)fail("Invalid method.");
 const int scalar=argc==3&&!strcmp(argv[2],"scalar");if(argc==3&&!scalar)fail("Invalid CPU selection.");
 const int width=513,height=259;const size_t length=(size_t)width*height*4;
 uint8_t *source=malloc(length);if(!source)fail("Allocation failure.");
 if(fread(source,1,length,stdin)!=length||fgetc(stdin)!=EOF)fail("Invalid source size.");
 if(scalar)VP8GetCPUInfo=NULL;
 WebPConfig config;WebPPicture picture;WebPMemoryWriter writer;
 if(!WebPConfigInit(&config)||!WebPPictureInit(&picture))fail("Initialization failure.");
 config.quality=75;config.method=(int)method;config.thread_level=0;config.lossless=0;
 if(!WebPValidateConfig(&config))fail("Invalid configuration.");
 WebPMemoryWriterInit(&writer);picture.use_argb=1;picture.width=width;picture.height=height;picture.writer=WebPMemoryWrite;picture.custom_ptr=&writer;
 if(!WebPPictureImportRGBA(&picture,source,width*4))fail("RGBA import failed.");
 bytes("MB83",4);
 if(!WebPEncode(&config,&picture))fail("Encoding failure.");
 if(calls!=(method==0?701u:1122u)||writer.size>1000000)fail("Unexpected decision or packet count.");
 bytes("PKT8",4);word(calls);word(profiles);word((unsigned)writer.size);bytes(writer.mem,writer.size);
 WebPMemoryWriterClear(&writer);WebPPictureFree(&picture);free(source);return 0;
}
