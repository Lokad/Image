# Architectural preparation and storage checkpoint

WP498 reconciles existing boundaries and records three current admission traces
using the qualified immutable DLL. No production build, performance measurement
or numerical-policy change occurs. The old 20–30 MB WebP allocation observations
are historical; current complete encodes already use much less storage.

| Frozen complete WebP encode | Current allocated B/op¹ | New admitted peak B² | Exact packet B |
| --- | ---: | ---: | ---: |
| Lossless photograph | 6,517,158 | 3,470,059 | 160,032 |
| Lossless text/diagram | 4,363,930 | 3,251,328 | 7,234 |
| Lossy RGBA8 with lossless alpha | 7,034,400 | 3,656,309 | 98,590 |

¹ Reused current lossless confirmations from [isolated prediction](WEBP-ISOLATED-GREEN-PREDICTION.md)
and the alpha control from [block preparation](VP8-SMALL-BLOCK-PREPARATION.md).
² Separate caller-side shared-budget callbacks record ordered coarse ownership
charges. Borrowed inputs and caller stream capacity are excluded. Callback work
perturbs execution; these are neither physical heap peaks nor CPU measurements.
All packets/source checks pass, the borrowed root survives and charges return to
zero. Surviving caller-owned output capacity is recorded separately.

Matching heads already borrow token capacity; rejected parsing storage serves
tracing, and sparse populations recycle merged clusters. Alpha trials retain a
match index and transfer retired token capacity; working samples later transfer
to coefficient storage. Selected tokens/maps/codes remain independent. The
[fused counting/addition stage](VP8L-FUSED-PREPARED-COUNTING.md) already supersedes
earlier rejected implementations; their thresholds and closed-family rules do not
govern further work. Native libwebp's block recycling remains insight, not code.

The next bounded candidate is the two temporary sparse-population row indexes:
134,744 admitted bytes per photograph/diagram, and 35,288 per alpha trial. Their
users finish after population preparation. Borrowing unused selected-token tail
capacity could avoid these arrays while preserving the live prefix and its full
charge. Eligibility by actual capacity, allocation benefit and complete-operation
cost remain unmeasured. No full-operation peak saving is predicted. Insufficient
capacity must select the existing allocation before admission, with no refusal
fallback. Two simultaneously live parsing candidates cannot simply share storage.

The subsequent [matching-index handoff](WEBP-MATCHING-POPULATION-STORAGE.md)
qualifies a revised design: token-tail capacity misses the photograph, so the
ordinary matching index lends storage after its final reader. Current allocation,
peak and timing evidence belongs to that report; this audit remains attribution.

Preparation boundaries remain explicit:

- Lanczos compares both complete one-shot operations and separately prepared
  execution with owned plans/workspace. Native graph/input/scratch and managed
  axes/rows stay retained only in the prepared boundary. Current costs are owned
  by [certificate preparation](LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md); initial
  asymmetric measurements remain history. Exact managed Q30/rational results and
  certified repair differ from native float and remain required.
- Current fresh-context PNG controls include codec/zlib/working-state preparation,
  complete output and disposal on both sides, including native word conversion.
  Older retained-context rows keep their boundary label. Initial 12–26 µs Deflate
  construction observations are historical after subsequent parser changes;
  they do not establish a current session benefit. No additional session is added.
- WebP normalization, candidate preparation/selection, fresh output growth and
  ownership/disposal remain inside complete one-shot timing. Explicitly prepared
  text/drawing references remain independent workloads, with their approved gates.

[Evidence](architectural-preparation-storage-audit-evidence.json) pins sources,
producers, all 2,102 ordered events, exact packets, guards and reusable qualification.
It retains the audit tool and existing fixture/reference receipts. No full/package
qualification is repeated for unchanged production. The raw archive identity and
qualified-build dependencies are recorded there. This audit adds no benchmark
renderer dependency or displayed timing claim. Publication remains postponed.

## Compact wider bicubic follow-up

CDG663 compares existing complete public Resize modes on the same qualified
package DLL, using one original RGBA8 4097×2051→1024×512 raster. Preparation,
workspace, independent result ownership and disposal are timed. This is a
managed-only generalization screen, with no native comparison or parity claim.

| Existing mode | Elapsed ms | Allocated B/op | Admitted workspace B |
| --- | ---: | ---: | ---: |
| Default cached execution | 26.159 | 1,585,375 | 1,585,440 |
| Compact execution, input + output + 4096 working limit | 643.414 | 174 | 4,096 |

Both retain a separately owned 2,097,152-byte result until disposal and no scratch
afterward. Governed peaks are 3,682,592 / 2,101,248 bytes; these exclude borrowed
input and differ from physical heap or idle pool capacity, which are unmeasured.
Three actuals per mode have separated intervals. Normal/default/compact and
scalar full-frame pixels agree exactly; source and independent ownership pass.
One diagnostic build reuses the immutable library; no production, native,
correctness-suite or package rebuild occurs.

The 24.60× slowdown rejects promoting this compact implementation globally from
its small-control allocation advantage. A bounded profile finds 910 samples in
general Resize and 302 in Kernel, versus 27 in the wide strip, out of 1,371 total.
Together with the source's certificate-decline boundary, this identifies full-frame
fall-through as the material lead; it does not establish cache/stride causality or
identify the first uncertain pixel. Samples are not phase times. The completed
capture was recovered after collector termination, without replay; its after-guard
is delayed. The separate statistical screen has both immediate guards.

The next design reuses existing original-order byte repair for uncertain wide
pixels, retaining tiny-alpha workspace retirement before rational replay. Default
dispatch stays unchanged. This is a revised hypothesis, without merge authority
or weaker numerical policy. [Evidence](compact-bicubic-boundary-evidence.json)
pins all actuals, identities, ownership, the profile and reproduction. Mirrored,
member-verified raw archive `compact-bicubic-boundary-attribution.tar.xz` has
10,982,340 bytes / 40 members, SHA256
`994a791ebbea29d5aad67acc0c00d70900477782eb7b2b929ab533249b6fd411`.
It adds no renderer dependency or advertised workload.

The subsequent [shared byte repair](BICUBIC-SHARED-BYTE-REPAIR.md) qualifies this
revised design: large compact execution now30.78 ms, with exact pixels and the
same admission. It also records a small-wide regression; default selection stays
unchanged. The original adverse screen above remains retained attribution.

## Current WebP lifetime follow-up

WAL672 reuses the prepared audit tool with the qualified TRP671 package DLL.
Three complete photograph/diagram/transparent-alpha encodes preserve exact
160,032 / 7,234 / 98,590-byte packets and source/root cleanup. All 2,094 admission
events are retained; admitted peaks remain 3,470,059 / 3,251,328 / 3,656,309 B.
No production/harness build, performance measurement or GC-allocation claim occurs.

The two largest token buffers overlap through independent candidate scoring;
the rejected buffer later serves tracing. Their apparent duplication is not free
scratch. The matching index already serves population row preparation afterward.
Those row readers finish before clustering allocates its slots, original/current
statistics and pair queue. For the frozen photo/diagram, their aligned typed
capacity needs 470,120 B, fitting the still-charged 531,468-byte matching index.
The existing separate coarse clustering reservation is 530,688 B. Two alpha
states reserve 30,912 B each; their actual reuse eligibility remains to be checked.

The next bounded design uses stack-confined execution views over this dead index,
with the allocated path selected by physical capacity before admission. Required
default statistics must be cleared; live tokens and surviving histogram owners
must remain independent. A ref-struct transition may change JIT costs, so neither
allocation nor CPU benefits are accepted before complete-operation checks.
Repeated Huffman construction is a later lead: its direct positive scratch
admission volumes total 844,208 / 126,648 / 981,214 B across the three controls.
Admission volume is neither total allocation nor peak live capacity.

[Evidence](webp-current-lifetime-audit-evidence.json) records the bounded lifetime
map, source pins, current ownership observations and next hypothesis. Mirrored,
member-verified raw `webp-current-lifetime-audit.tar.xz` is 82,424 B / 29 members,
SHA256 `6c39b732294b0e04d38605f0ae5273b2c4df86cfee386fd51c08ec9583ea6377`.
Unchanged full Windows/Linux and actual-package qualification is reused. No
displayed benchmark cost or renderer dependency is added.


The subsequent [Huffman workspace stage](WEBP-HUFFMAN-WORKSPACE.md) completes
operation-owned alphabet/group reuse without changing the queue or tree policy.
Its current allocation, peak, uncertainty and actual-package evidence supersede
the earlier storage lead; no cross-operation pool or extra session is introduced.
