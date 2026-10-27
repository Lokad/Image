# Local vector views for VP8 residual pricing

The photograph independently measures 19.42 ms: 1.996× reference time (upper
2.004×), 809,056 B/op and an exact 17,688-byte packet. Elapsed time is 9.71% below
the preceding independent mean; these separate runs are not a paired baseline
gain. The required conservative upper ≤1× remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 short | 19,632.11 | 9,732.51 | 2.017 / 2.109 | 809,056 | 17,688 |
| PhotographLossy8 confirm | 19,416.18 | 9,726.50 | 1.996 / 2.004 | 809,056 | 17,688 |
| DiagramLossy8 short | 10,935.19 | 5,822.51 | 1.878 / 1.976 | 741,121 | 7,100 |
| vp8 short | 78,731.33 | 53,402.64 | 1.474 / 1.638 | 23,258,168 | 98,590 |
| DiagramLossyTrellis8 short | 19,937.03 | 9,780.74 | 2.038 / 2.089 | 754,328 | 6,818 |

Both idle-v2 guards pass for every row. Photograph confirmation retains all twelve
actual and filtered samples; other rows are current shorts. Frozen inputs/settings
and complete normalization, fresh output ownership and release boundaries remain.
No preparation, startup, transport, validation or ToArray is timed. Complete packets
and registered normal/scalar reconstructed pixels agree.

Read-only sixteen-word views borrow two initialized local vectors, replacing two
separate stackalloc arrays and copies. Each vector occupies exactly 32 bytes; the
one-vector spans cast to sixteen words and remain inside the call. No reference
escapes and no input/table is written. Original length, type, context, level,
disposal and bounds checks remain, as do all integer costs, EOB logic and scalar
fallback. Existing 64-byte coarse workspace remains; no heap, owner, cache,
reservation or lifetime changes.

Compiler inlining exposes constant luma arguments. A validation-only JIT capture
shows residual/band calls removed from `Luma4`, with code growing from 4,969 to
5,806 bytes. Those instrumented timings are discarded. This changes storage and
call dataflow, leaving the pricing policy and table format intact.

Both systems pass 4,943 Release checks (1,961 internal, 2,035 public, 947 live),
622 scalar WebP checks and 30 focused checks. Native table/residual/context hashes,
extremes, tails, invalid/disposed behavior, trellis/macroblock traces, complete
packets, decoder/editing precision, ownership and cancellation agree. The same
actual private 0.2.0 package and three examples pass both systems; published 0.1.0
is unchanged. Production built once per system; harnesses, workers and selectors
were reused. [Evidence](warm-vp8-residual-vector-views-evidence.json) retains code,
identities, samples and the mirrored, member-verified raw archive.

Older affected encoder costs are historical; unrelated decoder/codec/editing/text
evidence remains valid. Stop residual/compiler variants. Resolve remaining named
uncertainties and prioritize the largest demonstrated gap, retaining the finite
scope, exact contracts and existing worker/build reuse rules.
