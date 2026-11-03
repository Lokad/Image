# Sparse WebP clustering storage

Immutable tile populations now use packed sparse entries. Only merged clusters
need dense arrays; an operation recycles retired arrays and can transfer a dead
cluster's buffer to its surviving logical identity. Two bounded decode buffers
serve existing dense kernels. Original leaves no longer need token reconstruction
during final remapping. Candidate order, costs, ties and complete packets remain
exact. There is no cross-operation pool, cache or public API change.

| Complete resident encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original RGBA8 / libwebp exact=1 | 128,500.88 | 122,922.14 | 1.045 / 1.068 | 7,281,824 | 183,812 |
| Opaque RGBA8 / libwebp exact=1 | 75,206.46 | 69,709.03 | 1.079 / 1.092 | 5,983,408 | 121,896 |
| Photograph / libwebp exact=1 | 95,779.05 | 83,539.55 | 1.147 / 1.208 | 6,517,238 | 160,032 |
| Text/diagram / libwebp exact=1 | 8,545.82 | 10,074.23 | 0.848 / 0.864 | 4,461,854 | 7,234 |
| Transparent VP8 RGBA8 / FFmpeg | 65,561.30 | 53,420.09 | 1.227 / 1.278 | 9,594,864 | 98,590 |
| Transparent VP8 RGBA16 / FFmpeg | 64,762.39 | 53,504.39 | 1.210 / 1.230 | 9,594,864 | 98,590 |

Independent fresh baseline/candidate confirmations show the original control
saving **5.07 ms / 3.79% raw**, with separated filtered/all-actual raw confidence.
Its normalized point improves 3.62%, but conservative intervals overlap. Allocated
bytes fall **24,899,424 → 7,281,824**, saving 17.62 MB/encode. Diagram still passes
and stops; the remaining five controls retain CPU gaps.

Photograph's **4.20 ms / 4.58% adverse raw point** has overlapping raw and normalized
confidence. Late actuals vary; all are retained. There is no photograph speedup or
confirmed causal regression claim. Its allocation falls **13,346,868 → 6,517,238 B**.
Companion rows refresh costs; historical differences are not incremental CPU
claims. Short screens guide the staged design, not these conclusions.

| Separate complete-operation storage diagnostic | Allocated B, before → after | Admitted peak B, before → after |
| --- | ---: | ---: |
| Photograph | 13,346,256 → 6,516,728 | 10,166,464 → 3,470,059 |
| Text/diagram | 6,255,920 → 4,460,936 | 4,802,292 → 3,386,043 |
| Transparent VP8 RGBA8 | 10,747,848 → 9,593,640 | 3,847,888 → 3,847,888 |
| Authored bars/mixed-alpha patch | 933,440 → 922,904 | 312,716 → 312,716 |

These governed high-water charges include overlapping capacity and exclude
borrowed inputs. They are not physical heap peaks. Scratch retention is zero;
native allocations and original/opaque governed peaks remain unmeasured.
Diagnostic allocation counts differ slightly from aggregate BDN counters; they
are separate observations. Output ownership remains independent.

The audit found 2,142 photograph leaves but only 28 live merged clusters at peak;
original and opaque peak at 34 and 31. Sorted sparse leaf/pair costs exactly match
the dense oracle. Format-bounded 12-bit indices and 20-bit immutable leaf counts
fit UInt32; merged counts remain Int32. Two passes allocate exact nonzero capacity.
Temporary row indexes end after preparation. Every substantial capacity is admitted
before allocation and remains charged until its actual users finish; refusal is
terminal, with no retry or fallback.

The first storage implementation materially regressed time, especially empty
diagram tiles. A second stage skips empty tiles; the completed stage clears and
prices touched symbols using idle preparation storage. Both adverse intermediate
stages, their binaries and all samples are retained. Earlier lazy predictor-price
caching was restored after flat/adverse screens and is retained separately; it
is not part of this implementation.

Five owner tests cover immutable originals, physical transfer/recycling, counts
beyond 65,535, refusal cleanup and subsequent use, exact sparse run costs and
cancellation. Windows/Linux each pass **5,502 full/live checks**, plus 300 internal
and 194 public WebP controls per normal/scalar mode. The **same actual Release
package** and all three examples pass both OSes, including sync/async exact
packets, hidden RGB, shared admission and surviving outputs.

One immutable production/internal build per OS serves the completed stage.
Existing fixtures, harnesses, selectors and native workers are reused. Eight
confirmation jobs retain twelve actuals each and native before/after brackets,
with quiet guards. A receipt assertion and a zero-actual setup failure were fixed
without replaying completed measurements. Complete preparation, selection,
normalization and independent output ownership remain timed. Libwebp exact=1 is
explicitly FFmpeg's encoder backend.

Other adaptive VP8L/transparent ALPH costs remain historical until refreshed.
Opaque VP8 without alpha, other codecs, reading, resizing and text paths reuse
unchanged evidence. This is development qualification; full 282-path parity is
open and publication postponed.

[Evidence](webp-sparse-clustering-workspace-evidence.json) pins exact producers,
all observations, intermediate stages, storage diagnostics and package proof.
The mirrored, member-verified raw archive and its audit/prior-production
dependencies are identified there; superseded experiments remain historical.
Raw `webp-sparse-clustering-workspace.tar.xz`: 33,997,616 B / 1,617 members,
SHA256 `54faf8e9b3a882676f5d4f8e1fbe1dcdca3b1a191d3ae4b9ea85171b00b3256e`.
