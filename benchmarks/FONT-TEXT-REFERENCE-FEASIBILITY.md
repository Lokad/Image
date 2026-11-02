# Font and text reference feasibility

The scoped FreeType reference, complete public text pipeline and 16-bit visual
gates were approved on 2026-11-02. [Current measurements](TEXT-DRAWING-CURRENT.md)
retain their narrow valid-face/integration scope. Original raw records preserve
their pre-approval state; that historical flag is not a current blocker.

The [complete public text adapter](PUBLIC-TEXT-LAYOUT-REFERENCE.md) now supplies
the layout/fitting boundary identified below, with four existing controls and
explicit timing/word-raster gates. The copied-font evidence remains valid.

The existing DejaVu Sans 2.37 regular face has an equivalent **copy/open/retain/
release** reference candidate using FreeType 2.13.2, FFmpeg's text backend.
Each native result owns its byte copy and memory face, survives caller-input
mutation/release and sibling disposal, then releases both. Managed Windows/Linux
results agree exactly with native face metrics and seven existing Unicode
mapping/advance probes. No font, style or script grid is added.

Each managed font retains a **757,332-byte admission charge**; the shared root
peaks at **1,514,664 bytes** for two fonts. A third load refuses terminally, leaving
both results valid. Disposal releases capacity, subsequent loads succeed,
cancellation cleans up and the caller's root remains borrowed. Final charge is
zero. These are retained-capacity figures, not measured allocations per operation.

A tooling-only FreeType exception for this one valid frozen font is approved.
The complete operation includes the input copy,
face initialization and disposal, excluding resident library/input setup, I/O and
verification equally. The parsers' eager/lazy validation policies differ; this
does not establish general parser equivalence. No timings or native allocation
comparison have been collected.

The pinned Fonts package's positioned-layout types are internal. Its public
Drawing API does expose prepared glyph paths. On the existing 18 px LatinKerning
control, `TextBuilder.GenerateGlyphs` followed by `Fill` reproduces normal
`DrawText` at RGBA8 exactly on Windows/Linux. At RGBA16, RGB is exact and six
alpha samples differ by one; alpha RMSE on the nonzero union is **0.138013**.
Word canvas storage is preserved. This is reference-to-reference rendering
feasibility, with no new performance exception or broader compatibility claim.

Path collection bounds start at (0,0), while the byte raster's ink spans
(8,11)–(56,25), with exclusive right/bottom edges. They cannot substitute for
layout, ink or fitting measurements. The remaining text reference must preserve
glyph/cluster metadata, wrapping, line boxes, actual bounds and the complete
fitting search; prepared-path pixel agreement alone does not qualify those APIs.

Production and the package are unchanged. Reuse 5,378 checks/OS, 948 live
comparisons and the same actual package/three examples. All 282 required paths
remain; nine audited controls still lack warm timings. The current summary's
110 comparisons and three tables are unchanged.

[Evidence](font-text-reference-feasibility-evidence.json). Retained
font-text-reference-feasibility.tar.xz: **1,440,696 bytes / 42 members**,
SHA256 **2370bef494d19bdf180d7c35fa802745acdd1bf767f8acb5424faa015597a849**,
mirrored and member-verified. It contains original consumers/native adapter,
actual DLLs/native library, both-platform records, rasters and member hashes.
Use the existing [font provenance](../tools/TextReference/font-provenance.json),
archived frf64-run-vm.py and retained Frf64/Ptr65 consumers. The
[packed-fill round](WARM-RECTANGLE-FILL-PACKED.md) retains unchanged full/package proof.
