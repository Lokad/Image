# Guarded bicubic geometry views

RGS680 removes repeated prepared-weight copies in the shared small/wide column
geometry reader. The candidate borrows a read-only result-footer view only when
the complete strip ends before the protected footer. Crossing prepared strips
keep a stack copy; unprepared tails keep original Kernel construction. Arithmetic,
tap order, certificates, repair, scheduling and ownership remain unchanged.

The guard matters: RGBA16 324×128→144×64 at exact2.25 scaling has a cached prefix
ending at64 and six-column suffix strips. The last prepared strip starts at130,
before geometry prefix132, but its final pixels134/135 overwrite footer weights
that exact repair could still read. Testing only the strip's start is insufficient.

| Complete existing operation | Original | Candidate | Allocated B/op, original / candidate |
| --- | ---: | ---: | ---: |
| Frozen RGBA8 →128×64, µs | 463.271 | 455.633 | 113 / 113 |
| Existing large default, ms | 26.126 | 25.977 | 1,585,375 / 1,585,375 |
| Existing large compact, ms | 31.816 | 31.541 | 148 / 154 |

Each point has three actuals on the quiet VM, using contemporaneous immutable
Windows-produced DLLs. Frozen input remains513×259; the existing large raster
remains4097×2051→1024×512. Preparation, working storage, independent output and
disposal stay timed. The small comparison retains its labeled zimg brackets
(351.31/349.20µs); large comparisons are managed-only. All exact/native/source/
ownership/idle gates pass. Small aggregate allocation differences are amortization,
not intrinsic storage changes. Large admitted peaks remain3,682,592/2,101,248 B;
result2,097,152 B and4KiB compact workspace remain independently accounted.

The small point saves7.64µs/1.65%raw/1.05%normalized. Large compact saves0.276ms/
0.87%, while unchanged default also moves0.149ms/0.57%. These do not establish
a useful robust redesign, CPU gain/regression/equivalence or native parity.
No independent confirmation or favorable repeat follows. The implementation is
unselected, without prohibiting materially revised preparation/execution designs.

Candidate Windows/Linux focused231internal/703public normal/scalar checks pass,
including a dirty/overlapping-view test. Production sources, main modules and
public bindings are restored exactly. The view-only API/test is removed; the
real crossing-strip exact-pixel case remains against production. Its33focused
normal/scalar checks pass each OS. Only the embedded test assemblies rebuild.
Unchanged production reuses6,026full/live checks and SAMEactual Release-package/
three-example proof from [Huffman workspace](WEBP-HUFFMAN-WORKSPACE.md), without
another broad suite or package proof. Current displayed costs remain unchanged.

[Evidence](bicubic-guarded-geometry-views-evidence.json) retains every actual,
source/module, lifetime proof and restoration. Mirrored/member-verified raw
`bicubic-guarded-geometry-views-experiment.tar.xz` has12,308,428 B /230members,
SHA256 `ed43a65e88f8a76a1d52633f923048da5d098fd02edcea7753155b45304505ef`.
No benchmark renderer dependency is added. Next attribute the more expensive
horizontal execution before another redesign; preserve exact pixels and the full
283-path objective. Publication remains postponed.
