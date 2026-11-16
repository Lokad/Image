# Original clipped-fill controls

Both original required fill boundaries now pass and stop on the current immutable
package DLL. No production change or speedup is claimed.

| Original complete public operation | Lokad / FFmpeg µs | Ratio / upper | Managed GC B/op |
| --- | ---: | ---: | ---: |
| FillRectangle/Rgba8Clipped | 1.194663 / 1.730184 | .690483 / .696649 | 0 |
| FillRectangle/Rgba16Clipped | 2.183548 / 4.829109 | .452164 / .456739 | 0 |

The original frozen513×259 byte/full-word rasters request(-3,5,257,129), clipped
to(0,5,254,129). Earlier measured fills request(-17,19,273,129), giving256pixels
per row; that does not prove the254pixel tail. Both named recipes stay separate.
Two existing rasters/required paths; zero new images, scope or settings grid.

Each timed operation includes the complete public mutation: validation, clipping,
required16→8color normalization, cancellation checkpoints and borrowed ownership.
The explicitly labeled **FFmpeg n8.1 public av_image_fill_color** performs the
same normalization/clipping and mutates an equivalent prepared canvas. Allocation,
canvas preparation, I/O, IPC and verification are excluded equally. Idempotent
overwrites need no reset, output allocation or full-frame copy on either side.

Original colors and full16-bit samples remain. Independent scalar-built expected
canvases establish every sample, alpha, hidden RGB and untouched pixels. The current
package core passes normal/scalar checks for both original cases. Native controls
cover both defaults and originals at each depth, including malformed input,
protocol and CPU-affinity rejection. Existing default CLI behavior is preserved.

Short3actual screens pass before independent12actual confirmations on quiet CPU0,
sequential Release, optimized JIT and natural nonconcurrent workstation GC. All
actuals and both native brackets remain;99.9%Student-t confidence retains the
worse filtered/all-actual upper. No favorable repeat or historical-clock gain.
Scalar/preparation clocks are discarded.

Total GC allocation is0B/op; operation-owned admission and retained workspace
remain0. Borrowed canvas storage survives each call. Fixture/oracle copies are
excluded tooling, not peroperation storage. Native allocator/physical/JIT peaks
are unmeasured; no total-memory claim follows from zero managed allocation.

Production sources/package remain unchanged. Reuse6,747full/live checks per OS,
existing normal/scalar drawing/root/cancel/refusal checks and the SAME actual
Release-package consumer/all three examples from [SMP1130](JPEG-STUFFED-PAIRS.md).
There are no new broad tests, package builds or qualification campaigns.

Preparation builds one parent/two generated child harnesses and one native worker.
BDN also compiles two private production dependency copies under ArtifactsPath;
the first identity preflight rejects one before timing. Both existing jobs are
then rebound to exact current package DLL10440506, with no further build. The
canonical production binary never changes. Keep this failed preflight and the
discarded dependency identities; all accepted costs use the verified package.

[Evidence](original-clipped-fill-evidence.json) pins source, packet/canvas hashes,
workers, current producers, all costs, raw checks and reused qualification.
Raw `tmp/original-clipped-fill-confirmation.tar.xz`, SHA-256
`92658709f9969c7874bb481f88be4ac77b952ba165d31a748353a331fff35bb7`,
13,032,644B/171members, is mirrored/member-verified with the SMP1130 dependency.
It retains inputs, prepared runtimes, native worker, scripts and all evidence.
Reuse `clf1131-measure.py` with the prepared jobs; do not rebuild production.

All289required paths remain. This closes two missing original measurements;
original clipped byte-copy/composite and six authored-font boundaries still need
their own evidence. Publication remains postponed.
