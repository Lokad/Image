# PNG partial transparency expansion

The existing palette8 decode with a partial transparency table is **33.3% faster**
and independently passes warm parity: **0.984× FFmpeg**, conservative
upper **0.992×**. Pixels, hidden RGB and allocations remain unchanged.

| Existing decode | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Frozen IDAT B |
| --- | ---: | ---: | ---: | ---: |
| Palette8, 256 RGB / two alpha entries, None | 360.10 / 365.80 | 0.984 / 0.992 | 2,208 | 86,745 |

The prior partial-alpha route expanded every pixel in a scalar loop. The new safe
AVX512VBMI/BW route uses the existing complete-palette byte lookup strategy,
completing omitted alpha entries to 255 in registers. It adds no heap table,
stack array, persistent field, reservation or logical work charge. Every byte
index is valid for the complete 256-entry palette. Scalar tails, cancellation
checkpoints, RGB at alpha 0, resource/failure order and independent ownership
remain. Complete-alpha SIMD and its passing dispatch prefix retain their bytes;
packed/short palettes, sparse Adam7, parsing and inflation bodies remain unchanged.
Unsupported hardware retains the original scalar expansion.

Both sides decode the same original 513×259 authored projection and frozen packet,
then acquire and release an independent RGBA8 output. FFmpeg n8.1 includes full
PNG decode and PAL8-to-RGBA conversion, with one codec/conversion thread. Frozen
pixels, alpha, hidden RGB, normal/scalar native agreement, source preservation
and two live independent owners pass. Startup, I/O, IPC, preparation and validation
are excluded. This is decode only: no encoder or compressed packet changes.
Native allocation is unmeasured; managed B/op includes amortized harness overhead.

One three-actual 200ms screen measured 0.989×, then one independent twelve-actual
confirmation measured the result above. All 15 actuals, including the one BDN-unselected
confirmation actual, both 5s idle-v2 guards and 50ms native pre/post brackets remain.
Upper is max(selected/all-actual bounds) against the conservative native lower
bracket. CPU0 only, normal SIMD, tiering off, nonconcurrent workstation GC.
One immutable producer/OS and existing harness/selectors/reference served the round.
No new image, workload or settings grid, and no repeat-until-pass follows.

All **3,739 Release tests per OS** pass: 1,404 internal, 1,437 public, 898 live FFmpeg.
Focused 119 internal and 141 public PNG checks/OS pass; the 119 internal checks also
pass with AVX-512 disabled. Twelve added checks cover partial-alpha 64-entry group
boundaries and cancellation before output. Unaffected scalar qualification is reused.
The **same actual private Release package** passes its consumer and all three
usage examples on Windows and Linux. Timed production bytes remain unchanged.

[Numeric evidence](warm-png-partial-alpha-evidence.json) retains actuals, guards,
source reconciliation, frozen reference and package hashes. Only this selected
row changes; 206 comparisons and all 62 prior passing proofs remain valid. Full 207-case
parity remains open: **63 passes, 143 mean gaps, one confidence case**, with 124 selected
independent confirmations and 83 qualified shorts. This newly passing path stops.
