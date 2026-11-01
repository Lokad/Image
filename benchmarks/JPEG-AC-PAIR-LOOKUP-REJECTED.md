# Rejected JPEG AC-pair lookup

The existing 513×259 direct-RGB444 JPEG decode becomes slower with an eight-bit
lookup decoding two complete AC symbols/amplitudes together. Reject the candidate;
current accepted production and benchmark costs remain unchanged.

| Complete decode | Lokad µs | Ratio / upper | Managed B/op |
|---|---:|---:|---:|
| Qualified original, independent12 | 2,098.407 | 1.449 / 1.458 | 966,367 |
| Rejected active candidate, short3 | 2,283.937 | 1.575 / 1.670 | 968,495 |

Elapsed cost rises **8.84% filtered /
8.77% all-actual**, normalized cost
**8.66%**, and allocation **2,128 B/op**.
No independent confirmation or width/table/layout/inlining variants follow.

One two-second current actual-IP diagnostic finds 38.65% fused-baseline and 7.74%
fallback AC samples, versus 17.14% inverse AVX2, among 1,925 samples. The independent
count-only parser finds 68,396 eligible pairs among 226,740 AC symbols: a potential
30.16% fewer iterations. These diagnostics are not component times or predicted
gains; the complete operation is decisive. Packing is already vectorized.

The frozen 133,057-byte packet, settings, exact normal/scalar/native pixels,
source, independent output ownership and both idle-v2 guards pass. Full decode,
normalization, fresh output acquisition and release remain timed on one CPU.
Native allocation is unmeasured; managed B/op includes harness costs.

Preparation exposed two mistakes: linked benchmark dependencies loaded the original
DLL, then an overly restrictive construction guard left pairs inactive when DHT
preceded SOF. Their three-actual executions are retained but do not judge the
algorithm. Physical dependencies and scan-time preparation correct them; only the
active candidate above is its screen. Two Linux core builds and one internal-test
build; no Windows, public-test, harness, native or selector build. No new workload.

The active candidate passes 49 internal/343 unaffected public checks normal/scalar
on Linux. Its additional admitted table capacity invalidates 27 old exact byte-peak
or reservation-ordinal tests; none is waived or declared qualified. The rejected
candidate receives no full/package qualification and those tests are unchanged.
All sources/core/PDB/XML/linked test assemblies restore byte-exactly. Restored
47 internal/**all 388 public** checks pass normal/scalar, including admission and
cancellation. Two tests dedicated to the removed implementation are removed too.
Windows production/package are unchanged. Applicable full Windows/Linux and same
actual Release-package/three-example proof are reused, without another full run.

[Evidence](jpeg-ac-pair-lookup-rejected-evidence.json) retains the decision,
samples, costs, build corrections, failures and restoration. Raw archive
`jpeg-ac-pair-lookup-rejected.tar.xz` is mirrored/member-verified: 1,887,512 B / 144
members, SHA256 `0a95d3e7bd6cecba8d712482ddcc2fbd1bd06f6b2851c35441ffd6ee82786997`. J4N31 and DRGB31 dependencies remain retained.
The full 268-path objective stays open; close this primitive.
