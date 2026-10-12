/* Original development-only canonical coefficient visitor using public libjpeg
 * APIs. JPEG coefficient/quantization layouts follow T.81; no upstream code. */
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <jpeglib.h>

static void fail(const char *message) { fprintf(stderr,"%s\n",message); exit(1); }
static void word(FILE *output, uint32_t value, int bytes)
{
    for (int i=0;i<bytes;i++) if (fputc((int)(value>>(i*8)&255),output)==EOF) fail("Coefficient dump write failed.");
}
int main(int argc, char **argv)
{
    if (argc!=3) fail("Usage: jpeg-coefficients INPUT OUTPUT");
    FILE *input=fopen(argv[1],"rb"); if (!input) fail("Open JPEG failed.");
    if (fseek(input,0,SEEK_END)) fail("JPEG seek failed.");
    long size=ftell(input); if (size<4 || size>32L*1024*1024 || fseek(input,0,SEEK_SET)) fail("Invalid bounded JPEG size.");
    struct jpeg_decompress_struct image;
    struct jpeg_error_mgr errors;
    image.err=jpeg_std_error(&errors); jpeg_create_decompress(&image);
    jpeg_stdio_src(&image,input);
    if (jpeg_read_header(&image,TRUE)!=JPEG_HEADER_OK || image.image_width!=513 || image.image_height!=259
        || (image.num_components!=1 && image.num_components!=3) || image.data_precision!=8) fail("Expected fixed original grayscale, 4:2:0 or full-resolution JPEG dimensions/depth.");
    int full_resolution=image.num_components==3 && image.comp_info[0].h_samp_factor==1
        && (image.comp_info[0].v_samp_factor==1 || image.comp_info[0].v_samp_factor==2);
    for (int i=1;full_resolution && i<3;i++)
        full_resolution=image.comp_info[i].h_samp_factor==1 && image.comp_info[i].v_samp_factor==image.comp_info[0].v_samp_factor;
    jvirt_barray_ptr *arrays=jpeg_read_coefficients(&image);
    FILE *output=fopen(argv[2],"wb"); if (!output) fail("Open coefficient dump failed.");
    if (fwrite("LJCF0001",1,8,output)!=8) fail("Coefficient dump header failed.");
    word(output,image.image_width,4); word(output,image.image_height,4); word(output,(uint32_t)image.num_components,4);
    for (int i=0;i<image.num_components;i++) {
        jpeg_component_info *component=&image.comp_info[i];
        int factor=(image.num_components==3 && !full_resolution && i==0)?2:1;
        int vertical=full_resolution?image.comp_info[0].v_samp_factor:factor;
        JDIMENSION columns=(i==0 || full_resolution)?65:33, rows=(i==0 || full_resolution)?33:17;
        if (component->h_samp_factor!=factor || component->v_samp_factor!=vertical
            || component->width_in_blocks!=columns || component->height_in_blocks!=rows) fail("Coefficient geometry differs.");
        JQUANT_TBL *quantization=image.quant_tbl_ptrs[component->quant_tbl_no]; if (!quantization) fail("Quantization missing.");
        word(output,(uint32_t)component->component_id,4); word(output,(uint32_t)component->h_samp_factor,4);
        word(output,(uint32_t)component->v_samp_factor,4); word(output,component->width_in_blocks,4); word(output,component->height_in_blocks,4);
        for (int k=0;k<DCTSIZE2;k++) word(output,quantization->quantval[k],2);
        for (JDIMENSION y=0;y<component->height_in_blocks;y++) {
            JBLOCKARRAY row=(*image.mem->access_virt_barray)((j_common_ptr)&image,arrays[i],y,1,FALSE);
            for (JDIMENSION x=0;x<component->width_in_blocks;x++)
                for (int k=0;k<DCTSIZE2;k++) word(output,(uint16_t)row[0][x][k],2);
        }
    }
    if (!jpeg_finish_decompress(&image)) fail("JPEG coefficient read incomplete.");
    jpeg_destroy_decompress(&image);
    if (fclose(input) || fclose(output)) fail("Coefficient dump close failed.");
    return 0;
}
