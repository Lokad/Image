# RGBA8 bicubic strip reuse

Updated 2026-10-17. TightDown8 is **16.5% faster**, remaining an open warm
single-CPU parity gap. These are qualified three-actual screens, not independent
parity confirmations. The predecessor managed mean was 783.42 µs.

| Existing TightDown8 variant | Managed µs | Reference µs | Ratio / upper | Managed B/op | Disposition |
| --- | ---: | ---: | ---: | ---: | --- |
| Paired horizontal accumulators | 711.57 | 419.17 | 1.698 / 2.009 | 132,294 | Superseded |
| Paired accumulators and wider strips | 654.01 | 416.59 | 1.570 / 1.660 | 132,293 | Accepted; parity open |

The original 513×259 RGBA8 raster produces an independently owned 256×129 bicubic
result with input+output+4,096 working bytes. Complete public Resize, admission,
output acquisition and disposal are timed. The unchanged approved zimg/FFmpeg
zscale backend includes a four-pixel clamped halo, deinterleaving, B=0/C=0.5 premultiplied
filtering and independent planar/packed output acquisition and release. Its
default byte worker retains its original complete boundary. Frozen managed/rational pixels, native RGB/alpha max1 and
both RMSE≤0.25, normal/scalar hashes, source and owner checks pass.

One six-second current profile placed 90.8% of 5,677 samples in the column core.
Two bounded candidates pair adjacent horizontal accumulators in 512-bit lanes,
preserving each column's tap order and separate multiply/add. Unmatched taps
remain in their original half. The selected version also reuses eight output
columns through nine row slots below vertical scale 2.25; larger scales retain
six columns and ten slots. Checked byte offsets address at most 25 decoded pixels.
Fixed arrays occupy 4,080 bytes under the same 4 KiB admission. Paired quotient
guards, scalar quantization, tiny-alpha unwind, work, cancellation and independent
owners remain unchanged. No heap workspace, persistent state or output cache.

Only TightDown8 replaces its old timing. Default Down8 keeps the unchanged full
row cache; other depths/directions/supports reject before this modified body.
The other **206 comparisons, including all 52 passing parity proofs**, are reused.
No performance raster, case or settings grid was added. Every actual, native
pre/post bracket and both `exclusive-vm-idle-v2` guards remain, including the
superseded candidate's broad upper. No routine twelve-actual job was run while
means remain materially above parity. Native allocation is unmeasured; managed
B/op includes harness amortization.

All **3,633 Release tests per OS** pass (1,331 internal, 1,404 public, 898 live
FFmpeg), plus 149 scalar resize controls per OS. Four new cases cover strip tails,
cache wrap, byte offsets and the eight/six-column scale boundary. The same actual
Release package passes its consumer and all three examples on Windows/Linux;
symbols match its qualified DLL. Selected Linux production bytes remain identical
through the screen and qualification. The second candidate reuses the first
selector and unchanged harness/reference/fixtures. Released 0.1.0 artifacts/tag
remain unchanged.

[Numeric evidence and archive hashes](warm-bicubic-byte-strips-evidence.json)
retain both builds, all statistics, receipts, profile and qualification. Main raw
archive: `parity-p18-qualified-evidence.tar.gz`, SHA256
`5c4f8084d4ca89bc6f0f56efa28b682fc6d4a895c9cefc5752bf699dcfcfcad7`.
