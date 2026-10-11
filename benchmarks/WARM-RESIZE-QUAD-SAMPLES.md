# Warm resize: grouped four-tap samples

Complete RGBA8 bilinear resize improves 20.51% in the
confirmation repeat on one original 513x259-to-256x129 pair. It reaches
2.08x FFmpeg; the uniform 2x target remains open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: |
| Paired packed-sample baseline | 1008.22 | 2.63980x | 2.68512x |
| Short screen | 809.90 | 2.11834x | 4.06966x |
| Confirmation repeat | 801.45 | 2.07876x | 2.11854x |

For exactly four consecutive horizontal sample offsets, the paired row kernel
uses one bounded 16-byte read per source row, replacing four separate four-byte
reads per row. Interleaving forms four independent RGBA sample pairs; four
unrolled taps retain the original accumulation order. Packed widening,
conversion and direct weighted-alpha broadcasts use the existing AVX512F/AVX2
path. Each pixel retains separate binary64 multiplication/addition, alpha
normalization, exact tiny-alpha handling, halves-away rounding and clamping.

Duplicate clamped offsets and other tap counts retain the general paired path.
Unequal vertical supports, odd tails, unsupported hardware and UInt16 retain
the existing filtering paths. Work charges, bounded cancellation, admission,
source and independent ownership remain. There is no new heap or stack
workspace. Three fixed batches of 16 complete Resize/Dispose calls retain
exactly the baseline allocation totals. No performance raster or pair was added.

The managed boundary includes public Resize, geometry preparation, admission,
allocation, all filtering and disposal. The public-FFmpeg-API graph includes
packed/planar conversion, widened triangle filtering, encoded-space alpha
handling and independent output acquisition/release. Native graph/source/
wrappers/geometry contexts are reused. Native samples before and after bracket
managed samples. Both subjects use one CPU/filter thread, thread type zero and
normal SIMD. Managed tiering is off with workstation/nonconcurrent GC. Startup,
IPC, I/O, generation, hashing and validation are excluded.

181 baseline controls per OS and 181 candidate controls per OS with intrinsics
enabled/disabled pass, covering frozen binary64 digests, rational references,
literal RGB halves, tiny alpha, tight memory, work/cancellation and ownership.
All 2128 Release tests per OS pass. The same actual dependency-free Release
package passes all three usage examples on both OSes. Both statistical child
DLLs and fresh production IL/flags match it. Only PairedResize.TryRows changes
among 542 existing production methods; one internal tap helper is added.
Other existing bodies and implementation flags remain equivalent with metadata
references resolved. Fourteen native controls pass. Managed rational-reference
pixels remain exact; native alpha is exact and RGB stays within maximum 1 /
RMSE 0.25. This output has no zero-alpha pixels; broader native alpha-edge
coverage remains open.

One actual-package JIT inspection accompanies allocation correctness controls
(2943 code bytes for the paired row kernel). It confirms grouped loads,
interleaving and inlined taps with packed widening/conversion/multiply/add,
without FMA or per-tap wide coefficient stack materialization. This is a code
shape check, with no diagnostic timing or instruction-share claim. Pre-run
quiet guards pass; post-run failures remain preserved. Independent quiet
confirmation and other material warm cases remain open. A later cooldown does
not certify statistical intervals.

The [numeric evidence](warm-resize-quad-samples-evidence.json) retains statistics,
boundaries, source and actual DLL identities. The immutable archive SHA-256 is
`4ce572b840d55505a19956891a7a1d7a58b2e754cdd6367619b4edca46b0642e`.
Regenerate with `python benchmarks/render-warm-resize-quad-samples.py`;
use `--check` to verify this report.
