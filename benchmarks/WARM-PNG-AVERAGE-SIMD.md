# SIMD PNG Average reconstruction

Complete Average decoding independently improves **20.3% for RGBA8 and 22.5% for
RGBA16**, with unchanged allocations. Both pass the warm one-CPU parity target
and stop. Raw filtered/unfiltered 99.9% CIs and conservative normalized envelopes
separate from the valid prior independent confirmations.

| Existing complete operation | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Average RGBA8 | 1,682.520 | 0.911 / 0.916 | 4,472 | Pass; stop |
| Average RGBA16 | 4,092.868 | 0.915 / 0.918 | 8,568 | Pass; stop |
| Indexed Adam7 validation | 5.320 | 1.359 / 1.369 | 1,168 | Gap |

One helper reconstructs four/eight encoded bytes using widened unsigned lanes,
floor((left+up)/2) and addition modulo256. Byte prediction precedes word endian
conversion. No buffers, caches, owners, unsafe code or API are added. Scalar,
overlap, truncated-neighbor, tail/guard and cancellation semantics remain.

The primary fresh three-actual pair passes strict5% triage (18.3% raw elapsed).
Independent primary confirmation reuses the unchanged original12actual PNP1
result; it improves22.518% filtered/22.486% all. The secondary improves20.346%/
20.442%. No duplicate original confirmations, variants, new images or settings.
Both subjects retain complete decode/normalization/independent output/disposal,
frozen original513×259 packets, exact RGBA/alpha/hiddenRGB and normal SIMD CPU0.
Managed allocations are B/op; native allocations are unmeasured.

The guard follows existing None/Up/Sub/Paeth fast returns; those kernels and their
relevant timed paths stay unchanged. Reuse their scoped confirmations. Indexed
validation reaches scalar dispatch, so its cost is refreshed with the identical
owned36byte metadata descriptor and native natural-frame boundary. Its temporary
proof is restored byte-for-byte. This refresh is not a causal gain claim. The
nonindexed validators certify filtered rows without reconstruction and keep PCF1
evidence, including the byte confidence-open result.

All5,088tests/OS pass (2,011internal,2,129public,948live), plus11focused kernel
and225public checks normal/scalar perOS. Six new independent tests cover every
predictor pair, floor/overflow, tails/guards/first rows, overlap, truncation and
cancellation. The SAME actual Release package and three examples pass on both
OSes. One core build/OS,zero harness/native/selector builds; immutable builds are
reused throughout. Published releases remain unchanged.45named PNG costs still
need current evidence; the full268path objective remains.

[Evidence](warm-png-average-simd-evidence.json) and
[prior scalar costs](CURRENT-PNG-RGBA-DECODE-COSTS.md).
Mirrored/member-verified archive png-average-simd-qualified.tar.xz, 171members,
SHA256 48639ed4b33b57bb465fee8a8118d7cd06c65a42ceb85b702e9ac9e6b30ab78a. Reproduction dependencies are pinned by hash.
