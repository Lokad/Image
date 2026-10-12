# Material warm coverage

The [benchmark summary](../docs/benchmark.md) reports confirmed fixed workloads,
not complete coverage of the library contract. Keep unknown cases open and add
one representative only when it exercises distinct substantial work. Reuse the
two existing original 513x259 RGBA8/16 rasters; documented projections for another
pixel format do not introduce another original image. Normally run one pair per
round, at most three, with correctness established before timing.

| Primitive group | Retained coverage | Remaining material work |
| --- | --- | --- |
| PNG decode | RGBA8/16, None/Paeth; RGBA16 Adam7; RGB8, gray16, gray-alpha8/16 and opaque gray1/4 and palette4 None confirmed; RGB16, gray8 and palette8 None above 2x | RGB16/gray8/palette8 parity; packed gray2 and palette1/2; other filter implementations |
| PNG encode | RGBA8/16, level 6/None | RGB and gray projections; compression policies with materially different work |
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
