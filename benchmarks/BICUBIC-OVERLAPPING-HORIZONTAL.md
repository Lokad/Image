# Prepared bicubic horizontal windows

RHW611 prepares contiguous neighboring eight-tap outputs once in existing
geometry storage. Five wide windows serve their ten distinct source pixels,
instead of sixteen separate vector loads per pair. Each output retains its tap
order, separate multiply/add, certification and exact repair. Clamped edges,
unequal taps, odd tails, identity axes, wide support and portable paths remain.
No public API, owned capacity, pool, session or cache is added.

Independent complete-operation confirmation saves **63.74µs / 11.64% raw /
11.04% reference-normalized** on byte reduction: **547.74 → 484.00µs**. Native
moves −0.68%. Filtered/all-actual raw and normalized confidence separates.
There is an explicit **vertical RGBA16 regression: 637.97 → 668.01µs**, losing
**30.04µs / 4.71% raw / 4.34% normalized**, also separated; native moves +0.35%.
This stage is retained for the larger reduction gain and current passing
horizontal/cross-axis controls. It is not a uniform speedup. Prepared versus
identity/ineligible entry and row scheduling remain an architectural follow-up;
their individual costs have not been isolated.

All controls use the existing 513×259 inputs and settings. Managed one-shot
geometry, workspace, filtering, independent output and disposal stay timed.
The labeled FFmpeg zscale zimg backend retains graph, input planes and scratch;
packing, halo, fresh output and disposal remain timed. Its float pixel bounds
remain distinct from exact managed rational/certified results.

| Existing control | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Byte reduction | 484.00 | 416.63 | 1.162 / 1.167 | 199,980 | Gap |
| Word reduction | 494.32 | 571.47 | 0.865 / 0.868 | 199,953 | Pass |
| Byte horizontal reduction | 476.42 | 565.27 | 0.843 / 0.847 | 199,980 | Pass |
| Byte vertical reduction | 637.50 | 589.78 | 1.081 / 1.088 | 384,090 | Gap |
| Byte width down / height up | 810.85 | 838.70 | 0.967 / 0.973 | 199,943 | Pass |
| Word horizontal reduction | 584.22 | 579.68 | 1.008 / 1.018 | 199,930 | Gap |
| Word vertical reduction | 668.01 | 589.94 | 1.132 / 1.139 | 384,013 | Gap |
| Word width down / height up | 788.95 | 830.32 | 0.950 / 0.960 | 199,891 | Pass |
| Byte 32×16 reduction | 365.35 | 374.87 | 0.975 / 1.034 | 197,300 | Uncertain |

The wider control's older passing cost is superseded by current uncertainty;
no favorable repeat was taken. Other historical points do not establish causal
gains. Passing cases stop. Nine costs refresh, four displayed comparisons update;
eleven unchanged word controls and other families reuse their applicable evidence.

Byte reduction still admits 199,840B workspace and retains zero scratch after
the operation. Its independent result holds 132,096B until disposal (264,192B
for word reduction). Total GC, admitted capacity, output/pool retention and
physical peak are separate; physical/native peaks are unmeasured. One-byte GC
rounding changes are not storage gains. Marker storage is unused geometry padding;
the evidence proves other valid small-support geometries leave that slot zero.

Both OSes pass **5,932 full/live checks** (2,667 internal, 2,317 public, 948
differential), 182/703 focused internal/public checks per normal/scalar mode and
the same affected checks with AVX2/AVX512 disabled. Linux normal mode verifies
44,128 bitwise horizontal intermediates; Windows uses portable kernels. Existing
final-pixel, frozen admission/refusal/cancellation/recovery checks pass.
The **same actual 0.2.1 package**, `f5982a95`, passes both OS consumers and all
three examples, including exact byte/word samples and independently retained
results. Published packages are untouched; publication is postponed.

One main/embedded build per OS is reused throughout. A completed wrong-case
fixed-selector attempt is retained unscored; the identity gate caught it and the
existing general selector was used for other cases. A new consumer's refusal
setup initially assumed SIMD scratch on portable Windows; caller-held admission
fixes the test without repacking or changing the DLL. Both failures are retained.
The only source-manifest change after timing is that consumer; reconstruction
recovers the exact measurement identity. No production repair or favorable timing
repeat was needed.

[Evidence](bicubic-overlapping-horizontal-evidence.json) includes source proof,
all-actual/normalized comparisons, adverse results, memory boundaries and package
qualification. Mirrored/member-verified raw `bicubic-overlapping-horizontal-qualified.tar.xz`,
SHA256 `8363947c550c0578bb2ffa324dce53833e20895724f1fbe59cfb013497184da9`,
6,312,172B/202 members, preserves original/candidate modules, source, model,
receipts, failures and reproduction scripts. [Attribution](BICUBIC-BYTE-CURRENT-ATTRIBUTION.md)
retains the preceding diagnostic trace and its failed I/O guard separately.
