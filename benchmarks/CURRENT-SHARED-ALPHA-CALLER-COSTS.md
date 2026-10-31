# Shared-alpha caller costs

The ten named bilinear/bicubic controls affected or potentially affected by the
[exact-alpha change](WARM-EXACT-ALPHA-INT128.md) are reconciled. Production is
unchanged. Six retain valid measurements because their frozen inputs cannot
reach the changed callee; four have fresh independent quiet-VM confirmations.
The previously passing word vertical enlargement still passes and stops.

| Existing complete warm one-CPU resize | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| bicubic-Up16 | 3,541.698 | 1.230 / 1.244 | 5,016,602 | confirmed-gap |
| bicubic-VerticalUp16 | 1,626.022 | 0.973 / 0.985 | 2,514,272 | confirmed-pass-stop |
| bilinear-Up16 | 2,609.380 | 1.395 / 1.414 | 4,498,923 | confirmed-gap |
| bicubic-Up8 | 3,649.155 | 1.200 / 1.212 | 2,884,554 | confirmed-gap |

The six no-call controls are bilinear Down8/Down16 and bicubic Down8,
TightCross16, Tight32x16_8 and Down32x16_8. Cached independently qualified frames match
complete output hashes. Down8 bilinear instead reproduces the frozen input
hash and computes only its exact alpha plane; it does not claim a regenerated
RGBA output. Exact alpha minima are 62, 15,891, 48, 7,131, 115 and 115 respectively.

A minimum rounded alpha alone does not prove reachability. An exact-rational
analysis bounds coordinate/coefficient and accumulation errors, including
packed-coefficient truncation and scalar/SIMD association. Deliberately loose
coefficient error 2^-12 gives strict floating-alpha lower bounds 49.027, 12,681.765,
27.670, 559.308, 107.235 and 107.235. All exceed 1, so neither the tiny-alpha branch nor
its deferred fallback can enter ResizeAlpha.Round. This analysis bound changes
no pixel tolerance. Only ResizeAlpha.cs changed in the original production delta.
Reused measurements keep their existing confidence status; no new pass or CPU
gain is inferred from this proof.

The four confirmations reuse the same immutable A128 library, cached selectors,
workers and inputs. Frozen managed/native frame hashes, exact pixels, original
source, independent output ownership and quiet-v2 checks all pass. VerticalUp16
retains only its approved native bound: RGB error ≤2 where reference alpha ≤16,
≤1 elsewhere; alpha error ≤1 and both RMSE ≤0.25. Other cases keep their original
bounds. No quality or tolerance changes. Native allocation remains unmeasured.

No production, core, test, harness, selector or native build; no new fixture,
profile or timing repeat. The initial incorrect bilinear worker filename was
fixed during preparation, before scoring. Existing 5,267 checks/OS, live tests,
the same actual Windows Release package on both OSes and three examples remain
applicable. These costs are not causal improvements over older producer timings.

[Numeric evidence](current-shared-alpha-caller-costs-evidence.json) retains the
proof, original evidence links, all actuals, producer hashes and confidence
views. Raw shared-alpha-caller-reconciliation.tar.xz, SHA256
56c6b1dafeac63ddbc2792ff7c1fcc24bd6d9d1992240e0491f611146cc1cdd2,
is mirrored and all 92 members verified: frames, scripts, receipts, guards,
selectors, actual library and workers. Required source archives are pinned in
the record. Reproduce the same named controls with the
[reference tooling](../tools/WarmReference/README.md).

The full 268-path ≤1× objective remains open. The largest confirmed substantive
gap is tight-memory byte bicubic reduction; already passing paths stop.
