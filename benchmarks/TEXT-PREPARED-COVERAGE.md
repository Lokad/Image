# Prepared exact text coverage

[Finite horizontal clipping](TEXT-GLYPH-ROW-BOUNDS.md) owns current
ordinary drawing/complete integration costs. Unchanged prepared execution and
font-loading evidence retain their scoped validity; historical observations follow.

Current complete one-shot/eight-draw costs are in [packed signed font bounds](FONT-PACKED-BOUNDS.md). The four drawing costs, retained coverage design and frame/owner evidence here remain source-valid; font construction is outside those drawing clocks.

`TextRenderPlan` prepares exact 16-bit coverage once, retaining compact row runs
in glyph order. Temporary geometry/events/crossings/working rows are released
before publication. Each draw reads fresh caller pixels and colors through the
unchanged blending arithmetic. Shared scan conversion selects immediate drawing
or row capture once per row; there is no duplicated raster engine, new API,
general cache, pool, independent budget or numerical relaxation.

| Complete public boundary / ImageSharp integration | Lokad / reference µs | Conservative upper / status | Managed GC B/op |
| --- | ---: | --- | ---: |
| Ordinary8 | 52.198 / 45.090 | 1.298 / uncertain | 83,136 |
| Ordinary16 | 55.581 / 46.369 | 1.326 / gap | 83,136 |
| Prepared8 | 2.932 / 36.369 | 0.087 / pass | 56 |
| Prepared16 | 5.298 / 37.684 | 0.152 / pass | 56 |
| OneShot | 1158.050 / 6179.061 | 0.213 / pass | 306,090 |
| PrepareDraw8 | 1254.761 / 6835.493 | 0.203 / pass | 315,808 |

The prepared16 three-pair screen is independently confirmed; all four current
drawing controls retain twelve actuals, three native brackets, original reset/
per-operation clocks and exact/layout/visual/owner gates. The two prepared paths
now pass and stop. Ordinary8 remains uncertain, ordinary16 a gap. This does not
establish an overall text slowdown or a uniform gain from historical subtraction.

Reference: ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12, never FFmpeg
or a bare shaping engine. Managed preparation retains coverage; reference preparation
retains positioned paths and allocates raster working storage while drawing.
Preparation must be included when comparing complete repeated-use scenarios.

The original ConstrainedFit16 integration includes all 30 fitting trials, three
reference public layout calls per trial and four for winner reconstruction,
fresh owned explicitly cleared canvases and disposal. Preparation-only is
1118.478 / 6174.196 µs
with prepared-owner disposal outside only that diagnostic. The directly timed
eight-draw batch amortizes to **156.845 /
854.437 µs/draw**, including preparation.
Its allocation is per whole batch. Reference complete one-shot/eight-draw GC
is 9,256,953 / 9,494,633 B. Font validation eagerness and GC-driven reference
FontCollection/FontFamily release remain explicit differences.

| Frozen control | Retained admitted capacity B | Admitted preparation peak B |
| --- | ---: | ---: |
| NarrowLongWord | 6,896 → 8,256 | 7,952 → 17,232 |
| LatinKerning | 48,984 → 2,112 | 58,264 → 58,264 |
| ArabicMarksDigits | 310,448 → 6,208 | 318,160 → 318,736 |
| ConstrainedFit | 48,480 → 4,160 | 56,736 → 56,736 |

This is not a universal memory reduction: narrow wrapping retains more and
constructs at a higher peak. Geometry and mask replacement overlap is admitted
before allocation; refusal is terminal and cleanup releases unused reservations.
Public prepared drawing stays 56 B/op; timing adapters add 24 B. Ordinary Latin
allocation stays 83,136 B. Complete-fit preparation allocates 314,465 B and has
governed peak 822,460 B. Approximate full-GC retained heap deltas are 4,336/22,504 B,
excluding idle pools; these are not allocation counts or physical/native/JIT peaks.

All eight frozen full-word frames pass on Windows/Linux, normal/AVX-disabled/
scalar. Two new public checks cover tight wrapped overlapping glyphs, fractional
clipping, five alpha values including 1/257/32769 and repeated compositing on
fresh nontransparent canvases. Every-growth refusal/cancellation, cleanup,
recovery, retained-root lifetime and font/layout disposal checks pass. Existing
Arabic ordinary 428.838 versus 426.954 µs has overlapping intervals and unchanged
590,552 B allocation: no confirmed generalization gain/regression or reference claim.

Each OS passes **6,730 full/live checks**, **266 internal/163 public** focused
normal/AVX-disabled/scalar coverage and the **SAME actual Release-package consumer /
all three examples**. One core/embedded/public build per OS, zero helper/harness/
native/fixture builds and one pack. The timed Linux DLL is never rebuilt.
Unchanged font loading/layout, codecs and image editing/resizing evidence is reused.

[Evidence](text-prepared-coverage-evidence.json) pins every boundary, memory cost,
source/owner proof and actual package. Raw `text-prepared-coverage-qualified.tar.xz` (4,719,928 B /
195 members), SHA256 `e2587bd8aa2ea210a4b48e4b9c2cd821c861f70d42862a85a3a6edd5b224bb1b`, is mirrored and member-verified
with retained baseline/reference dependencies. Private package `7b89e4563dbe12856ee35876d317928f6dfd873a0b4ec5dfc37690fb27a626e0`
contains the timed DLL. The full 289-path objective remains open; publication
is postponed and published packages stay immutable.
