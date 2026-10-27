# Fixed zigzag geometry in VP8 trellis

The timing below is historical after [flat-coefficient counting](WARM-VP8-FLAT-COUNTS.md).
Exact scan/packet proof remains; current affected costs are reconciled there.

The existing effort-6 diagram independently measures 18.64 ms: 1.901× reference
time (upper 1.910×), 754,328 B/op and an exact 6,818-byte packet. Elapsed time is
6.50% below the preceding quiet short; these runs are not a paired baseline gain.
The required conservative upper ≤1× remains open.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| short | 18,383.66 | 9,806.20 | 1.875 / 2.191 | 754,328 | 6,818 |
| confirm | 18,642.09 | 9,805.01 | 1.901 / 1.910 | 754,328 | 6,818 |

Both idle-v2 guards pass. Confirmation retains all twelve actual and filtered
samples. Frozen input/settings and complete normalization, fresh output ownership
and release boundaries remain. Preparation, startup, transport, validation and
ToArray are untimed. Complete packets and registered normal/scalar reconstructed
pixels agree.

Trellis now reads the sixteen-position RFC 6386 zigzag geometry as constant format
data, replacing its per-block diagonal-construction loops. Every position matches
the previous mathematical generation. This adds no upstream implementation table,
heap array, cache or kernel. It removes 16 bytes of temporary scan storage while
retaining existing coarse admission. Coefficients, weights, levels, DP costs,
strict ties, EOB, validation, cancellation, tails and ownership remain unchanged.

Both systems pass 4,943 Release checks (1,961 internal, 2,035 public, 947 live),
622 scalar WebP checks and 18 focused checks. These include 432 frozen signed
coefficient/context/probability fragments, native macroblock scores and complete
method-5/6 packets. The same actual private 0.2.0 package and all three examples
pass both systems; published 0.1.0 remains unchanged. Production built once per
system; harnesses, native workers and short/confirm selectors were reused.
[Evidence](warm-vp8-trellis-scan-data-evidence.json) retains source/core identities,
samples and the mirrored, member-verified raw archive.

Effort-4 encoding never enters this routine, so its current photograph, diagram
and alpha measurements remain valid. Unaffected decoder/codec/editing/text proof
also carries forward; older trellis timings are historical. Stop scan/DP variants
and select the largest remaining gap or named uncertain confirmation.
