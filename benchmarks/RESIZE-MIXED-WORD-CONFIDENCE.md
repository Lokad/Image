# Bounded mixed-word resize confidence

One counterbalanced capture resolves the next action for the existing 513×259
RGBA16→1027×129 bicubic control with input+output+4 KiB local workspace. It remains
**uncertain**, rather than a demonstrated optimization target:
**862.61 / zimg840.93 µs**, ratio1.026, conservative range **0.999440–1.041560**.
All 12 alternating pairs of 64 complete operations and 12 native batches before/
after are retained. Native means before/paired/after are845.77/845.46/840.93µs;
the faster bracket governs the comparison. Upper-outlier diagnostics cannot
improve the gate. No favorable repeat follows this result.

Timing includes the complete public managed Resize, exact geometry/filter/repair,
independent output creation/materialization and disposal. The explicitly labeled
FFmpeg zscale **zimg backend** retains graph/input/scratch; normalization, halo,
filtering, unpremultiplication, output packing/acquisition/release stay timed.
This is the established retained-reference comparison, not equivalent fresh-graph
or FFmpeg CLI performance. Fixture decode, JIT, validation and IPC are excluded.
CPU0/one thread, optimized JIT, nonconcurrent workstation GC and idle-v2 guards pass.

Frozen source/settings, exact managed rational/qualified-original samples,
approved native bounds, immutable source and independent owners all pass. Output
SHA256 remains `6e289a9a8316325b78988adbad8ab7102c81e302216b79c45ef086410f6f691a`.
Direct allocation is112B/op in every pair; the historical114B BDN figure has a
different counter boundary, so no saving is claimed. Output payload1,059,864B,
4KiB scratch admission, zero operation scratch retention and existing pool/owner
behavior are unchanged. Physical/native/JIT/pool peaks are unmeasured.

The prior standalone857.10/native848.80µs,range.996–1.023 remains separately
visible. No production speedup or full parity is inferred. Current resize scope
still has65passes/zero demonstrated gaps/eight uncertainties across73controls.
Existing byte-mixed/up16 adjacent evidence is reused without remeasurement.

The actual unchanged0.2.1 development package/DLL comes from the
[stored checksum milestone](PNG-STORED-CHECKSUM.md). Reuse6513Windows/Linux full/live
checks, SAME actual-package/all-three-example consumers and unchanged resize ISA/
storage qualification. Zero production, test, harness, driver, native, fixture or
package builds; no broad qualification repeat.

[Evidence](resize-mixed-word-confidence-evidence.json) pins sources, producer,
reference identities and original quality/ownership gates. Raw
`resize-mixed-word-confidence.tar.xz`, SHA256
`e75de0b38d8caa30bca56b59edd6bf6394cea7a6524650ce75aba4fd8cbed5ee`,
35,700B/17members, is mirrored/member-verified. Dependencies retain the source
package, generic prepared driver, reference workers and original standalone costs.
Preserve all289required paths and postponed publication. Stop passing controls;
choose independent work from material attribution, not this uncertain point alone.
