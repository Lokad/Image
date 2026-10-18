/* Original development-only unhinted grayscale render check using public APIs.
 * Not a production input or an approved performance baseline. */
#include <stdio.h>
#include <stdlib.h>
#include <ft2build.h>
#include FT_FREETYPE_H

int main(int argc, char **argv)
{
    if (argc != 7) return 2;
    FT_Library library = NULL;
    FT_Face face = NULL;
    FT_Error error = FT_Init_FreeType(&library);
    if (!error) error = FT_New_Face(library, argv[1], 0, &face);
    if (!error) error = FT_Set_Pixel_Sizes(face, 0, (FT_UInt)strtoul(argv[3], NULL, 10));
    FT_Vector shift = {strtol(argv[4], NULL, 10), -strtol(argv[5], NULL, 10)};
    if (!error) FT_Set_Transform(face, NULL, &shift);
    FT_UInt index = !error ? FT_Get_Char_Index(face, strtoul(argv[2], NULL, 10)) : 0;
    if (!error) error = FT_Load_Glyph(face, index, FT_LOAD_NO_HINTING | FT_LOAD_NO_BITMAP | FT_LOAD_NO_AUTOHINT);
    if (!error) error = FT_Render_Glyph(face->glyph, FT_RENDER_MODE_NORMAL);
    FILE *output = NULL;
    if (!error) {
        FT_Bitmap *bitmap = &face->glyph->bitmap;
        if (bitmap->pixel_mode != FT_PIXEL_MODE_GRAY || bitmap->num_grays != 256 || bitmap->pitch < (int)bitmap->width) error = 1;
        else if (!(output = fopen(argv[6], "wb"))) error = 1;
        else {
            for (unsigned int row = 0; row < bitmap->rows; row++)
                if (fwrite(bitmap->buffer + row * bitmap->pitch, 1, bitmap->width, output) != bitmap->width) { error = 1; break; }
            if (!error) printf("{\"glyphIndex\":%u,\"width\":%u,\"height\":%u,\"left\":%d,\"top\":%d}\n", index, bitmap->width, bitmap->rows, face->glyph->bitmap_left, face->glyph->bitmap_top);
        }
    }
    if (output && fclose(output)) error = 1;
    if (face) FT_Done_Face(face);
    if (library) FT_Done_FreeType(library);
    if (error) fprintf(stderr, "FreeType render error %d\n", error);
    return error ? 1 : 0;
}
