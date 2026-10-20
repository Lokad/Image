# Lokad.Image benchmark state

Updated 2026-10-21. Priority: **complete-packet compression at the frozen quality
gates**, lossy WebP first, then lossless WebP. Meaningful verified incremental
gains may cost more CPU. Warm single-CPU parity (independently confirmed upper≤1x)
remains a deferred objective; speed-only work and repeated confirmations wait
while compression gates remain open. The 239 numeric rows retain tested-producer evidence: **68 passes,
163 gaps, 1 confidence-open case and 7 short cost screens**. The finite
[material checklist](../benchmarks/WARM-COVERAGE.md) has 256 named cases, 17 unmeasured.
The completed ≤2x phase belongs to 0.1.0; compression currently takes precedence over CPU parity. Results apply
to the named fixed inputs/settings/hardware, without extrapolating to other profiles.

These tables retain the tested producers identified by each report, including
explicitly labeled compression-first cost screens.
The [typed source-metadata extension](source-metadata.md) is functionally qualified
by4,571 tests/OS,259 affected scalar checks/OS and the same actual private package
consumer/all three examples on Windows/Linux. **31 marked decode/inspection rows
predate its changed header preflight and await timing confirmation.** The complete
metadata-returning PNG reference also needs equivalent typed fields before timing.
There are23 unaffected current rows (two caller JPEG encodes, four Lanczos3, four
async outputs, one bicubic, seven VP8 encodes and five synchronous lossless encodes),
and216 historical/awaiting-confirmation rows. Pixel kernels, source-quality/resource
proofs and unaffected encoder/edit timings retain their identified scope. This does
not prove expanded-scope parity or replacement readiness. Published0.1.0 is immutable.
The checkpoint descriptions below retain their linked producer scopes; the metadata
qualification above supersedes any earlier description of these31 rows as current.

[Buffered JPEG EOB decoding](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) saves 10.0%
on sparse-2D CMYK and 12.2% on one-AC CMYK, preserving exact pixels and allocation
behavior. All21 JPEG rows were qualified at that checkpoint. Caller RGB420
had an independent pass at **0.926x/upper0.931x**; the other20 were gaps.
The unchanged progressive/live-token decoders, metadata, encoders and other
codecs retain their identified scopes. A late affected-list correction included
the already required fresh-stream JPEG case, with no new input or settings.
Full suites reused the verified frozen build through `test.ps1 -NoBuild`;
actual-package consumers and all three examples pass on both OSes.
Largest current gaps are now Lanczos3 reduction, **1.588x** at 16 bits and **1.548x**
at 8 bits. An [exact unsigned-product trial](../benchmarks/WARM-LANCZOS3-PRODUCT-TRIAL.md)
regressed 4.9% and was rejected. A current 944-sample profile puts 50.4% in RGBA16
horizontal filtering. The [vertical-first trial](../benchmarks/WARM-LANCZOS3-VERTICAL-FIRST-TRIAL.md)
cut allocation 71.1% but had no elapsed gain and was also rejected. Accepted
production is restored. [Four-column bicubic strips](../benchmarks/WARM-BICUBIC-FOUR-COLUMNS.md)
then save **16.2%** on TightWide8: **1.433x/upper1.644x**, 32,913 B/op, with exact
pixels and the same 4 KiB reservation. Full suites and the same actual package
qualify both OSes. Only that existing row changes. The
[original VP8 block reader](../benchmarks/WARM-VP8-COEFFICIENT-BLOCK.md) saved 11.2%
at its checkpoint. A [live-token block helper](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md)
now saves **11.6%**, reaching **1.395x/upper1.437x**, with 760,572 B/op unchanged,
while preserving every per-decision cancellation poll and exact failure state.
All five existing VP8 rows refresh; four control point means are 0.5–1.8% slower,
with all quality/owner/quiet gates retained and no small-movement retuning. A
[prepared-double Lanczos trial](../benchmarks/WARM-LANCZOS3-PREPARED-FMA-TRIAL.md)
is correct but 1.4% slower, at 154,894 B/op, and is rejected. Accepted production
and all selected rows remain unchanged. The already named
[caller-owned-root baseline](../benchmarks/WARM-COOPERATIVE-MEMORY.md) led to a
[materialized Deflate cursor](../benchmarks/WARM-DEFLATE-MATERIALIZED-CURSOR.md):
two retained decodes improve **15.8%**, to **1.265x/upper1.304x**, 2,574,671 B/op.
The admitted peak stays 2,306,560 bytes including resident input. Async PNG decode
improves **23.2%**, to **1.113x/upper1.176x**, at unchanged 12,784 B/op. Exact
pixels/work/failure state, independent survivors, refusal/cancellation cleanup and
both guards pass. All21 async managed controls revalidate; unchanged native
controls are reused. Two current rows refresh; previously historical PNG decode
rows remain historical. Metadata, encoding and other codec/editing paths retain
their scopes. The 924-sample profile describes the previous producer only.
The [FP32 byte Lanczos trial](../benchmarks/WARM-LANCZOS3-FP32-BYTE-TRIAL.md)
is rejected (50.7% slower); exact accepted production and tests are restored.
All60 focused normal/scalar/AVX512-off controls pass perOS. New sampled profiles
retain their respective producers. No selected row or full/package proof repeats.
The [current resident PNG checks](../benchmarks/WARM-CURRENT-RESIDENT-PNG.md)
resolve those two old measurements without builds or repeated qualification;
RGBA8 is1.134x/upper1.330x and RGBA16 is1.039x/upper1.064x.
Both remain gaps; no longer repeat. The
[paired-integer Lanczos trial](../benchmarks/WARM-LANCZOS3-PAIRED-BYTE-TRIAL.md)
parks its small3.5% point-mean gain; once-prepared factor packing is neutral.
Both variants retain exact pixels, original controls and admitted storage;
accepted production is restored with no full/package repeat. The
[current sparse JPEG diagnostic](../benchmarks/WARM-JPEG-SPARSE-COST.md) places
20.4% of963samples in the integer inverse transform; entropy/scan work is also
material. No timing claim. The stronger
[exact-quadrant trial](../benchmarks/WARM-JPEG-SPARSE-QUADRANT-TRIAL.md)
reduces frequency products64→16 but only saves a0.6% point mean; it is parked
and accepted production is restored without full/package repeats. The
[current direct-RGB diagnostic](../benchmarks/WARM-JPEG-DIRECT-RGB-COST.md)
places 47.6% of934 samples in baseline AC reading and1.3% in SIMD RGB packing.
The frozen Sparse2D packet contains no stuffed bytes. No timing claim or new
workload matrix. JPEG tuning is deferred while lossless WebP size takes priority.
[Certified FMA Lanczos3](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) saves
13.6–23.8%, with fewer allocations and exact pixels; its four gaps are now
1.061–1.588x. Two full-FMA horizontal trials and the earlier prepared-row trial
were rejected. Earlier profiles describe their original producers only.
Optimize demonstrated gaps; stop passing cases. No pixel precision is relaxed.
The [sparse JPEG FP32-FMA trial](../benchmarks/WARM-JPEG-SPARSE-FMA-TRIAL.md)
regressed7.2%. Subsequent [storage trials](../benchmarks/WARM-JPEG-STORAGE-TRIALS.md)
cut allocation 48.9–86.9%, but failed to improve elapsed time. Those candidates were
removed; their ownership checks passed. A [constant-DC trial](../benchmarks/WARM-JPEG-DC-TRIAL.md)
saved only 1.2% and was parked. Accepted integer/EOB rounds preserve exact pixels;
full parity remains open.

Complete async PNG and lossless WebP outputs independently pass. WebP uses approved
libwebp exact=1 (FFmpeg encoder backend), retaining exact hidden RGB and compressed
sizes **145,648/183,812 B**. Its passing timing does not establish compression parity.
Lossless size parity is a high-priority open gate: the fixed opaque photograph
must reach ≤160,032 B (currently174,748 B), and diagram ≤7,234 B (currently7,636 B),
with exact pixels and the unchanged libwebp exact=1 reference/settings. The
[caller-JPEG controls](../benchmarks/WARM-JPEG-CALLER-MCU.md) also leave a joint
size/quality gap: RGB42074,040/52,093 B at source-PSNR difference0.106110 dB
(gate0.25 dB); grayscale58,823/58,910 B. RGB420 compression improvements must
retain matched reconstruction quality and avoid material grayscale regression.
These content/sampling controls do not establish general compression equivalence.
The [spatial coding-group round](../benchmarks/WARM-VP8L-SPATIAL-ENTROPY.md)
reduces the fixed photograph from184,518 to174,748 B; diagram remains7,636 B.
The subsequent [coding-cost eligibility round](../benchmarks/WARM-VP8L-CODE-COST.md)
reduces the existing opaque control from170,156 to123,602 B against121,896 B.
All six affected complete encodes independently pass warm time parity;4,576 tests/OS
and the same actual package/examples qualify. Opaque allocation is9,669,728 B/op. This spends
more CPU and allocation for size: photograph9,912,952 B/op versus historical702,616.
Both lossless size gates remain open against160,032/7,234 B. The earlier
[structure audit](../benchmarks/WARM-VP8L-COMPRESSION-STRUCTURE.md) is diagnostic evidence.
The [direction-coverage milestone](../benchmarks/WARM-VP8-DIRECTION-COVERAGE.md)
now passes complete-packet size and quality for **both primary controls**:
17,366/17,688 B photograph and6,722/7,100 B diagram. The changed photograph uses
managed77 selected by the existing bounded closest-quality matcher; diagram75
is retained, native75/effort4 and all reference packets/inputs unchanged. Gaps
0.140944/0.117409dB retain the0.25dB gate and exact alpha. The former78 photograph
point remains18,284 B/0.179085dB; no claim of size parity at that older number.
4,603 tests/OS,136internal+2public+2differential scalar checks and the same actual
private package/examples qualify. Two short cost screens record3.491x/2.360x
and589,432/570,136 B/op; CPU parity remains deferred. Relevant unchanged proofs
are reused, with no independent timing repeats or workload grid.
The [seven-control size ledger](../benchmarks/WARM-VP8-SIZE-COVERAGE.md) retains
**five other fixed-q100 packet gaps**, including the work32M branch.
Those are benchmark inputs, not the defaultquality85. Primary parity is not
whole-codec parity; these larger lossy gaps precede lossless work. The earlier
[intra16/Y2](../benchmarks/WARM-VP8-INTRA16-COMPRESSION.md),
[whole-frame entropy](../benchmarks/WARM-VP8-FULL-MODEL-COMPRESSION.md) and
[reconstructed-mode](../benchmarks/WARM-VP8-RECONSTRUCTED-MODES.md) milestones
retain their producer-specific data. Only the two primary encode rows change.
The subsequent [compressed-alpha milestone](../benchmarks/WARM-VP8-COMPRESSED-ALPHA.md)
saves18,504 B on each existing alpha8/16/work32M packet, now268,028/268,028/452,932 B
against98,590 B. VP8 payloads, decoded RGBA and quality remain exact; references,
settings and tolerances are unchanged. It adds2KiB admitted workspace without
a filtered/RGBA frame or encoded-alpha buffer.4,613tests/OS,146internal+3public
scalar checks and the same actual package/examples pass. Three short cost screens
record0.400/0.402/0.441x and2,174,702/2,174,520/2,681,364 B/op; native allocations
remain unmeasured. They replace three prior independent observations, without
claiming current independent CPU parity. The fixed scope remains256 cases:
120independent/119short observations. All five procedural compression gates remain
open. Primary opaque content and unrelated algorithms reuse unchanged evidence.
The [alpha backward-reference milestone](../benchmarks/WARM-VP8-ALPHA-MATCHES.md)
then cuts unrestricted alpha8/16 packets268,028→170,196 B (36.5%) with exact old
RGBA/source quality. ALPH itself is18,579/24,588 B; complete packets still exceed
98,590 B. Full-frame q0 entropy also reduces opaque8/16 to149,570/72,380 B. The
work32M packet/work remains452,932 B/31,961,577 units, reusing its C58 short proof.
4,628tests/OS,161internal+51public+2diff scalar and the same actual private package/
examples qualify. Four short costs record1.397/1.426/0.523/0.532x and
1,543,152/1,543,152/1,793,800/1,793,800 B/op. CPU parity stays deferred. The owned
262,176 B history/tables are admitted through the shared root and survive actual
writing; there is no filtered/RGBA frame or encoded-alpha staging copy. A weaker
gradient-only trial is retained but not adopted, avoiding another budget-specific
algorithm. No source/settings/quality-gate change, new workload or timing repeat.
The five procedural packet gaps, primary content passes and finite counts remain.
The [alpha-token milestone](../benchmarks/WARM-WEBP-ALPHA-TOKENS.md)
reduces the work32M packet208,216→181,998 B (12.6%), with exact prior RGBA/source
quality and31,980,594 charged units inside32M. ALPH falls54,552→28,333 B; the
complete98,590 B target stays open. Same-root token/staging overlap, terminal
failures and surviving outputs pass4,654tests/OS, affected scalar checks and the
same actual package/examples. One cost screen records0.391x/upper0.435x and
2,295,207 B/op; native allocations are unmeasured and CPU parity/repeats deferred.
One immutable core/internal build perOS; public/diff/reference/harness/native/
selector binaries reused. Four unrestricted records remain exact; all five
procedural packet gates and the finite matrix remain unchanged. The earlier
[prefix](../benchmarks/WARM-VP8-PREFIX-WORK.md) and
[fixed-stage](../benchmarks/WARM-WEBP-ALPHA-SINGLE-PASS.md) proofs are historical.

The [source-quantization milestone](../benchmarks/WARM-VP8-SOURCE-QUANTIZATION.md)
cuts opaque149,570→83,838 B (ref72,380), alpha170,196→108,880 B (ref98,590)
and work32M181,998→120,146 B (ref98,590). Exact alpha and unchanged quality gates
pass; all five packet gaps remain open. Primary photograph/diagram packets,
pixels and quality stay exact. The new source-only analysis reserves its map
through the borrowed root, without another RGBA frame;31,752,723 actual units
fit32M. Both OSes pass4,666tests,194internal+8public+2diff scalar checks and the
same actual private package/all three examples. Seven short guarded jobs replace
all affected VP8 encode costs, including unchanged primary packets; no independent
timing repeat or CPU-parity claim. Opaque8/16 allocate1,002,712 B/op, alpha8/16
1,532,200, work32M2,033,809, photograph590,152 and diagram570,856. Native allocations
are unmeasured. Unchanged decoder inputs and other production paths retain their
qualified scopes; no workload, reference, input or tolerance changes.

The [interpolation-noise milestone](../benchmarks/WARM-VP8-INTERPOLATION-NOISE.md)
further cuts opaque83,838→74,962 B, alpha108,880→99,816 B and work32M120,146→111,094 B.
The respective complete references remain72,380/98,590/98,590 B; all five gates
stay open. Source-PSNR gaps0.016224/0.038933 dB, exact alpha and unchanged decoder
bounds pass; primary packets/pixels/quality remain identical. Independent noise
propagation includes centered chroma weights and coincident edge taps, with no
new buffer and unchanged admission/ownership. Actual31,916,945 units fit32M.
Both OSes pass4,674tests,202internal+8public+2diff scalar checks and the same actual
private package/all examples. Seven short guarded costs supersede affected encodes:
opaque8/16 allocate1,002,488 B/op, alpha8/16 1,532,032, work32M1,806,692, photograph
590,152 and diagram570,856. No independent timing repeat or CPU-parity claim;
one frozen producer/internal build perOS, unchanged public/diff/harness/native/
selector binaries reused. Other production paths retain their qualified scopes.

The [alpha-anchor milestone](../benchmarks/WARM-WEBP-ALPHA-ANCHORS.md)
reduces the work32M packet111,094→104,500 B (ref98,590), preserving exact prior
VP8/RGBA/source quality and all reference/settings/gates. ALPH28,333→21,740 B
beats the24,588 B component reference; the full packet remains5,910 B above target.
One token-start anchor per hash supplements rolling recent positions, under
131,136 B of admitted history ownership. Actual31,970,409 units fit32M; no extra
source pass/filtered plane/frame. BothOS4,682tests,210internal+8public+2diff scalar
and same actual package/all examples pass. One guarded cost screen replaces this
row:15,008.23/reference53,686.75 µs and2,012,156 B/op (prior1,806,692 B/op).
No CPU veto/independent repeat; four unrestricted records and primary opaque
paths retain six unchanged C70 cost/quality proofs. Five packet gates remain open.

The current [segment-search milestone](../benchmarks/WARM-VP8-SEGMENT-SEARCH.md)
admits existing intra16/Y2 and reconstruction/rate search for source-derived
segments at fixed100/baseq0. Opaque74,962→73,740 B (ref72,380), unrestricted
alpha99,816→98,760 B (ref98,590); gaps1,360/170 B remain. Quality gaps0.008682/
0.030410 dB pass with exact unchanged ALPH. Four guarded costs replace these rows:
59,760.91/60,143.94/67,738.66/68,204.81 µs and1,006,048/1,006,048/1,537,008/
1,537,008 B/op. These higher CPU costs are accepted for incremental compression,
without CPU-parity/independent-repeat claims. BothOS4,688tests,212internal+12public+
6diff scalar and the same actual package/all examples qualify. Entropy-only exact
comparisons useeffort3; separate78/100 controls qualify search at both depths.
Passing internal/public suites were reused after two differential scope assertions
were corrected. One frozen producer perOS, no harness/native/selector build.
The work32M full record is EXACT C72 (104,500 B/31,970,409units), primary paths
are unchanged, and those three identified cost/quality proofs are retained. No
benchmark input/reference/settings/tolerance or required-workload expansion.

The [coefficient-rate audit](../benchmarks/WARM-VP8-COEFFICIENT-RATE-AUDIT.md)
rejects a smaller packet with excessive quality loss and a quality-preserving
variant with immaterial byte savings. These historical trials do not alter the
current accepted producer or quality gates.
The [source-activity audit](../benchmarks/WARM-VP8-ACTIVITY-AUDIT.md) finds strong
native-quantizer/gradient rank correlation on the same two controls, supporting a
source-derived spatial-quantization experiment. It changes no production packet,
quality setting, timing or allocation; both size ceilings remain open.
The [source-only spatial policy](../benchmarks/WARM-VP8-SPATIAL-QUANTIZATION-SCREEN.md)
then fails the unchanged source-quality gate (0.590/0.577dB) and both size ceilings.
It is rejected without timing; the qualified source/build/package are restored.
The [lossless adaptive-predictor screen](../benchmarks/WARM-VP8L-ADAPTIVE-PREDICTOR-AUDIT.md)
saves2,652/0 B on the fixed photograph/diagram but misses both lossless size gates.
That predictor is not adopted; the subsequent coding-group round above supplies current production evidence.
The [opaque coding-cost audit](../benchmarks/WARM-VP8L-OPAQUE-COST-AUDIT.md)
identified the skipped plan subsequently selected and qualified above. Its original
untimed evidence remains historical; the six table rows use the accepted producer.

The two JPEG async-output cases remain short gaps; all timings/allocations follow.

The 17 unmeasured representatives cover header inspection, drawing and text.
[PNG fixed-workspace controls](../benchmarks/WARM-PNG-HEADERS-CONTROLS.md)
agree on all 21 original cases, including deliberately opaque CRC/Deflate. Timing
awaits the explicitly labeled bounded FFmpeg AVIO/descriptor adapter decision;
complete-packet parsing would add whole-input buffering outside this boundary.
Full warm parity and release readiness remain open.
The [default-instance font extension](text-default-instance-qualification.md) passes
4,596 correctness tests/OS,105 affected scalar checks, twelve public font controls
and the same actual package/examples on both platforms. It changes no codec/edit/
inspection benchmark path. One authored default-variation layout representative
is added because region/row validation is a distinct required path; it awaits
reference-boundary qualification and timing. Existing timing evidence is reused.


JPEG caller-sample packets are **58,823/58,910 B** grayscale (managed/SIMD libjpeg-turbo),
**74,040/52,093 B** Original RGB420, and **48,701/52,093 B** opt-in CompactColor
RGB420 (managed/FFmpeg). Source-PSNR gaps are0.000033/0.106110/0.017274dB inside
0.25dB. Equal quality numbers are not the matching rule; the
[optional-policy screen](../benchmarks/WARM-JPEG-COMPACT-COLOR.md) includes complete
time and allocations, while retaining the Original control. WebP photograph/diagram
lossless sizes are174,748/160,032 B
and 7,636/7,234 B; [lossy sizes](../benchmarks/WARM-VP8-DIRECTION-COVERAGE.md)
are 17,366/17,688 B and 6,722/7,100 B at the documented matched source quality (managed77/75, native75). Compression-
efficiency parity is not established. Current operation timings/allocations follow.

Dedicated AMD EPYC9V45 VM; Ubuntu24.04.4; .NET10.0.8/SDK10.0.204;
FFmpeg n8.1 (`9047fa1b084f`). One pinned CPU, one codec/conversion/filter thread,
normal SIMD, tiering off, nonconcurrent workstation GC. Inputs are the two original
513×259 RGBA8/16 rasters, the fixed photograph/text diagram pair and documented projections; exact case dimensions and
policies are in linked reports. Only the two required content controls were added; no settings grid.

Times include complete public operations, admission, conversion, fresh independent
output ownership and disposal. Native contexts may be reused; managed private
storage may be reused after release. No decoded output or descriptor is cached.
Fresh Stream rows include source-wrapper/input acquisition, reads/copies/EOF and
release. Startup, I/O, IPC, preparation/hashing and validation are excluded.

Ratio = Lokad.Image / faster native pre/post mean; lower is faster. Upper retains
the larger selected/all-actual confidence bound against the faster native bracket's
lower bound. Parity confirmed requires an independent quiet repeat with upper≤1;
a mean≤1 with upper>1 stays confidence-open. Gap rows exceed the new target.
Independent jobs retain all 12 actuals; explicitly labeled short screens retain
all three. Both retain 5s `exclusive-vm-idle-v2` guards. Earlier records
retain their original protocol and failures. **B/op** is managed allocation from
BenchmarkDotNet/raw GC bytes per operation, rounded up. Native allocations are
**unmeasured**, not zero. Timings were collected across rounds; unchanged-path
reconciliation now covers every numeric row.

**Decode and metadata**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG RGBA8, two retained outputs / caller-owned memory root](../benchmarks/WARM-DEFLATE-MATERIALIZED-CURSOR.md) | 3,604.48 | 2,850.06 | 1.265 / 1.304 | 2,574,671 | Gap (short; qualified); metadata timing open |
| [PNG DecodeAsync, fresh nonseekable Stream](../benchmarks/WARM-DEFLATE-MATERIALIZED-CURSOR.md) | 1,574.38 | 1,414.79 | 1.113 / 1.176 | 12,784 | Gap (short; qualified); metadata timing open |
| [PNG IdentifyAsync, fresh nonseekable Stream¶](../benchmarks/WARM-PNG-ASYNC-IDENTIFY.md) | 24.10 | 19.34 | 1.246 / 1.571 | 8,408 | Gap (short; qualified); metadata timing open |
| [PNG Identify, fresh Stream¶](../benchmarks/WARM-PNG-STREAM-IDENTIFY.md) | 22.94 | 15.87 | 1.445 / 1.459 | 184 | Gap (independent) |
| [WebP Identify, resident span (parser)](../benchmarks/WARM-WEBP-IDENTIFY.md) | 0.02 | 4.54 | 0.004 / 0.004 | 88 | Parity confirmed |
| [JPEG Identify, resident span‖](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 0.59 | 1.21 | 0.488 / 0.494 | 88 | Parity confirmed |
| [PNG Identify, resident span¶](../benchmarks/WARM-PNG-IDENTIFY.md) | 16.09 | 13.04 | 1.234 / 1.238 | 88 | Gap (independent) |
| [PNG palette4/full tRNS Adam7](../benchmarks/WARM-PNG-PALETTE4-ADAM7.md) | 1,480.35 | 1,135.35 | 1.304 / 1.387 | 2,592 | Gap (independent) |
| [PNG8 Stream, orient/crop/nearest fit](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,044.52 | 1,476.97 | 1.384 / 1.613 | 701,312 | Gap (short; qualified) |
| [PNG16 Stream, orient/crop/nearest fit](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 3,355.49 | 2,946.76 | 1.139 / 1.213 | 1,401,314 | Gap (short; qualified) |
| [PNG RGBA8, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 1,918.97 | 1,393.95 | 1.377 / 1.583 | 4,552 | Gap (short; qualified) |
| [PNG RGBA16, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 3,054.27 | 3,091.94 | 0.988 / 0.991 | 8,648 | Parity confirmed |
| [JPEG420 restart, fresh seekable Stream](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,329.08 | 996.80 | 1.333 / 1.411 | 761,840 | Gap (short; qualified); metadata timing open |
| [WebP VP8 + alpha, fresh seekable Stream](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md) | 8,284.99 | 5,977.38 | 1.386 / 1.435 | 1,966,744 | Gap (short; qualified); metadata timing open |
| [WebP VP8L, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 4,320.01 | 3,018.37 | 1.431 / 1.494 | 1,216,276 | Gap (short; qualified) |
| [PNG RGBA8, None](../benchmarks/WARM-CURRENT-RESIDENT-PNG.md) | 1,575.30 | 1,388.57 | 1.134 / 1.330 | 4,480 | Gap (short; qualified); metadata timing open |
| [PNG RGBA8, resident / ImageBudget quota](../benchmarks/WARM-SHARED-BUDGET.md) | 2,004.67 | 1,392.79 | 1.439 / 1.482 | 4,561 | Gap (short; qualified) |
| [PNG RGBA8, None / live cancelable token](../benchmarks/WARM-PNG-CANCEL-BLOCKS.md) | 1,732.37 | 1,390.42 | 1.246 / 1.320 | 4,456 | Gap (short; qualified) |
| [PNG RGBA8, None / exact working limit](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 2,028.73 | 1,395.91 | 1.453 / 1.597 | 68,200 | Gap (short; qualified) |
| [PNG RGBA8, None / exact limit + live cancelable token](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 2,067.97 | 1,397.05 | 1.480 / 1.826 | 68,200 | Gap (short; qualified) |
| [PNG RGBA8, stored Deflate / None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 150.38 | 136.49 | 1.102 / 1.114 | 4,456 | Gap (independent) |
| [PNG RGBA16, stored Deflate / None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 322.73 | 296.18 | 1.090 / 1.096 | 8,552 | Gap (independent) |
| [PNG RGBA16, None](../benchmarks/WARM-CURRENT-RESIDENT-PNG.md) | 3,015.32 | 2,901.23 | 1.039 / 1.064 | 8,576 | Gap (short; qualified); metadata timing open |
| [PNG RGBA16, None / exact working limit](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 3,103.58 | 2,902.28 | 1.069 / 1.242 | 70,248 | Gap (short; qualified) |
| [PNG RGBA16, None / live cancelable token](../benchmarks/WARM-PNG-CANCEL-BLOCKS.md) | 3,068.88 | 2,897.86 | 1.059 / 1.108 | 8,552 | Gap (short; qualified) |
| [PNG RGBA16, None / exact limit + live cancelable token](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 3,120.43 | 2,904.38 | 1.074 / 1.145 | 70,248 | Gap (short; qualified) |
| [PNG RGBA8, Paeth](../benchmarks/WARM-PAETH-SIX-CHANNEL.md) | 3,149.31 | 2,847.74 | 1.106 / 1.128 | 4,456 | Gap (short; qualified) |
| [PNG RGBA8, Up](../benchmarks/WARM-PNG-UP-SIMD.md) | 1,484.65 | 1,509.16 | 0.984 / 0.989 | 4,456 | Parity confirmed |
| [PNG RGBA16, Up](../benchmarks/WARM-PNG-UP-SIMD.md) | 1,902.38 | 1,911.94 | 0.995 / 0.997 | 8,552 | Parity confirmed |
| [PNG RGBA8, Sub](../benchmarks/WARM-PNG-SUB-PREFIX.md) | 584.20 | 686.53 | 0.851 / 0.855 | 4,456 | Parity confirmed |
| [PNG RGBA16, Sub](../benchmarks/WARM-PNG-SUB16.md) | 357.99 | 743.02 | 0.482 / 0.493 | 8,552 | Parity confirmed |
| [PNG RGBA8, Average](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,380.32 | 1,850.31 | 1.286 / 1.298 | 4,456 | Gap (independent) |
| [PNG RGBA16, Average](../benchmarks/WARM-PNG-AVERAGE16.md) | 5,894.17 | 4,384.37 | 1.344 / 1.357 | 8,552 | Gap (independent) |
| [PNG RGBA16, Paeth](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 5,401.14 | 5,511.74 | 0.980 / 0.990 | 8,552 | Parity confirmed |
| [PNG RGB8, Paeth](../benchmarks/WARM-PAETH-SIX-CHANNEL.md) | 3,140.69 | 2,632.59 | 1.193 / 1.241 | 3,432 | Gap (short; qualified) |
| [PNG RGB16, Paeth](../benchmarks/WARM-PAETH-SIX-CHANNEL.md) | 4,290.59 | 4,472.22 | 0.959 / 0.969 | 6,504 | Parity confirmed |
| [PNG gray8, Paeth](../benchmarks/WARM-PNG-GRAY8-PAETH.md) | 1,038.34 | 1,029.60 | 1.008 / 1.014 | 1,384 | Gap (independent) |
| [PNG gray-alpha8, Paeth](../benchmarks/WARM-PNG-GRAY-ALPHA8-PAETH.md) | 2,051.11 | 2,016.66 | 1.017 / 1.068 | 2,408 | Gap (independent) |
| [PNG RGBA16, Adam7](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) | 5,393.13 | 3,917.00 | 1.377 / 1.381 | 23,600 | Gap (independent) |
| [PNG gray1 → RGBA8, Adam7](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) | 1,512.17 | 1,150.81 | 1.314 / 1.320 | 1,424 | Gap (independent) |
| [PNG palette8 + tRNS, Adam7](../benchmarks/WARM-PNG-PALETTE8-ADAM7.md) | 1,710.03 | 1,465.40 | 1.167 / 1.177 | 4,960 | Gap (independent) |
| [PNG RGB16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,297.09 | 2,227.02 | 1.031 / 1.034 | 6,504 | Gap (independent) |
| [PNG gray8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 357.47 | 363.27 | 0.984 / 0.993 | 1,384 | Parity confirmed |
| [PNG gray8 + tRNS key127, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 357.50 | 758.36 | 0.471 / 0.474 | 1,416 | Parity confirmed |
| [PNG gray16 + tRNS key32639, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,970.49 | 2,055.01 | 1.445 / 1.450 | 2,440 | Gap (independent) |
| [PNG gray-alpha8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 845.98 | 1,156.04 | 0.732 / 0.733 | 2,408 | Parity confirmed |
| [PNG RGB8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,293.31 | 1,523.89 | 0.849 / 0.853 | 3,432 | Parity confirmed |
| [PNG RGB8 + tRNS key(48,139,231), None](../benchmarks/WARM-PNG-RGB-TRANSPARENCY-CONFIRM.md) | 1,302.04 | 1,286.75 | 1.012 / 1.014 | 3,464 | Gap (independent) |
| [PNG RGB16 + tRNS key(12347,35806,59265), None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,316.17 | 3,182.24 | 0.728 / 0.732 | 6,536 | Parity confirmed |
| [PNG gray16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 714.81 | 957.95 | 0.746 / 0.751 | 2,408 | Parity confirmed |
| [PNG gray-alpha16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,563.09 | 1,795.59 | 0.871 / 0.874 | 4,456 | Parity confirmed |
| [PNG byte indices / sixteen-entry palette + tRNS, None](../benchmarks/WARM-PNG-SHORT-PALETTE.md) | 144.22 | 109.47 | 1.317 / 1.327 | 1,496 | Gap (independent) |
| [PNG palette8 + tRNS, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 349.92 | 364.06 | 0.961 / 0.970 | 2,456 | Parity confirmed |
| [PNG palette8 + two-entry tRNS, None](../benchmarks/WARM-PNG-PARTIAL-ALPHA.md) | 360.10 | 365.80 | 0.984 / 0.992 | 2,208 | Parity confirmed |
| [PNG palette4 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 109.81 | 140.53 | 0.781 / 0.799 | 984 | Parity confirmed |
| [PNG palette2 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 66.42 | 97.53 | 0.681 / 0.686 | 688 | Parity confirmed |
| [PNG palette1 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 54.06 | 88.92 | 0.608 / 0.611 | 552 | Parity confirmed |
| [PNG opaque gray1 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 59.11 | 303.42 | 0.195 / 0.199 | 488 | Parity confirmed |
| [PNG gray1 + tRNS key1 → RGBA8, exact timed FFmpeg color-key](../benchmarks/WARM-PNG-GRAY1-TRANSPARENCY.md) | 64.68 | 684.86 | 0.094 / 0.098 | 520 | Parity confirmed |
| [PNG gray2 + tRNS key0 → RGBA8, None](../benchmarks/WARM-PNG-PACKED-TRANSPARENCY.md) | 71.49 | 497.90 | 0.144 / 0.145 | 648 | Parity confirmed |
| [PNG gray1/key1 and gray2/key1, unfiltered native](../benchmarks/WARM-PNG-PACKED-TRANSPARENCY.md) | — | — | — | — | Unscored: native alpha differs |
| [PNG opaque gray2 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 71.73 | 106.38 | 0.674 / 0.679 | 616 | Parity confirmed |
| [PNG opaque gray4 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 116.09 | 169.54 | 0.685 / 0.698 | 872 | Parity confirmed |
| [JPEG baseline gray](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 927.74 | 681.93 | 1.360 / 1.455 | 672,210 | Gap (short; qualified); metadata timing open |
| [JPEG gray → caller GRAY8](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 817.85 | 646.73 | 1.265 / 1.308 | 273,436 | Gap (short; qualified); metadata timing open |
| [JPEG untagged → caller CMYK8](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 467.59 | 330.92 | 1.413 / 1.474 | 1,635,495 | Gap (short; qualified); metadata timing open |
| [JPEG 4:2:0 → caller RGB24](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,075.13 | 1,160.67 | 0.926 / 0.931 | 624,524 | Parity confirmed; metadata timing open |
| [JPEG Adobe CMYK, four full-resolution planes](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 469.48 | 377.61 | 1.243 / 1.306 | 1,085,694 | Gap (short; qualified); metadata timing open |
| [JPEG Adobe CMYK, one AC control](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 547.30 | 389.60 | 1.405 / 1.469 | 1,085,688 | Gap (short; qualified); metadata timing open |
| [JPEG Adobe CMYK, general transform control](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 3,281.94 | 3,111.92 | 1.055 / 1.105 | 1,085,546 | Gap (short; qualified); metadata timing open |
| [JPEG Adobe CMYK, two-dimensional sparse control](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 642.99 | 440.11 | 1.461 / 1.519 | 1,085,689 | Gap (short; qualified); metadata timing open |
| [JPEG YCCK420, full-resolution black](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 786.35 | 708.51 | 1.110 / 1.178 | 1,470,280 | Gap (short; qualified); metadata timing open |
| [JPEG YCCK420 → caller RGB24](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 865.17 | 604.01 | 1.432 / 1.476 | 1,337,511 | Gap (short; qualified); metadata timing open |
| [JPEG H4V1 / 4:1:1 → owned RGBA8](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 588.74 | 456.30 | 1.290 / 1.347 | 751,303 | Gap (short; qualified); metadata timing open |
| [JPEG H4V2 / 4:1:0 → owned RGBA8](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 591.35 | 477.47 | 1.238 / 1.282 | 720,844 | Gap (short; qualified); metadata timing open |
| [JPEG baseline 4:2:0](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,159.27 | 1,008.12 | 1.150 / 1.246 | 757,369 | Gap (short; qualified); metadata timing open |
| [JPEG baseline 4:2:0, row restart markers](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,321.56 | 1,013.22 | 1.304 / 1.462 | 761,744 | Gap (short; qualified); metadata timing open |
| [JPEG baseline 4:2:2](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,469.44 | 1,348.59 | 1.090 / 1.194 | 829,172 | Gap (short; qualified); metadata timing open |
| [JPEG baseline 4:4:4, owned input](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 2,391.79 | 1,899.20 | 1.259 / 1.304 | 948,060 | Gap (short; qualified); metadata timing open |
| [JPEG baseline 4:4:4, native input](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 2,222.98 | 1,720.03 | 1.292 / 1.361 | 966,341 | Gap (short; qualified); metadata timing open |
| [JPEG baseline direct RGB, full resolution](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 2,093.21 | 1,451.23 | 1.442 / 1.524 | 966,341 | Gap (short; qualified); metadata timing open |
| [JPEG direct RGB 4:2:2, timed centered correction](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,401.73 | 1,094.82 | 1.280 / 1.405 | 829,172 | Gap (short; qualified); metadata timing open |
| [JPEG direct RGB 4:2:0, timed component-plane interpretation](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,020.52 | 743.18 | 1.373 / 1.414 | 757,378 | Gap (short; qualified); metadata timing open |
| [JPEG baseline 4:4:4, native input / live cancelable token](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,392.14 | 1,720.11 | 1.391 / 1.482 | 966,317 | Gap (short; qualified) |
| [JPEG progressive gray](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,647.80 | 1,712.34 | 1.546 / 1.634 | 683,138 | Gap (short; qualified) |
| [JPEG progressive 4:2:0](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,438.11 | 1,947.56 | 1.252 / 1.446 | 775,336 | Gap (short; qualified) |
| [JPEG progressive 4:4:4 / live cancelable token](../benchmarks/WARM-JPEG-411-SIMD.md) | 5,069.78 | 3,349.49 | 1.514 / 1.611 | 984,298 | Gap (short; qualified) |
| [JPEG progressive 4:2:2](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 3,071.46 | 2,238.43 | 1.372 / 1.508 | 847,162 | Gap (short; qualified) |
| [JPEG progressive 4:4:4](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 5,037.72 | 3,361.01 | 1.499 / 1.715 | 984,298 | Gap (short; qualified) |
| [WebP VP8 + alpha](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md) | 8,353.50 | 5,968.68 | 1.400 / 1.436 | 1,966,648 | Gap (short; qualified); metadata timing open |
| [WebP VP8 opaque, simple luma filter](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md) | 6,888.59 | 4,890.64 | 1.409 / 1.450 | 760,572 | Gap (short; qualified); metadata timing open |
| [WebP VP8 opaque, live cancelable token](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md) | 6,687.81 | 4,793.83 | 1.395 / 1.437 | 760,572 | Gap (short; qualified); metadata timing open |
| [WebP VP8, opaque](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md) | 6,638.33 | 4,795.62 | 1.384 / 1.443 | 760,572 | Gap (short; qualified); metadata timing open |
| [WebP VP8L, sixteen-color indexed input](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 785.58 | 544.96 | 1.442 / 1.495 | 1,351,972 | Gap (short; qualified) |
| [WebP VP8L, literal/run input](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 2,269.12 | 2,157.09 | 1.052 / 1.074 | 1,074,326 | Gap (short; qualified) |
| [WebP VP8L, native input](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 4,576.04 | 3,022.99 | 1.514 / 1.615 | 1,216,180 | Gap (short; qualified) |
| [WebP VP8L, native input / live cancelable token](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 4,573.29 | 3,016.50 | 1.516 / 1.579 | 1,216,180 | Gap (short; qualified) |

**Encode**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [WebP EncodeWebpAsync, exact backend* / fresh nonseekable Stream](../benchmarks/WARM-VP8L-CODE-COST.md) | 58,259.63 | 120,897.37 | 0.482 / 0.487 | 10,286,704 | Parity confirmed |
| [JPEG EncodeJpegAsync, caller RGB420 / fresh nonseekable Stream](../benchmarks/WARM-JPEG-ASYNC-SAMPLES.md) | 2,076.73 | 1,628.02 | 1.276 / 1.430 | 232,614 | Gap (short; qualified) |
| [JPEG EncodeJpegAsync, RGBA8 / fresh nonseekable Stream / matched quality](../benchmarks/WARM-JPEG-ASYNC-ENCODE.md) | 3,982.03 | 3,849.79 | 1.034 / 1.158 | 494,254 | Gap (short; qualified) |
| [PNG EncodePngAsync, fresh nonseekable Stream / level6](../benchmarks/WARM-PNG-ASYNC-ENCODE.md) | 5,397.36 | 17,268.11 | 0.313 / 0.314 | 658,560 | Parity confirmed |
| [PNG RGBA8, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 5,406.30 | 17,266.72 | 0.313 / 0.315 | 527,627 | Parity confirmed |
| [PNG RGBA8, NoCompression / None](../benchmarks/WARM-PNG-STORED-CAPACITY.md) | 245.37 | 189.47 | 1.295 / 1.430 | 604,319 | Gap (short; qualified) |
| [PNG RGBA16, NoCompression / None](../benchmarks/WARM-PNG-STORED-CAPACITY.md) | 349.92 | 406.43 | 0.861 / 0.870 | 1,140,504 | Parity confirmed |
| [PNG RGBA8, Fastest managed2/native1 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 3,053.16 | 4,355.98 | 0.701 / 0.704 | 527,694 | Parity confirmed |
| [PNG RGBA16, Fastest managed2/native1 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 13,289.83 | 17,822.40 | 0.746 / 0.776 | 2,103,738 | Parity confirmed |
| [PNG RGBA16, Fastest managed2/native1 / live cancelable token](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 12,846.40 | 17,513.32 | 0.734 / 0.743 | 2,103,738 | Parity confirmed |
| [PNG RGBA16, Fastest managed2/native1 / forward-only output](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 12,790.89 | 17,465.07 | 0.732 / 0.740 | 2,103,770 | Parity confirmed |
| [PNG RGBA16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 14,604.45 | 20,095.18 | 0.727 / 0.731 | 2,103,624 | Parity confirmed |
| [PNG RGBA8, SmallestSize9 / native9](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 14,486.25 | 17,324.40 | 0.836 / 0.843 | 527,564 | Parity confirmed |
| [PNG RGBA16, SmallestSize managed9/native9 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 19,610.01 | 20,110.26 | 0.975 / 0.980 | 2,103,522 | Parity confirmed |
| [PNG RGB8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-PACKING.md) | 8,712.51 | 9,507.55 | 0.916 / 0.922 | 527,032 | Parity confirmed |
| [PNG RGB16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 13,163.23 | 15,313.53 | 0.860 / 0.863 | 2,102,732 | Parity confirmed |
| [PNG gray8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-PACKING.md) | 1,871.37 | 1,934.46 | 0.967 / 0.974 | 263,699 | Parity confirmed |
| [PNG gray16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 5,762.37 | 4,913.26 | 1.173 / 1.246 | 1,051,421 | Gap (short; qualified) |
| [PNG gray-alpha8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-PACKING.md) | 4,174.10 | 4,289.79 | 0.973 / 0.980 | 264,179 | Parity confirmed |
| [PNG gray-alpha16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 9,426.43 | 10,150.04 | 0.929 / 0.933 | 1,052,317 | Parity confirmed |
| [JPEG 4:4:4, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 3,894.69 | 3,771.76 | 1.033 / 1.115 | 559,418 | Gap (short; qualified) |
| [JPEG 4:4:4, forward-only output / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 4,379.70 | 3,778.08 | 1.159 / 1.353 | 526,029 | Gap (short; qualified) |
| [JPEG 4:4:4, live cancelable token / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 4,465.15 | 3,775.40 | 1.183 / 1.364 | 525,997 | Gap (short; qualified) |
| [JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 3,980.90 | 3,855.74 | 1.032 / 1.135 | 556,284 | Gap (short; qualified) |
| [JPEG 4:2:2, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,579.93 | 2,354.66 | 1.096 / 1.227 | 294,116 | Gap (short; qualified) |
| [JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,633.91 | 2,421.91 | 1.088 / 1.220 | 294,116 | Gap (short; qualified) |
| [JPEG 4:2:0, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,022.21 | 1,766.91 | 1.144 / 1.247 | 297,781 | Gap (short; qualified) |
| [JPEG 4:2:0, shared budget / matched quality](../benchmarks/WARM-SHARED-BUDGET.md) | 2,026.04 | 1,769.43 | 1.145 / 1.272 | 297,781 | Gap (short; qualified) |
| [JPEG 4:2:0, adaptive / row restart markers](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,019.53 | 1,611.25 | 1.253 / 1.455 | 297,781 | Gap (short; qualified) |
| [JPEG 4:2:0, forward-only output / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 2,310.38 | 1,769.17 | 1.306 / 1.488 | 263,819 | Gap (short; qualified) |
| [JPEG 4:2:0 from RGBA16, forward-only output](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 2,393.98 | 1,839.60 | 1.301 / 1.415 | 263,819 | Gap (short; qualified) |
| [JPEG 4:2:0 from RGBA16, live cancelable token](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 2,444.30 | 1,841.37 | 1.327 / 1.443 | 263,787 | Gap (short; qualified) |
| [JPEG 4:2:0 from RGBA16, adaptive Huffman](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,173.56 | 1,841.00 | 1.181 / 1.300 | 297,779 | Gap (short; qualified) |
| [JPEG grayscale, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 1,350.95 | 1,404.64 | 0.962 / 0.970 | 154,892 | Parity confirmed |
| [JPEG from caller gray samples, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-COMPACT-COLOR.md) | 1,251.71 | 1,079.34 | 1.160 / 1.394 | 155,026 | Gap (short; qualified) |
| [JPEG 4:2:0 from caller RGB / optional CompactColor](../benchmarks/WARM-JPEG-COMPACT-COLOR.md) | 1,774.21 | 1,576.12 | 1.126 / 1.362 | 162,110 | Gap (short; qualified) |
| [JPEG 4:2:0 from caller RGB samples](../benchmarks/WARM-JPEG-CALLER-MCU.md) | 2,097.34 | 1,579.06 | 1.328 / 1.408 | 297,894 | Gap (short; qualified) |
| [WebP VP8, opaque‡](../benchmarks/WARM-VP8-SEGMENT-SEARCH.md) | 59,760.91 | 15,526.15 | 3.849 / 3.977 | 1,006,048 | Cost screen (CPU parity deferred) |
| [WebP VP8, opaque from RGBA16‡](../benchmarks/WARM-VP8-SEGMENT-SEARCH.md) | 60,143.94 | 15,587.76 | 3.858 / 4.106 | 1,006,048 | Cost screen (CPU parity deferred) |
| [WebP VP8 + alpha‡](../benchmarks/WARM-VP8-SEGMENT-SEARCH.md) | 67,738.66 | 53,475.79 | 1.267 / 1.357 | 1,537,008 | Cost screen (CPU parity deferred) |
| [WebP VP8 + alpha from RGBA16‡](../benchmarks/WARM-VP8-SEGMENT-SEARCH.md) | 68,204.81 | 53,813.37 | 1.267 / 1.342 | 1,537,008 | Cost screen (CPU parity deferred) |
| [WebP VP8 + alpha‡, caller work budget32M](../benchmarks/WARM-WEBP-ALPHA-ANCHORS.md) | 15,008.23 | 53,686.75 | 0.280 / 0.326 | 2,012,156 | Cost screen (CPU parity deferred) |
| [WebP VP8L, exact backend*](../benchmarks/WARM-VP8L-CODE-COST.md) | 57,489.75 | 121,784.50 | 0.472 / 0.481 | 10,286,728 | Parity confirmed |
| [WebP VP8L, opaque RGBA8 / exact backend*](../benchmarks/WARM-VP8L-CODE-COST.md) | 49,621.60 | 69,789.18 | 0.711 / 0.720 | 9,669,728 | Parity confirmed |
| [WebP VP8L from RGBA16, exact backend*](../benchmarks/WARM-VP8L-CODE-COST.md) | 66,156.61 | 122,061.47 | 0.542 / 0.550 | 10,286,320 | Parity confirmed |
| [WebP VP8L photograph RGBA8](../benchmarks/WARM-VP8L-CODE-COST.md) | 65,462.97 | 82,409.48 | 0.794 / 0.802 | 9,912,952 | Parity confirmed |
| [WebP VP8L text/diagram RGBA8](../benchmarks/WARM-VP8L-CODE-COST.md) | 6,715.35 | 10,254.85 | 0.655 / 0.669 | 107,568 | Parity confirmed |
| [WebP VP8 photograph RGBA8 / matched quality](../benchmarks/WARM-VP8-INTERPOLATION-NOISE.md) | 34,396.38 | 9,714.89 | 3.541 / 3.695 | 590,152 | Cost screen (CPU parity deferred) |
| [WebP VP8 text/diagram RGBA8 / matched quality](../benchmarks/WARM-VP8-INTERPOLATION-NOISE.md) | 13,942.60 | 5,832.30 | 2.391 / 2.583 | 570,856 | Cost screen (CPU parity deferred) |

**Editing**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [Horizontal flip RGBA8, two live outputs / per image](../benchmarks/WARM-HORIZONTAL-FLIP512.md) | 9.56 | 9.08 | 1.053 / 1.142 | 128 | Gap (short; qualified) |
| [Nearest resize RGBA8](../benchmarks/WARM-COVERAGE-QUIET.md) | 58.67 | 252.66 | 0.232 / 0.241 | 132,294 | Parity confirmed |
| [Nearest upscale RGBA8](../benchmarks/WARM-NEAREST-UP8.md) | 935.05 | 1,006.00 | 0.929 / 0.955 | 2,133,091 | Parity confirmed |
| [Nearest upscale RGBA16](../benchmarks/WARM-NEAREST-UP16.md) | 1,071.13 | 1,019.06 | 1.051 / 1.095 | 4,265,431 | Gap (independent) |
| [Nearest resize RGBA16](../benchmarks/WARM-PREPARED-CONFIRM.md) | 105.69 | 170.58 | 0.620 / 0.629 | 264,466 | Parity confirmed |
| [Bilinear resize RGBA8](../benchmarks/WARM-BILINEAR-BYTE-ROW-CACHE.md) | 500.71 | 387.70 | 1.291 / 1.406 | 157,960 | Gap (short; qualified) |
| [Bilinear resize RGBA16†](../benchmarks/WARM-BILINEAR-WORD-CACHE.md) | 534.35 | 418.51 | 1.277 / 1.336 | 290,127 | Gap (short; qualified) |
| [Bilinear upscale RGBA8†](../benchmarks/WARM-BILINEAR-BYTE-FACTORIZATION.md) | 3,138.47 | 2,155.65 | 1.456 / 1.549 | 2,235,599 | Gap (short; qualified) |
| [Bilinear upscale RGBA16†](../benchmarks/WARM-CURRENT-BILINEAR.md) | 2,563.23 | 1,869.40 | 1.371 / 1.382 | 4,498,974 | Gap (independent) |
| [RGBA8 bicubic, single-pixel reduction](../benchmarks/WARM-BICUBIC-SINGLE-PIXEL.md) | 1,045.31 | 4,897.54 | 0.213 / 0.227 | 130 | Parity confirmed |
| [Bicubic resize RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 635.08 | 416.65 | 1.524 / 1.610 | 332,178 | Gap (short; qualified) |
| [Bicubic resize RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 729.05 | 571.10 | 1.277 / 1.715 | 464,343 | Gap (short; qualified) |
| [Bicubic upscale RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 3,919.91 | 3,036.45 | 1.291 / 1.357 | 3,244,646 | Gap (short; qualified) |
| [Bicubic upscale RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 4,258.22 | 2,871.12 | 1.483 / 1.690 | 5,016,578 | Gap (short; qualified) |
| [Bicubic mixed-axis RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-PAIRED.md) | 1,135.34 | 868.67 | 1.307 / 1.473 | 530,385 | Gap (short; qualified) |
| [Bicubic mixed-axis RGBA16§](../benchmarks/WARM-BICUBIC-PAIRED-COLUMNS.md) | 1,237.22 | 846.01 | 1.462 / 1.883 | 1,060,429 | Gap (short; qualified) |
| [Bicubic width reduction / height enlargement RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 994.41 | 840.45 | 1.183 / 1.259 | 731,753 | Gap (short; qualified) |
| [Bicubic width reduction / height enlargement RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 1,056.31 | 819.48 | 1.289 / 1.388 | 1,263,255 | Gap (short; qualified) |
| [Bicubic RGBA8, 128×64 / wider support§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 531.73 | 346.74 | 1.534 / 1.540 | 231,229 | Gap (independent) |
| [Bicubic RGBA16, 128×64 / wider support§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 504.51 | 416.18 | 1.212 / 1.218 | 263,991 | Gap (independent) |
| [Bicubic width reduction, unchanged height RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 734.08 | 574.67 | 1.277 / 1.365 | 465,374 | Gap (short; qualified) |
| [Bicubic width reduction, unchanged height RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 866.64 | 579.18 | 1.496 / 1.646 | 730,691 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 2,211.79 | 1,516.35 | 1.459 / 1.596 | 1,930,617 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA16§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 1,965.91 | 1,460.21 | 1.346 / 1.467 | 2,129,333 | Gap (short; qualified) |
| [Bicubic RGBA8, 64×32 / support beyond 32§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 418.52 | 369.03 | 1.134 / 1.215 | 205,884 | Gap (short; qualified) |
| [Bicubic RGBA16, 64×32 / support beyond 32§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 522.57 | 390.56 | 1.338 / 1.351 | 214,071 | Gap (independent) |
| [Bicubic RGBA8, 32×16 / support beyond 64§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 452.57 | 375.86 | 1.204 / 1.208 | 199,355 | Gap (independent) |
| [Bicubic RGBA16, 32×16 / support beyond 64§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 455.52 | 412.28 | 1.105 / 1.110 | 201,398 | Gap (independent) |
| [Bicubic RGBA8, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 363.20 | 452.30 | 0.803 / 0.809 | 201,728 | Parity confirmed |
| [Bicubic RGBA16, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 441.93 | 496.89 | 0.889 / 0.893 | 202,231 | Parity confirmed |
| [Bicubic RGBA8, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-BYTE-STRIPS.md) | 654.01 | 416.59 | 1.570 / 1.660 | 132,293 | Gap (short; qualified) |
| [Bicubic RGBA16, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 818.75 | 570.81 | 1.434 / 1.445 | 264,469 | Gap (independent) |
| [Bicubic RGBA8, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-FOUR-COLUMNS.md) | 498.19 | 347.76 | 1.433 / 1.644 | 32,913 | Gap (short; qualified) |
| [Bicubic RGBA16, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 606.16 | 414.59 | 1.462 / 1.479 | 65,658 | Gap (independent) |
| [Bicubic RGBA8, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-WEIGHT-BROADCAST.md) | 560.51 | 367.05 | 1.527 / 1.797 | 8,313 | Gap (short; qualified) |
| [Bicubic RGBA16, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-WORD-BROADCAST.md) | 527.50 | 383.98 | 1.374 / 2.313 | 16,505 | Gap (short; qualified) |
| [Bicubic RGBA16, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 570.51 | 412.45 | 1.383 / 1.394 | 4,217 | Gap (independent) |
| [Bicubic RGBA8, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-PACKED-REDUCTION.md) | 477.99 | 377.49 | 1.266 / 2.275 | 2,169 | Gap (short; qualified) |
| [Bicubic RGBA16, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 487.53 | 495.21 | 0.984 / 0.995 | 1,145 | Parity confirmed |
| [Bicubic RGBA8, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 489.92 | 451.81 | 1.084 / 1.090 | 633 | Gap (independent) |
| [Bicubic RGBA16 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 4,188.86 | 2,870.82 | 1.459 / 1.637 | 4,264,616 | Gap (short; qualified) |
| [Bicubic RGBA8 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 4,766.98 | 3,042.21 | 1.567 / 1.693 | 2,493,057 | Gap (short; qualified) |
| [Bicubic RGBA16, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-PAIRED-COLUMNS.md) | 1,197.19 | 853.79 | 1.402 / 1.555 | 1,060,439 | Gap (short; qualified) |
| [Bicubic RGBA8, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-BYTE-PAIRED.md) | 1,192.42 | 868.12 | 1.374 / 1.571 | 530,385 | Gap (short; qualified) |
| [Bicubic RGBA16, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,298.90 | 825.27 | 1.574 / 1.579 | 1,063,477 | Gap (independent) |
| [Bicubic RGBA8, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,249.21 | 831.82 | 1.502 / 1.513 | 531,910 | Gap (independent) |
| [Bicubic RGBA16, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 445.77 | 693.80 | 0.643 / 0.651 | 377 | Parity confirmed |
| [Bicubic RGBA8, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 508.41 | 664.36 | 0.765 / 0.772 | 249 | Parity confirmed |
| [Bicubic RGBA16, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 624.81 | 1,235.83 | 0.506 / 0.509 | 185 | Parity confirmed |
| [Bicubic RGBA8, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 786.38 | 1,138.73 | 0.691 / 0.695 | 153 | Parity confirmed |
| [Bicubic RGBA16, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,069.82 | 2,336.26 | 0.458 / 0.487 | 154 | Parity confirmed |
| [Bicubic RGBA16, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,474.00 | 1,286.00 | 1.146 / 1.180 | 266 | Gap (independent) |
| [Bicubic RGBA16, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 808.81 | 789.44 | 1.025 / 1.030 | 409 | Gap (independent) |
| [Bicubic RGBA16, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA16.md) | 516.33 | 530.91 | 0.973 / 0.981 | 1,209 | Parity confirmed |
| [Bicubic RGBA8, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA8.md) | 538.78 | 494.65 | 1.089 / 1.100 | 665 | Gap (independent) |
| [Bicubic RGBA8, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,002.56 | 675.80 | 1.484 / 1.488 | 266 | Gap (independent) |
| [Bicubic RGBA8, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,337.80 | 1,050.39 | 1.274 / 1.286 | 194 | Gap (independent) |
| [Bicubic RGBA8, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,173.66 | 1,633.74 | 0.718 / 0.745 | 138 | Parity confirmed |
| [Bicubic height reduction, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 624.02 | 583.77 | 1.069 / 1.273 | 648,997 | Gap (short; qualified) |
| [Bicubic height reduction, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-BYTE-PAIRED.md) | 725.00 | 589.64 | 1.230 / 1.378 | 265,003 | Gap (short; qualified) |
| [Bicubic height reduction, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 782.41 | 592.55 | 1.320 / 1.672 | 913,737 | Gap (short; qualified) |
| [Bicubic height reduction, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-PAIRED-COLUMNS.md) | 770.52 | 589.55 | 1.307 / 1.361 | 529,847 | Gap (short; qualified) |
| [Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 838.73 | 579.56 | 1.447 / 1.459 | 530,858 | Gap (independent) |
| [Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL8.md) | 811.19 | 573.66 | 1.414 / 1.425 | 265,502 | Gap (independent) |
| [Bicubic height enlargement, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 1,776.78 | 1,742.77 | 1.020 / 1.024 | 1,664,251 | Gap (independent) |
| [Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 2,614.86 | 1,744.07 | 1.499 / 1.722 | 1,280,402 | Gap (short; qualified) |
| [Bicubic height enlargement, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 1,998.15 | 1,670.35 | 1.196 / 1.482 | 2,516,599 | Gap (short; qualified) |
| [Bicubic height enlargement, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 2,286.62 | 1,672.40 | 1.367 / 1.438 | 2,133,103 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 1,920.59 | 1,453.02 | 1.322 / 1.373 | 2,129,333 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 2,125.43 | 1,509.12 | 1.408 / 1.529 | 1,178,425 | Gap (short; qualified) |
| [Crop RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 2.37 | 2.27 | 1.042 / 1.053 | 96 | Gap (independent) |
| [Crop RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 4.11 | 4.30 | 0.955 / 0.963 | 96 | Parity confirmed |
| [Clone RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.50 | 7.95 | 1.195 / 1.209 | 96 | Gap (independent) |
| [Clone RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 19.06 | 17.02 | 1.120 / 1.141 | 96 | Gap (independent) |
| [Normal orientation RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.29 | 7.63 | 1.217 / 1.254 | 128 | Gap (independent) |
| [Normal orientation RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 18.58 | 16.70 | 1.112 / 1.132 | 128 | Gap (independent) |
| [Vertical flip RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.28 | 8.37 | 1.108 / 1.124 | 128 | Gap (independent) |
| [Vertical flip RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 17.68 | 17.39 | 1.017 / 1.036 | 128 | Gap (independent) |
| [Horizontal flip RGBA8](../benchmarks/WARM-HORIZONTAL-FLIP512.md) | 9.08 | 9.55 | 0.950 / 0.965 | 128 | Parity confirmed |
| [Horizontal flip RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 21.83 | 27.03 | 0.807 / 0.811 | 128 | Parity confirmed |
| [Half turn RGBA8](../benchmarks/WARM-EDIT-CONFIDENCE.md) | 11.47 | 11.42 | 1.004 / 1.024 | 128 | Gap (independent) |
| [Half turn RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 18.59 | 21.86 | 0.850 / 0.874 | 128 | Parity confirmed |
| [Clockwise quarter turn RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 40.77 | 37.31 | 1.093 / 1.126 | 128 | Gap (independent) |
| [Clockwise quarter turn RGBA16](../benchmarks/WARM-EDIT-CONFIDENCE.md) | 43.45 | 44.08 | 0.986 / 1.010 | 128 | Confidence open |
| [Counterclockwise quarter turn RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 38.11 | 37.49 | 1.016 / 1.030 | 128 | Gap (independent) |
| [Counterclockwise quarter turn RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 49.42 | 45.49 | 1.086 / 1.103 | 128 | Gap (independent) |
| [Transpose RGBA8](../benchmarks/WARM-EDIT-CONFIDENCE.md) | 38.15 | 38.42 | 0.993 / 0.995 | 128 | Parity confirmed |
| [Transpose RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 48.17 | 44.86 | 1.074 / 1.114 | 128 | Gap (independent) |
| [Mirrored transpose RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 44.34 | 43.25 | 1.025 / 1.064 | 128 | Gap (independent) |
| [Mirrored transpose RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 39.10 | 38.54 | 1.015 / 1.041 | 128 | Gap (independent) |
| [Lanczos3 reduction RGBA8](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 758.75 | 490.07 | 1.548 / 1.646 | 138,466 | Gap (short; qualified) |
| [Lanczos3 reduction RGBA16](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 804.22 | 506.28 | 1.588 / 1.759 | 138,454 | Gap (short; qualified) |
| [Lanczos3 enlargement RGBA8](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 4,324.39 | 4,076.28 | 1.061 / 1.225 | 319,046 | Gap (short; qualified) |
| [Lanczos3 enlargement RGBA16](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 5,248.30 | 3,935.96 | 1.333 / 1.459 | 4,584,270 | Gap (short; qualified) |

* VP8L encode uses **libwebp exact=1, FFmpeg's encoder backend**, through its
public API because FFmpeg does not expose hidden-RGB preservation. All decoded
RGBA samples remain exact. Single-component grayscale JPEG uses the approved
benchmark-only **SIMD libjpeg-turbo3.1.4.1** reference; FFmpeg emits three components.
Production stays safe managed .NET 10 without dependencies or native bindings.

‡ Procedural VP8 benchmark inputs freeze managed/native quality 100/75 and effort 4, retaining
the 0.25 dB source-quality gate and exact alpha. Matched-quality content controls use
their linked frozen settings. [Current packet sizes](../benchmarks/WARM-VP8-DIRECTION-COVERAGE.md)
and per-codec reports accompany time/allocation evidence; size parity remains open.

¶ PNG Identify references use the public FFmpeg PNG parser/CRC plus original
complete structural/EXIF validation and owned descriptors, without inflation;
the Stream reference also times fresh AVIO/scratch/input/EOF and release.
‖ JPEG Identify uses FFmpeg MJPEG discard-all, original complete structural/EXIF
validation and a timed17-byte canonical SOF2 metadata projection, fresh packet/
descriptor ownership and reset, with zero pixel-buffer requests. WebP Identify
uses the public FFmpeg WebP parser plus original complete RIFF/EXIF validation,
including mandatory parser buffering/EOF flush and descriptor ownership. Its
0.0040x/upper0.0045x is specific to this metadata boundary and parser copy cost.
All six managed metadata fields remain exact.

† RGBA16 bilinear and explicit RGBA8 upscale allow native-only alpha max1/RMSE≤0.25;
Up8 times zero-alpha RGB clearing. Managed pixels remain exact; Down8 alpha is exact.
§ Bicubic uses **zimg, FFmpeg's zscale backend**, with all required clamped halo,
packing, premultiplied float filtering, unpremultiplication/quantization, zero-alpha
RGB clearing and independent owners timed. Native RGB/alpha max1 and both RMSE≤0.25;
only RGBA16 513×259→513×519 permits native RGB≤2 where reference alpha≤16,≤1
elsewhere, retaining alpha≤1/both RMSE≤0.25. Managed frozen pixels and independently
certified rational-half cases remain exact. Linked reports specify every halo
and scope control; no managed tolerance is introduced.

PNG byte RGB/gray/gray-alpha Optimal uses managed9/native6; RGBA Fastest uses2/1,
SmallestSize9/9, with exact samples and the historical5% IDAT timing-equivalence
gate. [Complete-packet reconciliation](../benchmarks/PNG-COMPLETE-PACKET-AUDIT.md)
finds15 retained passes and two open level6RGBA8 boundaries, both218,775/210,973 B.
A timing pass or IDAT tolerance is not a packet-size pass. Packed-gray
transparency uses the approved exact timed native color-key correction. Raw
incompatible baselines/refusals remain unscored; approved equivalent replacements
are confirmed. GIF/animation, startup, concurrency and arbitrary profiles are
outside this still-image fixed warm scope.

Maintain the selected evidence in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, then run `--check`.
Preserve raw evidence, failures, controls and package/source qualification.
Defer CPU-only optimization while compression gates are open; retain passing
performance evidence. Add a later case only when evidence identifies a distinct
required algorithm or public boundary omitted from the finite checklist.
