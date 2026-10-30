# Exact SIMD JPEG horizontal area filtering

The following costs are superseded by [column preparation](WARM-JPEG-COLUMN-PLANS.md).
Its accepted filtering primitive and raw evidence remain retained.

Caller-RGB420 independently uses **16.1% less elapsed**,2.030ms versus a fresh
2.420ms baseline. The **1.283× / upper1.294** CPU gap remains open. Complete
packets stay52,091/52,093B, differing only by the approved attribution COM;
reconstructed pixels and373,416B/op remain unchanged.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Baseline callerRGB420, short | 2,424.90 | 1,579.90 | 1.535 / 1.744 | 373,413 |
| Baseline callerRGB420, confirm | 2,420.45 | 1,576.33 | 1.535 / 1.550 | 373,416 |
| SIMD callerRGB420, short | 2,026.47 | 1,576.54 | 1.285 / 1.568 | 373,416 |
| SIMD callerRGB420, confirm | 2,030.36 | 1,582.17 | 1.283 / 1.294 | 373,416 |
| GovernedRGBA16, short | 2,251.99 | 1,839.51 | 1.224 / 1.446 | 804,644 |

These existing513×259 controls retain Q85/native qscale5 and frozen inputs/settings.
Both operations include complete normalization/conversion, encoding and fresh
independent output acquisition/release. All actuals, including one filtered value
in each12-iteration confirmation, native brackets and idle-v2 guards are retained.
The16.1% comparison uses separate sequential independent means, not paired-sample
statistics or a general-content claim. Native allocation is unmeasured.

The existing scalar tap loops become eight interleaved Cb/Cr pairs on safe
AVX-512BW. Three tap-selector/weight vectors are prepared once perMCU and reused
across its rows. Generated area phases/edges, integer truncations, upper clips
and signed Q7 guard samples remain exact. Vertical filtering, portable/tiny-edge
fallbacks and cancellation boundaries stay unchanged. The384-byte plan and bounded
constructor scratch fit the existing8KiB admitted stack workspace; no heap array,
cache, frame copy, reservation or ownership change is added.

The VM compares widths16..1025, odd/paired rows and MCU phases against widened
scalar sums, including16..18sample windows, extrema, signed guards, unaligned
input/output tails, borrowed-source preservation and zero allocation. Existing
axis extent, matrix/coefficient and native complete-packet controls remain exact.
Both OS pass5,049 Release checks each, including947 live comparisons,4 internal/
293 public scalar checks, the SAME actual private0.2.1 package and three examples.
Published0.2.0 is unchanged. One explicit core build/OS serves the round; no
harness/native/selector build or new timing input/settings grid.

GovernedRGBA16 is a refreshed screen, not a new independent speedup/parity claim.
Its shared-root peak remains1,704,856B, including1,062,936B retained source, with
temporary cleanup, terminal refusal/recovery and cancellation verified. Packet
52,115/52,117B remains exact within the same COM exception. The short caller
allocation difference of3B reflects GC reporting across80/96invocations;
independent baseline/candidate figures agree exactly.

[Numeric evidence](warm-jpeg-horizontal-area-filter-evidence.json) records all
identities, samples, package checks and the mirrored/member-verified raw archive.
Only AVX-512BW subsampled420/422 encoding costs change, including caller,8/16bit,
stream/async, restart and cancelable callers. Other affected inventory costs need
current evidence before pass claims. Grayscale/444, scalar/tiny edges, decoding,
other codecs/editing/text and applicable unchanged proofs remain valid. Stop this
primitive; the full267-path warm CPU-parity objective stays open.
