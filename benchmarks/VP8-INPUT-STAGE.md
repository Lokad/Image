# VP8 encoder preparation

The private managed pipeline is verified through block reconstruction, prediction, distortion and entropy-rate scoring.
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

Block quantization matches914,485native output bytes across14,025reused DCT/Walsh
blocks from the opaque control and44signed/threshold fragments: nonzero flags,
quantized zigzag levels and reconstructed coefficients. A format-minimum-step
arithmetic probe covers the2,047level cap without another image or encoder setting.
Native SIMD/scalar output agrees. The managed kernel derives diagonal scan ranks,
borrows its matrix/caller buffers and preserves tails. Invalid coefficient ranges
or overlapping outputs fail before mutation; no per-block heap or admission exists.

Pixel reconstruction reuses the existing inverse DCT/Walsh functions unchanged.
All235,136native output bytes match:216,032pixels and9,552signedDCvalues from the
same control,72existing valid transform fragments and one paired-block probe.
The wrappers preserve coefficient inputs, row padding and AC slots with64/32-byte
stack snapshots. Aliased caller buffers and compact/native strides pass; incomplete
or overflowing layouts fail before writes. This qualifies the tested encoder
domain, without a broader decoder-range or actual RDO claim.

Allten4×4predictor modes now match8,000native pixel bytes on50bounded neighbor
contexts from the same image and two original rounding/clipping fragments. The
span entry reuses shared geometry unchanged, with explicit encoder/geometry
ordering for the three diagonal modes. Neighbors are snapshotted in84stack bytes;
aliased output and tails pass. This qualifies supplied-neighbor arithmetic,
not actual reconstructed-neighbor choice during RDO.

Squared-error and weighted-Hadamard texture scores match79,240native result bytes
on561opaque-control macroblock pairs and five original arithmetic pairs. Analytic
Gaussian importance generation reproduces the16-value profile without an imported
table. Texture rounding and absolute differences apply per4×4tile; reversed-tile
controls distinguish that rule from global accumulation. Independent strides,
maximum error, source preservation and invalid-layout rejection pass.

Generated rate costs match all16,752native table words and138,276residual costs
under normative defaults and one original probability profile including0/255.
The23,046cases reuse the same opaque coefficients and prior arithmetic fragments,
adding AC-only companions and one sparse last-position/category probe. Entropy
prices derive from boolean-range cycles; mode/level costs derive from normative
trees. Two native pricing quirks are explicit: UV branch inversions and omitted
low category-two cost for9/10. These affect decisions, not emitted token syntax.
One admitted20,192-byte owner retains costs without source/probability references;
1,024temporary stack bytes are separately charged and released after generation.

The same immutable assembly passes213focused checks per platform on Windows/Linux,
normal and SIMD disabled, including all207earlier checks. Shared admission covers
owned capacity, retained results and replacement overlap. Refusal, cancellation,
local/work limits and cleanup/recovery pass; sources and caller roots stay borrowed.
The matrix owner retains3,128/4,128admitted bytes for3/4used segments. Earlier source
analysis/segment owners retain2,835/769bytes on these561macroblocks. These are
accounting capacities; no new timing or GC-allocation measurements are claimed.

[Structured evidence](vp8-byte-parity-evidence.json) records exact producers,
reference observations, scope and preserved raw masters. Prior public codec and
package evidence retains its stated scope. Next are actual reconstructed-neighbor
macroblock decisions, entropy/framing and ALPH, followed by complete
packets and the bounded required-option audit. No readiness claim follows from
these private stages.
