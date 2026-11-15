# Font header validation and complete text workloads

Glyph scanning prepares immutable count/width/extent values and the borrowed token
once, advances a bounded location offset and reads each 2/4-byte field exactly.
Every per-glyph cancellation poll, exact header check, eager validation, owned copy
and cleanup remain. The borrowed budget is unchanged. Independent complete loading
saves **2.19 µs / 8.98% raw**, **3.71% reference-normalized**;
filtered/all-actual raw and conservative normalized intervals separate. Reference
time moves -5.48%. This is a loading gain, not an overall text speedup.

| Frozen public operation / explicitly labeled reference | Lokad µs | Reference µs | Ratio / upper | Managed GC B/op |
| --- | ---: | ---: | ---: | ---: |
| DejaVu Sans Regular load/dispose / FreeType 2.13.2 | 22.16 | 13.22 | 1.677 / 1.695; gap | 176 |
| Complete one-shot / ImageSharp public integration | 1,199.51 | 6,180.56 | 0.194 / 0.221; pass | 283,276 |
| Complete preparation + eight draws / ImageSharp public integration | 1,874.29 | 6,825.84 | 0.275 / 0.298; pass | 284,626 |

One-shot includes font load, all 30 fitting trials/winner layout, ordinary drawing,
fresh canvas/explicit clear, independent ownership and disposal. Repeated use prepares
once, releases font/layout, draws eight independent fresh cleared canvases and disposes
preparation. Direct batch/8 is **234.29 / 853.23 µs per draw**.
Creation-only preparation is **1,091.01 / 6,006.52 µs**, with prepared-owner disposal outside
that diagnostic. Never sum stages. Earlier complete costs are superseded; no causal
pipeline gain/regression is established. Prepared-draw diagnostic is 97.03/73.72 µs,
upper3.561, uncertain; all adverse samples remain without a favorable repeat.

FreeType 2.13.2 owned copy/open/retain/dispose validates lazily; managed loading eagerly
checks mapping/glyph headers. This is not general parser equivalence. Text uses
ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12 with every public layout/fitting
call; prepared raster state differs. Full 16-bit samples, exact glyph/cluster/line/
fitting/bounds checks, frozen frames and approved visual gates pass. No new timing grid.

Allocation/capacity/retention are separate: font 757,076 B + 256 B descriptor,
prepared 39,904 B, no retained font/layout after preparation, governed peak 807,708 B.
Default idle storage stays within two arrays/4 MiB; caller roots bypass it. Heap deltas are approximate,
excluding idle pools; physical/native/JIT peaks are unmeasured. No memory saving claim.

The constructor changes 3,161→3,169 B. Repeated field reads/multiplication disappear;
code size and generated instructions are not timing proof. The offset remains
(i+1)*width, with final value ≤262,144; the existing loca extent check bounds all reads.
Public signed-bound tests cover short/long locations,
extremes, reversed bounds, zero contours, input independence and failure recovery.
Each OS passes **6,714 full/live checks**, 258 internal/161 public focused normal/scalar
checks and the **same actual Release package/all three examples**. One core/embedded
build per OS; unchanged public/harness/native/font modules are
reused. One pack, no post-timing core build, no new timing matrix.
Quiet CPU0, natural GC, all 12 actuals/reference brackets and idle-v2 guards remain.
Unchanged kernels reuse evidence; published releases and publication postponement remain.

[Evidence](font-header-validation-evidence.json) records sources, all observations,
scan-equivalence proof and qualification. Raw font-location-preparation-qualified.tar.xz, 3,490,576 B /164 members,
SHA256 2d429199f877b26b63b7192e6d54b91265d4689f46c65d784279a08d7eb4b722, is mirrored/member-verified.
[Previous bounded-header stage](https://github.com/lokad/image/blob/c9e0486894d4b3d02224ff45b6a6136a5450f992/benchmarks/FONT-HEADER-VALIDATION.md)
and its cited raw archive retain history. The full 289-path objective remains open.
