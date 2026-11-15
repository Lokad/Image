# Inspection acquisition and admission ownership

[Direct result publication](PROBE-RESULT-PUBLICATION.md) supersedes the five costs
and probe allocations below. This report retains the ownership stage and its effects.

One private reader now owns acquisition and the existing admission scope.
Four probes allocate **608 instead of632 B/op**; complete PNG headers allocate
**568 instead of592 B/op**. All five current comparisons use the same immutable
timed/package DLL. Four small probe gaps remain; complete PNG headers pass.

| Complete original operation / FFmpeg AVIO + bounded header adapter | Lokad.Image µs | Reference µs | Ratio / upper | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| ProbeDimensionsPng | 0.128 | 0.123 | 1.036 / 1.066 | 608 | gap |
| ProbeDimensionsWebp | 0.126 | 0.122 | 1.038 / 1.066 | 608 | gap |
| ProbeDimensionsJpeg | 0.255 | 0.195 | 1.310 / 1.351 | 608 | gap |
| ProbeDimensionsJpegAsync | 0.287 | 0.206 | 1.393 / 1.433 | 608 | gap |
| PngHeaders | 0.301 | 0.574 | 0.525 / 0.532 | 568 | pass |

Original encoded bytes, fresh resident source adapters, parsing/admission,
independent returned descriptors and source/scratch disposal remain timed.
Probes stop at required prefixes; headers include framing/EXIF/EOF without
CRC/pixel validation. Async uses immediate callbacks; held-user ownership is
tested separately. The labeled adapter is not the FFmpeg codec parser.

Controlled twelve-pair complete JPEG sync/async/header comparisons confirm
**3.17% /3.45% /3.88%** scoped gains, **24 B/op** lower allocation. These are
managed DLL effects, not FFmpeg parity claims. Initial implementation was
adverse (ratio1.07350); the revised implementation was inconclusive (upper1.00505).
Both remain. The helper originally warmed32probe calls before262144-call
batches. One explicit tooling correction warms a full untimed batch per producer.
All twelve samples, natural GC, alternating order and original confidence rules
remain; no sample is excluded and no identical repeat is promoted.

The reader inherits the existing nonvirtual OperationContext, removing one
object/reference. Construction catches reserve and allocation failures; disposal
refunds actual workspace and finishes only its owned scope. A null-budget context
cannot acquire a shared scope, so its successful disposal skips an empty exchange.
Interface/idempotent cleanup, terminal refusal, callback failure, recovery and
public validation precedence have focused tests. Caller root/input remain borrowed.
Every acquisition/marker/skip/promotion body and cancellation checkpoint is exact.
Unrelated context methods/constructors are byte-for-byte source identical; no
virtual member or unrelated call binding is introduced by removing the sealed flag.

Admission remains **512 B initial /8448 B bulk maximum**, including existing
conservative header capacity. Zero retained operation workspace or new idle cache.
Allocation is distinct from admitted capacity and unmeasured physical/native/JIT
peaks. The24B reduction is not a measured peak-memory reduction.

**Windows/Linux:6712full/live each** (3273internal/2491public/948live), plus
focused10internal/148public normal/scalar and the **same actual Release package /
all three examples**. Existing pending-read/skip cancellation, failure cleanup and
surviving-output consumers pass. Affected performance reconciles before broad
qualification. Initial/revised Linux builds are retained; accepted Windows builds
once. The warmup correction builds only its helper; no post-timing production
rebuild. Unchanged codec/edit/resize/text and arithmetic/ISA evidence is reused.

Timed DLL `1632a4ae4479cf040f5cb0545edf05d7d2ca1f7773c53a721ee956b0096e9e19`; private package `4ed902ae789535949f55c219c1623120637dd175d2c6c61b7bb70afa227f10c6`,
462,932 B. [Evidence](inspection-owner-evidence.json) and raw
`inspection-owner-qualified.tar.xz` (30,223,784 B/673 members), SHA256
`7d910fc9bfeb5e960c544fcb4af496061cb1869e5ede253fe5a52635587f919c`, mirrored and member-verified, retain all sources, original and
corrected helpers, adverse/inconclusive samples, guards and package proofs.
Earlier [probe](PROBE-PREPARATION.md), [JPEG marker](JPEG-MARKER-PREPARATION.md)
and [skip](NONSEEKABLE-SKIP-PREPARATION.md) costs are superseded here; their
raw evidence remains history. Publication is postponed; all289paths remain required.
