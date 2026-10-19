# Reuse exact four-component materialization

Caller CMYK decoding takes **47.7% less elapsed time**, 1,010.51→528.07µs,
reducing its short gap from 3.057x to **1.590x**, upper 1.736x. Owned YCCK420
improves 9.0% to **1.463x**. Both remain gaps. Caller RGB420 independently
confirms parity at **0.962x / upper 0.966x**; the gray rejection control remains
a short gap. Allocations remain at the same approximate levels.

| Complete resident JPEG operation | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op | Input / output bytes | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Untagged → caller CMYK8 | 528.07 / 332.14 | 1.590 / 1.736 | 1,635,475 | 10,895 / 531,468 | Short; 3 actual |
| YCCK420 → owned RGBA8 | 1,033.62 / 706.68 | 1.463 / 1.539 | 1,470,252 | 6,704 / 531,468 | Short; 3 actual |
| YCbCr420 → caller RGB24 | 1,121.94 / 1,166.50 | 0.962 / 0.966 | 624,508 | 52,093 / 398,601 | Independent; 12 actual / 11 selected |
| Gray → caller GRAY8 | 868.77 / 646.09 | 1.345 / 1.411 | 273,422 | 147,825 / 132,867 | Short; 3 actual |

One selection in MaterializeSamplePlanes reuses the existing exact four-component
DC/sparse/general walk. This avoids general transforms for zero-AC blocks while
retaining the original binary64 arithmetic, full block work charge, component
order, progressive natural reorder, plane allocation, coefficient release and
general fallback. The preceding profile identified general IDCT cost; no new
profile, transform implementation, pool retention, cap or precision change.

The four rows above replace their prior timings because the common materializer
body changes. Owned full-resolution CMYK calls its private helper directly and
retains its evidence. All 220 other numeric rows, including 69 unaffected passing
proofs, remain identical. The changed RGB path gets its own independent repeat;
its old proof plus a new short was not treated as sufficient. All 24 actuals,
outliers, idle-v2 guards and native 50ms pre/post brackets are retained. A local
wrapper's selected-count assumption was corrected from terminal evidence without
repeating measurements; the conservative upper includes all 12 actuals.

Same frozen inputs, harness, native workers, compiled selectors and normal/scalar
CLI checks. Each side includes complete decode, necessary normalization, fresh
independent output ownership and release. CMYK keeps all four ink channels exact,
with no RGBA frame. YCCK keeps the approved native-only fixed-packet transition
maximum 16/interior 4, RGB RMSE ≤1.5 and exact alpha; managed pixels remain exact.
RGB/gray keep native maximum 4/RMSE 1.5. Source, caller padding, independently
surviving outputs, work/admission/cancellation and ICC-off/EXIF-opt-in behavior stay.

One immutable producer/OS passes 4,208 tests/OS (1,435 internal, 1,845 public,
928 live FFmpeg), 95 hardware-off checks/OS and the SAME actual private Windows
package consumer/all three examples on both OS. Reuse 67 existing focused checks
per OS; no redundant new tests, harness/native/selector rebuilds or package proofs
for unchanged inputs. Published 0.1.0 remains immutable.

Finite scope stays 247: 224 numeric and 23 unmeasured. All 70 current passing
proofs stop; 153 measured mean gaps and one confidence-open case remain.
The largest current gap returns to CMYK 2D sparse at 1.812x; its prior prefix trial
remains rejected. The named caller YCCK→RGB fallback still needs measurement.

[Numeric evidence](warm-jpeg-four-component-planes-evidence.json). Raw on both hosts:

- `h10s-measurements.tar.gz` SHA`7b188ac010e827b647cf0c8e978ed4ead74db9828b7146d6ef0b7be065800551`.

- `h10s-qualified-windows.tar.gz` SHA`e8302940ad0e8ecf30be0c478346cf5243f887e22ce2948a853b467ac203a9ba`.

- `h10s-qualified-linux.tar.gz` SHA`a1ab177577993f53700d2bdd982ca776d122b809d27e059223b3a7709c8d9c22`.
