# Decoder exactness

[The finite checklist and numeric evidence](decoder-exactness-evidence.json)
reuse existing controls and pinned references. Exact normalized samples and typed
source metadata are required; older lossy error bounds remain transitional gates.

| Required path | Current evidence and next action |
|---|---|
| PNG normalization, Adam7, entropy, packed depths and tRNS | Existing exact frozen/live assertions pass; stop at that scope. |
| VP8L transforms, palette, cache and copies | Existing60-fixture/authored/live exact assertions pass; stop at that scope. |
| JPEG inverse and progressive coefficients | Qualified integer primitive is public;14 frozen grayscale/full-resolution controls and named live cases are exact. Passing inverse cases stopped. |
| JPEG444/422/420 color and direct RGB | Full-resolution cases pass. Existing31x23 baseline422 has734 differing channels, maximum1; inspect chroma expansion before editing. |
| JPEG CMYK/YCCK and411/410 | Functional checks pass; exact color samples remain uncertain. |
| JPEG caller grayscale/RGB/CMYK | Native Y and full-resolution RGB/padding/orientation pass. Subsampled/CMYK and direct-RGB grayscale projection need exact confirmation. |
| VP8 reconstruction and deblocking | Measure planar reconstruction before changing a primitive. |
| VP8 chroma/RGB and ALPH | RGB exactness uncertain; retain passing exact-alpha controls. |
| Typed source metadata, orientation and delivery | Existing synchronous/asynchronous, caller-buffer and lifetime checks pass; retain them. |

E103 public JPEG adoption replaces the floating inverse and its alternate variants
with generated fixed-point arithmetic. Full-resolution color uses rationally
derived BT.601 coefficients at the pinned precision. Original arithmetic controls
cover ties, saturation/narrowing and a wide coefficient where native C differs
from normal AVX; managed scalar execution targets the pinned normal result.
The existing31x23 anchor has exact complete RGBA, caller Y/RGB and oriented pixels.
The matrix and inverse definitions follow public standards without upstream code
or implementation tables in production.

Windows and Linux each pass5,045 checks, including live FFmpeg, plus33 internal
and71 public affected scalar checks. The SAME actual private0.2.0 package passes
both consumers, exact JPEG stream/sample/ownership checks and all3 usage examples.
Resource/work charges, terminal refusal, cancellation and retained ownership are
preserved. Previous JPEG decode timing/allocation rows describe the replaced
producer; no new CPU or GC measurement is claimed. Broader exact decoding, warm
single-CPU parity and replacement readiness remain open.

The next work reuses the verified immutable build on the existing baseline422
control. No new quality/shape grid, PSNR calibration or retuning of passing cases
is needed. Add cases only for a distinct required algorithm/public boundary.
