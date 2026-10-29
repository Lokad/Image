# Current 8-bit PNG decode costs

All four existing controls independently pass the warm one-CPU parity target.
Stop them unchanged. These are current costs, without a historical speedup claim.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| RGB8 → RGBA8 | 1,091.946 | 0.717 / 0.721 | 3,448 |
| Gray-alpha8 → RGBA8 | 757.018 | 0.650 / 0.655 | 2,424 |
| Gray8 → RGBA8 | 333.291 | 0.908 / 0.916 | 1,400 |
| Stored RGBA8 | 100.559 | 0.735 / 0.748 | 4,472 |

All12actuals/case, native brackets, filtered/unfiltered bounds and both idle-v2
guards are retained. Complete decode/normalization/independentoutput/disposal and
frozen packets/sourceprojections/settings remain. ExactRGBA/alpha/hiddenRGB and
source/output lifetimes pass. Allocations are managedB/op; native unmeasured.

Zero builds, newimages/cases/settings or production changes. Reuse immutablePAI1
core/selector/harness/native worker, full5094tests/OS and SAMEactualpackage/three
examples. A stored8postcheck found only a provenance-description update from
BCLNoCompression toStoredNoCompression. The frozen packet/hash and reference
operations/settings were unchanged. Both quietguards and all12actuals were
already complete; their logs were recovered and verified without retiming.

33existingcosts remain unrefreshed; the full268pathobjective stays active.
[Evidence](current-png-byte-decode-costs-evidence.json). Mirrored/member-verified
archive current-png-byte-costs.tar.xz, 58members,
SHA256 ad0141493e6642fd5d825bdab353e18d05abb80c50725712f4509b0316435bb4. Qualified-build dependency pinned by hash.
