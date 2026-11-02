# Compositing reference feasibility

The scoped RGBA8 tolerance and ImageSharp 3.1.12 RGBA16 reference/tolerance
were approved on 2026-11-02. [Current measurements](TEXT-DRAWING-CURRENT.md)
use those exact scopes; the original feasibility evidence below is retained.

The fixed 513×259 RGBA8/RGBA16 controls pass an independent BigInteger
source-over oracle on the current Windows and Linux DLLs. Both borrow separate
source and destination images, composite at (17,19), and clip right/bottom.
A shared budget admits exactly two frames; source disposal leaves the destination
charged, and destination disposal releases the final charge. Source pixels stay
unchanged. These checks establish correctness and ownership, **not performance**.

A separate, same-revision FFmpeg n8.1 overlay-only build preserves every existing
reference prefix and worker. The first graph prototype copied the destination
and failed the borrowed-canvas boundary. The corrected public-API sequence submits
the immutable source at timestamp −1 and closes its timeline. Each subsequent
canvas reference passes through the graph and returns with the same pixel pointer.
Three independently reset operations preserve the pointer and source pixels;
graph threading is disabled. Reset, preparation and verification are outside
the operation. A scored managed/native harness is still outstanding.

Native RGBA8 differs from the exact rational result by at most **one RGB sample**,
RMSE **0.386072**. Alpha, untouched pixels and zero-source-alpha pixels are exact.
This reflects intermediate effective-alpha quantization. A proposed native-only
bound of RGB ≤1 and RMSE ≤0.5 is now approved. Managed pixels remain exact;
alpha, untouched pixels and zero-source-alpha pixels stay exact on both sides.

This FFmpeg overlay has no full-precision RGBA16/GBRAP16 path. Its existing GEQ
filter reproduces the exact 16-bit result using explicit integer rounding in
binary64. That check uses a pre-stacked input and format conversion; it is
**not an approved performance reference**. Neither downconversion nor a generic
expression-filter timing establishes equivalent compositing performance.

A bounded alternative check uses the existing ImageSharp 3.1.12 `Rgba64`
`DrawImage`, normal source-over and opacity1. Three reset operations preserve
destination storage and source pixels on Windows/Linux. RGB/alpha maxima are
one sample, with RMSE **0.040101/0.038991**; untouched and zero-source-alpha
pixels are exact. This is a separate reference, **not FFmpeg performance**.
A tooling-only exception for this fixed control, with RGB/alpha ≤1 and RMSE
≤0.25 each, is now approved. It remains explicitly labeled as an alternative
reference; no production dependency or managed pixel relaxation follows.

At this feasibility checkpoint all nine audited paths were unmeasured; the
current measurements linked above supersede that status. Production, the actual
package and existing qualification are unchanged: reuse 5,378 checks per OS and
the same package consumer proof. This round adds no production build, full-suite
repeat, package rebuild, timing or new fixture.

[Evidence](composite-reference-feasibility-evidence.json). Retained
composite-reference-feasibility.tar.xz: **2,561,772 bytes / 51 members**,
SHA256 **716a85d706f813721ce12c3137c00d316aa9e082a0646c919541d547293b1aa9**,
mirrored and member-verified. It contains the original probes, failed and passing
ownership records, managed diagnostic consumer, input/output rasters, actual
Linux producers, build flags and per-member hashes. With the pinned external
FFmpeg checkout, replay cmf61-build-overlay-vm.py in a fresh separate prefix,
then the archived cmf61-run-repeat-overlay-vm.py and cmf61-geq16-check-vm.py
from the repository root. Run the retained Cmf61Managed consumer against each
frozen platform DLL. The [packed-fill round](WARM-RECTANGLE-FILL-PACKED.md)
retains the unchanged full/package qualification dependency.

The alternative probe is retained separately in
composite-rgba16-alternative-reference.tar.xz, **1,457,252 bytes / 11 members**,
SHA256 **a3f0690a0b20af521a54634474f616f3b5542e868cd106f0fbb2a52f4d17cbda**,
mirrored and member-verified. It includes the portable diagnostic consumer,
actual reference DLL, both-platform records, output and member hashes. Its
original input/oracle dependency is the compositing archive above. Run the
retained Crf62 consumer against the repository root; it makes no timing claim.
