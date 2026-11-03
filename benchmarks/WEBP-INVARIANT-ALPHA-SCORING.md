# WebP invariant alpha scoring

Predictor selection checks every mode's actual alpha histogram. When all fourteen
have their entire nonnegative population in residual bin zero, they share one
exact alpha score under the tile's fixed history and prepared products. Compute
that score once, then add it to each RGB score. Full histograms, hierarchy costs,
mode order, neighbor biases and ties remain unchanged; other tiles keep four-plane
scoring. This uses no source-opacity assumption or additional storage.

| Complete warm encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original RGBA8 / libwebp exact=1 | 129,759.50 | 121,487.63 | 1.068 / 1.081 | 24,899,424 | 183,812 |
| Opaque RGBA8 / libwebp exact=1 | 76,053.83 | 69,943.04 | 1.087 / 1.096 | 12,956,744 | 121,896 |
| Photograph / libwebp exact=1 | 90,849.86 | 82,988.87 | 1.095 / 1.105 | 13,346,868 | 160,032 |
| Text/diagram / libwebp exact=1 | 9,113.69 | 9,621.89 | 0.947 / 0.958 | 6,256,838 | 7,234 |
| Transparent VP8 / FFmpeg | 65,037.57 | 53,579.15 | 1.214 / 1.223 | 10,749,072 | 98,590 |

The photograph saves **1.42 ms / 1.54% raw** against unchanged-path prior evidence,
with separated filtered/all-actual raw confidence. Its reference-normalized point
improves 2.03%, but normalized intervals narrowly overlap. Native timing is 0.50%
slower. The fresh paired direct screen independently suggested 1.37 ms / 1.49%;
it is not a statistical reference comparison. The remaining parity gap stays open.

Ordinary/opaque raw points improve 7.16%/4.88%, while normalized intervals overlap
their preceding wider evidence. Those native clocks are 2.16%/2.82% faster; do not
attribute their full raw differences to this stage. Transparent VP8's adverse
0.61% raw / 0.47% normalized points have overlapping confidence. Diagram remains
a confirmed pass and stops. All adverse points, actuals and brackets are retained;
no scored candidate rerun, retuning or workload expansion.

All four direct controls retain identical allocations and admitted peaks:
photograph 10,166,464 B, diagram 4,802,292 B, transparency 3,847,888 B, small authored
control 312,716 B. No new arrays, owners, retained scratch or admission requests.
Opaque's aggregate benchmark allocation counter falls 816 B/op; this is not a new
payload or capacity saving claim. Physical heap peaks, native allocation and
ordinary/opaque admitted peaks remain unmeasured. Existing trace-storage reuse is
preserved. Allocation totals, admitted live capacity and retention are distinct.

The count audit found 2,145 photograph alpha groups, each with fourteen identical
histograms: 30,030 plane scores become 2,145. These counts motivated the design,
not its speed claim. Every mode histogram sums to the clipped tile area, including
aggregated parents. Checking that area in bin zero proves all other bins zero.
Checking only the first mode is insufficient: an opaque tile may read translucent
neighbors. Exact one/three/four-plane cost composition uses the same integer
helper and format-bounded Int64 arithmetic.

Seven added checks compare every numeric score with the independent dense Shannon
oracle, including large counts and nonopaque prior history; verify all-mode
eligibility, translucent neighbors, clipped leaves and parent accumulation.
Windows/Linux each pass **5,483 full/live checks**, plus 291 affected internal and
194 public checks per normal/scalar mode. Frozen maps, costs, pixels, packets,
cancellation, refusal cleanup and shared ownership pass. The same actual 0.2.1
Release package and all three examples pass both OSes, including the existing
authored opaque regions/mixed-alpha patch and sync/async surviving outputs.

One production/internal build per OS is reused for every check and measurement;
no public, benchmark, selector or native rebuild. Five independent confirmations
retain twelve actuals each and before/after native samples with quiet guards.
Complete normalization, preparation, selection and independent output ownership
remain timed. Libwebp exact=1 is explicitly FFmpeg's encoder backend. Other
affected word/async/method/effort costs remain historical; unrelated codec,
read/edit/text algorithms retain applicable evidence.

This is development qualification, not a complete release refresh of all displayed
comparisons. Publication remains postponed. [Evidence](webp-invariant-alpha-scoring-evidence.json)
pins sources, producers, original controls, complete results and package proof.
Raw webp-invariant-alpha-scoring.tar.xz: 35,227,468 B / 4,504 members,
SHA256 2a268262dcb92ffa5b239174937a0a06cccb846d173d3a7eb417c3e50f231dac,
mirrored/member-verified. Prior production/reference and count-audit archives stay
pinned. The [current coverage](WARM-COVERAGE.md) preserves all 282 required paths
while separating old checkpoint narratives into linked Git history.
