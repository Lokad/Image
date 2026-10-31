# Current progressive JPEG costs

The ordinary-refinement window improves the matched 4:4:4 control **9.41%** with
exact pixels and unchanged allocation. All six existing affected paths have current
independent quiet-VM evidence on the same qualified producer.

| Existing complete warm one-CPU operation | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Progressive444 | 4,487.297 | 1.335 / 1.349 | 984,212 | Gap |
| ProgressiveGray | 2,270.109 | 1.326 / 1.332 | 683,126 | Gap |
| Progressive420 | 2,208.307 | 1.060 / 1.160 | 775,318 | Confidence open |
| Progressive422 | 2,724.566 | 1.206 / 1.223 | 847,116 | Gap |
| ProgressiveCancelable444 | 4,821.634 | 1.438 / 1.448 | 984,212 | Gap |
| ValidateJpegProgressive420 | 291.290 | 1.087 / 1.094 | 4,072 | Gap |

The original 513×259 packets/settings and one-CPU complete-operation boundaries
remain. Decoding includes conversion, fresh independently owned RGBA8 and release;
complete-coding validation excludes RGBA normalization on both sides. The FFmpeg
software decoder resets per operation and reuses private contexts. The live token
is never signaled; native has no token option. Startup, I/O, IPC and validation
checks are excluded. Native allocation is unmeasured.

All managed frames equal their frozen native frames. Source/output lifetimes,
CLI/scalar correctness and quiet guards pass. Only 4:2:0 decode is confidence-open;
no case yet has a ≤1× conservative upper. No timing repeat or new control is added.
Full Windows/Linux/live qualification and the same actual Release package/three
examples pass. Original temporary metadata/proof files are restored exactly.

[Current optimization and qualification](WARM-JPEG-REFINEMENT-WINDOW.md) and
[numeric evidence](warm-jpeg-refinement-window-evidence.json) pin all actuals,
producers, hashes and mirrored/member-verified raw evidence. Earlier
[grayscale/live costs](current-progressive-jpeg-costs-evidence.json) and
[ordinary 4:4:4 cost](current-progressive-jpeg-444-cost-evidence.json) are retained
as historical sources; only the latter supplies the matched gain baseline.
The full 268-path objective remains open.
