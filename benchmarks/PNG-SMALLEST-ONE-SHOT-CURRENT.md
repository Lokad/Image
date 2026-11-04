# Current PNG SmallestSize one-shot costs

These two existing 513×259 level-9/filter-None/RGBA controls now have current
complete-operation costs. Both sides acquire and dispose fresh encoder, zlib and
packet state and return independently owned output. The native word operation
also allocates, converts and releases its endian frame inside timing. Resident
source input is borrowed. No necessary managed preparation is excluded.

| Existing control | Lokad.Image µs | FFmpeg µs | Ratio / upper | Conservative lower | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| RGBA16 | 23,558.06 | 20,869.52 | 1.129 / 1.137 | 1.112 | 3,420,454 | 963,202 |
| RGBA8 | 18,221.17 | 17,251.11 | 1.056 / 1.065 | 1.049 | 932,516 | 210,973 |

Both conservative lower bounds exceed one: these are **confirmed gaps**, not
passing cases. This is current-cost reconciliation, with no production change
or incremental speedup claim. Historical retained-context measurements keep their
original boundary and remain linked history; they are not equivalent one-shot
comparisons. This is not a full release refresh.

Each control ran one short screen followed by one independent twelve-actual
confirmation, sequentially on one CPU of the dedicated quiet machine. Word/byte
confirmations batch eight/four complete operations per actual. Both filtered and
all-actual statistics, native brackets and before/after process guards are
retained. Small B/op differences versus earlier rows or the short screen reflect
different harness amortization; they do not establish allocation savings.

Full frozen packets, all 8/16-bit samples, alpha, hidden RGB, source immutability,
independent surviving outputs and caller stream ownership pass. Four native
normal/scalar preflight encodes preserve exact packets; four wrong-operation/depth
checks reject before readiness. Two scoped level-9 projections reuse the original
adapter in ignored storage. Existing source, builders and level-6/1 workers remain
byte-for-byte unchanged. The one-shot word `nativeEndianWorkspaceReused` field now
correctly says false; existing authoritative readiness/boundary fields and actual
level-6/1 lifetimes and costs were already correct. Old raw evidence is untouched.

One isolated Windows benchmark-harness build and two tiny native worker builds;
zero production, Linux harness, probe, selector or package builds. A global
intermediate-path attempt failed metadata resolution before compilation; the
corrected isolated harness directly references the immutable qualified core.
Original selectors are reused. Production remains the qualified
[sequential-hash build](PNG-SEQUENTIAL-DICTIONARY-HASH.md): its 5,549 full/live
checks per OS and same actual package/all three examples remain applicable,
supplemented by the separately qualified
[five reader cases](JPEG-REFINEMENT-FRONTIER-INCONCLUSIVE.md). No new full-suite or
package run is claimed or needed for reference-tooling-only changes.

Deflate admitted workspace remains 384 KiB; retained operation scratch is zero.
No new complete admitted/physical peak or native allocation measurement is claimed.
Only these two previously historical costs are refreshed. Other affected PNG
costs remain historical; unchanged-path confirmations remain valid. The full
282-path objective stays open, and publication is postponed.

[Evidence](png-smallest-one-shot-current-evidence.json) records exact producer,
harness, reference/source identities, packets, actuals and boundary checks.
Mirrored/member-verified raw `png-smallest-one-shot-current.tar.xz`: 6,450,112 B / 141 members,
SHA256 `d27fbdcfa1c79cab6d9a87549e41595d8829298936e1c59c390543ee3b1745a7`. It includes the isolated harness project,
frozen inputs, generated/native adapters, outputs, failed attempts, guards and
reproduction scripts. Qualified core/package and unchanged runtime archives are
pinned dependencies. Earlier [one-shot boundary](PNG-WORD-ONE-SHOT-BOUNDARY.md)
and [lazy-state work](PNG-LOCAL-EXECUTION-STATE.md) remain history.
