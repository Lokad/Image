# Current resident RGBA PNG decode costs

Two existing controls now have independent current-build evidence. No production
change or causal comparison against older producers is made.

| Complete decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| RGBA8, 513×259 | 1,574.391 | 1.131 / 1.166 | 4,472 | Confirmed gap |
| RGBA16, 513×259 | 2,891.613 | 0.999 / 1.006 | 8,568 | Confidence open |

All 12 actuals per case, native brackets, filtered/unfiltered bounds and both
idle-v2 guards are retained. RGBA16 has 10 filtered samples; all 12 raw samples
remain in its conservative bound. Its mean below parity is insufficient for a
pass. Do not pool, retime or assume a need to optimize it while larger gaps remain.

Both subjects perform complete decoding, normalization, independent output
ownership and release from the frozen original packets, including native word
conversion. Exact pixels, alpha/hidden RGB, source and output independence pass.
No new images or cases. Allocations are managed B/op; native allocations unmeasured.

The qualified PGRR1 build, harness, fixtures and native worker are reused. One
generic typed-int selector replaces repeated per-case selector builds; default
16-call unrolling, one full operation per invocation, 12 actuals, three warmups
and 200 ms stay fixed. The initial tooling compile failure is retained and was
corrected before any timing. Zero core/harness/native builds; unchanged full
5,082 tests per OS and SAME actual package/three examples remain valid.
49 affected costs remain unrefreshed; these observations do not complete parity.

[Evidence](current-png-rgba-decode-costs-evidence.json). Mirrored/member-verified
archive current-png-rgba8-rgba16-costs.tar.xz, 49 members,
SHA256 153ace08a59e8cba23bee3ea30f3cdde2ad4067f51713f74abfca2fca6c9c5a7. Its qualified-build dependency is pinned by hash.
