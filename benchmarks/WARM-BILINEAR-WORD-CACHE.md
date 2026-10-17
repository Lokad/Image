# RGBA16 bilinear row reuse

Updated 2026-10-17. The existing Down16 workload is **31.8% faster**, remaining
an open warm single-CPU parity gap. This is a qualified three-actual screen,
not an independent parity confirmation.

| Existing selector | Prior / current managed µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Down16 | 783.89 / 534.35 | 418.51 | 1.277 / 1.336 | 290,127 |

The original 513×259 RGBA16 raster produces an independently owned 256×129
bilinear result. Complete public Resize, admission, prepared geometry, output
acquisition and disposal are timed. The approved FFmpeg public graph includes
packed/planar conversions, widened triangle filtering, encoded-space alpha
premultiplication/unpremultiplication, final RGBA64 and output ownership. Managed
samples remain exact against the rational reference; native RGB/alpha max1 and
both RMSE≤0.25, frozen normal/scalar hashes and zero-alpha RGB policy remain.

One six-second current profile placed 93.6% of 5,437 samples in paired-row
filtering. Its historical predecessor predates accepted register/store changes
and could not attribute current code. The single new primitive filters source
rows once per six-column strip, reusing unrounded horizontal results vertically.
Exact unsigned products and the established conservative word error certificates
guard separable binary64 output. Uncertain ordinary-alpha pixels replay original
tap order; tiny alpha declines and unwinds before the existing exact-alpha path.
The optional 4 KiB reservation covers 3,640 bytes of fixed arrays after existing
prepared geometry. No new heap workspace, persistent state or output cache.

The specialization requires word bilinear reduction on both axes, scales≤2.5
and supported SIMD. It preserves complete original work admission, cancellation,
bounded reads and independent owners. Byte generic dispatch eliminates the new
condition; word enlargement returns through its unchanged helper first. Other
filters, directions, hardware and budgets retain their prior paths. Only Down16
replaces its old timing; **206 other comparisons, including all 52 passing parity
proofs**, are reused. No new raster, performance case or settings grid.

Every actual, native pre/post bracket and both `exclusive-vm-idle-v2` guards are
retained. No routine twelve-actual job was run while the mean remains above
parity. Managed B/op includes harness amortization; native allocation is unmeasured.

All **3,629 Release tests per OS** pass (1,331 internal, 1,400 public, 898 live
FFmpeg), plus 136 scalar resize controls per OS. Six new cases cover strip/cache
boundaries, half rounding, tiny alpha, owners, work and fallback budgets. The same
actual Release package passes its consumer and all three examples on Windows/
Linux; symbols match its qualified DLL. Linux production bytes remain identical
through the screen and qualification. Released 0.1.0 packages and tag are unchanged.

[Numeric evidence and archive hashes](warm-bilinear-word-cache-evidence.json)
retain all statistics, receipts, profile and qualification. Main raw archive:
`parity-p17-qualified-evidence.tar.gz`, SHA256
`efafb17c92fbd42efa3cd70d1e8f32cc29e0869331ac98e3c912fe697639b18f`.
