# Exact four-tap byte horizontal filtering

Four-tap register groups cut complete RGBA8 Lanczos3 reduction time **16.7%**
and enlargement **2.7%** from the preceding byte build.
Both remain measured gaps against independently confirmed warm single-CPU parity.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 963.93 | 490.45 | 1.965 / 2.193 | 146,655 | 16.7% |
| Enlarge8 | 5,856.16 | 4,075.64 | 1.437 / 1.523 | 351,910 | 2.7% |

One bounded 16-byte read supplies four RGBA8 pixels. Premultiplication is at most
65025; unsigned widening places each positive sample in an even Int32 lane.
The corresponding weight lane is the original signed32 coefficient. AVX512F
therefore produces eight exact signed32×32→64 products for each tap pair.
Every subset prefix and disjoint recombination is bounded by `65025 * 2³¹ < 2⁴⁷`.
Zero to three tail taps and other hardware retain the accepted AVX2 path.

Axis plans, word helper, vertical/normalization, scratch, owners, admission and
per-window work are unchanged. Four-tap steps visit every old 1024-tap poll;
the existing 1031→2 independent BigInteger control covers that boundary and a
three-tap tail. Four original full frames remain exact on both OS. No new array,
cache, work charge or budget fallback.

Two immutable-producer shorts reuse fixtures, native quality proofs, worker,
harness and compiled selectors. All four scored quiet guards pass. Reference is
**zimg, FFmpeg's zscale backend**, with the complete equivalent boundary from the
[baseline](WARM-LANCZOS3-BASELINE.md). Both native brackets and selected/all-actual
confidence views remain. No long confirmation above one or image/settings grid.

Qualification: **4,448 Release tests per OS** (1,635 internal, 1,881 public,
932 live FFmpeg), **47 Lanczos checks with all intrinsics disabled** and another
**47 with AVX512F disabled per OS**, same actual private 0.2.0 package consumer and
all three examples on Windows/Linux. Twelve font proofs reuse unchanged paths.
Published 0.1.0 stays immutable. Only two byte rows refresh; unchanged word paths
retain their identified [four-tap build](WARM-LANCZOS3-WORD-FOUR-TAP.md).
Other 227 rows retain historical scope. Parity and replacement readiness stay open.

One bounded current Reduce8 diagnostic has 1,445 CPU samples: **46.4% horizontal**,
22.4% vertical, 13.8% application/output and 6.7% math library. These are sample
fractions, not scored timings. Both diagnostic quiet guards and target/ownership
checks pass; collector status -15 is retained separately from successful target
completion. An earlier preguard refused before launch. Postguard precedes map/
sample formatting. The older word profile predates changed horizontal algorithms.
Next: a packed four-coefficient load and register permutations to reduce repeated
scalar broadcasts/vector inserts, with exact lane/work/poll/storage proof first.

[Numeric evidence](warm-lanczos3-byte-four-tap-evidence.json) retains samples,
allocations, identities, bounds and a sanitized diagnostic. Qualified source:
`1639b1f801b5d6080e50ce593bf6725e60ab44ab10dd9eecfc25af44a2303806`. Raw `l3u-qualified-windows.tar.gz`:
`595cde987de9a373ed39e8f8930e37a3eb71fee9b362f6f31dc319fd8409eef3`; `l3u-qualified-linux.tar.gz`: `eb87c77fed906f6c8fe45c940d413bb6eb8e50896bfd5061e5b0220ff35201cf`.
Unchanged dependencies, fixtures and worker remain in the baseline archive.
