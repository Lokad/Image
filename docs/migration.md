# Lokad.Image 0.2.0 development

Published 0.1.0 remains unchanged. Version 0.2.0 is under qualification; it is not
yet a claim of replacement readiness or performance parity for the expanded scope.

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

Text defaults to supplied-face logical layout with legacy kerning. Select
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

JPEG keeps its original quantization by default. Development adds opt-in
`JpegQuantizationPolicy.CompactColor` for color output; it retains grayscale,
quality100, sampling and flattening, while trading other reconstruction precision
for smaller packets. Assess complete size and quality together for the intended
content; equal numerical quality across encoders does not establish equivalence.
