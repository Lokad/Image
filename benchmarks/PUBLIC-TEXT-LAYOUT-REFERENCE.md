# Public text layout reference

A retained reference adapter now covers the four existing named controls using
public Fonts/Drawing APIs. Windows and Linux observations agree exactly. Glyph
IDs, source clusters, line counts and fitting decisions match Lokad.Image;
position error is below 0.000001 px and logical/ink/combined bounds error below
0.000008 px. No private layout API is invoked.

| Existing control | Lines | Maximum position/bounds error, px | Byte alpha RMSE | Word alpha RMSE, proposed |
|---|---:|---:|---:|---:|
| NarrowLongWord, Logical | 5 | 0 | 4.471 | 4.406 |
| LatinKerning, OpenType | 1 | 0 | 4.464 | 4.370 |
| ArabicMarksDigits, OpenType | 1 | 0 | 9.252 | 9.211 |
| ConstrainedFit, OpenType | 1 | 0.00000763 | 6.996 | 6.920 |

Each proposed complete layout operation includes four public calls: renderer
callbacks retaining paths and glyph observations, character advances/source
clusters, line count and advance measurement. Actual nonempty glyph geometry
supplies ink bounds; collection bounds and synthetic space rectangles are
excluded. Retained metadata and paths are released together. The adapter uses
value glyph records and avoids redundant advance-array copies/empty wrappers.

Fitting includes all 30 descending trials selecting 16.75 px, followed by complete
retained-winner construction. The existing public fitting helper's calls and
winner construction belong inside the operation. No rejected trial or winner
reconstruction is omitted; search allocates no raster.

Prepared-path drawing includes canvas mutation and translation, excluding the
resident font/layout and identical reset work on both subjects. All byte raster
controls pass the existing union-alpha gates: mean ≤8, RMSE ≤16 on 0..255,
coverage error ≤2% and raster edges ≤1 px. Word results retain full 16-bit samples;
applying those same visual gates after normalizing error units is **proposed and
unapproved**. Both depths preserve borrowed canvas storage. All managed admission
charges return to the font-only baseline between controls, then zero.

This public prepared-path renderer is not identical to normal `DrawText` on the
wrapped control: maximum alpha difference is 32/255 or 8192/65535. Both routes
pass the corresponding existing/proposed visual numbers against Lokad.Image.
The other three byte routes are exact; their word routes differ by at most one
alpha sample. These facts do not establish a bare font-engine or FFmpeg timing.

The proposed six-control ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 timing exception
and word raster gates await approval. No timings, allocation-per-operation
figures, new font inventory or performance controls are added. Production and
the actual package are unchanged; reuse full 5,378-check/OS and package proofs.
All 282 required paths remain; nine audited controls still lack warm timings.

[Evidence](public-text-layout-reference-evidence.json). Retained
public-text-layout-reference.tar.xz: **1,118,684 bytes / 50 members**,
SHA256 **f441edfbe2abfb8ce66841b083a3eff6131662409f977402adc9a9d8641d9be3**,
mirrored and member-verified. It contains original adapter/consumer, actual DLLs,
both-platform records, all rasters and member hashes. Run the retained Tlp66
consumer with the existing pinned public font; the archived source links the
unchanged FittingReference helper. Earlier [font/text feasibility](FONT-TEXT-REFERENCE-FEASIBILITY.md)
and [packed-fill qualification](WARM-RECTANGLE-FILL-PACKED.md) retain dependencies.
