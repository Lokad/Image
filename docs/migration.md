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

Current text support is supplied-face logical left-to-right layout with legacy
horizontal kerning, wrapping, bounds and unhinted rendering. Arabic/OpenType
shaping, bidirectional layout, alignment and qualified constrained-area fitting
remain pending. Lanczos3 is also pending; nearest/bilinear/bicubic are implemented.
GIF/animation and ICC conversion remain deferred. EXIF reorientation is opt-in.
Default option differences are accepted; select quality, sampling and orientation
explicitly when their behavior matters. [SPEC.md](../SPEC.md) is authoritative;
[resource admission](resource-admission.md) defines ownership and accounting.
