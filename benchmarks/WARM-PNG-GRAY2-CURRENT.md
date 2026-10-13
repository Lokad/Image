# Packed opaque gray2 with existing SIMD expansion

One new material case projects the existing original 513x259 RGBA8 raster:
red's top two bits select v=0/85/170/255, RGB=v, alpha=255. Original authoring
produces noninterlaced gray2 PNG, filter None and zlib level6. Frozen gray1/4
encoded/raster bytes remain exact. No original performance image is added; the
total stays two original rasters. Production and package are unchanged.

Complete resident public Decode/owned-output disposal is timed. FFmpeg includes
GRAY8 unpacking, one-thread conversion and independent output acquisition/release;
contexts and frame wrappers are reused. Full pixels/alpha, native CLI normal/scalar
pixels, source and held owners are exact. Startup/I/O/IPC/preparation/validation
stay outside timing. The development worker's gray whitelist was extended to
depth2; its timed native work is unchanged and never becomes a production input.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 86.87 | 110.68 | 0.785 / 0.974 | 9,136 |
| Independent twelve-actual confirmation | 83.19 | 110.11 | 0.756 / 0.783 | 9,136 |

This fixed pair is **confirmed: 0.756x / upper 0.783x**, with **9,136 B/op**.
Both guards pass; all twelve actuals and twelve filtered results remain retained.
Upper takes the larger filtered/all-actual bound. Allocation includes harness
overhead; native totals are unmeasured. Stop timing this pair. First-use, held-
output, tight-budget and transparency paths remain distinct.

The existing packed-gray AVX2 kernel already supports two-bit samples; no
production primitive was changed. Forced fresh and both timed children match all
595 methods and flags of the same actual [palette1 Release package](WARM-PNG-PALETTE1-ROW.md).
Its 2,682 tests per OS, focused normal/scalar controls, original authored profiles
and all three package usage examples remain validation. No full suite or pack was
repeated for this harness-only round.

The [numeric evidence](warm-pnggray2-current-evidence.json) retains every actual/
bracket, source/input/raster/child/package hash, frozen prior native worker and
immutable raw archives. Earlier failures/rejections stay unchanged. JPEG420's
upper-bound gap and the complete required material scope remain open. This fixed
confirmation is not uniform warm parity; PNG Up RGBA8 is the next distinct screen.
