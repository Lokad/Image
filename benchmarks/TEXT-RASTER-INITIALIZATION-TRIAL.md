# Raster buffer initialization trial

**Not retained.** Requesting uninitialized operation-private edge/event arrays
preserves behavior but establishes no useful complete ordinary-draw gain.

| Three reversed pairs, 4,096 original RGBA8 draws per subject | Baseline → prototype | Ratio interval | Public GC B/draw |
| --- | ---: | ---: | ---: |
| Existing growth → safe uninitialized allocation | 48.185898 → 48.658469 µs | .919013–1.109585 / inconclusive | 83,216 → 83,216 |

The adverse mean is not a demonstrated regression. Every actual and quiet guard
is retained; no confirmation, standalone reference retiming or favorable replay.
The timed adapter adds 24 B on both sides. Current production costs remain valid.

`Grow<T>` is used only for unmanaged edges and event coordinates. The prototype
adds that constraint and replaces its allocation with
`GC.AllocateUninitializedArray<T>`. Only initialized count is copied or consumed;
each complete new record is written before its count advances. Unused capacity
never escapes. Growth, capacities, replacement peaks, cancellation, admission,
refusal and cleanup stay verbatim. Crossing/range/row arrays, edge/event order,
all numerical kernels and exact pixels remain unchanged. No owner, cache, pool,
public API or numerical policy is introduced.

The source review reuses existing attribution and adverse capacity, edge-storage,
certified-order and shared-boundary trials. Unique shapes and active edges are
already reused. A safe uninitialized request does not prove that returned pages
needed zeroing, or attribute time to initialization. Allocation, admitted capacity
and retention stay unchanged; physical/native/JIT peaks are unmeasured.

Each OS passes **283 internal / 222 public** focused checks in normal and scalar
modes, including every-growth refusal/cancellation, recovery, exact arithmetic
and ownership. Original byte/word actor frames pass on both DLLs in both modes.
The screen retains resident font/layout/canvas, original stock ImageSharp
correctness checks, complete geometry/event preparation, scan conversion,
composition and cleanup. Reset and full-frame checks are outside each operation's
two clocks. CPU0, natural nonconcurrent GC and idle-v2 guards remain. This is an
incremental DLL diagnostic, not an ImageSharp/FFmpeg timing comparison.

One isolated Windows core and one embedded-test build; zero VM, canonical,
public-test, actor, harness, reference, fixture or package builds. Reuse the
unchanged accepted **6,790 full/live checks**, same actual package and three
examples each OS. No broad/package campaign follows this inconclusive result.

[Evidence](text-raster-initialization-trial-evidence.json) pins the source proof,
actuals, qualification and recipes. Mirrored/member-verified
`text-raster-initialization-trial.tar.xz`: **2,181,884 B / 64 members**, SHA256
`c39bea405ab907263741bc27ab17ec369dc3e65c7a1a74400668d2c1ae7a5989`.
Archived `rgi1188-*` / `rgu1189-*` and declared baseline dependencies reproduce the
round. No current displayed costs or renderer dependencies come from this trial.
Continue independent copy work; materially different evidenced preparation
designs remain eligible. The 289-path objective remains open.
