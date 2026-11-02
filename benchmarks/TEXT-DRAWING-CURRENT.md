# Current text and drawing comparisons

All nine previously missing controls now have twelve actual iterations with
before/after native brackets and passing quiet-VM guards. Production is unchanged.
Times are µs; upper bounds conservatively include filtered/all-sample confidence.

| Complete operation / reference | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Font loading / FreeType 2.13.2 | 339.68 | 14.88 | 22.824 / 23.618 | 757,598 | Confirmed gap |
| Wrapped logical layout / SixLabors public API | 6.07 | 96.16 | 0.063 / 0.097 | 3,153 | Confirmed pass |
| Latin OpenType layout / SixLabors public API | 20.40 | 57.62 | 0.354 / 0.717 | 6,409 | Confirmed pass |
| Arabic OpenType layout / SixLabors public API | 88.93 | 337.84 | 0.263 / 0.421 | 28,658 | Confirmed pass |
| Complete fitting search / SixLabors public API | 1,144.00 | 1,882.11 | 0.608 / 0.866 | 224,181 | Confirmed pass |
| Prepared text RGBA8 / ImageSharp.Drawing | 926.31 | 51.59 | 17.954 / 23.296 | 83,456 | Confirmed gap |
| Prepared text RGBA16 / ImageSharp.Drawing | 871.93 | 52.12 | 16.729 / 19.666 | 83,456 | Confirmed gap |
| Composite RGBA8 / FFmpeg | 5,086.39 | 492.69 | 10.324 / 10.470 | 56 | Confirmed gap |
| Composite RGBA16 / ImageSharp 3.1.12 | 5,247.40 | 696.04 | 7.539 / 7.708 | 56 | Confirmed gap |

Text is a COMPLETE PUBLIC-API INTEGRATION workload using ImageSharp.Drawing 2.1.7 /
Fonts 2.1.3, including multiple public layout calls, retained paths/metadata, or all
fitting trials plus the selected winner. These are not bare shaping-engine results.
Prepared drawing includes rasterization/compositing; resident font/layout setup
and canvas reset are excluded equally. RGBA16 retains full samples and approved
visual alpha tolerances in normalized 0..255 units: mean≤8, RMSE≤16, coverage≤2%,
edges≤1 pixel. Glyph/cluster/line/fit and position/bounds requirements remain.

Font loading copies/opens/retains/disposes the frozen DejaVu Sans 2.37 face against
FreeType 2.13.2. Eager/lazy validation differs; this is not general parser equivalence.
RGBA8 composites use FFmpeg with native RGB≤1/RMSE≤.5 and exact alpha/untouched/
zero-source-alpha pixels. RGBA16 uses ImageSharp 3.1.12 Rgba64 SrcOver, RGB/alpha≤1
and RMSE≤.25 each; it is not FFmpeg performance. Managed composites remain exact.

Each timed canvas operation starts from the original pixels, with reset outside
both timers. The generated driver originally declared unroll 1 while executing 16;
four initial canvas runs are invalid and retained. Only those four were corrected,
now checking exactly one operation and exact full output after every iteration.
The five valid font/layout/fitting confirmations were reused. Extra diagnostics
passes are disabled for canvas jobs; their allocations explicitly reuse
[ALC68](TEXT-DRAWING-ALLOCATION-CONTROLS.md). Missing GC counters never mean zero.

The largest relative gap here is font loading, but its absolute difference is
about 325µs. Compositing costs around 5ms. These remain visible alongside larger
codec/resizing work; they do not globally block architectural progress.

[Evidence](text-drawing-current-evidence.json) contains every actual, bracket,
confidence interval, allocation provenance and producer identity. Raw
text-drawing-current-comparisons.tar.xz: 1,926,060 B / 185 members,
SHA256 a09482d3ccd62804378dc09da442126e0d8cd01cbbfbc300508d8a6c0db1ba02,
mirrored/member-verified, preserving initial failures, corrected drivers/harnesses,
actual producers and replay scripts. Use the current ParseBenchmarkLog tool,
which preserves absent extra statistics, to parse logs without rerunning jobs.
