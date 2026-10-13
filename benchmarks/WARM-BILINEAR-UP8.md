# Warm RGBA8 bilinear upsampling

The new 513×259 to **1027×519** case exposes a **3.041× FFmpeg** gap:
Lokad.Image averages **6,529.25 µs**, conservative upper **3.403×**, with
**2,235,481 B/op**. Both quietness guards pass. This is a three-actual screen,
not a confirmed parity result; no twelve-actual repetition is warranted yet.

The same original transparent RGBA8 raster is reused. Managed pixels match the
exact rational reference on both OSes with intrinsics enabled and disabled.
Source bytes, geometry and independent held outputs are unchanged. The
[original strict refusal](WARM-BILINEAR-UP8-UNMATCHED.md) remains retained:
FFmpeg differs by one alpha sample at four pixels and retains RGB at one pixel
whose output alpha rounds to zero.

This case's explicit native compatibility option allows alpha maximum error
1/RMSE ≤0.25 and clears RGB at zero alpha. Writable output acquisition and the
clearing pass are inside every timed native operation. CLI/native agreement is
exact after that same normalization. Actual alpha RMSE is 0.002739; RGB maximum
1/RMSE 0.002959. Down8's alpha gate remains exact. All packed/planar conversion,
premultiplied filtering and independent output acquisition/release are timed on
one CPU/filter thread with normal SIMD. Neither subject caches pixels.

One six-second profile records 5,485 samples, 98.6% in the paired-row loop;
both guards pass. Two independently measured trials preserve exact pixels but
are slower: vectorized RGBA8 packing averages **8,078.06 µs / 3.752×**, while
the existing single-row kernel averages **8,890.76 µs / 4.398×**. Both are
rejected, retaining all actuals, native brackets and unchanged allocations.
The vector-packing trial also passes focused normal/scalar rounding checks on
both OSes, covering every byte half and its neighboring doubles.

Original production source and the verified immutable baseline DLL are restored
without another rebuild or package audit. The existing actual package's full
cross-platform/differential qualification is reused; production changes were
not accepted. The [numeric evidence](warm-bilinearup8-evidence.json) records the
strict refusal, compatible baseline, profile, rejected trials and immutable raw
builds. Next, investigate a dedicated two-tap upsampling kernel as one substantial
primitive. The 3.041× gap takes priority over small upper-bound margins; full
required warm coverage remains open.
