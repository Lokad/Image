# Material warm coverage

Updated 2026-10-18. The current **<=1x warm parity objective is open**: 58 named
numeric cases already have independent upper<=1, 145 means exceed1, and four
means<=1 have upper>1. Reuse passing paths and prioritize the largest current
gaps; do not add a new input or settings grid. The [current summary](../docs/benchmark.md)
distinguishes parity, measured gaps and confidence-open cases.

The finite material checklist's **previous <=2x phase was completed for 0.1.0**.
Every named required path then had independent quiet evidence with conservative
upper ≤2x or an explicit unchanged-path reuse proof. Subsequent qualified changes
record which comparisons they replace. Only confirmed <=1x paths are stopped
for the current objective.

The [three-table summary](../docs/benchmark.md) contains207 numeric rows plus one
unscored native-alpha incompatibility. Of those numeric rows,171 select independent
confirmations. Thirty-six select qualified screens after changed production: six
paired-column byte/word bicubic cases, byte bicubic TightDown8, word bilinear
Down16, live-token progressive444, five fixed JPEG encodes, two opaque VP8 encodes,
two PNG Paeth dispatch controls and three PNG encodes after chunk checksum folding, and eight adaptive JPEG encodes
(420 byte/word/restart33,422 byte/word,444 byte/word and gray8) after nonzero planning,
plus five VP8L decodes and two compressed-alpha VP8 decodes after borrowed lookup.
RGB16 Paeth now
has an independent passing proof. Both affected alpha VP8 encodes have independent
passing confirmations after their shared source conversion changed.
The thirty-six short screens remain gaps; older independent timings are explicitly
superseded. The two
former short-regression witnesses are now historical after changed AC emission.
The unscored raw PNG transparency reference is replaced by approved exact timed
color-key comparisons; it is not an unresolved managed gap.

Scope is the existing two original513x259 RGBA8/16 rasters, documented projections
and the named fixed selectors in the linked evidence/renderer. Required still
PNG/JPEG/WebP codecs,8/16-bit edits, orientation/crop/nearest/bilinear/bicubic,
resident/fresh-Stream and existing cancellation/limits/owner boundaries are
represented. Cases were chosen for distinct material algorithms or public
boundaries, not a Cartesian grid of settings. The61 bicubic selectors were already
established before this audit and were reconciled, not expanded.

| Item | Named fixed scope and reason | Current evidence / state |
| --- | --- | --- |
| B01 | Bilinear Down8/16,Up8/16: shared resize dispatch and byte/word alpha-aware filtering | [Previous phase closed](WARM-CURRENT-BILINEAR.md); [Down16 improved](WARM-BILINEAR-WORD-CACHE.md), still a <=1x gap |
| B02 | JPEG adaptive Encode8/16,Encode422_8/16: changed AC planner at444/422 and both input depths | [All four closed](WARM-CURRENT-JPEG-ADAPTIVE.md) |
| B03 | JPEG Decode8,NativeDecode8,NativeDecode420/422,OwnedDecodeGray,NativeDecode420Restart,DirectRgb422/420: changed ordinary AC loop plus distinct component renderers | [All eight closed](WARM-CURRENT-JPEG-DECODE.md) |
| B04 | JPEG ProgressiveGray/420/422 plus ordinary/live-token444: masked refinement and its guarded fused readers | [Four ordinary proofs reused](WARM-JPEG-REFINEMENT-FUSED.md); [live-token444 improved](WARM-JPEG-CANCELABLE-REFINEMENT.md), still a <=1x gap |
| B05 | The25 named PNG decode selectors in the linked report: byte/word predictors, expansion and input-pool changes | [All25 closed](WARM-CURRENT-PNG-DECODE.md) |
| B06 | PNG DecodeAdam7_16,DecodeGray1Adam7: interlaced placement on changed shared framing; later palette4/8 cases reuse their own proofs | [Closed](WARM-CURRENT-PNG-ADAM7.md) |
| B07 | Png8/16,PngCropFit8/16: complete fresh input, orientation/crop/fit and intermediate ownership | [Closed](WARM-CURRENT-PNG-STREAM.md), with word fusion proof reused |
| B08 | PNG Encode8/16: changed RGBA packing, exact samples and fixed IDAT-size policy | [Both closed](WARM-CURRENT-PNG-ENCODE.md) |
| B09 | VP8 Encode8/16 with alpha: changed output writer and word normalization | [Both closed](WARM-CURRENT-VP8-ENCODE.md) |
| B10 | VP8L Lossless8 literal/run input: changed entropy/run path | [Closed](WARM-CURRENT-VP8L-LITERAL.md) |
| B11 | Clone,crop,normal orientation,horizontal/vertical flip,half turn,CW/CCW,transpose,transverse at both depths: shared output-pool acquisition | [All20 closed](WARM-CURRENT-EXACT-EDITS.md); retained-two flip unchanged |
| B12 | The61 named bicubic selectors: changed dispatch/helper/guard paths, default/tight scratch, byte/word and established support widths | [Previous phase closed](WARM-CURRENT-BICUBIC.md); [word](WARM-BICUBIC-PAIRED-COLUMNS.md) and [byte](WARM-BICUBIC-BYTE-PAIRED.md) paired columns plus [byte strips](WARM-BICUBIC-BYTE-STRIPS.md) improved, still <=1x gaps |
| B13 | Existing PNG stored/Paeth/predictor,RGB/gray/gray-alpha8/16,packed gray1/2/4,palette1/2/4/8,tRNS,Adam7 and frozen compression-mode rows | [Up8/16](WARM-PNG-UP-SIMD.md) and [RGB16 Paeth](WARM-PAETH-SIX-CHANNEL.md) parity confirmed; other applicable proofs reused; precise selectors/allocations in the [summary](../docs/benchmark.md) |
| B14 | JPEG4208/16,restart,gray and geometric live-token/forward output; VP8 ordinary/simple/alpha/live-token/Stream and opaque8/16 encode; VP8L native/indexed/live-token/Stream and exact backend encodes | [VP8 source views](WARM-VP8-SOURCE-VIEWS.md) improve four encodes; [fixed JPEG encodes](WARM-JPEG-FIXED-AC-FUSION.md) and other unchanged proofs reused |
| B15 | Nearest Down8/16,Up8/16: unchanged early dispatch/integer kernels and allocation path | Existing independent proofs reused |
| C01 | PNG resident Identify: full container/CRC/EXIF without inflation, fresh descriptor; decode/cold timings do not cover it | [Closed](WARM-PNG-IDENTIFY.md),1.234x/upper1.238x,88B/op |
| C02 | JPEG resident Identify: complete marker/scan/stuffing/EXIF/EOI scan without entropy/pixels | [Parity confirmed](WARM-JPEG-ENTROPY.md),0.499x/upper0.506x,88B/op |
| C03 | WebP resident Identify: distinct RIFF/feature/header/EXIF algorithm, one existing VP8L packet | [Closed](WARM-WEBP-IDENTIFY.md),0.0040x/upper0.0045x,88B/op,explicit FFmpeg parser reference |
| C04 | Fresh non-exposed seekable Stream Identify on the same C01 PNG: fast metadata boundary exposes input acquisition/read/EOF/ownership costs | [Closed](WARM-PNG-STREAM-IDENTIFY.md),1.445x/upper1.459x,184B/op |

Demonstrated audit gaps are resolved: stored16 PNG2.219x→1.090x, wide-cache
bicubic2.121x→passing current-path evidence, PNG Identify2.797x→1.234x,
JPEG Identify33.725x→1.840x, and Stream Identify7.863x→1.445x. Each production
round retains correctness/control evidence, short mean before confirmation,
all actuals/quiet guards and full accepted-change qualification. Raw refusals,
failed guards and superseded experiments remain in their original reports.

The latest [VP8L lookup reconciliation](warm-vp8l-borrowed-green-evidence.json)
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
The scope excludes deferred GIF/animation/drawing/fonts, arbitrary profiles/
sizes/hardware, startup and concurrent throughput. Add a later case only when
concrete evidence identifies a distinct omitted required algorithm or public
boundary, with a named representative and reason. Do not expand settings grids
or retune a confirmed case without affected inputs or a demonstrated regression.
