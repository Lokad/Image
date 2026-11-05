# PNG complete-token output

An operation-owned 64 bit cursor packs whole match fields and literal triplets,
then emits 32 bit words into the existing buffer. This removes four independent
append/drain sequences per match and most byte-at-a-time output. Rare overflows
and buffer edges use bounded byte drains. Exact 4,096 byte publication, normal-state
bit alignment, failed-write prefixes, cancellation and ownership are preserved.
There is one production writer, no new buffer, cache, session or storage owner.

The independent Fastest8 comparison saves **407.66µs / 7.36% raw**
(5,538.29→5,130.63µs). Filtered/all-actual raw confidence
separates. The reference-normalized point improves 7.75%, but conservative
normalized confidence narrowly overlaps (before lower 1.210605; after upper 1.211213).
The faster native bracket moves +0.42%. This is an independently
supported raw gain, **not** independently separated normalized gain or FFmpeg parity.
No extra repeat follows solely to separate those intervals.

Smallest16 improves 9.23% raw, but its native clock is
-7.89% and normalized confidence overlaps. No causal or independently
normalized word gain is claimed. The two short screens, both original/current
confirmations, every actual and every native bracket remain in the evidence.
The old [global word-buffer trial](PNG-WORD-BIT-BUFFER-REJECTED.md) supplied a
lead previously rejected by a fixed 5% rule; this revised local/whole-token design
is measured independently, rather than automatically promoted.

## Current complete operations

All are frozen 513×259 inputs, quiet-VM CPU0, normal SIMD, warm single-thread
Release. Both sides acquire/dispose fresh codec/zlib/packet state and independently
owned output. Native word/color normalization and workspace stay timed. Delivery
rows preserve the declared live token, forward-only or complete awaited async
nonseekable boundary. Grayscale16 retains the explicitly labeled red-plane
reference. Twelve actuals per comparison; confidence includes unfiltered results.

| Existing control | Lokad µs | FFmpeg µs | Ratio / upper | Lokad B/op | Exact packet B, both | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| RGBA8, one-shot SmallestSize9 / native9 | 18,963.03 | 18,490.36 | 1.026 / 1.038 | 820,452 | 210,973 | gap |
| RGBA16, one-shot SmallestSize9 / native9 | 21,515.51 | 20,853.86 | 1.032 / 1.048 | 2,437,660 | 963,202 | uncertain |
| RGBA8, one-shot Fastest1 / native1 | 5,130.63 | 4,373.97 | 1.173 / 1.211 | 820,916 | 236,828 | gap |
| RGBA16, one-shot Fastest1 / native1 | 21,083.34 | 20,370.66 | 1.035 / 1.045 | 2,437,660 | 968,216 | gap |
| RGBA16, one-shot Fastest1, live cancelable token | 21,501.93 | 20,279.36 | 1.060 / 1.075 | 2,437,660 | 968,216 | gap |
| RGBA16, one-shot Fastest1, forward-only output | 21,117.46 | 20,341.13 | 1.038 / 1.046 | 2,437,692 | 968,216 | gap |
| grayscale8, one-shot Optimal6 / native6 | 2,305.00 | 2,176.07 | 1.059 / 1.093 | 551,364 | 72,586 | gap |
| RGB8, one-shot Optimal6 / native6 | 10,502.27 | 10,731.21 | 0.979 / 0.989 | 819,884 | 168,729 | pass |
| gray-alpha8, one-shot Optimal6 / native6 | 4,857.38 | 4,921.85 | 0.987 / 1.003 | 552,332 | 120,366 | uncertain |
| RGBA8, one-shot Optimal6 / native6 | 18,683.62 | 18,255.02 | 1.023 / 1.071 | 820,452 | 210,973 | uncertain |
| RGBA16, one-shot Optimal6 / native6 | 23,271.35 | 22,611.27 | 1.029 / 1.035 | 2,437,660 | 963,202 | gap |
| RGB16, one-shot Optimal6 / native6 | 17,609.30 | 17,695.33 | 0.995 / 1.002 | 2,431,492 | 739,472 | uncertain |
| grayscale16, one-shot Optimal6 / native6, red-plane reference | 5,387.62 | 5,891.43 | 0.914 / 0.935 | 822,740 | 265,433 | pass |
| gray-alpha16, one-shot Optimal6 / native6 | 11,288.46 | 11,366.92 | 0.993 / 1.006 | 1,360,900 | 506,887 | uncertain |
| RGBA8 async, one-shot Optimal6, nonseekable output | 18,520.84 | 18,913.40 | 0.979 / 0.995 | 882,212 | 210,973 | pass |

There are **3 passes, 7 gaps and 5 uncertain cases**. Passing controls stop;
resolve uncertain measurements before assuming they need optimization. Only the
Fastest8 incremental raw gain above is independently attributed. Current costs
are not a broader gain claim. The fifteen comparisons have one current renderer
owner; earlier [block](PNG-BLOCK-PREPARATION.md), [color](PNG-COLOR-ONE-SHOT.md),
[delivery](PNG-BOUNDARY-ONE-SHOT.md), [RGB16](PNG-RGB16-ONE-SHOT.md) and
[gray-alpha16](PNG-GRAY-ALPHA16-ONE-SHOT.md) reports remain history.

## Storage, correctness and reproduction

Allocation rises **8B/op** from widening scalar object state; no arrays or retained
buffer capacity are added. Deflate admission remains **384 KiB**, scratch retained
after disposal is zero. Complete physical/admitted peak and native allocation are
unmeasured; lower CPU time does not imply a smaller peak.

Windows/Linux each pass **5,707 full/live checks** (2,472 internal, 2,287 public,
948 differential), plus **581 internal/211 public PNG checks per normal/scalar
mode**. The independent actual-writer oracle now covers 1,152 valid-tree/alignment/
run/buffer-edge combinations, including 15 bit literal/distance codes and 48 bit
matches; all 256 lengths × 56 distance-range endpoints remain covered. Frozen complete
packets, actual failures before a write, after partial acceptance and after a complete write, tight shared admission,
terminal refusal, cancellation cleanup and subsequent root reuse pass.

The **same actual Release 0.2.1 package** (`4f24c8f820285693…`,411,105B)
and all three usage examples pass Windows/Linux. Windows main
`d94850983d5ba2f8705dd061ea03bd610b56776665d9b6c0eeea211cc73d2f82`; Linux main
`aadde52f525ec6909f78d48906a5964a6ad807b253c8bef21492cf0131049832`. Published packages are unchanged; publication
is postponed. This is a development milestone, not a full release refresh.

One successful core/embedded-test version per OS is reused throughout. One Linux
preflight fails a sign-extension warning before execution; its source/log remain,
and explicit unsigned conversions fix it. An optional disassembly selector emits
no file; its collection failure is retained without repeating completed tests or
claiming generated-code evidence. No harness/native/selector rebuild; one package.
The thirteen other existing current controls refresh once, reusing the two already
completed confirmations. No input/settings grid or speed-only follow-up variant.

[Numeric evidence](png-complete-token-output-evidence.json) pins producers,
source hashes, all confidence views, boundaries, gates, package and retained
history. Mirrored/member-verified raw `png-complete-token-output-qualified.tar.xz`:
12,628,024B/1,685members, SHA256
`46179805a4bfcc6a28d3e63322537807c1881fbec0ef85766658006c116bd6a1`. It contains original/selected source/modules/tests/package,
workers, commands, runtime selectors, actuals, guards and reproducible scripts;
unchanged reference-source/fixture provenance stays in pinned dependencies.
PNG level1/6/9 costs are refreshed; the three already-historical affected rows
remain H. Other 110 displayed comparisons reuse unchanged-path evidence. The
full 282-path objective remains open; JPEG refinement attribution is the next
independent material lead.
