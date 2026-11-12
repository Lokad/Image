# Compact bicubic paired weights

The existing packed horizontal coefficients now use two wider loads and direct
per-tap selection. All eight multiply/add operations retain their original order
and exact bits. Row scheduling, repair, storage/loan boundaries, cancellation,
admission and independent output ownership are unchanged. No prepared-state span,
kernel family, pool, cache, owner or public API is added.

| Complete bounded 513×259→256×129 RGBA8 resize | Baseline → current µs | Prepared zimg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| TightDown8 | 419.96 → 414.18 | 418.13 | 0.991 / 0.998 | 113 |

Independent confirmation saves **5.77 µs / 1.38% raw**; all-actual savings are
6.24 µs / 1.48%. Both raw interval pairs separate. The normalized 1.02% point gain
has overlapping confidence, with native mean moving −0.36%; do not describe that
as a confirmed normalized gain. The current standalone control passes/stops. Its
contemporary baseline was uncertain, so this does not attribute historical gap
closure solely to production code or establish a general speedup.

A three-actual screen precedes one independent 12-actual baseline/candidate
confirmation on the same immutable build, quiet VM, CPU0, optimized JIT and
workstation nonconcurrent GC. Every actual and native bracket remains, including
the baseline outlier. Original inputs/settings and complete boundaries are retained:
managed public Resize/preparation/filtering/repair/output/disposal against the
labeled FFmpeg zscale backend with retained zimg graph/scratch and timed source/
output normalization and independent ownership. This is not the separate equivalent
one-shot/prepared comparison. Exact rational samples and approved native bounds pass.

All 64 coefficient selections are proven identical to the old packed layout.
Existing independent ordered-bit oracles cover negative/signed-zero weights and
varied alpha/source patterns; complete pixel, poisoned phase, minimum-loan,
refusal/cancellation/cleanup and survivor checks also pass. Original production
source reconstructs outside the single changed leaf. Its guarded byteColumn call
reaches only TightDown8 in the current cost inventory: default Down8 selects the
unchanged cached path first, and prepared controls retain their cached workspace.
The actual shared Resize caller remains exactly 9,388 code bytes after address/
method-number normalization. Other 72 resize and unrelated-family costs reuse
valid evidence. Current resize scope is 73 controls: 64 pass / zero gaps / nine
uncertain. Nine uncertainties remain within the full 289-material-path objective.

Allocations remain 113 B/op. Output loans remain 11,776/12,800 B, local payloads
3,424/4,080/3,232 B and scratch admission 4 KiB; retained operation scratch is zero.
No new owned capacity is added. Physical/native/JIT peaks are unmeasured. Loop code
grows 6,612→6,730 B and 6,528→6,638 B; instruction counts are not CPU proof.

After affected costs reconcile, Windows/Linux each pass 6,512 full/live checks
(3,183 internal + 2,381 public + 948 live), 570 internal/791 public focused checks
per normal/scalar mode and all three named storage suites. Unchanged guards and
portable/word paths reuse disabled-ISA evidence. The SAME actual Release package
passes both consumer runs/all three usage examples, including frozen byteColumn
pixels, aggregate admission, cancellation and surviving owners. Its DLL is exactly
the timed Windows module. One core/embedded build per OS, no public/harness/native/
fixture builds, one package milestone; published releases are unchanged.

[Evidence](compact-bicubic-paired-weights-evidence.json) pins all costs, scoped
source/JIT/ownership proof, qualification and the actual 455,748-byte development
package. Raw `compact-bicubic-paired-weights-qualified.tar.xz`, SHA256
`1007e5e9b0677feb2b81c1bfbda55ae12d206b8bac78f1cfd28b5a5738510cc8`
(1,422,452 B / 93 members), is mirrored/member-verified. Failed expanded layouts
remain [separate history](COLUMN-COEFFICIENT-PREPARATION-TRIAL.md), outside renderer
dependencies. Publication is postponed. Continue the next demonstrated material
gap or justified uncertainty-resolution task; stop this passing control.
