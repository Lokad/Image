# Exact interior text coverage reuse

TIR605 calculates the clipped rectangle area once per strictly interior run of a
scan-band trapezoid. Boundary equality and nonfinite coordinates retain the
original clipping path. The four shoelace terms, signed-zero handling, sequential
row additions and per-pixel cancellation are preserved. Geometry, sorting,
coverage rounding, rational blending and ownership are unchanged.

| Complete prepared drawing | Contemporary original µs | Current µs | ImageSharp.Drawing µs | Ratio / upper | Allocated B/draw |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 | 138.80 | 130.55 | 45.72 | 2.856 / 4.456 | 67,368 |
| RGBA16 | 141.57 | 132.38 | 45.83 | 2.889 / 4.174 | 67,368 |

Filtered point means improve 5.94%/6.49%; all-actual means improve 4.05%/5.80%.
**Raw and normalized confidence intervals overlap: CPU gain remains inconclusive.**
Retain bounded exact work reuse supported by these point results; both reference
gaps remain. No favorable repeats or further variants follow. Three-actual screens
were mixed while reference clocks shifted materially; one contemporaneous
twelve-actual original/candidate confirmation per depth resolves that comparison.
All adverse screens and outliers remain in the raw evidence.

The frozen DejaVu Sans 18px “AV To” control, 384×160 canvas and origin 8/8 are
unchanged. Complete public DrawText includes geometry/events, integration,
composition and disposal; font/layout are resident and canvas reset is excluded
equally. The reference is ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 on ImageSharp
3.1.12, not FFmpeg or a bare shaping engine. Its complete public integration
includes every layout call and fitting trial. Approved visual tolerances and full
16-bit samples remain distinct from exact managed frames. No broader CPU claim.

All eight Latin, wrapping, Arabic and fitting frames remain exact. Actual C#
checks compare 40,800 finite/generated and 4,096 special trapezoids at eight pixel
positions, including strict bounds, signed zero and extremes. The original
always-clipped area oracle remains. Windows/Linux each pass **5,911 full/live
checks**, plus 162 internal/185 public affected checks per normal/scalar mode.
Admission refusal, cancellation cleanup and independent output lifetimes pass.

Allocation remains 67,368B/draw, admitted Latin workspace 48,544B, retained scratch
zero. All eight admission peaks are unchanged. Band clipping scratch stays 256B
inside the existing 4KiB allowance; physical/native peaks are unmeasured. No new
heap storage, pool, cache or public preparation API. One immutable main/embedded
build per OS serves the stage; workers, fixtures and native references are reused.
The SAME actual private Release package and all three examples pass both OSes.
Unchanged layout/fitting, font, compositing, codec and resize evidence is reused.

[Evidence](text-interior-coverage-evidence.json) retains identities, source
projection, costs and qualification. Mirrored/member-verified raw
`text-interior-coverage-qualified.tar.xz`, SHA256
`eea1e4fd85537a25536a43232290135e3cdabcedeebc85615abc391e6104c379`,
5,972,544B/147 members, preserves scripts, modules, all samples and package proof.
[Current pre-stage attribution](TEXT-CURRENT-RASTER-ATTRIBUTION.md) and
[earlier clipping](TEXT-CLIPPING-WORKSPACE.md) retain history and dependencies.
The full 283-path objective stays open; publication remains postponed.
