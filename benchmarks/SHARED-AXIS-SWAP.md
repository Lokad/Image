# Shared axis-swapping tiles

All eight original byte/word transpose and quarter-turn controls pass warm
single-CPU parity. Mirrored byte transpose independently saves **10.00 µs /
23.76% raw**, **21.24% reference-normalized**, against a contemporary
**42.07 / FFmpeg 40.80 µs** baseline. Both raw confidence views and normalized
confidence separate; the native clock moves **−3.20%**. Pixels remain exact.

| Complete original 513×259 → 259×513 operation | Lokad / FFmpeg µs | Upper | GC B/op |
| --- | ---: | ---: | ---: |
| Transpose RGBA8 | 27.65 / 37.89 | 0.740 | 112 |
| Transpose RGBA16 | 34.25 / 43.85 | 0.859 | 112 |
| Counterclockwise RGBA8 | 29.27 / 37.36 | 0.797 | 112 |
| Counterclockwise RGBA16 | 36.97 / 44.33 | 0.910 | 112 |
| Clockwise RGBA8 | 32.59 / 41.40 | 0.800 | 112 |
| Clockwise RGBA16 | 40.13 / 45.75 | 0.885 | 112 |
| Mirrored transpose RGBA8 | 32.07 / 39.50 | 0.826 | 112 |
| Mirrored transpose RGBA16 | 45.22 / 46.87 | 0.996 | 112 |

One ownership entry and shared 32×32 tile schedule now serve transpose, clockwise,
counterclockwise and mirrored transpose. Existing SSE2 UInt32/AVX2 UInt64 4×4
matrices keep their forward-column stores; clockwise/mirrored output additionally
reverses four complete pixel lanes. Closed row/column order selects placement
without a virtual pixel-loop policy. The real scalar and edge paths remain.
Three duplicate production classes are deleted: 81 fewer production lines.
No channel/alpha interpretation, numeric-policy change or intermediate pass.

The cheap original twelve-pair screen precedes independent primary confirmation.
The same immutable build supplies all eight current costs, including four protected
passes. Every actual pair and three native brackets is retained; original input,
exact normal/scalar native pixels, independent outputs and CPU0 idle-v2 gates pass.
Historical companion points move in both directions and their normalized bounds
overlap: they establish no causal companion gains or regressions. All eight pass
and stop. There is no new benchmark image, geometry or settings grid.

Complete public admission, output allocation, materialization and disposal stay
timed. FFmpeg's public transpose graph retains source/context/frame wrappers;
independent pixel allocation, processing and release are timed. Padded native rows
are exposed directly without a redundant packing copy. Startup, transport, I/O,
fixture generation, hashing and validation are excluded as declared.

Allocation stays **112 B/op**. Source and output each hold 531,468 B for byte or
1,062,936 B for word samples. No new owned/admitted capacity, scratch, cache or
retained operation state is introduced. The unchanged two-slot/4 MiB idle store
is bypassed for caller-governed storage. Consumer peaks are 15,540 / 31,080 B for
source plus two retained byte/word outputs. Physical/native/JIT peaks are unmeasured.

**6,600 full/live checks per OS** pass: 3,241 internal / 2,411 public / 948 live.
Focused 13 internal / 268 public checks pass normal/scalar, and all 268 public
checks pass with AVX2 disabled. Eight additional public cases exercise complete
pixel bits, hidden RGB, six matrix/tile boundary geometries, cancellation after
admission, terminal refusal and surviving output ownership. Existing EXIF and
meaningful editing coverage remain. Encoder packets, decoder pixels and exact
resizing stay correct; unchanged codec/resize/text/other-ISA evidence is reused.

The **same actual Release package**, containing the timed Linux DLL, passes both
OS consumers and all three examples. Its existing transpose consumer is generalized
to all eight operations/depths, including exact inverse recovery. The dispatcher
remains 243 native bytes with identical instructions/constants except the four
intentional helper targets; normal/flip/half-turn evidence stays valid.
One core/embedded/public build per OS, no harness/native/fixture build, one
Release/--no-build pack, and no production rebuild after timing. Published packages
remain immutable; publication is postponed.

Current image views: **172 pass / 5 gaps / 14 uncertain** across 191 comparisons;
the 20 independent-result edits have 13 passes / four gaps / three uncertainties.
All 289 material paths remain required. This is the tested control scope, with
no whole-library or arbitrary-size performance claim.

[Evidence](shared-axis-swap-evidence.json) records sources, immutable identities,
complete boundaries, all samples/confidence, memory and qualification. Raw
`shared-axis-swap-qualified.tar.xz`, SHA256
`95f7e1217518a1eef153c90c46254fb9457fbfa26d4b7f0e319709c0452eaefb` (2,840,432 B / 184 members), is mirrored
and member-verified. Its dependency retains the preceding qualified package,
[copy stage](SHARED-RASTER-COPY.md), [earlier tile stage](SHARED-TRANSPOSE-TILES.md)
and original reference inputs/recipes.
