# Reusing released image storage

Default cleared canvases and complete-overwrite images share the existing two-slot
store. A size miss preserves one array only when it leaves capacity and a slot for
the request; image disposal prefers a larger fitting released pair. Scratch/font
policies, fields, pixel algorithms and the 4 MiB ceiling stay unchanged. Caller-root
storage bypasses the cache. Overlapping public images retain independent mutable
storage; no new pool, quota, owner or aliasing wrapper is introduced.

The existing 513×259 RGBA16 PNG decode/orient/crop/nearest-fit operation independently
saves 317.42 µs / 9.74% and 1,063,078 allocated B/op. Raw, all-actual and native-normalized
confidence intervals separate. Its exact 127×64 output is unchanged; parity remains
uncertain. The same byte pipeline is a second-depth check, without a new raster or
settings grid. These complete pipelines include every public intermediate owner.

| Existing complete operation | Lokad µs | FFmpeg µs | Ratio / upper | GC B/op | Status |
|---|---:|---:|---:|---:|---|
| RGBA16 composition, independent confirmation | 2,942.04 | 2,951.97 | 0.997 / 1.007 | 338,268 | uncertain |
| RGBA8 composition, existing short job | 1,250.64 | 1,480.76 | 0.845 / 0.850 | 169,574 | pass |

All 191 affected existing comparisons have current costs: 161 pass, 14 gaps, 16 uncertain.
The full 289 material-path objective remains open. Scoped packet, managed-pixel,
quality, reference, cancellation and ownership gates are unchanged. Encoders,
caller-sample JPEG, metadata inspection and in-place drawing reuse unchanged paths.
Native retained codec/graph contexts and equivalent fresh alternatives remain labeled.

The bounded byte 128×64 control costs 1.588 µs / 0.446% more than a contemporary unchanged
baseline in separated raw/all-actual intervals; the normalized effect is inconclusive.
Both builds now miss parity, so the earlier pass is historical. Keep this adversity
and the candidate gap; do not erase it with resampling. Retained-context CMYK also
misses parity on both contemporary builds; its equivalent fresh-context view passes.

Source-derived payload for four overlapping word images remains 2,455,088 B. Idle
payload rises 1,327,128 → 2,125,872 B within the same two slots / 4,194,304 B. GC allocation,
active admitted capacity and idle retention are separate; physical/native/JIT peaks
are unmeasured. Governed images retain unchanged reserve-before-allocation, terminal
refusal, independent-result and disposal/cancellation/asynchronous lifetime contracts.

Both complete font/text integrations pass. Preparation and drawing stage times
remain diagnostics, not an additive model or proof of overall slowdown. The frozen
face, full 16-bit samples and exact layout/fitting/visual gates remain. Parser
validation and retained raster state still differ; this is the labeled complete
ImageSharp public integration, not FFmpeg or a bare shaping-engine comparison.

Each OS passes 6,538 full/live checks and 473 internal / 595 public focused checks in normal
and scalar modes. The three named resize-storage suites are included; unchanged
ISA arithmetic evidence is reused. The SAME actual Release package and all three
usage examples pass Windows/Linux, without another production build or pack.
Published releases are unchanged; publication is postponed.

The benchmark-only horizontal-flip constructor now specifies its operation,
avoiding an overload collision. One new harness build serves only those two cases;
all other measured modules remain frozen. Metadata/preparation failures and the
post-I/O-guard rejected capture remain in the raw archive, separate from passing
captures. No successful capture was repeated merely for favorable values.

[Evidence](released-image-storage-evidence.json) records every current cost,
boundary, source/reference identity, adverse result and qualification. Its raw
archive is mirrored and all 2,986 members verified. Earlier current costs remain in
[direct-input](DIRECT-STREAM-INPUT.md), [resize](RESIZE-HORIZONTAL-CONFIDENCE.md) and
linked family histories. Next: prioritize larger demonstrated absolute gaps;
preserve uncertain cases and stop confirmed passing font/text complete workloads.
