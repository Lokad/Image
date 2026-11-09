# Current bounded bicubic execution

[Shared overlap execution](BICUBIC-SHARED-OVERLAP.md) implements the hypothesis
below and changes the profiled loop. This capture remains motivating history,
without establishing current phase costs.

CTA714 reuses the qualified JLP713 actual-package DLL for one two-second capture
of existing RGBA8 513×259→256×129 bicubic resizing with input+output+4 KiB working
limit. Production, fixtures, displayed costs and qualification stay unchanged;
there are no builds or timing confirmations. Instrumented elapsed is discarded.
The source-valid complete cost remains **542.51 / zimg417.07 µs**, upper1.321,
113 B/op, under [shared byte repair](BICUBIC-SHARED-BYTE-REPAIR.md).

| Selected exclusive sampled locations | Samples, out of 1,996 |
| --- | ---: |
| Indexed horizontal pair blocks | 555 |
| Source conversion blocks | 258 |
| Paired vertical blocks | 225 |
| Publication blocks, including certificates | 442 |
| Original-order byte repair | 2 |

The two column-driver specializations contain 673/1,185 samples and have
5,433/5,545-byte generated bodies. Their perf names are identical; the mapper
disambiguates absolute addresses and unique JIT lengths before assigning offsets.
The selected blocks above do not exhaust the capture. Inlining and sampling skid
prevent treating locations as phase costs, removable percentages or speedup forecasts.

Horizontal blocks repeatedly load two byte offsets, check decoded indexes, load
two pixel vectors and construct wide-vector halves per tap. The cached narrow
path already prepares a distinct eight-tap overlap proof and reuses five source
windows for ten pixels, retaining each output's original tap order. A bounded
geometry model finds **124 / 128 pairs eligible** on each existing byte/word
256×129 control. Two pairs have different counts, two hit edge/stride conditions.
These are modeled geometry counts, not instrumented production branches.

The next coherent hypothesis is to share that existing fixed overlap kernel
between cached and bounded column execution. Prepare applicability once per
strip, using explicitly reset dead padding; preserve actual indexed/clamped,
unequal/full-capacity and odd-tail paths. The intended result is one ordered
two-output arithmetic body with specialized geometry and ownership at their
natural boundaries. No generic cache, session, API or retained state is proposed.

Current small/large explicit array payloads are 4,080/3,232 B under the existing
4,096-byte admission. Large caches and vertical geometry borrow unpublished
result storage; loans must retire before those pixels are written. The proposed
kernel adds no owned capacity or retention. These source capacities are not
physical/native peak measurements. Exact binary64 arithmetic order, certificates,
tiny-alpha unwind and rational replay remain requirements.

First prove direct ordered sums and actual-column edge/stale-padding/unequal/
footer behavior. Then use a complete TightDown8 short screen and independent
reversed confirmation if useful; existing Down8 and TightDown16 supply affected
and generalization controls. Finish the precise affected-cost audit before
acceptance. Calls, inlining and preparation can offset removed work; code structure
alone establishes no gain. Retain adverse/inconclusive outcomes and revise or
abandon based on completed workloads, without fixed thresholds or a family ban.

CPU0 executes and CPU1 samples on the exclusive quiet VM; both idle-v2 guards,
frozen rational/managed/native frames, source and independent ownership checks
pass. Managed preparation, workspace and output disposal stay inside each call;
zimg retains its labeled graph/scratch. Native float does not provide the managed
exactness guarantees. Unchanged 6,100 full/live checks per OS, scalar/ISA paths
and the same actual package/three examples are reused. JLP713 CI is green.

[Evidence](bicubic-bounded-current-attribution-evidence.json) pins sources,
producer, command, gates, JIT regions, model, ownership and next hypothesis.
Mirrored/member-verified raw `bicubic-bounded-current-attribution.tar.xz`:
139,496 B / 30 members, SHA256
`3cbfc562361892b1a0bceaac65cb2c19f5b0dd0b0640b8a776360d0601c2867a`.
This audit adds no renderer dependency or displayed timing claim. Publication
remains postponed; all 283 material paths remain required.
