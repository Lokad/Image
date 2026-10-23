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

JPEG decoding now uses one pinned fixed-point inverse policy for every block.
Full-resolution YCbCr and caller RGB share derived integer color conversion,
including oriented destinations. Existing grayscale/full-resolution reference
controls are exact; other required color layouts remain under exactness
qualification. Older decoded half ties and performance results are superseded.
The numerical policy is described in [inverse rounding](jpeg-inverse-rounding.md).

All four PNG compression policies use the managed Deflate encoder. Fourteen
8/16-bit controls match complete frozen reference packets, including4096-byte
IDAT chunks and neutral pHYs metadata. CPU/allocation qualification remains open.

Lossless WebP uses one managed transform/reference/entropy pipeline. Its19 selected
and two additional forced packet identities match the pinned libwebp exact=1 controls.
Effort0..6 still controls search; lossless quality remains ignored. RGBA16 normalizes
once and hidden RGB stays exact. Packet bytes and workspace requirements differ
from0.1.0. Encoding decisions are independent of available memory: insufficient
admission fails without switching to a smaller-workspace encoder. Prepared coding
state avoids extra normalized/encoded frame copies. Historical timings describe
their frozen producers; new performance qualification remains open.

Lossy WebP uses the same independently implemented reference policy at every
admitted local budget. Quality85 and effort4 remain defaults; all efforts0..6
are supported. Packet bytes and reconstructed RGB can change. The declared
controls now match complete native packets; alpha remains exact after8-bit
normalization. Async output delivers prepared partitions directly. Resource
refusal never selects another encoder. Tight work limits can fail earlier;
method6's exhaustive alpha search may need an explicit larger caller allowance.
The32M-work control is a required terminal Work failure. Historical VP8 timings
refer to the replaced producer; new CPU/allocation qualification remains open.

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
