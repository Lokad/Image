# Prepared-double Lanczos3 trial

Preparing one premultiplied double source row and reusing it across horizontal
FMA windows has no material elapsed gain. The candidate is rejected; accepted
production, test assemblies and symbols are restored exactly on both OSes.

| Existing Reduce16 complete operation | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Accepted certified FMA | 804.22 | 506.28 | 1.588 / 1.759 | 138,454 |
| Rejected prepared double | 815.63 | 506.45 | 1.611 / 1.796 | 154,894 |

The candidate mean is 1.4% above the accepted result and adds 16,448 bytes
of admitted row capacity. An independent prefix/error proof, exact integer row
checks, decimal/BigInteger final pixels and 58 focused Lanczos controls pass
normally, scalar and AVX512-off on each OS. The original full frame, native
normal/scalar quality, source preservation and independent output owners pass.
The strict rounding certificate and exact fallback introduce no pixel tolerance.

One three-actual short retains both official idle-v2 guards and native pre/post
brackets. The accepted producer's 4,478-test/OS and same-actual-package proof are
reused; no full suite or package proof runs for rejected code. All selected
numeric rows, the finite coverage checklist and parity/release objectives remain
unchanged. Earlier separate prepared-integer/on-demand-FMA trials remain rejected.

Next: qualify the already named two-retained-output shared-memory boundary using
the unchanged accepted build, before deciding whether optimization is needed.
[Evidence](warm-lanczos3-prepared-fma-trial-evidence.json) retains raw statistics,
identities, allocations and mirrored Windows/Linux archives.
