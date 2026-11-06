# VP8 unshifted live reservoir

Live coefficient decoding holds the code reservoir fixed while normalization
changes the interval and buffered-bit count. Bounded 32-bit refills replace 16-bit
loads. This removes repeated code-value shifts/stores without changing model,
token/category/sign order, any per-decision cancellation poll or the published
scalar prefix. Header, short/tail and noncancelable kernels remain unchanged.
There is no new owner, heap buffer, cache, pool, session or public API.

The representation follows RFC6386 section7.2 interval arithmetic. Before a refill,
at most seven buffered bits plus eight interval bits precede a32-bit append: the
reservoir fits47bits. Each normalization consumes at most seven bits. The unchanged
272-byte block admission covers at most266consumed bytes plus four-byte lookahead
and slack. Finally publication derives the same scalar cursor/value without input
reads or cancellation polls, including when a decision's poll throws.

| Complete513×259opaque decode, live token | Elapsed µs | Managed B/op |
| --- | ---: | ---: |
| Contemporary original,12actuals | 6,053.96 | 760,564 |
| Current, independent12actuals | 5,918.00 | 760,564 |
| Current FFmpeg, faster native bracket | 5,099.16 | Unmeasured |

Independent complete decoding saves **135.96µs/2.25%raw**; both filtered/
all-actual raw confidence views separate. Normalized point improves2.57%, but its
interval.951398–1.005474 overlaps1: no confirmed normalized gain, equivalence or
FFmpeg parity. Native point changes+0.331%. Currentratio1.161/upper1.173
remains a gap; all six reader controls remain gaps. Five unchanged controls reuse
their existing evidence. Old/new standalone ratios are not causal gain evidence.

The short against the previous unchanged reader has raw+2.22%/normalized−4.16%
points, with wide intervals and native+6.65%. That clock movement justifies the
contemporary original, followed by one independent candidate confirmation.
Every point and original bracket is retained. No unchanged favorable repeat.

Actual standalone operations retain the frozen72,380Binput and exact531,468Bframe,
source preservation, prepared caller cancellation source and independently owned
output/disposal. Complete parse/reconstruction/filter/color work stays timed.
Native codec/conversion contexts remain retained as explicitly labeled; this is
not one-shot setup parity. Setup/JIT/validation are excluded equally. No new image,
sampling, quality or shape grid, and no relaxed native/managed pixel gate.

One bounded current two-second capture supplies1,968samples:867token/576coefficient.
The earlier hot model-delivery code changed, while six other source identities
reuse prior attribution. Hot normalization/refill stores motivate this representation;
samples/skid are locations, not phase times or predicted savings. Instrumented
elapsed is discarded. Scoped source checks preserve the coefficient loop, model
selection, token and category bodies. This differs from failed generic ref-state
sharing; those adverse experiments remain linked history, not a family ban.

Total allocation stays760,564B/op. Additional owned heap capacity and retained
operation scratch are zero; reader state stays within the existing8KiBcoarse
allowance. Inputs/root are borrowed and actual users finish before release. No
physical/admitted whole-operation peak or native allocation is newly measured;
these remain distinct from GC totals and retained capacity.

Windows/Linux each pass **5,741full/live checks** (2,488internal/2,305public/948
differential), plus39internal/61public per normal/scalar mode. Existing4,096token
cases cover all12symbols/cursor/following reads;1,024live block contexts,
305cancellation ordinals, maximum-category273Bpartitions across eight phases and
164frozen native frame controls preserve exactness and cleanup. ISA kernels are
unchanged, so prior vector-path disabled-ISA proof is reused. One immutable core/
embedded revision perOS is reused throughout; no public/harness/native/selector/
fixture build or additional numerical policy.

The SAME private0.2.1Releasepackage/allthreeexamples passes bothOSes. Its existing
consumer decodes the frozen full packet through live sync/async APIs, compares the
ordinary reader and retains separately admitted results after input mutation.
Package `10a7fdc8d96e59b1a7d57653e466935df6c29aea62c436cfb4e3a4c6c9770715` is418,462B.
Published releases stay immutable. All125displayed numerical comparisons reuse
unchanged paths; this named live control and summary update are not a full actual-
package performance refresh or completion of the282-path objective. Publication
remains separate and postponed.

[Evidence](vp8-unshifted-live-reservoir-evidence.json) preserves sources, original/
candidate points, current status, unchanged-path reuse and actual-package proof.
Raw `vp8-unshifted-live-reservoir-qualified.tar.xz`: 3,645,176B/138members, SHA256
`5a773cc39f02710dc5dc27e744d60f7a77babdce2365b9b8bcb0f4614189fe03`, mirrored/member-verified. Pinned inputs retain reproducibility
without duplicating unchanged payloads. [Model delivery](VP8-LIVE-MODEL-PREPARATION.md)
and [earlier token state](VP8-PREPARED-TOKEN-STATE.md) retain their scoped evidence.

Stop this round. Next resolve the historical bilinear enlargement boundary before
assuming its older substantial gap still needs optimization; no reader micro-tuning.
