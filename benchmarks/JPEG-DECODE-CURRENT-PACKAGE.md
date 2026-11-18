# JPEG decode on the current package

All 24 original owned-output JPEG cases affected by the shared pool have
passing evidence on immutable 0.2.1 packages. Both horizontal and sparse-2D
Adobe CMYK pass with adjacent native samples. The initial five captures
below retain their earlier producer and timing outcomes.

| Complete operation | Lokad.Image µs | FFmpeg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Horizontal-only CMYK | 465.39 | 416.14 | 1.083395–1.133023 | 20,944 | Gap |
| Sparse-2D CMYK | 459.79 | 465.71 | 0.972862–1.001969 | 20,944 | Uncertain |
| Progressive grayscale | 1,714.69 | 1,845.30 | 0.922561–0.935937 | 151,432 | Pass |
| Progressive 4:4:4, live token | 3,276.96 | 3,644.66 | 0.890873–0.905322 | 169,664 | Pass |
| Progressive 4:4:4 | 2,896.31 | 3,361.26 | 0.853383–0.868653 | 169,664 | Pass |

The samples use the original generated delegates from one verified runtime,
three warmups and twelve measured actuals with natural GC. Both original
twelve-sample native brackets retain their production settings and prepared
state. Every operation completes acquisition of the owned RGBA result, decode,
normalization and disposal. Input, managed and native output hashes match the
prior receipts. The original case-specific reference bounds, alpha, live-token,
source and independent-owner gates and both quietness guards pass.

Horizontal CMYK remains a gap in both the filtered and all-actual analyses.
Managed actuals range from 460.89 to 467.94 µs, and native bracket means are
420.10 and 416.14 µs. Its earlier passing cost and the scoped 16.92 µs
paired improvement remain tied to the prior producer. The new comparison
does not attribute a slowdown to the pool or any kernel. The adjacent confirmation below tests the timing boundary directly.

One temporary orchestration build supplies the natural-GC driver. The
production package, harness, fixture and reference binaries are reused.
Managed GC allocation is distinct from admission, live capacity and retention.
The mirrored `jpeg-owned-decode-current-package-confirmations.tar.xz` contains 39
verified members and 44,028 B, SHA-256
`a0ddcefd684842b0ba9cb56e809a4864ee4bf8751c3d810876ed0a8c31019f86`. It retains all samples,
reference identities, guards and original receipts.
[Evidence](jpeg-decode-current-package-evidence.json)

## Matched module diagnostic

The original direct public Decode pair driver compares the earlier timed Linux
module with the current package module on the same frozen packet. Twelve
alternating 128-call pairs measure **380.67 / 383.64 µs**, current/earlier
interval **0.997905–1.017712**. Exact pixels, independent output lifetimes and
both quietness guards pass. This scoped assembly-load-context comparison
does not establish a material module slowdown. The complete generated
workload's earlier FFmpeg gap remains in the records. The adjacent
confirmation uses that same generated managed workload.

The mirrored `jpeg-horizontal-cmyk-current-pair.tar.xz` contains 8 verified members
and 17,196 B, SHA-256 `e36ea1b3254fcc3b0f59f626f2b87f1960329dfe1ebc56790577c60277e47830`.
All pairs, the reused driver identity, original receipts and guards are retained.

## Adjacent reference confirmation

Horizontal-only CMYK passes the original complete resident boundary at
**412.52 / 425.40 µs**, conservative interval **0.957482–0.980020** and
**20,948 managed B/op**. Two native samples of 216 operations surround every
managed actual outside its clock. The original twelve measured actuals,
three warmups and 200 ms pilot rule remain. The pilot selects 480 managed
operations per actual. Both original native brackets and both sets of
adjacent samples enter the full conservative rule. Native group means span
425.40–426.79 µs. The two paired upper bounds are 0.972499 and 0.970162.

Input, pixel, ownership, reference identity and quietness checks pass.
The prior 465.39 / 416.14 µs capture and the matched module diagnostic remain
available. The change in elapsed time has no verified cause. This pass
qualifies the declared adjacent schedule and establishes no production
speedup. The allocation counter is separate from owned capacity and idle
retention. The horizontal case stops here.

The mirrored `jpeg-horizontal-current-interleaved-confirmation.tar.xz` contains 13 verified members
and 25,108 B, SHA-256
`e3f67d8a7b38d1c9d727d13d34190fab1da3c0b524ad40577134a47edc7b07c5`.
It retains every sample, the generated-loop and runtime identities, original
and current receipts, and the guard checks.

## Remaining owned JPEG cases

All nineteen pass and stop.

| Complete case | Lokad.Image µs | FFmpeg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| `Progressive422` | 1,817.00 | 2,246.36 | 0.781415–0.820470 | 315,496 | Pass |
| `Progressive420` | 1,428.34 | 1,964.55 | 0.708744–0.744485 | 243,676 | Pass |
| `CmykAdobeGeneral` | 1,310.33 | 3,113.11 | 0.405694–0.433833 | 20,944 | Pass |
| `StreamDecode420Restart` | 801.57 | 996.79 | 0.776245–0.824624 | 262,914 | Pass |
| `NativeDecode420Restart` | 850.10 | 1,014.17 | 0.771236–0.855592 | 262,831 | Pass |
| `Decode8` | 1,430.25 | 1,899.18 | 0.733248–0.763840 | 4,320 | Pass |
| `NativeDecodeCancelable8` | 1,226.22 | 1,730.12 | 0.688612–0.730370 | 42,936 | Pass |
| `NativeDecode8` | 1,178.38 | 1,723.57 | 0.669002–0.692571 | 42,936 | Pass |
| `DirectRgb444` | 1,141.73 | 1,450.62 | 0.780278–0.795493 | 42,936 | Pass |
| `DirectRgb422` | 874.19 | 1,094.26 | 0.792658–0.804705 | 330,266 | Pass |
| `NativeDecode422` | 852.37 | 1,352.09 | 0.618348–0.640584 | 330,266 | Pass |
| `NativeDecode420` | 718.85 | 1,088.64 | 0.641920–0.671216 | 258,456 | Pass |
| `DirectRgb420` | 637.73 | 815.89 | 0.760107–0.800163 | 258,457 | Pass |
| `OwnedDecodeGray` | 617.57 | 747.80 | 0.812621–0.839363 | 140,455 | Pass |
| `Ycck420` | 460.94 | 778.26 | 0.585414–0.600071 | 895,562 | Pass |
| `Decode411` | 410.15 | 489.45 | 0.823056–0.849040 | 219,551 | Pass |
| `CmykAdobeAcFresh` | 411.60 | 831.50 | 0.488446–0.498143 | 20,944 | Pass |
| `Decode410` | 364.25 | 553.58 | 0.645863–0.669676 | 189,090 | Pass |
| `CmykAdobe` | 294.77 | 406.48 | 0.706025–0.730817 | 4,536 | Pass |

The two verified runtimes reuse the existing natural-GC driver, original
generated delegates, three warmups, twelve measured actuals and both original
twelve-sample native brackets. The fresh-seekable restart case includes input
delivery on both sides. The explicitly labeled fresh CMYK comparison retains
FFmpeg context creation inside each operation. Other cases retain their
original reference contexts. All include complete decode, RGBA normalization,
owned result acquisition and disposal.

Input, managed and native output identities match the original receipts.
Exact managed checks, case-specific native bounds, opacity, cancellation where
applicable and independent ownership remain. Both idle guards pass for every
case. Managed GC figures are separate from retained output capacity and
unmeasured physical peaks. No build or production change was needed.

The mirrored `jpeg-remaining-owned-decode-current-package.tar.xz` contains 120
verified members and 93,276 B, SHA-256
`4a0716ad5a2eb2039954d091fcdb342db631be226e54c018dff152b2585b8c66`.
It retains the inputs' identities, prior and current receipts, generated-loop
and native identities, guards and every measured actual. The unchanged
caller-sample, coding-only and inspection cases keep their own boundaries.

## Sparse-2D adjacent native confirmation

Sparse-2D Adobe CMYK passes on the latest actual package at **460.73 /
FFmpeg 471.84 µs**, with a conservative interval of **0.957700–0.987805**
and **20,944 B/op**. Both paired intervals and the all-actual and filtered
bounds pass. This closes the remaining owned-output JPEG case. The earlier
upper of 1.001969 stays with its original bracketed capture.

The original 432-call managed actuals, generated loops, natural GC and
both native brackets stay in place. A 216-call native sample directly
precedes and follows each of the twelve actuals. Packet, analytical
managed pixels, native CLI and scalar bounds, exact opacity, source and
independent output checks pass. The observed clocks establish confidence
under this schedule. The production decode path is unchanged.

The first wrapper failed before starting the timed operation. A corrected
wrapper completed a capture but failed to save its combined timing log.
Its validation and both quiet guards are retained without a performance
score. The next run used the same verified DLL and worker and wrote the
combined timing log directly to memory-backed storage. No additional
build or repeated qualification was needed.

The mirrored `jpeg-sparse2d-current-adjacent.tar.xz` contains 16 verified
members and 29,020 B, SHA-256
`455bfb35b9ffb715cba8c64523aa983beca7c504ca573da47ecca18e392c0136`.
[Evidence](jpeg-decode-current-package-evidence.json)
