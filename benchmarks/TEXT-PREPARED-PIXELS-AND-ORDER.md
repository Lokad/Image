# Prepared text pixels and edge order

TRP671 reuses the already sorted crossing permutation instead of sorting all
edges again. Pixel blending borrows typed spans and shares exact integer weights;
8-bit numerators fit UInt64, while 16-bit channel numerators retain UInt128.
Full 16-bit color/coverage, hidden RGB at rounded-zero alpha, tie order and all
geometry/coverage arithmetic remain exact. No storage, cache, session or public
API is added. Cancellation and admission remain inside complete drawing.

| Prepared drawing | Contemporary original µs | Current µs | ImageSharp.Drawing µs | Ratio / upper | Allocated B/draw |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 | 125.83 | 111.21 | 48.06 | 2.314 / 3.275 | 67,368 |
| RGBA16 | 142.41 | 97.71 | 45.93 | 2.127 / 3.348 | 67,368 |

Filtered point gains are 11.61%/31.39%; all-actual gains are 12.82%/25.55%.
Only the word filtered intervals separate. Both-depth all-actual and normalized
intervals overlap: **robust CPU gain remains inconclusive**. The bounded stage
is retained for exact work reuse supported by these point results; both reference
gaps remain. All three-actual screens and twelve-actual independent confirmations
are retained, without favorable repeats or further variants.

The frozen DejaVu Sans 18px “AV To”, 384×160 canvas and origin 8/8 are unchanged.
Complete DrawText includes geometry, preparation, integration, blending and
disposal. Font/layout are resident; canvas reset is excluded equally. The intended
Windows Release-package DLL supplies these costs on the quiet Linux VM, with
contemporary original/candidate runtimes and unchanged harness/reference binaries.
The reference is ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 on ImageSharp 3.1.12,
not FFmpeg or a bare shaping engine. The wider text suite measures complete
public-API integration, including its multiple layout calls and full fitting
search where applicable; prepared drawing is a separate repeated-state boundary.
Approved visual tolerances preserve full 16-bit samples, not sample exactness.

All eight Latin, wrapping, Arabic and fitting frames remain exactly unchanged.
The existing actual BigInteger blend oracle and new independent permutation tests
cover extreme samples, hidden RGB, ties, nonfinite keys, guarded spans,
cancellation and recovery. Windows/Linux each pass **6,011 full/live checks**,
plus 164 internal / 185 public affected checks per normal/scalar mode. The SAME
actual private Release package and all three usage examples pass both OSes.
No ISA dispatch changes; unchanged codec/resize ISA evidence is reused.

Allocation stays 67,368 B/draw; admitted Latin workspace stays 48,544 B and retained
scratch zero. All eight admitted peaks are unchanged. Physical/native peaks are
unmeasured. One immutable main/embedded build per OS serves the round; prepared
workers, fixtures and resource checks are reused. Only drawing costs change;
font/layout/fitting without rendering, composition, codecs and resizing retain
unchanged-path evidence. This is not a complete release benchmark refresh.

[Evidence](text-prepared-pixels-and-order-evidence.json) identifies sources,
actuals, confidence, ownership and the actual package. Mirrored/member-verified
raw `text-prepared-pixels-and-order-qualified.tar.xz` has 18,074,460 B / 227 members,
SHA256 `d5508445020fe2065983823f8054a8c66935266781f36de1f764e2d5b3449f71`.
[Interior coverage](TEXT-INTERIOR-COVERAGE.md) retains earlier results and worker
dependencies. The full 283-path objective remains open; publication is postponed.
