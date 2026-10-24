# Exact invariant color-transform costs

The existing 513×259 RGBA8 VP8L control retains Q75, method4 and raw-exact
183,812-byte packets, including hidden RGB and alpha. The reference is explicitly
labeled libwebp exact=1, FFmpeg's encoder backend. Complete operations include
fresh independent owned output acquisition/release. Inputs/settings are unchanged.

| Quiet one-CPU run | Lokad µs | libwebp µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| VP8L short | 302,762.69 | 121,583.05 | 2.490/2.980 | 48,060,256 |
| VP8L independent | 302,147.15 | 121,794.52 | 2.481/2.512 | 48,060,256 |

Independent confirmation uses 19.5% less elapsed than the
[preceding confirmation](WARM-VP8L-PAIRED-POPULATION.md). All 12 actual iterations
are retained and selected; both idle-v2 guards pass. Allocation increases by
4,144 B/op for the scratch array and owner fields.

Color-transform trials share fixed history and tile population. Preparing these
terms once permits exact integer score deltas for nonzero histogram bins.
Logarithms, rounding, residual bias, candidate order and tie decisions remain.
The existing search owner admits its full 7,296-byte capacity before allocation
and releases both arrays on all exits. Original work charges remain, with an
additional 512 units per tile for preparation. No image copy, independent memory
allowance, persistent cache or lower-memory admission fallback is introduced.

Both platforms pass 5,098 checks each, including 947 live comparisons, 38 scalar
color/prepared checks and 27 scalar public lossless packet checks. 65,536 bounded
histogram/history controls retain their frozen scalar costs. Native maps,
residuals, complete packets, capacity overlaps, refusal/cancellation cleanup and
recovery remain exact. The same actual private 0.2.0 package and all three examples
pass both platforms.

[Evidence](warm-vp8l-color-history-evidence.json) records samples, identities,
qualification and reuse. VP8's compressed-alpha path skips this color search;
its valid previous confirmation is reused. JPEG/PNG/decoders/editing/text are
unchanged. Claims are limited to this control; full 257-case CPU parity is open.
VP8 is now the largest measured encoder gap.
