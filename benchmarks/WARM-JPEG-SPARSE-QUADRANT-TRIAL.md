# Sparse JPEG quadrant trial

Exact quadrant symmetry reduces frequency products from64 to16 but yields only
**0.6% lower complete-operation point mean**. It is parked; accepted
source, core, internal tests and symbols are restored exactly on both OSes.

| Existing Sparse2D decode | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Parked quadrant variant | 638.93 | 436.65 | 1.463 / 1.541 | 1,085,689 |
| Accepted, retained | 642.99 | 440.11 | 1.461 / 1.519 | 1,085,689 |

All4,096 frozen Q16 basis values have exact signed x/y reflection symmetry.
Four frequency-parity sums and exact butterflies reconstruct the original64
integer samples from the top-left16. The same8KiB basis, full mass≤65,536 domain,
strict certificate, away-from-zero reference, clipping and whole-block-before-
write rule remain. No new table, scratch, cache, owner or shared-memory charge.
Work4,096 and cancellation remain unchanged; hardware without BW keeps the old path.

The independent proof checks2,048blocks,147,456quadrant prefixes and all64 outputs.
Twenty-five focused checks pass normally, with all intrinsics off and BW off per
OS. Actual accelerated Linux kernels match dense integer and original ordered
binary64 oracles. Exact analytical full frame, alpha/CMYK interpretation, native
normal/scalar bounds, independent outputs, work/cancellation and borrowed-root
gates pass. An earlier paired-frequency proposal is superseded before coding;
its feasibility evidence remains retained. Parity-mask dispatch replaces an
unmeasured switch variant before freezing; no scored source changes afterward.

One immutable short retains all three actuals, native3+3 brackets, both official
quiet guards and selected/all-actual confidence views. Existing fixture, harness,
selector and native worker are reused. No longer confirmation above one, extra
diagnostic, image/settings grid or full/package qualification for parked code.
Accepted4,484tests/OS and the same actual private0.2.0 package/examples remain
applicable. Published0.1.0, selected rows and all finite coverage counts are unchanged.

[Evidence](warm-jpeg-sparse-quadrant-trial-evidence.json) contains actual identities,
allocations, proof and two mirrored verified raw hashes. The subsequent
[direct-RGB diagnostic](WARM-JPEG-DIRECT-RGB-COST.md) finds entropy work material;
the frozen Sparse2D packet has no stuffed bytes. JPEG tuning is deferred while
lossless WebP compression size takes priority. Full parity and replacement
readiness remain open.
