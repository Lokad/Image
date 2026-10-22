/* Original bounded public-jpeglib adapter for the existing grayscale control.
 * Same normalization/quantizers/ISLOW/optimal coding; the sole new option is
 * an explicit non-row MCU restart interval3. No upstream implementation copied. */
#define _GNU_SOURCE
#include <math.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <jpeglib.h>

extern int jsimd_can_fdct_islow(void);
static void fail(const char *message) { fprintf(stderr,"%s\n",message); exit(1); }
static void write_file(const char *path,const uint8_t *data,size_t bytes)
{
    FILE *file=fopen(path,"wb"); if(!file) fail("Open output failed.");
    if(fwrite(data,1,bytes,file)!=bytes || fclose(file)) fail("Write output failed.");
}
int main(int argc,char **argv)
{
    if(argc!=4) fail("Usage: libjpeg-gray-restart-stage RGBA8 GRAY8_OUTPUT JPEG_OUTPUT");
    cpu_set_t affinity;
    if(sched_getaffinity(0,sizeof(affinity),&affinity) || CPU_COUNT(&affinity)!=1) fail("One CPU required.");
    if(!jsimd_can_fdct_islow()) fail("SIMD ISLOW support is required.");
    const size_t pixels=513U*259U, source_bytes=pixels*4;
    uint8_t *rgba=malloc(source_bytes), *gray=malloc(pixels);
    if(!rgba || !gray) fail("Allocate bounded source/workspace failed.");
    FILE *input=fopen(argv[1],"rb"); if(!input) fail("Open source failed.");
    if(fread(rgba,1,source_bytes,input)!=source_bytes || fgetc(input)!=EOF || ferror(input) || fclose(input)) fail("Source size/read differs.");
    for(size_t i=0;i<pixels;i++) {
        unsigned int a=rgba[4*i+3];
        unsigned int r=(rgba[4*i]*a+32*(255-a)+127)/255;
        unsigned int g=(rgba[4*i+1]*a+64*(255-a)+127)/255;
        unsigned int b=(rgba[4*i+2]*a+96*(255-a)+127)/255;
        gray[i]=(uint8_t)floor((0.299*r+0.587*g)+0.114*b+0.5);
    }
    struct jpeg_compress_struct codec;
    struct jpeg_error_mgr errors;
    codec.err=jpeg_std_error(&errors); jpeg_create_compress(&codec);
    codec.image_width=513; codec.image_height=259; codec.input_components=1; codec.in_color_space=JCS_GRAYSCALE;
    jpeg_set_defaults(&codec);
    unsigned int quantizers[64];
    for(unsigned int v=0;v<8;v++) for(unsigned int u=0;u<8;u++) {
        unsigned int q=((1+2*(u+v)+u*v)*62+50)/100;
        quantizers[8*v+u]=q<1?1:q>255?255:q;
    }
    jpeg_add_quant_table(&codec,0,quantizers,100,TRUE);
    codec.optimize_coding=TRUE; codec.dct_method=JDCT_ISLOW; codec.restart_interval=3;
    unsigned char *packet=NULL; unsigned long bytes=0;
    jpeg_mem_dest(&codec,&packet,&bytes); jpeg_start_compress(&codec,TRUE);
    while(codec.next_scanline<codec.image_height) {
        JSAMPROW row=gray+(size_t)codec.next_scanline*513;
        if(jpeg_write_scanlines(&codec,&row,1)!=1) fail("Incomplete row consumption.");
    }
    jpeg_finish_compress(&codec);
    if(!packet || !bytes) fail("No complete packet.");
    write_file(argv[2],gray,pixels); write_file(argv[3],packet,bytes);
    printf("{\"project\":\"Lokad.Image\",\"round\":\"E13\",\"sourceDepth\":8,\"width\":513,\"height\":259,\"components\":1,\"quality\":69,\"ownedQuantizers\":true,\"isLowSimdAvailable\":true,\"affinityCount\":1,\"codecThreads\":1,\"conversionThreads\":1,\"optimizeHuffman\":true,\"restartInterval\":3,\"sourceBytes\":%zu,\"normalizedGrayBytes\":%zu,\"packetBytes\":%lu,\"timingRuns\":0}\n",source_bytes,pixels,bytes);
    jpeg_destroy_compress(&codec); free(packet); free(gray); free(rgba);
    return 0;
}
