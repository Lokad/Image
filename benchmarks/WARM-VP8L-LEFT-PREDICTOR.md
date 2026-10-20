# Conditional lossless WebP spatial prediction

The fixed photograph shrinks **295,936→184,518 bytes (37.6%)**, a51.8% reduction
from its original383,154-byte packet. Diagram remains **7,636 B**, byte-for-byte.
Both still exceed their160,032/7,234-byte size ceilings. All six affected complete
encodes independently pass warm single-CPU parity, including all-actual confidence.

| Complete encode | Lokad / reference µs | Ratio / conservative upper | Lokad allocated B/op | Encoded B, Lokad / reference |
| --- | ---: | ---: | ---: | ---: |
| Original RGBA8 | 26,142.42 / 121,047.04 | 0.216 / 0.218 | 723,096 | 171,108 / 183,812 |
| Original RGBA16 →8-bit | 32,791.43 / 121,157.50 | 0.271 / 0.273 | 723,096 | 171,108 / 183,812 |
| Original opaque RGBA8 | 12,616.59 / 70,704.68 | 0.178 / 0.180 | 614,408 | 170,156 / 121,896 |
| Photograph RGBA8 | 29,933.56 / 83,170.39 | 0.360 / 0.362 | 702,616 | 184,518 / 160,032 |
| Text/diagram RGBA8 | 6,714.65 / 9,561.10 | 0.702 / 0.717 | 107,568 | 7,636 / 7,234 |
| Original RGBA8, async nonseekable output | 26,517.06 / 120,996.97 | 0.219 / 0.222 | 723,480 | 171,108 / 183,812 |

Reference: **libwebp exact=1, FFmpeg's encoder backend**, method4/lossless quality75.
Inputs, opacity projection,16→8 normalization, exact alpha/hidden RGB and equivalent
complete output ownership/disposal boundaries are unchanged. Async includes staging
and immediate nonseekable delivery on both sides. Startup, IPC, fixture preparation
and validation stay outside both timings. Native allocations are unmeasured.

This is a size/time tradeoff. Photograph mean rises15.93→29.93ms, while allocations
fall1,140,030→702,616B/op (38.4%). Original RGBA8/16 and async packets also shrink
252,476→171,108B, below the183,812-byte reference for these named controls. Their
means and allocations rise as shown. Opaque and diagram bytes remain exact, with
unchanged allocated capacities. No general-image compression or speedup claim.

The independent RFC9649 primitive predicts green-decorrelated pixels from LEFT,
with opaque black at the first pixel and TOP in the first column. On-demand
normalized neighbors avoid a row/frame copy. Source alpha determines the VP8L hint;
residual alpha has different semantics. One uniform mode image uses normative
single-leaf trees, without an auxiliary ImageBuffer allocation.

The existing first pass supplies a cheap literal-density eligibility check. Eligible
input gets a second frequency scan; weighted code bits include reference extra bits
and zero-bit singleton leaves. Serialized-header bytes plus rounded body bits bound
actual payload size within one byte. Prediction wins only with at least three bytes
of upper-bound improvement, covering RIFF alignment/padding. The actual selected
complete packet is then counted against the real output limit before caller writes.
Unselected header costing uses its own finite counting limit so it cannot reject a
smaller valid packet. Nonbeneficial noise preserves its exact accepted C34 packet.

Both candidate trees remain charged during comparison, including old/new overlap.
Unselected capacity is released promptly. Major capacity is reserved before
allocation through the same borrowed context, within the existing64KiB local bound.
Refusal stops the operation without retry, waiting or selecting another mode.
Work/cancellation cover analysis and output; three source passes remain for skipped
prediction, four for considered prediction. Mutable pixels require synchronization
across all passes. No general match search, entropy groups, unsafe/native code,
production dependency or imported upstream implementation/table is added.

Six three-actual screens and six independent12-actual confirmations retain all90
actuals, native pre/post brackets and24 passing official idle-v2 guards. One frozen
production build perOS is reused; **zero harness/selector/native rebuilds**. No
scored job repeats or new image/effort/quality grid. Prepared worker and quality/
ownership checks are reused. All **4,522tests/OS** (1,682internal,1,908public,932live
FFmpeg),22internal+114public focused checks normally/scalar perOS, and the **same
actual private0.2.0 package** consumer/all three examples pass on Windows/Linux.
Checks cover hand-computed borders/modulo channels, one-axis rasters,8/16hiddenRGBA,
source-alpha hints, nonbeneficial mode selection, overlapping-plan refusal,
cancellation/refund/subsequent success, output preflight and surviving owners.

Exactly the existing six lossless encode rows refresh. All other codec, decoder,
edit, text/drawing and inspection paths retain their identified evidence. Frozen
decode packets are not regenerated; existing VP8 source-quality packets reproduce
exactly without retuning.254cases,45current/193historical,72passes/165gaps/
1confidence-open and125independent/113short counts are unchanged. Published0.1.0
is immutable. Size parity, full warm parity and replacement readiness remain open.

Next: the larger JPEG caller-RGB420 size gap,74,040/52,093 B at the existing matched
source quality. Grayscale58,823/58,910 B remains a guard. The remaining lossless
photograph1.153x/diagram1.056x size gates stay required; they are not declared closed.

[Evidence](warm-vp8l-left-predictor-evidence.json) includes actuals, allocations,
producer identities and mirrored hashes for`c35-measurements.tar.gz`,
`c35-qualified-windows.tar.gz`,`c35-qualified-linux.tar.gz`.
Actual package SHA256:`600c5ce9f99e42d41f7f9c8b1d2ce2b6eb43d26c96b5bf94a8887658abdf4664`.
