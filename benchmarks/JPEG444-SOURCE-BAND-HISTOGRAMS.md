# JPEG444 source-band histograms

C178 counts the source trainer's AC symbols on the same spectral bands used by
the progressive packet and coefficient decisions. Complete-packet parity remains open.

| Existing 513 × 259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 135,342 | 135,237 / 133,041 | 0.219087 dB |
| Original 4:4:4, RGBA16 | 135,265 | 135,144 / 133,043 | 0.227127 dB |

Savings are 105/121 B, or 541 B across the five affected ledger rows—about 4.7%
of their previous byte excess. Both pass the unchanged 0.25 dB matched source-quality
gate, exact alpha and decoder bounds (maximum 4, RGB RMSE 1.5). Source, sampling,
managed quality 69/native qscale 5, Original DQT, DC, precision factors, lambda 100,
MCU error allowance and 0x204060 flattening stay fixed. Equal quality numbers do
not establish equivalence. These controls make no photographic, diagram or general
JPEG compression claim; optional CompactColor and passing controls stay separate.

The same fine-geometric training coefficients, 64 sample positions and source
decisions are retained. Only histogram run/EOB counting resets at the existing
equal-point-transform band ends. Fine coefficient units are retained, without
coarse-grid retraining. The two histograms keep the same 162-symbol +1 smoothing
and integer Q8 finishing rule; their derived prices change. The final C176
local/DAG/pair/triplet pipeline and its global MCU error/rate criteria are unchanged.
There is no final-output price refresh, settings grid or quality feedback.

Preparing two masks charges 1,920 work units once, before clearing histograms:
128 × 13 factor steps, 128 setup and 128 mask visits. One reusable 128-byte stack
span fits the same 8 KiB workspace; the same two histograms and 64 KiB adaptive
allowance remain. The final 905,445-unit MCU decision bound is unchanged. The
declared control plans 1,999,576,760 units under the unchanged default 2B limit.
There is no new heap owner, frame/input copy or second complete refinement pass.
Shared admission, terminal refusal, cancellation, async users and output ownership remain.

Both OS pass **5,037 Release tests** (2,118 internal, 1,983 public, 936 live FFmpeg),
plus 207 internal and 62 public scalar checks each. Six new checks cover source
counts, fine amplitudes, empty bands, mask factors, exact setup work, refusal,
cancellation and the disabled path. Public goldens cover default2B admission,
fixed/adaptive/caller/baseline/progressive/async paths and lifetimes. Actual packets
match the independent C177 prototypes byte for byte; all 411,840 source-nearest
and chosen coefficients per depth are independently verified. Each decoder's
normal/scalar/native pixels equal its own baseline-equivalent packet.

One core/internal/public build per OS stays frozen. The same actual private 0.2.0
package and all three usage examples pass on Windows and Linux. Benchmark, native,
driver, differential and parser binaries are reused. Unchanged paths retain evidence.

One quiet-VM three-actual complete forward444 screen records
**1,420,508.479 µs / reference 3,765.600 µs**, ratio 377.233/upper 388.341,
**611,264 B/op**. Both versioned idle guards pass; native allocations are unmeasured.
Prior C176 records 1,386,194.881 µs and 611,264 B/op. Cost is recorded and accepted
for the byte savings, without a CPU-parity pass or independent timing confirmation.
Speed-only optimization remains deferred.

Five 444 packet rows and their forward cost replace C176. Other invocations using
changed Original444 encoding retain historical timing until measured. Relevant
420/422/grayscale/CompactColor, native, decoder, PNG and WebP paths retain evidence.
The [packet ledger](JPEG-COMPLETE-PACKET-AUDIT.md) has 13 default passes/5 open and
one separate optional pass. Maximum remaining excess is about 1.65%; the finite
256-case warm objective and replacement readiness remain open.

[Numeric evidence](jpeg444-source-band-histogram-evidence.json) identifies frozen
builds, the actual package, original receipt and mirrored qualification raw. The
[benchmark summary](../docs/benchmark.md) remains limited to three tables.
