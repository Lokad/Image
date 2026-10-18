# PNG RGB8 transparency confirmation

One independent quiet confirmation measures the existing RGB8+tRNS case at
**1.012x FFmpeg**, conservative upper**1.014x**. It remains an open
demonstrated gap, replacing the earlier0.999x/1.001x straddling measurement.

| Existing case | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | ---: | ---: | ---: | ---: |
| RGB8, tRNS key(48,139,231), None filter | 1,302.04 / 1,286.75 | 1.012 / 1.014 | 3,464 | 12 / 12 |

The unchanged513x259case retains exact RGBA, alpha and hidden RGB, normal/scalar
native output, frozen packet/reference pins, source preservation and independent
owners. Complete acquisition, parsing, inflation/conversion, owned output and
disposal remain timed. Startup, IPC, I/O, preparation and validation are excluded.
All12actuals/outliers, native50ms brackets and both `exclusive-vm-idle-v2` guards
are retained. Allocations include harness; native allocation is unmeasured.

The already-qualified immutable producer and archived selector were reused with
zero builds or production changes. Existing Windows/Linux correctness, live FFmpeg,
scalar and same-actual-package proofs remain applicable; no package proof repeats.
Only this measurement changes. Other206comparisons/all60passing proofs remain;
totals are60passes/144gaps/3confidence,127independent+80qualified shorts.
[Numeric evidence](warm-png-rgb-transparency-confirm-evidence.json) records the
complete boundaries, pins, actuals, conservative bounds and reused qualification.
The full207-case warm parity objective remains open; no repeat-until-pass follows.
