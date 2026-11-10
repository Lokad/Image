# Narrow bicubic preparation boundaries

CBN781 makes no production change. It qualifies equivalent complete one-shot
and caller-owned prepared use on the existing original513×259→256×129 RGBA8/16
controls. These use the narrow-support row certificates, distinct from the wide
prepared controls. Only the owned public harness and native lifetime adapters
change. The actual package and compiled benchmark driver are reused.

| Complete boundary | Lokad µs | zimg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| One-shot RGBA8 | 494.39 | 1042.27 | 0.474 / 0.478 | 199,990 | pass |
| Prepared RGBA8 | 437.43 | 417.13 | 1.049 / 1.054 | 138 | gap |
| One-shot RGBA16 | 467.83 | 713.48 | 0.656 / 0.659 | 199,982 | pass |
| Prepared RGBA16 | 394.35 | 569.96 | 0.692 / 0.695 | 137 | pass |

All four have independent twelve-actual confirmations following the short screen,
with frozen full-frame managed/native hashes and both quiet guards. Three pass
and stop. Prepared RGBA8 has a20.30µs/4.87% point gap, conservative upper1.054.
The older fresh-managed/prepared-native Down8 comparison remains477.22/418.42µs;
its roughly59µs difference is not the equivalent prepared gap. Different current
views and harness costs are kept separately; this is no production speedup claim.

The reference is **zimg3.0.6, FFmpeg zscale’s backend**, not the FFmpeg CLI.
Original byte/word integer output, four-pixel halo, sampling/settings and native
pixel bounds remain. The wide float-output adapter is not substituted. The new
fresh adapter preserves original setup and the entire packing/filtering/output
body apart from two lifetime calls; normal/scalar frames match the original
retained workers byte for byte. Managed rational/certified pixels remain exact.

One-shot times graph/geometry, input-plane and scratch preparation, all source
reads, filtering, normalization, independently owned results and disposal on both
sides. Prepared execution retains explicit state on both sides, still processing
every source/output sample and disposing independent results. No output cache,
additional image or settings grid is introduced.

Each prepared plan retains199,840 B. Untimed governed checks measure331,936/
464,032 B peak owned capacity for one RGBA8/16 result, excluding borrowed input;
results retain132,096/264,192 B independently. Scratch retention ends with plan
disposal. Creation GC allocation, physical/JIT-stack peaks and native graph/input
retention or total allocations are unmeasured. Native ready-state scratch is
390,336 B. These are separate from138/137 B total GC allocation per prepared call.

The unchanged package retains [CBP780B](BICUBIC-SHARED-PREPARATION.md) qualification:
6,229 full/live checks per OS,362internal/776public resize checks per normal/
scalar/disabled-AVX2/AVX512 mode, and the same actual package/all three examples
on Windows/Linux. No production/embedded rebuild, full-suite repeat, repack or
package-proof repeat. One public harness build reuses its unchanged driver ABI;
each fresh native worker is built once. A measurement-wrapper syntax error was
caught before execution; no completed capture is restarted or sampled for a
favorable bound.

The two named prepared narrow paths extend285→287required paths. The two fresh
one-shot views reuse existing operations. [Evidence](bicubic-narrow-boundaries-evidence.json)
pins producers, current cases, gates, allocation/capacity, reused qualification
and all raw actuals. Mirrored/member-verified `bicubic-narrow-preparation-boundaries.tar.xz`
contains944,912 B/142members, SHA256
`b631b68a8450a5d7bf4d524a9763052c9443bfd38db932b4b2974ebac8d7c5d1`;
unchanged runtime/driver/package inputs come from its pinned CBP780B archive.
It includes the bounded public harness, source-exact lifetime adapters, build
recipe, gates and `cbn781-measure.py CASE short|confirm` reproduction. Publication
remains postponed. Resolve uncertain evidence before optimization, stop passing
cases, and prioritize larger attributable absolute savings.
