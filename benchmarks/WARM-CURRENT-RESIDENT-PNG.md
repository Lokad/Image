# Current resident PNG decode

Two existing default PNG resident cases now have quiet short evidence on the
accepted current producer. Both remain measured parity gaps, so no longer
confirmation is run. This is current-code reconciliation, with no production change.

| Existing complete operation | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Decode8 | 1,575.30 | 1,388.57 | 1.134 / 1.330 | 4,480 |
| Decode16 | 3,015.32 | 2,901.23 | 1.039 / 1.064 | 8,576 |

Original 513×259 RGBA8/16 packets, None prediction, exact pixel/precision/alpha/
hidden-RGB hashes, normal/scalar FFmpeg CLI controls and independent live outputs
are unchanged. Each timed call decodes the resident packet through the public
API, including local limits, inflation, reconstruction, owned result and release.
FFmpeg includes the complete software decode, required expansion, word-endian
conversion and independent output acquisition/release. Native allocation totals
are unmeasured. Neither path caches decoded pixels.

The verified core, current harness, native worker and compiled selectors are reused.
The word selector is extracted from retained evidence; no production, harness,
selector, native or package build. Current setup and complete managed/native frames
pass before scoring. A setup-only host base-directory error is corrected before
timing; its failed log is retained. Two sequential shorts retain all three actuals,
native 3+3 brackets, all four official guards and selected/all-actual confidence
bounds. No new image, filter/depth grid or allocation delivery advantage.

The accepted 4,484 tests/OS, affected hardware-off checks and same actual private
0.2.0 package plus all three consumer examples on both OSes remain applicable.
Published 0.1.0 stays immutable. Only these two historic rows become current:
40 current/198 historical, 125 independent/113 short observations. The finite
238 numeric+16 unmeasured scope and 72 passes/165 mean gaps/one confidence-open
counts are unchanged. Functional, memory and performance gates stay separate.

[Evidence](warm-current-resident-png-evidence.json) retains samples, allocations,
identities, boundaries and the mirrored raw archive hash. A first retention attempt
stopped on an absent optional XML file before writing archive data; removing that
uncited empty file and omitting XML preserved every result without repeating jobs.
Next: exact paired signed16 byte Lanczos arithmetic feasibility before coding.
Uniform warm parity and replacement readiness remain open.
