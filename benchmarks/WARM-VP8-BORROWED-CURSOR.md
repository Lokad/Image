# Borrowed arithmetic cursors for VP8 emission

The existing opaque RGBA16 control independently uses **6.8% less elapsed**:
21.978 ms versus a fresh 23.594 ms baseline. Its **1.410× / upper 1.420** CPU gap
remains. Complete packets stay exact at 72,380 B and allocation at 1,201,552 B/op.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Baseline opaque16, short | 23,693.27 | 15,594.13 | 1.519 / 1.586 | 1,201,552 | 72,380 |
| Baseline opaque16, confirm | 23,594.10 | 15,504.45 | 1.522 / 1.528 | 1,201,552 | 72,380 |
| Borrowed opaque16, short | 21,945.77 | 15,596.89 | 1.407 / 1.440 | 1,201,552 | 72,380 |
| Borrowed opaque16, confirm | 21,978.27 | 15,585.15 | 1.410 / 1.420 | 1,201,552 | 72,380 |
| PhotographLossy8, short | 15,075.12 | 9,710.51 | 1.552 / 1.603 | 809,072 | 17,688 |
| DiagramLossy8, short | 8,210.55 | 5,830.12 | 1.408 / 1.492 | 741,137 | 7,100 |
| DiagramLossyTrellis8, short | 14,974.25 | 9,821.74 | 1.525 / 1.565 | 754,344 | 6,818 |
| DiagramLossyDistortion8, short | 3,598.28 | 2,509.90 | 1.434 / 1.484 | 741,375 | 7,330 |
| Alpha8, short | 71,411.80 | 53,620.68 | 1.332 / 1.437 | 21,353,192 | 98,590 |

All controls retain their existing513×259 inputs/settings, complete normalization,
encoding and fresh independent output acquisition/release. Both main confirmations
retain12actual/11filtered samples each; conservative bounds cover both sets and
native brackets. Every idle-v2 guard passes. This gain compares separate sequential
independent means, not paired statistics or general content. Other rows refresh
five affected costs without independent speedup/parity claims. Native allocation
is unmeasured. No extra image/settings grid or quality search is added.

Each partition now lends one stack-only cursor to the synchronous frame encoder.
The cursor retains arithmetic state and a borrowed output span across symbols,
calling the owner only when logical capacity exceeds the admitted buffer. Commit
numeric state before growth and in finally before finishing, including failure.
The same32-bit publication, carry and termination remain; typed bool/byte symbols
replace private integer entry points. No forwarding wrapper or second arithmetic
implementation remains.

Physical capacity still belongs solely to the existing partition owner. Admission
at logical-byte boundaries, replacement overlap, per-bit/carry/growth cancellation,
terminal refusal and cleanup remain. Two bounded cursor values fit the existing
1024-byte frame scratch allowance. No heap array, cache, frame copy, reservation
or new ownership abstraction is added. Cursors cannot escape through async output;
the actual partition owners stay charged through awaited delivery.

Both OS pass5,050 Release checks each, including947 live comparisons,47 internal/
27 public scalar checks, the SAME actual private0.2.1 package and all three examples.
Before scoring,21focused normal/scalar checks cover8,192original partitions, all
probabilities,15native carry/termination/exact-limit controls, shared/local/output
refusal, replacement/cancellation cleanup and later success. New fragment-boundary
checks reproduce two native packets across six cursor commit intervals, including
growth and carry, while checking retained capacity and borrowed-root ownership.

One missing12-iteration selector was compiled against cached binaries. An initial
object-returning template failed at compile time; the existing int-returning
template corrected it. Production/harness/native binaries stayed immutable, with
one successful producer build perOS. No repeated full/package qualification.
Published0.2.0 is unchanged. [Numeric evidence](warm-vp8-borrowed-cursor-evidence.json)
retains samples, guards, source/binary/package identities and the mirrored,
member-verified raw master.

All VP8 encoder emission costs change; six displayed controls are refreshed.
Other affected named callers need applicable current evidence before pass claims.
VP8L alpha coding itself, all decoding, other codecs/editing/text and applicable
unchanged proofs remain valid. Stop cursor/word/fragment variants; the full
267-path independent warm CPU-parity objective remains open.
