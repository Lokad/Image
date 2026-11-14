# Borrowed drawing validation

Fill, copy and compositing now validate borrowed storage directly, removing a
56 B operation owner that never held workspace or a shared-budget scope. The
two-move copy pair removes 112 B. All seven frozen controls allocate **zero B/op**;
owned workspace and retained operation capacity remain zero. Caller storage,
local limits, shared-root ownership and default idle-pool policy are unchanged.
Allocation, admitted capacity, retention and unmeasured physical peaks are distinct.

| Complete frozen public operation | Lokad / reference µs | Ratio / upper | GC B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Separate RGBA8 copy | 1.976 / 1.926 | 1.026 / 1.029 | 0 | gap |
| Overlapping RGBA8 two-move pair | 3.812 / 3.790 | 1.006 / 1.014 | 0 | uncertain |
| Separate RGBA16 copy | 3.792 / 3.784 | 1.002 / 1.004 | 0 | gap |
| Clipped RGBA8 fill | 1.236 / 1.567 | 0.789 / 0.793 | 0 | pass |
| Clipped RGBA16 fill | 2.093 / 4.098 | 0.511 / 0.513 | 0 | pass |
| RGBA8 source-over / FFmpeg | 65.012 / 451.062 | 0.144 / 0.150 | 0 | pass |
| RGBA16 source-over / ImageSharp3.1.12 | 563.626 / 629.514 | 0.895 / 0.913 | 0 | pass |

Complete RGBA8 copy gains **2.74% / 0.057 µs** and the overlapping pair gains
**3.16% / 0.125 µs** in independent original/candidate confirmations. Each retains
12 alternating pairs of 8,192 complete operations per subject, exact full-canvas/
source/forward/final checks and isolated borrowed canvases. These are small
incremental effects, separate from current standalone reference costs; no stage
timings or percentages are added. Tiny copy gaps/uncertainty remain open. The
word copy's range overlaps its earlier uncertainty; a regression is not established.
Do not repeat unchanged jobs for favorable bounds. Four fill/composite passes stop.
Fill/composite costs are refreshed qualifications, without causal gain claims
against older artifact clocks. The earlier 1.19 µs byte-fill point remains in the
linked history; the current 1.24 µs cost is not claimed to improve it.

Copy/fill reference FFmpeg n8.1 public APIs. RGBA8 compositing uses FFmpeg overlay;
RGBA16 explicitly uses ImageSharp 3.1.12 Rgba64 SrcOver. Existing native-only
numeric bounds and exact managed rational pixels remain. Native wrappers/graphs
retain their original preparation. Both sides include complete public mutations;
source/canvas preparation and resets remain excluded equally. Overlap includes
two public moves and reverse restoration against six native directional bands.
Compositing keeps every-operation resets outside timing and invocation/unroll1.
No general copy, fill, compositing or arbitrary-shape performance claim.

Direct cancellation tokens preserve every chunk checkpoint. Pixel kernels,
traversal, SIMD arithmetic, clipping and exact rational rounding are verified
unchanged after token substitution. Nonempty regions fit an existing valid packed
ImageBuffer, establishing intrinsic depth/dimension/array capacity; caller-specific
region limits remain checked. Malformed policy, precancellation, canvas-first
working limits, disposed inputs and alias-counting retain their failure order.
No new quota, ownership wrapper, temporary pixels, session or common-context churn.

Windows/Linux each pass **6,659 full/live checks**, including 11 new public cases;
**25 internal/91 public** pass per normal/scalar mode. Existing arithmetic/ISA
qualification is reused. The **same actual Release package** and all three usage
examples pass both OSes, including borrowed-root/overlap/snapshot/cancellation
consumers. One core/embedded/public build per OS is reused throughout; no production
rebuild follows timing. Prepared harnesses/workers/fixtures require no builds;
one paired helper is adapted. Performance reconciliation precedes full/package work.
The fill wrapper's already-recorded comment-only identity refresh is reconciled;
its original native binary and behavior remain exact. Completed timings never restart.

Codec, resizing, inspection, independent-result edits and font/text paths retain
valid evidence and their original artifact identities. The 191 image-owner views
remain 179 pass / zero gaps / 12 uncertain; these in-place controls are separate.
The full 289-path objective remains open; publication is postponed.

[Evidence](borrowed-drawing-validation-evidence.json) pins sources, actual producers,
all current costs/allocations, correctness, confidence, qualification and reuse.
Raw `borrowed-drawing-validation-qualified.tar.xz`: 41,259,724 B / 514 members,
SHA256 `9959c7dbc95acb9a62d519b9a6aef878cf17161a896dc7e71accdc7c5f414f4f`, mirrored and member-verified. Dependencies retain
the original frozen inputs, tools/references, historical adversity and reproduction.
