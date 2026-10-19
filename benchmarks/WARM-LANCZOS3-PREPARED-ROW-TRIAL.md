# Rejected Lanczos3 prepared-row trial

Updated 2026-10-20. **Rejected; the qualified producer and all selected rows are
restored unchanged.** No parity, package or replacement-readiness claim follows.

| Rejected complete operation | Current µs | Candidate µs | Reference µs | Ratio / upper | Candidate B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Reduce16 | 931.20 | 862.41 | 507.09 | 1.701 / 1.850 | 154,873 | 7.4% |
| Enlarge16 | 6,661.47 | 6,408.76 | 3,941.91 | 1.626 / 1.685 | 4,625,322 | 3.8% |

The candidate prepared one admitted RGBA16 premultiplied source row and reused it
through overlapping horizontal windows, avoiding repeated source-alpha products.
It kept exact signed weighting, Int128 vertical sums and normalization. Successful
row work was admitted in one block with the original total; tight work/workspace
selected the old path before reservation. Shared refusal remained terminal.
The improvement is modest for an extra array, duplicated weighting path and about
8 KiB allocation per operation. This variant is parked; no further variants/grid.

One immutable build and one quiet three-actual screen per existing 16-bit case,
normal SIMD/one CPU, native brackets before/after and both idle-v2 guards. No long
run above parity. Reference remains the approved zimg/FFmpeg zscale backend with
unchanged complete normalization/ownership/quality boundaries. Figures are
experimental shorts, not selected fully qualified production results.

All 48 focused candidate checks pass normally, all-intrinsics-off and AVX512F-off
on Windows/Linux. The four original full output frames remain exact on Linux.
Tests cover negative/full-precision weights, tails/1024-tap polling, independent
BigInteger results, tight local workspace, aggregate/root refusal at every growth,
retained results, cancellation cleanup and subsequent success. Candidate tests,
source and actual binaries are retained in raw archives.

Original source bytes and exact qualified producer DLLs are restored on both OS;
47 restored focused checks pass per OS. Existing full/package/consumer proofs
remain valid; no repeated full/package qualification for a rejected experiment.
A separate scalar-FMA arithmetic prototype checks 1,003 original weighted-window
controls: 1,001 certificates agree with the exact integer result; two half-tie
controls require exact fallback. Prefix/error-envelope checks cover maximum
Int32 tap counts analytically. This is development evidence, **not a production
implementation or performance result**; actual SIMD, work and ownership integration
still need qualification. Its source/results are retained in the restored Windows
archive. The finite checklist remains **254 =237 numeric+17 unmeasured**. Next: a rigorous
double/FMA accumulation certificate with exact fallback, targeting the expensive
integer weighted arithmetic. Pixel precision and Q30 plans remain required.

[Evidence](warm-lanczos3-prepared-row-trial-evidence.json). Linux rejected raw:
`ebc88a3285546685226dbc8bf2d54bb1f7e906dd30808c5e9b4809718e26e569`; Windows rejected raw:
`7c19139f417efb8b1cab31e1b5d5f3f91e16ccb4a4206d78ebf7e86d2b1c0c34`. Both archives are mirrored on both hosts.
Old diagnostics were losslessly compressed with round-trip hashes/modes. Older
loose receipts were archived, all members verified, mirrored before pruning;
source, current dependencies, qualified producers/consumers/releases and all raw
proofs remain intact. Both checkouts stay below 5 GB, including build peaks.
