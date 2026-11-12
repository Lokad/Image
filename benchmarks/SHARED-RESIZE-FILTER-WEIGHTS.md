# Shared resize filter weights

The [shared byte-geometry follow-up](BYTE-CROSS-SHARED-GEOMETRY.md) now owns the
two bounded byte cross/identity costs; the other71 costs below remain source-valid.
The current gap snapshot includes those two updates. The filter-weight stage's
original captures and qualification remain in its evidence.

The Catmull–Rom polynomial used by the generic filter dispatcher and fixed
bounded-column preparation now shares one inline implementation. Division,
absolute value, coefficients, normalization and operation
order are unchanged. All four original files reconstruct exactly after removing
this extraction and its three direct calls. No new buffer, owner, pool or public API.

The smaller dispatcher also changes inlining opportunities in other callers.
Consequently all **61 existing bicubic controls, eight prepared/one-shot
boundaries and four bilinear controls** are reconciled on the same immutable DLL:
**57 pass / 10 gaps / 6 uncertain** in the original standalone protocol. No new
image, shape, depth, filter or settings grid. All exact managed outputs, approved
native bounds, independent ownership and idle-v2 guards pass. Codecs and other
editing paths retain source-valid evidence. This is not a uniform speedup claim.

The independent bounded-wide byte result saves13.66µs/3.82%raw/2.57%normalized;
narrow-word saves17.89µs/3.53%raw/3.16%normalized and packed-word
21.69µs/5.61%raw/7.95%normalized. Filtered/all-actual raw and normalized intervals
separate; native clocks move−1.28%/−0.39%/+2.54%, respectively. These are scoped
between-capture comparisons, not a paired-DLL experiment or an end-to-end sum.

Two passing deep controls cost more: word8×17 adds20.62µs/4.44%normalized;
byte8×4 adds24.85µs/13.14%normalized while native time moves−7.01%. Both remain
passing, upper0.905/0.755. Original kernels and portable paths remain; these
tradeoffs are retained explicitly. Stop passing cases rather than retuning them.

| Largest current gaps, complete public Resize | Lokad / zimg µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: |
| RGBA8, bounded2×9 | 1,338.67 / 1,049.55 | 1.275 / 1.280 | 114 |
| RGBA8, bounded width reduction / height enlargement | 1,063.49 / 841.66 | 1.264 / 1.270 | 114 |
| RGBA8, bounded width reduction / same height | 690.55 / 576.22 | 1.198 / 1.212 | 113 |
| RGBA16, bounded32×16 | 553.89 / 446.20 | 1.241 / 1.268 | 113 |

Ordinary Resize includes all managed preparation, workspace, filtering, exact
repair, independent output and disposal. Bicubic's zimg reference retains graph
and scratch; bilinear uses FFmpeg's retained scaler context. The eight equivalent
prepared/one-shot boundaries all pass; prepared
plans retain198,432B wide/199,840B narrow, and one-shot disposal retains no plan.
Allocation, retained state and peak live capacity are separate: no new owned or
retained capacity; bounded cases keep4KiB extra admission and zero retained
operation scratch. Physical/native/JIT peaks are unmeasured. GC counts include
owned output/cache allocations; they are not peak-memory measurements.

Two scoped counterbalanced64-operation captures preserve all twelve pairs and
before/paired/after native brackets. Bounded mixed byte854.87/zimg866.15µs,
range0.974–0.998, passes that scope; its original nonstationary standalone capture
900.25/870.63µs, range0.905–1.091, remains uncertain. Bilinear word enlargement
1,836.05/1,967.62µs, range0.858–1.001, remains uncertain, with4,498,558 direct GC
B/op. No favorable repeat or broader parity inference. Byte bilinear reduction
remains a gap; the contemporary previous-DLL comparison has overlapping confidence
and does not establish a candidate regression. All adverse/uncertain data remain.

One old horizontal reference predates an unused1×1 mode, an equal vertical-halo
alias and untimed ready metadata. Whole-file reconstruction proves the selected
operation source unchanged; both native identities are retained. Its completed
capture was recovered without retiming. A rejected driver binding failed before
fixture creation; the existing bicubic-capable driver then supplied the first
actual adjacent capture. No native, harness, fixture or driver rebuild.

Windows/Linux each pass **6,456 full/live checks**, resize528internal/791public
normal/scalar/AVX512-off/AVX2-off, including all three named storage suites. The
same actual Release package and all three examples pass both OSes. Exact packets,
pixels, cancellation and shared admission remain unchanged. Original immutable
core/embedded builds and focused evidence are reused; one pack. Development
package0.2.1 is454,713B, SHA256`b6b6ec02a0c6d83b936acbc88b77501d644461a75d6242d585453ab2970626b7`.
Published releases remain immutable; publication is postponed.

[Current evidence](shared-resize-filter-weights-evidence.json) records every
control, reference, confidence and qualification. Raw
`shared-resize-filter-weights-qualified.tar.xz`, SHA256
`4681cb0a8d7e5a0ac20218b471473a3eaf2df8af78c81fa44ec097470e203a6e`
(1,489,632B/855members), is mirrored/member-verified. The
[original variants](BICUBIC-COEFFICIENT-PREPARATION-ROUND.md) retain earlier
diagnostics; B/C remain unselected. All289 required paths stay in scope.

The geometry follow-up completes the first shared-preparation step. Next inspect
bounded byte2×9's current path, with a289.11µs gap; cross/identity gaps remain
221.83/114.33µs. Preserve exact arithmetic,4KiB and useful measured kernels.
