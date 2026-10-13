# RGBA8 bilinear upsampling: comparison unmatched

The 513×259 to **1027×519** transparent RGBA8 case has **no timing or allocation
score**. Lokad.Image matches the exact rational reference on Windows and Linux,
with intrinsics enabled and disabled. Geometry, source preservation and two
independent held outputs pass. It uses the same original raster as downsampling.

Normal and scalar FFmpeg CLI outputs agree with each other, but differ from the
reference at **four alpha samples**, each by one (RMSE **0.002739**). FFmpeg also
retains RGB at **one pixel whose output alpha is zero**, producing an RGB maximum
error of 214. The native worker correctly refuses the existing exact-alpha gate
before any performance iteration. Existing downsampling refusal/ownership
controls at both depths still pass.

An untimed analysis finds that clearing RGB where native alpha is zero would
leave RGB maximum error **1**, RMSE **0.002959**, with alpha unchanged. This is a
possible baseline adaptation, not an implemented or scored comparison. Any
adaptation must remain explicit, include its full cost inside the native timed
operation, and preserve Lokad.Image's exact pixel contract. The previously
approved RGBA16 alpha bound is not extended to RGBA8 automatically.

The development harness adds only this output geometry and explicit dimensions
to the native adapter. Production is unchanged: fresh builds on both OSes match
all 630 method bodies and implementation flags of the actual validated package.
The [numeric evidence](warm-bilinearup8-evidence.json) retains the
strict refusal, exact pixel controls, six geometry/option negatives, source and
native identities, and frozen raw archive. No new original raster is introduced.
The subsequent [measured round](WARM-BILINEAR-UP8.md) uses the explicitly approved
baseline adaptation and exposes a 3.041× gap. This report and its strict raw
refusal remain historical evidence; the managed pixel contract is unchanged.
