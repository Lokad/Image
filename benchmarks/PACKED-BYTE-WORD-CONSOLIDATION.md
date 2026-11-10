# Shared packed byte/word bicubic execution

PRV733 removes the separate word scheduler and extends the existing decoded-prefix
range to both depths. Word products are converted from unsigned 32-bit values to
exact doubles once per decoded source pixel, then reused across four output
columns. Ordered horizontal/vertical accumulation, certificates, scalar fallbacks
and original-order repair remain. Byte/word publishers are preserved verbatim.

| Complete public resize, original 513×259 → 64×32 | Managed µs | Native µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8, input + output + 4 KiB limit | 485.70 | 367.42 | 1.322 / 1.329 | 113 |
| RGBA16, input + output + 4 KiB limit | 479.19 | 387.34 | 1.237 / 1.262 | 113 |

Quiet VM, single CPU, twelve actuals each, filtered and all-actual conservative
views. Managed preparation, working storage, fresh independently owned output and
disposal stay timed. Native zimg retains its graph/scratch; source halo, conversion,
quantization, repack and output ownership stay timed. This does not compare fresh
native graph construction. Exact managed rational/frozen frames and original
native RGB/alpha max 1 / RMSE ≤0.25, source, ownership and idle-v2 checks pass.

Word improves from the [current original confirmation](CURRENT-PACKED-WORD-RESIZE.md)
522.34 to 479.19 µs: **43.16 µs / 8.26% raw / 8.09% normalized**, with separated raw and
normalized intervals in both confidence views; native −0.189%. Both controls remain
gaps. Earlier byte 485.40 / native 370.61 µs provides source-valid context, not an
equivalence, causal gain or nonregression claim. No passing family was retuned.

One 4,096-byte admission precedes stack storage. Sequential prefix/byte-suffix/
word-suffix payloads are 3,880/3,992/4,044 bytes. Thirty-two four-vector slots borrow
unpublished result tails; all users retire before tail publication or precision
replay. Returned 8,192/16,384-byte images remain independently charged until
disposal, retained operation scratch 0. Default output-pool retention is separate;
governed calls bypass it. Total GC 113 B/op is not a physical/admitted/native peak.
No new pool, session, cache API or owned capacity is introduced.

The first shared implementation measured 1,160.25 / native 387.40 µs in its short
screen. One bounded actual-code/profile capture identified repeated span-to-
readonly-view calls inside tap loops. The revision creates borrowed readonly
views once per range/decoded row. All adverse samples and instrumented capture
are retained; instrumented elapsed is discarded and sample counts are not phase
costs. This records a coherent transition, not favorable resampling of unchanged
code. The earlier [geometry-only trial](PACKED-WORD-GEOMETRY-EXPERIMENT.md) remains
unselected history.

All **6,147 full/live checks per OS**, 311 internal/729 public resize checks per
normal/scalar/AVX2-off/AVX512-off, and the SAME actual Release package/all three
examples pass Windows/Linux. Fourteen added controls cover frozen pixels, result
tail layouts, unsigned high products, tiny alpha, shared admission, retained
survivors, terminal refusal, cancellation and recovery. Actual-package consumer
now verifies both packed depths. Unchanged JPEG 453/375 per-mode qualification and
unaffected costs are reused. Two successful main/embedded candidates per OS; no
harness/native/selector/fixture builds or additional favorable repeats. Published
packages remain immutable; publication postponed. This is scoped development
qualification, not a full 127-row release refresh or 283-path completion.

[Evidence](packed-byte-word-consolidation-evidence.json) pins sources, modules,
every actual, guards, reference receipt identities and proofs. Actual private
package SHA256 `9a1187626603c2b6d8cd25b548cd89c2aed08f05ffc52b07c8e2c8403b9b86ab`.
Mirrored/member-verified raw archive `packed-byte-word-consolidation.tar.xz`:
`bb4695f2015b6c36b41501f9a80824e9c2ead24ac8bcb59686048778fd5ec4e6`, 11,254,884 bytes / 247 members. Reproduction scripts and original/both-stage
source/module snapshots are retained there; the evidence lists raw dependencies.
