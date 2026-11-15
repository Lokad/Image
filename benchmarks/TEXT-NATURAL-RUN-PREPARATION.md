# Natural-run text preparation

NRS1043C exploits monotone edge/event runs during geometry preparation. Short
ranges merge through a 2 KiB stack buffer; larger ranges split, rotate and merge
with a bounded nonrecursive pending stack. Excessive run counts use the original
depth-bounded sorter. The original active-crossing sort and Band body are restored
verbatim, keeping adaptive dispatch out of steady drawing. Exact paired endpoint
evaluation is also reused for intersection preparation.

The final ordinary-RGBA16 comparison saves **8.906 µs / 11.196% raw** and
**12.106% with paired reference normalization**; both intervals separate. Wider
bracket-normalized gain remains inconclusive. The paired reference clock moves
+1.022%. The verified immutable baseline capture is reused within this bounded
round; all final observations use the actual timed module, with no favorable
primary repeat or historical-stage sum.

| Public boundary | Lokad.Image µs | ImageSharp µs | Ratio / upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Ordinary RGBA8 | 66.757 | 45.139 | 1.479 / 1.800 | 59,304 | Scoped gap |
| Ordinary RGBA16 | 70.642 | 47.367 | 1.491 / 1.833 | 59,304 | Scoped gap |
| Prepared RGBA8 | 55.843 | 37.193 | 1.501 / 1.766 | 56 | Scoped gap; launch-sensitive |
| Prepared RGBA16 | 51.040 | 37.935 | 1.345 / 1.566 | 56 | Scoped gap |
| Complete one-shot | 1,184.966 | 6,187.373 | .192 / .218 | 283,276 | Pass/stop |
| Preparation plus eight owned draws | 1,853.704 | 6,894.140 | .269 / .293 | 284,622 | Pass/stop |

Drawing retains frozen DejaVu Sans 2.37 Regular “AV To”, 18 px, 384×160 and origin
8/8. Each actual groups 1,024 individually reset operations; both sides use two
timestamps per draw. Exact managed frame checks and allocation counters are outside
timing. All twelve actuals and three reference groups remain. Table GC uses current
public-only counters; timed dispatch adds 24 B/draw.

Prepared RGBA8 is launch-sensitive: the **same DLL** produced 55.843 and 47.194 µs.
One targeted current baseline/candidate comparison measured 47.637/47.194 µs and
did not reproduce an adverse incremental effect. The specific cause is unmeasured.
The table retains the original higher cost and upper bound; no further sampling
or uniform prepared-drawing gain is claimed. The original capture is not discarded.

One bounded generalization on the **existing frozen ArabicMarksDigits RGBA16**
control improves **739.251→565.409 µs**, saving **173.842 µs / 23.516% raw**, with
separated independent intervals and unchanged **441,696 B/draw**. Both managed
modules preserve exact full-word output and identical operation boundaries. This
is a managed-DLL comparison, with no external-reference elapsed or parity claim.

The complete ConstrainedFit16 workloads include all thirty fitting trials, every
public reference layout call, fresh owned cleared canvases and disposal. Eight-draw
amortization is **231.713 / 861.767 µs**, including preparation. Creation-only
preparation is **1,077.884 / 6,019.172 µs**, with disposal excluded only from that
diagnostic. The separate fit prepared-draw diagnostic is a scoped gap. Never infer
overall text performance by adding historical stage timings.

Reference identity remains **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp
3.1.12**. Managed plans retain geometry/crossings/workspace; reference Fill retains
positioned paths and prepares raster workspace per draw. Unchanged [FreeType 2.13.2
loading](FONT-HEADER-VALIDATION.md) retains its eager/lazy-validation disclosure.
These are public integration/visual comparisons, not bare shaping or FFmpeg.

Explicit temporary stack capacity is **2,560 B**, covered by the existing admitted
4,096 B frame allowance. There is no recursive merge or new heap/retained storage.
Latin active raster/preparation peak remain **40,408 / 42,488 B**; complete-fit
active capacity/governed peak remain **39,904 / 807,708 B**. Allocation, admitted
capacity, active retention, idle pools and unmeasured physical/native/JIT peaks
stay separate. Admission refusal is terminal; algorithm fallback never retries it.

Normal/scalar checks retain all eight exact frames, full-word/layout/visual gates,
payload ordering, signed-zero/NaN multisets, both buffer directions, large splits,
run fallback, admission and deterministic cancellation/recovery. Windows/Linux each
pass **6,721 full/live checks**, **265 internal / 161 public** focused checks per
mode, and the **same actual timed Release package / three examples**. Four coherent
Linux prototypes precede one broad/package milestone; Windows builds once. No
public/reference/fixture rebuild, one bounded probe build, one pack, and no final
Linux rebuild after timing. Intermediate screens and adverse captures are retained.

[Evidence](text-natural-run-preparation-evidence.json) records current scope,
source proofs and history. Mirrored/member-verified raw
`tmp/text-natural-run-preparation-qualified.tar.xz`: 6,226,836 B / 335 members,
SHA-256 `3d26fbc2072fc43b66cea889f661c41dc4e828b80b6d037d0755518e3917915f`.
Actual package: 465,409 B, SHA-256
`623215f5651ed9208aaa7d9f4ee605ab685a09e8af806f00debfc32fd5d3e2b4`.
Unchanged codec/resize/font-layout evidence is reused. Publication remains postponed.
