# Default-variation layout

ADV1135 qualifies the original `TextLayout/OpenTypeDefaultVariation` boundary
using the unchanged actual package DLL. **Pass and stop for this control.** This
is a new measurement, not a production speedup or overall text-processing claim.

| Complete repeated layout | Lokad.Image | Labeled public ImageSharp integration |
| --- | ---: | ---: |
| Independent confirmation | 3.551 µs | 20.357 µs |
| Allocated per operation | 3,608 B | 58,496 B |

Conservative ratio interval **0.164379–0.184945**, using all 12 managed actuals
and both independent 12-actual reference brackets. Short screen: 3.407/19.174 µs,
upper 0.210585. Every versioned idle/process guard passes; CPU 0, one processor,
optimized JIT and nonconcurrent workstation GC. No discarded actuals or restarts.

The supplied authored face is prepared outside both timers. Each operation
includes layout, outline decoding, measurement, positioned independent results
and disposal. The reference uses **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 /
ImageSharp 3.1.12**, with four public layout calls plus retained path/glyph/advance
arrays. This is public integration, not FFmpeg or bare shaping-engine performance.
No drawing, fitting search or canvas belongs to this particular layout control.
Eager/lazy font validation and retained parser/cache state differ; this does not
measure one-shot font loading. Existing complete DejaVu one-shot/eight-draw passes
remain stopped; historical stage times are never summed into end-to-end costs.

Frozen two-glyph `AV`, size 16, wrap 64, spacing 1.25. The 1,548 B authored font
has contextual GSUB and active GDEF regions with signed word/byte corrections
summing to zero at the default coordinate. Exact managed observations and frozen
reference results pass Windows and Linux, including scalar execution on Linux.
The [preflight](AUTHORED-FONT-REFERENCE-PREFLIGHT.md) retains exact glyph/cluster/line
and position/ink checks; no font bytes, reference assembly or gate changes.

Allocations come from BDN GC totals/operations, independently cross-checked with
three warmed 128-operation public-API diagnostics. Managed admission separately
reports font capacity **1,804 B**, font plus layout **5,372 B**, governed peak
**5,738 B**, and surviving layout charge **3,568 B** after font disposal, returning
to zero on layout disposal. Borrowed input is separate. Reference retained
capacity and physical/native/JIT peaks are unmeasured. The Windows reference
allocation diagnostic is 66,240 B; the table reports the Linux timed boundary.

No production change or production/VM/package builds. Three Windows tooling
builds prepared the reused BDN layout driver, correctness entry and single-precision
JSON readback; no timed outputs were retried. Reuse unchanged 6,747 full/live checks
per OS and SAME actual package consumer/all three examples. Five other original
font cases remain unmeasured because their reference-output gates fail; that is
not a demonstrated CPU gap. Independent codec/copy/resizing work remains available.

[Evidence](default-variation-evidence.json) records all identities, memory and
qualification reuse. Its mirrored, member-verified raw archive contains the own
driver/adapter, exact compiled runtime, all logs/actuals/guards and preparation
checks; the dependency preflight archive retains font/frame provenance. Reproduce
with archived `adv1135-deploy.py` then `adv1135-measure.py short` and `confirm` on
the quiet VM, reusing the actual package DLL. Publication remains postponed and
the complete 289-path objective remains open.
