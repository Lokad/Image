# VP8 encoder preparation

The private managed pipeline is verified through quantization-matrix setup.
The public lossy encoder is unchanged; complete VP8 packet replication remains open.

Seven existing encoding rows have four reference packet identities:

| Original 513×259 input | Existing rows | Complete reference bytes |
|---|---|---:|
| Opaque procedural | RGBA8 / RGBA16 | 72,380 |
| Alpha procedural | RGBA8 / RGBA16 / work-budget boundary | 98,590 |
| Photograph | RGBA8 | 17,688 |
| Text/diagram | RGBA8 | 7,100 |

References remain FFmpeg n8.1/libwebp1.6.0, quality75, method4, preset none,
one codec thread and BGRA delivery. There is no WebP metadata exemption.
Frozen packets, original inputs and settings are reused without a quality sweep.

Input conversion matches six complete native logicalYUVA streams: the four
rasters plus existing singleton/two-color controls. Both original16-bit procedural
inputs normalize identically. Mathematical color/gamma generation introduces no
upstream table. A single admitted owner retains padded planes and exact alpha;
normalization happens on access, without an RGBA frame copy. In-place transparent
cleanup matches native exact=0. One original33×17 control covers complete8×8 runs,
resets and partial edges; alpha and visible luma remain exact.

The forward4×4DCT matches215,424native coefficients from the opaque control
against neutral128 prediction, plus36signed/extreme fragments. Y2Walsh matches
8,976coefficients and36signed/odd-half fragments. Histograms match1,122native
luma/UV summaries and ten population fragments. Large-block prediction matches
861,696candidate pixels across allfour modes at16×16Y/8×8UV, including missing
borders, clipping and edge replication. Native SIMD/scalar streams agree.

Composed source analysis matches8,976native candidate shapes on allfour rasters,
then exact derived susceptibility, seed modes, distributions and means. This uses
original source borders, preceding reconstruction and RDO. Selection/mixing is
separately inspected encoder policy. Weighted centroid clustering and initial
quantizer/filter setup reproduce all2,244native segment-map bytes and16quantizers.
One original5×3 majority fragment verifies smoothing without neighbor feedback;
frozen controls use preprocessing0. Final frame filtering remains a later gate:
reconstructed Y2edges raise some photograph/diagram filter levels.

The admitted matrix owner matches all3,360entries of42used native matrices,
including reciprocals, rounding biases, zero thresholds and generated sharpening,
plus140rate/distortion cost scalars and the quantizer deltas. Native observations
call parameter setup on zero macroblocks with the qualified susceptibility inputs;
this qualifies matrix setup, not independent full analysis or actual encoding.
Normative quantizer steps and independent formulas replace implementation tables.

The same immutable assembly passes166focused checks per platform on Windows/Linux,
normal and SIMD disabled, including all157earlier checks. Shared admission covers
owned capacity, retained results and replacement overlap. Refusal, cancellation,
local/work limits and cleanup/recovery pass; sources and caller roots stay borrowed.
The matrix owner retains3,128/4,128admitted bytes for3/4used segments. Earlier source
analysis/segment owners retain2,835/769bytes on these561macroblocks. These are
accounting capacities; no new timing or GC-allocation measurements are claimed.

[Structured evidence](vp8-byte-parity-evidence.json) records exact producers,
reference observations, scope and preserved raw masters. Prior public codec and
package evidence retains its stated scope. Next are quantized/reconstructed
coefficients, macroblock decisions, entropy/framing and ALPH, followed by complete
packets and the bounded required-option audit. No readiness claim follows from
these private stages.
