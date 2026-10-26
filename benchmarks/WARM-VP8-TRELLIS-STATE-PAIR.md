# Two predecessor states in VP8 trellis

Historical encoder cost: [large-block prediction](WARM-VP8-LARGE-BLOCK-ROWS.md)
now supplies current affected timings. Exact DP mathematics remains unchanged.

The existing effort-6 diagram independently measures 23.35 ms: 2.380× reference
time (upper 2.394×), 854,968 B/op and an exact 6,818-byte packet. Elapsed time is
11.6% below the preceding independent mean; these separate runs do not establish
a paired baseline gain. Full warm parity remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| short | 23,461.03 | 9,784.67 | 2.398 / 2.491 | 854,968 | 6,818 |
| confirm | 23,348.31 | 9,810.30 | 2.380 / 2.394 | 854,968 | 6,818 |

Both idle-v2 guards pass. Confirmation retains all twelve actual and filtered
samples. Frozen input/settings and complete normalization, output ownership and
release boundaries remain. Preparation, startup, transport and validation are
untimed. Complete packets and registered normal/scalar reconstructed pixels agree.

Trellis scoring keeps its two predecessor states in scalar locals, borrowing rate
and matrix views once per block and computing both row offsets once per coefficient.
Invalid choices retain dead scores and their next contexts. Strict comparisons
preserve predecessor and terminal ties; the same integer arithmetic, level bounds,
scan, validation, cancellation and unwinding remain. No table, cache, allocation,
owner or admission changes; 36 bytes of stack scratch are removed.

Both operating systems pass 4,937 Release checks (1,955 internal, 2,035 public,
947 live), 616 scalar WebP checks and 63 focused checks. These include 432 frozen
signed coefficient/context/probability fragments, method-5/6 packet identities,
macroblock traces, live-token fallback and ownership. The same actual private
0.2.0 package and three usage examples pass both systems. Published 0.1.0 stays
unchanged. Production built once per system; harnesses, workers and both selectors
were reused. [Evidence](warm-vp8-trellis-state-pair-evidence.json) retains identities,
raw samples and the mirrored, member-verified archive.

This routine is absent from effort-4 encoding, so those current costs and unrelated
decoder/codec/editing/text evidence remain valid. The photograph is now the largest
measured gap. Stop trellis variants; one bounded photograph profile will resolve
the hotspot uncertainty introduced by shared prediction rows before another
primitive is chosen. Instrumented timings will not be scored.
