/* Original development-only observer of supplied boolean symbols. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "src/utils/bit_writer_utils.h"
static void fail(const char *s){fprintf(stderr,"%s\n",s);_Exit(1);}
static uint32_t read32(void){uint8_t b[4];if(fread(b,1,4,stdin)!=4)fail("Truncated input.");return(uint32_t)b[0]|(uint32_t)b[1]<<8|(uint32_t)b[2]<<16|(uint32_t)b[3]<<24;}
static void word(uint32_t v){for(int p=0;p<4;p++)if(fputc((v>>(8*p))&255,stdout)==EOF)fail("Output failure.");}
int main(void){
 char tag[4];if(fread(tag,1,4,stdin)!=4||memcmp(tag,"BW79",4))fail("Unknown stream.");
 uint32_t cases=read32();if(!cases||cases>16)fail("Invalid case count.");word(cases);
 for(uint32_t c=0;c<cases;c++){
  uint32_t count=read32();if(count>8192)fail("Invalid symbol count.");
  VP8BitWriter writer;if(!VP8BitWriterInit(&writer,0))fail("Initialization failure.");
  for(uint32_t p=0;p<count;p++){int bit=fgetc(stdin),prob=fgetc(stdin);if(bit<0||bit>1||prob<0)fail("Invalid symbol.");VP8PutBit(&writer,bit,prob);}
  VP8BitWriterFinish(&writer);if(writer.error||writer.pos>100000)fail("Encoding failure.");word((uint32_t)writer.pos);
  if(fwrite(writer.buf,1,writer.pos,stdout)!=writer.pos) { fail("Output failure."); }
  VP8BitWriterWipeOut(&writer);
 }
 if(fgetc(stdin)!=EOF) { fail("Trailing input."); }
 return 0;
}
