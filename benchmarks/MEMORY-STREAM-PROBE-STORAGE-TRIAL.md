# Memory-stream probe storage trial

**Not retained.** The original JPEG probe controls use private custom `Stream`
callbacks, so selecting 32-byte storage for exact ordinary `MemoryStream` inputs
does not affect their allocation: **568 B/op** on both DLLs. The three-pair CPU
screen is inconclusive: **0.222035 → 0.218828 µs**, ratio interval
**0.905472–1.072728**. No CPU gain, regression or FFmpeg parity is established.

Unlike the earlier [growth trial](PROBE-INITIAL-WORKSPACE-TRIAL.md), this candidate
selects smaller storage only for a known seekable type and never promotes it.
The exact-type check introduces no early user metadata callback. Grammar, reads,
counters, cancellation and the admission owner stay unchanged. Custom/derived
streams retain their original forward-draining buffer.

Six public controls verify ordinary-memory prefixes at nonzero positions, independent
metadata, exact counters, shared refusal, cancellation/failure cleanup, recovery
and derived-stream callbacks. Ordinary-memory admission falls **512 → 288 B**;
that logical capacity saving is outside the frozen benchmark delivery. Its total
GC allocation is unmeasured. Original callbacks/settings/inputs remain unchanged;
no new delivery benchmark or image/settings grid is added to justify the candidate.

Linux passes **10 internal/154 public** normal/scalar checks. Four initial test
expectations incorrectly treated the two direct-field constructors differently;
they were corrected before timing. Retain those failures. One immutable production
build and public build, plus two embedded builds; no Windows, helper, harness,
native, fixture or package build. No independent confirmation, other-case timing,
generated-code capture or broad/package qualification follows.

Qualified sources, core/embedded/public modules and source inventory are restored
exactly without rebuilding. Candidate-specific test changes are removed; their
sources/results remain in raw evidence. Windows modules and package never change.
Reuse **6,721 full/live checks per OS** and SAME actual-package/all-three-example
qualification. Current costs remain [unchanged](REFILL-COMPLETION-INLINING.md).
Zero retained operation workspace; physical/native/JIT peaks remain unmeasured.

[Evidence](memory-stream-probe-storage-trial-evidence.json) pins the actual input
type, all samples, sources/modules and restoration. Raw `memory-stream-probe-storage-trial.tar.xz`
(2,137,040 B/76 members), SHA256
`b3ec78ed6d6e3332a0a1c60268aa4281e9e1217df62cdf76e0448d3ae69e30ba`, is mirrored/member-verified with the original helper and
qualified-package dependencies. This rejected trial is outside renderer dependencies.
The full 289-path objective remains open; publication is postponed.
