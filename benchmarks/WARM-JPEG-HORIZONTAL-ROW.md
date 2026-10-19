# Reuse identical sparse JPEG output rows

The horizontal one-AC CMYK control has **10.0% less measured elapsed time**,
746.37→671.44µs, at **1.726x FFmpeg**. It remains a short-screen parity gap.
DC/general means are0.2%/1.2% higher; no statistical equivalence or timing gain
is claimed for those controls. Allocations remain about1.086MB/op.

| Complete owned CMYK decode | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op | JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| DC blocks | 528.15 / 378.54 | 1.395 / 1.454 | 1,085,663 | 10,910 / 531,468 |
| Horizontal one-AC | 671.44 / 388.99 | 1.726 / 1.853 | 1,085,662 | 17,346 / 531,468 |
| All63 AC | 3,503.88 / 3,112.05 | 1.126 / 1.180 | 1,085,522 | 347,676 / 531,468 |
| Two-dimensional sparse | 795.30 / 437.20 | 1.819 / 1.928 | 1,085,656 | 22,708 / 531,468 |

When all active frequencies have v=0, every frozen vertical factor equals the
same C(0). Evaluate one horizontal row with the original binary64 multiply/add
order and round/clamp, then copy its exact8bytes into each row. Safe AVX-512 and
scalar paths agree. No reassociation, FMA, reduced precision, tables or heap
workspace. The original4,096work charge still precedes writes. Other spectra
retain the original ordered2D sparse/general algorithms. Only the private owned
full-resolution CMYK sparse helper selects this shortcut. Common IDCT, pool cap,
live-memory/work admission, cancellation and owner lifetimes are unchanged.

The shortcut creates one concrete coverage omission: the retained2D sparse path
needs a representative that rejects horizontal reuse. Exactly one named case,
`JpegDecode/CmykAdobeSparse2DResident`, uses firsttwo zigzag AC=4 at(u=1,v=0) and
(u=0,v=1), on the SAME original513x259 projection, geometry and unit quantization.
Its analytical joint contribution is rounded once; normal/scalar native gates
remain maximum4/RMSE1.5 (observed max1/RMSE0.70911), with exact opaque alpha.
No new photograph, shape, depth, quality or sampling grid. It measures1.819x and
becomes the largest current gap; no optimization need was inferred before timing.
The finite scope is246=222numeric+24pending;70passes/151gaps/one confidence-open.

One immutable producer per OS is reused for all four screens. Existing three
packets, expected pixels, native worker and compiled selector are retained. One
narrow benchmark-harness build adds the control; the complete Apply method and
original input/output arrays/boundaries remain unchanged. All12actuals/outliers,
both idle-v2 guards and50ms native brackets remain. Above-one means do not get
long confirmations. Native still includes four components, black conversion,
fresh owned RGBA normalization and release. ICC stays off and EXIF opt-in.

Accepted qualification:4,197tests/OS (1,432internal/1,837public/928live FFmpeg),
61affected hardware-disabled checks/OS, and the SAME actual private Windows
package consumer/all three examples on both OS. Exact general-reference tests
cover every frequency, signed mixtures/quantizers/halves, 2D rejection, guards
and work/cancel failures. Three numeric rows are refreshed and one added;
all70unaffected passing proofs remain stopped. Released0.1.0 remains immutable.

[Numeric evidence](warm-jpeg-horizontal-row-evidence.json) includes source/build
mappings, allocations, gates, actuals and qualification. Raw on both hosts:

- `h10l-measurements.tar.gz` SHA`6ef227ca6a89fdf5e91c408b02a895d9162953417ab83a27f087cb53ff2e0799`.

- `h10l-qualified-windows.tar.gz` SHA`93d7a36311759e227ed16c765704fbd9b14872058b3da6c5a6a1a0b902d4191b`.

- `h10l-qualified-linux.tar.gz` SHA`68905ad91d69e0c594644920e8c68ac2bf72a56627d767e8936b7b7e01440f82`.
