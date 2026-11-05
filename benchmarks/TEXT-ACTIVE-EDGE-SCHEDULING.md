# Active text-edge scheduling

Raster preparation retains activation order once. Successive bands compact expired
crossings and activate each edge once in the same crossing buffer. Original edge
identity preserves X ties and winding order; intersection/area arithmetic, sample
rounding, full 16-bit composition and exact managed frames remain unchanged.
No array, cache, pool, public API or production dependency is added.

| Complete prepared drawing | Original µs | Current µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 | 369.02 | 189.86 | 45.43 | 4.179 / 5.230 | 92,384 |
| RGBA16 | 359.85 | 189.35 | 45.98 | 4.118 / 5.226 | 92,384 |

Independent twelve-actual confirmations follow short screens. Gains are
**179.16 µs/48.55%** and **170.49 µs/47.38%**, with separated filtered/all-actual
raw and normalized confidence. Native clocks shift −1.10%/−0.95%; all brackets and
actuals remain. Both controls still have CPU gaps. No repeated completed timings.

The frozen DejaVu Sans 18px Latin AV To layout/font are resident; complete public
DrawText includes preparation, scan conversion, composition and disposal. Canvas
reset is excluded equally. The explicitly labeled reference is ImageSharp.Drawing
2.1.7/Fonts2.1.3. Separate full public layout/fitting integration retains every
layout call and fitting trial. Full16-bit samples and approved normalized visual
gates remain. No general font/script CPU claim follows these two controls.

Allocation rises **8,672 B/draw**, 83,712→92,384. Edge stride 32→40 preserves identity
within existing 40 B/item admission; crossings stay 16 B. A comparer/owner field also
add allocation. Actual admitted peaks remain 48,544 B for Latin and unchanged on all
eight existing frames; scratch returns zero. These are admission observations,
not physical peaks. Native allocation is unmeasured. Eight frozen Arabic/wrapped/
fitting/Latin frames and two new complex authored frames remain exact.

Both OSes pass **5,659 full/live checks**, 120 internal/148 public affected checks per
normal/scalar mode and the SAME actual Release package/all three examples. The
package checks exact clipped mixed-alpha text at both depths. Unchanged AVX512
proof is reused. Initial Linux internal checks used baseline embedded code; one
internal rebuild with current source/new tests establishes candidate proof.
The prior rejected literal-model report receives the same coverage correction;
no retroactive rerun or new claim. Main/internal each build once per OS; all
measurement/reference/resource tools are reused, with one private original-frame
freeze tool. Published releases are immutable and publication is postponed.

[Evidence](text-active-edge-scheduling-evidence.json) retains current costs,
confidence, allocations, exact frames, admission and qualification. Earlier
[interval preparation](TEXT-EDGE-INTERVAL-PREPARATION.md) is linked history.
Raw `text-active-edge-scheduling-qualified.tar.xz`: 3,858,352B/244members,
SHA256 `7ed3f9fae89a499de9ca22727af3b60f58ce0170ea29728d0c108bde4ec135c0`, mirrored/member-verified. Retained tae315 scripts and
pinned dependencies reproduce the stage. The full282-path objective remains open.
