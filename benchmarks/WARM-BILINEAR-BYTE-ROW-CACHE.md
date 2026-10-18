# RGBA8 bilinear row reuse

The existing byte bilinear reduction is **23.7% faster**, from 656.29 to 500.71 µs,
remaining a **qualified three-actual gap: 1.291× / upper 1.406×**. It does not close
the independent warm-parity target. Other 206 comparisons/all 58 passing proofs
retain their hot paths and evidence.

| Existing selector | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Down8 | 500.71 | 387.70 | 1.291 / 1.406 | 157,960 |
| Down16 | 541.18 | 416.46 | 1.299 / 1.772 | 290,127 |
| Up8 | 3,139.26 | 1,994.39 | 1.574 / 1.666 | 2,235,599 |

Down16 and Up8 are unchanged controls; their managed means are within 1.3% of
retained results. All brackets/actuals are retained, including the faster current
Up8 native bracket. These control timings do not replace their selected evidence.
Native allocations remain unmeasured; B/op includes harness amortization.

The two original 513×259 RGBA8/16 rasters and established 256×129 reduction /
1027×519 enlargement are unchanged. Complete public Resize includes admission,
geometry, filtering, fresh independent output and disposal. The existing FFmpeg
zscale triangle graph includes widened reduction support, packed/planar conversion,
encoded-space premultiplied-alpha filtering/unpremultiplication, output acquisition
and release. Native contexts may be warm; each call filters the complete input.
No decoded output is cached. Startup, IPC, I/O and validation are excluded.

A separate byte specialization adapts the existing owned word-cache strategy:
six-column strips retain up to ten unrounded horizontal rows within 3,640 bytes
of fixed arrays, admitted as 4 KiB after existing geometry. Scales≤2.5 and supported
SIMD are required. Bounded 8-byte/two-pixel and 4-byte/tail reads produce exact
signed 32-bit premultiplied samples before binary64 row reuse. No heap workspace,
persistent cache or output alias is added; original work admission/charges and
bounded cancellation remain.

The established 2^-40*16^2 error budget, normalizers≥1 and alpha above the tiny-alpha
boundary permit conservative RGB 1e-4 / alpha 1e-6 rounding margins. Uncertain
ordinary-alpha pixels replay original tap order. Tiny alpha declines and unwinds
before exact scratch. Byte packing and clipping retain exact pixels. Exact
rational-reference RGBA, source bytes and independent owners pass. Down8 native
alpha remains **exact**, RGB max1/RMSE≤0.25. Existing bounded native alpha for the
two controls is unchanged; no baseline, tolerance or quality adjustment is added.

Removing the new byte dispatch restores the entire prior PreparedResize source.
The word cache file is byte-identical, and word reduction/both enlargements return
before the new branch. Other filters, hardware and budgets retain their hot paths;
other codecs, metadata and editing are unchanged. Only Down8 changes its selected
result. Six authored cases cover geometry/tails, source/owners, exact work, tight
working limits, cancellation, rounding halves and tiny-alpha fallback.

One immutable build serves the primary and two controls, in batches ≤3. All three
200 ms actuals, 50 ms native pre/post samples and both 5 s quiet guards pass. Means
were read before full qualification. Since the primary remains 1.291×, no routine
12-actual confirmation or profile is added. The prepared driver, worker, harness
and fixtures are reused without rebuilding them during measurement.

Windows/Linux each pass **3,678 Release cases** (1,357 internal, 1,423 public,
898 live FFmpeg), plus **132 scalar controls**. The same actual Release package
passes its isolated consumer and all three examples on both OSes, with matching
symbols. Measured production bytes survive qualification unchanged. Unaffected
qualification is reused; published 0.1.0 packages and tag remain intact.

[Numeric evidence](warm-bilinear-byte-row-cache-evidence.json) retains the three
screens, allocations, brackets, guards, source proof and package qualification.
The [current summary](../docs/benchmark.md) replaces only Down8; full 207-case
parity remains open at 58 confirmed passes, 145 measured gaps and 4 confidence cases.
