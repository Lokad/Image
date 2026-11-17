# Ordinary drawing with symmetric output checks

The two current ordinary-drawing uncertainties resolve as **one scoped gap and
one pass**. This is a sampling-tool correction on the unchanged qualified package
DLL, not a production speedup or a claim about overall text processing.

| Retained-layout draw / stock ImageSharp.Drawing 2.1.7, Fonts 2.1.3, ImageSharp 3.1.12 | Lokad /reference µs | Conservative ratio interval | Lokad /reference GC B/op |
| --- | ---: | ---: | ---: |
| RGBA8 | 48.357 /47.503 | 1.002961–1.028560 /gap | 75,840 /48,576 |
| RGBA16 | 47.696 /48.575 | 0.955422–0.997532 /pass, stop | 75,840 /48,576 |

The earlier [polygon-scratch capture](TEXT-CONDITIONAL-POLYGON-SCRATCH.md) keeps
all its uncertain results. Source review finds an untimed-work asymmetry: managed
drawing checks the exact full frame after **every** operation; the reference checks
it only in preflight and cleanup. The corrected sampling tool checks every reference
frame against its own unchanged frozen output, outside the timer. It also reads
reference allocation counters outside the timer, matching the managed placement.
The public reference Apply, Reset and original Check bodies remain verbatim.
This is not a reference-library defect or a library patch; all three stock public
library identities remain unchanged. The exact sampling-source patch is archived.
Untimed work can affect execution context; no causal speed claim is made by
subtracting historical costs.

The frozen DejaVu Sans face, `AV To`, 18 px, 384×160 canvases, color, input delivery
and ownership remain unchanged. Both sides retain font/layout; the reference
retains positioned paths, while ordinary managed drawing prepares geometry and
workspace per call. These different preparation states remain explicit. Complete
public drawing, rasterization, composition and cleanup stay timed. Every draw has
an untimed reset and full-frame check on both sides, with two timestamp reads.
The existing exact managed/full-depth frames and approved visual comparisons stay
unchanged. No fixture substitution or quality-gate relaxation obtains a pass.

One capture per depth retains all **twelve 16,384-operation actuals**, three
reference groups and the same conservative confidence calculation. A fixed
16,384-operation warmup on both sides precedes all statistical groups. CPU0,
one processor, optimized JIT, natural nonconcurrent GC and all idle-v2 guards pass.
Batch CPU/wall observations are diagnostics only; no normalization, clock
subtraction, outlier filtering or favorable retry is used. Four normal/scalar
preflights preserve the original observations and frames exactly.

Managed allocations additionally include the unchanged **24 B adapter** in the
sampling loop. Reference 48,576 B/op is newly measured for this boundary, not
inferred from complete integrations. The new reference oracle retains 245,760 /
491,520 B of test data outside RGBA8/16 operations; both sampling sides retain
their own expected frames. This is harness storage. Production admitted peaks,
standing font/layout/canvas storage, retained plans and unmeasured physical/native/
JIT peaks remain distinct from GC allocations. No new production retention exists.

The actual immutable Linux DLL is `e1ae64a3…`, from the same private package
`62bcbdd9…`. Reuse **6,819 full/live checks per OS**, the SAME actual package and
all three examples. One reference-tool build, zero production, managed harness,
VM, fixture or package builds; no broad qualification repeat. The unchanged
one-shot, preparation-plus-eight-draw, prepared and authored passing paths stop.
RGBA16 now stops too. The RGBA8 mean gap is about **0.854 µs**; select further work
from attributable current work, before the smaller owned-load and copy gaps.
No broad compression, font-engine or FFmpeg claim follows from these controls.

[Evidence](ordinary-draw-symmetric-checks-evidence.json) pins sources, stock and
corrected-tool identities, all actuals, allocation observations and qualification
reuse. Mirrored/member-verified `ordinary-draw-symmetric-checks.tar.xz`:
**7,335,976 B /96 members**, SHA256
`7000420c7dc16c94d40be69af82f6bdefc8bd982a652b73934155b2557adce87`.
Restore its declared dependencies and archived `osa1217-*` recipes. The original
control source, explicit patch, compiled runtimes, every guard and raw record are
retained. The full 289-path objective and final complete package refresh remain
open; publication stays postponed.
