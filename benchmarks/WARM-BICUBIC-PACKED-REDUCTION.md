# Packed byte bicubic reduction

The existing tight-workspace RGBA8 32×16 bicubic operation improves **19.2%**,
with exact pixels and unchanged allocation strategy. Its **1.266×** mean remains
above parity; the final result is a qualified short, not a passing confirmation.

| Stage, same complete operation | Lokad / reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Packed reduction / intermediate | 554.02 / 377.57 | 1.467 / 1.552 | 2,169 |
| Generic constructor / rejected | 1,004.80 / 376.99 | 2.665 / 2.794 | 2,170 |
| Explicit insert / final qualified | 477.99 / 377.49 | 1.266 / 2.275 | 2,169 |

Two alternating horizontal tap streams accumulate in the existing eight double
lanes and fold once per source row. At most 37 dependent additions replace the
previous 72; the original 72-term error certificate remains conservative. Exact
products and weights, vertical reduction, normalization, quotient and rounding
guards, original-order local replay and tiny-alpha unwind remain. Explicit vector
broadcast and upper-half insertion preserve every coefficient lane bit. There is
no new heap array, field, table, workspace reservation or logical work charge;
cancellation, admission, source preservation and independent owners remain exact.

The generic-constructor intermediate mistakenly omitted the explicit insert in
the already accepted word-kernel recipe. Its 2.665× result is retained and rejected.
One correction applied the actual recipe. All three stage means were read before
qualifying the final source; no constructor, width, inlining or precision grid
followed. Each changed producer was built once per OS per stage; only the final
source received full qualification and package verification.

Both sides measure complete operations on the original 513×259 RGBA8 input
(531,468 B), producing an independently owned 32×16 output (2,048 B). The managed
working limit is input + output + 4 KiB. The approved native reference is zimg,
explicitly labeled as FFmpeg's zscale backend: timed 32-pixel halo construction,
normalization/premultiplication, float filtering, unrounded-alpha unpremultiplication,
quantization, zero-alpha RGB clearing and fresh planar/packed output acquisition
and release. Native contexts are reused, with one thread. Existing RGB/alpha
maximum 1 and RMSE 0.25 gates pass; managed pixels match the independent rational
reference and original output exactly. Native allocation is unmeasured. Managed
B/op includes amortized harness overhead; compressed sizes do not apply to edits.

One CPU, normal SIMD, tiering off, nonconcurrent workstation GC. Three short jobs
retain all nine 200ms actuals, 50ms native pre/post samples and both 5s idle-v2
guards. Upper is the larger selected/all-actual bound against the conservative
native lower bracket. Every mean exceeds 1, so no long confirmation was warranted.
The harness, selector, native worker and authored fixture were reused unchanged.

All **3,726 Release tests per OS** pass: 1,391 internal, 1,437 public and 898 live
FFmpeg comparisons. Each stage passes 130 focused public checks per OS. The final
10 affected public checks also pass with intrinsics disabled on both OSes. The
new independent BigInteger pixel test covers two adversarial color/alpha patterns;
existing odd/even taps, negative lobes, tiny alpha, limits, cancellation, ownership
and large absolute-coordinate tests remain. The **same actual private Release
package** passes its consumer and all three examples on Windows and Linux.
Measured producer bytes remain unchanged through qualification.

[Numeric evidence](warm-bicubic-packed-reduction-evidence.json) retains every
stage, exact source reconciliation, guards, reference and package hashes. Exactly
one selected timing changes; other 206 comparisons and all 62 passing proofs retain
their paths. The finite 207-case objective remains open: 62 passes, 144 mean gaps
and one confidence case, with 124 independent selections and 83 qualified shorts.
