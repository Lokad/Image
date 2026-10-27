# Prefix probing and pixel validation

Twelve named controls qualify the new read boundaries, without a settings grid.
The [current summary](../docs/benchmark.md) gives times, allocations and status;
[evidence](warm-image-read-evidence.json) retains every sample and identity.
No production code changed in this round.
The subsequent [dimension-workspace round](WARM-PROBE-WORKSPACE.md) supersedes
only the four probe costs/admission figures below; validation evidence is unchanged.

Dimension probes use fresh seekable sources for PNG/JPEG/WebP and a nonseekable
async JPEG source. They return independent requested dimensions, stop before
pixel payloads and pass controls with the unvisited tail removed. Actual acquired
bytes and traversed position are separate. The explicitly labeled reference uses
FFmpeg AVIO/descriptor APIs with an original bounded header adapter. These are
prefix operations, without complete-container, CRC or pixel validation.

Pixel validation checks complete coding and returns coded source metadata, with
no caller raster, orientation transform or RGBA normalization. The reference
decodes every packet into FFmpeg's natural frame and releases it, using private
codec scratch and an original source-metadata adapter. A valid PNG container with
invalid Deflate fails both validators. Async includes fresh nonseekable input,
actual acquisition/growth/EOF and cleanup; callbacks complete immediately on
resident bytes. Input preparation, IPC and real I/O are excluded from both sides.

Seven deterministic packets cover PNG8/16, indexed Adam7, baseline/progressive420
JPEG and VP8L/VP8 with alpha. Six use `OriginalExif.Create(profile,513,259)`;
indexed Adam7 uses the original `AuthoredPng` 19×17 all-filter fixture. Recipes,
input hashes, descriptor comparisons and acquired/traversed bytes are retained.

All twelve short screens pass both exclusive-VM idle/process guards. Baseline420
validation independently passes at 0.917× / upper0.935; stop this unchanged case.
Progressive420 independently confirms 1.053× / upper1.063. Other controls remain
screened gaps: probing 2.97–4.65×, PNG16 validation 1.75×. No full CPU-parity claim
follows. Three short or twelve confirmation actuals are retained, including
unfiltered confidence bounds and native brackets before/after each operation.

Allocation uses the default null shared budget and warm permitted private pools.
It includes managed temporaries, source/result objects and cleanup; it is not
native allocation or peak admitted storage. The separate
[governed storage comparison](PIXEL-VALIDATION-STORAGE.md) remains valid.

One frozen qualified 0.2.1 production build serves every screen/confirmation.
Its existing Windows/Linux 5,037 checks each, 66 targeted scalar checks and SAME
actual package/three examples are reused; production, full suites and package
proofs were not rebuilt/repeated. Encoding/editing paths are unchanged. The
mirrored, member-verified raw archive named in the evidence retains workers,
selectors, inputs, sources, manifests, logs, guards and all receipts.
