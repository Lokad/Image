# Prepared VP8 quantizer operands

A private readonly value snapshots five fixed vector operands once per eligible
ordered Luma4 search. The same exact Q17 core serves real one-shot quantization;
the obsolete static preparation entry is removed, with all callers/tests updated.
Matrix span interpretation leaves the candidate loop. Integer wrap, sign/threshold,
2047 clamp, saturation/zigzag, publication, mode/rate/trellis order and per-mode
cancellation stay exact. Scalar/AVX2 algorithms remain unchanged; no new cache,
pool, session, heap buffer or admission abstraction.

Independent complete Q75/effort6 diagram confirmation saves **153.24µs /
1.23%**, 12,412.84→12,259.60µs;
ratio0.987659, interval[0.983884,0.991442],
754,344B/op unchanged. This is the complete stage effect, including its one-shot
kernel. The trellis branch does not use the new prepared Luma4 search, so the
gain is not attributed solely to that preparation.

Photograph Q75/effort4 confirmation is **inconclusive**: ratio0.994851,
interval[0.985971,1.003765],809,072B/op
unchanged. It establishes no gain, loss or equivalence. Each confirmation retains
12 alternating pairs ×16 complete encodes after a separate three-pair screen.
Identical resident-source setup/JIT/checks are excluded; source processing,
preparation/refresh, public EncodeWebp/admission, fresh output growth, independent
ownership and disposal stay timed on quiet pinned CPU0. Exact packets remain
6,818/17,688B. No unchanged-candidate favorable repeat follows.

| Current frozen complete513×259 encode | Managed µs | FFmpeg/libwebp µs | Ratio / upper | B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque RGBA8, effort4 | 15,403.27 | 15,445.79 | 0.997 / 1.008 | 1,201,552 | 72,380 |
| Opaque RGBA16, normalization | 15,557.20 | 15,599.48 | 0.997 / 1.002 | 1,201,552 | 72,380 |
| Opaque photograph, effort4 | 11,415.45 | 9,641.52 | 1.184 / 1.189 | 809,225 | 17,688 |
| Opaque text/diagram, effort4 | 6,595.82 | 5,815.18 | 1.134 / 1.138 | 741,137 | 7,100 |
| Diagram, effort2/distortion | 2,963.83 | 2,514.66 | 1.179 / 1.182 | 741,386 | 7,330 |
| Diagram, effort6/trellis | 11,374.90 | 9,810.83 | 1.159 / 1.170 | 754,497 | 6,818 |
| RGBA8 + lossless alpha, effort4 | 54,501.24 | 53,396.89 | 1.021 / 1.029 | 7,034,392 | 98,590 |
| RGBA16 + alpha, normalization | 55,243.18 | 53,439.25 | 1.034 / 1.045 | 7,033,984 | 98,590 |

Current status: 6 gaps, 2 uncertain. Every original input/settings, complete packet/pixel/
alpha/quality, source/output ownership and quiet gate passes. Each row retains12
actuals, filtered/all-actual confidence and both native brackets. The native
reference retains documented wrappers/private scratch; normalization/import, all
pixel work and independent output ownership stay timed. Standalone trellis
normalized intervals overlap the preceding owner; no independently separated
native-normalized incremental gain is claimed. Other rows are current observations,
not proved incremental effects. Other117 displayed paths reuse unchanged evidence.

The original quantizer region checks matrix bounds80/32/48/64/16 per candidate;
the existing count observes88,359 fused photo calls. That is441,795 logical checks,
not a phase time, bandwidth measurement or predicted saving. The matrix is one
80-int span with a16B view. Five snapshots occupy320 logical stack bytes inside
the existing16KiB admitted workspace in both caller families, reserved before
entry. No new owned heap capacity, retained state or root reservation. Individual
compiled Luma4 stack-pointer subtraction grows1448→1832B; one-shot remains152B.
Code grows8261→8437B and395→426B. These facts do not establish complete physical/
admitted peaks; those and native allocation are not newly measured. Report actual
standalone GC/batch allocations separately from unchanged paired readings.

One immutable main/embedded build perOS passes5,716 full/live checks
(2,479 internal/2,289 public/948 differential),86 internal/137 public checks in
normal, scalar and AVX512-disabled modes. Independent scalar/native quantization,
wrapped products/thresholds/saturation/DC/tails and complete-packet oracles stay;
reuse/lifetime tests now mutate the source matrix before later quantizations.
Cancellation/refusal/shared/async/independent-survivor behavior passes. The SAME
actual private0.2.1 Release package/all3 examples passes both OSes. Published
packages remain immutable; no full actual-package125-row refresh or publication.

The final word-alpha comparison initially fails the unchanged idle guard on3%
CPU0 I/O wait before any log/sample exists. Seven completed comparisons remain;
only the unstarted case resumes after fresh passing guards, without rebuild or
completed-job restart. The failed guard is retained. The
[compact scorer](VP8-COMPACT-VECTOR-SCORING-INCONCLUSIVE.md) remains history with
exact restoration, not a current-row dependency. Full282-path parity remains open.

[Evidence](vp8-quantizer-operand-preparation-evidence.json) pins every source/module,
pair/actual/bracket/guard and package proof. Raw `vp8-quantizer-operand-preparation-qualified.tar.xz`:
6,047,840B/286members, SHA256 `9a10ccc8b687357e7adbe3535ec4aa5ec3055e6394cb6e56f5d49d4339b73d4a`,
mirrored and member-verified, with qualified-original/preceding-history dependencies.
