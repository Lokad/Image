# Two-dimensional subsampled RGB JPEG rows

Direct-RGB JPEG420 decoding improves from **5.563× FFmpeg** to independently
confirmed **1.638× / upper1.646×**, reducing managed time **70.6%**.
Every frozen managed pixel remains exact. Final allocation is **1,619,540 B/op**.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Original generic interpolation | 4,156.72 | 747.24 | 5.563 / 5.948 | 1,619,496 | Above target |
| Sixteenth-weight rows, three actuals | 1,224.94 | 742.78 | 1.649 / 1.682 | 1,619,540 | Screen |
| Independent twelve actuals | 1,220.97 | 745.27 | 1.638 / 1.646 | 1,619,540 | Confirmed |
| Horizontal RGB422 regression | 1,646.35 | 1,095.09 | 1.503 / 1.570 | 1,978,655 | Three-actual mean passes; prior confirmed |
| YCbCr420 regression | 1,642.77 | 1,064.99 | 1.543 / 2.312 | 1,619,530 | Three-actual mean passes; prior confirmed |

The original 513×259 byte raster's native JPEG420 packet is reused. Original
marker-only authoring adds Adobe transform0, assigns R/G/B component identifiers
in SOF/SOS and removes conflicting JFIF metadata. Quantization/Huffman tables,
all entropy bytes and sampling22/11/11 remain unchanged. The quantized planes
are interpreted as RGB; there is no new original image or settings/shape grid.
Both subjects decode the same52,109-byte packet.

One six-second profile records5,794 samples: **41.1% interpolation** and
**22.4% rounding**. The accepted primitive specializes horizontal/vertical2:1
direct RGB. Centered coordinates are exact quarters on each axis; integer
sixteenth weights retain every original binary64 interpolation/away-from-zero
result. Two component rows are combined without quantizing an intermediate row.
AVX2 handles eight output pixels per batch, with scalar clamped edges/tails.
Equivalent sampling factors remain supported. No extra workspace or pixel cache
is introduced; existing admission and per-row work/cancellation are unchanged.
Other RGB/YCbCr layouts retain their paths.

Both subjects include complete identical-packet decode, pixel conversion,
independent owned output acquisition and release. The native software mjpeg
decoder exposes these coded RGB420 components as YUVJ420P. Its adapter therefore
**times the RGB component-plane interpretation**, expands the two owned gray
component planes on the centered2x2 ceil-sized grid, crops to513×259 and packs
through a GBRP view ordered green/blue/red, with no YCbCr matrix. Both expanded
planes and the final owned RGBA output are allocated/released every operation.
Input/context/frame wrappers are reused; still-image decoder state resets each
time. Every operation decodes and converts all pixels.

An independent normal/scalar public CLI extractplanes/expand/crop/mergeplanes
graph matches the worker exactly. Applying ordinary native YCbCr conversion
instead differs by max116/RMSE37.000 and remains an untimed negative control.
The **unchanged** decoder gate is RGB max4/RMSE1.5 and exact opaque alpha;
this pair actually differs by max1/RMSE0.115. No managed tolerance is added.
Every frozen managed sample, source preservation and two simultaneously live
independent owners pass.

Sequential Release measurements use CPU0, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass on every run. All twelve confirmation actuals are retained; eleven
are selected. The upper includes selected/all-actual intervals against the faster
native bracket's lower bound. RGB422 and YCbCr420 regression means pass before
full qualification; short intervals do not replace earlier independent proofs.

Allocation figures include harness overhead and batch amortization; the new
primitive adds no allocation. Native allocation totals are **unmeasured**.
Startup, I/O, IPC, fixture creation, hashing and validation are excluded.
No decoded output is cached.

Thirteen row/edge/tail/guard and single-rounding controls pass with normal/scalar
execution on each OS. Full **3,482 Release tests per OS** pass:1,198 internal,
1,386 public API and **898 live FFmpeg comparisons**. The same actual Release
NuGet package's consumer and all three examples pass on Windows/Linux.
One native-worker build, two necessary harness builds and one production
candidate build suffice. Untimed utility-path/vertical-factor preparation
refusals are retained; no actual measurement ran from them. Cached generated
jobs and verified immutable DLLs are reused within the experiment, with one full
qualification per OS and no repeated method audit or profile.

[Numeric evidence](warm-jpeg-rgb420-row-evidence.json) retains all actuals,
allocations, guards, source/build/native/fixture identities, refusals, profile,
regressions and qualification. Stop this confirmed pair; full required warm
coverage remains open.
