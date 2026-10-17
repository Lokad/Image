# Exact byte bilinear factorization

Updated 2026-10-18. The existing RGBA8 enlargement is **16.8% faster**, with
unchanged exact pixels and allocation. It remains a **qualified three-actual
gap** against the warm single-CPU parity target. All actuals and both quiet
guards are retained; no twelve-actual confirmation is run for a demonstrated gap.

| Existing case | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 bilinear, 513×259 → 1027×519 | 3,138.47 | 2,155.65 | 1.456 / 1.549 | 2,235,599 |

The complete public Resize operation includes admission, geometry, filtering,
fresh independently owned output and disposal. The unchanged public FFmpeg
zscale triangle graph includes packed/planar conversion, encoded-space alpha
premultiplication/unpremultiplication, output acquisition/release and timed
rounded-zero-alpha RGB clearing. Graph contexts may be warm; each operation
filters the full original input. No decoded output is cached. Startup, IPC, disk
I/O, preparation, hashing and validation are excluded. Native allocation is
unmeasured; managed B/op includes harness amortization.

One primitive factors horizontal premultiplied sums in signed 32-bit integers
before widening for two vertical rows, reducing eight wide SIMD multiplications
to four per four pixels. The guard `65025 * (2 * outputWidth) <= int.MaxValue`
permits widths through 16,512; larger outputs retain the original four taps.
Integer distributivity preserves every final sum below 2^47. Existing division,
error bounds, quantization, original-order replay, tiny-alpha work/memory,
bounded reads, tails, cancellation and output ownership stay unchanged. No
workspace, heap state, reservation or work charge is added.

Only the existing **Up8** selector changes. Other byte reduction, word bilinear,
bicubic, nearest, exact edits, codecs and metadata retain their executed paths:
**206 comparisons and all 58 passing proofs are reused**. Original managed and
native output hashes, exact rational managed pixels, source preservation and
independent owners pass. The existing native-only RGB/alpha maximum 1 and RMSE
≤0.25 bounds remain; no baseline, quality or boundary policy changes.

One immutable producer/harness/worker/fixture build serves the single short
screen. The retained generated selector is reused without rebuilding; the mean
is read before qualification. All **3,659 Release tests per OS** pass (1,349
internal, 1,412 public, 898 live FFmpeg), plus 132 scalar bilinear/reference
checks per OS. Three authored correctness facts cover exact integer lanes,
maximal products, the width guard, clamped edges, tails, tiny alpha, source and
owners. The two public facts also pass on the predecessor package. These are
correctness inputs, with no new performance raster or settings grid. The same
actual private package passes its consumer and all three examples on Windows
and Linux, with matching symbols. The screened Linux production bytes survive
qualification unchanged; published 0.1.0 packages and tag remain intact.

[Numeric evidence](warm-bilinear-byte-factorization-evidence.json) retains full
statistics, receipts, identities, scope and qualification. Raw archive
`parity-p37-qualified-evidence.tar.gz`, SHA256
`55004f222a8392ef87e6ce0560f7c606a1e09570c251d23a97c753194651b765`.
