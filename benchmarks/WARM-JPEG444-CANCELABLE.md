# JPEG444 encoding with a live cancelable token

Complete warm RGBA8 JPEG444 encoding independently confirms **1.819× FFmpeg /
upper 1.879×**, at **6,899.40 µs** and **526,019 B/op**. The short mean is 1.807×
with a wide 2.479× upper. All twelve confirmation actuals and both quiet guards
are retained on the same frozen core/harness. Stop this fixed pair.

The existing original 513×259 RGBA8 raster uses managed quality69/native qscale5
and background RGB 0x204060. A live cancelable token, never signaled during timing,
selects the fixed geometric Huffman fallback on a stock MemoryStream. Token/source
creation and disposal are excluded. Fresh output acquisition, growth, alpha
flattening, color conversion, transform, serialization and disposal are timed.
The unchanged native worker includes flattening, conversion, complete encoding
and owned packet acquisition/release, reusing wrappers/contexts. Native has no
equivalent cancellation setting. No output is cached.

Every managed byte equals the frozen [forward-only packet](WARM-JPEG444-FORWARD.md),
which already has an exact geometry/quantization/coefficient proof against the
qualified adaptive anchor. That proof is reused without repeating its visitor.
Registered decoder pixels and source errors remain unchanged. The source-PSNR
gap is **0.082440 dB**, within **0.25 dB**; decoder RGB maximum3 / RMSE0.227803
stays inside maximum4 / RMSE1.5. Opaque output alpha, source/owner independence
and already-canceled rejection without output pass before timing. Managed/native
packets remain 170,978 / 133,041 bytes. Compression efficiency parity is not claimed.

Only one benchmark input changes. Qualified production, native worker, raster,
packet and one matched quality pair are reused; there is no quality sweep.
Existing **3,334 Release tests per OS**, including **896 live FFmpeg comparisons**,
and consumer/three examples through the same actual package remain applicable.
No production/native rebuild, full suite, package proof or method audit repeats.
Sequential Release measurements use one pinned CPU, one codec/conversion thread,
normal SIMD, tiering off and workstation nonconcurrent GC. Startup, I/O, IPC,
preparation and validation are excluded. Native allocations are unmeasured.
[Numeric evidence](warm-jpeg444cancel-evidence.json) links immutable receipts,
actual jobs/four identities, packet/coefficient proof and existing qualification.
Remaining material public/current-source warm coverage stays open.
