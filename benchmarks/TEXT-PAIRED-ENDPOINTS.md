# Paired text endpoints

TEP1040 evaluates each edge's top/bottom coordinates together with SSE2, preserving
the scalar subtraction/division/multiplication/addition order. The portable path
uses the original coordinate calculation. Polygon order, cancellation checkpoints,
40 B edge layout, admission, ownership and exact managed pixels are unchanged.
No new cache, prepared storage, dependency or numerical policy is introduced.

Independent contemporary prepared-RGBA16 pairs save **1.615 µs / 3.107% raw** and
**3.413% with paired reference normalization**; both intervals separate. The wider
bracket-normalized gain remains inconclusive, with no separated adverse result.
The paired reference clock moves +0.316%. This supports the scoped change; it does
not establish a complete-pipeline gain or a broad text-processing slowdown.

| Public boundary | Lokad.Image µs | ImageSharp µs | Ratio / upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Ordinary RGBA8 drawing | 76.335 | 45.104 | 1.692 / 2.056 | 59,304 | Scoped gap |
| Ordinary RGBA16 drawing | 79.565 | 46.133 | 1.725 / 2.046 | 59,304 | Scoped gap |
| Prepared RGBA8 drawing | 47.260 | 37.025 | 1.276 / 1.426 | 56 | Scoped gap |
| Prepared RGBA16 drawing | 50.371 | 37.868 | 1.330 / 1.561 | 56 | Scoped gap |
| Complete one-shot | 1,175.026 | 6,218.257 | .189 / .216 | 283,274 | Pass/stop |
| Preparation plus eight owned draws | 1,850.663 | 6,827.534 | .271 / .304 | 284,624 | Pass/stop |

Drawing uses the frozen DejaVu Sans 2.37 Regular “AV To” 18 px, 384×160 canvas,
origin 8/8. Each actual groups 1,024 independently reset operations, with two
timestamps per draw on both sides and exact managed frame validation outside the
timer. All twelve actuals and three reference groups remain. Public-only allocation
counters are measured separately; timed dispatch adds 24 B/draw. The prior
[aggregation](TEXT-PREPARED-DRAW-AGGREGATION.md) reported its full 80 B adapter
counter, so the 56 B column is not a production allocation reduction.

The complete controls retain ConstrainedFit16, thirty fitting trials and every
public reference layout call, owned fresh cleared canvases and disposal. The
direct eight-draw total amortizes to **231.333 / 853.442 µs per draw**, including
preparation. Creation-only preparation is **1,070.505 / 6,107.349 µs**; disposal
is outside that diagnostic and inside complete operations. Never add stage times.
The separate complete-fit prepared-draw diagnostic remains uncertain.

Reference identity remains **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp
3.1.12**, not FFmpeg or a bare shaping-engine comparison. Managed drawing retains
geometry/crossings/coverage workspace; reference Fill prepares working storage
from retained positioned paths. Public reference collection reclamation is
GC-driven; managed admission releases on disposal. Unchanged [FreeType loading](FONT-HEADER-VALIDATION.md)
remains a scoped 22.164 / 13.218 µs gap with eager/lazy validation differences.

Active raster capacity remains **40,408 B Latin / 39,904 B complete-fit**;
Latin preparation peak is **42,488 B**, complete governed peak **807,708 B**.
Draws add no admitted storage. Borrowed canvases/input, font retained by the
diagnostic harness, idle pools, approximate heap deltas and physical peaks are
distinct. Reference draw allocations and physical/native/JIT peaks are unmeasured.
Complete reference allocations are 9,256,953 B one-shot / 9,494,633 B eight-draw.

All eight frozen narrow/Latin/Arabic/fitting frames at both depths remain exact,
with original visual/layout/ownership gates. 161,444 coordinate comparisons cover
signed zero, subnormals, neighbors, extremes and overflow; NaNs are checked by
classification. Actual Band code contains separate packed divide/multiply/add,
no FMA; size grows 2,461→2,694 B. Instrumented elapsed time is discarded.

Windows/Linux each pass **6,715 full/live checks**, focused **259 internal / 161
public** normal/scalar checks and the **same actual Release package / three
examples**. One core/embedded build per OS, zero public/harness/reference/fixture
builds, one pack; the timed Linux DLL is never rebuilt. Unchanged codec/resize
ISA, font-loading and layout/fitting-only evidence is reused. Historical reports
remain linked, outside current-cost renderer dependencies. Publication is postponed.

[Evidence](text-paired-endpoints-evidence.json) pins sources, all observations,
qualification and ownership. Raw `tmp/text-paired-endpoints-qualified.tar.xz`:
5,225,268 B / 127 members, mirrored/member-verified SHA-256
`e632b290ca4e7376d212eba9fe85107c3a4b3d1f12945df7ddb64b57e4409441`.
Actual package: 464,536 B, SHA-256
`2805ceb9cad515878d7657681144fdcf8627f8a4088ee0898f552504d34e449e`.
