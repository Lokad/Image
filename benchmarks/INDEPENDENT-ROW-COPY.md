# Shared independent row copying

The later [byte vertical confidence](VERTICAL-FLIP-CURRENT-CONFIDENCE.md)
supersedes this report's standalone byte-flip cost; its drawing gains remain valid.

Complete RGBA8 rectangle copying saves **0.240 µs / 10.73% raw**, **10.75%
reference-normalized**. The overlapping forward/reverse pair saves **0.342 µs /
8.17% raw**, **9.14% normalized**. Both managed confidence views and normalized
bounds separate from contemporary unchanged baselines. These remain small parity
gaps, not passing controls. No encoder packets, managed pixels or public APIs change.

| Existing original 513×259 control | Lokad / FFmpeg µs | Upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| Separate RGBA8 rectangle | 1.993 / 1.906 | 1.065 / gap | 56 |
| Separate RGBA16 rectangle | 3.832 / 3.789 | 1.026 / uncertain | 56 |
| Overlapping RGBA8 pair, two moves | 3.847 / 3.807 | 1.017 / gap | 112 |
| Owned RGBA8 vertical flip | 8.752 / 8.299 | 1.083 / gap | 112 |
| Owned RGBA16 vertical flip | 17.551 / 17.963 | 0.991 / pass | 112 |

One safe `Vector512` primitive now serves vertical flips and disjoint RGBA8
rectangle-row chunks. Distinct owners or different tightly packed rows establish
disjointness; original direction/chunk traversal preserves rectangle snapshot
semantics. Same-row self-copies and all word rectangle copies retain BCL memmove.
Small/unsupported cases use its portable fallback. Original 4,096-pixel drawing
and 1,024-pixel flip cancellation checkpoints, clipping, admission and owners remain.

Candidate A's word rectangle took 4.138 µs versus contemporary 3.927 µs. Its raw
results and modules are preserved, but that drawing dispatch is not retained.
Candidate B is byte-specific, not a fixture-width rule. Word now takes 3.832 µs and
remains uncertain; no word optimization claim. Shared-helper byte flip is a current
gap, superseding older uncertainty. Historical confidence overlaps; no demonstrated
regression or favorable timing replay. Word flip passes and stops.

Complete rectangle calls include validation, admission, cancellation and cleanup,
mutating borrowed canvases. Source/canvas preparation, resets and allocation are
excluded equally. FFmpeg uses public `av_image_copy_plane`. The overlapping pair
times **two public moves including reverse restoration**; native uses six calls
over disjoint directional bands, with no temporary raster. Vertical flips include
independent owned output acquisition/materialization/disposal; native retains its
graph and wrappers. These are distinct boundaries, not a general copy-engine claim.

All 12 actuals, filtered diagnostics, adverse samples and three native brackets
remain. Quiet CPU0 idle-v2 guards and original hashes/full-channel gates pass.
Native clock changes +0.026%/+1.075% for separate byte/pair; normalized gains are
reported separately. No new benchmark images, settings, worker or harness builds.

GC allocation is 56 B per separate copy, 112 B per pair/flip. **No new owned capacity,
temporary pixel storage or retained operation state.** Separate borrowed source and
canvas total 1,062,936/2,125,872 B for byte/word; the overlapping byte canvas is 531,468 B.
The two-slot/4 MiB default idle store is unchanged. Allocation, borrowed/live capacity
and retention remain distinct; physical/native/JIT peaks are unmeasured.

Windows/Linux each pass **6,624 full/live checks** (3,248 internal/2,428 public/948 live).
Focused 25 internal/76 public pass normal/scalar/AVX512-off/AVX2-off on each OS. Four
new tests cover wide horizontal word aliases and clipped partial-vector tails.
The **same actual Release package**, containing the timed Linux DLL, passes bothOS
consumers/all three examples. Added byte/word overlapping-copy consumers verify exact
snapshot pixels, precancellation/no mutation, repeated recovery and zero extra
admission with the shared root fully reserved. Existing codec/edit checks pass.

Two core/embedded builds and one public build per OS cover A/B. Performance
reconciliation precedes one broad/package milestone; no accepted artifact rebuild
after timing. Unchanged paths reuse valid evidence. Current 191 image views have
173 passes/one gap/17 uncertainties; 20 independent-result edits have 14 passes/one gap/
five uncertainties. In-place drawings/probes remain separately scoped. The full
289-path objective remains open. Publication is postponed; published releases stay
immutable.

[Evidence](independent-row-copy-evidence.json) records identities, current costs,
adversity and qualification. Raw `independent-row-copy-qualified.tar.xz`: 3,534,964 B /
282 members, SHA256 `04a8dc53d05ffbf7280f05e501de5ac75839441e6e25526fa10b0d69e2968ba4`, mirrored/member-verified.
Dependencies preserve original fixtures, reference workers, packages and recipes.
