# Bicubic RGBA8 paired columns

Updated 2026-10-17. A qualified intermediate improvement toward warm single-CPU
parity. **All three cases remain open against independent upper ≤1.000.** These
are three-actual quiet screens, not independent parity confirmations.

The existing original 513×259 byte raster is unchanged. Mixed cases produce
1027×129; the vertical case produces 513×129. Tight limits allow 4,096 scratch
bytes beyond input/output buffers. Complete public Resize, admission, independent
output and disposal are compared with the approved zimg/FFmpeg zscale backend,
including timed halo, packing, premultiplied FLOAT filtering, normalization,
quantization and output ownership.

| Existing selector | Prior / current managed µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| TightMixed8 | 1,617.87 / 1,192.42 | 868.12 | 1.374 / 1.571 | 530,385 |
| Mixed8 | 1,600.47 / 1,135.34 | 868.67 | 1.307 / 1.473 | 530,385 |
| TightVertical8 | 1,063.64 / 725.00 | 589.64 | 1.230 / 1.378 | 265,003 |

Managed time falls **26.3–31.8%**. Every actual, native pre/post bracket and both
`exclusive-vm-idle-v2` guards are retained, including the short jobs' uncertainty.
No routine twelve-actual confirmation was run while means remain materially
above parity. Native allocation is unmeasured. Managed B/op includes harness
amortization; the production change adds no heap arrays or output cache.

One six-second profile placed 89.3% of samples in the selected core. The single
candidate pairs adjacent source columns in AVX-512 vertical sums. Six cache slots
cover five active taps and one prefetched column; eight-row bands reduce repeated
geometry work. Scales above 2.125 retain four-row bands. Validated row offsets fit
bytes, keeping arrays within 3,972 bytes and the unchanged 4 KiB reservation.
Exact byte products, each vertical tap's multiply/add order, horizontal filtering,
division, quantization guards and tiny-alpha unwind remain intact.

Only `MixedColumnBicubicResize` changes. These three timings replace their older
independent results. Default Vertical8 keeps the unchanged preferred row cache;
word paths and other geometry reject at unchanged guards before the modified
body. The other **204 comparisons, including all 52 passing parity proofs**, are
reused. No raster, workload or settings grid was added.

The cached vertical reference initially refused an older shared-source identity
before timing. Only that worker was rebuilt against the same compiler, flags and
zimg archive. The source delta adds an unused single-pixel selector, aliases the
unchanged vertical halo and adds untimed X/Y halo receipt fields. Frozen normal/
scalar hashes, complete operation boundaries and native max1/RMSE0.25 gates stay
unchanged. TightVertical8 retains its qualified original binary64 pixels and 25
independently certified rational alpha halves; no managed tolerance is introduced.

All **3,623 Release tests per OS** pass (1,331 internal, 1,394 public, 898 live
FFmpeg), plus 43 scalar mixed-axis resize controls per OS. Four new cases cover
cache wrap, five-tap integer centers, odd/one-column input, row-band tails and
scale/admission boundaries. The same actual Release package passes its consumer
and all three examples on Windows/Linux; its symbols match the qualified DLL.
The screened Linux production bytes remain identical through qualification.
Released 0.1.0 packages and tag remain untouched.

[Numeric evidence and archive hashes](warm-bicubic-byte-paired-evidence.json)
retain statistics, receipts, the setup refusal, source reconciliation and
qualification. Main raw archive: `parity-p16-qualified-evidence.tar.gz`, SHA256
`5d364413793022345b3b62c777a795ab291ee3f12eeeb16cdf96c9c0cca8bea3`.
