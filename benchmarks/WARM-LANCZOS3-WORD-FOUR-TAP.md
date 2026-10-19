# Exact four-tap word horizontal filtering

Four-tap register groups cut complete RGBA16 Lanczos3 reduction time **26.9%**
and enlargement **6.4%** from the preceding accepted build.
Both remain measured gaps against independently confirmed warm single-CPU parity.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce16 | 967.31 | 506.54 | 1.910 / 2.223 | 146,637 | 26.9% |
| Enlarge16 | 6,851.27 | 3,924.00 | 1.746 / 1.852 | 4,617,090 | 6.4% |

One bounded 32-byte input read supplies four RGBA16 pixels. SIMD premultiplication
is exact in UInt32; unsigned extension precedes two eight-lane Int64 weighted
products. Four tap subsets and their disjoint recombination each obey
`abs(sum) <= 65535² * sum(abs(weights)) <= 65535² * 2³¹ < 2⁶³`.
The remaining zero to three taps retain one-pixel arithmetic. The typed word loop
hoists depth dispatch; generated code retains scalar weight broadcasts/vector
inserts and adds no coefficient table or converted-input row.

Axis plans, byte helper, vertical/normalization arithmetic, scratch, owners and
admission remain unchanged. Work still admits each complete window before reads;
four-tap steps visit every old 1024-tap cancellation boundary. The independent
BigInteger row theory now also covers 1031→2 at both depths, crossing that poll
and exercising a three-tap tail. All four original full frames remain exact on
both systems. No extra array, retained cache, work charge or budget fallback.

Two frozen-producer shorts use the original fixtures, worker, quality proofs,
harness and compiled selectors. All four quiet guards pass. Reference is
**zimg, FFmpeg's zscale backend**, with the complete equivalent operation from
the [baseline](WARM-LANCZOS3-BASELINE.md). Both native brackets and selected/all-actual
confidence views are retained. No long confirmation above one, repeated profile,
new raster or settings grid.

Qualification: **4,448 Release tests per OS** (1,635 internal, 1,881 public,
932 live FFmpeg), **47 Lanczos controls with all intrinsics disabled** and
**47 with AVX512F disabled per OS**, the same actual private 0.2.0 package consumer
and all three examples on Windows/Linux. Twelve font comparisons reuse unchanged
paths. Published 0.1.0 stays immutable.

Only two word rows refresh. The [two byte rows](WARM-LANCZOS3-WIDE-VERTICAL.md)
retain their identified build: the unchanged byte helper returns before new word
dispatch; axes, vertical/normalization, owners and operation boundaries are unchanged.
Other 227 rows retain their historical scope. Parity and replacement readiness
remain open. Next: byte-depth four-tap signed32 products, Reduce8 first (2.360×),
with exact subset/poll/work/storage proofs before implementation.

[Numeric evidence](warm-lanczos3-word-four-tap-evidence.json) includes samples,
allocations, identities and bounds. Qualified source: `15f265d8a7308468ac193379a713a8814d23d06a3d180c256d5c1c1511765430`.
Raw `l3t-qualified-windows.tar.gz`: `299f2229852aa8a19c6135e78c057651e973c707ecaf643e7262920ef9e4e529`;
`l3t-qualified-linux.tar.gz`: `2f738e1a9705a60bdde780781f150f3ea375b324998af882faa5ca2f6395ca82`.
Unchanged dependencies, original fixtures and worker remain in the baseline archive.
