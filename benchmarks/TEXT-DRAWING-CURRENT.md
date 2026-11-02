# Current text and drawing comparisons

All nine controls have independent twelve-actual measurements with reference
brackets and passing quiet-VM guards. Five unchanged font/layout/fitting controls
reuse valid costs; shared compositing rows and interval-ordered raster preparation
provide the other four current comparisons. Times are µs; upper bounds include
filtered/all-sample confidence.

| Complete operation / reference | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Font loading / FreeType 2.13.2 | 339.68 | 14.88 | 22.824 / 23.618 | 757,598 | Confirmed gap |
| Wrapped logical layout / SixLabors public API | 6.07 | 96.16 | 0.063 / 0.097 | 3,153 | Confirmed pass |
| Latin OpenType layout / SixLabors public API | 20.40 | 57.62 | 0.354 / 0.717 | 6,409 | Confirmed pass |
| Arabic OpenType layout / SixLabors public API | 88.93 | 337.84 | 0.263 / 0.421 | 28,658 | Confirmed pass |
| Complete fitting search / SixLabors public API | 1,144.00 | 1,882.11 | 0.608 / 0.866 | 224,181 | Confirmed pass |
| Prepared text RGBA8 / ImageSharp.Drawing | 360.59 | 45.27 | 7.966 / 9.721 | 83,712 | Confirmed gap |
| Prepared text RGBA16 / ImageSharp.Drawing | 392.22 | 50.08 | 7.832 / 10.030 | 83,712 | Confirmed gap |
| Composite RGBA8 / FFmpeg | 625.41 | 498.73 | 1.254 / 1.270 | 56 | Confirmed gap |
| Composite RGBA16 / ImageSharp 3.1.12 | 627.24 | 696.25 | 0.901 / 0.922 | 56 | Confirmed pass |

Text uses the COMPLETE PUBLIC-API INTEGRATION pipeline of ImageSharp.Drawing 2.1.7 /
Fonts 2.1.3, including multiple layout calls or the complete fitting search and
winner reconstruction. It is not a bare shaping comparison. Prepared drawing
includes raster preparation, scan conversion, composition and disposal; resident
font/layout setup and per-call canvas reset are excluded equally. Exact managed
frames and glyph/cluster/line/fitting checks remain. Approved RGBA16 visual gates
use normalized 0..255 errors: mean≤8, RMSE≤16, coverage≤2%, edges≤1 pixel, preserving
full sixteen-bit samples.

[Interval-ordered preparation](TEXT-EDGE-INTERVAL-PREPARATION.md) cuts drawing time
58.24% / 50.35% raw, with separated raw/normalized confidence. Allocation rises
256 B/draw; admitted Latin workspace rises 44,416→48,544 B, with no retained scratch.
Eight existing frames remain exact; Windows/Linux full checks and the same actual
Release package pass. Claims cover the two frozen drawing controls.

Font loading compares copy/open/retain/dispose on the frozen DejaVu Sans face
against FreeType 2.13.2. Eager/lazy validation differs; this is not general parser
equivalence. RGBA8 composites use FFmpeg with native RGB≤1/RMSE≤.5 and exact alpha,
untouched and zero-source-alpha pixels. RGBA16 uses explicitly labeled ImageSharp
3.1.12 Rgba64 SrcOver, RGB/alpha≤1 and RMSE≤.25 each; it is not FFmpeg performance.
Managed composition remains exact.

Passing layout/fitting and RGBA16 compositing controls stop. Drawing, font loading
and RGBA8 compositing gaps stay visible alongside larger codec/resizing costs.

Current evidence: [raster preparation](text-edge-interval-preparation-evidence.json),
[shared compositing rows](compositing-shared-rows-evidence.json). The
[original confirmations](text-drawing-current-evidence.json) and
[allocation controls](TEXT-DRAWING-ALLOCATION-CONTROLS.md) retain superseded drawing
costs, initial driver failures/corrections and raw archive identities. Their
unchanged font/layout/fitting results remain valid; historical costs are not
substituted for refreshed drawing measurements.
