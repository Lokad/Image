# Moving to Lokad.Image

Development 0.2.1 adds selective `Probe`/`ProbeAsync`. Use nullable facts and
`EstablishedFacts`; an unexamined tail is not validated or evidence of absence.
Actual acquisition has its own bound, distinct from encoded extents. Complete
Identify/InspectHeaders behavior stays available. Optional `ImageMetadataPolicy.IgnoreInvalid`
tolerates unusable EXIF across read routes; strict remains the default. Permissive
orientation probing may traverse the entire container to resolve overrides.
See [source metadata](source-metadata.md).

`ValidatePixels`/`ValidatePixelsAsync` add complete coding validation without an
owned output image. They return coded source metadata and keep the same limits,
cancellation and metadata-policy controls; memory remains codec-dependent.

## 0.2.0

Published 0.1.0 packages remain unchanged. The supported scope has expanded;
general replacement readiness and complete warm CPU parity are not claimed.

- **Breaking APIs:** `ImageBudget`, `BudgetedImage`, their codec overloads,
  `ImageLimits.MaxWork` and `ImageLimitKind.Work` are removed. Use regular codec
  methods with one caller-owned `ImageLimits.MemoryBudget` root and dispose
  returned images directly. No forwarding wrappers or CPU quota remain.
  `BufferLength`'s enum numeric value changed; persist named categories.
- **Memory:** images, fonts and layouts keep shared charges until disposal.
  Keep their budget ancestor alive and finish row-span/async users first.
  Refusal is a typed terminal limit failure, without retry or fallback.
  See [resource admission](resource-admission.md).
- **Codec output:** managed PNG/JPEG/WebP encoders now match their declared
  [reference controls](codec-parity.md). Encoded bytes and reconstructed lossy
  pixels can differ from 0.1.0. Color JPEG quality maps to
  `1 + (100 - Quality) * 30 / 99`; grayscale has its own quantizers.
  Equal quality numbers across encoders do not imply equal quality.
- **JPEG inputs:** caller Gray8/RGB8 encoding and Gray8/RGB8/CMYK8 decoding avoid
  mandatory RGBA intermediates. Select source-color and CMYK inversion explicitly
  when necessary. Decoding adds CMYK/YCCK and 4:1:1/4:1:0 with fixed-point output.
- **Metadata/streams:** `ImageInfo` adds encoded source color, alpha and component
  descriptors. Existing construction/deconstruction remains; record equality and
  serialization include the fields. `InspectHeaders[Async]` uses bounded workspace;
  its validation differs from Identify and full decode. Caller streams stay open.
- **Composition:** drawing, alpha compositing, Lanczos3 and supplied TrueType text
  are available at 8/16 bits. Select `TextShaping.OpenType` for supported shaping
  and bidi text. `TryFit` returns an owned layout; position its bounds explicitly.
  Supply actual face styles; unsupported font features fail with typed details.

GIF, animation and ICC conversion remain deferred. EXIF reorientation is opt-in.
Choose sampling, quality, orientation and text options explicitly when their values
matter. [Supported contract](spec.md), [usage](usage.md) and
[benchmark snapshot](benchmark.md) state verified scope and limitations.
