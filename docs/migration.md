# Lokad.Image 0.2.0 development

Published 0.1.0 remains unchanged. Version 0.2.0 is under qualification; it is not
yet a claim of replacement readiness or performance parity for the expanded scope.

JPEG uses one baseline encoder with explicit quality, sampling, background and
restart options, including direct caller gray/RGB samples. Color quality maps to
qscale `1 + (100 - Quality) * 30 / 99`; default85 maps to5. Grayscale keeps its
owned quantizer formula with the reference integer transform and optimized tables.
Packets and reconstructed lossy pixels may differ from0.1.0. Unreleased
experimental quantization/progressive switches are removed. Table capacity and
temporary tree peaks are admitted before allocation; refusal stops the operation.
[Codec compatibility](codec-parity.md) states the exact tested scope and attribution
exception. Historical JPEG times refer to their recorded producers.

PNG `Optimal` now uses the managed level6 encoder for every supported color type
at8/16bits. It emits4096-byte IDAT chunks and the reference's neutral pHYs field.
The eight frozen controls match complete reference packets; source samples,
cooperative admission, cancellation and stream ownership remain supported.
Other policies still use runtime compression; their byte replication is pending.
CPU/allocation qualification of the new path remains open.

`ImageInfo.SourceColor`, `SourceAlpha` and `SourceComponentCount` describe encoded
samples, alongside source `BitDepth`; they do not describe normalized RGBA pixels.
Unlabelled JPEG color models remain `Unknown`. VP8L always codes alpha even when
its header hint leaves `HasAlpha` false. The six-argument constructor/deconstructor
is retained; manually constructed records default to unknown descriptors, and
record equality/serialization include the new fields.

- Pass one caller-owned adapter through `ImageLimits.MemoryBudget` for aggregate
  live-memory admission. Implement Lokad.Image's three interfaces against the
  caller's shared root; no governor package is required. Reuse these limits in
  codec/text options and editing calls, including nested operations.
- Dispose `ImageBuffer`, `TrueTypeFont` and `TextLayout` after their final users.
  Retained objects keep their own charges beyond operation completion. Prepared
  layouts own decoded outlines and remain renderable after font disposal.
- Keep the supplied budget ancestor alive through every operation and surviving
  result. Caller inputs and budgets stay borrowed. Admission refusal is a typed
  `ImageLimitException`, distinct from cancellation and allocation failure; it
  terminates the operation without retrying or waiting.
- `ImageBudget` remains a separate codec memory/work quota. It can be used with
  cooperative memory hooks; local `ImageLimits` and work limits still apply.

Text defaults to supplied-face logical layout with horizontal `kern` kerning. Select
`TextShaping.OpenType` for supported Latin/Hebrew/Arabic shaping and bidirectional
layout; `Direction` and `Alignment` make paragraph flow and line alignment explicit.
Unsupported font features fail with structured details. `TextLayout.TryFit`
returns an owned layout on a finite size grid; translate its bounds origin into
the target area. Limits/refusal remain terminal across fitting trials.
Select `ResizeFilter.Lanczos3` for radius-three antialiased composition resizing
at either pixel depth. It shares existing centers, edge extension and alpha
semantics; its warm-performance qualification remains pending.
GIF/animation and ICC conversion remain deferred. EXIF reorientation is opt-in.
Default option differences are accepted; select quality, sampling and orientation
explicitly when their behavior matters. [SPEC.md](../SPEC.md) is authoritative;
[resource admission](resource-admission.md) defines ownership and accounting.
