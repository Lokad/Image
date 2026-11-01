# Required text and drawing performance coverage

The coverage audit finds **14 omitted required paths**. Preserve all existing
268 codec, geometry and read controls: the finite completion checklist now has
**282 paths**. The [two separate-buffer copy controls](WARM-RECTANGLE-COPY-SPANS.md)
now have current independent costs: RGBA8 is a confirmed gap and RGBA16 is
confidence-open. Both [clipped fill controls](WARM-RECTANGLE-FILL-PACKED.md) pass
and stop at conservative uppers 0.775/0.567. The remaining ten have no warm measurements.
Existing text/drawing correctness and memory qualification remain valid;
they are not warm CPU evidence.

The immutable original inventory contains no text-layout, font, fitting, drawing
or compositing comparison rows. At the inspected revision, no benchmark C# entry
point referenced these public APIs. The earlier coverage statement that they reused
unchanged performance evidence was incorrect. The [audit record](material-text-drawing-coverage-audit-evidence.json)
pins the inspected revision, source query, original inventory and capability check.

| Required control | Public boundary | Why material |
|---|---|---|
| DrawingFill8 | FillRectangle, RGBA8 | Packed byte replacement including hidden RGB; supplied color, clipped rectangle. |
| DrawingFill16 | FillRectangle, RGBA16 | Distinct ushort destination loop and full-precision color. |
| DrawingCopy8 | CopyRegion, separate RGBA8 buffers | Exact rectangle placement into a borrowed existing canvas; no new output frame. |
| DrawingCopy16 | CopyRegion, separate RGBA16 buffers | Distinct ushort span-copy boundary. |
| DrawingCopyOverlap8 | CopyRegion, overlapping RGBA8 rectangle | Required snapshot semantics through reverse traversal; exclude reset equally on both subjects. |
| DrawingComposite8 | Composite, RGBA8 source/canvas | Straight-alpha rational source-over, including zero and partial alpha. |
| DrawingComposite16 | Composite, RGBA16 source/canvas | Full-precision samples and 64-bit coverage arithmetic. |
| FontLoadRegular | TrueTypeFont.Load, existing DejaVu Sans regular | Copy/validate supplied bytes and release an independently retained face. |
| TextLayoutLogicalWrapped | TextLayout.Create, Logical | Default logical mapping, kerning, wrapping, bounds and retained layout ownership. |
| TextLayoutOpenTypeLatin | TextLayout.Create, OpenType Latin | Required substitution/positioning path on the existing Latin kerning control. |
| TextLayoutOpenTypeArabic | TextLayout.Create, OpenType Arabic | Existing ArabicMarksDigits control exercises joining, bidi and mark positioning. |
| TextFitOpenType | TextLayout.TryFit, existing ConstrainedFit | Complete bounded size search, releasing rejected candidates and retaining the winner. |
| DrawText8 | ImageDrawing.DrawText, RGBA8 | Prepared-layout rasterization and alpha compositing into a borrowed canvas. |
| DrawText16 | ImageDrawing.DrawText, RGBA16 | Distinct full-precision destination/color behavior with the same prepared layout. |

Use the existing original raster recipe and the already qualified public DejaVu
fonts/text controls. Freeze one representative input per named control. Do not
add a shape, face, script, style or settings grid. Existing correctness cases
retain mixed-depth compositing, clipping, overlap, supplied styles, combining
marks, alignment, ownership, refusal and cancellation coverage.

Before timing, establish an equivalent complete reference operation. For drawing,
both subjects borrow an existing canvas: include the mutation and operation
workspace; exclude resident source/font/layout preparation and identical reset
work. Font loading includes the input copy and retained face lifetime. Layout
includes bounds and retained output ownership; fitting includes every trial.
Rendering borrows the same prepared layout and includes raster workspace and
compositing. Keep admission and actual output release within the operation.

The pinned FFmpeg CLI and libavfilter archive currently contain no drawbox,
drawtext or overlay filter. Its public `av_image_copy_plane` API passes the
separate-buffer feasibility check at both depths: copy the existing 256×129
rectangle at (127,65) to (17,19) in a borrowed 513×259 canvas. All samples, alpha,
hidden RGB, unchanged source and untouched canvas pixels are exact after two
operations. Invalid depth and missing output are rejected. This is a native
correctness check, not a managed timing result or completed warm qualification.
The existing ImageSharp/Fonts comparison and
FreeType checks are correctness tools, **not approved performance baselines**.
Do not time a substitute or relax pixel/layout gates implicitly. Resolve the
missing native boundary or an explicitly approved tooling-only exception before
scoring text, fitting or compositing; do not compare to FFmpeg process
startup or to a differently shaped operation.

Public FFmpeg av_image_fill_color supplies the fill boundary, including timed
clipping and color normalization; its two independently confirmed controls now pass.

The copy controls and current costs are recorded in their linked report.
Other additions remain named unmeasured work; existing confirmed passing cases
stay stopped. The initial audit required one standalone native correctness build
and no production/benchmark build, timing or repeated qualification.
The subsequent copy round updates only the affected costs and qualifies its
accepted production change on both platforms and through the actual package.

Reproducibility: retained `material-text-drawing-coverage-audit.tar.xz` contains
the original adapter, exact native verification script, binary and records.
SHA256 `87de35a5b34fad1dfe09a0940ca4c358b1501e7749728ecacc9245baa7ea0826`,
96,572 bytes / seven members, mirrored and member-verified. The JSON pins the
compiler, public-header/archive hashes and both original input/output hashes.
Run its `mca52-copy-native.py` at the repository root with the existing pinned
FFmpeg development installation and retained original byte/word rasters.
