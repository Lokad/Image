# Resize execution attribution before word preparation and paired output

This capture predates word-row preparation and
[paired row output](LANCZOS-PAIRED-ROW-OUTPUT.md). Retain it as the reproducible
basis for that work; its affected hot offsets/shares are historical attribution,
not current component costs or remaining savings.

One two-second quiet-VM capture reuses the frozen public prepared-Lanczos worker
with the current qualified DLL. The existing 513×259 → 256×129 RGBA8 control
preserves exact output and source/ownership/native checks. Preparation is outside
both prepared operations; each execution reads current pixels and independently
owns/disposes its output. No builds or production changes. Profiled elapsed is
discarded; sampled regions and instruction skid are not component timings.

| Sampled location | Samples, out of 1,995 |
| --- | ---: |
| Exact horizontal byte filtering | 1,130 |
| FMA execution/normalization wrapper | 391 |
| Vertical FMA accumulation | 386 |
| Exact-row to double conversion wrapper | 66 |
| Horizontal dispatch | 3 |

No exact-pixel repair samples are observed; this does not establish zero cost.
The evidence supports investigating horizontal execution before weakening any
arithmetic guarantee. Its hot instructions span bounds/cancellation checks,
coefficient preparation and arithmetic, so sample counts do not establish the
gain available from any single change.

Lanczos already shares admitted axes, row/sum workspace and one-shot/prepared
execution. Cached bicubic already shares geometry, row scheduling and decoded
source products across support ranges. Retain those owners and useful kernels.
Combining their Q30 and floating/certified pixel loops into one generic loop is
not justified by this audit.

One concrete repeated operation remains: Lanczos widens samples and forms alpha
products inside every horizontal tap. The frozen geometry has 3,060 taps across
513 source pixels per row, about six uses per source pixel. Cached bicubic forms
its source products once per row. These are geometry-derived counts, not measured
costs or a claim about actual row-cache misses.

The next bounded design prepares source products once in operation-owned row
capacity and shares exact filtering preparation between byte/word paths. Preserve
specialized multiplication and portable fallbacks; introduce no cache framework
or per-pixel generic dispatch. Additional row storage/preparation must be admitted
and reported, including retained plan capacity and complete one-shot cost. Exact
Q30/rational results, certified repair, cancellation and surviving result lifetimes
remain required. Screen existing prepared controls first; confirm useful completed
stages and qualify affected paths/package at a coherent milestone.

Reuse unchanged [preparation and boundary evidence](LANCZOS3-PREPARED-EXECUTION.md)
and [bicubic ownership evidence](RESIZE-OVERWRITE-OUTPUT-STORAGE.md). Their retained
costs are not a new release refresh. [Evidence](resize-current-execution-attribution-evidence.json)
pins current sources, actual workers, exact output and the failed setup/correction.
Raw `resize-current-execution-attribution.tar.xz`: 802,928 B / 48 members,
SHA256 `0be7f373e434e447e83b8863a8f318e1e9a8fa28717b1d43544e50f4f21a8175`,
mirrored/member-verified, with pinned qualified-core and prepared-worker archives.
