# Exact header lower bounds for VP8 distortion search

The existing 513×259 opaque diagram, Q75/effort2, independently uses **5.7% less
elapsed**: 3.772 ms versus a fresh 4.001 ms baseline. Its **1.490× / upper 1.518**
CPU gap remains open. Complete packets remain exact at 7,330 B; allocation stays
741,375 B/op, with identical reconstructed pixels and alpha.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Baseline, short | 4,193.49 | 2,510.96 | 1.670 / 2.137 | 741,375 |
| Baseline, confirm | 4,000.89 | 2,508.93 | 1.595 / 1.618 | 741,375 |
| Header bound, short | 3,770.87 | 2,504.13 | 1.506 / 2.046 | 741,375 |
| Header bound, confirm | 3,772.35 | 2,532.10 | 1.490 / 1.518 | 741,375 |

Both subjects include complete normalization, encoding and fresh independent
packet acquisition/release. Native brackets and idle-v2 guards pass. Every actual
is retained: baseline12/12 and candidate10/12 filtered confirmation samples,
with conservative filtered/unfiltered bounds. Separate sequential means do not
establish a paired-sample statistic or general-content gain. Native allocation
is unmeasured; no input/settings grid or quality search is added.

One current two-second diagnostic identifies mode search among remaining costs.
A count-only own-source helper finds16,302/30,980 candidates (52.6%) whose header
alone cannot win. Distortion-only scoring is256*nonnegative SSD+11*header;
header>=best therefore proves the candidate cannot win a strict-less decision.
Skip prediction/error calculation after the existing per-mode cancellation check.
The first mode always evaluates. This is distinct from the closed RDO-only header
experiment, whose eligibility was1.8% under a different metric.

No scratch, owner, reservation, table, cache or heap allocation is added. Existing
16KiB admission, input snapshot/alias behavior, guards, exact ties, quantization,
terminal refusal, cancellation and independent outputs remain. Eight existing
native rolling-decision/statistics/final-pass and ownership/guard controls pass
normal/scalar before scoring; complete methods0..6 packets remain exact.
Both OS pass5,049 Release checks each, including947 live comparisons,21 internal/
27 public scalar checks, the SAME actual private0.2.1 package and three examples.
Published0.2.0 is unchanged. One explicit core build perOS is reused throughout;
harness/native/selectors are unchanged. No mirrored bound test is needed.

[Evidence](warm-vp8-effort2-header-bound-evidence.json) retains all samples,
identities, diagnostic/count controls and mirrored/member-verified raw masters.
Two failed retention attempts are kept; correctedv3 explicitly verifies current
perf/map/summary/count/source members. No timing depended on invalid archives.
Only distortion-only method0/1/2 small-block encoding costs change. Other affected
named callers need current evidence before pass claims; RDO/trellis, decoding,
other codecs/editing/text and applicable unchanged proofs remain valid. Stop this
primitive; the full267-path warm CPU-parity objective remains open.
