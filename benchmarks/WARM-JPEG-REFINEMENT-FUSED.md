# Fused progressive JPEG refinement

The later [cancelable refinement round](WARM-JPEG-CANCELABLE-REFINEMENT.md)
supersedes only the live-token444 timing below with a qualified short screen.
The four ordinary progressive comparisons retain their applicable independent
proofs. The results below describe this earlier round.

Grayscale warm decode falls from 3,343.59 to 2,665.76 microseconds, **20.3% less
managed time**. The 420/422 and ordinary444 cases improve about 15–16%; the
live-token fallback remains about 1.93x. This is a qualified intermediate gain:
**the <=1x goal remains open for all five selectors**.

One pinned CPU, normal SIMD, the existing original 513x259 byte raster and frozen
progressive packets are reused. Complete public decode, admission, reconstruction,
fresh independent output ownership and disposal remain timed against the same
FFmpeg software MJPEG reset/conversion/output boundary. No image, quality,
sampling or scan-strategy grid was added.

| Existing profile | Input B | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Gray | 75,112 | 2,665.76 | 1,715.98 | 1.553 / 1.557 | 1,232,526 | 12 / 12 |
| 4:2:0 | 52,364 | 2,671.13 | 1,954.79 | 1.366 / 1.371 | 1,637,526 | 12 / 12 |
| 4:2:2 | 71,251 | 3,283.57 | 2,241.02 | 1.465 / 1.471 | 1,996,661 | 12 / 11 |
| 4:4:4 | 132,961 | 5,312.60 | 3,355.07 | 1.583 / 1.589 | 2,682,364 | 12 / 12 |
| 4:4:4, live cancelable token | 132,961 | 6,477.56 | 3,352.81 | 1.932 / 1.939 | 2,682,364 | 12 / 12 |

Allocation totals include harness costs; small amortization differences remain
visible. No production heap storage is added. Native allocation totals remain
unmeasured. Packets and frozen managed/native output hashes are unchanged.

The guarded refinement primitive keeps the bit cache and exact work tally local
across a complete block, retains nonzero-position masks, and admits complete
correction fields speculatively. A refused field uses the original per-bit
partial updates and errors. Short-field reversal is an independently generated
four-bit permutation; signed-magnitude updates retain zero, sign, already-refined
bits and overflow behavior. Original fallback methods remain intact.

A block costs fewer than 2048 logical units, so the fast path requires at least
that remaining budget, no cancelable token and AVX2/BMI2. Tight budgets, live
cancellation and other hardware retain the preceding path. Canonical Huffman
tails publish state/work before using the existing reader. All exits preserve
the admitted prefix; no pixel cache or reduced work charge is introduced.

Thirty new xUnit cases cover short masks, 1.8 million signed-value comparisons,
complete refinement versus an independent one-bit traversal, valid long codes,
partial marker/truncation failures, work state and dispatch refusals. Both OSes
pass **3,576 Release tests**: 1,292 internal, 1,386 public and 898 live FFmpeg.
Both also pass 440 scalar JPEG controls and 10 public frozen work/pixel controls.
The **same actual Release package** passes its isolated consumer and all three
usage examples on Windows/Linux; symbols match the qualified Windows build.

Primary gray/420/422 screens precede independent twelve-actual confirmations.
After qualification, a separate two-case444 batch reuses the immutable core,
harness, native workers, fixtures and qualification. Every confirmation retains
all twelve actuals, selected/all-actual bounds and both versioned idle guards.
Cached selection drivers change only Case assignment before Setup; their timed
workload and the benchmark harness remain unchanged.

The preceding exploration stays historical and unaccepted:

| Grayscale-only trial | Screen µs | Decision |
| --- | ---: | --- |
| Dense SIMD updates | 3,347.37 | No useful gain |
| Scan-wide accounting | 3,165.51 | About5% gain, deferred; would disable the new fast path |
| Wide correction fields | 3,389.98 | Slower |
| Short-field wrappers | 3,355.66 | No useful gain |
| Branchless updates alone | 3,161.60 | About5% gain; helper reused only inside the fused path |
| Fused scalar traversal | 3,339.69 | No useful gain |
| Fused mask prototype | 2,642.36 | Partial-failure behavior corrected before acceptance |

Two six-second CPU profiles and one untimed packet counter guide the change.
The original packet has46% empty correction calls, 19% singleton calls and 24%
two-to-four; fewer than1% exceed sixteen. No timing is inferred from profiles.

[Numeric evidence](warm-jpeg-refinement-fused-evidence.json) retains all results,
guards, qualification, source/binary identities and exploratory archive hashes.
Main raw archive: `parity-p08-qualified-evidence.tar.gz`, SHA256
`ed321307b90ee41eed43d447c16a284336a31451e329dfe354d2576bc6adbefe`.

Only the five progressive timings are replaced. Baseline JPEG, encoders,
metadata/Stream acquisition, other codecs and every edit/resize keep their
executed source and existing proofs, including all 51 already passing <=1x
cases. Rerank the remaining gaps; this round does not certify uniform parity.
