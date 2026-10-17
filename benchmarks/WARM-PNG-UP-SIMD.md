# PNG Up SIMD

Updated 2026-10-17. Complete RGBA8/16 Up decoding is **32.8%/45.2% faster**
than the retained predecessors. Both existing cases independently meet warm
single-CPU parity, including conservative selected/all-actual uncertainty. Stop
both cases; the full fixed 207-case objective remains open.

| Existing Up decode | Managed µs | FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8, 513×259 | 1,484.65 | 1,509.16 | 0.984 / 0.989 | 4,456 | Parity confirmed |
| RGBA16, 513×259 | 1,902.38 | 1,911.94 | 0.995 / 0.997 | 8,552 | Parity confirmed |

The two original byte/word rasters, frozen Up/Optimal PNGs and complete public
Decode/admission/framing/inflation/reconstruction/endian/output/disposal boundary
are unchanged. The approved FFmpeg backend repeats full identical-input decode;
word conversion and its second independently owned output remain timed. Exact
full samples, alpha/hidden RGB, source preservation, normal/scalar native pixels
and two live independent owners pass. Native allocations are unmeasured.

One current six-second profile places 42.7% of 5,987 userspace samples in Up
reconstruction; this is attribution, not wall-time timing. PNG Up sums independent
bytes modulo 256 at either precision. The new helper uses safe 512/256/128-bit
vectors and scalar tails. Widths divide 65,536, retaining the original cancellation
positions. Short/overlapping previous rows and unsupported hardware retain the
old scalar loop. No workspace, field, allocation, work/admission or quality change.

Removing the filter2 dispatch and helper restores the entire predecessor PngCodec
file byte for byte. The existing scalar loop and every other method/field remain.
Only DecodeUp8/DecodeUp16 replace timings; **205 comparisons and all 52 prior passing
proofs are reused**. No raster, workload or settings grid was added.

One immutable production/harness/worker/fixture build serves both short screens
and both independent confirmations. All three/twelve actuals, native pre/post
brackets and both exclusive-vm-idle-v2 guards pass and remain retained. No harness,
selector or native rebuild was needed. Historical/current native source/binary
identities are reconciled with unchanged complete RGBA decode/conversion/ownership;
unrelated colorkey/encoding support and CPU telemetry do not alter this boundary.

All **3,642 Release cases per OS** pass (1,340 internal, 1,404 public, 898 live
FFmpeg), plus five scalar Up and 127 scalar public PNG controls per OS. Three new
cases cover unaligned vector tails/byte wrap/64 KiB boundaries, overlapping rows
and empty/pre-canceled behavior. The same actual Release package passes its
consumer and all three examples on Windows/Linux; symbols match its qualified
DLL. Selected Linux production bytes are identical through qualification.
Unchanged scalar proofs are reused. Released 0.1.0 artifacts/tag remain intact.

[Numeric evidence and archive hashes](warm-png-up-simd-evidence.json) retain all
statistics, receipts, pixel/packet identities, profile and qualification. Main raw
archive `parity-p21-qualified-evidence.tar.gz`, SHA256
`b5054c0766cc80b4090bfe617243eaafb65fc9f7bc3ee6fd3a13bb484864d196`.
