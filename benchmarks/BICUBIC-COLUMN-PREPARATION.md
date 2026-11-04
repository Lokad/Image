# Bicubic column geometry preparation

The [subsequent word stage](BICUBIC-WORD-COLUMN-PREPARATION.md) reuses this helper
and qualifies its separate RGBA16 control; the byte-stage evidence below is retained.

The byte small-support column path now prepares vertical geometry once in already
owned, unpublished output-row tails. Aligned prefix strips finish before those
tails retire; the final strips rebuild geometry directly. Pixel kernels, tap
order, rounding/repair, portable fallbacks and the word implementation are unchanged.
Tiny results retain direct construction. There is no new owner, pool, cache or API.

The existing complete **513×259 RGBA8 →256×129, input+output+4 KiB** control has an
independent **8.84 µs /1.41% raw gain**. Filtered/all-actual raw and bracket-normalized
confidence separate. Normalized gain is9.59%, with the native bracket clock9.05%
slower; this is not a9.6% raw gain. The remaining ratio upper1.363 is still a gap.

| Complete resize | Managed µs | zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Current baseline, 12 actuals /11 filtered | 626.27 | 418.30 | 1.497 /1.528 | 113 |
| Selected geometry stage, independent 12 /12 | 617.43 | 456.16 | 1.354 /1.363 | 113 |
| Restored scheduling extension, independent 12 /12 | 612.21 | 437.31 | 1.400 /1.459 | 113 |

The selected stage's three-actual screen is614.35µs. A coherent extension also
prepared cache offsets/fill masks, using144B rather than96B per output row. Its
screen602.46µs and independent612.21µs remain in the evidence. Raw confidence
separates baseline, but normalized confidence narrowly overlaps; no incremental
gain over the simpler stage is established. Restore the simpler stage without
rebuilding or further sampling. This does not permanently exclude the design.

At the selected geometry,96B×129=12,384B of metadata borrows the existing132,096B
result capacity. Geometry construction falls from32 strip passes to one preparation
plus three tail passes. These are logical work counts, not timing attribution.
The original4,096B admission and4,080B fixed-array payload remain; added owned
capacity and retained operation scratch are zero. The result already has its
owner/charge before preparation and never escapes with metadata in its pixels.
Total113B/op is unchanged; complete physical/admitted peaks and native allocation
are unmeasured. Existing ungoverned pixel pooling remains separately bounded;
governed output ownership and terminal refusal are unchanged.

Reference is explicitly **zimg3.0.6, FFmpeg's zscale backend**, retaining graph,
input workspace and scratch before timing. Halo/deinterleaving, filtering,
independent output allocation/repacking/release stay timed. Managed complete public
Resize includes geometry/working preparation, admission, output ownership/disposal.
This retained-state boundary is not a one-shot setup-parity comparison. Managed
rational pixels remain exact; native RGB/alpha maximum1 and RMSE≤0.25 remain.

Both stages pass focused30internal/521public checks per normal/scalar mode on their
compiled platforms; the extension is Linux-only. The selected stage passes
**5,623 full/live checks per OS** (2,388internal,2,287public,948live) and nine final
column checks per normal/scalar mode. Six new cases cover borrowed-tail lifetime,
alignment, cancellation/recovery and exact odd-width generalization. The original
opaque151×83→75×41 reduction matches the independent public direct path, preserving
source and4KiB admission; this is functional coverage, not a broader CPU claim.
The **same actual Release package/all three examples** pass Windows/Linux, including
the frozen tight-memory column control and shared-owner survivors. Published
packages are unchanged; publication remains postponed.

The coherent round uses one Windows/two Linux production versions, three internal
test versions per OS and zero public-test/harness/native/selector builds. Final
test-only generalization reuses the measured production modules; selection restores
without builds. One broad/package milestone follows stage selection. An initial
baseline postcheck mistook inapplicable zero half-boundary counts for failed gates;
the corrected frozen-count comparison recovers every completed actual without retiming.

[Evidence](bicubic-column-preparation-evidence.json) pins sources, immutable
producers, every short/independent point, clock movements, qualification and actual
package. Mirrored/member-verified `bicubic-column-preparation-qualified.tar.xz`:
4,656,916B /313members, SHA256
`1451535c7a5858ebf477df9121bb396af38340e529b66dac81fa43ac70c6c27d`.
Both stages, originals, failure/recovery records, guards, complete actuals,
qualification, package and reproduction scripts remain, with dependencies pinned.
Only the displayed byte256×1294KiB cost changes. Word, shared streaming, Lanczos,
codec and text/drawing paths reuse unchanged evidence; all282 required paths
remain in scope and full parity remains open.
