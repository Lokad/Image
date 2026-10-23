# VP8 encoder preparation

The private managed pipeline reproduces the four method4/Q75 packet identities,
the named diagram method/quality controls, and asymmetric method0/1 packets.
Method4 ALPH is exact from RGBA8/16. All sixteen high-quality ALPH6 candidate bodies
are exact and complete ALPH6 search now passes at its stated work allowance. The public lossy encoder is
unchanged. The named header retry now passes; work/public delivery boundaries and adoption remain open.

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
The smaller controls require zero header-strengthening retries; the named native-positive
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

The same immutable assembly passes475focused checks per platform on Windows/Linux,
normal and SIMD disabled, including the durable method3 packet row. The direct
statistics extension reuses the prior decision/packet checks on that same producer. Shared admission covers
owned capacity, retained results and replacement overlap. Refusal, cancellation,
local/work limits and cleanup/recovery pass; sources and caller roots stay borrowed.
The matrix owner retains3,128/4,128admitted bytes for3/4used segments. Earlier source
analysis/segment owners retain2,835/769bytes on these561macroblocks. These are
accounting capacities; no new timing or GC-allocation measurements are claimed.

[Structured evidence](vp8-byte-parity-evidence.json) records exact producers,
reference observations, scope and preserved raw masters. Prior public codec and
package evidence retains its stated scope. Next are required alpha effort forwarding and public work32M/delivery checks, then integration
and packaged consumer qualification. No readiness claim follows from these private stages.

The bounded option audit adds only settings that exercise a distinct required
policy, reusing the existing diagram and alpha inputs. These are frozen native
targets; source-only qualification is distinct from complete-packet agreement:

| Existing input / settings | Required policy | Native bytes | Managed status |
|---|---|---:|---|
| Diagram Q75 / method0 | Fast source and distortion selection, non-token probe | 9,980 | Complete packet exact |
| Diagram Q75 / method1 | Fast source plus UV reexamination | 9,244 | Complete packet exact |
| Diagram Q75 / method2 | Distortion-only I4/I16 reexamination | 7,330 | Complete packet exact |
| Diagram Q75 / method3 | Basic RDO/token path, non-full SNS | 7,082 | Complete packet exact |
| Diagram Q75 / method5 | Selected-mode luma AC trellis; ordinary Y2 | 7,026 | Complete packet exact |
| Diagram Q75 / method6 | Trellis on every candidate | 6,818 | Complete packet exact |
| Diagram Q1 / method4 | Minimum quality and quantizer clamps | 2,304 | Complete packet exact |
| Diagram Q85 / method4 | Public default quality | 8,898 | Complete packet exact |
| Diagram Q100 / method4 | Maximum quality and high-quality DC policy | 15,172 | Complete packet exact |
| Alpha Q75 / method6 | Internal alpha VP8L quality100 search | 85,796 | Complete packet exact with explicit work allowance |
| Diagram Q100 / method0 | High-quality fast source and distortion-only DC policy | 27,668 | Complete packet exact |
| Alpha Q75 / method0 | Low-effort alpha VP8L | 93,386 | Complete packet exact |
| Alpha Q75 / method1 | Alpha VP8L without cache/trace; small clustering bound | 106,588 | Complete packet exact |

One named large-header case uses the same original procedural formula at
5,632×5,632. Native-positive header strengthening is now confirmed below, with one retry
and an exact complete packet on both platforms. There is no additional dimension grid.

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
Every accepted capture is gated by its unchanged complete native packet. Managed complete-packet agreement now also passes for methods0/1/2. The remaining
named quality, trellis, alpha and header policies precede public integration.
No CPU or GC-allocation measurements were made.

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
the fast high-quality packet now passes; the two alpha confirmations remain open.
The low-quality clustering sampler must retain
its nine-entry candidate capacity even when its greedy threshold is smaller.
No shape/depth/quality cross-product is introduced. The other named policies and
public integration remain open.

Direct non-token statistics now matches the native post-statistics coefficient
probabilities for methods0/1/2. All1,262sampled macroblocks also match complete
nonzero-context inputs:140for method0 and561each for methods1/2. The macroblock
recorder forwards its existing direct/token policy explicitly; the default token
path stays exact. This adds no retained storage or per-block admission. Earlier
native captures provide all targets, so no new encode, fixture or quality search
was needed. Final-loop skip resets and complete non-token packets now also pass.

Historical validation and profiler reports are stored losslessly in mirrored
archives. Original producer, quality, size, timing and stack data are preserved;
restore the original paths before historical replay. Release packages, reference
sources and protected caches remain. This creates build headroom without rerunning
measurements. The generated performance summary still matches retained evidence.

The complete fast packets now match: diagramQ75 methods0/1/2 are9,980/9,244/7,330
bytes; diagramQ100 method0 is27,668bytes. One admitted frame owner performs the
statistics/final passes using rolling borders. Skip syntax resets Y/UV contexts,
preserving Y2 only for I4. The high-quality case also verifies disabled DC
correction and the zero-map-cost branch that leaves default probabilities intact.
Both sync/async packets survive disposed source/frame owners. Owned/scratch/rate
refusal, cancellation, work/local limits, replacement peaks and recovery pass.
These cases are stopped as passing; public lossy encoding is still unchanged.

Method4 diagram qualities1/85/100 now produce exact complete2,304/8,898/15,172-byte
packets. The first two passed on the unchanged prior build and required no tuning.
Only the demonstrated Q100 mismatch needed a policy change: disable chroma DC
correction above quality98. Incoming diffusion errors are ignored and no errors
propagate in that branch; source/result aliasing and cancellation remain covered.
There is no additional allocation or changed admission boundary. The three cases
are stopped as passing. The named alpha/header cases remain.

The private luma trellis primitive matches432native records across the existing36
signed transform fragments, two coefficient-probability profiles and allthree
initial contexts:28,080bytes of nonzero decisions, dequantized coefficients and
quantized levels. I16 AC preserves both DC slots; I4 includes DC. Weights and scan
are generated independently. No implementation tables or upstream source are
production inputs. The instrumented observer retains exact7,026/6,818-byte native
method5/6 packets before its observations are accepted. Y2 and chroma trellis are
disabled in that reference policy and add no required implementation grid.

There is no retained primitive storage; the enclosing operation admits2,048bytes
of bounded stack scratch. Invalid inputs, overlap, tails, cancellation and work
refusal preserve borrowed buffers, with successful recovery. Method5 complete-packet composition now also passes. Method6 all-candidate
composition now also passes.

Selected-mode method5 now reproduces the complete7,026-byte diagramQ75 packet and
all561native macroblock records, including scores, levels, reconstructed pixels,
nonzero contexts, diffusion errors and updated probabilities. Initial RDO modes,
scores and filter statistics remain; final luma reconstruction uses trellis.
I16 refinement advances nonzero edges, while selected I4 reuses the original
edges for every sub-block. A second ordinary UV reconstruction updates its local
six errors while retaining the first selected diffusion edges for the next block.

The first differing macroblock isolated the I4 context rule; correcting that
advance resolved both the full trace and packet. No size/quality search was used.
There is no new retained storage or reservation boundary; the existing16KiB
macroblock scratch allowance covers nested trellis scratch. Frame replacement
peaks, terminal admission refusal, cancellation/late-work/local limits, aliasing,
tails, recovery and independent sync/async output lifetimes pass. Method5 is stopped
as passing on this scope. Named alpha/header cases and public adoption remain.

All-candidate method6 now reproduces the complete6,818-byte diagramQ75 packet and
all561native macroblock outputs/probability profiles on its first screen. Each I16
candidate resets and advances its AC nonzero contexts; each I4 candidate uses the
current selected-block contexts, which advance only after choosing that block.
Walsh/chroma quantization remain ordinary. This method has one UV reconstruction
and no final selected-mode reconstruction. Shared owned capacity and the16KiB
scratch envelope remain unchanged; replacement/refusal/cancellation/work/local
limits, aliases/tails, recovery and independent packet lifetimes pass. The case is
stopped as passing. The named ALPH6, header and public integration gates remain.

Low-effort method0 now matches the asymmetricQ75 RGBA8 complete93,386-byte packet,
including its15,238-byte ALPH payload. The unchanged prior build already matched
the78,102-byte color payload; its hardcoded method4 alpha was the isolated gap.
Alpha now selects effort0/internal VP8L search quality0, preserving exact samples.
FAST filtering also tries NONE when effort exceeds3 or alpha has more than192
colors. The same admitted green workspace, payload ownership, strict ties and
replacement overlap remain. Terminal refusal, cancellation/work/local/output
cleanup, recovery and source-independent sync/async delivery pass. Defaultmethod4
RGBA8/16 packets stay exact; the method0 claim covers the named RGBA8 control.
No native encode, new image, quality search or timing was needed. ALPH6, header
and public integration remain open.

Method1 now matches the asymmetricQ75 RGBA8 complete106,588-byte packet and its
30,101-byte ALPH payload. Its76,440-byte color payload already matched on the
unchanged prior build. The internal VP8L quality8 policy disables cache/trace.
The shared histogram pair pool now covers the sampler's nine candidates separately
from its quality-dependent greedy threshold; one helper sizes allocation and
admission. Sampling starts with at least four live tiles. Defaultquality75 and
alpha4quality32 capacities remain unchanged; lower qualities can reserve more.
Scratch/final-owner refusal, cancellation and late-work cleanup/recovery pass at
qualities8/75, alongside ALPH lifetime/ownership checks. The case is stopped as
passing. The high-quality ALPH6 cruncher, header case and public adoption remain.

Single-family method6/search-quality100 preparation now matches twelve complete
headerless candidate bodies: both existing alpha filters across direct, spatial,
subtract-green, spatial/subtract-green, plain palette and spatial palette families.
Those first twelve use MinDelta for palettes. Each owner retains its entire transform and
coding state, independently of erased/released input. Cached/uncached choices keep
strict earlier ties and skip an uncached retry when caching already selected zero.
The original global red/blue-zero decision is preserved across forced families.

All490 focused checks pass on the same immutable assembly on Windows/Linux,
with intrinsics enabled and disabled. An admission refusal during a later coding
trial disposes the earlier winner and fails the operation. Cancellation, late work
failure, cleanup, retained ownership and subsequent same-root operations pass.
The NONE spatial/subtract-green candidate requires3,295,459,208 accounted work
units: its positive byte proof uses an explicit8B caller allowance; default2B
refusal remains a separate passing safeguard. These are accounting figures,
without CPU or GC-allocation measurements. No limit was raised or work rescaled.

The second palette ordering and four corresponding bodies now pass below;
complete eight-configuration selection remains open. Matching a guessed winning
subset does not establish complete ALPH6 parity. The two native alpha controls,
all sixteen candidate bodies and their final unchanged85,796-byte reference packet
are frozen; further work reuses them without another native encode or image grid.
Completed qualification reports were compacted losslessly with byte verification,
keeping both checkouts below5GB and preserving original replay paths and evidence.

ModifiedZeng ordering now matches both frozen109/256-color permutations and the
four remaining complete candidate bodies. All sixteen family/filter/order bodies
are exact. This algorithm builds four-neighbor adjacency with one previous row,
then orders by accumulated affinity and weighted end placement with fixed ties.
Its matrix, row and bounded ordering spans reserve4*N*N+4*width+16*N bytes before
allocation and release them promptly. Retained palette capacity stays charged.

The same immutable assembly passes502 focused checks on both platforms with
intrinsics enabled and disabled. Tests include exact permutations, every packed
source sample, a hand-calculated tie example, singleton storage, terminal shared
refusal, cancellation, independent local/work limits and successful recovery.
The old boolean palette selector was replaced by an explicit private ordering
enum. Existing Sorted/MinDelta decisions and public routing remain unchanged.
No new native encode, timing or GC-allocation measurement was needed. Complete
search composition, ALPH6 and the named header case subsequently pass below; public adoption remains open.

Complete method6 alpha search now matches the frozen11,667-byte ALPH payload and
85,796-byte asymmetricQ75 RGBA8 packet on its first screen. Every family/order
runs in fixed native order, resetting one admitted trial plane from immutable
filtered input. Old and new complete owners overlap until strict size selection;
losers release promptly. Small source palettes enable Box matching for every
exhaustive family, including non-palette transforms. No encoded-frame copy is added.

The same immutable build passes510 focused checks on both platforms with and
without intrinsics. Refusal/cancellation in the final family and late work failure
discard the earlier winner, preserve caller input/charges and allow recovery.
ALPH output replacement, limits and source-independent sync/async lifetimes pass.
The positive complete preparation consumes17,660,696,844 accounted work units
(and17,660,868,436 including both deliveries), under an explicit64B caller allowance.
Default2B refusal is separately qualified; no limit, work scale or fallback changed.
These are resource-accounting figures, without timing or GC-allocation claims.
No new reference encode or image/settings grid was needed. The named native-positive
header observation and coherent public adoption/qualification remain open.

The named opaque5,632×5,632 Q75/method4 control takes one native header retry,
reducing its limit65,536→32,768. The observer first reproduces the existing72,380-byte
anchor, and independently verifies every generated source sample. The unchanged
managed build already matches all token bytes and the16,787,132-byte packet length;
only three header bytes differ. They describe an unused segment after merging.

A retry now refreshes that unused quantizer from segment0 and recomputes its
initial filter using retained beta. The initial pass keeps its original padding.
The complete packet now matches every byte, SHA256
`bf036683da898b9243c9992ef79060fbd6cf58cd5881f03235d3dd5438d5232f`,
on Windows/Linux with and without intrinsics. All511 focused checks also pass
on the same immutable build. There is no new production storage/admission boundary;
source/frame disposal, retained packet ownership and independent sync/async delivery
are verified under an explicit512B work allowance. These are correctness and
resource proofs, without timing or GC-allocation measurements. The passing header
case is stopped. Public alpha effort2/3/5 forwarding remains a concrete adoption
boundary: public options accept these methods, while the private alpha factory
currently rejects them. Existing input/settings/reference tools will be reused.
