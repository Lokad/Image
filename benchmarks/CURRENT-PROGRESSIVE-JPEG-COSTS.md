# Current progressive JPEG costs

Four decode costs are refreshed after exact completed-row reuse. Grayscale and
complete-coding validation retain proven unchanged-path evidence. All six named
paths have valid independent quiet-VM costs; none yet meets the ≤1× upper target.

| Existing complete warm one-CPU operation | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Progressive444 | 4,557.046 | 1.351 / 1.392 | 984,212 | Gap |
| ProgressiveGray | 2,270.109 | 1.326 / 1.332 | 683,126 | Gap |
| Progressive420 | 2,138.627 | 1.096 / 1.108 | 775,318 | Gap |
| Progressive422 | 2,728.898 | 1.215 / 1.258 | 847,116 | Gap |
| ProgressiveCancelable444 | 4,788.190 | 1.421 / 1.451 | 984,212 | Gap |
| ValidateJpegProgressive420 | 291.290 | 1.087 / 1.094 | 4,072 | Gap |

The original 513×259 packets/settings and one-CPU complete-operation boundaries
remain. Decoding includes conversion, fresh independently owned RGBA8 and release;
complete-coding validation excludes RGBA normalization on both sides. The FFmpeg
software decoder resets per operation and reuses private contexts. The live token
is never signaled; native has no token option. Startup, I/O, IPC and validation
checks are excluded. Native allocation is unmeasured.

All managed frames equal their frozen native frames. Source/output lifetimes,
CLI/scalar correctness and quiet guards pass. 4:2:0 decode now has a confirmed
gap (lower 1.080); its old confidence-open cost is superseded. No new control is added.
Full Windows/Linux/live qualification and the same actual Release package/three
examples pass. Original temporary metadata/proof files are restored exactly.

[Current decode costs](WARM-JPEG-COMPLETED-ROW-REUSE.md) and
[numeric evidence](warm-jpeg-completed-row-reuse-evidence.json) pin refreshed costs
and qualification. [Refinement-window evidence](warm-jpeg-refinement-window-evidence.json)
retains unchanged grayscale and validation costs. These pin all actuals,
producers, hashes and mirrored/member-verified raw evidence. Earlier
[grayscale/live costs](current-progressive-jpeg-costs-evidence.json) and
[ordinary 4:4:4 cost](current-progressive-jpeg-444-cost-evidence.json) are retained
as historical sources; only the latter supplies the matched gain baseline.
The full 268-path objective remains open.
