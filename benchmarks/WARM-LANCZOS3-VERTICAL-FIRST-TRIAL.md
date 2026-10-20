# Vertical-first RGBA16 Lanczos3 trial

The existing Reduce16 complete operation takes **825.27 µs**, against
**508.83 µs** for the approved native backend:
**1.622x/upper1.778x**. The candidate is rejected.
The accepted 804.22 µs implementation is restored exactly on both OSes.

| Existing Reduce16 operation | Lokad µs | Native µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Accepted certified FMA | 804.22 | 506.28 | 1.588 / 1.759 | 138,454 |
| Rejected vertical-first | 825.27 | 508.83 | 1.622 / 1.778 | 40,042 |

Reversing the exact separable axes removes the horizontal row cache and broadcasts
one integer coefficient across contiguous source pixels. One admitted source-width
row converts to double in place; the original strict certificate and exact fallback
decide every pixel. Allocation falls 71.1%, but elapsed time has no material
gain (mean 2.6% above the accepted result), so it does not advance the timing goal.

Fifty-seven focused Lanczos controls pass normally/scalar/AVX512-off per OS.
Independent integer/envelope checks and decimal/BigInteger end-to-end pixels cover
tails, negative lobes, tiny/zero/full alpha and long supports. Existing shared-root
tests cover retained outputs, replacement peaks, terminal refusal, cancellation
cleanup and subsequent success. The original full frame and native normal/scalar
quality gates pass, with independent outputs and source preservation.

One short retains all three actuals, native pre/post brackets and both official
idle-v2 guards. No new fixture, settings grid, worker/harness build, long repeat,
full-suite run or package proof for rejected code. Restored production retains its
4,469-test/OS and same-actual-package qualification. Accepted numeric rows stay
unchanged; full parity and release readiness remain open.

Next: resolve current-code uncertainty on the existing tight-memory Bicubic
TightWide8 case. Its historical 1.687x mean is the largest remaining named gap;
measure the current accepted producer before deciding whether to optimize.
[Evidence](warm-lanczos3-vertical-first-trial-evidence.json) retains raw statistics,
allocations, identities and the mirrored archives.
