# Refresh VP8 prices in their existing owner

The photograph independently measures 21.50 ms: 2.214× reference time (upper
2.222×), 809,056 B/op and an exact 17,688-byte packet. Allocations fall by
100,640 B/op (11.1%). Elapsed time is 1.91% below the preceding independent mean;
these separate runs do not establish a paired baseline CPU gain. Full warm parity
remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 short | 21,836.56 | 9,710.01 | 2.249 / 2.357 | 809,056 | 17,688 |
| PhotographLossy8 confirm | 21,504.39 | 9,711.21 | 2.214 / 2.222 | 809,056 | 17,688 |
| DiagramLossy8 short | 12,203.50 | 5,833.47 | 2.092 / 2.276 | 740,968 | 7,100 |
| vp8 short | 80,109.96 | 53,494.76 | 1.498 / 1.935 | 23,257,760 | 98,590 |
| DiagramLossyTrellis8 short | 21,201.56 | 9,795.35 | 2.164 / 2.255 | 754,328 | 6,818 |

Both idle-v2 guards pass. Photograph confirmation retains all twelve actual and
filtered samples; other rows are current shorts. Frozen inputs/settings and
complete normalization, fresh output ownership and release boundaries remain.
Preparation, startup, transport, validation and ToArray are untimed. Complete
packets and registered normal/scalar reconstructed pixels agree.

Mid-frame probability updates reuse the operation-owned rate array, regenerating
only coefficient-dependent EOB/variable prices. Factory and refresh share one fill
body. Entropy, modes and fixed-level prices remain unchanged. The frame borrows
no views across updates; pass cadence, statistics and header retries are preserved.
There is no global cache, independent allowance, new owner or fallback strategy.

The frozen photograph avoids five actual array/owner replacements. Charged
rate-update storage falls from 41,408 B (two plans and factory scratch) to
20,192 B; this is not an overall-operation peak measurement. Retained capacity
stays charged until disposal. Refresh requests no admission and allocates nothing,
including with the shared root full. Initial admission and independent-plan
replacement/refusal contracts remain. Refusal is terminal; cancellation still
checks before mutation and within coefficient-table generation.

Both systems pass 4,943 Release checks (1,961 internal, 2,035 public, 947 live),
622 scalar WebP checks and 30 focused checks. Four full-table profiles match fresh
plans and preserve the invariant prefix, capacity and borrowed input independence.
Invalid/pre-cancelled updates preserve tables; disposal releases charges and later
operations succeed. The same actual private 0.2.0 package and all three examples
pass both systems; published 0.1.0 stays unchanged. Production built once per system;
harnesses, workers and selectors were reused. [Evidence](warm-vp8-rate-plan-refresh-evidence.json)
retains identities, samples and the mirrored, member-verified archive.

Older affected encoder costs are historical; unrelated decoder/codec/editing/text
evidence stays valid. Stop refresh/pricing variants. The next bounded code lead
is residual-call scratch and constant-argument inlining, preserving exact integer
costs and safe managed storage. Prior rejected table-format and emitter experiments
remain closed; no new profile, kernel, cache or settings grid is needed.
