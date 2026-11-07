# Shared byte/word bicubic enlargement

Word costs are superseded by the [publication pipeline](BICUBIC-WORD-PUBLICATION-PIPELINE.md).
Byte costs and unchanged workspace evidence remain valid; this report retains
the earlier consolidation and its adverse stages.

Byte and word enlargement now share geometry/filter preparation and two
output-backed prefix/tail schedules. Typed decoding, certified pixel publication
and exact repair remain separate. This removes duplicated scheduling while adding
word output reuse: enlargement sources shrink from 613 to 572 lines, with three
independent schedules reduced to two. No new owner, pool, cache or session.

Independent complete default word enlargement improves **3,608.43→3,429.17 µs:
179.26 µs / 4.97% raw and 4.90% reference-normalized**. Both filtered/all-actual
confidence views separate; native time changes −0.074%. Allocation falls
**5,016,602→4,264,538 B/op**, retaining the required independently owned 4,264,104 B
output. Do not describe that output as removable scratch or enlarge the idle pool.

| Existing 513×259 source / zimg boundary | Managed µs | Reference µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8, 1027×519, default | 3,315.52 | 3,040.01 | 1.091 / 1.120 | 116 | uncertain |
| RGBA16, 1027×519, default | 3,429.17 | 3,125.34 | 1.097 / 1.113 | 4,264,538 | gap |
| RGBA8, 1027×259, default | 1,538.01 | 1,520.28 | 1.012 / 1.040 | 114 | uncertain |
| RGBA16, 1027×259, default | 1,599.46 | 1,457.53 | 1.097 / 1.104 | 114 | gap |
| RGBA8, 513×519, default | 1,927.92 | 1,887.15 | 1.022 / 1.046 | 115 | uncertain |
| RGBA16, 513×519, default | 1,769.95 | 1,670.93 | 1.059 / 1.087 | 114 | gap |
| RGBA8, 1027×519, 4 KiB extra | 3,212.58 | 3,052.03 | 1.053 / 1.072 | 116 | gap |
| RGBA16, 1027×519, 4 KiB extra | 3,444.03 | 3,070.60 | 1.122 / 1.148 | 4,264,538 | gap |
| RGBA8, 1027×259, 4 KiB extra | 1,546.70 | 1,524.36 | 1.015 / 1.066 | 114 | uncertain |
| RGBA16, 1027×259, 4 KiB extra | 1,641.27 | 1,470.19 | 1.116 / 1.136 | 114 | gap |
| RGBA8, 513×519, 4 KiB extra | 1,811.37 | 1,745.56 | 1.038 / 1.061 | 115 | gap |
| RGBA16, 513×519, 4 KiB extra | 1,799.72 | 1,699.62 | 1.059 / 1.102 | 115 | gap |

All twelve existing full/axial/depth/default/bounded controls refresh; eight remain
gaps and four are uncertain. Every run retains twelve actuals, both native brackets,
frozen managed frames, source/independent ownership, original native pixel bounds
and idle/process guards. No shape grid or broader parity claim. The byte incremental
comparison is inconclusive: raw −2.01%, normalized +6.91% point with overlapping
intervals and native −8.34%. Preserve that adverse point without claiming gain,
regression or equivalence, or repeating unchanged jobs for favorable numbers.

Reference remains explicitly zimg 3.0.6, FFmpeg zscale's backend, with graph/input/
scratch prepared before timing. Native halo, conversion, filtering, independent
output, quantization and release remain timed. Managed complete public Resize,
preparation/admission, materialization, independent output and disposal remain
timed. This is the declared repeated-reference boundary, not one-shot setup parity.
Keep the existing scoped vertical-word native bound separate from exact managed
pixels. Full 16-bit samples, rational/qualified-half behavior and exact repair stay.

The first coherent stage shares execution but retains the strip path's broader
word certificate. Its raw −5.77% follows native −8.25%, with a separated normalized
+2.69% loss; allocation savings alone establish no CPU gain. The completed stage
uses the unchanged small-support word-pair certificate from the previous default
cache path: its established ≤16-tap envelope covers enlargement's ≤5 taps per axis.
No constant, precision, rounding target or tolerance changes. This avoids the broad
certificate's extra vector division; counts are applicability evidence, not timing.
Nine geometry/filter/typed-publication/repair method bodies stay exact; unsigned
full-range word decoding and cooperative checkpoints remain explicit.

Additional word working admission falls 751,876→4,096 B; source+output+working
capacity bound falls 6,078,916→5,331,136 B. Already admitted output borrows 254,680 B
through 488 prefix rows, then 31 tail rows retire those views. Byte uses 457/62 rows.
Existing 80 B prefix/4,000 B tail stack frames do not overlap, and tiny-alpha replay
runs after scratch release. Retained operation workspace is zero; independent
results remain charged through disposal. These algebraic bounds are separate from
measured GC totals; whole-operation physical peak/native allocations are not newly
measured. The unchanged 4 MiB idle pixel pool is bypassed for governed outputs.

Each OS passes **5,741 full/live checks** (2,488 internal/2,305 public/948 differential),
plus 91 internal/61 public checks per normal/scalar/AVX512-disabled mode. Existing
opaque/tiny tall controls now also cover word default/bounded prefix retirement,
exact rational pixels, refusal/cancellation cleanup and independent survivors.
The SAME actual private Release package/all three examples passes Windows/Linux;
its full-frame consumer covers both depths and both limit boundaries. Published
packages remain immutable. Development Linux costs are not a 125-row actual-package
release refresh. Publication and the full 282-path objective remain separate/open.

Two immutable core/internal revisions per OS and one public-test build support the
coherent transition. One named-case confirmation selector reuses the frozen harness;
its initial missing Perfolizer reference is fixed without core/harness rebuilding.
A completed horizontal run is recovered after normalizing only a redacted library
search prefix; code/compiler/worker/archive identities remain equal. The earlier
wrong word-selector ID is rejected and retained. No completed timing job restarts.

[Evidence](bicubic-shared-enlargement-evidence.json) retains both stages, uncertainty,
all twelve current controls, sources, ownership bounds and actual-package proof.
Raw `bicubic-shared-enlargement-qualified.tar.xz`: 4,212,712 B / 395 members,
SHA256 `a65b2dda8201a4d856f3b6750094f9b2fa10907e85bb34aa9213c654d8199f41`,
mirrored/member-verified with pinned reused tooling. Earlier
[byte output reuse](BICUBIC-DEFAULT-OUTPUT-WORKSPACE.md) remains linked history.
Next prioritize larger codec attribution over further small enlargement tuning.
