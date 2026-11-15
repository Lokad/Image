# Direct exact quadrilateral area

TQA1045 removes polygon materialization and generic clipping traversal for three
proved four-vertex topologies: inside both boundaries, left-only clipping and
right-only clipping. The original clipper remains the fallback. Intersections
retain their subtraction/division/multiplication/addition expressions, including
zero terms; area retains the original four ordered shoelace terms. No reciprocal,
FMA, numerical-policy, cancellation, allocation or ownership change is introduced.

The current-module read-only profile (NTA1044) captured 1,993 locations across
25,146 checked draws: Area 170 and Clip 153. These are attribution leads, not phase
costs or a gain forecast; instrumented elapsed is discarded.

Independent contemporary prepared-RGBA16 comparisons save **8.612 µs / 17.085% raw**
and **17.278% with paired reference normalization**; both intervals separate.
Wider bracket-normalized gain remains inconclusive. Paired reference clock moves
+0.226%. Every actual and reference group remains, with no favorable primary repeat.

| Public boundary | Lokad.Image µs | ImageSharp µs | Ratio / upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Ordinary RGBA8 | 58.159 | 44.723 | 1.300 / 1.585 | 59,304 | Uncertain |
| Ordinary RGBA16 | 61.651 | 46.495 | 1.326 / 1.602 | 59,304 | Scoped gap |
| Prepared RGBA8 | 38.990 | 37.107 | 1.051 / 1.204 | 56 | Uncertain |
| Prepared RGBA16 | 41.795 | 37.889 | 1.103 / 1.307 | 56 | Uncertain |
| Complete one-shot | 1,152.648 | 6,158.685 | 0.187 / 0.210 | 283,278 | Pass/stop |
| Preparation plus eight owned draws | 1,730.292 | 6,805.463 | 0.254 / 0.285 | 284,626 | Pass/stop |

Reference identity remains **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp
3.1.12**, never FFmpeg or a bare shaping engine. Ordinary/prepared drawing uses
frozen DejaVu Sans 2.37 Regular “AV To”, 18 px, 384×160, origin 8/8; each actual
groups 1,024 individually reset complete draws. Public counters above exclude
the timed adapter's additional 24 B/draw. Managed output remains exact; reference
visual gates and layout/cluster/line/fitting tolerances remain unchanged.

The two complete public-API integrations use the frozen ConstrainedFit RGBA16
control: all thirty fitting trials and every public reference layout call, fresh
owned explicitly cleared canvases and disposal. Eight-draw amortization is
**216.286 / 850.683 µs/draw**, including preparation. Creation-only
preparation is **1,081.459 / 6,091.260 µs**; disposal is excluded only from that diagnostic.
The separate complete-fit prepared-draw diagnostic remains uncertain. Never add
historical stage timings or infer overall slowdown from individual diagnostics.

One existing frozen ArabicMarksDigits RGBA16 generalization compares contemporary
managed modules: **599.376→515.753 µs**, saving **83.623 µs / 13.952%**,
with separated independent intervals and unchanged **441,696 B/draw**. This is not
an ImageSharp/FFmpeg elapsed or parity comparison and makes no broader font claim.

Latin active raster/preparation peak stay **40,408 / 42,488 B**; complete-fit active
capacity/governed peak stay **39,904 / 807,708 B**. No font/layout is retained by
preparation. GC allocations, admitted peak, active retention and idle storage are
separate; GC heap deltas exclude idle pools and are approximate. Physical/native/
JIT process peaks remain unmeasured. Parser validation and retained preparation
still differ between libraries; [font-loading evidence](FONT-HEADER-VALIDATION.md)
remains valid and scoped. No overall pipeline gain is claimed.

The existing always-clipped oracle checks 30,736 finite/generated and 4,096 special
inputs against exact bits/NaN classification, with poisoned reused spans/canaries.
All eight frozen narrow/Latin/Arabic/fitting frames, normal/scalar checks and owner,
admission, cancellation and recovery gates pass. Each OS passes **6,721 full/live
checks**, **265 internal / 161 public** focused checks per mode, and the **same
actual timed Release-package consumer / all three examples**. One core/embedded
build per OS, no harness/reference/fixture build, one pack, no Linux rebuild after
timing. Unchanged codec/resize/font-loading/layout-only evidence is reused.

[Evidence](text-direct-quadrilateral-area-evidence.json) records source proofs,
boundaries and complete observations. Mirrored/member-verified raw
`tmp/text-direct-quadrilateral-area-qualified.tar.xz`: 5,293,920 B / 212 members,
SHA-256 `898beb44f60d271548a3ee8544282fb1618cece5693a1c2722852c96cef7158f`.
Actual package: 465,545 B, SHA-256 `532a5f72a55eff993d18bcf4c8eb0175344a494ccbbb8e0736de4dd9b23b9c19`.

[Natural-run preparation](TEXT-NATURAL-RUN-PREPARATION.md) retains its prior costs,
adverse captures and prepared8 launch-sensitivity control; these are history,
not a gain baseline for this stage. All four drawing costs and both integrations
are refreshed because shared Area execution changes. No whole-library refresh,
new workload grid or publication; the complete 289-path objective remains open.
