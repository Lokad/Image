# Current text raster attribution

TAP604 reuses the qualified Linux `df93859a` DLL and frozen DejaVu Sans
18px `AV To` control. One two-second single-CPU capture contains 1,994 samples
across 17,295 complete public drawing calls. Font/layout are prepared; each draw
still constructs geometry/events, integrates coverage, composites and disposes
its workspace. The runner also resets and checks the canvas between calls;
those locations are separate. Instrumented elapsed time is discarded.

Coverage integration is material: Area/Clip have 224/208 sample locations and
Band has 262. Sorting has 356: crossing order 132, event order 114 and edge order
110. Actual JIT sizes/address ranges distinguish the three generic sort bodies;
erased perf-map names alone do not. BuildEvents has 114 and BuildEdges 31.
These are sampled locations, not phase timings or predicted speedups.

The next bounded hypothesis reuses one exact area result across strictly interior
pixels of each trapezoid. Boundary pixels retain polygon clipping; row accumulation,
per-pixel cancellation, geometry, blend arithmetic and workspace ownership stay.
An IEEE-754 model matches the original ordered clipping/shoelace result in 7,920
enumerated and 20,000 random finite cases, including signed zero and extremes.
Boundary equality is excluded because duplicate vertices can change rounding.
Actual C# oracle checks and the eight frozen frames must pass before timing;
the model does not authorize production acceptance or relax pixel contracts.

There are no production edits, builds, new fixtures or statistical measurements.
[Current drawing costs](TEXT-CLIPPING-WORKSPACE.md) and package qualification
remain unchanged. The reference is explicitly ImageSharp.Drawing 2.1.7 /
Fonts 2.1.3, with the existing complete integration boundary and visual gates.

[Evidence](text-current-raster-attribution-evidence.json) retains module/source
identities, typed JIT attribution and the numerical model. Mirrored/member-verified
raw `text-current-raster-attribution-tap604.tar.xz`, SHA256
`8856a627784b21fedeb1574cf941a0f40107d51da683756d536c110a65d66a18`,
110,428B/25 members, preserves the capture, exact output checks, idle-v2 guards,
scripts and source, with the existing worker/reference dependencies linked.
