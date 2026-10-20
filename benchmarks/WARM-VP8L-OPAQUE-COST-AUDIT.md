# Opaque lossless WebP coding-cost audit

Historical C48 audit of the C47 producer. The subsequent
[coding-cost round](WARM-VP8L-CODE-COST.md) selects and fully qualifies this plan;
references to current production below describe the original audit only.

The existing opaque513×259 control reveals a skipped compression opportunity.
Current production remains170,156 B; an untimed probe considering the existing
prediction/grouping code produces123,602 B. It is **not production-selected or
performance-qualified**. The frozen libwebp `exact=1` reference is121,896 B.

| Same frozen control | Complete bytes | Status |
| --- | ---: | --- |
| Current public encoder | 170,156 | Independently timed in the coding-group round |
| Existing skipped LEFT/group plan | 123,602 | Exact pixels; untimed probe only |
| Frozen reference, method4/quality75 | 121,896 | Unchanged baseline |

There are24,348 literal misses, below the current25% early-out threshold of33,216.
Cache symbols still cost bits: low literal count does not prove prediction is
unhelpful. The skipped LEFT plan has a151,758-byte complete upper bound; considering
the existing spatial-group selector yields the smaller emitted probe above.
Current low-literal RAW plans intentionally omit body-cost calculation because
they never enter that comparison; their zero `BodyBits` is not a valid estimate.

The native packet uses subtract-green, spatial prediction, cross-color, a two-bit
cache and nine spatial code groups. Production uses subtract-green, a ten-bit
cache and one group on this input. These observations identify distinct paths;
no native map, coefficients or implementation are imported.

The source is the already measured original RGBA8 RGB with alpha set to255.
Input hash, settings and reference bytes remain frozen; no image or settings grid
is added. Managed and normal/scalar FFmpeg decoding preserves every sample.
One verified immutable production build is reused, with zero rebuilds or scores.
The current packet's0.182 conservative time bound cannot qualify the probe.

Next: a bounded eligibility change using complete RAW coding cost alongside the
existing literal-heavy rule, retaining complete-packet selection, admission,
cancellation and all size/time gates. No performance or allocation claim is made
for that future change.

[Numeric evidence](vp8l-opaque-cost-audit-evidence.json) includes identities and
parsed format observations. Mirrored raw source/scripts, packets and checks:
`c48-opaque-cost-audit.tar.gz`, SHA256
`41f15b531b4b5e88e72f9659efdb2746fb72ccdcd3fdbeb6d4c7ea1137dd7c3a`.
