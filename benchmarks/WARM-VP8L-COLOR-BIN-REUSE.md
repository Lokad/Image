# Lossless WebP color-bin reuse

Retain construction-derived nonzero bins in existing inactive-plane scratch,
then score those bins without scanning all 256 entries. No capacity, allocation,
ownership, arithmetic, tie, admission or cancellation contract changes.

The existing opaque control independently measured **4.02% less filtered
elapsed**, **3.65% across all actuals** and **4.26% normalized point improvement**.
Raw confidence intervals separate; normalized intervals overlap. This specific
result is explicitly accepted with that caveat. The original decision and raw
measurements remain intact; the opaque operation is not retimed.

Three existing affected companions each receive one independent 12-iteration run.
These are current costs, without separately paired speedup claims. Reference is
libwebp exact=1, FFmpeg's encoder backend, lossless/effort 4/quality 75, one CPU.
All inputs, settings, normalization and complete owned-operation boundaries stay
frozen. All four complete packets, RGBA/alpha/hidden RGB and ownership gates pass.

| Existing 513×259 control | Lokad µs | Ratio / upper | Managed B/op | Packet B, exact both |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 | 158,892.27 | 1.308 / 1.335 | 29,696,928 | 183,812 |
| RGBA16 normalization | 166,167.06 | 1.356 / 1.385 | 29,696,928 | 183,812 |
| Opaque RGBA8 | 99,191.77 | 1.393 / 1.447 | 19,992,640 | 121,896 |
| Opaque photograph | 110,839.47 | 1.309 / 1.326 | 20,418,338 | 160,032 |

All four controls remain gaps against warm single-CPU upper ≤1×. Broader content,
settings or timing claims are not established. Palette diagram at effort 4 and green-only
compressed VP8 alpha skip this search and retain unchanged-path evidence.

Both OSes pass **5,240 Release checks each** (2,111 internal, 2,181 public, 948 live),
plus 25 internal/153 public focused cases in normal/scalar modes. The same actual
Windows Release package passes on both OSes, including exact WebP controls,
cooperative admission/async ownership and all three usage examples. Previously
measured immutable candidate DLLs are reused: zero additional core, harness,
native or selector builds. No variant or additional profile is introduced.

[Evidence](warm-vp8l-color-bin-reuse-evidence.json) identifies the current
producers/package and mirrored/member-verified `vp8l-color-bin-reuse-retained.tar.xz`, SHA256 6e8e51e4e107d33bd0fe9391ced520e28c6c336de66b53bda9ffee30a77ca87f.
The [original decision](VP8L-COLOR-BIN-REUSE-REJECTED.md) remains historical.
All 268 required material paths remain in the performance objective.
