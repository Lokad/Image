# Material warm coverage

Updated 2026-10-24. The finite scope is **257 named cases: 240 numeric and
17 unmeasured**. All previous 256 controls and frozen inputs remain. Current
verification counts come from the [three-table summary](../docs/benchmark.md).
Warm single-CPU parity and replacement readiness remain open.

[E152 sparse JPEG traversal](WARM-JPEG-SPARSE-SCAN.md) confirms governedRGBA16
at2.791×/upper2.804, saving17.0% against its preceding confirmation. Current
ungovernedRGBA16/RGBA8 screens are2.732×/2.611×. The previously advertised gray
0.962× pass predates the raw-exact grayscale encoder adoption in3f36e3f and is
invalid for current code: the existing raw-exact58,910-byte gray control now
demonstrates6.973×. It is the largest current gap. Older caller-gray and affected
JPEG timing rows are prior-production. No cases were added. Both platforms pass
5,074 checks and the same actual private package. Other codecs/decoders/edit/text
reuse unchanged-path evidence. The finite257case/1× objective remains open.

[E149 exact vector quantization](WARM-VP8-AVX2-LEVELS.md) confirms the existing
RGBA8 VP8 control at3.109×/upper3.128, with7.8% less elapsed than the preceding
confirmation, unchanged24,080,952 B/op and exact98,590-byte packets. Both
platforms pass5,072 checks and the same actual private package. No cases were
added. JPEG/PNG/VP8L and all decoder/editing/text paths reuse unchanged evidence;
older affected VP8 encoder timings remain historical.

[E147 AVX2 row reductions](WARM-JPEG-AVX2-ROWS.md) reconcile the current color-JPEG
anchors without adding cases: governed RGBA16 confirms 3.366×, with 803,968 B/op,
exact packets and 8.35% less elapsed time than its preceding screen. Ungoverned
RGBA16/RGBA8 have current 3.390×/3.218× screens. Both platforms pass 5,068 checks
and the same actual private package. Other affected color timings remain marked
prior-production; unchanged grayscale/other paths reuse their relevant evidence.

[MemoryBudget/Jpeg420Rgba16](WARM-JPEG-COOPERATIVE-OWNER.md) is the one added
required boundary: E136 introduced private coefficient storage for governed
JPEG encoding, so pooled ungoverned evidence and the coarse `ImageBudget` quota
cannot qualify it. It reuses the original resident RGBA16 input, settings and
complete normalized encode/output operation. Its initial guarded screen demonstrated
a 3.667× CPU gap with 803,968 B/op and exact packets under the approved COM
exception. Root borrowing/refunds, terminal refusal/recovery, cancellation and
independent surviving outputs pass. Production is unchanged; its existing full
qualification remains valid. No additional image/depth/settings grid is added.

Declared codec byte controls remain qualified at their pinned scopes; see
[decoder evidence](DECODER-EXACTNESS.md) and [encoder scope](../docs/codec-parity.md).
Resolve uncertain current measurements before optimizing demonstrated gaps,
one substantial primitive at a time. Stop passing unchanged cases. The earlier
entries below retain their recorded producer scopes and historical counts.

E115/E116 resolves four existing current encoder anchors with packet-exact smoke
and guarded short screens, reusing the qualified production DLL. JPEG420 is13.763×,
VP8L5.004×, VP8 4.701× and PNG Optimal1.460× their references. Allocation and
complete packet sizes are in [the current cost report](WARM-CURRENT-ENCODER-COSTS.md).
These are single screens, not independent confirmations or speedup claims. JPEG
is the next primitive target; no workload grid or repeated production qualification.
The VP8L reference helper had one necessary provenance refresh for a CLI-only
method selector, with timed operation/backend/settings and packets preserved.
The summary restores distinct VP8L decoder rows mistakenly duplicated by a label
edit; their unchanged original evidence remains valid.

E113 resolves the current fresh-Stream VP8 cost using the same98,590-byte input,
unchanged native worker/settings and E112 production DLL. All three existing
VP8 packets now have exact normalized pixels; the worker replaces former managed
hashes/error bounds with exact equality. One smoke and one guarded short screen
reuse the prepared BDN host:10,523.95µs versus5,981.68µs,1,966,591B/op, ratio1.759
(upper1.903). No independent confirmation or optimization claim. Other four VP8
timing rows remain prior-production; metadata timing uncertainties decrease31→30.
Screen current encoder costs before selecting smaller decoder work.

The following inventory retains its earlier producer milestones; the current
finite scope and verification counts are stated above.

E62 adopts the complete managed VP8L encoder:19 selected packet identities plus two
additional forced spatial/box candidates (21 distinct packets). The25 existing
selected settings, original8/16bit and public sync/async/ImageBudget boundaries pass. The
photograph/diagram are exactly160032/7234B on their declared controls. BothOS pass
4865 full/live tests and27 affected scalar checks using one immutable worker set.
A test-only extension against the unchanged core passes26 internal/27 public
packet checks and27 scalar packet checks, adding the existing16383 axes. The SAME actual private0.2.0 package and all3examples pass. Six previous VP8L encode
rows are superseded for CPU/allocation claims; no new timing or workload grid.
Decoder inputs remain frozen and unchanged decoder/other-code evidence is reused.
The chronology below records its original producers, rather than current behavior.

E4/E6 add an explicit managed JPEG Reference profile: all six original444/422/420 controls are
byte exact (with the single approved attribution change) through image/caller,
seekable/forward-only and async boundaries. It uses bounded MCU storage and the
shared root. Six small functional controls cover paired RGB input and tiny filters;
these add no timed case. Its CPU/allocations are unmeasured; no performance pass is inferred.
Existing JPEG encoding observations predate the added dispatcher and require
affected confirmation when CPU qualification resumes. Other unchanged paths retain
valid evidence; the numeric counts below describe their recorded producers.
These paragraphs retain historical producer evidence; unpublished encoder branches and switches carry no compatibility obligation.

[C193 JPEG444 coded source histograms](JPEG444-CODED-SOURCE-HISTOGRAMS.md) save
65 B on RGBA8 and add one byte on RGBA16: 134,676/133,041 and 134,688/133,043 B.
Net259 B across five rows removes3.07% of prior excess; FIVE packet gates remain
open (maximum1.24%). Source-PSNR gaps0.218264/0.226798dB pass the unchanged0.25dB
quality/exactalpha/decoder gates. Both OS pass5,067 tests and237+62 scalar checks
each; exact C192 packets across public boundaries and the SAME actual private0.2
package/all3examples pass. One core/internal/public build passes first time and
supplies the SAME frozen binaries across OS; no production rebuild after freeze.
Benchmark/native/differential/driver/parser binaries are reused.
One complete forward444 cost is1,929,269.703us /644,112Bop; prior C191 is historical.
No repeated timing confirmation, CPU-parity pass or speed claim. Only source
histogram amplitude units change; fine training choices/samples/bands and final
RDO/error/DQT/reference/settings remain fixed. The128byte lookup keeps source
scratch7336<8192;1920 extra setup work plans1999982933<unchanged2B.
All decision-enabled Original three-component444 qualities1..98 change coefficient
decisions/lossy pixels/packets/encode costs, INCLUDING baseline/fixed/restart,
caller samples/async and containing invocations. The five packet rows and forward
cost supersede C191; other affected timings remain historical. Qualities99/100,
other policies/sampling, native/decode/edit/inspect/PNG/WebP retain evidence.
Decoder fixture packets stay frozen; no new finite case or workload grid is added.

[PNG Optimal RGBA8](PNG-OPTIMAL-RGBA8.md) closes the two sync/async packet gaps:
209,407/210,973 B, exact RGBA/alpha/hidden RGB and9,368 B savings. Optimal is a
library policy:actual managed9/native FROZEN6. All17 declared PNG packet gates
pass; other15 paths retain evidence. A5% IDAT/timing pass is not packet parity.

C161 passes5,000 tests/OS,465+157 PNG scalar checks/OS and the same actual private
0.2 package/all3examples on both OS. One frozen core/OS and one affected portable
benchmark build, shared across OS. Native worker and identical PNG driver reused.
Normal/scalar actual default9 sync/async packets match the independent candidate;
RGBA16 remains byte-identical. Resource/admission/cancel/failure/ownership probes
pass. Decoder input bytes remain frozen.

One sync cost screen records14,417.461 us/reference17,231.327 us,527,580 B/op,
ratio0.837/upper0.885; both idle-v2 guards pass. It remains a short cost, without
independent confirmation. Prior default6 sync cost5,406.297 us is superseded;
old default8 async timing is historical and awaits later CPU qualification.
Compound PNG8 crop/nearest/encode invocation costs also contain changed default
encoding and remain historical. Other15 PNG paths and WebP costs retain
validity; JPEG reuse follows the C193 scope above. No whole-library method audit
or repeat is needed.

[Default420](JPEG420-AC-TRELLIS.md) retains nine packet passes; optionalCompactColor
stays separate. C193 updates the five444 packets and forward cost above;
C146caller42068,162.147 us/170,648 Bop remains valid on its unchanged path.
All7 lossy and6 lossless WebP packet gates pass/stay stopped. No broad JPEG,
photographic or diagram compression claim follows from procedural controls.

Only two existing warm rows change for C161:sync becomes a current cost screen,
async becomes superseded. Earlier4other444/8other420/2other422 superseded rows
remain historical. Another31 metadata confirmations remain pending, and PNG typed
reference fields need equivalence first. Exact pixels, matched lossy quality,
complete operations, shared admission, cancellation, ownership and quiet-VM
confidence remain mandatory. CPU-parity enforcement waits while JPEG compression
gates remain open.

Earlier qualification milestones below retain their historical producer scopes.


Updated 2026-10-20. The **warm single-CPU parity objective remains open**:
68 independent passes,167 measured mean gaps, one confidence-open case and three
compression-first short cost screens across
239 numeric rows. The [three-table summary](../docs/benchmark.md)
contains120 independent observations and119 qualified shorts.17named required
boundaries remain unmeasured. Reuse unchanged passing paths; no image/settings grid.
Compression now precedes CPU parity: lossy WebP first, then lossless WebP.
Accept verified incremental packet savings within unchanged quality/resource gates;
CPU costs are recorded, with parity enforcement and speed-only work deferred.

The finite scope is now **256 cases:239 numeric+17 unmeasured**. After [typed source metadata](../docs/source-metadata.md),23 unaffected current
rows and216 historical/awaiting-confirmation rows remain. Exactly31 formerly
current decode/inspection rows need confirmation because their metadata factories
and header preflight changed. The [named invalidation list](source-metadata-qualification-evidence.json)
identifies every case; no image/settings matrix expands. One PNG metadata-returning
reference also needs equivalent typed fields. Existing pixel/resource proofs and
unaffected encoders/edits are reused. No timing is claimed for this functional round.
All4571tests/OS,259affectedscalar/OS and the same actual private package/three
examples pass. The numeric pass/gap counts describe their retained tested producers,
not an assertion that the changed complete-operation timings are already qualified.

The [exact SIMD forward DCT](WARM-VP8-FORWARD-DCT.md) refreshes exactly seven
existing VP8 encode rows with current production evidence: five independent time
passes and two opaque short gaps. Diagram becomes a pass at0.831x/upper0.834x;
photograph remains passing at0.679x/upper0.684x. Coefficients, packets, quality,
alpha, work/admission and owners stay exact. All4535tests/OS,169affectedscalar/OS,
and the same actual private package/three examples pass. The finite scope is
unchanged:255cases,239numeric+16unmeasured,73passes/165gaps/1confidence-open,
125independent/114short,54current/185historical. These seven encoder paths changed;
unchanged decoders/lossless/other codecs/edit/text retain their identified scopes.
Both photograph/diagram compression-size gates remain open.

The [bounded lossy WebP audit](WARM-VP8-COMPRESSION-AUDIT.md) changes no
production row or count. Skip-only, top-two/all-six reconstructed-rate candidates
and one existing full-mode coverage diagnostic all miss the fixed size gates.
No scored timing or package qualification follows those screens; C37 source and
build are restored exactly. The subsequent [intra16/Y2 screen](WARM-VP8-INTRA16-SCREEN.md) also fails both
size gates and is slower; its prototype/format proof are retained. Both content
gates remain open; this rejection does not change the seven accepted DCT results.

The [optional CompactColor JPEG policy](WARM-JPEG-COMPACT-COLOR.md) adds exactly
one required algorithm case, `JpegSamplesEncode/CompactColorRgb420Resident`:
the same borrowed packed RGB samples and complete owned JPEG boundary with an
explicit new quantization policy. It produces48701/52093B at sourcePSNRgap
0.017274dB, with short1.126x/upper1.362x and162110B/op. The Original RGB420 row
remains separately open; its74040B packet is unchanged. The existing caller-gray
row refreshes at1.160x/upper1.394x,155026B/op and unchanged58823/58910B. Both are
qualified shorts, not independent passes. At the C37 checkpoint:47current/192historical rows;
255cases,239numeric/16untimed,72passes/166gaps/1confidence-open,
125independent/114short. All4534tests/OS,12newpublic normal/scalar checks/OS and
the same actual private package/examples pass. Existing default quantizers,
grayscale, other codecs/decoders and confirmed paths retain their identified
evidence. Its subsequent bounded lossy audit and DCT round are recorded above; no grid.

The [conditional LEFT predictor](WARM-VP8L-LEFT-PREDICTOR.md) refreshes those same
six current lossless encode rows, all independently passing time parity. Photograph
184518/160032 B remains1.153x; diagram7636/7234 B stays byte-for-byte1.056x.
All4522tests/OS,22internal+114public normal/scalar checks/OS and the same actual
private package/examples pass. Both size gates remain open; the larger JPEG RGB420
compression gap was next at that checkpoint. No scope/count expansion or decoder fixture regeneration.

The [bounded VP8L color-cache round](WARM-VP8L-COLOR-CACHE.md) replaces five
historical synchronous encodes and the already-current async encode with six
independent time-parity confirmations. Exactly45current/193historical rows;
all254cases,72passes/165gaps/1confidence-open and125independent/113short counts
are preserved. Photograph/diagram at that cache checkpoint295936/7636B versus160032/7234B: both size
gates remain open. All4509tests/OS,18prefix+105public normal/scalar checks/OS and
the same actual private package/examples pass. No decoder fixture regeneration,
reference change, new workload grid or retuning of unaffected passing paths.

The [current resident PNG reconciliation](WARM-CURRENT-RESIDENT-PNG.md) replaces
the two historical default RGBA8/16 None-filter rows with current qualified shorts.
Original packets, exact frames/owners, native normal/scalar controls and all four
score guards pass. No code, harness, selector, native or package builds. Both means
remain above parity, so no longer repeat. Only these two historical rows become
current at that checkpoint:40 current/198 historical; other PNG boundaries retain their named scope.

The [paired-integer Lanczos byte trial](WARM-LANCZOS3-PAIRED-BYTE-TRIAL.md)
parks the small3.5% point-mean gain; once-prepared coefficient packing is neutral.
Exact pixels, independent integer envelopes, retained ownership and all four
score guards pass. Both experimental sources/binaries are archived; accepted
production/tests are restored exactly. No selected row, finite count or current
qualification changes; no full/package repeat for parked code.

The [materialized Deflate cursor](WARM-DEFLATE-MATERIALIZED-CURSOR.md) now improves
the two current PNG decode rows: two retained outputs1.265x/upper1.304x,
2,574,671B/op (15.8% faster), and async input1.113x/upper1.176x,12,784B/op
(23.2% faster). All4,481 tests/OS,122Zlib+36root controls normally/scalar,
178affected public scalar checks, and the same actual private package/examples
pass. Both short pairs pass all pixels/owners/quiet guards; no longer repeat
above one. Other PNG rows reaching changed `Walk` remain historical, including
count-only/cancelable routes. Metadata, encoding, `WalkRgba` and other codecs/
editing retain their identified scopes. No new finite case or matrix is added.

The [VP8 helper for cancelable tokens](WARM-VP8-CANCELLABLE-BLOCK.md) then improves
the existing live case 11.6% to 1.395x/upper1.437x at unchanged 760,572B/op.
Every original decision still polls cancellation; exact completed-work/cursor
publication includes canceled exits. All five existing VP8 rows refresh because
their shared dispatcher changes; four control point means are 0.5–1.8% slower.
All pixel/quality/owner/quiet gates, 4,484tests/OS,132VP8+36root normal/scalar,
305public scalar controls and the same actual package/examples pass both OSes.
No long above one or new case/settings grid. The finite counts stay unchanged.
[Lanczos3 functional qualification](../docs/lanczos3-qualification.md) adds four
required representatives using the same513×259 asymmetric RGBA rasters. Their
[certified FMA screens](WARM-LANCZOS3-CERTIFIED-FMA.md) improve all
four operations13.6–23.8% on one immutable selected producer. All remain gaps:

| Named case | Fixed output and reason | State |
| --- | --- | --- |
| LanczosReduce8 | 256×129; widened radius-three antialias support and byte output | Gap1.548x/upper1.646x |
| LanczosReduce16 | 256×129; full word precision and alpha normalization | Gap1.588x/upper1.759x |
| LanczosEnlarge8 | 1027×519; negative-lobe enlargement, byte saturation and tiny alpha | Gap1.061x/upper1.225x |
| LanczosEnlarge16 | 1027×519; full word precision, overshoot and tiny-alpha normalization | Gap1.333x/upper1.459x |

The clamped halo/active-region and premultiplied float-output reference is now
qualified for all four, including normal/scalar checks and twelve rejection
controls each. All normalization, ownership and disposal are timed with one CPU.
No other rows, gates or performance evidence are replaced. Aspect-fit uses the
same kernel after small geometry work; add a separate timing case only if that
boundary is material. Exact horizontal rows convert in place, followed by FMA
vertical sums with strict rounding certificates and exact per-pixel fallback.
Scratch falls; local work preselection, shared refusal, ownership and cancellation
are qualified by4457tests/55scalar/55AVX512F-off and the same actual private
package consumer/examples on both OS. All four rows refresh; other233 rows retain
their build scopes. Two full-FMA horizontal trials were slower and rejected;
earlier word-product/prepared-row trials stay parked/rejected. Earlier sampled
fractions describe their original producers, not these changed loops.
The [current CMYK sparse-2D check](WARM-JPEG-CMYK-CURRENT.md) resolves its old-code
uncertainty; the subsequent [integer round](WARM-JPEG-SPARSE-INTEGER.md) saves11.3%:
714.01µs/1.617x/upper1.693,1,085,682B/op. Exact managed pixels,
normal/scalar native quality and two-live-output independence pass. The native
worker, fixture and existing selector/harness are reused. The integer production
change qualifies4464tests/OS,8sparse+29public scalar checks/OS and the same actual
private package/examples on both OS, including sparse-AC shared owners. The original
8KiB basis is immutable module data with no heap table. All64 bins certify before
writes; uncertain blocks retain the original calculation. Only this existing row
refreshes. The subsequent [one-AC current-code check](WARM-JPEG-CMYK-AC-CURRENT.md)
resolves another named row:668.26µs/1.714x/upper1.764,1,085,686B/op. It reuses this
qualified producer, fixtures, worker, harness and selector without builds or
repeated full/package checks. Exact analytical pixels and normal/scalar quality/
owner gates pass. This largest demonstrated current gap motivates a separate
row-specific integer primitive; the accepted2D kernel stays unchanged. Other225
historical codec/edit rows keep their scopes; counts remain237numeric+17untimed.
The [axis-cross trial](WARM-JPEG-AXIS-CROSS-TRIAL.md)
regressed4.5% and is rejected, with production restored. Five freshly compiled
sparse tests pass normally/scalar on both OS; current full/package evidence is
reused. The [FP32-FMA sparse trial](WARM-JPEG-SPARSE-FMA-TRIAL.md) also regresses
7.2% and is rejected. Exact qualified source/DLLs are restored, five fresh
normal/scalar checks pass per OS, and existing 4457-test/full-package evidence
remains valid. The [storage trials](WARM-JPEG-STORAGE-TRIALS.md) reduce allocation
48.9–86.9% but are neutral/slower in elapsed time; both are removed. Restored
production passes29 affected public normal/scalar controls per OS, retaining four
new ownership/refusal/cancellation/peak cases. Selected numeric rows and the finite
checklist stay unchanged. The [async nonseekable PNG inspection screen](WARM-PNG-ASYNC-IDENTIFY.md)
moves one already-named boundary to numeric:24.10µs/native19.34µs,
1.246x/upper1.571,8,408B/op. All21 managed controls revalidate; unchanged normal/
scalar native controls are reused. Complete equivalent input/metadata/EOF/owners
pass, with no producer/harness/selector/native build or full/package rerun.
The [async nonseekable PNG decode screen](WARM-PNG-ASYNC-DECODE.md) adds the second
already-named input boundary:2,049.34µs/native1,412.89µs,1.450x/upper1.683,
12,784B/op. All21 complete-image controls agree; normal/scalar worker and CLI
pixels are exact. The native reference includes the required structural/CRC
adapter and rejects decoder error severity, including partial frames. One harness
build/OS, two native builds, no producer/selector build or repeated package proof.
The [async nonseekable PNG encode pair](WARM-PNG-ASYNC-ENCODE.md) independently
passes at5,397.36µs/native17,268.11µs,0.313x/upper0.314,658,560B/op. Both sides
include complete staging, output writes/growth, independent ownership and release.
Same level6/filterNone, exact alpha/hidden RGB and5%IDAT gate; packets218,775/
210,973B andIDAT218,682/210,283B. All12 actuals retained despite11 selected results;
the conservative bound covers both. One immutable pair, one tooling build/OS,
one native build, no producer/selector build or repeated package proof.
Stop this passing case. The [async JPEG RGBA8 output screen](WARM-JPEG-ASYNC-ENCODE.md)
records3,982.03µs/native3,849.79µs,1.034x/upper1.158,494,254B/op. Full adaptive
admission probe, contiguous staging/tail clearing, normalization/encode and output
delivery/ownership are included. Exact frozen packets158,812/133,041B preserve
the0.082440dB source-quality gap. Current quality samples revalidate once; hashes
reuse setup/cleanup and unchanged native scalar evidence. One tooling build/OS,
one native build, no producer/selector build, calibration grid or full/package repeat.
The [async caller-RGB420 screen](WARM-JPEG-ASYNC-SAMPLES.md) records2,076.73µs/
native1,628.02µs,1.276x/upper1.430,232,614B/op. Both sides borrow the same packed
RGB24 without an RGBA frame, including complete conversion/encode, separate sample
admission/staging and independent caller-output delivery/release. Packets74,040/
52,093B and the0.106110dB source-quality gap stay frozen; current quality samples
revalidate once. Shared output/cancel/failure checks and full/package evidence
are reused. No new grid, producer/selector build or long confirmation above one.
The [async lossless WebP pair](WARM-WEBP-ASYNC-ENCODE.md) independently passes:
11,286.87µs/native121,728.04µs,0.093x/upper0.094,2,494,170B/op, using the approved
libwebp exact=1 (FFmpeg encoder backend). Exact alpha/hidden RGB, original packets
533,100/183,812B, complete repack/import/encode/staging/output delivery/growth/
ownership/release and shared cancel/failure controls pass. All12 actuals retained;
first confirmation preguard launched no benchmark, fresh attempt passed both guards.
One immutable build/OS and one native compiler build; producer/selector/full/package
proofs reused. Stop this passing case; compression efficiency parity remains open.
All four Lanczos3 gaps remain required. Finite254/237numeric/17untimed; other236
numeric observations retain their builds. Next: three named header boundaries below.

[Fixed-workspace PNG controls](WARM-PNG-HEADERS-CONTROLS.md) now pass on the current
producer and normal/scalar native adapter:21 original cases, fixed scratch,
opaque payload seeking, EOF and independent descriptors. CRC/Deflate remain
intentionally opaque. No timing: the explicitly labeled original bounded header
adapter with FFmpeg AVIO/descriptor APIs awaits its reference decision; complete-
packet parsing requires whole-input buffering. Numeric237/untimed17 is unchanged.
A [completed current Reduce16 diagnostic](WARM-LANCZOS3-CURRENT-PROFILE.md) identifies
horizontal filtering at43.4% of958 samples, vertical27.3%, Apply14.5%. These are
sampling locations, not timing results. The [prepared-row trial](WARM-LANCZOS3-PREPARED-ROW-TRIAL.md)
produced7.4%/3.8% gains with an extra row allocation and was rejected. Exact qualified
source/binaries and native controls are restored;47 focused checks/OS pass. No
numeric row is replaced. A scalar-FMA arithmetic prototype passes1,003 independent
weighted-window controls (two need exact fallback), before SIMD integration. Next:
certified double/FMA accumulation, one primitive, same four cases, original Q30
plans and exact pixels; preserve work/memory/root/cancellation/ownership. No new
image/settings grid or passing retune.

A bounded audit of the accepted 0.2.0 additions identifies exactly three required
paths absent from the older checklist; these are not a shape/depth/settings grid:

- `TextLayout/OpenTypeArabicMarksDigits`: reuse the fixed `ArabicMarksDigits`
  text/settings and licensed regular face from the twelve functional comparisons.
  Arabic joining, bidi order, GSUB and mark positioning are distinct from the
  three original logical-order outline-layout controls below. Complete owned layout,
  measurement and disposal; immutable font preparation excluded.
- `TextFit/OpenTypeConstrained`: reuse `ConstrainedFit`, 96×30, maximum24,
  minimum8, step.25 and the same face. The descending search performs30 candidate
  layouts and selects16.75; cumulative work/admission and rejected-owner cleanup
  are a new public boundary, absent from single-layout measurements.
- `MemoryBudget/Png8Retained2`: two sequential decodes of the existing resident
  PNG8 packet, one borrowed shared test-adapter root, with the first output alive
  until the second completes. Include both complete admissions, materializations
  and disposals. Governed uncached storage and independent retained reservations
  differ from default pooling and the earlier `ImageBudget` quota.

The two text cases remain **unmeasured**; establish equivalent native font
adapters and quality/ownership gates before timing. They add no new image or
font/settings grid. [MemoryBudget/Png8Retained2](WARM-COOPERATIVE-MEMORY.md) now has
an initial three-actual short:4,280.80µs/native2,850.88µs,1.502x/upper1.567x,
2,574,662B/op. Two complete decodes and overlapping independent outputs, exact
pixels/hidden RGB, borrowed root, retained charges, refusal/cancellation cleanup
and both official guards pass. Managed admission is included additional work;
FFmpeg has no matching root API. Current production's full/package proofs are
reused;36 adapter controls pass normally/scalar per OS. One924-sample diagnostic
locates49.6% in Deflate walking,22.0% literal copying,8.0% long-literal decoding.
Next: establish one materialized-block decoder strategy's feasibility, preserving
exact work/error/cursor/ownership semantics. No new workload or long above one.
That initial result and diagnostic describe the previous producer; the
materialized-cursor round above supersedes the two current numeric rows.
The
[prepared-double Lanczos trial](WARM-LANCZOS3-PREPARED-FMA-TRIAL.md) is correct but
1.4% slower and rejected. Accepted producers and numeric rows remain unchanged.
Further additions require evidence of a distinct omitted required algorithm or
public boundary.

[Exact SIMD JPEG410](WARM-JPEG-410-SIMD.md) reduces complete time76.7%,
3427.72→799.34us:1.669x/upper1.877x,720,810B/op. Exact1/32 chroma fractions,
binary64 matrix/final rounding and scalar borders/tails/fallback preserve pixels.
Native maximum1/RMSE0.136/alpha and complete timed ownership stay unchanged.
Only410 and the411 H4 selector control refresh;225other rows and70passes reuse
evidence. Full4212tests/99HWoff per OS and the SAME actual private Windows package
consumer/examples pass on both OS. No profile/tool/worker/selector/fixture rebuild,
long above one or image/settings grid. Scope247=227numeric+20unmeasured remains
70passes/156gaps/one confidence-open. Largest current gap:CMYK2D sparse1.812x,
with its prefix trial still rejected. Next named missing boundary:async PNG Identify.

[Exact SIMD JPEG411](WARM-JPEG-411-SIMD.md) cuts complete time82.6%,
3527.46→613.09us, reducing7.771x to1.352x/upper1.587x,751,279B/op. Same original
packet and exact managed/native pixels; unchanged centered reference and complete
owned-output operation. One exact integer-eighth/SIMD/binary64 row primitive,
scalar borders/tails/fallback, no work/admission/pool change. Only411 and two
EOI selector rejection controls refresh;223other rows/70passes reuse evidence.
Full4210tests/97HWoff checks per OS and the SAME actual Windows private package
consumer/examples pass on both OS. No long above one, new profile or settings grid.
Scope247=226numeric+21unmeasured stays70passes/155gaps/one confidence-open.
Largest current gap:CMYK2D sparse1.812x; its prefix trial remains rejected.
Next named missing case:JPEG410 H4V2; establish native equivalence before timing.

[Exact SIMD caller YCCK RGB](WARM-JPEG-YCCK-CALLER-SIMD.md) reduces time80.2%,
4532.59→898.47us:1.488x/upper1.550x,1,337,490B/op. Shared ownedYCCK improves20.3%
to1.163x; callerCMYK control1.592x. These three rows refresh after the caller
selector/shared color body changed. All222others and70passing proofs retain evidence.
Full4209tests/96hardware-off checks per OS and the SAME actual private Windows
package consumer/examples pass on both OS. Same packet/native/CLI/harness/selectors,
exact pixels/approved bounds/owners/work; no new image/settings grid or profile.
Scope247=225numeric+22unmeasured,70passes/154mean gaps/one confidence-open.
Current largest gap:CMYK2D sparse1.812x; its prefix trial remains rejected.

[The first caller YCCK420 screen](WARM-JPEG-YCCK-CALLER-RGB.md) demonstrated the largest
gap:7.504x/upper7.804x,4532.59/604.01us,1,337,431 B/op. The already-named retained
caller matrix/black algorithm uses the existing packet/reference, with exact managed
pixels, approved native bounds and equivalent fresh caller ownership. Direct native
RGB24 matches the old native RGBA colors exactly. No new case/image/settings grid.
Production/all224previous rows/full4208/95HWoff/package proof are reused. One short
and one6sec profile; no long above one. Scope247=225numeric+22unmeasured,70passes/
154mean gaps/one confidence-open. The next primitive targets scalar caller rows.

[Four-component materialization](WARM-JPEG-FOUR-COMPONENT-PLANES.md) reduces caller
CMYK time 47.7%, 1010.51→528.07us: 1.590x/upper1.736x, 1,635,475 B/op. Owned
YCCK improves 9.0% to 1.463x. Caller RGB420 independently reconfirms at 0.962x/
upper0.966x; gray remains a 1.345x short gap. Exactly these four numeric rows
refresh after the common materializer selection; all 220 others and 69 unaffected
passes retain evidence. Current totals stay 224 numeric/23 unmeasured, 70 passes/
153 mean gaps/one confidence-open. Reuse frozen workers/fixtures/harness/selectors;
one immutable producer per OS passes 4208 tests, 95 hardware-off checks and the same
actual private package consumer/examples on both OS. Full objective remains open.
Largest measured gap: CMYK 2D sparse 1.812x; its prefix trial stays rejected.

[The preceding caller CMYK packing round](WARM-JPEG-CMYK-PACKING.md) cut elapsed time64.1%,
2817.13→1010.51us:3.057x/upper3.169x,1,635,459B/op. Exact four-channel samples,
work/admission/cancellation, source/padding and independent owners remain.
Only that numeric row changes;223others and70passing proofs are reused. Full
4208tests/OS,72hardware-off checks/OS and the same actual Windows private package
consumer/all three examples pass on both OS. Production/harness/native/selector
builds during the bounded experiment:one/zero/zero/zero; no repeated profile.
Current finite scope247=224numeric+23unmeasured. The new named control below
documents one retained required algorithm omitted from the earlier checklist,
with the existing packet/reference, not an image/settings grid. Parity stays open.

[The initial caller CMYK measurement](WARM-JPEG-CALLER-CMYK.md) demonstrated the largest
gap:8.522x/upper8.986x,2817.13/330.56us,1,635,373B/op. All four canonical ink
channels and normal/scalar public CLI are exact, with varying black preserved.
Complete fresh caller ownership/decode/packing/release are timed on one CPU.
Untagged original projection, no RGBA/header patch/new image/settings grid.
All223previous rows and full4202/66HWoff/package proof are reused; production
unchanged. One named pending boundary becomes numeric, scope246=224+22.
One6sec profile selects scalar sample packing as the next substantial primitive.

[Exact SIMD YCCK420 rows](WARM-JPEG-YCCK-SIMD.md) reduce time76.1%,
4,761.24→1,135.97us:1.604x/upper1.636x,1,470,246B/op. Exact original pixels,
approved native-only transition16/interior4/RMSE1.5, source/owners/work remain.
Four shared-dispatch CMYK gaps refresh at1.391x/1.732x/1.113x/1.812x.
These five rows replace their prior measurements; all218other rows and70passing
proofs are reused. One immutable producer/OS; frozen harness/native/selector/
fixtures/CLI unchanged. All4,202tests/OS,66affected hardware-off checks/OS and
the same actual Windows private package consumer/examples pass on both OS.
Scope246=223numeric+23pending, unchanged. CMYK2D sparse1.812x is largest;
its bounded prefix trial remains rejected. No new image/settings grid.

[The bounded sparse-prefix trial](WARM-JPEG-SPARSE-PREFIX-TRIAL.md) was rejected:
3.1% mean gain still left1.75x. The exact accepted source and producers are
restored on both OS; all existing rows/qualification are reused. One current
6sec profile and one3actual screen, no new case or full package rerun. The later YCCK420 measurement and SIMD round supersede that next step.

[Identical horizontal JPEG rows](WARM-JPEG-HORIZONTAL-ROW.md) improve the one-AC
CMYK mean10.0%,746.37→671.44us, at1.726x. DC/general remain1.395x/1.126x,
about1.086MB/op. The new `JpegDecode/CmykAdobeSparse2DResident` selects the retained
2D sparse algorithm after horizontal-row rejection. This is one concrete omitted
algorithm: firsttwo AC=4 at(1,0)/(0,1), SAME projection/geometry/unit quantization.
Analytical pixels and original native gates pass; its first short measures1.819x.
No new shape/depth/filter/quality grid. Scope246=222numeric+24pending. All4,197tests/
OS,61affected hardware-off checks/OS and same actual Windows private package
consumer/all three examples pass on both OS. Three rows refreshed/one added;
all70unaffected passing proofs stay stopped. One immutable producer/OS, one new
control-harness build/OS, original native/selector/three fixtures reused; all12
actuals/guards/brackets retained. No long confirmations above one or new profile.

[Direct caller RGB420 sample writing](WARM-JPEG-CALLER-RGB420.md) removes the
largest3.369x gap:71.3% less elapsed time, independently0.966x/upper0.971x,
624,508B/op, exact398,601-byte caller RGB24. Gray's new rejection dispatch is
refreshed at1.326x/273,422B/op and remains a short gap. Precisely these two rows
change; all69existing passes stay valid and the new RGB pass stops. Exact pixels,
normal/scalar native gates, padding/source, limits/cancellation and independent
survivors pass. All4,176tests/OS,73affected hardware-off checks/OS and same actual
private Windows package consumer/all three examples pass on both OS. Scope stays
That round retained218numeric/25unmeasured; the CMYK row below makes219/24.
Next largest measured gap is RGBA8 128x64 tight-memory bicubic1.687x.

[Caller-RGB MCU preparation](WARM-JPEG-CALLER-MCU.md) improves the largest known
gap33.7%,3,162.12→2,097.34µs, retaining exact74,040/52,093-byte matched-quality
packets. Current RGB short1.328x/upper1.408,297,894B/op; gray control1.162x,
155,007B/op. Precisely the two caller-sample rows are replaced, no new case.
Safe admitted384-byte MCU preparation shares packed RGB loads/channel extraction;
old scalar/tight/callback/cancelable/non420 samplers remain. Source/precision/work/
owners and normal/scalar native gates pass. All4,169tests/OS,109affected hardware-off
checks/OS and same actual private package consumer/all3examples pass on both OS.
RGBA/shared-budget JPEG encoding and all other executed paths retain evidence;
all69passing cases stop until affected. That round retained216numeric/27pending; the caller-gray/RGB rows below make218/25.

[Input reconciliation](WARM-INPUT-RECONCILIATION.md) refreshes the remaining22
prior rows after shared JPEG parsing and final encoded-input copy removal.
Fifteen JPEG pixel decodes remain1.118–1.546x short gaps; JPEG Identify independently
passes0.488x/upper0.494,88B/op. Of six PNG/WebP Stream cases, RGBA16 PNG independently
passes0.988x/upper0.991,8,648B/op; the other five remain1.139–1.562x short gaps.
The preceding [baseline grayscale result](WARM-JPEG-GRAY-CURRENT.md) remains current
and is reused. All90actuals/guards/quality/ownership checks remain. Exactly the same
qualified immutable producer/harness, native workers, fixtures and timing drivers
are reused; missing selectors were restored from exact retained archive members.
Only the21 changed-path metadata controls were replayed; no production/harness/
native/selector build, source synchronization, broad audit or package recheck.
Full4,167tests/OS,183affected intrinsics-disabled checks/OS and same actual package
consumer/all3examples on both OS remain valid. Stop the two newly passing paths.

[Bounded VP8 coefficient adaptation](WARM-VP8-ADAPTIVE-PROBABILITIES.md) changes
exactly six existing default complete encode timings/packet sizes and adds
`WarmWebp/EncodeBudget8`: the existing alpha8 raster with MaxWork32,000,000,
which declines optional adaptation and exercises the original fixed-model encoder.
This distinct required caller-budget algorithm, not a new image/settings grid,
has independent passing evidence and byte-exact old output. Opaque bytes shrink
54.5%, allocations41.6%, time7.9–9.8%; both opaque cases remain1.320–1.323x short
gaps. Alpha bytes shrink39.2%, time8.5–8.8%. Photograph/diagram packets shrink
1.8%/7.5%, with1.6%/1.7% higher times. Diagram independently measures1.000208x/
upper1.009341x, superseding its old pass. It remains open; no repeated unchanged
measurement to force a pass. The other three default passing cases plus budget
fallback independently pass. All81 actuals, guards, quality/ownership gates and
identities are retained. Exactly4,167tests/OS,183affected intrinsics-disabled
checks/OS and the same actual private package consumer/all3examples pass on
both OS. Five lossless proofs and other unaffected paths remain current. Stop
passing cases. Compression efficiency is improved; full parity remains open.

[Previous-row references](WARM-VP8L-PREVIOUS-ROW.md) reduce the diagram's VP8L
size31,072→15,120B (51.3%) and allocation67,720→34,928B/op (48.4%). The photograph
shrinks385,854→383,154B (0.7%). The changed lossless encoder invalidates precisely
the three existing WarmWebpLossless encodes and two new WarmCompression lossless
cases. All five have fresh independent passing evidence, conservative uppers
0.0816/0.0995/0.1316/0.1132/0.2838. Stop them until affected. Additional matching
work raises their prior-round mean times about40–55%; the tradeoff is explicit,
with all five still faster than their reference. No extra workspace/frame copy.

All4,161 tests/OS,30 affected hardware-off checks/OS and the same actual Windows
private package consumer/all three examples on Windows/Linux pass. Other codecs,
decoding/Identify, editing, drawing and text production paths are unchanged.
Existing literal/run decoder input packets also stay byte-identical. Lossy WebP
packets reproduce frozen anchors exactly; no quality retuning or timing repeat.
Shared/async lossless paths remain already named/unmeasured. No case/settings
grid, native/harness/timing-driver rebuild or broad qualification repeat for
unaffected production. Historical [opaque alpha coding](WARM-VP8L-OPAQUE-ALPHA.md)
and its qualification remain archived. Counts are unchanged by this replacement.

The [four content comparisons](WARM-WEBP-COMPRESSION.md) add exactly two fixed
513×259 opaque RGBA8 inputs, with provenance in [their manifest](Fixtures/Compression/manifest.json):
`WarmCompression/PhotographLossless8` covers photographic texture and lossless
size, `DiagramLossless8` covers repeated flat regions and antialiased text,
`PhotographLossy8` covers photographic rate/distortion, and `DiagramLossy8` covers
text edges and flat-region rate/distortion. These distinct required content
families were absent from the procedural controls. No shape/depth/effort grid.
All four measure complete output ownership and allocations with equivalent
delivery. The two lossless cases and photograph VP8 independently pass; the later
bounded-model diagram result is the open comparison above. The earlier lossless sizes were2.394x/2.090x; the current cache round above reduces them to1.849x/1.056x the exact native packets; VP8
sizes are 1.212x/1.167x at matched source quality. The preceding lossless change replaces only the five affected timing proofs;
other production paths and qualification evidence are reused. Known scope is now247 named
cases:225 numeric plus22 unmeasured. Stop these three time-passing cases unless
their relevant production path changes; compression-size work remains required.

The two [shared-budget codec boundaries](WARM-SHARED-BUDGET.md) are now measured:
`SharedBudgetDecode/PngRgba8Resident` reuses `WarmPng/Decode8`; complete public
budgeted decode and retained-owner disposal measure1.439x,4,561B/op.
`SharedBudgetEncode/Jpeg420Rgba8` reuses `WarmJpeg/Encode420_8`; complete output/
work settlement measures1.145x,297,781B/op, unchanged74,040/52,093-byte packets.
Both are qualified short gaps, not passing confirmations. One finite long-lived
caller budget and source/input reservation are prepared outside timing; original
per-operation limits remain. Two live owners/packets, exact retained charges,
surviving outputs, source stability and temporary-memory/work settlement pass.
No new case/grid, native/production/selector build or repeated full package proof.
Only benchmark harness inputs changed; existing executed production evidence stays
valid.23new boundaries remain unmeasured. The original unbudgeted paths are reused.

Direct JPEG sample decoding has two named representatives, both currently
measured in [the RGB420 writing round](WARM-JPEG-CALLER-RGB420.md).
`JpegSamplesDecode/GrayscaleResident` reuses `WarmJpegDecode/OwnedDecodeGray`:
new rejection-dispatch control1.326x/upper1.445x,273,422B/op,132,867-byte GRAY8.
`JpegSamplesDecode/Rgb420Resident` reuses `WarmJpegDecode/NativeDecode420`:
independent0.966x/upper0.971x,624,508B/op,398,601-byte RGB24. Stop this new pass.
Both include fresh caller destination acquisition, complete decode/normalization
and independent output lifetime with equivalent native delivery. RGB additionally
includes native centered2x2 chroma expansion/crop and BT.601 packing. There is no
RGBA frame. Original input/settings/native workers and JPEG quality gates stay
fixed. Exact qualified managed pixels, native CLI delivery, normal/scalar gates,
padding/source and independent surviving owners pass. The new SIMD packing
primitive changes precisely RGB and gray dispatch timings; older other evidence
is retained. Full cross-platform/differential/package qualification is recorded.
JPEG sample decoding also supports CMYK, as described below.

Direct JPEG sample encoding adds two now **screened** representatives:
`JpegSamplesEncode/GrayscaleResident` uses the fixed grayscale projection and the
approved SIMD-enabled libjpeg-turbo reference; `JpegSamplesEncode/Rgb420Resident`
uses the fixed opaque RGB projection and FFmpeg's 4:2:0 reference. Retain existing
quality/size matching, with equivalent raw sample delivery, complete row
normalization, streaming and independent encoded output ownership. The cases
cover single-component samples and packed RGB conversion, without a settings grid.
Both direct-sample decode and shared-budget boundaries above have qualified shorts. The [current sample screens](WARM-JPEG-CALLER-MCU.md) measure gray1.162x/upper1.422x,
155,007B/op and RGB4201.328x/upper1.408x,297,894B/op, after the33.7% RGB MCU win. Both are mean gaps, so no long
confirmation follows. Native raw-input adapters use the same pinned libraries
and exact frozen packets; each operation includes necessary normalization/coding
and fresh independent output ownership. Production and H4 qualification are reused.

The [current direct-sample quality check](warm-jpeg-sample-quality-evidence.json)
reproduces both frozen managed packets exactly, using the same original gray/RGB
projections. Gray:58,823/58,910 bytes and PSNR gap0.000033dB; RGB420:74,040/52,093
bytes and gap0.106110dB. Both pass the existing0.25dB gate, decoder agreement,
source preservation and independent-output checks. This proves reusable quality
and size anchors. The new raw-input screens supply the distinct time/allocation
evidence; existing RGBA operation timings were not substituted for these paths.

JPEG compatibility changes shared frame/header parsing (one/three/four components),
scan setup (four component slots), sampling validation (horizontal ratios up to
four), Adobe marker handling and final decode dispatch. These changes originally invalidated **all17 existing
JPEG decode/inspection rows**, including the
fresh-Stream restart case and resident Identify. Their old numbers are historical; the current input reconciliation above
replaces all17, without treating an unchanged IDCT kernel as complete-operation proof. Entropy/IDCT kernels are unchanged, but that alone does not prove the
complete operation unchanged. JPEG encode, PNG/WebP and editing evidence is reused.
All17 affected rows now have current evidence in the reconciliation above.

Synchronous Stream decode now leases its private input instead of making a final
encoded-input copy, admitting actual retained capacity through decoding. This
originally invalidated exactly seven existing complete operations: `Png8`, `Png16`,
`PngCropFit8`, `PngCropFit16` (WarmStreamDecode), `StreamNative8` (WarmVp8Decode),
`StreamNativeLossless8` (WarmWebpDecode), and `StreamDecode420Restart` (WarmJpegDecode).
The last was already among the seventeen prior JPEG rows; six additional rows
became prior production, for23 total. All23 are now current, including the
preceding grayscale result; historical allocation counts are superseded.
No new workload is needed. Resident codec paths, synchronous Identify, async input,
encoding and editing remain unchanged. No unaffected operation was retimed.

Five representatives cover the required JPEG compatibility algorithms; all five have measured current gaps; no JPEG compatibility representative remains unmeasured:

- `JpegDecode/CmykAdobeResident` (**measured**1.391x DC gap): four component planes, Adobe inversion and
  complete owned RGBA conversion from an original CMYK projection. The
  separately named horizontal, 2D sparse and all63-AC controls above cover each
  retained transform algorithm.

- `JpegDecode/Ycck420Resident` (**measured**1.163x gap): subsampled color
  reconstruction, explicit matrix and full-resolution black, original varying input.
- `JpegSamplesDecode/CmykResident` (**measured**1.592x gap): untagged full-resolution
  canonical ink, all4raw native planes preserved and packed to fresh caller storage.
  Adobe inversion has correctness coverage; no RGBA intermediate.
- `JpegDecode/411Resident`: [measured1.327x control gap](WARM-JPEG-410-SIMD.md); quarter-width horizontal chroma reconstruction on the
  existing RGB projection; native sample-grid normalization and complete ownership verified.
- `JpegDecode/410Resident`: [measured1.669x gap](WARM-JPEG-410-SIMD.md); quarter-width, half-height chroma
  (JPEG factors4×2, not video4×4). Original native samples and centered normalization
  verified; complete owned-output boundary measured.

The accepted full-resolution CMYK caller selector exposes one concrete omitted
required path: `JpegSamplesDecode/Ycck420RgbResident` (**measured**1.488x gap). SourceYcck
and outputRgb reject raw byte packing; the retained WriteCmykSamples path performs
centered chroma expansion, binary64 matrix/inversion/black and directRGB24 stores.
Neither owned-RGBA YCCK SIMD nor caller YCbCr420 RGB measures that complete public
boundary. Reuse the existing YCCK420 packet and analytical RGB; establish equivalent
fresh native RGB24 ownership/normalization and preserve the approved fixed-fixture
quality bound before timing. This single additional case changes246to247; no
new image, shape, depth, filter, quality or sampling grid. Other sampling/orientation
combinations retain correctness coverage, without a Cartesian timing expansion.

Baseline/progressive analytical and live FFmpeg compatibility checks cover these
layouts; they are not performance confirmations. Reuse the existing source
projections and fixed settings. The current finite total is stated at the top;
the named async boundaries below are already included. Do not add a
shape, depth or quality grid. Add cases only for a distinct required path omitted
from this list, with its concrete reason.

Async input has exactly two named common front-end boundaries:
`AsyncStreamIdentify/PngRgba8FreshNonSeekable` and
`AsyncStreamDecode/PngRgba8FreshNonSeekable` now have qualified short gaps above.
Both reuse the existing PNG8 input
and full structural/pixel gates, adding actual async reads through EOF, fresh
private input acquisition/growth/release and independent descriptor/pixel owners.
They cover the shared async acquisition path and its two different complete
public operations. Shared-budget awaits have correctness coverage; no new
codec/depth/stream scheduling grid is added. Resolve equivalent native input
delivery before timing. These cases are already included in the finite total.
Original synchronous methods and codec kernels are unchanged by async acquisition;
the later synchronous input-copy change's23affected rows now have current evidence.

Async output has four representatives, all on existing inputs:

- `AsyncStreamEncode/PngRgba8FreshNonSeekable`: batched chunk staging, complete
  encode, asynchronous delivery and independent caller-owned output. Independently
  passing pair above; stop unless affected production changes.
- `AsyncStreamEncode/JpegRgba8FreshNonSeekable`: the distinct stock-memory adaptive
  admission probe and bounded contiguous staging for the existing RGBA8 anchor.
  Qualified short gap above, with unchanged matched-quality packets.
- `AsyncStreamEncode/JpegRgb420Samples`: direct borrowed RGB memory and the separate
  sample/adaptive admission boundary, reusing the fixed RGB420 projection.
  Qualified short gap above; exact frozen matched-quality packets.
- `AsyncStreamEncode/WebpLosslessRgba8FreshNonSeekable`: byte-at-a-time bit-writer
  emission into staging, distinct from PNG's batched writes, with exact hidden RGB.
  Independently passing pair above; stop unless affected production changes.

All four now have current-code numeric evidence. These cover distinct
staging/admission/delivery paths, without a codec,
sampling, depth or scheduling grid. Resolve equivalent native destination
acquisition/copies and output lifetime before timing. Shared-await correctness
is separately qualified. These four cases are already included in the finite total.
Original encoder bodies/state/codec kernels are unchanged. Later synchronous input
acquisition changes affect only the separately reconciled operations above.

Fixed-workspace header inspection adds three **unmeasured** representatives using
the existing encoded inputs, each for a distinct required container grammar:

- `InspectHeaders/PngRgba8FreshSeekable`: chunk framing, dimensions/EXIF and skipped
  opaque IDAT, including source acquisition and EOF; excludes PNG CRC/pixel checks.
  [21 controls pass](WARM-PNG-HEADERS-CONTROLS.md); bounded reference decision pending,
  still unmeasured. Public complete-packet parser buffering is outside this contract.
- `InspectHeaders/Jpeg420FreshStream`: full marker/stuffing/restart scan through
  entropy and late metadata to EOF, without entropy-symbol decoding.
- `InspectHeaders/WebpLosslessRgba8FreshSeekable`: RIFF framing, VP8L dimensions,
  feature flags and metadata following skipped image data, through EOF.

Use equivalent native header validation, input delivery and output ownership before
timing; full `Identify` timings cannot substitute for these cheaper boundaries.
Async scheduling/shared reservation variants have correctness coverage, without
another timing grid. The new inspector uses independent methods and types; existing
successful method bodies, state layouts and numeric evidence are unchanged by the
inspector. The separate synchronous decode acquisition change is accounted above.

Drawing adds six **unmeasured** cases on the existing RGBA8/16 rasters:

- `FillRectangle/Rgba8Clipped` and `FillRectangle/Rgba16Clipped`: distinct byte/word
  store loops and color precision; one fixed clipped rectangle per existing raster.
- `CopyRegion/Rgba8Independent`: two caller-owned buffers and exact row/chunk copy.
- `CopyRegion/Rgba16Overlapping`: one owner, reverse traversal and overlap-safe
  word-sample copying without a snapshot raster.
- `Composite/Rgba8StraightAlpha`: complete clipped source-over on two byte owners.
- `Composite/Rgba16Overlapping`: full-width rational arithmetic, word samples and
  overlap-safe self-compositing with one owner.

Reuse original 513×259 rasters and fixed 257×129 fill/copy regions; placement (-3,5)
clips fill/independent copy/composite, with overlap shifted (1,1). No image/shape/
alpha grid. Resolve equivalent native in-place canvas delivery, state restoration,
rounding and ownership before timing. Exact BigInteger and bounded native profile
correctness checks are not performance confirmations. Existing paths/statuses are
unchanged: 190 current rows, 23 prior rows. Including text below, there are 29
named unmeasured boundaries.

Text layout/measurement adds three **unmeasured** complete operations on original
generated fonts, each exercising a distinct required outline/mapping path:

- `TextLayout/OriginalUnicode`: cmap12, long loca, ordinary coordinate words,
  quadratic extrema and a basic compound accent.
- `TextLayout/PackedBmp`: cmap4, short loca, packed/repeated flags, implied
  quadratic points and scaled component offsets.
- `TextLayout/CompositeTransforms`: two-by-two matrices, point attachment and
  inherited component metrics.

Reuse the three tiny `--font-layout` controls and one fixed string
`AV g Á\u0301 \uE000\nAV\tg AV`, size16, wrap64, line spacing1.25. Prepare the
same immutable face outside timing on both sides; include complete layout,
needed outline decoding, measurement, positioned values and independent output
ownership. Resolve equivalent FFmpeg/font-backend scope before timing. The
15-glyph native geometry check is correctness evidence only. No text timing or
allocation claim, font/style/size grid, rasterization case or changed numeric
status follows from this addition.

Rendering adds `TextRender/Rgba8Clipped` and `TextRender/Rgba16Clipped`, exercising
distinct byte/word canvas sample writes and precision. Reuse the original font,
fixed text/settings above, existing 513×259 canvases and position(-.25,1.5).
Only immutable font preparation is outside timing. Include layout/needed geometry,
coverage and complete in-place drawing with equivalent native canvas delivery,
state restoration and output ownership; do not substitute a cached rendered mask.
Both time and allocations are **unmeasured**. Analytical quadratic coverage checks
and the separately recorded default FreeType raster differences are development
correctness evidence; equivalent native quality/scope still needs resolution before
timing. No existing native pixel allowance or numeric evidence changes.

Latest [wider RGBA8 horizontal reversal](WARM-HORIZONTAL-FLIP512.md) improves
single-output flip **22.0%**, passing parity at **0.950× / upper 0.965×**. Two live
outputs improve **27.9%** but remain a qualified **1.053×** gap. Both retain
**128 B/image**, exact pixels, hidden RGB and independent owners. The RGBA16
passing kernel and original fallbacks remain unchanged; no new allocation or
resource/work charge. At that round, other 205 comparisons/all 63 prior passing
proofs remained valid; the JPEG rows above now require reconfirmation.
All 3,739 Release tests/OS, 67 affected public hardware-off checks/OS and the same
actual private package consumer/all three examples pass on Windows/Linux.
The newly passing single-owner path stops; no width/unroll/inlining grid follows.

Earlier [partial PNG transparency](WARM-PNG-PARTIAL-ALPHA.md) retains its 33.3%
gain and passing proof; fixed JPEG fields, packed bicubic reduction and PNG/JPEG
reconstruction retain their measured gains. Those executed paths remain unchanged.

The finite checklist's **previous <=2x phase was completed for released0.1.0**.
Its original independent evidence and unchanged-path proofs remain historical;
subsequent production changes identify exactly which current rows they supersede.
Only independent upper<=1 paths are stopped for the present objective. The coverage
items below name required algorithms and public boundaries; links marked closed
refer to that previous phase. Current results and allocations are in the summary.

Scope is the existing two original513x259 RGBA8/16 rasters, documented projections
and the named fixed selectors in the linked evidence/renderer. Required still
PNG/JPEG/WebP codecs,8/16-bit edits, orientation/crop/nearest/bilinear/bicubic,
resident/fresh-Stream and existing cancellation/limits/owner boundaries are
represented. Cases were chosen for distinct material algorithms or public
boundaries, not a Cartesian grid of settings. The61 bicubic selectors were already
established before this audit and were reconciled, not expanded.

| Item | Named fixed scope and reason | Current evidence / state |
| --- | --- | --- |
| B01 | Bilinear Down8/16,Up8/16: shared resize dispatch and byte/word alpha-aware filtering | [Previous phase closed](WARM-CURRENT-BILINEAR.md); [Down8](WARM-BILINEAR-BYTE-ROW-CACHE.md), [Down16](WARM-BILINEAR-WORD-CACHE.md) and [Up8](WARM-BILINEAR-BYTE-FACTORIZATION.md) improved, still <=1x gaps |
| B02 | JPEG adaptive Encode8/16,Encode422_8/16: changed AC planner at444/422 and both input depths | [All four closed](WARM-CURRENT-JPEG-ADAPTIVE.md) |
| B03 | JPEG Decode8,NativeDecode8,NativeDecode420/422,OwnedDecodeGray,NativeDecode420Restart,DirectRgb422/420: changed ordinary AC loop plus distinct component renderers | [All eight closed](WARM-JPEG-GRAY-CURRENT.md) |
| B04 | JPEG ProgressiveGray/420/422 plus ordinary/live-token444: masked refinement and its guarded fused readers | [Four ordinary proofs reused](WARM-JPEG-REFINEMENT-FUSED.md); [live-token444 improved again](WARM-JPEG-REFINEMENT-LOOKAHEAD.md), still a <=1x gap |
| B05 | The25 named PNG decode selectors in the linked report: byte/word predictors, expansion and input-pool changes | [All25 closed](WARM-CURRENT-PNG-DECODE.md) |
| B06 | PNG DecodeAdam7_16,DecodeGray1Adam7: interlaced placement on changed shared framing; later palette4/8 cases reuse their own proofs | [Closed](WARM-CURRENT-PNG-ADAM7.md) |
| B07 | Png8/16,PngCropFit8/16: complete fresh input, orientation/crop/fit and intermediate ownership | [Closed](WARM-CURRENT-PNG-STREAM.md), with word fusion proof reused |
| B08 | PNG Encode8/16: changed RGBA packing, exact samples and fixed IDAT-size policy | [Both closed](WARM-CURRENT-PNG-ENCODE.md) |
| B09 | VP8 Encode8/16 with alpha: changed output writer and word normalization | [Both closed](WARM-CURRENT-VP8-ENCODE.md) |
| B10 | VP8L Lossless8 literal/run input: changed entropy/run path | [Closed](WARM-CURRENT-VP8L-LITERAL.md) |
| B11 | Clone,crop,normal orientation,horizontal/vertical flip,half turn,CW/CCW,transpose,transverse at both depths: shared output-pool acquisition | [All20 closed](WARM-CURRENT-EXACT-EDITS.md); retained-two flip unchanged |
| B12 | The61 named bicubic selectors: changed dispatch/helper/guard paths, default/tight scratch, byte/word and established support widths | [Previous phase closed](WARM-CURRENT-BICUBIC.md); [word](WARM-BICUBIC-PAIRED-COLUMNS.md) and [byte](WARM-BICUBIC-BYTE-PAIRED.md) paired columns plus [byte strips](WARM-BICUBIC-BYTE-STRIPS.md) and [byte](WARM-BICUBIC-WEIGHT-BROADCAST.md) and [word](WARM-BICUBIC-WORD-BROADCAST.md) packed weights improved, still <=1x gaps |
| B13 | Existing PNG stored/Paeth/predictor,RGB/gray/gray-alpha8/16,packed gray1/2/4,palette1/2/4/8,tRNS,Adam7 and frozen compression-mode rows; four named exact-limit RGBA8/16 ordinary/live-token boundaries | [Up8/16](WARM-PNG-UP-SIMD.md), [RGBA8 Sub](WARM-PNG-SUB-PREFIX.md), [stored RGBA16 encode](WARM-PNG-STORED-CAPACITY.md) and [RGB16 Paeth](WARM-PAETH-SIX-CHANNEL.md) parity confirmed; [tight RGBA streaming](WARM-PNG-STREAMING-ROWS.md) improves four gaps; other applicable proofs reused; precise selectors/allocations in the [summary](../docs/benchmark.md) |
| B14 | JPEG4208/16,restart,gray and geometric live-token/forward output; VP8 ordinary/simple/alpha/live-token/Stream and opaque8/16 encode; VP8L native/indexed/live-token/Stream and exact backend encodes | [VP8 source views](WARM-VP8-SOURCE-VIEWS.md) improve four encodes; [fixed JPEG encodes](WARM-JPEG-FIXED-AC-FUSION.md) and other unchanged proofs reused |
| B15 | Nearest Down8/16,Up8/16: unchanged early dispatch/integer kernels and allocation path | Existing independent proofs reused |
| C01 | PNG resident Identify: full container/CRC/EXIF without inflation, fresh descriptor; decode/cold timings do not cover it | [Closed](WARM-PNG-IDENTIFY.md),1.234x/upper1.238x,88B/op |
| C02 | JPEG resident Identify: complete marker/scan/stuffing/EXIF/EOI scan without entropy/pixels | [Prior-production confirmation](WARM-JPEG-ENTROPY.md),0.499x/upper0.506x,88B/op; reconfirm after four-component header support |
| C03 | WebP resident Identify: distinct RIFF/feature/header/EXIF algorithm, one existing VP8L packet | [Closed](WARM-WEBP-IDENTIFY.md),0.0040x/upper0.0045x,88B/op,explicit FFmpeg parser reference |
| C04 | Fresh non-exposed seekable Stream Identify on the same C01 PNG: fast metadata boundary exposes input acquisition/read/EOF/ownership costs | [Closed](WARM-PNG-STREAM-IDENTIFY.md),1.445x/upper1.459x,184B/op |

Historical <=2x audit gaps were resolved: stored16 PNG2.219x→1.090x, wide-cache
bicubic2.121x→passing current-path evidence, PNG Identify2.797x→1.234x,
JPEG Identify33.725x→1.840x, and Stream Identify7.863x→1.445x. Each production
round retains correctness/control evidence, short mean before confirmation,
all actuals/quiet guards and full accepted-change qualification. Raw refusals,
failed guards and superseded experiments remain in their original reports.

Historical [stored-capacity reconciliation](warm-png-stored-capacity-evidence.json)
replaces only the two storedRGBA8/16 encodes: word independent0.861x/upper0.870,
1,140,504B/op; byte1.295x/upper1.430,604,319B/op qualified short. Capacity allocation
is inside the complete fresh-output operation; packets, packing, compression,
checksums, work/working admission, writes/callbacks, limits and owners remain.
Other205comparisons/all59previouspasses are reused. Full3701+65scalar/OS and the
same actual package consumer/all3examples bothOS pass. No added case/grid.
60passes/143gaps/4confidence;145selected independent+62short. Passing word stops.

The preceding [Sub-prefix reconciliation](warm-png-sub-prefix-evidence.json) replaces
only DecodeSub8:50.5% faster,independent0.851x/upper0.855,4456B/op. Stride4 Sub
uses exact SIMD byte prefixes with unchanged resources/work,cancellation and
partial-failure behavior. Other206 comparisons/all58 previous passes are reused;
word Sub/byte Paeth control means remain within1.2%. No newcase/raster/settings
grid or profile. Full3692+92scalar/OS and the same actual package consumer/all3
examples bothOS pass.59passes/144gaps/4confidence;144selected independent+63short.

The preceding [packed-weight reconciliation](warm-bicubic-weight-broadcast-evidence.json)
replaces only Tight64x32_8 with a qualified short:12.5% less time,1.527x/upper1.797.
One exact vector-construction expression changes in the >20/≤36tap byte packed
core; all arithmetic/order, guards, workspace, work/cancellation and owners remain.
Generic join first regressed78.7%; retained JIT evidence identifies a full-width
stack dependency. Direct insert removes it, with no new profile or settings grid.
Unchanged controls retain their selected evidence; other206 rows/all58 passes
are reused. Full3680+138scalar/OS and the same actual package consumer/all3examples
bothOS pass.144selected independent+63qualified short; full parity remains open.

The preceding [live refinement reconciliation](warm-jpeg-refinement-lookahead-evidence.json)
replaces only ProgressiveCancelable444:10.1% less time, still1.552x/upper1.731.
Bounded ordinary lookahead retains original logical cursor, pending bits, per-bit
cancellation/work and immediate updates on every exit. Original fused/scalar
readers are untouched; two methods and one dispatch are added. Other206 rows and
all58 passing proofs are reused; unchanged ordinary-progressive/baseline controls
are within0.9%. Full3680+475scalar/OS and the same actual package consumer/all3
examples bothOS pass. All three short jobs retain actuals/both quiet guards on one
immutable producer/harness/selector/reference build. No new performance matrix.

The preceding [general byte-row reconciliation](warm-bicubic-byte-emitter-evidence.json)
replaces those same thirteen cache rows without new coverage. Five byte paths
improve11.6–20.8%; one dispatch/depth predicate and private name change, with the
entire emitter body unchanged. VerticalUp8's1.010x short warrants one independent
uncertainty check:1.020x/upper1.024x, a gap. Other12 remain qualified short gaps;
no unchanged repeat until pass. Other194 comparisons/all58 passing proofs retain
paths. Full3663+209scalar/OS and the same actual package consumer/all3 examples
bothOS pass. Native workers/gates and full boundaries are unchanged; no new
profile, raster, selector build or settings grid.

The preceding [byte-row reconciliation](warm-bicubic-byte-row-evidence.json) replaces
exactly thirteen existing default-cache timings, conservatively including word
and cross controls sharing the changed hot method. HorizontalUp8 improves16.4%;
all remain qualified short gaps. One suspicious vertical control is checked once
against the frozen predecessor:0.94% mean difference rather than4.14% against
older evidence. All actuals/guards retained, no repeat-until-pass. Other194
comparisons/all58 passing proofs retain their paths. Full3663 Release tests/OS,
209 scalar cache/reference checks/OS and the same actual package consumer/all3
examples bothOS pass. Original arithmetic, guards, allocation/work and approved
native policy remain. Two vertical reference identities reuse existing source
proofs without rebuilding. No new performance selector, input or settings grid.

The preceding [cancelable JPEG reconciliation](warm-jpeg-cancelable-ac-cache-evidence.json)
replaces only NativeDecodeCancelable8 with a qualified short gap after 9.3% less
managed time. One local cache method changes; prefix/amplitude work, cancellation
and partial-state semantics remain exact. Other206 comparisons/all58 passing
proofs retain their paths. Full3662 Release tests/OS,96 scalar JPEG checks/OS and
the same actual package consumer/all3 examples bothOS pass. The current approved
ordinary native444 worker is reused with original packet/output hashes and
unchanged complete boundaries; direct-RGB mode additions do not apply here.
No new performance image, selector or settings grid.

The preceding [byte bilinear reconciliation](warm-bilinear-byte-factorization-evidence.json)
replaces exactly Up8 with a qualified short gap after 16.8% less managed time.
Exact integer factorization preserves every final sum, original rounding and
admission. Only byte bilinear enlargement on both axes executes the changed
helper. Other206 comparisons/all58 passing proofs retain their paths. Full3659
Release tests/OS,132 scalar bilinear/reference checks/OS and the same actual
package consumer/all3 examples bothOS pass. Reference, quality and complete
operation boundaries are unchanged; no new performance input or settings grid.

The preceding [enlargement-strip reconciliation](warm-bicubic-enlargement-strips-evidence.json)
replaces exactly seven timings: tight both-axis/horizontal/vertical byte and word,
plus default horizontal word. All are qualified short gaps after8.3–17.8% less
managed time. Only existing strip capacities change; reversing them recovers both
entire predecessor files. Fixed arrays4000B remain within4096B admission; all
arithmetic, work, source/owner and exact-pixel/approved native gates are unchanged.
All200 other comparisons/all58 passing proofs retain their paths. Full3656
Release tests/OS,163 scalar resize controls/OS and the same actual package
consumer/all3 examples bothOS pass. Native horizontal identity reuse follows the
existing source proof; no reference build, new baseline policy or timing repeat.

The preceding [VP8L lookup reconciliation](warm-vp8l-borrowed-green-evidence.json)
replaces exactly seven decode timings with qualified short gaps: native VP8L
ordinary/live-token/Stream, literal/run, indexed16 and compressed-alpha VP8
ordinary/Stream. Native lossless improves13.5–13.7%; literal/run's mean is2.1%
above its preceding result and alpha controls are roughly flat. Original private
pixel tail scratch and stable grouping add no allocation/reservation/work.
All three predecessor files are recovered by removing only this primitive.
All200 other comparisons/all58 passing proofs retain their paths. Full3654
Release tests/OS,43 scalar Huffman/lookup checks/OS and the same actual package
consumer/all3 examples bothOS pass. Original validator refusals were reconciled
without repeating completed timings; no baseline/tolerance/boundary changed.

The preceding [adaptive JPEG reconciliation](warm-jpeg-adaptive-ac-evidence.json)
replaces all eight affected encode timings with qualified short gaps,9.9–14.8%
less managed time. One optional occupancy branch changes AC planning; removing
it restores the entire predecessor JpegEncoder file. All199 other comparisons
and58 passing proofs remain applicable. Packets, compressed sizes, approved
grayscale reference and quality/admission/complete-output boundaries are frozen.
Full3650 Release tests/OS,20 scalar adaptive checks/OS and the same actual package
consumer/all3 examples bothOS pass. No case was added or long gap confirmation run.

The preceding [JPEG scanner reconciliation](warm-jpeg-entropy-evidence.json)
replaces only resident JPEG Identify:0.499x/upper0.506x,72.9% less managed time.
One noncancelable branch/helper batches admitted entropy/stuffing; removing it
and three imports restores the entire predecessor JpegIdentifier file. All206
other comparisons and57 prior passing proofs retain their executed paths.
Full3648 Release tests/OS,20 scalar scanner checks/OS and the same actual package
consumer/all3 examples bothOS pass. The approved reference and all21 equivalent-
boundary controls remain unchanged. No case was added.

The preceding [byte packing reconciliation](warm-png-byte-packing-evidence.json)
replaces exactly gray8/RGB8/gray-alpha8 encode timings; all three independently
pass upper<=1 with exact packets and frozen Optimal9/native6/5% IDAT policy.
The entire predecessor PngCodec file is restored by removing one dispatch and
two packing helpers. All204 other comparisons and54 prior passing proofs remain
applicable. Current3646 Release cases/OS,36 scalar packing/admission controls/OS
and the same actual package consumer/all3 examples bothOS pass. No case was added.

The preceding [PNG Up reconciliation](warm-png-up-simd-evidence.json) changes only
filter2 dispatch and a safe SIMD row helper. DecodeUp8/16 independently pass
at0.984x/upper0.989x and0.995x/upper0.997x. The entire predecessor PngCodec file
is restored by removing that dispatch/helper; all205 other comparisons and52
prior passing proofs remain applicable. No admission/work/allocation or quality
change. Current3642 Release cases/OS, five scalar Up and127 scalar public PNG
controls/OS and the same actual package consumer/all3 examples bothOS pass.

The preceding [PNG chunk checksum reconciliation](warm-png-chunk-crc-evidence.json)
affects exactly16 existing PNG encode selectors. Ten independently reconfirm
upper<=1; six retain qualified short gaps. Stored8/16 means are1.624/1.664x;
the word gain uses a contemporaneous accepted-build control because its old
504.25us result was not reproduced. All packets/settings/quality/ownership gates
remain frozen. The other191 comparisons, including42 passing proofs, retain
unchanged executed paths. Decode/metadata Compute and its old helpers are
byte-identical. Current3,639 Release cases/OS,83 scalar CRC and49 scalar public
PNG controls/OS and the same actual package consumer/all three examples on
Windows/Linux pass. No material case or input was added.

The preceding [six-byte Paeth reconciliation](warm-paeth-six-channel-evidence.json)
adds one SIMD reconstruction kernel: RGB16 independently passes at0.959x/upper0.969x.
RGBA8/RGB8 dispatch controls have qualified short screens; their kernels remain
unchanged. The other204 comparisons, including all51 earlier passing proofs,
are reused. The stopped passing RGBA16 Paeth first branch is unchanged.

The prior [VP8 source reconciliation](warm-vp8-source-views-evidence.json)
changes only source conversion/sample access for four existing encode selectors.
Opaque8/16 are qualified screens at1.661/1.685x; alpha8/16 independently reconfirm
passing upper<=1. The other203 comparisons, including49 passing proofs, retain
their executed paths. No other codec, decode, metadata or edit path changed.

The prior [fixed AC reconciliation](warm-jpeg-fixed-ac-fusion-evidence.json)
changes the selected fixed writer traversal while preserving exact packets,
ordered work/callbacks, cancellation, quotas and partial state. Forward4208/16,
forward4448 and live-token4448/42016 have current qualified short screens at
1.399–1.548x; no independent parity confirmation is claimed. Adaptive coding,
general writer methods/fields, scalar fallback and all other codecs/edits retain
their paths. The other202 comparisons and all51 passing proofs remain applicable.

The preceding [cancelable refinement reconciliation](warm-jpeg-cancelable-refinement-evidence.json)
adds one post-fast-path live-token dispatch and a guarded block reader/helper/
wrapper. Previous methods and fields are unchanged. Only ProgressiveCancelable444
replaces its old timing: the qualified short screen is1.726x/upper1.798x, with
per-bit cancellation, immediate updates and exact work/failure state retained.
Ordinary progressive paths remain unchanged; subsequent encoder updates are above.

The preceding [paired-column reconciliation](warm-bicubic-paired-columns-evidence.json)
changes only the admitted word column-filter body and its checked row-offset
storage. Mixed16, TightMixed16 and TightVertical16 have fresh qualified quiet
screens; no independent parity confirmation is claimed while their means remain
1.307–1.462x. Other resize paths retain applicable proofs. No new case or raster
was added.

The preceding [fused refinement reconciliation](warm-jpeg-refinement-fused-evidence.json)
changes ReadBlock's refinement dispatch and adds a guarded reader plus small
helpers. All five affected progressive selectors have fresh independent
comparisons. Existing fallback methods, baseline JPEG, encoders, metadata,
Stream acquisition and other codecs retain their executed source paths and proofs;
the later three resize changes are recorded above.
The [preceding Stream reconciliation](warm-png-stream-identify-evidence.json)
still applies; its common acquisition boundary is represented once, with three
separate resident parsers. At that checkpoint, 3,619 Release cases per OS and consumer/all
three examples through the same actual Release package on Windows/Linux pass.

The original audit at349fedd reconciled203 numeric rows and fourteen historical
short regressions. Subsequent changes invalidate only the identified executed
paths; source/whole-DLL hash changes alone do not invalidate unrelated methods.
The linked B01-B12 reports record each old/new path and the accepted reuse or
replacement confirmation. Latest short regressions do not substitute for an
independent current-path proof when the timed production path actually changed.

Approved baselines, exact managed pixels, source-quality/size comparisons,
complete operation owners and quiet-VM confidence requirements remain unchanged.
Centered/clamped crop, AutoOrient and ResizeToFit use the named kernels after
small geometry work; correctness/composition controls cover those aliases.
The measured scope excludes GIF/animation, unmeasured drawing/text, arbitrary profiles/
sizes/hardware, startup and concurrent throughput. Add a later case only when
concrete evidence identifies a distinct omitted required algorithm or public
boundary, with a named representative and reason. Do not expand settings grids
or retune a confirmed case without affected inputs or a demonstrated regression.

The [buffered JPEG EOB round](WARM-JPEG-BUFFERED-EOB.md) refreshes all 21 existing
noncancelable baseline JPEG decode rows, including the fresh-stream restart case.
The shared AC reader changes their timed paths; earlier C7/C9 timings are
superseded. Caller RGB420 samples retain an independent current pass at
0.926x/upper0.931x. The other 20 rows remain gaps. Progressive/live-token readers,
metadata, encoders, other codecs, editing and text retain their identified scopes.
The finite checklist remains 254 cases (237 numeric, 17 unmeasured), with 72
independent passes, 164 mean gaps and one confidence-open case. Its 206 historical
codec/edit rows still need current-admission performance evidence. Normal-operation
measurements do not qualify performance with a supplied shared root. Functional,
memory and performance qualification remain separate; full parity stays open.


Current admission-path reconciliation (2026-10-20): [TightWide8](WARM-BICUBIC-ADMISSION-CURRENT.md)
is now a current accepted-code gap at1.704x/upper1.949x,32,913B/op. Original rational
pixels,568332B local limit, normal/scalar native gates, complete packed/filter/
quantization/output ownership and official quiet guards pass. Its old independent
timing predates admission/disposal changes and remains historical. The other
numeric rows keep their identified scopes. Current totals:32normal codec/edit
rows and205historical;127independent observations and110shorts;72passes,164gaps,
oneconfidence-open. Supplied-root timing remains separate. One983sample profile
locates86.8%in the selected striphelper; four-column feasibility fits the same4KiB
workspace. No speedup is claimed before the next single-case trial.


The [four-column round](WARM-BICUBIC-FOUR-COLUMNS.md) supersedes only TightWide8:
498.19µs,1.433x/upper1.644x,32,913B/op,16.2%faster with exact pixels and the same
4KiB coarse reservation.4,475tests/OS,14focused normal/scalar/AVX512-off controls,
and the same actual private0.2.0 package/three examples pass on both OSes.
The original per-pixel arithmetic/certificates, shared-root refusal/cancellation
and survivor ownership remain intact. Default cached/word/other selected paths
retain their identified evidence; the finite scope/counts above are unchanged.
Next: current-code uncertainty on the existing VP8 SimpleOpaque8 case. No new
packet/settings matrix, retuning of passing cases or long repeat above parity.


[Current VP8 SimpleOpaque8](WARM-VP8-ADMISSION-CURRENT.md) resolves one older
admission/disposal uncertainty:1.556x/upper2.065x,760,572B/op. Original packet,
managed/native hashes, exact alpha, normal/scalar CLI, source/owners and official
quiet guards pass. Accepted C18 qualification is reused without builds. Current
normal rows become33, historical rows204; all finite counts and performance gates
remain unchanged. A current975sample profile puts76%in coefficient decoding;
one block-scoped reader is the next bounded primitive. Earlier per-Read/window/
local-state/local-work trials stay rejected; no new image or settings grid.


[Block-scoped VP8 coefficients](WARM-VP8-COEFFICIENT-BLOCK.md) save11.2% on the
matched current SimpleOpaque8 baseline. All five existing VP8 rows refresh with
exact input/frame/work/cursor/native/ownership and quiet gates. The live-token
entry predicate changes, so that row also refreshes while retaining the original
bitreader. The latest Stream AVIO/copy/EOF boundary is preserved. No separate
VP8async timing is added.4,478tests/OS,129internalnormal/scalar,305publicscalar
and the same actual private package/three examples pass. Currentnormalrows37,
historical200; all finite counts unchanged. Next: bound a distinct prepared-
double/FMA row strategy for the larger LanczosReduce16 gap before a single-case
trial. Existing separate preparation/FMA failures remain parked.

The [two-retained-output shared-root screen](WARM-COOPERATIVE-MEMORY.md) then
qualifies one previously named boundary on unchanged production:1.502x/upper1.567x,
2,574,662B/op, exact pixels/retained charges, independent survivors and both guards.
The finite scope is now254=238numeric+16unmeasured,72passes/165gaps/1confidence-open;
38 current normal/governed rows and200historical,127independent/111shorts.
The subsequent924sample diagnostic identifies Deflate walking/literal decoding,
not root admission, as the sampled cost. Next: bound one specialized materialized
block cursor, preserving original error/work/input/output/memory semantics.
No production build or repeated full/package proof;36 current adapter controls
pass normally/scalar per OS. All accepted producers and existing rows are retained.

The [work-bounded Deflate round](WARM-DEFLATE-MATERIALIZED-CURSOR.md) supersedes
those two PNG decode results after full Windows/Linux/differential/package
qualification:1.265x for overlapping caller-root outputs and1.113x for async input,
with exact pixels/work/cursor/failure state and preserved ownership. All21 async
managed controls revalidate; unchanged normal/scalar native evidence is reused.
Prior924sample fractions describe the old loop only.38current/200historical and
all finite counts stay unchanged; historical PNG timings are not relabeled.
Next: bound live-token VP8 block work settlement with every original per-decision
cancellation poll. Its1.577x current gap remains relevant because this round
changes PNG only; repeated failed Lanczos strategies remain retired.

The [subsequent VP8 round](WARM-VP8-CANCELLABLE-BLOCK.md) resolves that live-token
item with an 11.6% gain, preserving every poll and exact partial state. Five current
rows refresh; all coverage counts remain unchanged. Next: one current Reduce8
Lanczos diagnostic and feasibility of wider FP32 horizontal arithmetic with
rigorous conversion/FMA bounds and the existing exact fallback. The required
1.548x byte/1.588x word gaps remain; rejected word strategies stay retired.


The [spatial coding-group round](WARM-VP8L-SPATIAL-ENTROPY.md) supersedes the same
six lossless encode timings, all independently below parity. Photograph174748/
160032 B and diagram7636/7234 B retain open size gates.4,575 tests/OS and the same
actual0.2.0 package/examples qualify; unchanged paths retain evidence. Scope255=
239 numeric+16 pending and23 current/216 historical-or-pending stay unchanged,
including31 C43 metadata timings. Existing Opaque8 cached/raw packet170156/121896 B
is the next bounded size audit: a demonstrated gap on the distinct path skipped
by LEFT-only grouping, with no new fixture.


The [coding-cost eligibility round](WARM-VP8L-CODE-COST.md) supersedes exactly the
same six VP8L encode timings, all independently below parity. Existing Opaque8
falls170156 to123602 B against121896 B; its allocation is9669728 B/op. Primary
photo174748/160032 B and diagram7636/7234 B retain open size gates.4,576 tests/OS,
24internal+117public scalar and the same actual0.2.0 package/examples qualify.
No workload is added:255=239 numeric+16 pending,23 current/216 historical-or-pending,
73 passes/165 mean gaps/1 confidence-open,125 independent/114 short unchanged.
C43 metadata31 queue remains open. JPEG/PNG/VP8/edit/text/inspection paths unchanged.


The [default-instance font round](../docs/text-default-instance-qualification.md)
adds exactly one required algorithm case, `TextLayout/OpenTypeDefaultVariation`.
Use an authored two-glyph contextual-substitution program with two GDEF regions,
signed word/byte deltas and default-zero GPOS adjustment. This reader validates
region coordinates and delta rows, absent from the existing logical-order/Arabic cases.
Include complete owned OpenType layout, bounds measurement and disposal; exclude
immutable font preparation. Equivalent reference output/ownership, quality and
quiet-VM gates precede timing. **Unmeasured; allocations unmeasured.** No new font,
image, style, size or axis grid is introduced; private inputs are not benchmark fixtures.
Scope becomes256=239numeric+17pending;73passes/165gaps/1confidence-open,
125independent/114short,23current/216historical numeric rows,31metadata confirmations
unchanged.4,596tests/OS,105affectedscalar/OS,12publicfontcontrols/OS and the same
actual0.2.0 package/three examples qualify; codec/edit/inspection paths unchanged.


The [VP8 activity audit](WARM-VP8-ACTIVITY-AUDIT.md) reuses exactly the frozen
photograph/diagram controls and native packets. Header quantizers correlate with
source gradients, supporting one source-only spatial-quantization candidate.
No production, quality-setting, timing/allocation or checklist change follows.
All current size gates remain open; no image or quality grid is added.


The [source-only VP8 spatial policy](WARM-VP8-SPATIAL-QUANTIZATION-SCREEN.md) is
rejected at the byte/quality screen:20,284/7,868 B, source-PSNR gaps0.590/0.577dB.
References, settings and the0.25dB gate remain frozen. One Linux core build, no
scored jobs or new package proof; qualified sources/objects/core/package restored.
No coverage count or performance row changes. All size gates remain open.


The [intra16/Y2 compression milestone](WARM-VP8-INTRA16-COMPRESSION.md) reconsiders
C39 under compression-first ordering:21,202/7,642 B, source-PSNR gaps0.021969/
0.214030dB PASS, ceilings17,688/7,100 B still OPEN. The same original controls and
settings remain fixed.4,600 tests/OS,135internal+2public scalar perOS and the same
actual private package/examples pass. Two short quiet cost screens replace only
the two content VP8 rows:1.152/1.077x,601,419/487,843 B/op; CPU parity deferred.
The other five existing VP8 encode rows use Quality100/q0 and are unchanged.
Scope256=239numeric+17pending and23current/216historical stay fixed; selected numeric
ledger becomes71passes/167gaps/1confidence-open,123independent/116short. This is an
honest replacement of old independent producer passes, not a whole-scope regression
claim. No timing confirmations or unrelated optimization follow while compression
is open. The [17-row PNG packet audit](PNG-COMPLETE-PACKET-AUDIT.md) separately
finds15 retained packet-size passes and two gaps; the5% IDAT timing gate never
established complete-packet parity. Optional JPEGCompactColor remains distinct
from Original/default and from untested broader content.


The [whole-frame VP8 entropy milestone](WARM-VP8-FULL-MODEL-COMPRESSION.md) changes
only the same two content encode rows. Photograph19,808/17,688 B remains open;
diagram7,046/7,100 B now passes complete-packet size and the unchanged quality
gate. All reconstructed pixels are exact to C53; no broader content claim follows.
4,601 tests/OS,136internal+2public scalar and the same actual private package/
examples pass bothOS. Costs1.201x/1.139x and625,323/571,203 B/op are short screens;
CPU parity remains deferred. No case is added:256=239numeric+17pending,71passes/
167gaps/1confidence-open,123independent/116short,23current/216historical and31metadata
confirmations remain. Five Quality100/q0 VP8 encodes retain their branch. All other
codec/edit/font/inspection and frozen decode proofs are reused where unchanged.


The [reconstructed-mode milestone](WARM-VP8-RECONSTRUCTED-MODES.md) reuses C38's
owned six-mode search on the materially changed C54 base and adds reconstructed
four-mode chroma scoring. The fixed packets become18,772/6,752 B against17,688/
7,100 B, source-PSNR gaps0.087712/0.130170dB PASS. Photograph remains6.13% above
reference; diagram remains passing. Two short cost screens2.641x/2.006x record
660,776/554,595 B/op; CPU parity is deferred.4,603tests/OS,136internal+2public+
2differential scalar/OS and the same actual package/examples qualify. The two
new functional alpha8/16 tests cross optional-cache admission at129x129; no
performance case is added. Counts stay256=239numeric+17pending,71passes/167gaps/
1confidence-open,123independent/116short,23current/216historical,31metadata queue.
Only the same two content VP8 encode rows change; five q0 encodes and all other
codec/edit/font/inspection/frozen decode evidence retain their original scope.


The [direction-coverage milestone](WARM-VP8-DIRECTION-COVERAGE.md) changes only the
two primary content rows:all ten existing luma directions become eligible on the
admitted mixed path. Reusing the existing bounded quality sampler for the changed
photograph selects77 (six evaluations), closer0.140944dB than former78/.179085dB;
diagram75 stays valid without another search. Native75/4 and source/reference
packets/tolerances are unchanged. Both primary packet gates nowPASS:17,366/17,688
and6,722/7,100 B. This is not whole-codec parity: the [size ledger](WARM-VP8-SIZE-COVERAGE.md)
retains five q100 gaps.4,603tests/OS,136internal+2public+2diff scalar/OS and the
same actual package/examples pass. Costs3.491x/2.360x and589,432/570,136 B/op are
short screens, CPU parity deferred. No case is added:256=239numeric+17pending,
71passes/167gaps/1confidence-open,123independent/116short,23current/216historical,
31metadata confirmations unchanged. q0/other codec/edit/font/inspection/frozen
input evidence retains its original valid scope.


The [compressed-alpha milestone](WARM-VP8-COMPRESSED-ALPHA.md) supersedes exactly
three existing VP8 alpha8/16/work32M rows with current short cost screens. Each
packet saves18,504 B with exact VP8 payloads, decoded RGBA and source quality:
268,028/268,028/452,932 B versus98,590 B. All five procedural size gates remain
open. Costs0.400/0.402/0.441x and2,174,702/2,174,520/2,681,364 B/op are recorded;
CPU parity is deferred, so the three prior independent confirmations remain
historical rather than being relabeled current passes. Counts256=239numeric+
17pending,68independent passes/167gaps/1confidence-open/3cost screens,
120independent/119short,23current/216historical and31metadata confirmations.
4,613tests/OS,146internal+3public scalar and the same actual package/examples pass.
Frozen decoder corpus packets remain unchanged. Opaque q0, primary photograph/
diagram, other codecs/edit/font/inspection paths reuse unchanged relevant proofs.


The [alpha backward-reference milestone](WARM-VP8-ALPHA-MATCHES.md) refreshes four
existing unrestricted q0 rows. Full-frame coefficient fitting saves2,036/2,048 B;
bounded ALPH matches then save95,784 B. Alpha8/16 now170,196/98,590 B, opaque8/16
149,570/72,380 B; all five procedural packet gates remain open. Exact old decoded
RGBA/source quality, settings/reference packets and gates are unchanged. The
work32M packet/work remains452,932 B/31,961,577, retaining its C58 short cost proof.
4,628tests/OS,161internal+51public+2diff scalar and the same actual package/examples
qualify. Four short costs1.397/1.426/0.523/0.532x and allocations1,543,152/1,543,152/
1,793,800/1,793,800 B/op replace only these existing rows. CPU parity is deferred;
no independent repeats, new workload or codec/edit/font/inspection invalidation.
Counts256=239numeric+17pending;68independentpasses/167gaps/1confidence-open/
3cost screens,120independent/119short,23current/216historical and31metadata queue.
The unadopted gradient-only candidate does not introduce another required path.


The [limited-work prefix milestone](WARM-VP8-PREFIX-WORK.md) refreshes exactly
WarmWebp/EncodeBudget8:452,932→268,028 B with exact old RGBA/source quality and
31,818,464 actual work units inside the unchanged32M cap. The target98,590 B is
still open. Other four procedural records remain byte/work/quality-identical;
primary content and unrelated algorithms retain their relevant unchanged proofs.
4,635tests/OS,163internal+8public+2diff scalar and the same actual package/examples
qualify. One short cost records0.396x/upper0.429x and2,174,726 B/op, without an
independent repeat or CPU-parity claim. The seven added tests do not expand the
performance matrix:256=239numeric+17pending,68independentpasses/167gaps/
1confidence-open/3cost screens,120independent/119short,23current/216historical;
31metadata confirmations unchanged. The earlier190B q0 luma trial is retained
without adoption; no timing/full/package repeat was spent on that small result.


The [single-pass alpha milestone](WARM-WEBP-ALPHA-SINGLE-PASS.md) supersedes only
WarmWebp/EncodeBudget8:268,028→208,216 B, exact prior RGBA/source quality and
31,997,511 actual units inside32M. ALPH114,363→54,552 B; the98,590 B packet target
remains open. Four unrestricted records remain byte/work/quality-identical.
4,644tests/OS,172internal+8public+2diff scalar and the same actual package/examples
qualify charged staging and terminal failures. One short cost records0.385x/
upper0.418x and1,707,064 B/op; no independent repeat or CPU-parity claim.
Nine new correctness cases do not expand the finite performance matrix:
256=239numeric+17pending,68independentpasses/167gaps/1confidence-open/3cost screens,
120independent/119short,23current/216historical;31metadata confirmations unchanged.


The [alpha-token milestone](WARM-WEBP-ALPHA-TOKENS.md) supersedes only existing
WarmWebp/EncodeBudget8:208,216→181,998 B (12.6%), exact prior RGBA/source quality,
31,980,594 actual units inside32M. ALPH54,552→28,333 B; complete target98,590 B
remains open. Four unrestricted records remain byte/work/quality-identical.
4,654tests/OS,182internal+8public+2diff scalar and the same actual package/examples
qualify token/encoded overlap and terminal failures. One short cost records0.391x/
upper0.435x and2,295,207 B/op, without an independent repeat or CPU-parity claim.
Ten new tests do not expand the matrix:256=239numeric+17pending,68independentpasses/
167gaps/1confidence-open/3cost screens,120independent/119short,23current/216historical;
31metadata confirmations unchanged. The preceding coefficient/EOB candidate saves
1528/1536B and remains unadopted for later; the larger alpha gain took priority.


The [adaptive predictor plus groups milestone](WARM-VP8L-ADAPTIVE-GROUPS.md) saves
2,330B on the fixed lossless photograph:174748→172418/ref160032. Diagram7636/7234,
opaque123602/121896 and alpha8/16+async145648/183812 retain packets. Complete-packet
lossless gates remain open. Exactly six existing VP8L encode rows replace C49
independent timings with current compression-first short costs; old producer proofs
remain historical.62independentpasses/163gaps/1confidence-open/13costscreens,
114independent/125short;256=239numeric+17pending,31metadata confirmations unchanged.
4,772tests/OS,37internal+117public scalar and the same actual package/examples
qualify. No public/differential/harness/native/selector build or independent timing
repeat. All seven lossy size/quality gates stay closed; no new required workload.


The [residual-color milestone](WARM-VP8L-RESIDUAL-COLOR.md) saves2,610B on the fixed
lossless photograph:172418→169808/ref160032. Five other packets remain unchanged:
diagram7636/7234,opaque123602/121896 and alpha8/16+async145648/183812. All three
lossless packet gaps remain open. Six compression-first short costs replace C91
shorts, no independent timing repeat or additional workload.4,783tests/OS,
48internal+117public scalar and the SAMEactualpackage/all3examples qualify inherited
predictor and color ownership, refusal/cancellation/work cleanup and exact outputs.
No public/differential/harness/native/selector build. All seven lossy gates stay
closed. Finite counts remain256=239numeric+17pending,62independentpasses/163gaps/
1confidence-open/13costscreens,114independent/125short;31metadata timings pending.


The [uncached-model milestone](WARM-VP8L-NO-CACHE.md) saves5,800B on the fixed
lossless photograph:169808→164008/ref160032. Five other packets stay unchanged:
diagram7636/7234,opaque123602/121896 and alpha8/16+async145648/183812. Three
lossless complete-packet gaps remain open. Exactly six existing short cost rows
replace C93; CPU parity/independent repeats remain deferred.4,792tests/OS,
57internal+117public scalar and the same actual package/all3examples qualify
inherited owners, green280 models, terminal failure cleanup and exact outputs.
No public/differential/harness/native/selector build or new required workload.
All seven lossy gates stay closed. Counts remain256=239numeric+17pending,
62independentpasses/163gaps/1confidence-open/13costscreens,114independent/125short;
31metadata confirmations pending. JPEG optional CompactColor and two PNG complete
packet gaps retain their separately stated open/default scope.


The [bounded-reference milestone](WARM-VP8L-REFERENCES.md) closes the fixed diagram
complete-packet gate:7636→7160/ref7234B, exact pixels,74B headroom. Photograph
164008/160032 and opaque123602/121896 stay open; alpha8/16+async145648/183812 pass
unchanged. All seven lossy gates stay stopped with identical primary anchors.
Exactly six short costs replace C97 because the common scanner changed; no new
required workload or independent timing repeat. Diagram extraCPU/allocation is
accepted for compression:2.318x/upper3.789x,1721182B/op.4,804tests/OS,
69internal+117public scalar and the SAME package/all3examples qualify retained
token capacity, replacement peaks, terminal cleanup and exact independent outputs.
Other relevant evidence is reused. Finitecounts256=239numeric+17pending,
62independentpasses/163gaps/1confidence-open/13costscreens,114independent/125short;
31metadata confirmations pending. CPU parity remains deferred; optional JPEG
CompactColor does not close the default gate, and two PNG complete-packet gaps
remain open. Passing diagram compression stops here.


The [code-cost predictor milestone](WARM-VP8L-CODECOST-PREDICTION.md) saves2,598B
on the fixed photograph:164008→161410/ref160032. Diagram7160/7234 passes unchanged;
opaque123602/121896 remains open, alpha8/16+async145648/183812 pass unchanged.
All seven lossy gates retain identical primary anchors and stay stopped.
Exactly six current costs replace C99 after the shared entry change; no independent
repeat or new material workload. Photograph extraCPU/allocation is accepted for
compression:4.800x/upper5.633x,44090320B/op.4,815tests/OS,80internal+117publicscalar
and the SAMEpackage/all3examples qualify predictor replacement, shared color-owner
transfer, terminal cleanup and exact independent outputs. Other valid proofs reused.
Finitecounts256=239numeric+17pending,62independentpasses/163gaps/1confidence-open/
13costscreens,114independent/125short;31metadata confirmations pending. CPU parity
deferred; optional JPEGCompactColor does not close its default gate, and two PNG
complete-packet gaps remain open. Opaque now has the larger remaining VP8L gap.


The [seeded-reference milestone](WARM-VP8L-SEEDED-REFERENCES.md) closes the opaque
complete-packet gap:123602→41088/ref121896 B. Exact alpha8/16/async improves
145648→60904/ref183812 B. Diagram7160/ref7234 passes unchanged. These cases stop;
photograph161410/ref160032 is the only remaining declared VP8L gap(1378 B).
All seven lossy gates keep identical primary anchors and remain stopped. Exactly
six current costs replace C104 after the shared entry change;18actuals/12passing
idle-v2guards, no independent repeats or new required workload. Extra CPU/allocation
is accepted for compression: opaque6.927x/upper8.486x,53104872 B/op.
4828tests/OS,93internal+117publicscalar and the SAMEpackage/all3examples qualify
retained capacity, replacement peaks, borrowed trees, terminal cleanup and exact
independent outputs. A single-chunk async packet required only an untimed sink-check
fix; one Linux harness build, same timed boundary/drivers/worker. Other valid proofs
reused. Finite256=239numeric+17pending,62independentpasses/163gaps/1confidenceopen/
13costscreens,114independent/125short;31metadata confirmations pending. CPU parity
remains deferred; optional JPEGCompactColor does not close its default gate, and
two PNG complete-packet gaps remain open. No broad content/codec parity claim.


The [bounded second predictor refinement](WARM-VP8L-SECOND-REFINEMENT.md) saves492 B:
photograph161410→160918/ref160032,886 B still open. At most two passes, second only
following a first accepted map; fixed color owner/parser/settings and all resource
contracts. Six new second-replacement/guard/terminal-cleanup cases;4834tests/OS,
99internal+117public scalar, SAMEactualpackage/all3examples pass. One photo short
cost(3actuals/2passing idle-v2guards) replaces C110:5.721x/upper7.099x,51847984 B/op.
Five other C110 observations/allocations/guards/producer identities are reused after
proving unchanged early-return paths and identical complete packets, including async.
A launcher-resolution setup failure produced zero actuals; its uniquely named retry
is retained, without rebuild or qualification repeat. No independent timing repeat,
workload expansion or CPU-parity acceptance. Passing controls remain stopped; default
JPEG and two PNG packet gaps stay open. Finite256=239numeric+17pending remains
62independentpasses/163gaps/1confidenceopen/13costscreens,114independent/125short;
31metadata confirmations pending. All existing full objectives remain open.


The [color refit after predictor changes](WARM-VP8L-COLOR-REFIT.md) saves306 B:
photograph160918→160612/ref160032,580 B still open. One existing coefficient recipe,
fixed predictor/cache0/parser and one groupfit; admitted color replacement borrows
one predictor owner. Ten new ownership/guard/terminal-cleanup/hidden-RGB cases;
4844tests/OS,109internal+117public scalar, SAMEactualpackage/all3examples pass.
One photo shortcost(3actuals/2passing idle-v2guards) replaces C112:6.636x/upper7.566x,
59631048 B/op. Five C110 observations/allocations/guards/producer identities are
reused after proving skipped new analysis and identical complete packets, including
async. No new build for public/diff/harness/drivers/native/selector, no independent
timing repeat, workload expansion or CPU-parity acceptance. Passing controls stop;
default JPEG and two PNG packet gaps stay open. Finite256=239numeric+17pending stays
62independentpasses/163gaps/1confidenceopen/13costscreens,114independent/125short;
31metadata confirmations pending. Full existing objectives remain open.


The [color-code-cost milestone](WARM-VP8L-COLOR-COST.md) closes the last declared
VP8L gate: photograph160612→159104/ref160032,928 B below reference. Exact normalized
and hidden RGBA; diagram7160/7234,opaque41088/121896,alpha8/16/async60904/183812
unchanged/pass. All SIX declared complete-packet gates stop, with the bounded scope
and ledger retained; no arbitrary-content/profile claim. All seven lossy gates stay
stopped with identical primary anchors and unchanged ALPH helpers.4856tests/OS,
121internal+117public scalar,SAMEactualpackage/all3examples qualify. One currentphoto
shortcost(3actuals/2passing idle-v2guards) replaces C114:7.330x/upper8.250x,67414112 B/op.
Five C110 observations/allocations/guards/producers reused after skip/identical-packet
proof, including async. SPEC status figures were corrected after qualification;
API/targets/tolerances and binaries unchanged, measured SPEC retained, no repeat.
Onecore/internalbuildperOS,zero public/diff/harness/native/driver/selectorbuild.
No new workload or independent CPU pass. Finite256=239numeric+17pending stays
62independentpasses/163gaps/1confidenceopen/13costscreens,114independent/125short;
31metadata confirmations pending. Compression now moves to default Original JPEG
and two PNG complete-packet gaps; optionalCompactColor remains separate. Full warm
CPU/P4/readiness objectives remain OPEN.
