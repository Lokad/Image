# Shared exact VP8 small-prediction rows

The existing photograph independently confirms 24.54 ms, 2.524× reference time /
upper 2.538×, 909,696 B/op and exact 17,688 B. Elapsed is 11.8% lower than the
preceding quiet short; this is not a paired independent-baseline gain claim.
Full parity remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 short | 24,410.28 | 9,663.98 | 2.526 / 2.823 | 909,696 | 17,688 |
| PhotographLossy8 confirm | 24,541.14 | 9,722.14 | 2.524 / 2.538 | 909,696 | 17,688 |
| DiagramLossy8 short | 14,351.18 | 5,828.44 | 2.462 / 2.723 | 841,608 | 7,100 |
| vp8 short | 83,452.21 | 53,391.09 | 1.563 / 1.832 | 23,358,400 | 98,590 |
| vp8-stream short | 7,838.48 | 5,987.65 | 1.309 / 1.337 | 1,966,583 | 531,468 |

Both idle-v2 guards pass for every row. Photograph retains 12 actuals, 11 filtered;
its conservative upper includes all-actual uncertainty. Other rows are current
shorts. Reference settings/inputs and complete normalization, delivery, output
ownership and release boundaries are unchanged. No ToArray, setup, startup,
transport or validation is timed. Encoder packets and decoder pixels remain exact.

Encoder borrowed 4×4 predictions now share the decoder's existing whole-row
formulas. Snapshot eight top, four left and one corner sample before writes,
preserve geometric mode mapping, and emit four explicit little-endian rows.
Average/DC ranges stay byte; true motion retains its clamp. No heap, cache, owner,
reservation or lifetime change; snapshot storage shrinks 84 B→52 B inside existing
admitted workspace. Mode-level cancellation remains. Per-pixel geometry is removed
from production and retained as an independent mathematical test oracle only.

Both OS pass 4,933 Release checks (1,951 internal / 2,035 public / 947 live), 612
scalar WebP checks and 42 focused prediction/decision checks. Native mode hashes,
8,064 prepared geometry comparisons, ten hand-computed modes and 7,680 new partial
alias/output placements agree; source, holes/tails, invalid inputs and zero
allocation pass. Actual macroblock decision and complete codec, editing, memory
and cancellation gates remain. The same actual private 0.2.0 package and all three
examples pass both OS; published 0.1.0 is unchanged.

Production built once per OS and stayed immutable. One stale test-only call was
corrected after its first build failure; only tests were rebuilt. Original failed
logs are retained. Benchmark/native workers and prepared selectors were reused;
no repeated profile or new image/settings grid. Earlier affected VP8 encode/decode
costs are historical. Other valid path evidence remains unchanged.
[Evidence](warm-vp8-shared-small-rows-evidence.json) retains all samples, precise
qualification identities and the mirrored, member-verified raw archive. Stop this
primitive round; inspect unchanged residual pricing as the next bounded code lead.
