# Bicubic RGBA16 paired columns

Updated 2026-10-17. A qualified intermediate improvement toward warm single-CPU
parity. **All three cases remain open against independent upper ≤1.000.** These
are three-actual quiet screens, not independent parity confirmations.

The existing original 513×259 word raster is unchanged. Mixed cases produce
1027×129; the vertical case produces 513×129. Tight limits allow 4,096 scratch
bytes in addition to input/output buffers. Complete public Resize, admission, independent output and disposal
are compared with the approved zimg/FFmpeg zscale backend, including timed halo,
packing, premultiplied FLOAT filtering, quantization and output ownership.

| Existing selector | Prior / current managed µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| TightMixed16 | 1,644.19 / 1,197.19 | 853.79 | 1.402 / 1.555 | 1,060,439 |
| Mixed16 | 1,627.26 / 1,237.22 | 846.01 | 1.462 / 1.883 | 1,060,429 |
| TightVertical16 | 1,059.29 / 770.52 | 589.55 | 1.307 / 1.361 | 529,847 |

Every actual and both `exclusive-vm-idle-v2` guards are retained, including the
broader native uncertainty on Mixed16. No repeat was sampled to improve a bound.
Native allocation is unmeasured. Managed B/op includes harness amortization;
the production change adds no heap arrays or reusable output cache.

The selected helper now decodes two adjacent columns in one vector load and
filters both with AVX-512. Six cache slots cover five active taps plus one
prefetched column. Eight-row bands reduce repeated geometry work; scales above
2.125 retain four-row bands. Checked row offsets fit bytes, keeping all arrays
within 3,972 bytes and the unchanged 4 KiB reservation. Exact unsigned products,
tap order, division, quantization guards and tiny-alpha fallback remain intact.

One six-second profile and five bounded short candidates were sufficient:

| TightMixed16 candidate | Managed µs | Disposition |
| --- | ---: | --- |
| Four rows, adjacent columns, six slots | 1,490.17 | Superseded |
| Eight rows, five slots, restricted prefetch | 1,429.40 | Superseded |
| Eight rows, byte offsets, six slots | 1,197.19 | Accepted |
| Refined reciprocal and vector packing | 1,284.92 | Rejected regression |
| Exact divide with vector divisor/margin assembly | 1,212.86 | Rejected no improvement |

Only `MixedWordColumnBicubicResize` changes. The three affected named selectors
replace their older timings. Default Vertical16 still selects its unchanged row
cache; byte paths and other shapes reject at unchanged guards before the modified
body. The other 204 comparisons, including all 51 passing parity proofs, are reused.

The vertical reference initially refused stale source identity before timing.
Only that worker was rebuilt, with unchanged compiler, flags and scaler archive.
Its source delta adds an unused single-pixel macro path, aliases the unchanged
vertical halo and extends untimed readiness fields. Frozen normal/scalar pixel
hashes, complete boundaries and native max1/RMSE0.25 gates remain unchanged.

All **3,580 Release tests per OS** pass (1,292 internal, 1,390 public, 898 live
FFmpeg), plus 141 scalar resize controls per OS. Four new controls exercise cache
wrap, integer-center five-tap support, row-band tails and the scale/admission
boundaries. The same actual Release package passes its consumer and all three
examples on Windows/Linux; symbols match its qualified DLL. Released 0.1.0
packages and tag remain untouched. No new performance workload or raster.

[Numeric evidence and archive hashes](warm-bicubic-paired-columns-evidence.json)
retain all statistics, receipts, refusals, trial identities and qualification.
The main raw archive is `parity-p09-qualified-evidence.tar.gz`, SHA256
`28b95bf7e797f6084f971effe5c3f021aa4804ff0dc1c2b3e2518bb5da527611`.
