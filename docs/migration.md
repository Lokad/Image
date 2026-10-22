# Lokad.Image 0.2.0 development

Published 0.1.0 remains unchanged. Version 0.2.0 is under qualification; it is not
yet a claim of replacement readiness or performance parity for the expanded scope.

Original JPEG 4:4:4 encoding may now use progressive scans when its complete plan
is admitted and restart markers are disabled. Set `JpegEncodeOptions.BaselineOnly`
for consumers requiring baseline syntax. This switch retains the same quantizers
and reconstructed pixels; local workspace selection may also use baseline syntax.
Encoded bytes and lossy reconstruction may change between encoder versions.
The final Original4:4:4 decisions now use the intended coefficient precision grid
with a source-derived error bound. DQT values, sampling and explicit quality
remain unchanged; qualified compression claims apply only to their stated controls.
Joint three-component AC choices reuse the pair stage's error/prices to fit the
existing default work limit; terminal admission failure still aborts the operation.
Final AC prices now follow the progressive spectral bands, retaining one global
MCU error allowance and the existing memory/work limits.
Source histograms now count those bands on the same fine training coefficients;
mask setup adds1,920 admitted work units once per operation.
Source prices use canonical Huffman lengths per band and observed EOB-run costs,
including extra bits, amortized per terminal block. Pending runs stay separate
per component. They fit the same8KiB workspace;
the default work limit and final MCU error bound are unchanged.

PNG `Optimal` now uses the deeper BCL search for every 8-bit color type, including
RGBA. This may increase CPU time and change packet bytes; samples, filter None,
memory admission, cancellation and ownership remain unchanged. It is a library
policy, not a fixed numeric compression level. Explicit compression choices and
16-bit output keep their previous backend selections.

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
