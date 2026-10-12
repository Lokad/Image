# Bounded PNG compressed-input reuse

PNG decode now leases concatenated IDAT storage from a separate one-array cache,
retained at most 1 MiB. Capacity equals the admitted compressed byte count, every
byte is copied before framing, and the lease ends after the inflater/streams
are disposed on every success or failure exit. Larger inputs stay uncached.
Pixel reuse retains its separate 4 MiB ceiling. Live leases never share arrays;
caller input and returned pixels are independent. Admission, work charges,
framing, inflation, cancellation and failure ordering are unchanged.

One palette8 + full tRNS case reuses the original 513x259 RGBA8 projection and
identical resident PNG bytes. Both subjects complete independent RGBA acquisition
and release. Native PAL8 decoding/conversion/allocation remain timed on one CPU
with normal SIMD and reused contexts. Managed outputs and private input storage
may reuse released arrays; no output is cached. Native CLI normal/scalar pixels,
alpha/hidden RGB, held owners and source are exact. Startup/I/O/IPC and validation
remain outside timing.

| Quiet screen | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Released output only | 886.34 | 364.35 | 2.433 / 2.634 | 89,564 |
| Released output and private input | 867.02 | 361.58 | 2.398 / 2.597 | 2,760 |

Allocations fall **96.9%, from 89,564 to 2,760 B/op**; the measured GC record has
no collections in the new screen. The small timing change is not independently
certified. **2.398x / upper 2.597x remains open**, with both unchanged quiet guards
passing and all three actuals retained. No long repeat is launched. Allocations
include harness overhead; native totals are unmeasured. Held or first-use owners
remain distinct from this released-storage warm loop.

Each OS passes 2,650 Release tests (896 internal, 858 public, 896 live FFmpeg),
35 focused PNG public controls normal/scalar, and five pool controls. Tests cover
separate ceilings, exact capacities, live lease independence, different equal-size
inputs around a partial failure, unchanged caller bytes, all 72 authored PNG
profiles and exact memory/work thresholds. The same actual dependency-free Release
package and all three usage examples pass on both OSes; all 583 fresh/timed/profile
method bodies and flags match the package.

One six-second diagnostic profile has 5,974 samples: BCL inflation 37.8%, literal
framing 23.8%, framing Validate 10.4%, canonical decoding 1.1%, and palette
expansion 19.7%. Both guards pass. This attributes CPU work, not a parity result.
With remaining allocation small, prioritize substantive parsing/inflation work.
The earlier [packed-table trial](WARM-PNG-PALETTE-WORD-REJECTED.md) stays reverted
and unscored after its failed post-guard.

The [numeric evidence](warm-pnginput-reuse-evidence.json) retains every actual,
bracket, input/raster/source hash, four timed child identities, raw/QA/package/
profile archives and the earlier allocation observation. Other reported gaps and
required/material unknowns remain open; this allocation win does not complete
the uniform warm target.
