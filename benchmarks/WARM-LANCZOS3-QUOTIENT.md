# Exact Lanczos3 quotient normalization

One normalization primitive reduces complete Lanczos3 time **25.0–39.1%** on
the four existing cases. All remain measured gaps against the independently
confirmed ≤1× target; no long confirmation was run above one.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 5,547.79 | 489.71 | 11.329 / 12.268 | 146,614 | 25.0% |
| Reduce16 | 5,375.64 | 505.40 | 10.636 / 11.042 | 146,614 | 27.5% |
| Enlarge8 | 48,128.04 | 4,056.92 | 11.863 / 12.360 | 351,966 | 35.3% |
| Enlarge16 | 45,560.36 | 3,924.95 | 11.608 / 12.572 | 4,616,094 | 39.1% |

After the unchanged sign/saturation guards, dropping 32 low bits provides a
cheap quotient estimate. The candidate stays in the sample range, keeping its
product within the original arithmetic bounds. An **exact Int128 remainder**
certifies the integer quotient; one-step correction is accepted only when the
remainder lies in `[0, denominator)`. Otherwise the original exact division
remains. Final half rounding uses integer arithmetic. No approximate pixel,
buffer, allocation, admission, work or cancellation policy was introduced.
Tiny per-operation allocation differences reflect amortized diagnostic overhead.

Five new checks cover exact integers/halves ±1 beyond binary64 precision,
deterministic wide fractions, saturation and zero/misleading projections. All
four full outputs match the retained independent oracle on both OS. Native
worker, reference pixels, RGB/alpha bounds, harness and generated selectors
are unchanged. Complete operation boundaries and single-CPU settings remain
those of the [baseline](WARM-LANCZOS3-BASELINE.md).

The direct Int128-to-double candidate was screened first on Enlarge16 and
retained as an unselected control. The cheaper projection was then screened on
the same case and the other three affected paths. One Enlarge8 preguard failed
I/O-wait admission and launched no benchmark; the fresh retry passed both guards.
All four scored shorts pass eight versioned guards and retain both native
brackets plus selected/all-actual confidence views. No other case was retuned.

Qualification: **4,431 Release tests per OS** (1,618 internal, 1,881 public and
932 live FFmpeg), **30 intrinsics-disabled checks per OS**, the same actual
private 0.2.0 package consumer and all three usage examples on Windows/Linux.
Published 0.1.0 is unchanged. Only these four timings refresh; the other 227
numeric rows retain their identified historical builds and qualification scope.

A bounded diagnostic profile no longer samples wide integer division; final
normalization and vertical/output processing dominate. The next substantial
experiment is a proven quotient-error interval for image arithmetic, with exact
arithmetic at uncertain boundaries. Profile timings are not scored.

[Numeric evidence](warm-lanczos3-quotient-evidence.json) includes all samples,
allocations, the unselected candidate, failed preguard, identities and reuse.
Qualified source: `47cb718d86548c686969a3b866095fb4620b1c046b4d5cd72456e38843c4b7a8`.
Raw `l3q2-qualified-windows.tar.gz`:
`2aee54273b722d01a8f38f1474024c77c7ac9f2ee8cfe008e3d96051e8280e73`;
`l3q2-qualified-linux.tar.gz`:
`aba281bfd65187a1f528f3ff525cbc56d5a625c7c792717a25b83f67f1acc405`.
Unchanged runtime dependencies, original fixtures and native worker remain in
the baseline archive. No replacement-readiness or parity claim follows.
