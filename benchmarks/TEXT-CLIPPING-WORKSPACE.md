# Band-owned text clipping workspace

Drawing costs are superseded by [interior coverage reuse](TEXT-INTERIOR-COVERAGE.md).
The evidence below records this earlier stage.

Text scan conversion now reuses two 128B polygon buffers throughout a band.
Half-plane clipping is skipped only when it would copy every vertex unchanged.
The remaining intersection calculations, vertex order, shoelace sum, winding,
coverage rounding, blending and cancellation checkpoints are unchanged. No cache,
pool, public preparation API or new owned capacity is introduced.

| Complete prepared drawing | Contemporaneous original µs | Current µs | ImageSharp.Drawing µs | Ratio / upper | Managed B/draw |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 | 147.75 | 136.57 | 45.17 | 3.023 / 4.678 | 67,368 |
| RGBA16 | 150.12 | 137.43 | 46.74 | 2.941 / 4.136 | 67,368 |

Filtered means improve 7.6%/8.5%; all-actual means improve 5.4%/7.5%.
**Raw and normalized confidence intervals overlap:** a causal CPU gain remains
inconclusive, and both reference gaps remain demonstrated. The bounded change
is retained for band-level scratch reuse and removal of redundant copying,
supported by these positive point results. Allocation does not fall.

Three-actual screens cost 163.72/163.65µs while aggregate native means move to
52.40/51.96µs from the retained 45.58/45.90µs. That clock contradiction justified
one contemporaneous twelve-actual original/candidate confirmation per depth,
using the saved immutable original DLL. The table uses the faster native bracket,
as elsewhere. All shorts, outliers and original observations are retained; no
completed measurement was repeated to obtain favorable confidence.

The existing control is DejaVu Sans 18px “AV To”, 384×160 canvas, origin 8/8.
Complete DrawText includes geometry/events, rasterization, composition and disposal;
font/layout are resident and canvas reset is excluded equally. The reference is
explicitly ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 on ImageSharp 3.1.12, not FFmpeg
or a bare shaping engine. The full integration reference includes every layout
call and fitting trial. Approved visual tolerances preserve full 16-bit samples;
managed frames remain exact. This is not broader font or text CPU qualification.

All eight existing Latin, wrapping, Arabic and fitting frames remain exact.
Allocation stays 67,368B/draw, admitted Latin workspace 48,544B and retained drawing
scratch zero; every observed frame admission peak is unchanged. Explicit clipping
scratch remains 256B within the existing 4KiB frame allowance. Its lifetime moves
to the band; physical stack and native peaks are unmeasured.

Windows/Linux each pass **5,909 full/live checks**, plus 160 internal and 185 public
affected checks per normal/scalar mode. The added always-clipped oracle compares
30,736 finite/generated and 4,096 special-coordinate cases exactly, including
reused poisoned storage and canaries. Existing admission refusal, cancellation,
cleanup, contour and independent-lifetime checks pass. The SAME actual Release
package and all three examples pass both OSes. Main/embedded assemblies build once
per OS; harnesses, fixtures, native references and resource workers are reused.

[Evidence](text-clipping-workspace-evidence.json) records current costs, source
scope, checks and identities. Raw `text-clipping-workspace-qualified.tar.xz`,
SHA256 `3ab0d954235be6477b98b583c4dde5e239203d00ce860e927e3f67cdc06d5434`,
5,955,596B/148 members, is mirrored and member-verified. Its retained tcr587 scripts
and dependencies reproduce this stage. [Shared sorting](TEXT-RASTER-SHARED-SORT.md)
retains the earlier allocation gain and attribution. Unchanged layout/fitting,
codec, compositing and resize paths reuse evidence. The full 283-path objective
remains open; publication is postponed.
