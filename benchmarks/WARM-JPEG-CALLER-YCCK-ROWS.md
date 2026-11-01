# Exact caller YCCK420 RGB rows

Current costs are superseded by the
[bounded container scan](WARM-JPEG-CONTAINER-SCAN.md). The exact row implementation,
paired gain and raw evidence remain retained.

The existing caller-YCCK420 RGB24 control independently saves **87.40% elapsed**
(87.41% all-actual, **87.39% native-normalized**), with separated confidence.
It now passes and stops at **0.709× / upper 0.711**, down from **5.622×**.

| Existing complete 513×259 decode | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Status |
|---|---:|---:|---:|---:|---|
| Ycck420CallerRgb | 428.98 | 605.32 | 0.709 / 0.711 | 762,704 | confirmed-pass-stop |
| Ycck420 | 426.18 | 710.51 | 0.600 / 0.617 | 895,547 | confirmed-pass-stop |
| CmykCaller | 346.34 | 331.63 | 1.044 / 1.059 | 1,085,961 | confirmed-gap |

Only the caller-RGB primary has a paired speedup claim. The frozen original
6,704-byte packet produces exactly the same 398,601 RGB bytes as normal/scalar
FFmpeg. Inputs, sampling, reference settings, black/inversion arithmetic and
complete output acquisition/normalization/release remain. Native allocations
are unmeasured. The new measured allocation is **762,704 B/op**, versus 762,625
before (+79 bytes, 0.01%). No new storage, owner, cache or admission is introduced.

The existing exact half-grid row kernel now writes either RGB or RGBA. Shared
traversal replaces per-pixel interpolation, black multiplication and one-pixel
color dispatch for normal-orientation half-grid YCCK caller RGB. Horizontal then
vertical floor averages, signed Q16 black products and the current inverse-color
matrix/packing are unchanged. Bounded 192-byte stack scratch and tile cancellation
are reused; other layouts, orientations, formats and inversion retain their contracts.
No full-frame RGBA intermediate. The portable scalar path remains exact.

The current original independent confirmation is reused from the
[retained-result round](WARM-JPEG-DIRECT-RGB-RESULT.md). One short and one independent
confirmation, each with one pinned CPU, tiering off, native brackets and idle-v2
guards; no new raster/settings grid, profile or repeated valid score. Both affected
companions have current independent costs. Owned YCCK remains a pass and stops;
caller CMYK retains its measured status. Other 26 JDO36 costs and all unrelated
paths reuse applicable unchanged evidence. No extra timing confirmation follows.

Fresh **5,369 checks per OS** (2,176 internal, 2,245 public, 948 live) pass.
Four focused internal and 399 public checks pass normal/scalar on both OSes.
Existing per-pixel reference tests now cover both output widths, including all
luma/black values, chroma signs, tails, protected spans, cancellation and zero
leaf allocation/admission. Public/package controls cover baseline/progressive,
native exact pixels, orientation, padding, surviving caller buffers and borrowed
shared-root cleanup. The **same actual Release package/three examples** pass both
OSes. One core/internal build per OS; zero public, harness, selector, native or
differential builds. Published releases are unchanged.

[Evidence](jpeg-caller-ycck-row-evidence.json). Raw jpeg-caller-ycck-rows-qualified.tar.xz,
171 members, SHA256 2062ec67e919e6b50feae4c01c29ec18e6a53767404a8f0d21136eb6e507442b.
Mirrored/member-verified raw evidence retains source/binary originals and selected
builds, fixtures/selectors/workers, all actuals/guards/receipts, both-platform
qualification and actual packages. The prior archive is pinned as a dependency.
This resolves one existing required path; the full 268-path warm parity objective
remains open. No broader content/layout or release-wide benchmark claim.
