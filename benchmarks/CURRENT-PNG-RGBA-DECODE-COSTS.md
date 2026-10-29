# Current resident RGBA PNG decode costs

Average rows are superseded by [SIMD reconstruction](WARM-PNG-AVERAGE-SIMD.md).
None/Paeth paths remain unchanged and retain the scoped evidence below.

Six existing 513×259 controls have independent evidence on the qualified PGRR1
build. No production change or causal comparison against older producers is made.

| Complete decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| None RGBA8 | 1,574.391 | 1.131 / 1.166 | 4,472 | Gap |
| None RGBA16 | 2,891.613 | 0.999 / 1.006 | 8,568 | Confidence open |
| Paeth RGBA8 | 3,040.484 | 1.014 / 1.031 | 4,472 | Confidence open |
| Paeth RGBA16 | 5,173.711 | 0.907 / 0.962 | 8,568 | Pass; stop |
| Average RGBA8 | 2,112.275 | 1.143 / 1.154 | 4,472 | Prior; superseded |
| Average RGBA16 | 5,282.373 | 1.180 / 1.189 | 8,568 | Prior; superseded |

All 12 actuals per case, native brackets, filtered/unfiltered bounds and both
idle-v2 guards are retained. RGBA16 None and Paeth8 remain confidence-open; their
conservative intervals cross parity. Do not pool, retime or assume a need to
optimize them while larger gaps remain. Paeth16 passes and stops.

Both subjects perform complete decoding, normalization, independent output
ownership and release from the frozen original packets, including native word
conversion. Exact pixels, alpha/hidden RGB, source and output independence pass.
No new images or cases. Allocations are managed B/op; native allocations unmeasured.

The qualified PGRR1 build, harness, fixtures and native worker are reused. One
generic typed-int selector serves all six controls: default 16-call unrolling,
one full operation per invocation, 12 actuals, three warmups and 200 ms. Its
initial tooling compile failure was retained and corrected before any timing.
PNP1's four extra cases require zero builds. Average16's earlier native record is
explicitly reconciled to the already-approved current worker: unchanged RGBA16
decode/conversion/ownership, static FFmpeg archives and flags; additions for other
formats and diagnostics do not alter its settings or workload.

Unchanged full 5,082 tests per OS and SAME actual package/three examples remain
valid. 45 affected costs remain unrefreshed. Average8/16 demonstrate gaps and
their scalar reconstruction supports one bounded SIMD trial; no broad parity claim.

[Evidence](current-png-rgba-decode-costs-evidence.json). Mirrored/member-verified
archives: current-png-rgba8-rgba16-costs.tar.xz, 49 members,
SHA256 153ace08a59e8cba23bee3ea30f3cdde2ad4067f51713f74abfca2fca6c9c5a7;
current-png-paeth-average-costs.tar.xz, 58 members, SHA256 a86a7b56383ed1d3449416581b61a5e6be3258cdd45835dbcd698809a567612e.
Qualified-build dependencies are pinned by hash.
