# VP8 source-activity audit

The two existing opaque513×259 controls retain open size gates. Native segment
quantizers correlate strongly with source block gradients, while the managed
encoder currently uses one quantizer per image. This supports testing one
source-derived spatial policy; it does not establish compression or quality gains.

| Frozen control | Managed / reference bytes | Current uniform q index | Native segment q range | Gradient/q rank correlation |
| --- | ---: | ---: | ---: | ---: |
| Photograph8 | 21,432 / 17,688 | 28 | 19–36 | 0.859 |
| Diagram8 | 8,288 / 7,100 | 32 | 22–36 | 0.881 |

Correlations use512 complete16×16 blocks per image; the raw report separately
retains all561 blocks, including padded edges. Source activity follows the owned
RGB-to-luma formula and edge replication. It is a diagnostic, not an independent
pixel-conformance check. The original C38 arithmetic header reader supplies native
segments/modes; source and native packet hashes match the frozen quality catalog.
No new image, setting, encoder/native invocation or timing job is introduced.

The photograph's four native groups have median mean gradients1.67/5.40/8.79/11.68
at q19/25/32/36. The diagram groups have0/4.81/18.57/35.33 at q22/29/33/36. These
observations do not identify the native algorithm. The next candidate must derive
its map and quantizers from source activity, with no copied native assignments.

Keep managed qualities78/75, native75/effort4 and the existing0.25dB source-quality
band, exact alpha, decoder bounds, complete output ownership, admission/work and
independent single-CPU warm parity gates. Required complete sizes remain17,688/
7,100 B. Equivalent input delivery, normalization and surviving output ownership
stay inside both operation boundaries. Stop a failed byte/quality screen before
scored timing; reuse valid reference evidence and immutable builds.

Production remains unchanged. The [current package qualification](../docs/text-default-instance-qualification.md)
is reused, with zero core/harness/native/selector rebuilds and no allocation claim.
The finite checklist remains256 cases; every remaining size/performance gate stays
open. [Numeric observations](vp8-activity-audit-evidence.json) retain source identities,
all/interior correlations and grouped statistics. Original input/reference bytes
remain in the cited h9b archive; the maps and reader/scripts are retained separately:

`c51-vp8-activity-audit.tar.gz`, SHA256 `07e1225f869b57393fb4bd1e1d3c25d6305825a15eacec96bf7c5e7aa27312a2`.
