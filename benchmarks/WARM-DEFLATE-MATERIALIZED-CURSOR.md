# Work-bounded materialized Deflate cursor

This earlier production experiment is retained history.
[Current two-retained-output decoding](CURRENT-RETAINED-PNG-DECODE-COST.md)
supersedes that row's old cost, allocation and governed peak.

One local bit cursor improves complete PNG decoding without new storage or a
precision change. Both existing operations remain parity gaps, so neither gets
a longer confirmation.

| Complete operation | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | Elapsed reduction |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Two retained PNG8 outputs / caller root | 3,604.48 | 2,850.06 | 1.265 / 1.304 | 2,574,671 | 15.8% |
| PNG8 DecodeAsync / fresh nonseekable source | 1,574.38 | 1,414.79 | 1.113 / 1.176 | 12,784 | 23.2% |

The reductions compare the relevant unchanged-path baselines: 4,280.80 µs for
two retained outputs and 2,049.34 µs for async input. Native allocations remain
unmeasured. The caller-root admitted peak stays 2,306,560 bytes including its
218,775-byte resident packet. Both outputs remain independently owned through
overlap and disposal. Managed admission is additional timed work; FFmpeg has no
corresponding root API. Async timing includes actual input acquisition, copies,
EOF, decoding and independent result ownership, with the original native boundary.

The existing materialized loop now keeps literal/length/distance/history state
local across an entire block. RFC1951 permits a conservative bit-work bound of
`16*remainingOutput+48`. Selection checks this before the block; only actual bits
settle. Tight work and live tokens retain the original paths. Long prefixes use
the original canonical decoder; every exit publishes exact work/cache/cursor
state. Error order, checksum position, overlapping copies and pixel precision
are unchanged. No buffer, table, cache, owner or shared allowance is added.
The [proof](https://github.com/lokad/image/blob/27fc0425be9e21d913e7bd049fe3ce629bf28955/docs/deflate-materialized-cursor.md) explains failure prefixes and
the 47-bit refill bound. This is a complete-block strategy, distinct from earlier
literal-only batching and already accepted local-work accounting.

Both OSes pass **4,481 Release tests**:1,657 internal,1,892 public API and932 live
FFmpeg comparisons. Each also passes122 Zlib and36 cooperative-memory controls
normally/scalar, plus178 affected public PNG/memory controls scalar. New authored
tests cover block phases, deep canonical distance fallback, matches and extras,
every truncation, malformed tokens, guarded partial output and exact work. The
same actual private0.2.0 package and all three usage examples pass both OSes,
including default versus live-token PNG pixels and independent charged survivors.
One production build per OS is reused throughout; no post-freeze production,
harness, selector or native build. Public/differential test assemblies are reused.

All21 async managed boundary controls revalidate on the new producer; unchanged
normal/scalar native controls and CLI pixels are reused. Each three-actual short
passes both official idle-v2 guards and original pixel/alpha/ownership gates.
The previous924-sample profile describes the previous producer only; it is not
a profile or cost attribution for this new loop.

Only the two current PNG decode rows refresh. Historical PNG resident/Stream/
crop-fit rows reaching the changed `Walk` remain identified historical observations,
including count-only/cancelable routes through that method. They are not silently
requalified. PNG metadata/encoding, rolling-row implementation and other codecs,
editing and font paths retain their identified unchanged scopes. Coverage stays
254=238numeric+16unmeasured,72passes/165gaps/one confidence-open; full parity and
release readiness remain open. Published0.1.0 is immutable.

Next: bound live-token VP8 coefficient-block work settlement, preserving each
original cancellation poll and exact failure state, for its current1.577x gap.
The1.588x word-Lanczos gap remains open; its rejected strategies remain retired.
[Evidence](warm-deflate-materialized-cursor-evidence.json) retains statistics,
allocations, exact identities, qualification and mirrored raw archives.
