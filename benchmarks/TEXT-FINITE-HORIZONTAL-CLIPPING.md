# Certified finite horizontal text clipping

Two clipping fast paths replace horizontal-intersection ratios multiplied by zero
with their exact signed-zero additions. A finite-sum guard certifies finite
operands; overflow and nonfinite cases retain the original arithmetic. The left
bottom intersection preserves negative zero when its endpoint is zero. Polygon,
shoelace, coverage, glyph order, blending, polls and ownership are unchanged.
There is no new array, owner, cache, pool, API or numerical relaxation.

Independent complete ordinary16 pairs measure **55.325→53.656 µs**, a **3.01% /
1.668 µs** scoped gain, interval **0.956865–0.983045**. Both all-actual mean
intervals separate. The earlier screen, every actual and original gates remain.
Each comparison uses 12 alternating 1,024-operation batches, natural nonconcurrent GC
and one CPU0 operation. Reset, validation and allocation reads are outside clocks.
This is a controlled DLL effect, not an overall text-processing gain.

| Original public boundary / ImageSharp.Drawing 2.1.7, Fonts 2.1.3, ImageSharp 3.1.12 | Managed / reference µs | Upper / status | Managed GC B/op |
| --- | ---: | --- | ---: |
| Ordinary8 | 51.797 / 45.097 | 1.283 / uncertain | 83,136 |
| Ordinary16 | 55.052 / 46.155 | 1.326 / gap | 83,136 |
| Complete OneShot | 1140.191 / 6177.274 | 0.213 / pass | 306,084.17 |
| Complete PrepareDraw8 | 1236.276 / 6804.000 | 0.202 / pass | 315,808.33 |

Contemporary standalone ordinary16 baseline/candidate points are 56.543/55.052 µs;
they reconcile the controlled direction. Ordinary8 stays uncertain; ordinary16
retains a scoped gap. Neither new point is a historical pipeline speedup.
Complete public integrations retain all 30 fitting trials/public layout calls,
fresh owned explicitly cleared canvases, full 16-bit visual/layout/fitting gates
and disposal. Preparation 1125.333/6040.427 µs is diagnostic; direct eight-draw
amortization 154.535/850.500 µs includes preparation. Eager/lazy font validation,
GC-driven reference font release and managed coverage/reference positioned-path
retention remain explicit. Text is never described as FFmpeg or bare shaping.

The existing Arabic control is 431.155→423.199 µs with overlapping intervals;
no confirmed generalization gain/regression. Its allocation stays 590,552 B, Latin
83,136 B. Prepared execution is unchanged and reuses its passing costs. Retained
Latin 2,112 B, fit 4,160 B and full-fit governed peak 822,460 B remain unchanged;
preparation allocates 314,471 B. Allocation, admitted capacity and retained state
are separate; physical/native/JIT peaks are unmeasured.

The new independent always-clipped oracle covers 316,224 finite/extreme/subnormal/
signed-zero/nonfinite cases with canaries. Existing oracles and all eight frozen
full-word frames pass Windows/Linux normal/AVX-off/scalar. Each OS passes 6,743 full/live
checks plus 268 internal/163 public focused checks per mode. The same actual package
and all three consumer examples pass both OSs. One core/embedded build per OS, zero
public/harness/native/fixture builds, one NoBuild pack and no timed-Linux rebuild.
The completed RGBA8 capture's summary KeyError is recovered from its passing
receipt; no timing job is repeated. The Windows frame helper is reused as a
verified portable DLL after an untimed missing-path preflight.

[Evidence](text-finite-horizontal-clipping-evidence.json) pins scope, source proof,
all actuals and package `a0e4f728cd51ff1f525be99ccc206d752741e12a4a6f0b270ff02a232f1a520f` / timed DLL
`878f2bd106ca3ca428ea4145b47fd8d7e2398d507bf4807701ea4104518ce70c`. Raw `text-finite-horizontal-clipping-qualified.tar.xz`
(4,751,988 B/195 members), SHA256
`a0c09f829d4eed391a8a1bae6cc4448cb1f855d50b51543de48e76c3c3f13988`, is mirrored/member-verified with prior producer/reference dependencies.
Reproduce with archived fhc1119 prepare/screen/confirm/cost/qualify/package recipes
on the pinned input/tool identities; completed captures are never restarted.
Other codec/resize/font paths reuse unchanged evidence. Publication is postponed;
Full 289-path parity remains open and published packages are immutable.
