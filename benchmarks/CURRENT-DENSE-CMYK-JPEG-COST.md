# Current dense-CMYK JPEG decode cost

The existing dense/general four-component control independently passes:
**2,786.896 µs, 0.894× / conservative upper 0.916**, **1,085,661 B/op**.
Stop this case at its tested scope. This resolves a stale baseline-entropy cost;
no optimization or causal gain against historical producers is claimed.

The original 513×259 Adobe-transform0 packet has unit quantizers and all 63
AC coefficients set to four, exercising the required general inverse path.
Both subjects time complete resident decoding, all four component planes,
device-color normalization, fresh zeroed RGBA ownership, packing and release.
Input, reference executable/settings, ICC/reorientation choices and one-CPU
boundary remain fixed. Native allocation is unmeasured.

Preflight catches old floating-inverse expected pixels before any timing.
An independent arbitrary-size integer matrix evaluates the original block means
and spectra with the documented Q14 row/column and Q16 black-product boundaries.
Every managed normal/scalar byte on both OSes and every normal/scalar FFmpeg byte
agrees exactly. The old derived reference differs at 208,403 RGB samples,
maximum 2/RMSE 0.769001, with exact alpha. Only derived expected samples and
their metadata are overlaid temporarily; original packet, raster, projection,
means, quantizers and native output remain unchanged. No tolerance is relaxed.
The original derived files are retained and restored byte-exact afterward.

One independent twelve retains every actual, both native brackets and passing
idle-v2 guards. Frozen immutable production, harness, generic selector, workers
and fixtures are reused with **zero builds**, profiles or repeated timings.
Applicable 5,289 checks per OS and the same actual Release-package/three-example
qualification carry forward. Input preservation, independent output ownership
and exact pixels pass before and after measurement.

[Numeric evidence](current-dense-cmyk-jpeg-cost-evidence.json) pins every sample,
producer, independent proof, failed preflight and restoration. Raw
current-dense-cmyk-jpeg-confirmation.tar.xz, SHA256
8f17c929f04a30039cd17e8add6f7eaed919ecfd22ece4b8b7301ef593c4081d,
is mirrored with all 63 members verified. It retains old/corrected derived data,
original input, oracle, actual runtime/selector/worker, receipts and guards;
prior qualified archives are pinned dependencies. This adds one displayed
comparison already in the unchanged 268-path required scope. Broader JPEG
performance and the full parity objective remain open.
