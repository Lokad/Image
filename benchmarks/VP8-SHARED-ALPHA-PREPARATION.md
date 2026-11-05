# Shared VP8 alpha preparation

Opaque and weighted execution now use one borrowed row-pair/tile reader. Two
sixteen-pixel word tiles (128B) feed the existing exact luma kernel and opaque/
weighted chroma. A detached8-byte value shares four normalized alpha samples and
the truncated reciprocal across RGB; scalar fallback uses it too. No full-frame
copy, new heap buffer, owner, cache or pool is added. Zero/full-alpha unweighted
behavior, gamma, rounding, edges, hidden RGB and transparent cleanup stay exact.

One independent counterbalanced comparison confirms **834.78µs / 1.35% elapsed
improvement** on the frozen513×259 transparent RGBA16 control, Q75/effort4:
61,903.91→61,069.12µs. Twelve alternating pairs × sixteen complete operations
per subject, all retained; ratio interval **0.98537–0.98766**. Both subjects
allocate7,165,064B/op. Source normalization, fresh output creation/growth, complete
encode and independent output/disposal remain timed. Equal resident-source setup,
JIT and checks are excluded. This scoped managed-build gain is not FFmpeg parity.

| Complete frozen513×259 control | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Managed B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 + ALPH / normalization | 61,680.25 | 53,671.76 | 1.149 / 1.164 | 7,165,472 | 98,590 |
| RGBA8 + ALPH | 61,090.39 | 53,733.74 | 1.137 / 1.154 | 7,165,880 | 98,590 |
| Opaque RGBA16 | 16,886.67 | 15,603.30 | 1.082 / 1.085 | 1,201,552 | 72,380 |
| Opaque photograph, effort4 | 12,178.18 | 9,706.71 | 1.255 / 1.261 | 809,072 | 17,688 |
| Opaque RGBA8 | 16,808.81 | 15,548.79 | 1.081 / 1.086 | 1,201,552 | 72,380 |
| Opaque diagram, effort4 | 6,884.38 | 5,814.93 | 1.184 / 1.192 | 741,137 | 7,100 |
| Diagram, effort2 | 3,065.30 | 2,511.15 | 1.221 / 1.226 | 741,386 | 7,330 |
| Diagram, effort6 | 12,874.57 | 9,789.92 | 1.315 / 1.329 | 754,344 | 6,818 |

All eight costs remain gaps. All twelve actuals per case, including estimator
exclusions, both native brackets, exact complete packets and original pixel/
source/quality/ownership/quiet gates are retained. Other117 displayed costs reuse
unaffected proof. Native allocations and physical/complete admitted peaks are
unmeasured. Private comparisons have different batching/GC amortization from
current diagnostic B/op; there is no allocation-saving claim.

Alpha8's independent paired point improves0.14% but spans no change, interval
0.99259–1.00463. Opaque word/photograph three-pair screens are inconclusive.
The refreshed photograph ratio worsens1.197→1.255 while managed elapsed is nearly
flat and native is4.66% faster. One targeted twelve-pair comparison finds0.27%
less elapsed but remains uncertain, interval0.99355–1.00099. Retain the adverse
capture and clock movement without causal gain/regression or equivalence claims.
No repeated sampling, opaque retuning or broader speed claim follows.

Eligible alpha scratch admission rises **1→4KiB** before stack use. Opaque vector
4KiB and scalar1KiB remain. The reader borrows source/128B workspace; the8-byte
weight value owns no storage. Shared refusal is terminal, without scalar retry;
preparation releases scratch before returning. Retained result capacity and heap
allocation stay unchanged. This is coarse admission, not a physical-peak measure.

Windows/Linux pass **5,656 full/live checks each**,867internal/357public affected
checks normal/scalar and28 input cases with AVX512 disabled. Five new checks cover
all alpha sums/rotations/extreme values against BigInteger, independent scalar
byte/word YUV/gamma, hidden colors/rounding thresholds, edges/padding, zero warmed
allocation and both-depth scratch refusal. Existing cancellation, cleanup,
retained output and complete native packet checks pass. The **same actual private
Release package/all three examples pass both OSes**, including exact sync/async
alpha packets at both depths, scratch refusal/cancellation and surviving outputs.

One immutable production build per OS serves the round. Initial Linux tests
expose a stale1KiB allowance and128B allocation-harness observation. Fix only the
tests, isolate/warm the measured loop and assertion comparer, then rebuild the
internal assembly once. The production module never changes. Initial failed
source/module/logs and a pre-build log-prefix conflict are retained. Existing
test/native/harness/selector modules are reused; one private driver adaptation
supports bounded3/12-pair counts and accurate depth/alpha labels. No completed
performance measurement repeats.

The source/frozen-input audit finds33,410 mixed-alpha cells in both controls;
100,230 reciprocal expressions become33,410. These are source-level counts,
not measured division instructions, phase times or predicted savings. Reproduce
the audit with retained `wat311-reproduce.py`. Reproduce the stage with
`wsp312*-checks.py`, `wsp312-compare*.py`, refresh/qualification/package scripts.
[Evidence](vp8-shared-alpha-preparation-evidence.json) pins every producer,
paired sample, adverse observation and source epoch. The
[opaque stage](VP8-SHARED-INPUT-PREPARATION.md) retains its4.44% word gain;
its displayed VP8 costs are superseded here. Other affected H paths remain
historical. Full282-path parity stays open; publication is postponed. This is
development-source qualification, not a full actual-package release refresh.

Raw `vp8-shared-alpha-preparation-qualified.tar.xz`: 6,732,608B / 384 members,
SHA256 `9960d62548ab5c642ca402e7a7fcdfeca540a1f9ada25cd849163b406337613e`; mirrored/member-verified.
