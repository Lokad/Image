# Conditional polygon scratch

Ordinary/clipped drawing remains source-valid here. Current owned-font loading and complete integrations use [retained prefixes](FONT-RETAINED-PREFIX.md).

Complete ordinary RGBA8 drawing saves **0.769 µs /1.65%** on the
final timed DLL: **46.671672 →45.902237 µs**, paired ratio interval
**0.978329–0.988739**, with separated raw mean intervals. Allocations remain
**75,840 B/draw**, plus a 24 B adapter. This is a scoped five-glyph complete-draw
effect; prototype and historical clocks are not cumulative gains.
Measurements use the quiet VM, Release, CPU0 and natural nonconcurrent GC.

| Operation at this milestone | Lokad /reference µs | Ratio upper /status | Managed GC B/op |
| --- | ---: | --- | ---: |
| Ordinary RGBA8 | 48.134 /45.273 | 1.127899 /uncertain | 75,840 |
| Ordinary RGBA16 | 47.173 /47.040 | 1.054277 /uncertain | 75,840 |
| Authored clipped RGBA8 layout + draw | 41.223 /224.429 | 0.299789 /pass | 41,064 |
| Authored clipped RGBA16 layout + draw | 38.190 /225.899 | 0.294813 /pass | 41,064 |
| Earlier complete constrained-fit one-shot | 1,206.808 /6,552.156 | 0.209938 /pass | 298,786 |
| Earlier complete preparation + eight draws | 1,270.748 /6,946.246 | 0.207805 /pass | 308,516 |

References remain stock ImageSharp.Drawing 2.1.7 /Fonts 2.1.3 /ImageSharp 3.1.12
for ordinary/complete integration and explicitly [source-corrected tooling](AUTHORED-FONT-COMPARISONS.md)
for authored controls. These are public integration costs, not FFmpeg or bare
shaping-engine comparisons. All original layout calls, thirty fitting trials,
fresh owned explicitly cleared canvases, outputs and disposal remain timed in
the complete integrations. Ordinary/authored borrowed canvases reset outside
every operation on both sides. Preparation-only is diagnostic, never summed
with historical stages. Different validation eagerness, reference GC-driven font
release and positioned-path versus full-precision coverage retention stay explicit.

Two bounded Point[8] polygons move from every scan band to the private general
clipping helper that actually uses them. Fast area cases no longer prepare or
pass unused scratch. All fast/general arithmetic, comparisons, vertex ordering
and source outside these boundaries remain byte-identical. The helper cannot
inline into fast cases. Stack views never escape. No heap allocation, cache,
pool, public API, admission or cancellation boundary changes.

The exact always-clipped bit oracle and production Clip canaries remain, covering
interiors, clipping, signed zero, subnormals, extreme/nonfinite coordinates and
frozen full-depth frames. The removed private signature has no forwarding wrapper.
Actual prototype code moves the 256 B polygon preparation out of Band: code sizes
2,401→2,214 B for Band, 1,075→668 B for Area, plus 424 B for ClippedArea.
This verifies lifetime placement, not elapsed causality or physical stack peaks.

Independent prototype confirmation saves 0.869 µs /1.87%. The final ordinary
standalone byte point differs, so one targeted final-DLL paired comparison
resolves artifact transfer before broad qualification; only its effect above is
accepted. Both external-reference ordinary controls remain uncertain. All twelve
actuals, three reference groups, adverse points and idle-v2 guards are retained.
The paired 4,096-operation actor binding is corrected before any guard/sample;
the caught mismatch and exact correction remain reproducible. The standalone
controller keeps its established 16,384-operation cadence throughout.

The existing Arabic control preserves exact frames and 501,600 B/draw on both
DLLs. Its separate-block prototype intervals overlap; no causal gain, regression
or external-reference parity is established. Existing coarse frame admission,
full-fit governed peak **821,900 B** and retained fit/Latin plans **4,160 /2,112 B**
remain unchanged. Allocation, admitted capacity and unmeasured physical/native/JIT
peaks are distinct. [Initial edge storage](TEXT-INITIAL-EDGE-STORAGE.md) retains
the earlier allocation reduction and its own evidence.

Windows/Linux each pass **6,803 full/live checks**, focused **292 internal/222 public**
normal/AVX-disabled/scalar checks and the **same actual Release package/all three
examples**. Each final core/embedded assembly builds once; the timed DLL is never
rebuilt. Unchanged font loading, layout, prepared execution, codecs, resizing and
copies reuse identified evidence. The full 289-path objective and final complete
package refresh remain open; publication stays postponed.

[Evidence](text-conditional-polygon-scratch-evidence.json) pins sources, outputs,
all actuals and package. Mirrored/member-verified `text-conditional-polygon-scratch-qualified.tar.xz`:
**3,843,136 B /237 members**, SHA256
`2fb0f7e5372e4261eb7d046ba9602daf47b53c14d1fb5163ccdbf8c10a73c0f8`. Archived `pas1201-*` /`pcs1202-*` recipes and declared
dependencies reproduce the round.
