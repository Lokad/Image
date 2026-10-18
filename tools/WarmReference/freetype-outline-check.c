/* Original development-only geometry check; no production build input.
 * Uses FreeType's public API. No upstream implementation is included. */
#include <stdio.h>
#include <stdlib.h>
#include <ft2build.h>
#include FT_FREETYPE_H
#include FT_BBOX_H

int main(int argc, char **argv)
{
    if (argc != 3) return 2;
    FT_Library library = NULL;
    FT_Face face = NULL;
    FT_Error error = FT_Init_FreeType(&library);
    if (!error) error = FT_New_Face(library, argv[1], 0, &face);
    FT_ULong scalar = strtoul(argv[2], NULL, 10);
    FT_UInt index = !error ? FT_Get_Char_Index(face, scalar) : 0;
    if (!error) error = FT_Load_Glyph(face, index, FT_LOAD_NO_SCALE | FT_LOAD_NO_HINTING | FT_LOAD_NO_BITMAP);
    FT_BBox box = {0, 0, 0, 0};
    if (!error) error = FT_Outline_Get_BBox(&face->glyph->outline, &box);
    if (!error)
        printf("{\"glyphIndex\":%u,\"advance\":%ld,\"bbox\":[%ld,%ld,%ld,%ld],\"points\":%d,\"contours\":%d}\n",
            index, face->glyph->metrics.horiAdvance, box.xMin, box.yMin, box.xMax, box.yMax,
            face->glyph->outline.n_points, face->glyph->outline.n_contours);
    if (face) FT_Done_Face(face);
    if (library) FT_Done_FreeType(library);
    if (error) fprintf(stderr, "FreeType error %d\n", error);
    return error ? 1 : 0;
}
