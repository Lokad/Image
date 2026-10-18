# Direct JPEG sample encoding

Two new complete public boundaries were screened on the dedicated quiet VM,
using the existing original 513×259 gray/RGB projections and quality anchors.
Both remain demonstrated warm single-CPU parity gaps; no long confirmation or
optimization was attempted. Production bytes and full H4 qualification are reused.

| Packed caller input | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Compressed bytes, managed / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| Grayscale | 1,240.94 | 1,083.45 | 1.145 / 1.363 | 155,007 | 58,823 / 58,910 |
| RGB → 4:2:0 | 3,162.12 | 1,577.78 | 2.004 / 2.216 | 297,896 | 74,040 / 52,093 |

The grayscale reference is the approved SIMD-enabled libjpeg-turbo 3.1.4.1
benchmark-only exception. RGB uses FFmpeg n8.1 MJPEG. Both adapters reuse the
already pinned native libraries; original workers and their evidence remain.
The new adapters borrow the same packed input samples supplied to Lokad.Image.
Every operation includes necessary block/color normalization, transform, coding,
fresh independent output acquisition and release. Managed admission and fresh
MemoryStream creation/growth/disposal are included. Private native contexts and
conversion scratch may remain warm; encoded results are never cached. Neither
subject stages an RGBA frame or performs unnecessary alpha flattening. Input
projection, startup, disk I/O, IPC, hashing and quality checks are excluded.

Packets exactly match the qualified quality anchors. Grayscale source PSNR gap
is 0.000033 dB; RGB 4:2:0 gap is 0.106110 dB, both within 0.25 dB. Quality numbers
are encoder-specific. Decoder RGB agreement remains max4/RMSE1.5; opaque alpha,
caller input and simultaneously surviving independent outputs pass. Native
allocations remain unmeasured. No image, quality, shape or sampling grid was added.

Each screen retains three 200 ms managed actuals, three 50 ms native brackets
on each side, and both five-second idle/process guards. One pinned CPU, normal
SIMD, tiering disabled, nonconcurrent workstation GC. One changed benchmark DLL
and two raw-input adapters were built; production, native libraries and cached
job selectors were reused. No production/package requalification was needed.

[Numeric evidence](warm-jpeg-sample-encode-evidence.json) retains all actuals,
bounds, allocation counters, guards, packet quality and source/build identities.
The first result reader expected capitalized native sample keys; its failure is
retained and corrected against the same completed job, without remeasurement.
Raw archive `h5-jpeg-sample-encode-evidence.tar.gz`, SHA-256
`469370cf41c8f7d4aa03f8d800bd4d09220f3be6ab2fb1d4f937c5e6bab62f86`, is retained on both hosts.

The finite checklist stays at 218 cases: 209 numeric rows (17 prior-production
JPEG decode/inspection rows), plus nine unmeasured operations. Current status:
63 independent passes, 128 mean gaps and one confidence-open case. Other paths
keep their valid evidence; parity work remains open.
