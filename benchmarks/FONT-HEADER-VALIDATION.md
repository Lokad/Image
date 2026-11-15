# Font header validation and complete text workloads

The inlined validator reads four signed bounds from one eight-byte big-endian word.
The ten-byte guard, zero-contour shortcut, exact bounds predicate, eager validation,
per-glyph cancellation, owned copy and cleanup remain. Independent complete loading
saves **3.30 µs / 11.99% raw**, **10.46% reference-normalized**;
filtered/all-actual raw and conservative normalized intervals separate. Reference
time moves -1.72%. This is a loading gain, not an overall text speedup.

| Frozen public operation / explicitly labeled reference | Lokad µs | Reference µs | Ratio / upper | Managed GC B/op |
| --- | ---: | ---: | ---: | ---: |
| DejaVu Sans Regular load/dispose / FreeType2.13.2 | 24.24 | 13.31 | 1.822 / 1.830; gap | 176 |
| Complete one-shot / ImageSharp public integration | 1,197.20 | 6,148.87 | 0.195 / 0.221; pass | 283,276 |
| Complete preparation + eight draws / ImageSharp public integration | 1,888.95 | 6,905.47 | 0.274 / 0.308; pass | 284,626 |

One-shot includes font load, all30 fitting trials/winner layout, ordinary drawing,
fresh canvas/explicit clear, independent ownership and disposal. Repeated use prepares
once, releases font/layout, draws eight independent fresh cleared canvases and disposes
preparation. Direct batch/8 is **236.12 / 863.18 µs per draw**.
Creation-only preparation is **1,076.17 / 6,042.93 µs**, with prepared-owner disposal outside
that diagnostic. Never sum stages. Earlier complete costs are superseded; no causal
pipeline gain/regression is established. Prepared-draw diagnostic is 96.99/75.07 µs,
upper2.287, uncertain; all adverse samples remain without a favorable repeat.

FreeType2.13.2 owned copy/open/retain/dispose validates lazily; managed loading eagerly
checks mapping/glyph headers. This is not general parser equivalence. Text uses
ImageSharp.Drawing2.1.7 / Fonts2.1.3 / ImageSharp3.1.12 with every public layout/fitting
call; prepared raster state differs. Full16-bit samples, exact glyph/cluster/line/
fitting/bounds checks, frozen frames and approved visual gates pass. No new timing grid.

Allocation/capacity/retention are separate: font757076B+256Bdescriptor, prepared39904B,
no retained font/layout after preparation, governed peak807708B. Default idle storage
stays within two arrays/4MiB; caller roots bypass it. Heap deltas are approximate,
excluding idle pools; physical/native/JIT peaks are unmeasured. No memory saving claim.

The constructor shrinks3325→3161B; code size and the preceding1996-location profile
are attribution, not timing proof. Public signed-bound tests cover short/long locations,
extremes, reversed bounds, zero contours, input independence and failure recovery.
Each OS passes **6714 full/live checks**,258internal/161public focused normal/scalar
checks and the **same actual Release package/all three examples**. One core/embedded/
public build perOS, existing workers/fonts reused, one pack, no post-timing core build.
Quiet singleCPU0, natural GC, all12actuals/reference brackets and idle-v2 guards remain.
Unchanged kernels reuse evidence; published releases and publication postponement remain.

[Evidence](font-header-validation-evidence.json) records sources, all observations,
word-model proof and qualification. Raw font-header-bounds-qualified.tar.xz, 5,353,860B /202members,
SHA256 09dcc1a72097f9469a7ca341fcecc14e2e550ce868745a7a4b71b2a965f944ac, is mirrored/member-verified.
[Previous header stage](https://github.com/lokad/image/blob/5bfc4364f66053ac4f1096f098822de13f223b1a/benchmarks/FONT-HEADER-VALIDATION.md)
and its cited raw archive retain history. The full289-path objective remains open.
