# JPEG probes acquire marker pairs

The existing complete async JPEG dimension probe independently confirms
**6.73% less filtered elapsed**, **6.48% across all actuals** and **6.73% normalized**,
with separately separated raw and normalized confidence envelopes. The fresh
three-actual pair gives10.6%; its stable original reuses applicable original12.
Allocation stays744B/op and dimension workspace stays512admitted bytes.

| Existing complete operation | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Seekable sync | 0.354809 | 1.842 / 1.895 | 744 |
| Nonseekable completed-async | 0.396830 | 1.976 / 2.017 | 744 |

These remain gaps against warm single-CPU upper≤1x. The synchronous companion
supplies a current cost, without a paired causal gain claim. Reference is the
unchanged original bounded JPEG adapter using FFmpeg AVIO and owned descriptors,
with the same original513×259packet, settings, acquisition and release boundaries.
All actuals, filtered/unfiltered bounds, native brackets and idle-v2 guards remain.

An empty ordinary-marker buffer now acquires FF/code as a natural pair. Existing
FillAsync handles partial/pending reads, counts, input/acquisition caps and
cancellation; fill continuation and entropy grammar stay unchanged. If fewer
than two input bytes remain, request one so an invalid prefix still fails as data
before the input-limit sentinel. No storage, owner, scope, cache or fallback added.

Non-timed public traces show15→11reads/33acquired bytes/3seeks for seekable input,
18→14reads/308bytes/0seeks forward-only; both traverse308bytes and stop at the
same valid prefix. Sync/async descriptors, source and surviving ownership agree.
Eight original bound/error traces agree. Three focused tests cover early caps,
invalid prefix classification, partial/pending/mixed reads, cleanup and recovery.
Existing cancellation, terminal admission refusal and full-header checks pass.

Both OSes pass **5,240 Release checks each** (2,111 internal,2,181 public,948live)
and104focused cases normal/scalar. The SAME actual Release package passes both
OSes, including governed JPEG prefix/error checks, existing async/retained-result
controls and all three usage examples. One successful core build/OS; no native,
worker or selector build, variant, profile or successful timing repeat.

[Evidence](warm-jpeg-probe-marker-pairs-evidence.json) pins all producer/package
identities and mirrored/member-verified `jpeg-probe-marker-pairs-qualified.tar.xz`,
SHA256 48efc8a3b9bea1b8ef590c00baf02b75ae93b9af4e0ab8ea3c4cc27920e326c9. Other codecs, JPEG resident Identify,
pixel validation/decode/encode and editing/text retain applicable proof. Complete
or other-fact JPEG inspection has behavioral coverage; these timings make no
broader CPU claim. All268required paths remain in scope.
