# Text and drawing allocation controls

The nine existing controls now have stable allocation-only observations on the
quiet VM and frozen current Linux DLL. Three samples of sixteen complete
operations agree exactly after four warmups. Both idle/process-v2 guards pass.
There are **no elapsed-time samples, native comparisons or CPU parity claims**.

| Existing control | Allocated B/op |
|---|---:|
| Composite RGBA8 | 56 |
| Composite RGBA16 | 56 |
| Load supplied regular font | 757,272 |
| Logical wrapped layout | 3,152 |
| OpenType Latin layout | 6,408 |
| OpenType Arabic layout | 28,656 |
| Complete constrained fitting | 224,168 |
| Prepared text drawing, RGBA8 | 83,456 |
| Prepared text drawing, RGBA16 | 83,456 |

GC.GetAllocatedBytesForCurrentThread surrounds each public operation and output
disposal. Sources, fonts, layouts and caller options are resident inputs where
appropriate. Canvas reset happens outside the counter before every operation;
no increasingly opaque destination is substituted. Fitting includes its entire
search and releases its winner. Composite pixels equal the integer oracle;
both drawn frames equal the frozen current text frames. Borrowed inputs remain
unchanged. These are direct allocated-byte diagnostics, not BenchmarkDotNet
timing statistics or retained-capacity charges.

The standalone font worker and managed control are also prepared. Preflight
verifies copied input, exact metrics, independent ownership and shutdown with
**zero timed samples**. Invalid zero/oversized/unknown protocol requests are
rejected before timing. One immutable generated selector supports the existing
three/twelve cadence; this allocation round did not run a scored job. The
FreeType timing exception was subsequently approved on 2026-11-02.
No protocol or quota model enters production.

Production and the actual package are unchanged; full 5,378-check/OS and same
package qualification are reused. All 282 required paths and 110 selected time
comparisons were retained at this checkpoint. The later
[nine scoped timing comparisons](TEXT-DRAWING-CURRENT.md) reuse this allocation
evidence where explicitly labeled. No workload/font/settings grid is added.

[Evidence](text-drawing-allocation-controls-evidence.json). Retained
text-drawing-allocation-controls.tar.xz: **322,192 bytes / 47 members**,
SHA256 **a2ffb809e6a075102594d38830cebec1a6af16d976d76a51e782a97c5455b48a**,
mirrored and member-verified. It contains actual consumers, worker/selector,
sources, records, guards and member hashes. Replay the retained alc68 consumer
with one CPU, tiering off and nonconcurrent workstation GC. Existing raster/font
inputs and frozen text outputs are dependencies of the
[compositing](COMPOSITE-REFERENCE-FEASIBILITY.md),
[font](FONT-TEXT-REFERENCE-FEASIBILITY.md) and
[text-reference](PUBLIC-TEXT-LAYOUT-REFERENCE.md) records.
