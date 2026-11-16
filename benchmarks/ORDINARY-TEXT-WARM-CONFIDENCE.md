# Ordinary text warm confidence

OTC1144 resolves both ordinary-drawing uncertainties as **scoped gaps** on the
unchanged actual-package DLL. This is a measurement update, not a production
speedup or an overall text-processing slowdown. Qualified owning one-shot,
preparation-plus-eight-draw, prepared drawing and authored-font passes stop.

| Retained-layout ordinary draw / stock ImageSharp.Drawing 2.1.7, Fonts 2.1.3, ImageSharp 3.1.12 | Managed / reference µs | Conservative ratio interval | Managed allocation B/op |
| --- | ---: | ---: | ---: |
| RGBA8 | 47.286 / 45.437 | 1.016202–1.059120 | 83,216 |
| RGBA16 | 51.745 / 46.593 | 1.093911–1.122973 | 83,216 |

The existing separate aggregate worker explicitly resets, counts and checks every
draw; it has no generated BDN unroll defect. Its previous before/after reference
groups calibrated from one operation. The 200 ms setting chose 2,329/2,426 calls,
giving 104–134 ms actual groups; initial before batches cost 55.528/55.056 µs/op
and dominate their wider intervals. The specific underlying cause is unmeasured.
Those [earlier samples](TEXT-GLYPH-ROW-BOUNDS.md) remain uncertainty evidence for
that protocol; none is discarded or retrospectively called warmup.

This capture predeclares **4,096 complete warmup operations on each side before
any statistical group**, followed by fixed 4,096-operation samples. All 12 actuals
and reference-before/paired/reference-after groups remain. Reference bracket means
are now 45.437/45.594/45.612 µs for RGBA8 and 46.638/46.593/46.698 µs for RGBA16.
One capture per depth, no favorable repeat or outlier filtering. CPU0, one processor,
optimized JIT, natural nonconcurrent workstation GC and all idle-v2 guards pass.

A small orchestrator reuses the compiled public worker and reference process. Its
existing correctness-only setup skips calibration; reflection outside clocks sets
the fixed before bracket for the worker's original after/check/dispose path.
Every actual still includes the original public drawing operation, geometry/events/
workspace preparation, scan conversion, composition and cleanup. Font/layout and
reference positioned paths are retained. The frozen DejaVu Sans 2.37 `AV To`,
18 px, 384×160 canvas, color, input delivery, ownership and output gates are unchanged.
Both sides reset outside every operation and use two timestamp reads; full managed
frame checks and allocation reads stay outside clocks. These are explicitly
different retained preparation states, not bare shaping, FFmpeg or one-shot text.
No historical stage timings are added into an end-to-end claim.

Public allocation remains **83,216 B/draw**, plus the unchanged 24 B timed adapter.
The reused reference worker does not measure allocations for this boundary;
complete-integration allocation figures are separate evidence. No owned capacity,
cache, pool or retained raster workspace is added. Standing font/layout/canvas
storage, admitted peaks, allocations and unmeasured physical/native/JIT peaks
remain distinct. All four new normal/scalar byte/word checks pass original frozen
frames, layout, reference and ownership gates. Reuse unchanged **6,749 checks per
OS/SAME actual package/all three examples**. Two small Windows orchestrator builds
correct an initial assembly-reference name; zero production, VM or package builds.

One read-only two-second current RGBA16 profile then records 30,913 public draws,
with reset/exact-frame validation between operations. Application CPU0, sampler
CPU1, both guards and original gates pass; instrumented elapsed is discarded.
Of 1,994 sampled locations, Band has 365, Area 202, BuildEvents 141, BuildEdges 134,
UInt128 division 73 and BlendWord 49. These include reset/validation/GC and skid/
inlining; they are locations, not phase costs or removable percentages. The earlier
[transparent-destination implementation](TEXT-TRANSPARENT-DESTINATION-EXPERIMENT.md)
remains inconclusive. Do not blindly replay it or infer a speedup from these counts.

[Evidence](ordinary-text-warm-confidence-evidence.json) pins all producers, immutable
workers, new orchestration, warmup/actual records, current profile and qualification
reuse. Its mirrored/member-verified raw archive retains the exact runtimes,
sources, all guards/logs, profile/map/JIT and before/paired/after samples. Restore it
and its declared dependencies, then run archived `otc1144-prepare.py` and
`otc1144-measure.py` in a fresh ignored workspace on the quiet VM.

The meaningful current ordinary16 difference is about **5.151 µs**. Choose a
materially justified implementation from current attribution, then compare complete
operations and reconcile affected boundaries before broad/package qualification.
Font-loading validation costs and copy gaps remain in the queue; nanosecond probe
tuning stays deferred. Publication is postponed and all 289 paths remain required.
