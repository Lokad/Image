# Raster stride preparation trial

The trial computed immutable row stride once in the private `ImageBuffer`
constructor instead of on each property access. It is **not retained**: current
sources, production/embedded-test DLLs and the actual development package are
restored byte-for-byte. [Bilinear metadata](BILINEAR-OPERATION-METADATA.md) remains
the latest accepted production milestone and current benchmark evidence.

| Existing complete RGBA8 resize | Baseline / trial µs | Native baseline / trial µs | Interpretation |
| --- | ---: | ---: | --- |
| TightDown8 | 443.90 / 441.82 | 440.16 / 454.20 | Raw and normalized confidence overlap |
| Wide8 | 380.19 / 351.11 | 378.27 / 346.51 | Raw gain 29.08 µs / 7.65%; native improves 8.39%, normalized confidence overlaps |

Both filtered and all-actual Wide8 raw intervals separate. Its normalized point
is 0.82% adverse; neither normalized gain nor regression is established. The
TightDown8 trial passes its standalone ratio gate while its contemporary baseline
is uncertain, but this does not establish a causal improvement. No further
sampling is taken to obtain favorable bounds. These results do not replace
current costs, establish an overall raster speedup or prohibit revised designs.
Contemporary baseline ratio uppers are 1.022 / 1.014, both uncertain. The older
source-valid gap captures remain recorded, but this mixed evidence must be
resolved before assuming that either current path requires optimization.

All original rational/exact pixels, approved native bounds, input identities,
independent outputs, cancellation/admission and idle-v2 guards pass. Boundaries
remain complete public Resize versus the explicitly retained native zimg graph
and scratch, including required source/output handling. Three-actual screens
precede one independent 12-actual baseline/candidate confirmation per case.
Instrumented JIT output is diagnostic; no phase costs are inferred from it.

Allocation-only controls on Windows/Linux both measure 56 B per reflected
constructor call and 112 B per warmed public factory call, unchanged at either
depth and including identical reflection overhead. Public stride/row bounds,
disposed-image behavior and independent storage agree. Resize allocations remain
113 B/op for TightDown8 and approximately 198,450 B/op for Wide8; its 2 B counter
difference does not establish object growth. No substantial ownership/admission
capacity or pool changes. Physical/native/JIT peaks are unmeasured.

Each OS has one core/embedded prototype build and 640 internal / 893 public
focused checks per normal/scalar mode, including the three named storage suites.
After exact restoration, the existing 6,511 full/live checks and actual-package
consumer evidence remain valid. No broad qualification or package repeat is
needed for a rejected production change. No published package changes.

[Evidence](raster-stride-preparation-trial-evidence.json) retains the hypothesis,
both source states/modules, every actual, adverse results, footprint controls and
restoration proofs. Raw `raster-stride-preparation-trial.tar.xz`, SHA256
`c0f9756ff0b626e174c866aa61e6d59e71cf676a6e8d249fa4ccbc8335bd55bc`
(1,429,268 B / 102 members), is mirrored and member-verified; reproducibility
dependencies are pinned in the evidence. Rejected timings are not
renderer dependencies. Next investigate shared coefficient preparation and its
complete row/tail costs using the current bounded-byte attribution; sample or
instruction counts alone do not authorize a production change. All 289 material
paths remain required; publication stays postponed.
