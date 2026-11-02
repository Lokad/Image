# WebP matching storage becomes token storage

The existing empty token owner now supplies dense matching-head storage, then
transfers to greedy/main parsing after matching returns. The independent match
index and selected candidates keep separate ownership. Capacity selection uses
actual8N token bytes versus the existing1MiB table, before admission. Smaller
inputs retain their original storage path. No pool, new owner, governor, budget
snapshot, refusal fallback or cross-operation retention.

| Existing complete warm encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Admitted peak owned B | Complete packet B |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Photograph lossless / libwebp exact=1 | 104,631.18 | 91,365.41 | 1.145 / 1.156 | 15,787,102 | 12,217,052 | 160,032 / 160,032 |
| Text/diagram lossless / libwebp exact=1 | 10,791.71 | 10,311.55 | 1.047 / 1.069 | 6,939,701 | 5,055,600 | 7,234 / 7,234 |
| Transparent VP8 / FFmpeg | 73,075.33 | 58,807.56 | 1.243 / 1.250 | 12,141,728 | 4,085,883 | 98,590 / 98,590 |

Every independent confirmation retains twelve actuals, twelve native samples
before/after and both quiet guards. All three retain confirmed CPU gaps; diagram
confidence is now resolved. Libwebp exact=1 is labeled as FFmpeg's encoder backend.
Complete packets are exact. Existing reconstructed-pixel/quality, alpha/hidden
RGB, source/settings and output-ownership gates pass unchanged.

Matched direct allocation diagnostics save1,048,728 /1,048,632 /2,097,360B/op,
including array/object overhead, with the same admitted peaks above and zero
retained root storage. The same existing four-control screen also retains its
small authored control; no new performance input/settings grid. BenchmarkDotNet
B/op and direct per-thread counters include different small harness costs; both
raw figures are retained. Admitted capacity excludes borrowed input/caller stream
and is not a physical heap measurement. Native allocations remain unmeasured.

This is a storage gain, without an incremental CPU-gain claim. Versus valid
unchanged before-confirmations, raw time rises0.89% /3.84% /6.93%; native clocks
shift+5.04% /+2.53% /+7.62%. Raw diagram/alpha loss confidence separates;
normalized before/after confidence overlaps for all three. These observations
do not establish a causal regression. The slower short diagram point is retained,
not treated as a confirmed regression or sampled until favorable.

Complete public operations still include normalization, preparation, candidates,
fresh output growth/acquisition and disposal; source is borrowed/resident. Native
prepared wrappers/scratch retain their existing labels, with full per-call import,
encoding and owned packets timed. This is not newly equivalent one-shot context
timing. Startup, I/O, IPC and checks are excluded. Necessary work was not hidden.

Hash chains, sentinels, matching/parser order, integer costs, ties and packets are
unchanged. The token owner stays charged from pre-allocation admission through
actual selected-output users. Same-operation empty-capacity checks and explicit
handoff/catch/finally cleanup prevent live candidate reuse. Sparse/owned table
access and portable execution remain. Trace-only reset is renamed for reuse,
without a forwarding wrapper.

Windows/Linux each pass5,431 full/live checks and276 affected checks normal/scalar.
Eight new handoff checks cover capacity boundaries, foreign/live owners, exact
indices/token replay, refusal/cancellation cleanup and new-operation recovery.
The same actual0.2.1Release package and three examples pass both OSes. Its existing
authored consumer is enlarged once to513×259 to exercise this handoff under the
same6MiB shared root, including sync/async and independent surviving outputs.
One production/internal build perOS, no public/harness/native/selector builds;
the immutable DLL is reused through all scoring and broad qualification.

Only affected WebP encoding costs are invalidated. Three anchors refresh here;
other affected costs remain historical. Opaque VP8, all decoders, other codecs,
resizing and text/drawing reuse applicable unchanged proof. Published releases
stay immutable; publication is postponed.
[Evidence](webp-head-token-handoff-evidence.json) retains all actuals, direct
diagnostics, source/producers, package and qualification. Raw
webp-head-token-handoff.tar.xz:12,735,004B /424members, SHA256
fe5c32ee773e77e32c5cdfce4f50c79e76a53e440f02e372a9e75c5dfcbad91b,
mirrored/member-verified, with reused before-confirmations pinned to the retained
trace-forward-window archive. The subsequent [constant-bank audit](WEBP-CONSTANT-BANK-AUDIT.md) bounds
a material working-storage opportunity; its gains remain unimplemented.
