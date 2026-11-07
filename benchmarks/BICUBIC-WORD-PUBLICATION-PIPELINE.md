# Bicubic word publication pipeline

The three mixed/tight-vertical word costs are superseded by
[shared quartet scheduling](BICUBIC-MIXED-WORD-SCHEDULING.md). Other costs and
this enlargement-stage history remain valid.

Batching two independent pixel pairs before certification saves **130.43/135.29 µs
(4.11%/4.25% raw, 4.40%/4.20% reference-normalized)** on complete bounded/default
word enlargement. Independent original/candidate twelve-actual confirmations
separate in both filtered/all-actual views. Native clocks change +0.31%/−0.06%.
Both cases retain CPU gaps; this is an accepted incremental gain, not parity.

The row pipeline shares vertical-weight loads across four pixels and issues both
exact binary64 divisions before either certificate. Each pixel keeps its original
tap order; each pair retains its own acceptance bit and exact repair. The existing
pair and new quartet use one prepared-values certificate. Its original arithmetic,
bounds, guards, margins, clipping and packing reconstruct byte for byte; original
word/tiny-alpha repair bodies stay exact. This changes scheduling, rather than
replaying the earlier [vector-guard construction](BICUBIC-WORD-GUARD-VECTOR-REJECTED.md).
No precision policy, public API, cache, session or owned buffer is added.

All fifteen existing affected word controls now have current standalone costs.
Inputs remain the original deterministic 513×259 RGBA16 raster. `Tight` means
input+output+4 KiB; other rows retain their default workspace. Only the two full
enlargements have an incremental causal claim; the remaining costs are current
boundary evidence. Earlier output-allocation differences must not be attributed
to this pipeline.

| Existing control | Managed µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Up16 → 1027×519 | 3,048.24 | 2,873.36 | 1.061 / 1.067 | 4,264,573 | gap |
| TightUp16 → 1027×519 | 3,045.35 | 2,875.06 | 1.059 / 1.070 | 4,264,573 | gap |
| HorizontalUp16 → 1027×259 | 1,569.57 | 1,452.84 | 1.080 / 1.085 | 114 | gap |
| TightHorizontalUp16 → 1027×259 | 1,547.88 | 1,443.43 | 1.072 / 1.080 | 114 | gap |
| VerticalUp16 → 513×519 | 1,698.43 | 1,669.15 | 1.018 / 1.023 | 114 | gap |
| TightVerticalUp16 → 513×519 | 1,689.73 | 1,676.62 | 1.008 / 1.019 | 114 | uncertain |
| Down16 → 256×129 | 566.64 | 576.01 | 0.984 / 0.988 | 199,949 | pass |
| Horizontal16 → 256×259 | 667.63 | 577.80 | 1.155 / 1.161 | 199,924 | gap |
| Vertical16 → 513×129 | 660.49 | 592.04 | 1.116 / 1.125 | 384,013 | gap |
| Mixed16 → 1027×129 | 1,137.43 | 851.42 | 1.336 / 1.343 | 114 | gap |
| Cross16 → 256×519 | 940.78 | 819.83 | 1.148 / 1.154 | 199,893 | gap |
| TightHorizontal16 → 256×259 | 691.22 | 580.55 | 1.191 / 1.199 | 113 | gap |
| TightVertical16 → 513×129 | 718.36 | 590.25 | 1.217 / 1.225 | 113 | gap |
| TightMixed16 → 1027×129 | 1,134.10 | 852.37 | 1.331 / 1.336 | 114 | gap |
| TightCross16 → 256×519 | 967.78 | 821.74 | 1.178 / 1.182 | 114 | gap |

One pass stops (`Down16`); thirteen gaps and one uncertain case remain. The six
word enlargement controls are five gaps/one uncertain; unchanged byte enlargement
costs remain in [shared preparation](BICUBIC-SHARED-ENLARGEMENT.md). No new grid.
Every run retains twelve actuals, both native brackets, exact rational/frozen
managed pixels, original native-only bounds, source/independent ownership and quiet
CPU0/idle-v2 guards. The approved low-alpha vertical-word bound remains scoped.

Total measured GC allocation rises **4,264,538→4,264,573 B/op (+35)** on both primary
controls. The mandatory independent 4,264,104-byte result remains; existing 4,000-byte
fixed arrays/4 KiB admission are unchanged. No new owned capacity or cross-operation
retention. Existing default pixel-pool ceiling and governed bypass stay unchanged.
Shared-root refusal/cancellation/recovery and surviving outputs pass focused checks;
physical/native peaks are unmeasured. Do not infer capacity or CPU effects from GC.

Reference remains explicitly **zimg 3.0.6, FFmpeg zscale's backend**, with graph/input/
scratch prepared outside timing. Native conversion/halo/filtering/independent output/
quantization/release stay timed. Managed complete Resize, preparation/admission,
materialization, independent output and disposal stay timed. This is the declared
repeated-reference boundary, not one-shot setup parity; native float does not relax
managed exact rational/qualified-half results.

Each OS passes **5,812 full/live checks** (2,547 internal/2,317 public/948 differential),
plus 51 internal/571 public per normal/scalar mode. Four frozen 1,008-case guard
controls cover either pair position, valid/refused companions, independent rounding
and output canaries. Linux exercises AVX512; Windows/scalar runs cover portable
paths. Unchanged disabled-ISA proof is reused. The SAME actual private Release
package/all three examples passes both OSs, including both-depth/both-workspace
complete enlargement, exact packets and ownership. Published releases stay immutable.

One core/embedded build per OS; no public-test, harness, native, selector or fixture
build. An older-harness identity assumption is corrected before affected timings.
`Down16` initially selects a bilinear witness with the same case ID; the completed
bicubic samples are recovered against the correct family without any timing replay.
All failures and original evidence remain. Four summary rows refresh, 121 numbers
reuse unchanged paths. This is not a 125-row actual-package release refresh or the
full 283-path goal. Publication remains postponed.

[Evidence](bicubic-word-publication-pipeline-evidence.json) retains the comparisons,
fifteen controls, identities, ownership scope and actual-package qualification.
Raw `bicubic-word-publication-pipeline-qualified.tar.xz`, 1,914,832 B / 210 members,
SHA256 `006449bcfecf092b10268af7896f820a45ae326f615e3db8c6c51206baed4818`, is mirrored/member-verified. The
[geometry study/current attribution](BICUBIC-TAIL-GEOMETRY-EXPERIMENT.md) remains
separate history. Next select larger independent codec work from current attribution;
passing cases stop and uncertain cases need evidence before tuning.
