# Byte-bilinear enlargement reuses horizontal rows

The existing 513×259 RGBA8 →1027×519 control independently confirms **21.66% less
filtered elapsed**, **21.62% across all actuals** and **21.74% normalized**.
Raw and normalized confidence envelopes are separately disjoint. It remains a CPU
gap against warm single-CPU upper ≤1×, with exact managed pixels and unchanged
native RGB/alpha comparison bounds.

| Complete operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Original | 3,740.50 | 2,158.22 | 1.733 / 1.739 | 2,235,501 |
| Retained | 2,930.45 | 2,160.61 | 1.356 / 1.363 | 2,235,512 |

Two unrounded horizontal rows use existing geometry padding: slots 4..7 of each
eight-double cell hold two RGBA Int32 sums; the first three weights remain
available for exact replay. The fixed geometry needs 259 horizontal row builds
instead of 1,038, a non-timed operation count. No storage, admission, owner, root or
persistent cache is added. Observed allocation differs by 11 B/op; admitted capacity
is unchanged. The existing width guard also bounds creation of the cache view.
Wide/portable/tight-memory fallbacks, exact integer products, normalization,
certificates, quantization, tiny alpha, scalar tails and cancellation remain.

The reference is the unchanged FFmpeg/zscale triangle graph, including packing,
encoded-space alpha normalization, writable zero-alpha RGB normalization and
independent output acquisition/release. Managed Resize includes geometry,
admission, fresh materialization and disposal. Inputs/settings are frozen; all
pixel/source/owner/normal-scalar CLI and idle-v2 gates pass. Native allocation is
unmeasured. No broader content, geometry or speedup claim is established.

Both OSes pass **5,241 Release checks each** (2,111 internal, 2,182 public, 948 live),
plus 185 focused public cases normal/scalar. New coverage proves exact existing
admission peaks, terminal refusal cleanup, recovery and surviving output ownership.
The same actual Windows Release package passes both OSes, including this byte
row/admission control and all three usage examples. The small governed control
peaks at 2,748 bytes and retains only 1,276 output bytes after its source is disposed.

The short pair gives 21.4%. Native-screen drift requires a fresh original twelve;
one independent candidate twelve passes. A bounds-only cache-view correction
then requires one extra core build/OS and one confirmation on the final DLL.
Initial scores/qualification remain; final full checks reuse unchanged public
and differential assemblies. Total two core builds/OS, zero harness/native/
selector builds and no profile or performance variant. Package verification
runs once per OS after final source. Successful unchanged-producer timings are
never rerun.

[Evidence](warm-bilinear-byte-row-reuse-evidence.json) pins producers,package,
every actual/native bracket and mirrored/member-verified
`bilinear-byte-row-reuse-qualified.tar.xz`, SHA256 7c17dfe3b8abe2363aa097c6294f3e169e004049a9bea66d0ac2ab3f06b6148b.
Other kernels retain applicable evidence. Close this primitive; all 268 required
material paths remain in scope and full performance parity is still open.
