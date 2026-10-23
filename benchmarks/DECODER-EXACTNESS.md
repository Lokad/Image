# Decoder exactness

The finite checklist in [the evidence ledger](decoder-exactness-evidence.json)
reuses existing controls and the verified E99 production build. Completion requires
exact normalized samples, explicit depth/stride/channel/range conventions and
consistent typed source metadata. Older lossy RGB bounds establish interoperability,
not exactness. Functional, admission and ownership safeguards remain mandatory.

| Required path | Current evidence and next action |
|---|---|
| PNG normalization, Adam7, entropy, packed depths and tRNS | Existing frozen/live exact assertions pass on both platforms; stop at that scope. |
| VP8L transforms, palette, cache and copies | Existing60-fixture and authored/live exact assertions pass; stop at that scope. |
| JPEG inverse transform and progressive coefficients | Private E101 coefficient/inverse primitive qualified; public adoption and progressive confirmations open. |
| JPEG444/422/420 color and direct RGB | Private E102 complete444 slice qualified; public adoption and subsampled confirmations open. |
| JPEG CMYK/YCCK and411/410 | Existing named controls have functional/tolerance evidence; exact samples uncertain. |
| JPEG caller grayscale/RGB/CMYK buffers | Gray/RGB initial gap shares the inverse/color stages; CMYK exact proof uncertain. |
| VP8 reconstruction and deblocking | Existing RGB tolerance does not isolate planar reconstruction; measure before editing. |
| VP8 chroma/RGB and ALPH | RGB exactness uncertain; retain passing exact-alpha controls. |
| Typed source metadata, opt-in orientation and delivery | Existing synchronous/asynchronous metadata, caller-buffer and lifetime tests pass; retain them. |

The first diagnostic uses only the existing31Ãƒâ€”23 baseline444 JPEG, SHA
`ad1d9d11e42273349e85cfebc9b73c89231b595b883cd25d2b7832f62c353573`.
Public decoding differs from the pinned FFmpeg n8.1 result at7 of713 luma samples
(maximum1) and50 of2,139 RGB samples (maximum2); caller RGB agrees with normalized
RGBA. Native scalar and normal SIMD outputs agree for this case. These numbers
are sample comparisons, without PSNR calibration or CPU/allocation measurements.
They establish a real gap before RGB conversion; coefficients and inverse rounding
still need to be distinguished. Metadata remains a typed semantic comparison,
not equality of implementation-specific strings.

No production rebuild, new image/settings grid or repeated package proof was
needed. The next primitive must use independently derived arithmetic and retain
shared admission, cancellation, bounded workspace and caller ownership. Unpublished
rounding policies impose no compatibility requirement. Passing cases are stopped;
future cases require evidence of a distinct required path missing here.

E101 qualifies the independent fixed-point inverse primitive before public
adoption. All36 visible blocks match the independent coefficient visitor, and
all2,139 reconstructed Y/Cb/Cr samples match actual FFmpeg output. Eighteen
original arithmetic controls pin rounding, saturation and narrowing, including
a large-coefficient case where the native C and normal AVX functions differ.
The managed target remains the pinned normal result in both execution modes;
there is no pixel tolerance or extra encoder/quality control.

All23 focused checks pass on Windows and Linux, normal and intrinsics disabled,
using one immutable test assembly. The public production build is unchanged;
its earlier package qualification is reused. The next private primitive is
full-resolution inverse color, followed by coherent public adoption and removal
of the superseded inverse paths. No compatibility layer or old-rounding option
is required. CPU/allocation measurements remain deferred.

E102 qualifies full-resolution inverse color using exact rational BT.601 weights
and the pinned integer precision boundaries. The composed inverse/color slice
reproduces all2,852 frozen RGBA bytes, with identical RGB packing. A single
10-pixel arithmetic row separately covers clipping, full-range gray and rounding.
All31 focused inverse/color checks pass on both platforms with normal and disabled
intrinsics, using one immutable test assembly. Public decoder adoption remains
the next coherent milestone; current public pixels are not claimed changed.
