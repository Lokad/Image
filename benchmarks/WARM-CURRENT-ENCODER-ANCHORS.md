# Current encoder cost anchors

PNG encoding costs below are superseded by the [shared checksum round](WARM-PNG-SHARED-ADLER.md). Unchanged codec paths retain their recorded scope.

These existing 513×259 controls retain frozen inputs/reference settings and
complete independent output ownership. Normalization is timed where required;
the caller-RGB boundary borrows identical packed samples on both subjects. One
CPU per operation. No production source, image or settings grid changed.

| Quiet run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet bytes, Lokad / reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| JPEG444, forward-only short | 4,971.54 | 3,769.20 | 1.319 / 1.434 | 820,123 | 133,039 / 133,041 |
| JPEG caller RGB420 short | 2,606.80 | 1,576.55 | 1.653 / 1.899 | 373,413 | 52,091 / 52,093 |
| PNG RGBA8 SmallestSize short | 24,517.23 | 17,302.12 | 1.417 / 1.498 | 1,325,670 | 210,973 / 210,973 |
| VP8 + alpha independent | 95,003.17 | 53,483.81 | 1.776 / 1.789 | 24,424,944 | 98,590 / 98,590 |

All idle-v2 guards pass. The VP8 independent run retains all 12 actuals and
resolves its preceding short screen's broad confidence interval. Other rows are
current cost screens without independent parity or speedup claims. All remain
warm 1× gaps; old producer timings do not establish a cleanup speedup/regression.

Complete PNG and VP8 packets are raw exact. JPEG differs only in the approved
attribution COM; coded bytes, source normalization, reconstructed pixels and
caller/output ownership checks remain frozen. No dB tolerance or equal quality
number substitutes for those byte gates.

The same immutable library, prepared native workers and generated/benchmark hosts
serve all four runs. No source synchronization, rebuild or repeated package proof
was needed. [Production qualification](WARM-VP8L-SPARSE-PREDICTOR.md) is reused:
4,907 checks per OS, affected scalar/Vector256 and the SAME actual private 0.2.0
package/all three usage examples pass Windows/Linux. Published 0.1.0 is unchanged.

One bounded profile on the existing VP8 control supplies 744 workload samples;
leading exclusive attributions are `Vp8lReferenceMatcher.Build` 11.7%, `Vp8MacroblockDecision.Luma4` 6.5%, `Vp8PartitionWriter.Write` 5.9%. These are diagnostic sample shares,
not component timings. Profile times are discarded. The unchanged collector and
parser are reused; trace size is capped at 2 MB. The raw trace is retained.

[Evidence](warm-current-encoder-anchors-evidence.json) records samples, packet
identities, allocations, guards, reused qualification and the raw master. VP8's
current independent gap is the largest of these encoder anchors; use the profile
to select one substantial primitive. Stop repeating these unchanged measurements.
Other named producer uncertainties and the full 253-path warm 1× objective remain
open. This is the declared content/settings scope, not a general speed claim.
