# Prepared-text preparation attribution

TAP782 makes one isolated own-source diagnostic build. Three coarse timestamps
outside pixel loops separate the `TextRasterizer` constructor from complete
scan conversion, coverage and exact compositing. Restoring the instrumented
wrapper reconstructs the original source exactly; no production source or DLL
changes. These are phase observations, not official package costs or speedups.

The existing frozen DejaVu Sans18px “AV To”,384×160 canvas and origin8/8 are
unchanged. Font/layout remain resident. Each depth has three200ms-active-drawing
batches; reset and exact frame checks are excluded as in the prepared workload.

| Phase, µs/draw | RGBA8 batches 1 / 2 / 3 | RGBA16 batches 1 / 2 / 3 |
| --- | ---: | ---: |
| Geometry/events/storage preparation | 28.74 / 27.16 / 27.17 | 27.08 / 27.21 / 27.10 |
| Raster/coverage/compositing | 49.69 / 49.65 / 49.64 | 52.51 / 52.56 / 52.50 |
| Outer work/disposal/timestamp overhead | 0.15 / 0.16 / 0.16 | 0.14 / 0.15 / 0.14 |

Byte batches contain2546/2599/2599 operations; word2509/2503/2509. The diagnostic
complete costs are76.97–78.58µs and79.73–79.92µs, respectively. They differ from
qualified98.44/102.57µs: source-clone compilation, code layout and instrumentation
can perturb timing. Do not replace current costs or subtract27µs from them to
predict a saving. No native measurement or unchanged confirmation is repeated.

Preparation includes admission/allocation, glyph subdivision, events/intersections/
ordering and crossing/row storage. All pixel work remains in the next phase.
The repeated geometry is material enough to investigate explicit caller-owned
reuse, provided an equivalent prepared reference boundary can be established.
The existing complete public text integration must retain every layout call and
fitting trial; ordinary drawing keeps necessary preparation timed on both sides.

Both depths allocate59,000 B/draw in the clone, matching the retained allocation
evidence. Draw-only governed peak remains42,336 B for Latin,238,848 B for Arabic,
41,688 B for fitting and7,424 B for narrow wrapping; retained scratch is zero.
Borrowed font/layout/canvas storage stays outside this draw-only root. Native,
physical and JIT-stack peaks are unmeasured. A retained raster would deliberately
extend capacity lifetime and must reserve it before allocation until disposal.

All eight existing Latin/wrapping/Arabic/fitting frames match their frozen bytes.
Font/source, borrowed-root cleanup and both idle-v2 guards pass. No new font,
image, settings grid, production/embedded build, full suite or package proof.
Current6,229 full/live and same-package Windows/Linux qualification are reused.

[Evidence](text-preparation-attribution-evidence.json) pins the clone, source
reverse proof, fixtures, operation counts, phase boundaries and reproduction.
The mirrored/member-verified `text-preparation-attribution.tar.xz` contains
763,460 B/30members, SHA256
`0508456d9e43a3376b842e745f548c692bd688d69251e9dd06f8880f56f1c98d`.
Its ignored `tap782-prepare.py` / `tap782-run.py` scripts reproduce the single
build/capture with the pinned current producer. Existing
[endpoint storage](TEXT-ENDPOINT-STORAGE.md) and
[public prepared boundary](TEXT-PREPARED-PIXELS-AND-ORDER.md) retain current
comparative costs and adverse/inconclusive evidence. Displayed timings and
renderer dependencies are unchanged; publication remains postponed.
