# Full-resolution CMYK row conversion

Complete Adobe CMYK JPEG decode is **63.1% faster**,2,754.84→1,017.42µs.
It remains a demonstrated **2.687x** warm single-CPU gap; parity is open.
The original10,910-byte513×259 authored DC projection, native worker and all
correctness/quality/ownership gates are unchanged.

| Complete public operation | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op | RGBA bytes, both |
| --- | ---: | ---: | ---: | ---: |
| Resident Adobe CMYK JPEG decode | 1,017.42 / 378.58 | 2.687 / 2.809 | 1,635,137 | 531,468 |

The single primitive reads full-resolution CMYK rows directly, eliminating four
general coordinate/interpolation calls per pixel. Sixteen-pixel safe AVX2 batches
perform exact byte products and nearest division by255, then pack opaque RGBA.
For0≤n≤65025, nearest(n/255)=((n+128)×257)>>16; the odd divisor excludes half
ties. Canonical untagged ink is inverted first, while Adobe-inverted samples are
used directly. Scalar tails and hardware-disabled execution keep the same exact
integer policy. No new heap workspace, frame/input copy, retained field or table.
Existing allocation/admission and per-row work/cancellation charges stay unchanged.
YCCK, subsampled CMYK and direct caller CMYK/RGB sample writers keep their bodies.

Every65,536byte-product pair is checked against independent decimal rounding in
both inversion modes, with independent channels, source/padding guards and scalar
tails. Public baseline/progressive Adobe/untagged frames have analytical pixels
and independently surviving output owners. All4,182tests/OS (1,419internal,
1,835public,928live FFmpeg),49affected hardware-disabled checks/OS, and the SAME
actual private Windows package consumer/all three examples on both OS pass.
Package SHA`74d27233edd5815bebd16a41456a7b31d4eb07e8f35f9eb5a7307755a1515e7e`.
Published0.1.0 artifacts/tag are preserved.

The complete public operation still includes four-plane decode/materialization,
device conversion, fresh owned RGBA allocation and disposal. Native software
MJPEG still includes K conversion, GBRAP packing and decoder/output release.
Exact analytical managed pixels, normal native CLI equality, existing normal/
scalar maximum4/RMSE1.5, exact alpha, source stability and independent owners pass.
Normal reference maximum1/RMSE0.71044 is unchanged. All three actuals, native
pre/post50ms brackets and both idle-v2 guards are retained. No independent long
repeat follows an above-one mean. The measured allocation change is+63B/op;
there is no claimed allocation improvement.

One frozen production candidate per OS is reused. No harness/native/selector
build, new fixture/settings grid or quality retuning. Exactly this numeric row
is replaced; all70passing paths remain stopped and retain applicable evidence.
Current219numeric/24unmeasured scope is unchanged; full warm parity remains open.

One subsequent six-second, single-CPU diagnostic has4,148samples, exact source/
pixel/owner controls and both idle-v2 guards. It is **profiling only**, with no
latency claim. IDCT's separable path receives56.0% of samples, materialization7.6%,
baseline block reading6.4%, and the new color writer3.5%. Next investigate a
constant-block shortcut scoped to CMYK materialization, preserving the frozen
binary64 rounding and original work charges. Separately, the three-slot coefficient
pool cannot retain a fourth549,120-byte component buffer; its existing4MiB byte
cap is sufficient for four. Keep this allocation opportunity separate and gated
to four-component decoding; do not broaden or repeat the profile.

[Numeric evidence](warm-jpeg-cmyk-row-evidence.json) retains timings, allocations,
receipts, exact identities, source and both-OS qualification. Raw archives on
both hosts:

- `h10i-measurements.tar.gz` SHA`4519baa0d800f0a21ace20f18e5153b3af14e7754676419d652978238a662c51`.

- `h10i-qualified-windows.tar.gz` SHA`0ce356375ddfb4e1f0d59fd2ae640725f023892554e14f6fdb37429e6568c7b5`.

- `h10i-qualified-linux.tar.gz` SHA`7e770f9931ae4b39482ab169de2d6b8c6ff91eba34cc0c989a13304150d469a1`.

- `h10i-profile.tar.gz` SHA`564ca5d8b48c94d0c0d17aa1d88e88141a52c20b5fe2721f31fc54ffc59acabe`.
