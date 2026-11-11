# Current adjacent-operation confidence

WAC805/BCC806 resolve consequential confidence questions on the actual immutable
package DLL. These are read-only measurements, **not production speedup claims**.
The one parameterized development driver reuses the authored Wpc299 methodology
and existing complete benchmark methods, fixtures, native settings and checks.
No new workload grid, production dependency or native-reference relaxation.

| Existing complete operation | Lokad µs | FFmpeg µs | Ratio / conservative upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Transparent VP8 RGBA8, exact 98,590 B packet | 52,994.15 | 53,664.11 | 0.988 / 0.998 | 6,241,792 | pass |
| Bilinear RGBA16 enlargement, 1027×519 | 1,830.73 | 1,884.92 | 0.971 / 1.013 | 4,498,558 | uncertain |

**Transparent alpha passes/stops**, range .977–.998. Native before/paired/after
means are 53.664/53.717/53.679 ms. Complete packets remain exactly 98,590 B,
SHA256 `8fcc1c935850d7813ebfce329a8b00c41fe9b02683d2be40b91dedfd74192a10`.
Input, quality 75/method 4, full source processing, hidden RGB/alpha, source-quality
sanity and independently surviving outputs remain frozen. Exact packets, rather
than equal numerical quality, establish codec parity. The lossless/alpha family
now has six passes/two uncertain; opaque lossless and word alpha stay open.

**Up16 remains uncertain**, range .884–1.013. Native bracket means are
1.885/1.907/2.050 ms; all observations remain. No pass, confirmed gap or causal
gain claim from its point ratio. Do not repeat an unchanged job for a favorable
result or tune a presumed gap. The accepted vector redesign's independently
confirmed 18.12% normalized gain remains separately valid.

Each job retains twelve alternating adjacent pairs and twelve native batches
before/after: four complete operations per alpha subject, 64 per Up16 subject.
The bound takes the worse of paired log-ratio and all-actual/diagnostic managed
confidence against the fastest before/paired/after native bracket. All twelve
managed observations remain in the all-actual view. These change adjacency from
the original global-bracket captures, not acceptance thresholds or operation
boundaries. No successful timing repeat follows either result.

Managed source normalization/preparation, complete processing, fresh independent
output and disposal remain timed. Native conversion/repacking/filtering/encoding
and output acquisition/release remain timed; FFmpeg codec or graph/input/scratch
contexts remain retained as declared. These are not equivalent fresh-context
comparisons. Existing exact packet/pixel, native RGB/alpha bounds, normal/scalar
CLI, source/owner and idle-v2 CPU0 guards pass. Full 16-bit samples remain.

Direct allocation counters give 6,241,792/4,498,558 B/op for alpha/Up16. Their
differences from BDN counters are observations, not allocation savings. Alpha's
unchanged 3,656,309 B admitted peak is reused. Up16 retains its source-based
102,892 B geometry/131,520 B row-cache capacities and 4,264,104 B independently
owned result. Operation scratch retention is zero; default pools/governed bypass
are unchanged. Physical/native/JIT peaks are not remeasured.

Reuse the source-valid **6,253 Windows/Linux full/live checks**, affected ISA
and SAME actual-package/all three examples. One offline driver build serves both
controls, with no production/embedded/harness/native/fixture rebuild or package
repeat. A receipt-prefix assertion failed after the completed alpha capture;
the saved log/receipt were recovered without remeasurement.

Actual private 0.2.1 package `f22ab4015ca38494228059978b8fa7e0899f7786ae5fec485cb7c52c9976f98f` (448,505 B),
DLL `6ef75b8185e03309b29dacbe9cf50e757e3b49336e996c60078b756970e4a272`; Linux qualified DLL
`29ca7c976cdba35357c7f1cb779936decb36cfbb5189169a5ec1263cb894cd81`. Published releases remain immutable;
publication postponed. [Evidence](warm-adjacent-confidence-evidence.json) pins
all identities, brackets, checks and capacities. Mirrored/member-verified raw
`warm-adjacent-current-confidence.tar.xz`: 1,093,116 B / 57 members,
SHA256 `580bfd3573ace40973bed1ae61c7f4365e47b4af085ea83702f5ca695b4a42fa`. It retains the parameterized source/project/binary,
frozen harnesses/statistics references and both completed captures; pinned
dependencies retain fixtures/reference tools and qualification.

[Prior global alpha scope](WEBP-HUFFMAN-WORKSPACE.md),
[vector production gain](BILINEAR-VECTOR-CERTIFICATION.md) and
[original adjacent photograph methodology](WEBP-LOSSLESS-PHOTOGRAPH-CONFIDENCE.md)
remain separate history. Photograph stays passed/stopped. All 289 required paths
remain; continue independent demonstrated gaps without a global blocker.
