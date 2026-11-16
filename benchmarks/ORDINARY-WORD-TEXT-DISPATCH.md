# Ordinary word text dispatch

Ordinary RGBA16 drawing avoids three general channel quotients when destination
alpha is zero. For positive effective source alpha, the original channel numerator
is source times `sa × 65535²`, and its divisor is that same positive weight. The
rounded result is therefore exactly the supplied word channel. Alpha rounding,
hidden RGB when stored alpha rounds to zero, untouched pixels and zero-source
no-op remain unchanged. Nonzero destination alpha uses the original rational blend.

This is a small dispatch in ordinary word drawing. Shared/prepared blending,
byte blending, geometry, sorting, cancellation, admission and ownership stay
unchanged. There is no added array, cache, pool, retained capacity or public API.
The [earlier shared-helper trial](TEXT-TRANSPARENT-DESTINATION-EXPERIMENT.md)
remains inconclusive. The current ordinary-only call boundary and current profile
justify this revised implementation; the old result supplies no gain claim.

Independent controlled pairs on the **final actual-package DLL** measure
**50.703 → 46.559 µs**, saving **4.144 µs / 8.17%**, interval **0.906710–0.929977**.
All twelve alternating 1,024-operation pairs and raw mean intervals separate;
natural nonconcurrent GC, CPU0 and versioned quiet guards pass. Public allocation
remains **83,216 B/draw**, plus the existing 24 B timed adapter. The promising
three-pair screen and separate isolated-build confirmation remain retained.
A rejected I/O quietness check preceded the first confirmation attempt: zero
workers/samples started; the next guard passed before execution. No timed actual
is removed or replaced. A targeted final-DLL pair reconciles the isolated build
with its actual immutable production artifact.

| Final complete public boundary / labeled ImageSharp integration | Managed / reference µs | Conservative ratio upper | Managed allocation B/op |
| --- | ---: | ---: | ---: |
| Ordinary RGBA8 | 47.783 / 45.278 | 1.095680 / uncertain | 83,216 |
| Ordinary RGBA16 | 49.516 / 48.324 | 1.119569 / uncertain | 83,216 |
| Authored RGBA8 layout + draw | 42.013 / 244.089 | 0.319341 / pass | 49,448 |
| Authored RGBA16 layout + draw | 40.357 / 246.852 | 0.268106 / pass | 49,448 |
| Owning one-shot text | 1264.473 / 6645.076 | 0.213391 / pass | 306,162.25 |
| Owning preparation + eight draws | 1363.248 / 7422.880 | 0.212100 / pass | 315,896 |

Paired DLL gains do **not** establish standalone parity. Ordinary drawing retains
layout/font versus reference positioned paths; necessary geometry/raster work,
composition and cleanup stay inside, every reset/check outside, with two clock
reads per operation. The [fixed warm protocol](ORDINARY-TEXT-WARM-CONFIDENCE.md)
uses 4,096 operations before all statistical groups and per actual. All twelve
managed actuals and three reference groups remain. Current intervals include
parity: **0.962359–1.095680 / 0.937233–1.119569**. Earlier gaps and all uncertain
or adverse captures remain; no repeated sampling merely to obtain a pass.

Authored controls use the explicitly [source-corrected Fonts/Drawing reference](AUTHORED-FONT-COMPARISONS.md),
all nine public layout calls/tab adaptation, complete layout/coverage/drawing,
independent results/disposal and original reset boundaries. Ordinary and owning
controls retain stock ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12.
These are public integration workloads, never FFmpeg or bare shaping performance.
Owning controls include supplied-font loading, every layout call, all thirty
fitting trials, fresh cleared independent RGBA16 canvases and cleanup. Preparation
1234.276/6603.153 µs is diagnostic; directly timed eight-draw amortization is
170.406/927.860 µs/draw including preparation. Historical stage times are not added.
Font validation and retained reference state still differ; no overall slowdown or
broader font/content claim follows.

Ordinary governed capacity and retained scratch are unchanged. Prepared execution
stays 56 B/op on its unchanged source path; retained Latin/fit capacities remain
2,112/4,160 B and complete-fit governed peak 822,460 B. Approximate prepared heap
deltas remain 4,336/22,504 B, separate from allocations, idle pools and unmeasured
physical/native/JIT peaks. Owning reference allocation is 9,256,953/9,494,633 B;
the separate ordinary reference worker does not measure allocation. Allocation
noise in owning diagnostics is not a claimed storage reduction.

One new public regression checks **196 full-precision color/alpha/destination
cases, 392 clipped/overlapping draws and 401,408 word pixels** against unchanged
prepared rational blending, including transparent hidden RGB and low alpha. Shared
root charges remain constant between operations and return to zero after disposal.
The existing independent BigInteger composition coverage and frozen frames remain.
Windows/Linux pass **6,750 full/live checks each**, plus **268 internal/166 public**
checks per normal/scalar mode. The **same actual Release package / all three usage
examples** pass both OSes. Published 0.2.0 remains byte-identical.

One final core/embedded/public build per OS, one isolated Linux prototype, one
small Windows probe, zero existing harness/reference/fixture builds and one
NoBuild Release pack. The accepted timed Linux DLL is not rebuilt. Qualification
follows affected performance reconciliation. A final record assumed the published
0.2.0 artifact existed on the VM; full tests, pack and consumer had already passed.
Only that record was resumed, verifying the published artifact on Windows, with
zero repeated tests, builds, packing or consumer checks.

[Evidence](ordinary-word-text-dispatch-evidence.json) pins sources, all actuals,
producers, exact package, qualification and unchanged-path reuse. Its mirrored,
member-verified raw archive retains the prototype/final implementations and
runtimes, pixel probe, guards, screen/confirmations, rejected preflight, complete
comparisons, qualification logs and actual package/symbols. Archived `tdw1145-*`
scripts reproduce the bounded round with its declared dependencies.

Stop unchanged prepared/layout/font/codec/resize passes. Meaningful owned-font
loading costs and copy gaps remain available while ordinary uncertainty stays
open pending a justified measurement/design change. This is a scoped accepted
gain; the full 289-path objective remains open and publication stays postponed.
