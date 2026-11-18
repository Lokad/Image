# Async encoding on the current package

All three remaining fresh nonseekable encode controls pass the conservative
single-CPU ratio gate on the actual 0.2.1 package. Together with the existing
PNG pass, this covers the four original asynchronous encoded-output pool
boundaries. [Nonseekable PNG encoding](PNG-CURRENT-PACKAGE.md)

| Complete operation | Reference | Lokad.Image / reference µs | Conservative interval | GC B/op | Lokad.Image / reference packet B |
| --- | --- | ---: | ---: | ---: | ---: |
| RGBA8 lossless WebP | libwebp exact=1 | 113,899.00 / 129,908.55 | 0.863768–0.889992 | 5,949,048 | 183,812 / 183,812 |
| Caller RGB24 JPEG 4:2:0 | FFmpeg | 1,469.27 / 1,628.99 | 0.854606–0.921474 | 309,523 | 52,091 / 52,093 |
| RGBA8 JPEG 4:4:4 | FFmpeg | 3,173.59 / 3,998.09 | 0.759588–0.829650 | 820,502 | 133,039 / 133,041 |

The timed operations include the original input delivery, conversion or
flattening where applicable, encoding, fresh nonseekable output, staging,
awaited delivery and release. The callback sink completes immediately.
Separate untimed checks verify preserved samples, packet and reference
settings, source ownership, surviving outputs, refusal, cancellation and
sink failure recovery. The WebP packet matches byte for byte. Each JPEG
packet matches the frozen content with the approved COM attribution
difference. The probes use private proof files and retain the original
input and native reference identities.

Twelve managed actuals, with twelve native samples in each bracket, retain
the original generated operations and natural nonconcurrent GC. The bounds
include the filtered and all-actual views. One tools-only build binds the
frozen workloads to the actual package DLL. Existing package qualification
carries forward. All guards passed. The prior producer's timings stay in
their original reports.
[Evidence](async-encode-current-package-evidence.json)

The mirrored `current-package-async-lossless-webp.tar.xz` retains 10 verified members and 21,508 B.
SHA-256 `8f1993e49ed091f12d705127c63428cef10787cb14902870d102cf53a1f7ffab`.

The mirrored `current-package-async-jpeg.tar.xz` retains 26 verified members and 431,600 B.
SHA-256 `3874f568ee36ac0925fa04ed3bd7d22abdc5a100f3f05489702f4df6a2d8ae49`.
