# JPEG 4:2:2 encoding from RGBA16

The complete word-source encode independently confirms **1.874x FFmpeg /
conservative upper 1.884x**, at **294,110 managed allocated B/op**. Production is
unchanged. The source-quality gate passes without a sweep or tolerance change;
uniform current-source coverage remains open.

| Measurement | Lokad.Image us | FFmpeg us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 4,514.78 | 2,418.33 | 1.867 / 2.034 | 294,110 |
| Independent twelve-actual confirmation | 4,538.42 | 2,422.35 | 1.874 / 1.884 | 294,110 |

The existing original 513x259 RGBA16 raster is encoded into a fresh MemoryStream
using adaptive Huffman coding, managed quality65, 4:2:2 sampling and background
RGB0x204060. FFmpeg uses qscale5 and optimal Huffman coding. Every operation
includes full word-depth normalization and alpha flattening with one rounding,
color conversion, sampling, transform/histogram/table construction, entropy
coding, independent output acquisition and release. Native contexts/frame/packet
wrappers and private managed entropy workspace may be reused; source pixels
and entropy contents are recomputed each time. No output cache is used.

Only **one owned quality setting** is evaluated. The source-PSNR gap is
**0.245890 dB**, within the unchanged **0.25 dB** gate. Both encoded packets have
the required opaque byte-depth JPEG geometry and relative4:2:2 sampling. Native
SOF factors2x2/1x2/1x2 and managed2x1/1x1/1x1 express the same sampling ratios.
The registered decoders disagree by at most3 RGB samples, RMSE at most0.5994,
inside the unchanged maximum4/RMSE1.5 limits. Alpha is exact; source preservation,
deterministic packets and independently held output streams pass before/after
measurement. Every native normalized sample matches the integer reference.

Managed output is **90,253 bytes**, SHA-256
`109037ee6675b3a896e9eb94b9c7598fa28746c15a0386b1193abc0465fab04e`;
native output is **71,210 bytes**. This is a source-quality timing comparison;
compression efficiency parity is not established. No original image, codec
policy or quality/profile search is added.

Both versioned pre/post quiet guards pass. Confirmation retains all twelve
actuals; BenchmarkDotNet selects eleven. The published upper bound takes the
larger filtered/all-actual interval against the faster native bracket's lower
bound, so the excluded actual remains represented. Sequential Release runs use
one pinned CPU, one codec/conversion thread, normal SIMD, disabled tiering and
workstation nonconcurrent GC. Startup, file I/O, IPC, fixture/quality preparation,
hashing and validation are excluded. Native allocations are unmeasured; managed
figures include harness overhead.

Screen and confirmation use the same verified immutable production DLL/harness
and existing native worker. Source inventories differ from the accepted build
only in two benchmark files. Existing **3,293 Release tests per OS**, including
**896 live FFmpeg comparisons**, and consumer plus three examples through the
same actual Release package are reused. No production rebuild, full suite,
package proof or whole-library method audit is repeated.

The [numeric evidence](warm-jpeg42216-evidence.json) retains every sample,
allocation, guard, actual job/assembly identity and qualification link. Stop
this confirmed pair; remaining material branches and final current-source
coverage stay open.
