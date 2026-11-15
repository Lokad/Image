# Current owned-flip confidence

The existing RGBA16 vertical flip **passes and stops**:
**17.425784 / FFmpeg 17.538755 µs**, conservative ratio interval
**0.987363–0.997935**, **56 B/op**. RGBA8 remains **uncertain**:
**8.494356 / 8.412304 µs**, interval **0.998230–1.019278**,
also **56 B/op**. This measurement-only round establishes no production speedup.

One prespecified integration increase from 4,096 to **16,384 complete operations
per subject** gives roughly 140/290 ms batches. Twelve alternating adjacent pairs,
twelve native batches before/after, every actual and the original conservative
paired/all-actual/fastest-native-bracket rule remain. Longer batches reduce the
relative effect of short disturbances. The original [uncertain samples](SNAPSHOT-BLOCK-COPY.md)
remain; no identical favorable retry follows. Uncertainty does not establish a
need for optimization.

Both complete operations flip the original 513×259 resident RGBA8/16 raster into
independent owned output, including materialization, allocation/admission and
disposal. FFmpeg retains its graph/source/frame wrappers, obtains a negative-stride
view and allocates/copies/releases a separate output inside every timed call.
Managed public validation, cancellation and ownership remain timed. Source setup,
IPC, JIT, hashes and validation stay outside both clocks. CPU0, natural
nonconcurrent workstation GC and idle-v2 process guards remain. Exact full samples,
alpha, hidden RGB, unchanged sources and independent live outputs pass.

Use the actual immutable qualified package DLL. Reuse **6,721 full/live checks per
OS**, focused normal/scalar and the SAME actual package/all three examples.
One small driver build changes only its harness/count guard; timing/statistical
bodies are unchanged. Zero production, embedded, public-test, harness, native,
fixture or package builds, and no broad qualification repeat or new workload grid.

Allocation, capacity and retention remain separate: **56 B/op**, source/result
payload **531,468/1,062,936 B** per byte/word frame, zero added admitted capacity
or retained operation workspace. Default two-slot/4 MiB idle storage and caller-root
bypass stay unchanged; physical/native/JIT peaks and native allocations are unmeasured.

Current 20 owned-edit/composition views have **18 passes, zero gaps and two
uncertainties** (byte flip and word PNG composition). Across the existing 192 image
views: **189 pass, zero gaps, three uncertain**. Separate in-place drawing and
inspection gaps remain; this does not establish all 289 required paths pass.

[Evidence](owned-flip-current-confidence-evidence.json) pins actuals, producers,
original reference inputs/settings, exact checks and qualification reuse.
Raw `owned-flip-current-confidence.tar.xz` (81,236 B/60 members), SHA256
`fc255ec6be1a7f54e340dd8438c61199a7df0d9884f9e79ec681c9d77c68e900`, is mirrored/member-verified with the original harness/worker
and current actual-package dependencies retained. Publication is postponed.
