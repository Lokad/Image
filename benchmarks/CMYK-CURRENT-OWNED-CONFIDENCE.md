# Current owned CMYK JPEG confidence

Historical checkpoint. [Horizontal inverse preparation](JPEG-HORIZONTAL-PREPARATION.md)
supersedes these costs on the accepted current package. Raw evidence below is retained.

The accepted immutable build retains a demonstrated horizontal-AC gap against
FFmpeg's reused contexts. The separately timed equivalent fresh-context operation
passes. These boundaries describe different complete workloads; their costs are
neither additive nor evidence of a production change.

| Original 513×259 Adobe CMYK decode | Lokad / FFmpeg µs | Conservative ratio interval | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Retained FFmpeg contexts | 434.53 / 392.72 | 1.090–1.119 | 20,944 | gap |
| Fresh contexts on both sides | 399.32 / 748.79 | 0.471–0.562 | 20,944 | pass |
| Sparse-2D, retained contexts | 449.95 / 461.96 | 0.948–1.001 | 20,944 | uncertain |

The two horizontal views use the frozen 17,346-byte packet and exact 531,468-byte
RGBA result. Sparse-2D retains its separate frozen packet and exact pixel oracle. Timing
includes public decode, device-color normalization, independent output acquisition
and disposal. The first view reuses native decoder/frame/packing contexts; the
second includes their creation and disposal. Source delivery, settings, ownership,
normal/scalar native bounds and exact managed pixels are unchanged. One CPU,
optimized JIT, nonconcurrent workstation GC and both idle-v2 guards pass.

Each confirmation retains all 12 actuals and 12 native samples before/after.
Filtered and all-actual confidence views remain explicit. These first contemporary
measurements replace older affected displayed costs; no gain, causal regression or
favorable repeat is inferred. The first contemporary sparse-2D confirmation,
made while checking
the compact-output transition, replaces its older pass with uncertainty. It does
not establish a production regression or an optimization gap. The other JPEG costs
remain source-valid; no favorable retry is scheduled.

Allocation remains separate from live/admitted capacity and retention. No storage
or pool policy changes: two slots / 4 MiB, governed calls bypass idle storage,
and outputs retain their owners. Physical/native/JIT-stack peaks are unmeasured.

The production DLL and actual package are byte-identical to
[shared axis-swapping](SHARED-AXIS-SWAP.md): reuse 6,600 full/live checks per OS,
scalar/ISA and the same package consumer/all three examples. Stronger tail tests
also pass on the restored build in normal, scalar, AVX512-off and AVX2-off modes
on both platforms. The [word-product experiment](CMYK-WORD-PRODUCT-EXPERIMENT.md)
is rejected and supplies no current production result.

All 289 material paths remain required; current image views are 171 pass,
five gaps and 15 uncertain. Publication remains postponed.

[Evidence](cmyk-current-owned-confidence-evidence.json) records samples,
boundaries, reference identities and reuse. Raw `cmyk-word-product-decision.tar.xz`,
SHA256 `f80eb561e098d68146c42934c4aca534e14134ad789501dd9cfe5d7bc36fea9d` (4,998,464 B / 75 members), is mirrored
and member-verified; dependencies preserve the original capture tools and package.

The [compact-output study](JPEG-COMPACT-OUTPUT-STUDY.md) preserves useful horizontal
increments, rejected sparse-2D regressions and the new original measurement. Its
mirrored/member-verified raw supplement is `jpeg-compact-output-study.tar.xz`,
SHA256 `7b42ec38fe65e7380dffe4e15929a6c446632d440ef9ae79337cdc3ad35f22fa`. Candidate costs do not enter the current summary.
