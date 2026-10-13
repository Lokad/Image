# Material warm coverage

The [benchmark summary](../docs/benchmark.md) reports confirmed fixed workloads,
not complete coverage of the library contract. Keep unknown cases open and add
one representative only when it exercises distinct substantial work. Reuse the
two existing original 513x259 RGBA8/16 rasters; documented projections for another
pixel format do not introduce another original image. Normally run one pair per
round, at most three, with correctness established before timing.

| Primitive group | Retained coverage | Remaining material work |
| --- | --- | --- |
| PNG decode | RGBA8 all five filters; RGBA16 None/Paeth/Adam7; RGB8/16, gray8/16, gray-alpha8/16 and opaque gray1/2/4 and palette1/2/4/8 None confirmed | Other material depths/layouts; partial palette/alpha paths |
| PNG encode | RGBA8/16, RGB16, gray16 and gray-alpha16, level 6/None confirmed | RGB8, gray8 and gray-alpha8 exact samples, unscored compression-size mismatches; materially different compression work/public boundaries |
| JPEG decode | Baseline/progressive gray, 4:2:0, 4:2:2, 4:4:4 | Distinct restart handling or unusual supported layouts if they dominate work |
| JPEG encode | RGBA8 to 4:4:4, matched quality; 4:2:0 repeat mean below 2x, upper remains above | 4:2:0 upper-bound gap; 4:2:2, grayscale, full 16-bit source normalization |
| WebP | VP8 with alpha decode/encode; VP8L native and managed inputs; exact VP8L backend encode | Material opaque/alpha alternatives and full 16-bit source normalization |
| Resize | Nearest/bilinear at both depths, fixed downsample | Bicubic at both depths; upsample/wide-support paths; tight-memory fallback |
| Exact edits | Both depths and every orientation/permutation, fixed shapes | Retained live output owners versus released-storage reuse |
| Public boundaries | Resident encoded span to owned output; complete single operations | Fresh Stream decoding and decode/orient/crop/fit compositions |

Centered/clamped crop, AutoOrient and ResizeToFit route to the measured copy or
resize primitives after small geometry/metadata work. They need correctness
controls and one relevant composed boundary, not a duplicate matrix of timings.
Fresh Stream and retained-owner boundaries remain distinct from the reported
resident-span/released-storage results.

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
