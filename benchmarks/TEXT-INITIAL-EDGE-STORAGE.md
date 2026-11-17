# Single-pass initial edge storage

Complete ordinary RGBA8 drawing independently saves **1.323 µs /2.56%** on the
final timed DLL: **51.699457 →50.376303 µs**, paired ratio interval
**.968268–.980547**. Raw mean intervals overlap; the paired confidence establishes
this scoped effect. Public allocation falls **83,216 →75,840 B/draw**. The
prototype's larger effect and historical clocks are not cumulative gains.

| Current complete operation | Lokad / reference µs | Ratio upper / status | Managed GC B/op |
| --- | ---: | --- | ---: |
| Ordinary RGBA8 | 46.439 /45.079 | 1.042030 / uncertain | 75,840 |
| Ordinary RGBA16 | 49.224 /46.493 | 1.116692 / uncertain | 75,840 |
| Authored clipped RGBA8 layout + draw | 37.624 /224.872 | .295642 / pass | 41,064 |
| Authored clipped RGBA16 layout + draw | 40.435 /239.773 | .335218 / pass | 41,064 |
| Complete constrained-fit one-shot | 1,109.654 /6,188.340 | .195146 / pass | 298,790 |
| Complete preparation + eight draws | 1,309.647 /6,828.242 | .219046 / pass | 308,518 |

References are stock ImageSharp.Drawing 2.1.7 /Fonts 2.1.3 /ImageSharp 3.1.12 for
ordinary/complete integration and explicitly [source-corrected tooling](AUTHORED-FONT-COMPARISONS.md)
for the authored controls. Every original layout call, fitting trial, canvas reset,
quality gate and ownership boundary remains. These are public integration costs,
not FFmpeg or bare shaping-engine comparisons. Preparation-only remains diagnostic;
historical stages are never summed. All twelve actuals/reference brackets stay.

One bounded 64-edge stack prefix replaces early 8/16/32/64-element arrays during
the existing traversal. Small shapes materialize one exact owned array; larger
shapes spill into the original geometric growth. Initialized prefixes copy before
old capacity is released. No stack view escapes. Ordered geometry, events, scan
conversion, blending and pixels remain verbatim. Shapes are private and built once;
failure abandons their constructor and cleans up before any canvas mutation.
No counting pass, retained hint, cache, pool, public API or admission model is added.

The 4,992 B actual edge/curve scratch uses existing coarse frame admission.
Observed core frame chains are 5,504 B on Linux and 5,744 B on Windows; the minor
48 B Windows ABI difference from the 5,696 B base/curve estimate remains within
coarse descriptor/runtime accounting. Physical/runtime/JIT peaks are unmeasured.
The authored rectangle's logical peak falls **6,744 →6,408 B**; the complete-fit
governed peak falls **822,460 →821,900 B**, with retained coverage **4,160 B** unchanged.
The existing Arabic control preserves exact frames and reduces allocation
**590,824 →501,600 B/draw**. Its separate-block timing is inconclusive; no broader
speedup or regression is claimed. Timed ordinary adapters add 24 B; authored BDN
scaffolding adds 736 B separately from its public allocation observations.

Windows/Linux each pass **6,803 full/live checks**, **292 internal/222 public**
normal/AVX-disabled/scalar checks and the **same actual Release package/all three
examples**. Nine new controls cover empty, initial/exact/spilled prefixes,
independent input, charged capacity and release; every-growth refusal/cancellation
tests remain. The timed Linux DLL is never rebuilt. New tests are normalized to
repository LF before broad qualification; only their embedded assemblies rebuild.

The initial final-cost controller used the older 4,096-call integration. It stays
diagnostic; one restoration of the established 16,384-call controller supplies the
ordinary rows above on the same packaged DLL. Unchanged qualification is reused.
A cached worker alias is corrected before sampling; a postcheck updates only the
intended 560 B governed-peak reduction. Completed samples/checks are never replayed.
The five authored/layout passes and unaffected font, prepared execution, codec,
resizing and copy evidence remain stopped and identified. Full 289-path parity and
the final complete package refresh remain open; publication stays postponed.

[Evidence](text-initial-edge-storage-evidence.json) pins actuals, sources and package.
Mirrored/member-verified `text-initial-edge-storage-qualified.tar.xz`:
**2,817,836 B /255 members**, SHA256
`2f59fb14f7537bdd7e8aadddb2e236c71746a1f00b53d1a2535ae59e3cfa9ede`.
Archived `tsp1199-*` / `tes1200-*` recipes and declared dependencies reproduce the
round. Resolve the allocation-history dependency through the retained
`text-edge-capacity-trials` manifest; its original attribution remains intact.
