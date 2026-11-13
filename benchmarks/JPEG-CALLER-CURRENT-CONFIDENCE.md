# Current caller-RGB JPEG confidence

The existing caller-RGB420 control remains **uncertain** on the current immutable
package DLL. This is a measurement update, not a production change or speed claim.

| Complete 513×259 caller-RGB420 encode / FFmpeg | Managed / reference µs | Conservative ratio interval | Managed B/op | Packet B, managed / reference |
| --- | ---: | ---: | ---: | ---: |
| Current adjacent capture | 1,712.07 / 1,650.90 | 0.991–1.057 | 388,220 | 52,091 / 52,093 |

Twelve fixed, alternating managed/native pairs each run 64 complete operations;
twelve native batches before and after remain. All actuals and the original
worse-of paired/global-bracket confidence rule are preserved. Native bracket
movement keeps the result uncertain. The older 1584.61/1578.98 µs observation remains
history; no causal regression or speedup is inferred from different clocks.
Do not retry unchanged code for a favorable bound or assume an optimization gap.

The original public EncodeJpeg call includes sample validation/normalization,
RGB conversion, transform, quantization, adaptive Huffman/entropy, fresh
MemoryStream creation/growth, independent output and release. FFmpeg n8.1 borrows
the same packed RGB24, retains codec/conversion scratch and creates/releases each
complete packet. Neither side uses RGBA staging or alpha flattening. The original
input, settings, exact packets apart from the approved 2-byte attribution exception,
quality sanity checks, source/independent owners and CPU0 idle-v2 guards all pass.
Equal quality numbers are not the equivalence criterion.

One private diagnostic-driver build adapts the existing adjacent tool to the
prepared sample harness. Production, embedded tests, reference worker and fixtures
are reused without builds. The actual package contains the timed DLL; unchanged
6,600 full/live checks per OS, scalar/ISA and the same-package consumer/all three examples are
reused. No broad/package qualification is repeated.

Allocation is separate from capacity and retention. No production storage,
admission, pool or retained-state policy changes; existing ownership evidence
remains valid. Physical/native/JIT peaks are unmeasured. The other 16 JPEG encode
controls retain their identified evidence; all 289 material paths remain required.
Publication stays postponed. Complete font/text integration controls remain
passing; their stage diagnostics do not block independent work.

[Evidence](jpeg-caller-current-confidence-evidence.json) retains the full capture,
reference/source identities and qualification reuse. Raw `jpeg-caller-current-adjacent.tar.xz`:
375,112 B / 23 members,
SHA256 `05c70c4b21a3494519fdc35791a62e5e831a0e344c5dfb7a3886db41ff935f0d`, mirrored and member-verified.
Dependencies retain original tools/fixtures and the qualified actual package.
