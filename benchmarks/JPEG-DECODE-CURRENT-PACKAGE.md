# JPEG decode on the current package

Five original owned-output JPEG cases were measured on the immutable 0.2.1
package. Horizontal-only Adobe CMYK passes with adjacent native samples.
The earlier gap remains in the records below. Sparse-2D remains uncertain.
All three progressive cases pass and stop.

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
