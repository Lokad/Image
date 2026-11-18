# JPEG decode on the current package

Five original owned-output JPEG cases were measured on the immutable 0.2.1
package. The horizontal-only Adobe CMYK case shows a 49.25 µs gap in the
complete resident comparison. Sparse-2D remains uncertain. All three
progressive cases pass and stop.

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
does not attribute a slowdown to the pool or any kernel. The gap warrants
targeted attribution on the current package.

One temporary orchestration build supplies the natural-GC driver. The
production package, harness, fixture and reference binaries are reused.
Managed GC allocation is distinct from admission, live capacity and retention.
The mirrored `jpeg-owned-decode-current-package-confirmations.tar.xz` contains 39
verified members and 44,028 B, SHA-256
`a0ddcefd684842b0ba9cb56e809a4864ee4bf8751c3d810876ed0a8c31019f86`. It retains all samples,
reference identities, guards and original receipts.
[Evidence](jpeg-decode-current-package-evidence.json)
