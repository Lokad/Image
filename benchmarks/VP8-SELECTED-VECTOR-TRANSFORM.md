# VP8 selected-block forward transform

The frozen 513×259 effort-2 diagram passes on the exact Release-package DLL.
The complete operation measures **2,575.756 / FFmpeg-libwebp 2,659.649 µs**,
with conservative ratio interval **0.954182–0.979850**, **406,664 B/op** and
an exact **7,330-byte** packet.

Independent alternating managed pairs measure **2,656.543 → 2,593.172 µs**,
saving **63.371 µs (2.39%)** with candidate/prior interval **0.969515–0.982805**.
Both captures retain all twelve 128-operation units and use the quiet VM,
one CPU, optimized JIT and natural nonconcurrent workstation GC. The native
worker retains its declared wrapper and scratch. Both sides process the full
resident input, acquire independent outputs and dispose them every operation.
Source, ownership, reconstructed quality, alpha and complete bytes pass.

In the distortion-only search, the chosen 4×4 predictor and its source block
already occupy registers in pixel order. The selected transform widens before
subtraction, then performs both exact integer passes in registers. Byte
provenance bounds the signed residuals and coefficients. The search still
uses the original order and tie rules, reconstructs the chosen pixels before
the next block and retains the cancellation checkpoints and admitted stack
workspace. The checked scalar transform and other callers remain unchanged.
The isolated branch adds zero heap capacity or retained storage. Complete
physical and native peaks were outside this measurement.

The earlier shared-scalar split saved 29.8 µs in matched distortion-only calls.
Its full photograph RDO comparison was uncertain in the original bracket
schedule and showed a gap with interleaved brackets. The isolated branch
leaves that shared scalar path intact. The earlier scores, sources and
validation are retained in the 35-member archive
`tmp/final-refresh/vp8-selected-transform/results.tar.xz` (`3c90842c`).
Unchanged RDO, trellis and alpha paths reuse their
[source-specific evidence](VP8-VARIABLE-PRICE-PREPARATION.md).

Windows and Linux each pass 6,866 Release checks, including 948 live FFmpeg
comparisons, plus 1,043 internal and 367 public focused checks per fallback
mode with AVX512F disabled and with hardware intrinsics disabled. The same
actual package `e5ef192c` contains timed DLL `d71ba2bc` and passes the consumer
and three usage examples on both platforms. The revised Linux test setup
binds the solution root so the already staged PNG fixtures resolve.

The [evidence](vp8-selected-vector-transform-evidence.json) records immutable
identities, case boundaries, performance, qualification and the verified
archive. Gray8 PNG and the final consolidated package refresh remain open.
