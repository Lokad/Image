# JPEG444 coded source histograms

C193 counts the existing training choices in the units actually written by
progressive JPEG. The five declared packet-parity gates remain open.

| Existing 513 × 259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 134,741 | 134,676 / 133,041 | 0.218264 dB |
| Original 4:4:4, RGBA16 | 134,687 | 134,688 / 133,043 | 0.226798 dB |

The 8-bit packet saves 65 B; the 16-bit packet adds **one byte**. Across the five
existing boundaries this saves 259 B, removing 3.07% of their previous excess.
Maximum remaining excess is about 1.24%. Both controls pass the unchanged
0.25 dB source-quality gate, exact alpha and decoder bounds (maximum 4, RGB RMSE
1.5). Inputs, reference packets, managed quality 69/native qscale 5, sampling and
0x204060 flattening stay frozen. Equal quality numbers do not establish equivalence.
These controls establish no photographic, diagram or general JPEG claim;
optional CompactColor and passing controls retain separate evidence.

Every fine-geometric training choice remains unchanged. A signed arithmetic
right shift projects its amplitude only while counting the source histogram;
positive values may become zero. Samples, bands, smoothing, canonical fitting,
EOB amortization, final decisions/error allowance and DQT policy stay fixed.
Existing source models replay exactly before this one unit change. This differs
from the rejected coarse-grid retraining experiment. C191 table sharing remains.

One 128-byte stack lookup is prepared once per operation, charging 1,920 work
units before mutation. Source substantial scratch totals 7,336 B inside the same
8 KiB fixed admission; adaptive workspace remains 64 KiB. The declared complete
plan is 1,999,982,933 below the unchanged 2B default. Exact async admission edges,
setup refusal/cancellation and untouched training inputs are tested. No frame,
input copy, cache or escaped owner is added.

Both platforms pass **5,067 Release tests** (2,148 internal, 1,983 public,
936 live FFmpeg), plus **237 internal + 62 public scalar checks** each. One core,
internal and public build supplies the same frozen binaries on both platforms;
all pass first time. The same actual private 0.2.0 package and all three usage
examples pass on both platforms. Actual packets exactly match C192, retaining
411,840 source-nearest/chosen coefficient checks per depth and twelve
normal/scalar/native baseline-equivalent pixel and ownership checks. Shared
admission, terminal refusal, cleanup and surviving outputs retain their checks.

One complete forward444 quiet-VM screen records **1,929,269.703 µs / reference
3,768.092 µs**, ratio 512.002 / upper 585.587, **644,112 B/op**. Prior C191 records
1,949,513.811 µs and 643,160 B/op. Both idle guards pass; native allocations remain
unmeasured. Cost is accepted for compression, without a speed claim, CPU-parity
pass or independent timing confirmation.

All decision-enabled Original444 source-price paths at qualities 1–98 change
coefficients, lossy pixels, packets and encode costs, including baseline, fixed,
restart, caller-buffer and async paths. The five current packets and forward cost
supersede C191; other affected timings remain historical. Other policies/sampling,
qualities 99/100 and unrelated paths retain evidence. Decoder fixture packets
stay frozen. The [packet ledger](JPEG-COMPLETE-PACKET-AUDIT.md) remains 13 default
passes / five open plus one separate optional pass. Full warm CPU parity and
replacement readiness remain open. [Numeric evidence](jpeg444-coded-source-histogram-evidence.json)
identifies the source, actual package, original cost receipt and mirrored raw.
