# VP8 encoder preparation

The private managed pipeline reproduces the four method4/Q75 packet identities
and the method3/Q75 diagram packet, including exact method4 ALPH from RGBA8/16. The public lossy encoder is unchanged. Required
options, the named header/work boundaries and public integration remain open.

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
aliased output and tails pass. That isolated check qualifies supplied-neighbor arithmetic; composed checks below
add actual reconstructed-neighbor choice during RDO.

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

Composed basic-RDO decisions and rolling state match2,011,746native before/after
bytes on the existing opaque and diagram controls:1,122macroblocks. This includes
all coefficients, reconstructed pixels, captured scores, modes and nonzero/diffusion
state. The diagram adds353selectedI16 blocks,255skips, flat regions and allfour UV
modes. Allsix adaptive probability snapshots per control match. Native captures
are accepted only after their unchanged complete72,380/7,100-byte packets match.
Offline tests regenerate every input and predecessor state without oracle files.

Counter rescaling arithmetic, update-header costs and the explicit token-buffer
statistics policy pass focused checks. Borrowed macroblock inputs are snapshotted;
aliases, tails, invalid layouts, cancellation and late work failure pass. The
caller admits16,384scratch bytes once, without per-block owners or reservations.
Final frame-filter adjustment is now qualified by the complete packets below.

The private frame/partition pipeline reproduces allthree complete opaque packets:
72,380/7,100/17,688bytes for the procedural/diagram/photograph controls. Sources
and settings are unchanged. Final coefficient refresh, segment/mode headers,
filter levels and canonical partition termination match. The native VP8 chunk
counts its final zero padding internally; this declaration is also exact.

The admitted frame retains coefficients/modes/probabilities, with rolling borders
instead of a reconstructed image. Owned partitions deliver directly to sync/async
streams without a joined frame copy. Input/frame owners can release first; packet
charges persist through actual asynchronous users, including cancellation. Refusal
and failed growth copying refund unused capacity without retry or fallback.
Allcurrent controls require zero header-strengthening retries; a native-positive
large-header confirmation remains an explicit required coverage item.

The fourth complete packet now matches98,590bytes from the existing transparent
RGBA8 and normalized RGBA16 inputs. Its24,588-byte ALPH payload is also exact.
Default filter estimation compares NONE and one predicted filter, with strict
size ties. Alpha is carried in green with A/R/B zero; shared VP8L preparation
uses quality32/method4 and emits its headerless body directly. Retained storage
is only the selected payload plus128accounting bytes; one temporary green-only
workspace and coding owners release before delivery. Replacement capacity
includes both old and new payloads. This is a private pipeline, not public lossy
encoder adoption or a claim about other encoder settings.

Alpha and color deliver directly through VP8X/ALPH/VP8 framing, including sync
and async output. They survive disposed input/frame owners. A blocked actual
alpha-buffer write remains charged through cancellation or stream failure until
its task finishes. Terminal admission refusal, cleanup/recovery, local/work/output
limits and mismatched dimensions pass. Quality75 lossless packet checks also pass
with the shared preparation changes; the scoped native inputs/settings stay fixed.

The same immutable assembly passes430focused checks per platform on Windows/Linux,
normal and SIMD disabled, including the durable method3 packet row. The direct
statistics extension reuses the prior decision/packet checks on that same producer. Shared admission covers
owned capacity, retained results and replacement overlap. Refusal, cancellation,
local/work limits and cleanup/recovery pass; sources and caller roots stay borrowed.
The matrix owner retains3,128/4,128admitted bytes for3/4used segments. Earlier source
analysis/segment owners retain2,835/769bytes on these561macroblocks. These are
accounting capacities; no new timing or GC-allocation measurements are claimed.

[Structured evidence](vp8-byte-parity-evidence.json) records exact producers,
reference observations, scope and preserved raw masters. Prior public codec and
package evidence retains its stated scope. Next are the bounded required-option,
work32M and native-positive header-retry confirmations, then public integration
and packaged consumer qualification. No readiness claim follows from these private stages.

The bounded option audit adds only settings that exercise a distinct required
policy, reusing the existing diagram and alpha inputs. These are frozen native
targets; source-only qualification is distinct from complete-packet agreement:

| Existing input / settings | Required policy | Native bytes | Managed status |
|---|---|---:|---|
| Diagram Q75 / method0 | Fast source and distortion selection, non-token probe | 9,980 | Decisions/statistics exact; emission open |
| Diagram Q75 / method1 | Fast source plus UV reexamination | 9,244 | Decisions/statistics exact; emission open |
| Diagram Q75 / method2 | Distortion-only I4/I16 reexamination | 7,330 | Decisions/statistics exact; emission open |
| Diagram Q75 / method3 | Basic RDO/token path, non-full SNS | 7,082 | Complete packet exact |
| Diagram Q75 / method5 | Selected-mode trellis, including I16/Y2 | 7,026 | Open |
| Diagram Q75 / method6 | Trellis on every candidate | 6,818 | Open |
| Diagram Q1 / method4 | Minimum quality and quantizer clamps | 2,304 | Open |
| Diagram Q85 / method4 | Public default quality | 8,898 | Open |
| Diagram Q100 / method4 | Maximum quality and high-quality DC policy | 15,172 | Open |
| Alpha Q75 / method6 | Internal alpha VP8L quality100 search | 85,796 | Open |
| Diagram Q100 / method0 | High-quality fast source and distortion-only DC policy | 27,668 | Open |
| Alpha Q75 / method0 | Low-effort alpha VP8L | 93,386 | Open |
| Alpha Q75 / method1 | Alpha VP8L without cache/trace; small clustering bound | 106,588 | Open |

One named large-header case uses the same original procedural formula at
5,632×5,632. Native-positive header strengthening is still unconfirmed; observe it
before assuming an optimization is needed. There is no additional dimension grid.

The existing32M-work call exhausts source analysis at31,999,818consumed units on
the frozen E80 assembly. Complete color-only encoding/delivery costs297,778,565
units, excluding ALPH/container; its73,956-byte VP8 payload is exactly native.
Both runs release all owned storage before context disposal. These counts describe
deterministic admission, not CPU performance. The32M boundary stays a required
structured-failure/cleanup check at public adoption. Its older successful benchmark
row retains its original producer scope; failed operations must not become timing
or packet passes. No work rescaling, limit increase or alternate-encoder retry is
introduced. Full default-operation comparisons remain required.

Fast method0/1 source analysis now matches every pre-segmentation record and
histogram on the existing Q75 diagram:561macroblocks per method,5sourceI4 and
556I16 decisions, global means244/42. It compares dispersion of sixteen padded
4×4luma-cell sums and reuses the qualified chroma analysis. Source DC4 is stored
in the existing mode plane; retained/scratch capacities remain2,835/1,024bytes.
Default method4 source analysis and all earlier packet checks remain exact.

The development observer captures iterator advancement before segmentation;
observing only after analysis would report centroid-substituted susceptibility.
Every accepted capture is gated by its unchanged complete native packet. Managed
fast-source agreement is not yet full method0/1 packet agreement. Distortion
refinement and non-token statistics/emission remain next, followed by the other
named policies. No CPU or GC-allocation measurements were made.

Distortion-only method0/1/2 selection now matches all2,945native statistics/final
pass decisions on the existing Q75 diagram, including modes, coefficients,
reconstructed pixels and decision scores. Initial segment parameters/matrices also
match. Original prediction SSE and fixed header biases drive these decisions;
selected blocks provide their reconstructed neighbors. This path performs local
UV DC correction without propagating errors to adjacent macroblocks. Result aliasing,
tails, refusal, cancellation, late work failure and cleanup/recovery pass.

The unchanged production build also reproduces the complete7,082-byte method3
packet through sync/async delivery after source/frame disposal. Its non-full-SNS
matrix policy was the remaining difference. Only11,008accounting bytes remain
owned during delivery; this is retained capacity, not measured GC allocations.
The case is stopped. A durable packet regression is added for full CI; the local
same-build proof reused429checks and the small packet probe without rebuilding.

Inspection identified three specific omitted boundaries in the table: high-quality
fast encoding, low-effort alpha, and non-low-effort alpha below the cache/trace
threshold. They reuse the same images; allthree native targets are frozen, and
managed confirmation remains open. The low-quality clustering sampler must retain
its nine-entry candidate capacity even when its greedy threshold is smaller.
No shape/depth/quality cross-product is introduced. Non-token statistics/skip
emission is next; full nondefault packet parity and public integration remain open.

Direct non-token statistics now matches the native post-statistics coefficient
probabilities for methods0/1/2. All1,262sampled macroblocks also match complete
nonzero-context inputs:140for method0 and561each for methods1/2. The macroblock
recorder forwards its existing direct/token policy explicitly; the default token
path stays exact. This adds no retained storage or per-block admission. Earlier
native captures provide all targets, so no new encode, fixture or quality search
was needed. Final-loop skip resets and complete non-token packets remain open.

Historical validation and profiler reports are stored losslessly in mirrored
archives. Original producer, quality, size, timing and stack data are preserved;
restore the original paths before historical replay. Release packages, reference
sources and protected caches remain. This creates build headroom without rerunning
measurements. The generated performance summary still matches retained evidence.
