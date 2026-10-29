# Bounded cancelable PNG row cursor

Complete tight+cancelable PNG8 decoding improves **21.0%**, with unchanged
**68,224 B/op**. It remains a 1.094× / upper 1.107× gap. The existing 16-bit
companion independently passes parity at upper 0.984× and stops.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Prior tight+cancelable RGBA8 | 1,934.514 | 1.387 / 1.395 | 68,224 | Prior |
| Current tight+cancelable RGBA8 | 1,529.094 | 1.094 / 1.107 | 68,224 | Gap |
| Current tight+cancelable RGBA16 | 2,840.627 | 0.978 / 0.984 | 70,272 | Pass; stop |

Cancelable rolling frames reuse the existing local bit cursor instead of a
per-token loop. Checks before and after each slab bound a cursor call to at most
65,536 produced bytes; history consumption/sliding is also bounded by that
64 KiB buffer. Block, canonical-fallback and row checkpoints remain. No new
capacity, buffer, owner, quota or admission mechanism is introduced. The replaced
loop and its two otherwise-unused literal/match writers are removed.

Noncancelable cursors and cancelable materialized framing keep their paths.
Complete-tree eligibility, incomplete-tree fallback, framing/filter/checksum
ordering, exact pixels, hidden RGB, limit/cancel flags and ownership remain.
Only the two existing affected controls are refreshed; unchanged proof is reused.

The fresh three-actual byte pair clears the strict 5% gate. Independent twelve
actuals reuse the prior original twelve: 20.957% filtered/21.041% unfiltered
elapsed reduction and 21.114% normalized point improvement. Both raw 99.9% CIs
and conservative normalized envelopes separate. The word row is a current
confirmation, without a small causal gain claim. All raw samples, native
brackets and idle-v2 guards are retained. Native allocations are unmeasured.

All 5,132 checks/OS pass (2,049 internal, 2,135 public, 948 live), plus 122 internal
and 258 public checks normal/scalar per OS. Three new BCL-backed controls cover
many history slides, checksums, stored/compressed blocks, pre-cancellation,
shared-root refunds and recovery; existing truncation/fallback/live-cancel and
ownership tests remain. The same actual Release package and three examples pass
both OSes. One core build/OS, zero harness/selector/native builds; immutable
binaries are reused throughout. Published releases remain unchanged.

The planned PNG cost refresh is complete; gaps and confidence-open cases remain.
This round does not complete the full 268-path warm parity objective.
[Evidence](warm-png-cancelable-row-cursor-evidence.json) and
[prior policy costs](CURRENT-PNG-POLICY-PACKED-KEY-COSTS.md).
Mirrored and verified archive png-cancelable-row-cursor-qualified.tar.xz, 151 members,
SHA256 0b6b99307b39f74b350e4dac9c438dd1d52a5d7db825b1f9d7756b6aea573c87; reproduction dependencies pinned by hash.
