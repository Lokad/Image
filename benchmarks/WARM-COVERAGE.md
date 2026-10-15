# Material warm coverage

The [benchmark summary](../docs/benchmark.md) reports confirmed fixed workloads,
not complete coverage of the library contract. Keep unknown cases open and add
one representative only when it exercises distinct substantial work. Reuse the
two existing original 513x259 RGBA8/16 rasters; documented projections for another
pixel format do not introduce another original image. Normally run one pair per
round, at most three, with correctness established before timing.

| Primitive group | Retained coverage | Remaining material work |
| --- | --- | --- |
| PNG decode | RGBA8 all five filters and stored Deflate/None; RGBA16 None/Paeth/Adam7; RGB8/16, gray8/16, gray-alpha8/16 and opaque gray1/2/4 and palette1/2/4/8 None; palette8 two-entry tRNS, gray8/16 tRNS and RGB8/16 tRNS confirmed | Other material depths/layouts, shorter palettes or distinct public boundaries |
| PNG encode | RGBA8/16, RGB16, gray16 and gray-alpha16 level6/None; byte RGB/gray/gray-alpha Optimal managed9/native6 with unchanged5% size gate confirmed; RGBA8 NoCompression/None confirmed | Other materially different compression work/public boundaries |
| JPEG decode | Baseline/progressive gray, 4:2:0, 4:2:2, 4:4:4 | Distinct restart handling or unusual supported layouts if they dominate work |
| JPEG encode | Adaptive RGBA8 4:4:4 confirmed 1.772x / upper 1.818x, 4:2:2 confirmed 1.821x / upper 1.862x and 4:2:0 confirmed 1.943x / upper 1.988x; adaptive RGBA16 4:4:4 confirmed 1.757x / upper 1.790x, 4:2:2 confirmed 1.874x / upper 1.884x and 4:2:0 confirmed 1.888x / upper 1.931x; forward-only RGBA8 4:2:0 confirmed 1.916x / upper 1.936x and RGBA16 4:2:0 confirmed 1.948x / upper 1.954x; adaptive RGBA8 row restarts confirmed 1.958x / upper 1.973x | Other material fallback layouts, grayscale and distinct public boundaries |
| WebP | VP8 with alpha decode/encode; opaque VP8 decode confirmed 1.803x / upper 1.808x and alpha VP8 confirmed 1.803x / upper 1.819x; RGBA16 VP8/VP8L encode with full normalization; VP8L native and managed inputs; exact VP8L backend encode | Other material opaque/alpha alternatives and distinct public boundaries |
| Resize | Nearest/bilinear downsample and enlargement at both depths; bicubic downsample, enlargement on both axes, both mixed-axis directions and 128×64 / 64×32 wider supports at both depths; horizontal reduction/enlargement and vertical reduction at both depths; vertical enlargement, 32×16 and 16×8 adaptive support at both depths; RGBA8/16 256×129, 128×64, 64×32, 32×16 and 16×8 reduction, plus RGBA8/16 both-axis enlargement and RGBA8/16 width enlargement/height reduction plus RGBA8/16 width reduction/height enlargement and RGBA8/16 8×4 beyond 256 taps plus RGBA8/16 4×2 packed support beyond 304 taps and RGBA8/16 2×2 generated support beyond 544 taps plus RGBA8/16 2×9 bounded row blocks and RGBA8/16 4×9 packed row blocks plus RGBA8/16 8×17 full-coefficient row blocks and RGBA8/16 vertical reduction with unchanged width plus RGBA8/16 horizontal reduction with unchanged height and RGBA8/16 vertical enlargement with unchanged width plus RGBA8/16 horizontal enlargement with unchanged height, with input+output+4 KiB limit confirmed | Larger/unmatched supports and other materially distinct shapes |
| Exact edits | Both depths and every orientation/permutation, fixed shapes; RGBA8 horizontal flip with two simultaneously live outputs confirmed 1.447x / upper 1.484x per image | Other materially distinct retained/composed public boundaries |
| Public boundaries | Resident encoded span to owned output; complete single operations; fresh seekable non-exposed PNG8 Stream decode confirmed 1.907x / upper 1.916x and PNG16 confirmed 1.819x / upper 1.830x; PNG8 Stream/normal-orient/centered-crop/nearest-fit composition confirmed 1.891x / upper 1.906x | Other material fresh input/depth and composed boundaries |

Centered/clamped crop, AutoOrient and ResizeToFit route to the measured copy or
resize primitives after small geometry/metadata work. They need correctness
controls and one relevant composed boundary, not a duplicate matrix of timings.
Fresh Stream boundaries remain distinct from resident-span results. The retained-two
RGBA8 flip has its own complete two-owner boundary, normalized per image.

Tight-memory reduction is now confirmed at both depths through the existing
16×8 workload. The [word streaming round](WARM-BICUBIC-TIGHT-16X8-RGBA16.md)
confirms 0.964x / upper 0.984x at 1,145 B/op, and the
[byte streaming round](WARM-BICUBIC-TIGHT-16X8-RGBA8.md) confirms 1.084x /
upper 1.103x at 633 B/op. Both preserve exact pixels within the 4-KiB optional
workspace, with unchanged smaller-support regression means checked before
full qualification. Stop these fixed pairs. Other tight-memory directions,
public boundaries and final current-source coverage remain open.

The [tight-memory RGBA8 bicubic round](WARM-BICUBIC-TIGHT8-STRIPS.md) reduces a
20.990x gap by 91.1%, independently confirming 1.878x / upper 1.895x at
132,295 B/op. Six-column strips reuse bounded rows and compact decoded source
windows within admitted stack storage, adding no heap workspace. Every frozen
and rational managed sample remains exact; uncertain strips replay the original
operation after admission unwinds. A default-memory regression mean passes
before one full both-platform/same actual package qualification. Stop this pair;
other tight budgets/depths/supports and remaining material coverage stay open.

The [tight-memory RGBA16 round](WARM-BICUBIC-TIGHT16-STRIPS.md) reduces a 13.961x
gap by 89.7%, independently confirming 1.458x / upper 1.467x at 264,469 B/op.
Exact unsigned word products and localized original-order ordinary-alpha replay
extend the bounded strip strategy. Tiny alpha unwinds before exact scratch;
managed samples remain frozen/rational-exact. A shared-depth trial regresses byte
timing above two and is rejected without full qualification. The final separate
word specialization restores the byte kernel, which independently confirms
1.855x / upper 1.876x at unchanged 132,295 B/op on the same build. Both pass before
one full both-platform/same actual package qualification. Stop both pairs;
other tight supports/directions and remaining material coverage stay open.

The [wider-support tight-memory byte round](WARM-BICUBIC-TIGHT-WIDE8-STRIPS.md)
reduces a 28.316x gap by 94.0%, independently confirming 1.680x / upper 1.691x
at 32,889 B/op. Bounded three-column/twenty-row reuse, checked wide stores and
fixed strip indexing add no heap workspace and preserve every frozen/rational
pixel. Initial upper 2.014x remains unaccepted; a slower coefficient-vector
trial is restored. One profile guides the accepted hot-loop simplification.
The unchanged small-support regression mean passes before one full
both-platform/same actual package qualification. Stop this pair; wider word
timing and remaining tight/unmatched/current-source material coverage stay open.

The [wider-support tight-memory word round](WARM-BICUBIC-TIGHT-WIDE16-STRIPS.md)
reduces a corrected 23.963x gap by 94.0%, independently confirming 1.453x /
upper 1.470x at 65,658 B/op. A separate word strip specialization keeps exact
unsigned products, guarded paired/single packing and original-order ordinary-alpha
replay; tiny-alpha scratch follows complete frame unwind. Frozen/rational pixels
remain exact, with no heap workspace. An initial default-budget selection stays
unscored; corrected setup and receipts assert the tight limit. A byte regression
mean passes before one full both-platform/same actual package qualification.
Stop this pair; other tight directions and larger/unmatched/current-source
material coverage stay open.

The [tight-memory byte round beyond twenty taps](WARM-BICUBIC-TIGHT-64X32-RGBA8.md)
reduces a 24.951x gap by 92.7%, independently confirming 1.796x / upper 1.818x
at 8,313 B/op. Two-column/thirty-six-row reuse packs exact byte products and
relative offsets into UInt16, with full Int32 source coordinates and no pixel
quantization or heap workspace. Every frozen/rational sample stays exact.
Existing wider-support regression mean passes before one full both-platform/
same actual package qualification. Stop this pair; word timing and remaining
material tight/unmatched/current-source coverage stay open.

The first additional codec screen uses noninterlaced RGB16 PNG, filter None, preserving
the original RGBA16 RGB samples and setting alpha to 65535. This exercises the
generic sample-expansion path absent from the existing RGBA rows. Native timing
includes RGB48BE-to-owned-RGBA64LE conversion on one thread. The
[row-expansion round](WARM-PNG-RGB16-ROW.md) reduces its screened mean 36.5%,
from 3.167x to 2.014x; upper 2.187x remains open. Rank this gap against the next
eligible bicubic pair. The [4:2:0 luma round](WARM-JPEG420-LUMA-ROW.md) reduces
its screen from 4.206x to 3.462x, upper 3.731x. This larger measured gap now
was prioritized. The [chroma round](WARM-JPEG420-CHROMA-ROW.md) lowers its
independent mean to 1.978x; upper 2.022x remains open. Return to material unknown
paths before further small tuning. Two [framing experiments](WARM-PNG-LITERAL-REJECTED.md) were
reverted without a useful parity gain. No averages, easy-case
selection or tolerance changes establish the uniform warm target.

The [gray8 round](WARM-PNG-GRAY8-ROW.md) exposes a 7.921x gap and reduces it to
2.245x by direct expansion, with the same existing raster projection. The updated
profile places the remaining substantial cost in inflation/framing. Keep this
gap and the JPEG/RGB16 upper-bound gaps open; use one primitive per round.

A [compact literal classifier](WARM-PNG-LITERAL-CLASSIFIER-REJECTED.md) screen
lowered gray8 mean only 3.1%, to 2.181x (upper 2.522x), and was reverted.
The summary retains the accepted gray8 result. The
[gray-alpha8 row](WARM-PNG-GRAY-ALPHA8-ROW.md) then lowers its newly covered
3.358x screen by 59.0%, confirming 1.372x / upper 1.409x. Stop tuning that fixed
pair. The [RGB8 row](WARM-PNG-RGB8-ROW.md) then lowers a new 2.956x screen
by 51.9%, confirming 1.416x / upper 1.440x. Stop tuning this fixed pair. The
[gray16 row](WARM-PNG-GRAY16-ROW.md) lowers a newly covered 4.199x screen
by 56.8%, confirming 1.815x / upper 1.837x. Its failed first post-confirmation
guard remains unscored. Stop tuning this fixed pair. The
[gray-alpha16 row](WARM-PNG-GRAY-ALPHA16-ROW.md) reduces a new 3.114x screen
by 38.3%, confirming 1.776x / upper 1.785x. Stop tuning that fixed pair. The
[palette8 row](WARM-PNG-PALETTE8-ROW.md) lowers a new 7.895x screen by
65.7% to 2.715x, upper 2.954x; it remains open. Its profile places 33.7% in
framing validation and 20.1% in palette lookup. Prioritize this largest measured
matched gap with a substantial strategy; preserve checks and avoid repeating
the rejected small changes. The subsequent audit below limits the proposed
four-literal opportunity and records why a larger expansion gap was selected.

The [packed-gray row](WARM-PNG-GRAY1-ROW.md) lowers a new gray1 8.323x screen
by 88.3%, confirming 0.980x / upper 1.001x. The same safe kernel handles depths
1/2/4 correctly; gray2 remains unmeasured and gray4 remains above 2x. A palette literal-width audit
finds only 46.5% eligible four-eight-bit groups, a roughly 11% opportunity under
uniform per-literal cost before overhead. No four-literal grouping experiment was launched.
Stop tuning gray1. The [gray4 boundary](WARM-PNG-GRAY4.md) is exact but remains
2.630x / upper 2.775x. Its profile puts 26.8% in framing and 40.1% in BCL
inflation; prioritize a substantial data-flow opportunity, preserving contracts.
Do not repeat or micro-tune its 14.2% unpacking share first.

The [full token-loop experiment](WARM-PNG-TOKEN-RUN-REJECTED.md) kept literals
and short-code matches in one cache, but lowered palette8 only 4.2%, to 2.603x /
upper 2.815x. It was reverted; no long confirmation. Four new controls retain
exact work/failure precedence across budgets, outputs and truncations.

The [packed palette round](WARM-PNG-PALETTE4-ROW.md) then exposed a new 17.984x
gap and reduced its mean 84.5%, to 2.704x / upper 4.328x. No longer repeat was
launched. The new primitive handles palette1/2/4 correctly; only palette4 is
measured. Its profile places 38.4% in BCL inflation, 29.7% in framing and 13.5%
in packed expansion. Palette8 (2.715x), palette4 (2.704x) and gray4 (2.630x)
remain the largest measured mean gaps. Investigate substantial inflation/framing
data flow; another small row or token-dispatch gain cannot close them alone.

The [shared PNG storage round](WARM-PNG-DECODE-REUSE.md) reduces palette4 screen
time 41.4% and allocations 97.5%, independently confirming 1.572x / upper 1.617x.
Palette8 screen time falls 10.7% and allocations 85.6%, but 2.433x / upper 2.634x
remains open; no longer repeat. Every frozen PNG profile overwrites poisoned
storage, live owners stay independent, and partial failures never publish pixels.
The preceding split/whole-row BCL diagnostic improves only 7.2% and introduces no
read-pattern change. Next check gray4 with the shared policy, then rank the
remaining measured gaps. Other PNG rows retain their earlier measured sources;
held-owner and first-use allocation patterns remain distinct and open.

The [gray4 storage check](WARM-PNG-GRAY4-REUSE.md) independently confirms 1.504x /
upper 1.532x, with allocations down 97.5% to 13,544 B/op. Production/package are
unchanged; their complete validation is reused. Stop tuning gray4. Palette8 at
2.433x remains the largest reported mean gap; gray8/RGB16 retain older measured
sources and JPEG420 remains above the upper-bound target. Other material required
warm coverage remains open; no average or coverage narrowing completes the goal.

The [packed-palette table trial](WARM-PNG-PALETTE-WORD-REJECTED.md) retains an
unscored 2.189x mean: its post-guard fails aggregate I/O wait at 1.45%. It was
reverted without a long confirmation or repeated screen. The accepted palette8
result remains 2.433x. Two wide-palette memory/work controls remain; production
and the actual package are unchanged after exact restoration. Prioritize a
substantial remaining allocation or shared-work strategy over another table tweak.

The [compressed-input reuse round](WARM-PNG-INPUT-REUSE.md) lowers palette8
allocation 96.9%, from 89,564 to 2,760 B/op, with exact admitted capacities and
independent live leases. Both guards pass; 2.398x / upper 2.597x stays open and
no long repeat is launched. Its 5,974-sample profile places 37.8% in BCL inflation,
about 35% in framing and 19.7% in palette expansion. Allocation is now small;
prioritize substantive parsing/inflation before further small allocation tuning.
Other reported rows retain their measured sources; held/first-use owners and
material unknowns remain open. No average or narrowed scope completes parity.

The [buffered inflation round](WARM-PNG-INFLATE-BUFFER.md) reduces palette8 mean
14.9%, to 2.028x / upper 2.446x. Both quiet guards pass; no long confirmation.
8 KiB is admitted only for single-pass, noncancelable decodes with complete row
work preflight. Speculative compressed errors replay from delivered bytes with
original reads. Tight budgets, cancelable calls and Adam7 retain the original path.
One larger-buffer follow-up is not retained. Remaining profile cost is about41%
framing,30% BCL inflation and24% palette expansion; prioritize a substantial
remaining primitive. Other rows retain their earlier measured sources and all
required/material unknowns remain open.

The [current gray8 check](WARM-PNG-GRAY8-CURRENT.md) independently confirms 1.563x /
upper 1.580x and 9,904 B/op after shared PNG improvements. Both guards pass, all12
actuals retained. Production/package unchanged; full validation reused. Stop tuning
gray8. The following palette8 round closes its measured gap; RGB16 (earlier2.014x)
and JPEG420 upper-bound gap remain open, together with all required/material unknowns
and distinct public boundaries.

The [palette8 SIMD expansion](WARM-PNG-PALETTE-SIMD.md) reduces screen mean24.5% and
independently confirms1.526x /upper1.543x with10,976B/op. All12 actuals retained;
both guards pass. Full both-OS Release/live/package validation passes. Stop tuning
this fixed pair. Full palettes with absent/full alpha use safe register byte lookups;
partial palettes/alpha and other CPUs retain scalar expansion. Their performance
is distinct. The next RGB16 check measures its shared PNG improvements.

The [current RGB16 check](WARM-PNG-RGB16-CURRENT.md) independently confirms 1.819x /
upper 1.823x with15,024B/op (99.2% lower allocation). All12 actuals retained; both
guards pass. Production/package unchanged; full validation reused. Stop tuning
this fixed pair. JPEG420's earlier upper-bound gap remains open. Packed palette2
is a useful next material screen: its dense expansion still follows the scalar
path, distinct from the confirmed palette4/8 SIMD kernels. Reuse the original
RGBA8 raster and add one fixed projection, without a shape/quality/image matrix.

The [packed palette2 round](WARM-PNG-PALETTE2-ROW.md) measured a 3.388x scalar gap
and targets one dense expansion primitive. Safe sixteen-pixel SSE2/SSSE3 expansion
reduces mean 76.7%; independent confirmation is 0.774x / upper 0.789x and 9,208 B/op
unchanged. All 12 actuals retained; both guards pass. Full both-OS Release/live and
same-package/all-three-example checks pass. Stop tuning this fixed pair. Partial
palettes/alpha and other CPUs keep scalar behavior and distinct performance scope.
The fixed original-raster projection adds no original image; palette4/8 bytes stay
unchanged. Initial native-worker whitelist rejection is retained without timing.
JPEG420's upper gap stays open. Packed palette1 remains scalar and is the next
useful one-pair screen before more small tuning of near-parity cases.

The [packed palette1 round](WARM-PNG-PALETTE1-ROW.md) measures a 3.739x scalar gap
and improves one dense expansion primitive. Safe sixteen-pixel SSE2/SSSE3 expansion
reduces decode mean 80.9%; independent confirmation is 0.713x / upper 0.723x and
9,072 B/op unchanged. Both guards pass; all 12 actuals are retained. Full both-OS
Release/live tests and same-package/all-three-example checks pass. Stop tuning
this fixed pair. The original-raster projection adds no original image; palette2/4/8
bytes and the generic native PAL8 worker remain unchanged. Partial palette/alpha,
other CPUs and public ownership boundaries remain distinct. JPEG420's upper gap
stays open; packed gray2 is the next one-pair material screen.

The [packed gray2 check](WARM-PNG-GRAY2-CURRENT.md) independently confirms 0.756x /
upper 0.783x with 9,136 B/op. All 12 actuals retained; both guards pass. The existing
AVX2 expansion supports this depth; production/package unchanged, full validation
reused. Stop timing this fixed pair. Gray1/4 bytes remain exact; no original image
was added. The development gray-depth whitelist was extended with the same timed
native conversion/output boundary. JPEG420's upper-bound gap and full material
unknowns remain open. PNG Up RGBA8 is the next useful screen: its reconstruction
still follows the generic scalar filter loop, distinct from None and Paeth.

The [PNG Up RGBA8 check](WARM-PNG-UP8-BASELINE.md) independently confirms 1.756x /
upper 1.764x and 12,976 B/op. Both guards pass; all 12 actuals remain retained.
Production/package/native worker unchanged, full validation reused; stop timing/
tuning this pair. Original RGBA8 and frozen Paeth8/16 bytes remain exact; no original
image was added. Descriptive errata clarify packed palette1/2 and gray2 recipe text
outside their original immutable observations; timing/allocation/pixel hashes stay
unchanged. Sub RGBA8 is the next distinct material screen; JPEG420's upper-bound
gap and the complete required scope remain open.

The [PNG Sub RGBA8 check](WARM-PNG-SUB8-BASELINE.md) independently confirms 1.772x /
upper 1.780x with 12,976 B/op. Both guards pass; all 12 actuals are retained.
Production/package/native worker unchanged, full validation reused; stop timing/
tuning this fixed pair. Original RGBA8 and frozen Up/Paeth bytes remain exact; no
original image was added. Average RGBA8 is the next distinct reconstruction screen;
JPEG420's upper-bound gap and the full required material scope remain open.

The [PNG Average RGBA8 check](WARM-PNG-AVERAGE8-BASELINE.md) independently confirms
1.704x / upper 1.743x with 12,976 B/op. Both guards pass; all 12 actuals retained.
Production/package/native worker unchanged, full validation reused; stop timing/
tuning this fixed pair. The original raster and frozen Sub/Up/Paeth bytes remain
exact; no original image was added. All five filter types have confirmed RGBA8
cases, with other material layouts still open. RGB8 PNG encoding is the next useful
one-pair screen, reusing the existing opaque RGB projection. JPEG420's upper-bound
gap and the full required scope remain open.

The [PNG RGB8 encode boundary check](WARM-PNG-RGB8-ENCODE-BOUNDARY.md) adds that
material case using the same projection. Native packing runs within each encode
operation; exact scanlines/pixels, normal/scalar CLI and held output owners pass.
The existing 5% compression-size gate stops preparation: both use level 6/None,
but IDAT payloads are 191,758 versus 168,159 bytes. A single managed write saves
only 120 bytes. No performance samples or allocations are scored, no production
change is made, and the comparison remains open. The prior full verification is
reused after all fresh/generated production methods and flags match the same
actual package. Other material cases and JPEG420's upper-bound gap remain open.

The [PNG RGB16 encode check](WARM-PNG-RGB16-ENCODE.md) independently confirms
0.865x / upper 0.871x with 2,102,765 B/op. Both quietness guards pass; all 12
actuals are retained. Timed native RGBA64LE-to-RGB48BE packing, exact words and
scanlines, held owners and matched level-6/None compression are established using
the existing projection. Production is unchanged and full verification reused.
The initial harness classification failure and CPU0 I/O-wait guard failure stay
unscored in the evidence. Stop tuning this fixed pair. RGB8's comparison, gray
encoding projections, JPEG420's bound and full required warm coverage stay open.

The [PNG gray8 encode boundary check](WARM-PNG-GRAY8-ENCODE-BOUNDARY.md) places
native full-range RGBA-to-GRAY8 conversion within each operation using the existing projection.
Exact samples, scanlines, normal/scalar CLI and held owners pass. Both level-6/
None outputs exceed the existing 5% size gate: 86,745 versus 72,304 IDAT bytes.
A single managed write saves only 34 bytes. The original preparation failure
remains unscored, with no timing or allocation claim. Production is unchanged,
full verification reused, and eight positive/two negative native controls pass.
Gray8 and RGB8 encode comparisons, other material cases and JPEG420's bound stay
open; confirmed fixed pairs remain stopped.

The [PNG gray16 encode check](WARM-PNG-GRAY16-ENCODE.md) independently confirms
1.150x / upper 1.156x with 1,051,455 B/op. Both quietness guards pass; all 12
actuals are retained. Ordinary native conversion changed words by up to 253 and
remains unscored. FFmpeg's exact red-plane extraction/endian conversion provides
the matched reference, including intermediate ownership within every operation.
The original grayscale projection, exact normal/scalar CLI, source/owners and
level-6/None size gate pass. Production is unchanged and full verification reused.
Stop tuning this fixed pair; gray-alpha encoding projections, RGB8/gray8 comparison
policies, JPEG420's bound and the full required scope remain open.

The [gray-alpha8 encode boundary](WARM-PNG-GRAY-ALPHA8-ENCODE-BOUNDARY.md) preserves
original alpha and 279 hidden-RGB pixels using exact storage packing inside the
native operation before FFmpeg PNG encoding. Both colorspace conversion attempts
fail exactness and stay unscored. The matched pixel/scanline case fails the
existing size gate: 126,485 versus 119,940 IDAT bytes, 5.17%. Batching saves only
61 bytes. No timing/allocations are scored; production is unchanged and prior
verification reused after method/flag identity checks. The comparison and full
required material scope remain open.

The [gray-alpha16 encode check](WARM-PNG-GRAY-ALPHA16-ENCODE.md) independently
confirms 0.903x / upper 0.915x with 1,052,368 B/op. Both guards pass; all 12 actuals
are retained. Exact full-word red/alpha extraction and endian packing occur
inside the native operation, followed by FFmpeg PNG encode/owned packet release.
Canonical normal/scalar CLI, original alpha/hidden RGB, source/owners and the
level-6/None size gate pass. Production is unchanged and verification reused.
Stop tuning this fixed pair. Unmatched 8-bit compression comparisons, JPEG420's
bound and full required material scope remain open.

The [RGBA8 NoCompression encode check](WARM-PNG-STORED8-ENCODE.md) exposes an
11.526x gap dominated by repeated component access. One bulk RGBA8 row-copy
primitive reduces the screen mean by 83.7%; the independent repeat confirms
1.883x / upper 1.933x with 2,102,472 B/op. All actuals, exact encoded bytes,
admission and cancellation are preserved. Both quietness guards pass and the
same actual package/full Release/differential tests pass on both OSes. Stop
tuning this fixed case. Other material work and full required parity stay open.

The [stored-Deflate RGBA8 decode check](WARM-PNG-STORED8-DECODE.md) independently
confirms 1.159x / upper 1.168x with 12,976 B/op. Both guards pass; all 12 actuals
are retained. It uses the same frozen level-0 PNG from the encode case with exact
RGBA, alpha, hidden RGB, normal/scalar CLI, source and held owners. Production,
native worker and package are unchanged and full verification reused after
fresh/actual-child method/flag identity checks. Stop tuning this fixed case;
other material work and the full required scope stay open.

The [palette8 partial-alpha check](WARM-PNG-PALETTE8-PARTIAL-ALPHA.md)
independently confirms 1.953x / upper 1.967x with 10,728 B/op. The same original
palette indices and RGB use two explicit alpha entries, then default opacity.
Native palette expansion and output ownership are timed; exact samples and both
guards pass. Production is unchanged and verification reused after fresh/actual
child method/flag checks. Stop tuning this fixed case; required scope stays open.

The [JPEG 4:2:2 encode check](WARM-JPEG422-ENCODE.md) independently confirms
1.963x / upper 1.990x with 527,307 B/op and both guards passing. Two settings on
the existing original raster establish one fixed source-quality anchor; the
first unmatched setting remains untimed. Flattening, conversion and owned
packet release are included. Output sizes and Huffman differences are explicit;
production is unchanged and verification reused. Required scope stays open.

The [RGBA16 JPEG component-row round](WARM-JPEG16-COMPONENT-ROW.md) exposes a
3.107x scalar sampling gap and reduces the screen mean 38.4%. One safe eight-pixel
row primitive independently confirms 1.913x / upper 1.922x with 1,051,739 B/op.
Full 16-bit blending/normalization, conversion, encoding and owned release are
timed. Exact normalized and encoded bytes, resource/cancellation contracts and
both quiet guards are preserved; full tests and the same actual package pass
on both OSes. Stop tuning this fixed case; subsampled RGBA16 remains open.

The [RGBA16 4:2:0 chroma round](WARM-JPEG42016-CHROMA-ROW.md) reduces its
screened mean 42.9%, from 3.549x to 2.022x, upper 2.796x, with 527,306 B/op.
Exact bytes, tap order, normalization and resource contracts are preserved;
full tests and the same actual package pass on both OSes. The pair remains
open and has no twelve-actual confirmation. Prioritize the next material
normalization gap before small further tuning of this remainder.

The [RGBA16 VP8 encode check](WARM-VP8-RGBA16-ENCODE.md) independently confirms
0.809x / upper 0.819x with 2,615,472 B/op. Full channel normalization and native
repacking are timed; exact alpha, integer-reference checks, source/owners and
the unchanged quality gate pass. Production is unchanged and verification is
reused after all current/actual-child method and flag checks. Stop tuning this
fixed pair; RGBA16 lossless WebP and the remaining required scope stay open.

The [RGBA16 VP8L encode check](WARM-VP8L-RGBA16-ENCODE.md) independently confirms
0.0654x / upper 0.0664x against the approved exact backend, with 2,100,119 B/op.
Normalization, repacking, fresh import allocation and owned packet release are
timed. All normalized RGBA samples and hidden RGB are exact; both guards pass.
Production is unchanged and same-package validation is reused after all method/
flag checks. Stop tuning this pair; PNG transparency paths and required scope
remain open.

The [gray8 tRNS row round](WARM-PNG-GRAY8-TRANSPARENCY.md) exposes a 3.593x
generic mapping gap and reduces the screen mean 79.4%. One exact-key row
primitive independently confirms 0.739x / upper 0.761x with unchanged 9,936 B/op.
The native format rejection remains untimed; explicit YA8 conversion, exact
hidden RGB, source/owners, both guards, full tests and the same actual package
pass. Stop tuning this pair; RGB transparency and required scope remain
open.

The [gray16 tRNS check](WARM-PNG-GRAY16-TRANSPARENCY.md) independently confirms
1.722x / upper 1.755x with 10,960 B/op and unchanged production. Full word
samples, hidden RGB and alpha match managed and normal/scalar FFmpeg outputs;
the previous gray8 fixture remains byte-identical. Both guards pass and the
same-package validation is reused after all fresh/actual method and flag checks.
Stop tuning this pair; RGB16 transparency and the required scope remain open.

The [RGB8 tRNS row round](WARM-PNG-RGB8-TRANSPARENCY.md) exposes a 3.524x
generic mapping gap and reduces the screen mean 59.9%. One full-triplet row
kernel independently confirms 1.417x / upper 1.439x with unchanged 11,984 B/op.
Native alpha expansion occurs inside decoding into already owned RGBA; no
redundant conversion is timed. Exact original RGB, hidden RGB, prior native
controls, both guards, full tests and the same actual package pass on both OSes.
Stop tuning this pair; JPEG420 gaps and the required scope remain open.

The [JPEG420 RGBA16 entropy investigation](WARM-JPEG42016-ENTROPY-REJECTED.md)
retains a fresh 2.046x / upper 3.240x baseline with 527,306 B/op. Inlining,
cached byte dispatch and batched symbols do not establish useful headroom and
are discarded without twelve-actual repetitions. All original production is
restored, with fresh both-OS method/flag proofs against the validated package.
Investigate a larger Huffman coding change next; both JPEG420 gaps remain open.

The [adaptive Huffman correctness prototype](JPEG-HUFFMAN-PROTOTYPE.md) cuts the
same JPEG420 RGBA16 packet by 49.9%, preserving every coefficient and decoded
pixel. Safe C# and independent table/packet generation agree; both managed OSes
and normal/scalar FFmpeg pass. Production is unchanged. Bounded cache admission,
complete-operation timing and allocations remain open; this is no parity score.

The integrated [adaptive Huffman round](WARM-JPEG-ADAPTIVE-HUFFMAN.md) confirms
JPEG420 RGBA16 at 1.888x / upper 1.931x with 297,772 B/op. The restored baseline
mean falls 7.8%, allocations 43.5% and packet size 49.9%, with exact coefficients
and decoded pixels. Corrected exact output quotas, historical fixed quality
evidence, full tests and the same actual package pass on both OSes. Stop tuning
this pair; revalidate JPEG420 RGBA8 and the shared encoder scope next.

The [RGBA8 adaptive revalidation](WARM-JPEG420-RGBA8-ADAPTIVE.md) retains an
independent quiet mean of 1.977x / upper 2.014x with 297,772 B/op. Coefficients,
decoded pixels and the one-setting quality anchor are unchanged; allocations
fall 43.5%. The pair remains open because its upper exceeds two. Production and
the tested package are unchanged, with all fresh/actual method and flag checks
passing. Do not repeat unchanged timing; revalidate one other shared JPEG case.

The [RGBA16 JPEG444 adaptive revalidation](WARM-JPEG444-RGBA16-ADAPTIVE.md)
independently confirms 1.757x / upper 1.790x with 556,280 B/op. Full normalization,
coefficients and decoded pixels are unchanged; allocations fall 47.1% against
the earlier fixed-table confirmation. Both guards and all fresh/actual method
and flag proofs pass; the unchanged package's both-OS validation is reused.
Stop tuning this pair; JPEG422 RGBA8 shared-encoder revalidation remains open.

The [RGBA8 JPEG422 adaptive revalidation](WARM-JPEG422-RGBA8-ADAPTIVE.md)
independently confirms 1.821x / upper 1.862x with 294,110 B/op. Quality65,
coefficients and decoded samples are unchanged; allocations fall 44.2% against
the earlier fixed-table confirmation. Both guards and all fresh/actual method
and flag proofs pass; unchanged-package validation is reused. All twelve actuals
remain recorded. Stop tuning this pair; JPEG444 RGBA8 shared revalidation remains
open, using its existing quality69 anchor without repeating a quality sweep.

The [RGBA8 JPEG444 adaptive revalidation](WARM-JPEG444-RGBA8-ADAPTIVE.md)
independently confirms 1.772x / upper 1.818x with 559,384 B/op. Its previous
quality69 anchor is evaluated once; coefficients and decoded samples are exact.
Allocations fall 46.8% against the earlier fixed-table result. An initial guard
fails before any actual iteration, remains unscored, and is frozen before the
same child completes the quiet confirmation. Both guards and all method/flag
proofs pass; unchanged-package validation is reused. Stop tuning this pair;
JPEG420 RGBA8's upper and the remaining material scope remain open.

The [RGB16 tRNS row round](WARM-PNG-RGB16-TRANSPARENCY.md) exposes a 2.117x
generic mapping gap and reduces the screened mean 40.8%. One full-word triplet
primitive independently confirms 1.272x / upper 1.302x with unchanged 15,056 B/op.
Two transparent pixels retain hidden RGB; 169 upper-byte collisions stay opaque.
Native endian conversion/output ownership is timed, with no colorspace conversion.
The original failed post-run I/O guard remains unscored; the unchanged child
passes a quiet repeat after pending writes are flushed outside timing. Full
tests and the same actual package pass on both OSes. Stop tuning this pair;
material opaque WebP paths and the remaining required scope stay open.

The [opaque VP8 decode check](WARM-VP8-OPAQUE-DECODE.md) retains a quiet repeat
at 1.983x / upper 2.004x with 760,548 B/op. The upper remains above two. No
production changes are made; fresh/actual method and flag checks permit reuse
of the tested package. Normal/scalar native and both managed OSes preserve the
existing RGB/alpha/geometry bounds and independent owners. The extended native
adapter retains the previous alpha packet/pixels; original untimed rejections
remain recorded. Do not repeat unchanged timing; profile one complete decode
and select a substantial primitive. Full required warm coverage remains open.

The [normalization investigation](VP8-NORMALIZATION-REJECTED.md) records a
diagnostic profile dominated by token decisions and rejects a 2.016x bulk-shift
trial without twelve-actual repetition. Original production/tests are restored,
with fresh both-OS proofs against the tested package. Opaque VP8 remains open;
prototype a wider arithmetic window before another production/timing round.

The [arithmetic-window correctness prototype](VP8-WINDOW-PROTOTYPE.md) passes
5,418,833 scalar/actual-package comparisons per OS/mode and preserves exact
opaque/alpha samples and held owners. Production remains unchanged; no timing
claim is made. The private state adds at most 32 bytes across eight partitions.
Fixed admission, one-primitive integration and complete-operation VM measurement
were left open by that correctness-only round.

The [wider-window integration](VP8-WINDOW-REJECTED.md) subsequently measures
2.052x / upper 2.115x with unchanged 760,548 B/op, slower than the retained
9.505-ms baseline. Both guards and focused normal/scalar checks on both OSes
pass, but the candidate is rejected without a twelve-actual repetition.
Production and tests are restored; all 630 fresh/actual bodies and flags match
the validated package. No reader gain is accepted. Opaque VP8's baseline upper
and JPEG420 RGBA8 remain open; cover a remaining material workload next.

The [four-pixel upscale kernel](WARM-BILINEAR-UP8-BATCH.md) reduces the newly
covered RGBA8 bilinear gap from 3.041x to independently confirmed 1.764x / upper
1.787x, with 2,235,599 B/op and exact retained pixels. Full tests and the same
actual package pass on both OSes. Stop tuning this fixed pair; screen RGBA16
upscaling or another materially distinct remaining path next.

The [RGBA16 horizontal-row round](WARM-BILINEAR-UP16-ROWS.md) exposes a 2.624x
gap and reduces its baseline mean 52.4%, independently confirming 1.331x / upper
1.416x with 4,498,974 B/op. Full precision, original pixels, resource fallbacks,
full tests and the same actual package pass on both OSes. Stop tuning this fixed
pair; remaining material coverage and final-source parity remain open.

The [RGBA16 bicubic enlargement round](WARM-BICUBIC-UP16-ROWS.md) reduces a
4.154x baseline to independently confirmed 1.505x / upper 1.516x, with
5,016,578 B/op. The existing row cache now admits word enlargement on both axes;
managed rational pixels stay exact and both native modes satisfy the explicit
scaler bounds. Full both-platform tests and the same actual package pass once.
Stop tuning this fixed pair; mixed-axis, wide-support and public boundaries
remain open.

The [mixed-axis RGBA8 bicubic round](WARM-BICUBIC-MIXED8-ROWS.md) exposes a
19.938x direct-evaluation gap. Row reuse plus checked two-pixel cache indexing
independently confirms 1.945x / upper 1.978x, reducing time 90.3% at
1,283,063 B/op. Exact rational pixels, normal/scalar bounds and quiet guards
pass. Full both-platform tests and the same actual package pass once; two
quiet regression screens remain below two. Stop tuning this fixed pair;
other mixed directions/depths and required current-source coverage remain open.

The [word mixed-axis row round](WARM-BICUBIC-MIXED16-ROWS.md) reduces a 20.419x
RGBA16 gap by 90.6%, independently confirming 1.924x / upper 1.975x at
1,813,110 B/op. Exact exponent bounds avoid helper calls and adjacent four-tap
outputs reuse five source vectors. An initial byte regression at 2.013x holds
acceptance; the same shared-tap path at both depths restores byte confirmation
at 1.848x / upper 1.867x with unchanged 1,283,063 B/op. Both final confirmations
use the same immutable build. Full final both-platform tests and the same actual
package pass. Stop these pairs; opposite mixed direction and remaining material
required/current-source coverage remain open.

The [RGBA8 width-reduction / height-enlargement round](WARM-BICUBIC-CROSS8-ROWS.md)
reduces a 21.910x gap by 94.6%, independently confirming 1.190x / upper 1.206x
at 731,753 B/op. One admission change reuses the existing checked row cache;
managed pixels and original precision/fallback contracts stay exact. The failed
pre-run I/O guard starts no actual iteration and remains recorded; the same
build passes after preparation writes are flushed. Two quiet regression screens
stay below two. Full both-platform tests and the same actual package pass once.
Stop this fixed pair; RGBA16 in this direction and remaining material scope stay
open.

The [RGBA16 width-reduction / height-enlargement round](WARM-BICUBIC-CROSS16-ROWS.md)
reduces a 23.838x gap by 94.7%, independently confirming 1.285x / upper 1.297x
at 1,263,255 B/op. One admission change reuses the existing checked word-row
kernel, preserving exact products, packing, precision guards and fallback.
Two quiet regression screens remain below two. Full both-platform tests and the
same actual package pass once. Stop this fixed pair; single-axis resizing, wider
support, tight budgets and remaining material required/current-source coverage
stay open.

The [wider-support RGBA8 row round](WARM-BICUBIC-WIDE8-ROWS.md) reduces a 26.954x
fallback gap by 94.0%, independently confirming 1.631x / upper 1.642x at
231,227 B/op. A bounded thirty-two-row/tap byte kernel preserves exact rational
pixels through conservative wider guards and original-order/tiny-alpha fallback.
The explicit native eight-pixel halo is timed, adding 9.489% input area; quality
bounds stay unchanged. Setup refusals remain unscored. Two quiet existing-path
regression screens stay below two. Full both-platform tests and the same actual
package pass once. Stop this pair; wider word support, larger kernels, single-axis
resizing, tight budgets and remaining required/current-source coverage stay open.

The [wider-support RGBA16 row round](WARM-BICUBIC-WIDE16-ROWS.md) reduces a 25.058x
fallback gap by 94.2%, independently confirming 1.432x / upper 1.446x at
263,988 B/op. The same bounded row kernel now handles exact unsigned word
products and packing, with conservative word precision guards and original-order
fallback. Native halo and complete operation boundaries stay explicit and timed.
Two quiet regression screens stay below two. Windows and Linux each pass 2,847
tests and the same actual package checks once. Stop this pair; larger kernels,
single-axis resizing, tight budgets and required current-source parity stay open.

The [RGBA8 horizontal-only row round](WARM-BICUBIC-HORIZONTAL8-ROWS.md) reduces a
7.306x gap by 79.0%, independently confirming 1.535x / upper 1.558x at
465,375 B/op. One admission change reuses the existing sixteen-row kernel for
width reduction with unchanged height. Frozen qualified pixels remain byte-exact;
six independently certified exact alpha halves retain SPEC.md's binary64 rounding.
One quiet shared-path screen passes before full both-platform qualification and
the same actual package checks. Stop this pair; other single-axis directions and
depths, tight budgets and remaining material current-source parity stay open.

The [RGBA16 horizontal-only row round](WARM-BICUBIC-HORIZONTAL16-ROWS.md) reduces
a 7.286x gap by 74.8%, independently confirming 1.837x / upper 1.853x at
730,683 B/op. One admission change reuses the existing checked word cache;
every sample matches the independent rational reference. The screen is faster,
but the independent repeat supplies the published result. One quiet byte
regression screen passes at 1.520x before full both-platform qualification and
the same actual package checks. Stop this pair; other single-axis directions,
tight budgets and remaining material required/current-source parity stay open.

The [RGBA8 support-beyond-thirty-two round](WARM-BICUBIC-64X32-RGBA8.md) reduces
a 24.783x gap by 92.7%, independently confirming 1.811x / upper 1.820x at
205,880 B/op. A bounded sixty-four-row byte cache and per-pixel uncertainty guards
avoid repeated horizontal filtering and redundant original-order evaluation.
Every guard and rational pixel remains exact. The initial cache and vectorized
fallback remain above two and receive no twelve-sample/full qualification.
One six-second profile identifies the substantial fallback cost. Native padding
is explicit and timed: sixteen pixels, 19.364% extra input area. Two quiet byte/
word regression screens pass before full both-platform qualification and the
same actual package checks. Stop this pair; wider word support, single-axis
directions, tight budgets and remaining material current-source parity stay open.

The [RGBA16 support-beyond-thirty-two round](WARM-BICUBIC-64X32-RGBA16.md) reduces
a 23.466x gap by 94.1%, independently confirming 1.387x / upper 1.414x at
214,071 B/op. One word extension reuses the sixty-four-row cache with wider
conservative word bounds and unsigned certain-neighbor packing. Every managed
sample matches the rational reference; original/tiny-alpha fallback remains exact.
The native adapter's existing word macro reuses the same timed sixteen-pixel halo
and complete workload boundary. Two quiet byte/word regression screens pass before
full both-platform qualification and the same actual package checks. Stop this
pair; other single-axis directions, larger supports, tight budgets and remaining
material required/current-source parity stay open.

The [RGBA16 support-beyond-sixty-four round](WARM-BICUBIC-32X16-RGBA16.md) reduces
a 21.564x gap by 94.8%, independently confirming 1.125x / upper 1.131x at
201,397 B/op. The existing 128-row/tap kernel now admits words, with exact unsigned
products and quotient-error bounds using the full sample maximum. Every rational
pixel stays exact; the native maximum-1 / RMSE-0.25 bounds are unchanged. Timed
padding remains thirty-two pixels, 40.269% extra input area. One quiet shared
byte regression passes before full both-platform/same actual package qualification.
Stop this pair; support beyond 128 taps, tight budgets and remaining material
required/current-source parity stay open.

The [RGBA8 adaptive-support round](WARM-BICUBIC-16X8-ADAPTIVE.md) reduces a 19.318x
gap by 92.9%, independently confirming 1.357x / upper 1.368x at 201,721 B/op.
Support above 128 selects a bounded power-of-two capacity up to 2048; larger row
metadata moves into admitted existing arrays and the same absolute quotient-error
budget scales with capacity. Fourteen byte/word cases qualify larger capacities;
every measured rational pixel stays exact. The explicit timed halo is sixty-four
pixels, 86.703% extra input area; both native modes are exact here. One quiet word
regression passes before full both-platform/same actual package qualification.
Stop this pair; larger supports, tight budgets and
remaining material required/current-source parity stay open.

The [unchanged-production RGBA16 16×8 check](WARM-BICUBIC-16X8-RGBA16.md)
independently confirms 0.952x / upper 0.958x at 202,229 B/op. Every rational managed
sample remains exact; normal/scalar native maximum-one and RMSE bounds pass with
the same timed sixty-four-pixel halo. Screen and repeat reuse the qualified
production assembly. Full both-platform/actual-package qualification is reused;
no production optimization, repeat suite or profile is needed. Stop this pair;
tight budgets, larger/unmatched supports and remaining material coverage stay open.

The [RGBA16 vertical-only enlargement round](WARM-BICUBIC-VERTICAL-UP16-ROWS.md)
reduces a 2.743x gap by 53.3%, independently confirming 1.288x / upper 1.297x at
2,516,605 B/op. One admission change reuses the checked word-row cache and exact
unchanged-width identity copy, preserving every rational pixel. The scoped
approved native RGB maximum-2 allowance applies only where reference alpha is
at most sixteen; elsewhere maximum one remains, with unchanged alpha and RMSE
bounds. Negative controls qualify that scope. The original refusal and rejected
input conversion remain unscored. A quiet byte regression screen passes before
full both-platform/same actual package qualification. Stop this pair; wider word
support, tight budgets and remaining material current-source parity stay open.

The [RGBA8 vertical-only row round](WARM-BICUBIC-VERTICAL8-ROWS.md) reduces a
10.018x gap by 80.6%, independently confirming 1.941x / upper 1.956x at
648,991 B/op. One admission change reuses the existing sixteen-row cache for
height reduction with unchanged width. Frozen qualified pixels remain byte-exact;
twenty-five independently certified alpha halves retain SPEC.md's binary64
rounding. One quiet horizontal regression screen passes at 1.512x before full
both-platform qualification and the same actual package checks. Stop this pair;
other single-axis directions/depths, tight budgets, larger supports and remaining
required/current-source parity stay open.

The [RGBA8 support-beyond-sixty-four round](WARM-BICUBIC-32X16-RGBA8.md) reduces a
22.139x gap by 94.7%, independently confirming 1.183x / upper 1.192x at
199,354 B/op. Bounded 128-row reuse and the existing absolute-error budget applied
to actual quotient denominators preserve every rational pixel while avoiding
unnecessary direct evaluation. The fixed-margin trial remains 2.954x and receives
no twelve/full qualification. Native padding is explicit and timed: thirty-two
pixels, 40.269% extra input area; both modes are exact on this raster. One quiet
64-tap regression screen passes before full both-platform/same actual package
qualification. Stop this pair; wider word support, tight budgets and remaining
material required/current-source parity stay open.

The [shared vertical identity-filter round](WARM-BICUBIC-VERTICAL16-IDENTITY.md)
reduces a 10.099x RGBA16 gap by 85.7%, independently confirming 1.441x / upper
1.480x at 913,737 B/op. Word admission alone stays above two and receives no
twelve/full qualification. Copying decoded exact products removes the identity
horizontal filter at both depths without changing intermediates, guards, work,
cancellation or fallback. On the same immutable final build, byte vertical resize
also confirms 1.331x / upper 1.345x, 761.11 µs / 648,992 B/op, improving its
previous confirmed mean 31.3%. Both pass before one full both-platform/package
qualification. Stop both pairs; other single-axis directions, larger supports,
tight budgets and remaining material current-source parity stay open.

The [RGBA8 horizontal-only enlargement round](WARM-BICUBIC-HORIZONTAL-UP8-ROWS.md)
reduces a 2.490x gap by 30.5%, independently confirming 1.727x / upper 1.742x at
1,930,602 B/op. Admission alone has an independent upper above two; reusing five
source vectors instead of eight for adjacent four-tap outputs supplies the accepted
gain. Original tap order, guards and every rational pixel remain exact. One quiet
horizontal-reduction regression screen passes before full both-platform tests
and the same actual package qualification. Three pre-run guard refusals remain
unscored. Stop this pair; other single-axis directions/depths, larger supports,
tight budgets and remaining material required/current-source parity stay open.

The [unchanged-production RGBA16 horizontal enlargement check](WARM-BICUBIC-HORIZONTAL-UP16.md)
independently confirms 1.960x / upper 1.973x at 2,232,080 B/op. Only the existing
benchmark's word selection changes; its complete native boundary and every
rational managed pixel pass. The qualified library and full both-platform/actual
package qualification are reused, avoiding an unnecessary production change,
profile or repeat suite. Stop this pair; other single-axis directions, larger
supports, tight budgets and remaining material current-source parity stay open.

The [RGBA8 vertical-only enlargement round](WARM-BICUBIC-VERTICAL-UP8-ROWS.md)
reduces a 3.108x gap by 58.8%, independently confirming 1.279x / upper 1.286x at
1,664,240 B/op. One admission extension reuses the existing sixteen-row cache and
exact unchanged-width identity copy. Every original byte stays exact; one
independently certified rational blue half retains SPEC.md's binary64 convention.
The shared height-reduction regression screen passes before full both-platform/
same actual package qualification. Preparation failures remain unscored. Stop
this pair; word enlargement, larger supports, tight budgets and remaining material
required/current-source parity stay open.

The [tight RGBA16 vertical enlargement round](WARM-BICUBIC-TIGHT-VERTICAL-UP16.md)
reduces a 12.065x gap by 86.6%, confirming 1.618x / upper1.636x at 2,133,096 B/op.
One strip primitive admits identity axes and defers only tiny-alpha pixels until
its scratch frame unwinds. Every original/rational word is exact. The existing
both-axis tight enlargement regression screen passes before full qualification.
The approved native low-alpha RGB2 exception stays scoped to the exact vertical
RGBA16 geometry. Admission-only replay, registration and test-setup failures
remain retained and unscored. Stop this pair; other material tight-memory
boundaries and current-source uniform parity remain open.

The [tight RGBA8 vertical enlargement round](WARM-BICUBIC-TIGHT-VERTICAL-UP8.md)
reduces an 11.445x gap by84.8%, confirming1.748x / upper1.764x at1,280,402 B/op.
One strip primitive admits identity axes while retaining its arithmetic, guards,
2416-byte fixed arrays and deferred exact tiny-alpha replay. Every original byte
stays exact, including one certified rational blue half. The existing both-axis
tight enlargement regression mean passes; its three-sample upper2.011x is a
screen, not new independent certification. No new raster/profile or comparison
adjustment. Stop this pair; other material coverage and uniform parity stay open.

The [tight RGBA16 horizontal enlargement round](WARM-BICUBIC-TIGHT-HORIZONTAL-UP16.md)
reduces a2.086x gap by22.9%, confirming1.606x / upper1.614x at2,129,311 B/op.
One word strip primitive omits zero vertical rows only where the original binary64
center is exactly integral; full work admission, precision and every rational
pixel remain. The two-axis regression mean passes; its upper2.623x is a screen,
not certification. Tight RGBA8 horizontal enlargement staysopen at2.064x and is
next. A premature confirmation was stopped and unscored. No new raster/profile
or comparison adjustment. Uniform current-source coverage remains open.

The [tight RGBA8 horizontal enlargement round](WARM-BICUBIC-TIGHT-HORIZONTAL-UP8.md)
reduces a2.064x gap by23.4%, confirming1.583x / upper1.591x at1,178,425 B/op.
One byte strip primitive omits vertical zero rows only at an exactly integral
original center, preserving every rational pixel, work charge and precision guard.
The same immutable build independently confirms existing both-axis tight cases:
RGBA8 at1.783x / upper1.795x and RGBA16 at1.773x / upper1.783x, resolving their
recent unconfirmed short bounds. Three workloads, unchanged harness, no new
original raster/profile or comparison adjustment. Stop all three confirmed pairs;
remaining material codec/public/current-source coverage stays open.

The [byte PNG Optimal encoder round](WARM-PNG-BYTE-OPTIMAL.md) closes the three
previous size-gate refusals without relaxing them. Public Optimal selects BCL9
for byte gray/RGB/gray-alpha; FFmpeg remains6 and actual levels are labeled.
Independent confirmations are gray1.802x / upper1.818x at263,664 B/op,
RGB1.093x / upper1.107x at527,098 B/op, and gray-alpha1.374x / upper1.382x
at264,179 B/op. Exact samples, filterNone and the5% IDAT gate remain. Decoder
fixture bytes stay pinned and three untimed controls pass. One encoder policy,
three existing workloads, two original rasters, shared immutable drivers.
Original same-level failures/audits remain retained and unscored. Stop these
three confirmed pairs; other material codec/public/current-source coverage stays open.

The [shared RGBA8 chroma round](WARM-JPEG420-CHROMA-PAIR.md) closes the adaptive
4:2:0 upper bound at **1.943x / upper 1.988x**, with unchanged **297,772 B/op**
and the exact original packet. Optional admitted 64-byte stack storage shares
unpacking/flattening while preserving arithmetic and component order. Adjacent
RGBA16 4:2:0 and RGBA8 4:2:2 short regression means pass before one complete
both-platform/same-package qualification. Stop this fixed pair; constrained
fallback, grayscale and remaining material/current-source coverage stay open.


[Fresh seekable PNG8 Stream decode](WARM-STREAM-PNG8.md) independently confirms
**1.907x / upper 1.916x**, at **756,605 B/op**, on unchanged qualified production.
Both subjects reacquire input through fresh stream contexts and time all copies,
EOF checks, full decode and owned output release. Exact samples, source/caller
lifetime and two-live-output independence pass. The existing original raster and
frozen encoded input are reused. Stop this fixed pair; other fresh input kinds,
codec/depth boundaries, compositions and current-source uniform coverage remain open.


[PNG8 Stream/orient/crop/fit](WARM-PNG-COMPOSITION8.md) independently confirms
**1.891x / upper 1.906x**, with **1,453,517 B/op** and exact RGBA coordinates.
Public normal AutoOrient, centered256x129 crop and nearest fit128x64 yield127x64.
All managed intermediate materialization, native graph/conversions, fresh input
copies and release are timed; native intermediate views are allowed. All twelve
confirmation actuals remain included in the conservative bound. Production,
original raster and encoded input are unchanged, so existing qualification is
reused. Initial guard/protocol refusals remain unscored. Stop this fixed pair;
other material public input/depth boundaries and uniform current-source coverage
remain open.


[Fresh seekable PNG16 Stream decoding](WARM-STREAM-PNG16.md) independently
confirms **1.819x / upper 1.830x**, at **3,075,772 B/op**. Fresh input acquisition,
all copies, complete decode, native BE-to-LE conversion and independent output
release are timed. Every 16-bit sample, alpha/hidden RGB, caller-stream lifetime
and two-owner independence pass. Existing raster/input and qualified production
are reused, with no full/package/method-audit repeats. Stop this fixed pair;
other material paths and final current-source uniform coverage remain open.


[Eight-pixel VP8 loop filtering](WARM-VP8-FILTER8.md) closes the opaque upper-bound
gap: **1.803x / upper 1.808x**, with exact frozen pixels and unchanged760,548B/op.
The retained managed mean falls8.8%. Alpha-bearing VP8 also independently confirms
**1.803x / upper 1.819x**, at unchanged1,966,616B/op. One bounded integer-vector
primitive preserves scalar thresholds/clamps/rounding/work and fallback. Four
new cases and1,280 scalar plane comparisons per mode precede timing; bothOS pass
3,293Release tests(896live) and consumer+3examples through the same actual package.
All guards pass; one frozen core/harness, two existing workloads, no new image or
quality adjustment. Stop both pairs; final current-source material coverage remains open.


[JPEG422 from RGBA16](WARM-JPEG422-RGBA16.md) independently confirms
**1.874x / upper 1.884x**, at **294,110 B/op**. Complete word normalization,
single-round alpha flattening, color conversion and adaptive encoding are timed.
One owned quality65/native qscale5 passes the unchanged0.25dB source band at
0.245890dB. Exact packets/source/owners and bounded registered-decoder agreement
pass. All twelve actuals remain represented in the bound. Two benchmark inputs
change; qualified production, native worker and original raster are reused,
without repeated full/package/method audits. Stop this fixed pair.

Single-component grayscale JPEG encoding remains unscored: the pinned
[FFmpeg MJPEG encoder accepts YUV layouts](https://github.com/FFmpeg/FFmpeg/blob/9047fa1b084f76b1b4d065af2d743df1b40dfb56/libavcodec/mjpegenc.c#L699),
and its [frame header writes at least three components](https://github.com/FFmpeg/FFmpeg/blob/9047fa1b084f76b1b4d065af2d743df1b40dfb56/libavcodec/mjpegenc_common.c#L284).
That output does not match Lokad.Image's single-component grayscale contract.
A comparable native reference must be defined before timing this branch;
three-component output must not silently stand in for it.

[JPEG row restart encoding](WARM-JPEG-ADMITTED.md) closes a2.128x gap at
**1.958x / upper1.973x**, with **297,780 B/op** and identical packets. Whole-packet
admission removes repeated entropy byte checks only on eligible stock streams;
original writes, work and failure behavior remain. Matched DRI33 and sixteen
RST markers use a verified serial native executor. Nonrestart adaptive and
fixed streaming RGBA16 regression means pass before full qualification.
Both OSes pass3,298Release tests(896live) and consumer+threeexamples through the
same actual package. Stop this confirmed pair; other material/current-source
coverage stays open.
