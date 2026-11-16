# Packed signed font bounds

Eager glyph-header validation compares both signed coordinate pairs together
after the same validated eight-byte read. SSE2 observes only the two relevant
lanes; the original scalar predicate is the portable fallback. Location checks,
mapping, zero/composite-header policy, errors, every glyph poll and ownership
remain. No new capacity, cache, pool, API, lazy validation or production dependency.

Independent complete owned font loading saves **1.288µs/6.53%**
across all twelve actuals, with separated raw intervals. Filtered6.43% also
separates. Reference-normalized13.57% separates while its clock moves+8.26%;
these are different views. The causal claim is the scoped raw effect, not a sum
of earlier stages or an overall text-processing speedup.

| Complete public operation / labeled reference | Managed / reference µs | Conservative upper / status | Managed GC B/op |
| --- | ---: | --- | ---: |
| Owned font load/dispose / FreeType2.13.2 | 18.424 / 14.327 | 1.324 / scoped gap | 176 |
| Load/fit/layout/draw/dispose / ImageSharp public integration | 1240.261 / 6633.727 | 0.208 / pass | 306,078 |
| Prepare once plus eight owned draws / ImageSharp public integration | 1237.224 / 6845.537 | 0.203 / pass | 315,808 |

References and frozen inputs are unchanged: DejaVuSans2.37Regular,757076bytes/
6253glyphs; FreeType2.13.2ownedcopy/open/retain/dispose; ImageSharp.Drawing2.1.7 /
Fonts2.1.3 / ImageSharp3.1.12complete public integration. Managed font validation
is eager and FreeType is lazy. Reference font families release through GC;
Lokad.Image ownership releases admission explicitly. This is not parser equivalence,
FFmpeg text performance or a bare shaping-engine comparison.

The existing ConstrainedFit16control includes all30fitting trials, every reference
public layout call, exact glyph/cluster/line/fitting decisions and position/bounds
tolerances, full16-bit samples and approved visual gates. Each operation owns a
fresh explicitly cleared canvas and disposes its owners; surviving outputs remain
independent. Managed preparation retains exact coverage while reference preparation
retains positioned paths and allocates per-draw raster storage.

Creation-only preparation is **1212.367/6550.435µs**;
only that diagnostic excludes prepared-owner disposal. The directly measured
eight-draw batch amortizes to **154.653/855.692µs/draw**,
including preparation. Allocations above cover complete operations/whole batches.
Do not reconstruct end-to-end cost by adding or subtracting separate stage times.
Reference complete one-shot/eight-draw GC is9,256,953/9,494,633B.

Font owned bytes757076+256B allowance, Latin prepared capacity2112B, complete-fit
capacity4160B and governed peak822460B are unchanged. Preparation allocates
314,467B; approximate full-GC retained heap deltas
4336/22504B exclude idle pools.
Allocation, retained capacity and peak live admission remain separate; physical,
native/JIT peaks are unmeasured. Caller-root calls bypass unchanged default idle storage.

Both OSes pass **6731full/live checks**, **267internal/163public** normal,
AVX-disabled and scalar checks, and the **SAME actual Release-package consumer /
allthreeexamples**. The new actual-validator test covers8898authored signed-header
cases plus10truncations, empty/composite policy, unaligned reads and poisoned
neighbors. Existing malformed fonts, cancellation, admission/cleanup/recovery,
independent faces/layouts and text behavior remain covered. The unchanged drawing
paths reuse their [qualified costs/frames](TEXT-PREPARED-COVERAGE.md); no retuning.

Current generated uint/ushort scans shrink537/553→510/526B and contain the packed
instructions. Instrumented elapsed is discarded. One core/embedded build perOS,
zero public/helper/harness/native/fixture builds and onepack. Affected performance
reconciles before this single broad/package milestone; the timed Linux DLL is
never rebuilt. The current [profile/extent trial](FONT-EXACT-EXTENT-TRIAL.md)
retains attribution and its inconclusive alternative. All observations stay linked.

[Evidence](font-packed-bounds-evidence.json) pins source proof, costs and actual
package `22f96b5ada3f2692de446a764a1ef6ae81c4ee963159142083b2d40e4c9c4f2a` containing timed DLL `8b76bfbf2b6f7b5db2d6dc04c01f5a155e130b81c2a4fdb1b4d7eb2b6f1af25e`.
Raw `font-packed-bounds-qualified.tar.xz` (4,738,956B/157members), SHA256
`102241c2f54f218d9b49d3567bc9b327b152bb25d0b31676be847cda005c8f5d`, is mirrored/member-verified with baseline/reference dependencies.
Published packages remain immutable; publication and full289-path completion stay open.
