# Warm VP8L encode: exact backend baseline

Complete public lossless WebP encoding independently confirms at
0.0606x **libwebp exact=1 (FFmpeg encoder backend)**,
conservative upper 0.0629x. Both idle guards pass.
Production is unchanged. This fixed exact-pixel case clears the timing target;
full required warm coverage remains open.

| Run | Lokad.Image microseconds | Backend microseconds | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | ---: | --- |
| Coverage screen | 7,312.00 | 120,594.12 | 0.06063x | 0.06960x | Pass |
| Independent quiet coverage confirmation | 7,363.30 | 121,587.95 | 0.06056x | 0.06294x | Pass |

The encoder-backend baseline is an explicitly approved exception to the public
FFmpeg API baseline. FFmpeg's libwebp wrapper exposes no `exact` option. On the
same original raster its output changes 837 RGB samples in 280 fully transparent
pixels; normal and scalar FFmpeg decoders agree on the changes. Alpha and RGB
under nonzero alpha remain unchanged. That unmatched output is retained as
correctness evidence and receives no parity timing. The public libwebp API with
`exact=1` preserves every RGBA sample, including hidden RGB. Both encoded outputs
also pass managed decode, RIFF/VP8L syntax checks and normal/scalar FFmpeg decode.

The single existing original 513x259 RGBA8 raster is reused, SHA-256
`70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80`. Lossless encoding uses fixed effort 4;
native quality 75 is a compression control, not lossy quality calibration.
Lokad.Image's literal/run coding produces **533,100 bytes**,
while the adaptive native encoder produces **183,812 bytes**.
These timings establish exact-pixel operation performance, **not compression
efficiency parity or an algorithm-for-algorithm speedup**. No raster, quality,
effort or strategy sweep was performed. Other material workloads remain open.

Managed timing includes public EncodeWebp, admission, source processing, a fresh
MemoryStream, growth and disposal, with no redundant ToArray. Native timing
includes full-frame RGBA-to-BGRA repacking, fresh pixel import/allocation, encode,
independent owned packet acquisition and release. Config/picture/writer wrappers
and BGRA scratch storage are reused; converted pixels and outputs are not cached.
Startup, I/O, IPC, preparation, hashing and validation are excluded. Both run on
CPU0 with one codec/conversion thread, normal SIMD; managed tiering is disabled
and workstation GC is nonconcurrent.

Managed allocation is **2,100,170 B/op**, raw GC bytes / operations rounded
up including harness overhead. Native allocations are unmeasured. The independent
repeat retains all 12 actual and 10 filtered measurements, plus 12 samples in
each native bracket. Upper uses the larger filtered/all-actual bound. The first
pre-run idle guard failed and launched no benchmark; its immutable archive is
retained. One fresh bounded attempt passed. Both scored runs pass
`exclusive-vm-idle-v2`; historical load averages are diagnostic only.

Both actual timing children match all 568 production method bodies and flags
in the accepted Release package. Previous 2,388 tests and 254 scalar JPEG controls
per OS and same-package/all-three-example checks on both OSes remain applicable.
New adapter protocol, source integrity, independent packet ownership, invalid
depth/affinity and normal/scalar exact decoder controls pass. One retained
six-second CPU profile is diagnostic only; no production optimization was applied
because this fixed pair already clears the target.

[Numeric evidence](warm-vp8lencode-exact-evidence.json) preserves all actuals,
brackets, guards, source/input/output/actual-child/package identities and immutable
archive digests, including the stock FFmpeg mismatch and failed pre-run guard.
Original public-API adapters contain no upstream implementation or tables.
See the [recipe](../tools/WarmReference/README.md) and
[current three-table summary](../docs/benchmark.md).
Regenerate with `python benchmarks/render-warm-vp8lencode-exact.py`; `--check`
verifies this report. Stop tuning this passing pair and cover the next material gap.
