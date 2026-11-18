# Bicubic storage on the current package

Of the 61 original bicubic controls, 59 have valid passing evidence and two
remain uncertain. The actual 0.2.1 package covers all 59 pool-affected
controls. The two 4,264,104-byte RGBA16 full enlargement outputs exceed the
unchanged 4 MiB pool cap and retain their source-valid passes.
[Full word enlargement evidence](BICUBIC-ENLARGEMENT-HORIZONTAL-MODES.md)

The open cases are tight RGBA8 128×64 reduction and tight RGBA16 width
reduction with height enlargement. The default RGBA8 reduction now passes
on the [qualified output row loan](BICUBIC-DEFAULT-ROW-LOAN.md).

## Original package comparison

| Complete operation | Lokad.Image µs | Prepared zimg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Default RGBA8 reduction | 416.73 | 417.10 | 0.945039–1.039399 | 199,984 | Uncertain |
| Tight RGBA8 reduction | 414.38 | 419.26 | 0.974644–1.003866 | 113 | Uncertain |
| Tight RGBA8 enlargement | 3,031.48 | 3,301.11 | 0.906749–0.925615 | 116 | Pass |
| Default horizontal RGBA16 enlargement | 1,496.19 | 1,572.94 | 0.932678–0.961315 | 114 | Pass |

All cases retain twelve actuals, both native brackets and idle-v2 guards.
The original workload delegates share one runtime with 127 verified non-core
files and the immutable package DLL. The complete public Resize includes
preparation, admission, filtering, exact repair and independent output disposal.
The labeled FFmpeg zscale backend retains geometry and scratch. Packing, filtering,
normalization and output ownership stay timed. Exact managed output hashes,
original native bounds, source preservation and independent output checks pass.

Default RGBA8 reduction's first seven managed actuals range from 414.55 to
415.38 µs. Its final three range from 424.94 to 435.16 µs. Native bracket
means move from 417.10 to 423.40 µs. The adjacent confirmation below investigates this shift. Its cause remains
unverified. The tight
reduction's upper is 0.999062 for the filtered view and 1.003866 with every
actual; it stays open under the conservative rule. Neither reduction establishes
a production gap. Both enlargements pass and stop.

This is measurement-only work on the previously qualified package. GC allocation
stays separate from retained storage, admitted capacity and physical peak.
The mirrored `bicubic-storage-current-package-confirmations.tar.xz` has 28 verified
members and 109,044 B, SHA-256
`23a5cbf0938c4478ab5043d267d781b4253f9aa545642fe73fb351920a143245`.
It retains the original and current commands, every sample, quietness guards
and validity checks. [Evidence](bicubic-current-package-evidence.json)

## Adjacent native confirmation

Default RGBA8 reduction remains uncertain at **414.12 / 416.74 µs**, interval
**0.895484–1.024193**, **199,985 B/op**. Each original managed actual has
240 complete native operations immediately before and after, outside its clock.
The twelve actuals use 496 operations each under the original 200 ms pilot rule.
Both original native brackets also remain. The before-actual paired upper
is 0.995325. The after-actual paired upper is 1.010318. The faster original
bracket and all-actual rule keep the full result open. The reference groups
still differ, so the result supports neither a parity pass nor a demonstrated
gap. Further identical captures are deferred pending a concrete timing
diagnosis or affected production change.

The initial launcher supplied an extra argument. The driver rejected it before
setup or sampling, and the corrected command reused the same binary, runtime,
input and checks. The mirrored archive retains both attempts, all valid samples,
guard and validation records. `bicubic-default-byte-interleaved-confirmation.tar.xz` has 17 verified
members and 23,980 B, SHA-256
`e7c5a136f1940309ecbb731b6f24e8c6ff00cadc6cab678969f5243f1133e160`.

## Mixed-axis RGBA16

| Complete operation | Lokad.Image µs | Prepared zimg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Tight width up, height down | 872.48 | 907.53 | 0.927146–0.974925 | 114 | Pass |
| Default width up, height down | 804.55 | 847.89 | 0.845794–0.999391 | 114 | Pass |
| Tight width down, height up | 820.10 | 825.87 | 0.964968–1.007289 | 113 | Uncertain |

All three results fit into the changed idle pool. The local 4 KiB workspace
limit still allows that pool. Each frozen case matches its latest passing
receipt on the input, managed and native output, operation and reference
identities. One existing runtime with 127 verified non-core files serves all
three. The original complete workload loops, twelve actuals, natural GC
and both native brackets and quietness guards remain. Exact managed pixels,
approved reference bounds and surviving ownership checks pass.

Both width-enlarging cases pass and stop. The default case's two native bracket
means differ, at 926.34 and 847.89 µs. The conservative interval uses the
faster bracket and all managed actuals. The tight width-reducing case stays
uncertain, including its filtered comparison. These clocks establish current
costs. They give no causal estimate of a storage change.

The mirrored `bicubic-mixed-word-current-package-confirmations.tar.xz` contains 26 verified members
and 111,552 B, SHA-256 `f5d3673b399303028d2dcd1bb8913a046decf8c3c8581066910b828cef22e457`.
It includes all samples, matched prior receipts, identity checks and quietness
guards. [Evidence](bicubic-current-package-evidence.json)

## Tight 128×64 reductions before compact word preparation

| Complete operation | Lokad.Image µs | Prepared zimg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8 | 380.07 | 375.17 | 0.992689–1.024773 | 112 | Confidence open |
| RGBA16 | 469.55 | 444.85 | 1.012479–1.078234 | 112 | Gap |

The same frozen driver ran twelve alternating 64-call pairs and both
twelve-sample reference brackets for each case. Both reductions start from
513×259. The working limits are input and owned output plus 4 KiB. Managed
preparation, filtering, exact repair, independent output and disposal are
timed. The zimg 3.0.6 FFmpeg scaler backend retains the graph and scratch.
Pixel processing, packing, output allocation and release remain timed.

The RGBA16 paired interval is 1.019543–1.050974. Native group means run from
444.85 to 457.33 µs. Even the conservative interval that includes all
reference groups shows a gap, with a 24.71 µs point difference against the
fastest bracket. RGBA8 remains uncertain under the full rule. Earlier passing
measurements are tied to their original package. The elapsed-time difference
between package versions has no verified cause.

All original input and output hashes, exact managed pixels, approved native
bounds and independent output checks pass. The owned output sizes are
32,768 and 65,536 B. GC allocation is 112 B/op for both, separate from owned
capacity and retained or physical memory. The driver, 128 non-core runtime
files and both reference workers were reused without a build. Both guarded
captures used the current immutable 0.2.1 package.

The mirrored `bicubic-bounded-wide-current-package.tar.xz` contains 15 verified
members and 24,548 B, SHA-256
`fa6c8307079760183b675fdc2c5e2ae86698f4ade4c9404c5d44c0897c895a8a`.
It retains both captures, matched original receipts and guard results,
driver and reference identities, and the reproducing command.

### RGBA16 gap attribution

Twelve alternating 64-call managed pairs on the same VM compare the earlier
and current immutable modules under the same public Resize boundary.
They measure **465.28 and 464.93 µs**, with a current/earlier interval of
**0.988575–1.010048** and **112 B/op** per side. Pixel and surviving-owner
checks pass. The matched comparison gives little support for a material
slowdown from the module change. The standalone prepared-zimg gap stays open.

An independent six-second CPU profile on the current package records 5,951
samples and zero lost samples. The four-column word executor accounts for
5,523. Its paired horizontal tap loop accounts for 1,202, including the
repeated coefficient broadcasts and inserts. A source unpack block accounts
for 1,008 and the vertical quartet loop for 613. Word quotient/bounds and
certified packing account for 352 and 168 samples in their two hottest blocks.
These are block sample counts. The first implementation trial will prepare
compact paired horizontal coefficients once per eligible strip, reusing the
existing weight storage and borrowing decoded scratch for rare exact replay.
The [existing byte preparation](BICUBIC-PAIRED-COEFFICIENT-PREPARATION.md)
provides a lifetime and tap-order precedent. The earlier [expanded-coefficient
trial](BICUBIC-EXPANDED-COEFFICIENT-TRIAL.md) retains its adverse evidence.

The first profile finished, but its root-owned output could not be copied out
of temporary storage. The export was corrected and a valid non-timing profile
captured. The matched-DLL receipt was recovered from its completed samples
after correcting the serialized tuple field names. One private diagnostic
driver was built. Both production modules and the profile driver were reused.

The mirrored `bicubic-word-gap-module-and-profile.tar.xz` contains 31 verified
members and 104,732 B, SHA-256
`23160c69b5a1f13a9cd3daa6ed1573dd421b0dfca83ddb9f5e0c1fa9633e00f4`.
It retains the scoped managed pairs, raw profile, sampled instruction blocks,
guards, exact output evidence and both tooling corrections.

## Compact paired RGBA16 coefficients

The 513×259 to 128×64 resize with 4 KiB of extra workspace passes on the
actual Release package at **405.66 / prepared zimg 448.29 µs**,
interval **0.891280–0.918803**, and **112 B/op**.
The original driver retains twelve alternating 64-call pairs, both native
brackets, complete managed preparation and disposal, and the prepared reference
boundary. RGBA8 remains uncertain at its original current-package interval.

Eligible sixteen-tap word pairs pack four taps from each column into one
eight-double group in their existing coefficient storage. This replaces
repeated broadcasts and inserts with one grouped load and lane selection.
The original tap order, separate multiply/add operations and certified
publication stay intact. During exact repair, one lane's original coefficients
are reconstructed in decoded scratch after all horizontal users complete.
Fallbacks retain the original arithmetic and tiny-alpha unwind. The operation
keeps its 4 KiB workspace and owns a 65,536 B result. There is no added retained
working storage. Physical and native peaks were unmeasured.

In matched private DLL pairs, the prototype saves **16.32 µs /
3.76%**, interval **0.956689–0.968184**.
Both sides allocate 112 B/op. The first standalone capture was unscored after
CPU0 I/O wait reached 1.40% in the post-run guard. Putting the harness's untimed
validation and fixture writes in memory preserves the original binaries and
operation. The valid prototype comparison is 420.56 / 444.30 µs,
upper 0.957916. The Release-DLL result above establishes parity for
this case. Cross-capture clock differences do not measure the implementation gain.

Windows and Linux each pass **6,849 checks** including live FFmpeg.
The 94 focused tests include original ordered SIMD bits, coefficient restoration,
clipped and odd strip paths, terminal refusal, cancellation and independent
survivors. Scalar and Linux AVX512-disabled checks pass. Both platforms consume
the same actual package with all three usage examples. The package contains the
measured Linux DLL without rebuilding it.

Only the bounded 128×64 RGBA16 comparison reaches the changed path in the
existing measured bicubic set. Default wide RGBA16 uses its prior full-row
cache route. The byte stages keep their byte-specific path. Existing results
for those workloads retain their producer identities.

The mirrored `bicubic-compact-word-weights-qualified.tar.xz` contains 50 verified
members and 1,893,128 B, SHA-256
`0415a9a2b9ceb5dbeb6df769b214ba9988f89c33d99f64c2d6b713d7409f45b6`.
It retains the exact source, immutable private and Release binaries, timings,
guard failure, builds, qualification logs and actual package.

## Additional current-package cases

The first batch passed twenty-two of twenty-three cases on the same actual
package. The [interleaved confirmation](#interleaved-confirmation-for-tight-6432-rgba8-reduction)
now closes the tight 64×32 RGBA8 reduction. The original measurements follow.

| Complete operation | Lokad.Image µs | Prepared zimg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Tight RGBA16 reduction to 16×8 | 362.74 | 534.04 | 0.670029–0.683124 | 112 | Pass |
| Tight RGBA8 width enlargement | 1,427.16 | 1,534.77 | 0.911409–0.946599 | 114 | Pass |
| Default RGBA8 enlargement | 3,007.79 | 3,271.89 | 0.883457–0.932694 | 116 | Pass |
| Tight RGBA8 width up, height down | 921.58 | 944.33 | 0.968241–0.984461 | 114 | Pass |
| Default RGBA8 width up, height down | 921.08 | 939.58 | 0.961704–0.988217 | 114 | Pass |
| Default RGBA8 width down, height up | 810.11 | 902.40 | 0.886220–0.906412 | 199,938 | Pass |
| Tight RGBA8 reduction to 64×32 | 384.37 | 384.07 | 0.950588–1.014632 | 113 | Confidence open |
| Tight RGBA16 reduction to 32×16 | 435.03 | 442.74 | 0.962730–0.991529 | 113 | Pass |
| Tight RGBA16 width enlargement | 1,433.74 | 1,548.01 | 0.908726–0.931626 | 114 | Pass |
| Default RGBA8 height enlargement | 1,473.84 | 1,840.39 | 0.780064–0.812049 | 114 | Pass |
| Tight RGBA8 height enlargement | 1,562.10 | 1,830.43 | 0.818744–0.864226 | 126 | Pass |
| Default RGBA16 height enlargement | 1,389.73 | 1,825.94 | 0.746134–0.775474 | 114 | Pass |
| Tight RGBA16 height enlargement | 1,380.69 | 1,822.25 | 0.748878–0.765398 | 114 | Pass |
| Default RGBA16 reduction to 256×129 | 408.45 | 575.33 | 0.693044–0.723866 | 199,958 | Pass |
| Default RGBA16 width down, height up | 732.70 | 825.62 | 0.865501–0.898772 | 199,898 | Pass |
| Tight RGBA8 width down, height up | 754.04 | 840.84 | 0.878112–0.913422 | 113 | Pass |
| Tight RGBA16 reduction to 256×129 | 495.34 | 569.89 | 0.863185–0.873750 | 113 | Pass |
| Default RGBA8 width reduction | 407.09 | 573.55 | 0.697881–0.718390 | 199,980 | Pass |
| Default RGBA16 width reduction | 499.47 | 580.50 | 0.846519–0.867729 | 199,929 | Pass |
| Tight RGBA8 width reduction | 490.21 | 576.13 | 0.839039–0.862076 | 113 | Pass |
| Tight RGBA16 width reduction | 445.73 | 578.89 | 0.758921–0.782179 | 113 | Pass |
| Default RGBA8 height reduction | 495.71 | 631.51 | 0.766276–0.804162 | 367,676 | Pass |
| Default RGBA16 height reduction | 471.14 | 623.43 | 0.731037–0.781547 | 367,596 | Pass |

The 16×8 word case retains its original twelve alternating 256-call pairs
and twelve native samples in each outer bracket. The earlier uncertain
capture and the later streaming-column pass remain in the evidence.
The new result confirms the current package. The compact-weight change
does not execute on this shape. [Streaming-column results](STREAMING-COLUMN-REUSE.md)

The other twenty-two cases use three sequential batches with the same 127 verified
non-core runtime files and original generated delegates. Each retains three warmups, twelve measured
actuals with natural GC and twelve samples per native bracket. The conservative
interval includes both the filtered and all-actual views. The 64×32 byte
interval spans parity. Its point difference is 0.30 µs.

Every case preserves the original input and output hashes, exact managed
pixels, native bounds, source and independent output checks. The reference
retains zimg preparation. Both idle guards pass with untimed files in memory.
No new build or production change was needed. Allocations are reported
separately from owned capacity and idle retention. Nine existing summary rows use the refreshed measurements.

The mirrored `bicubic-current-package-extended-cases.tar.xz` contains 55 verified
members and 54,932 B, SHA-256
`2c474d5cc4e48535dd41ebe08a23493bd6ca602a799f34143f449d2bd2aae70d`.
It retains the original and current receipts, complete timing records and
reproduction commands. Validation of the 16×8 bracket counts uses the
adjacent driver's log, where all twelve original pairs and both twelve-sample
brackets are retained.

The axial enlargements and default word reduction are retained in the mirrored
`bicubic-current-package-axial-and-word.tar.xz` with 40 verified
members and 36,916 B, SHA-256
`c3c263816b7139098ab1852053802d44fb5e5d6db7b1fc0b2a878f48af282944`.
Both word height enlargements retain their approved low-alpha native limits.

The nine further mixed-axis and axial reductions are retained in the mirrored
`bicubic-current-package-axial-reduction-cross.tar.xz` with 58 verified
members and 52,388 B, SHA-256
`02e428f80a128445710f996ef6ac01275a12240be520c1420706f295c4245614`.
The exact certified sample-half results and existing native bounds remain.

## Default width enlargement and smaller reduction cases

All fourteen additional cases pass on the actual package. The default 128×64
RGBA8 reduction retains twelve 256-call pairs and the default width enlargement
retains twelve 64-call pairs. Both keep their two twelve-sample native brackets
and all 128 original non-core files.

| Complete operation | Lokad.Image µs | Prepared zimg µs | Upper ratio | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Default RGBA8 reduction to 128×64 | 371.44 | 381.07 | 0.987210 | 198,350 |
| Default RGBA8 width enlargement | 1,586.68 | 1,668.84 | 0.960436 | 112 |
| Default RGBA16 reduction to 128×64 | 389.86 | 459.82 | 0.857492 | 198,445 |
| Tight RGBA16 reduction to 64×32 | 400.85 | 429.31 | 0.940276 | 113 |

Twelve smaller reductions and axial cases retain the generated delegates and the same 127 verified
non-core runtime files. The exact managed rational frames, scoped reference
bounds, independent owners and both idle guards pass throughout. The warm
reference retains its original prepared graph and scratch. Each retains twelve measured
actuals and both native brackets. There were no builds.

The mirrored `bicubic-current-package-adjacent.tar.xz` retains 15 verified members and 25,848 B.
SHA-256 `f271a345194a23261280569c4999b49ae940bd4ab04e829d9b3ff2b3ebdc4caa`.

The mirrored `bicubic-current-package-small-and-vertical.tar.xz` retains 76 verified members and 63,440 B.
SHA-256 `be4a6b29ee785330fa25c027bc9b5e0e1fac72067d9f76a61b6c45e71ec80aa2`.

## Final pool-affected controls

All thirteen original small reduction cases pass with their generated operation
loops, at conservative upper ratios of 0.096139–0.916770 and 113 B/op.
Each keeps twelve actuals, two native brackets, its exact managed output and the
original native pixel, source, ownership and quietness checks. The evidence
records every shape and sample. No new controls or builds were needed.

The mirrored `bicubic-current-package-extreme-reductions.tar.xz` retains 82 verified members and
65,052 B, SHA-256 `fe8f8dcc16eea25a0a8d47a0a8e3f47d661d85f31e89f187fc57c518e6e3c028`.

## Prepared plans

The four separately required prepared paths pass on the same actual package.
The shared pool affects the independently owned result of each plan call.
The inputs and retained preparation remain those of the original controls.
[Previous qualification](CACHED-BICUBIC-SHARED-HORIZONTAL-SCHEDULE.md)

| Complete prepared operation | Lokad.Image µs | Prepared zimg µs | Conservative interval | GC B/op | Retained plan B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Wide RGBA8 to 128×64 | 279.83 | 346.38 | 0.798853–0.819148 | 137 | 198,432 |
| Wide RGBA16 to 128×64 | 310.36 | 419.12 | 0.735033–0.744601 | 137 | 198,432 |
| Narrow RGBA8 to 256×129 | 391.85 | 417.84 | 0.902302–0.952322 | 137 | 199,840 |
| Narrow RGBA16 to 256×129 | 398.53 | 573.73 | 0.666640–0.708526 | 137 | 199,840 |

Both sides prepare before timing. Fresh source reads, exact filtering and
repair, and an independently owned result and disposal stay in each managed
operation. Native packing, filtering, normalization and output acquisition
and disposal stay timed. Native scratch is reported separately from total
retained state. The narrow governed peaks remain 331,936 and 464,032 B for
byte and word. The wide benchmark receipts report retained capacity.

Two frozen runtimes each reuse 33 non-core files and the original generated
loops. Twelve managed actuals and both twelve-sample native brackets use
natural GC, original exact pixel, ownership and reference gates, and both
quiet guards. One tooling build reuses the original actions. Production,
package, harnesses, fixtures, reference workers and qualified cases remain
as recorded. [Evidence](bicubic-prepared-current-package-evidence.json)

The mirrored `bicubic-prepared-current-package.tar.xz` retains 29 verified members and
29,684 B, SHA-256 `dd6a6391917096c05cbacee93946f3b6f287ce94a6b45285a60251fafdd7f080`.

## Adjacent sampling for the tight byte reduction and mixed word resize

Both comparisons remain uncertain on the actual package under the full
conservative rule. The earlier bracket-only comparisons remain in their
original tables.

| Complete operation | Lokad.Image µs | Prepared zimg µs | Conservative interval | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Tight RGBA8 reduction to 256×129 | 444.06 | 446.80 | 0.934107–1.050981 | 113 |
| Tight RGBA16 width down and height up to 256×519 | 825.67 | 820.97 | 0.979831–1.034007 | 121 |

The byte case's two paired upper bounds are 0.994563 and 0.998267.
Its full bound remains open. The word case's paired uppers are 1.023795
and 1.020481. Both native brackets and the two adjacent groups remain in
the conservative interval. Timings do not establish a production regression
or explain the clock movements.

Each case keeps twelve managed actuals, natural GC and the original generated
loops. Adjacent native groups use 240 calls per side for byte and 120 per
side for word. Exact pixels, approved native bounds, source preservation
and independent outputs pass. Both quiet guards pass per case. One
tooling build reuses 127 original non-core runtime files with the actual
package and unchanged native workers. No unchanged repeat follows from
these outcomes. [Evidence](bicubic-current-package-evidence.json)

The mirrored `bicubic-tight-adjacent-current-package.tar.xz` retains 20 verified members and
35,664 B, SHA-256 `2194789851b083158d926ae31989564997688434cf380dc4c8b13f80e910834f`.

## Interleaved confirmation for tight 64×32 RGBA8 reduction

The original reference group means were 384.07 and 397.64 µs, a 3.5% spread
across groups. With interleaving, the tight RGBA8 reduction passes on that
same package at **391.88 µs / prepared zimg 405.68 µs**, conservative interval
**0.953692–0.976805**, with **112 B/op**. The separate paired interval is
0.957786–0.974268. The native group means are 405.73, 405.68 and 405.97 µs.
The earlier 1.014632 upper and every observation are retained. The cause of
the variation between sessions remains unverified.

The run keeps 127 frozen non-core files and the original generated workload
delegates. Each of the twelve units keeps 528 complete managed operations
and 128 complete native operations per group. Sixteen subdivisions each run
33 managed calls and three sets of eight native calls. The source, exact
managed output, original native RGB/alpha bounds, independent ownership and
both quiet guards pass. One tooling build and the existing package suffice.

The mirrored `tight64x32-bicubic-partitioned-current-package.tar.xz` has
14 verified members, 146,920 B and SHA-256
`6d977b7b57513844709c370b65d69f2f659fc377f0d1a00504b8a0fd8dc89259`.
The [current evidence](bicubic-current-package-evidence.json) pins this
archive, all actuals, the frozen runtime and the prior receipt.

## Interleaved timing for tight 256×129 RGBA8 reduction

The current package measures **458.97 µs / prepared zimg 460.68 µs**,
**112 B/op**. The paired interval is **0.992053–0.997669**, and the full
conservative interval is **0.986948–1.002833**. The case remains uncertain.
Native group means of 460.68, 461.34 and 461.59 µs differ by about 0.2% in
this capture. Both earlier adjacent paired passes and the wider 1.050981
upper remain in the evidence.

With the same library, worker and 127 pinned runtime files, the twelve units
each contain 448 complete managed operations and 128 per native group.
Sixteen subdivisions interleave 28 original generated managed calls with
three eight-call native samples. All exact input, pixel, native output,
ownership and quiet checks pass. One tooling build was needed. The measured
variation is small enough to defer further sampling with this code and
schedule while pursuing a concrete source improvement.

The mirrored `tight256x129-bicubic-partitioned-current-package.tar.xz` has
14 verified members, 143,648 B and SHA-256
`3614ad2fe356a335d415b07ea5ae257629ed6266c3a3c0aa9c63a1df9e58c951`.
The [current evidence](bicubic-current-package-evidence.json) retains both
confidence views, all subdivisions, runtime hashes and replay.

## Deferred fallback views candidate

The narrow-column prototype moves checked offset and coefficient slices to
the branches that consume them. The common byte pair uses its original compact
coefficient slice. The word pair keeps its ordered weights. The remaining
counts, arithmetic, exact repair, result row loan, 4 KiB working limit and
cancellation checkpoints are unchanged.

| Complete tight RGBA8 256×129 resize | Baseline or Lokad.Image µs | Candidate or zimg µs | Conservative ratio interval | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Three managed-DLL pairs | 448.77 | 435.61 | 0.915574–1.029092 | 112 |
| Twelve managed-DLL pairs | 461.37 | 419.35 | 0.905344–0.912507 | 112 |
| Standalone private DLL / prepared zimg | 395.94 | 418.47 | 0.939953–0.950535 | 112 |

The independent paired reduction is about **42.02 µs / 9.1%**. The standalone
prototype passes its complete parity boundary with the same 448 managed and
128 native operations per unit, the original generated loops, and twelve
units. All original frames, native bounds, ownership checks and quiet guards
pass. Windows passes 701 focused public resize tests.

The private build also passes the bounded RGBA16 control at 516.65 / 618.61 µs,
upper 0.841760 and 113 B/op. The retained private results precede the final
package qualification below.

[Candidate evidence](bicubic-column-view-candidate-evidence.json) retains
the three mirrored, member-verified archives, candidate source and module
hashes, all observations, checks and replay commands.

## Deferred fallback views qualification

| Complete tight 256×129 reduction | Lokad.Image µs | Prepared zimg µs | Conservative upper ratio | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 | 422.56 | 450.23 | 0.947041 | 112 |
| RGBA16 | 512.36 | 618.28 | 0.834755 | 113 |

The Linux Release DLL containing the exact tested source passes both direct
reductions. The byte control uses the established partitioned schedule.
The word control uses the original generated job with natural GC and both
native brackets. Both runs keep twelve actuals, every reference sample and
the original operation, exact pixels, native bounds and ownership gates.
Allocation, scratch admission and retained owner capacity are separate.
The controlled 9.1% reduction above belongs to the private paired run.

Across the 61 frozen direct controls, only four meet the changed method's
geometry bounds. Untimed checks confirm that the two default controls use
the cached path and the two tight controls use bounded columns. Prepared
resize uses the separate cached executor. The other direct and prepared
controls keep their existing scoped qualification.

Both systems pass 6,849 full and live checks and 37 focused tests in each
normal, AVX-512-disabled and scalar mode. The actual dependency-free Release
package `1699cfb1` contains the exact timed Linux DLL `bb3d063d`. Its consumer and all three usage
examples pass on both systems. The package remains a development artifact.
Pre-timing quiet checks that found our idle build workers are retained. After
the workers exited, the original validations and both statistical guards passed.
The [final evidence](bicubic-current-package-evidence.json) pins the
27-member `bicubic-column-view-qualification.tar.xz` and the two timing
archives.

[Subsequent tight 128×64 preparation trials](BICUBIC-WIDE-PREPARATION-TRIALS.md)
retain one adverse short screen and a second candidate with a confirmed paired
gain. The standalone zimg comparison remains uncertain and the candidate stays
private. The current package and its three open cases remain unchanged.

## Shared geometry for the tight mixed-axis word tail

The private `TightCross16` candidate shares the existing exact byte-tail
geometry helper. Once the full-row prefix has returned, it prepares the
45 remaining rows in their unpublished final columns. The first 31 of
32 strips reuse the entries. The last strip rebuilds the original geometry
as it overwrites those columns. This reduces source-level tail geometry
builds from 1,440 to 90 on the frozen control. The 4 KiB workspace, pixel
arithmetic, safe alias lifetime and independently owned output stay intact.

The short screen moves from 881.99 to 873.93 µs. Twelve independent pairs on
the same DLL confirm **825.91 → 814.23 µs**, interval **0.973222–0.998714**,
**112 B/op** on both. Against prepared zimg in the original standalone
256-call generated workload, the candidate measures **830.69 / 823.66 µs**,
interval **0.965544–1.052384**, **113 B/op**. Keep the candidate private.
There is a scoped incremental gain and the strict standalone gate remains
open. The three original package uncertainties remain in force.

Exact RGBA16 pixels, approved native bounds, source, surviving owners and
both quiet guards pass in all three captures. Two preflight corrections
restore the frozen word fixture suffix and 64-operation native bracket.
Both happened before sampling. A single private core build and one actor
build reuse the original inputs, worker and standalone driver.
[Trial evidence](bicubic-cross16-tail-geometry-evidence.json) pins the three
mirrored archives and all 50 members, including commands and raw observations.

A separate trial reused the existing four-pixel vertical filter in the
word full-row prefix. Each quartet shares the weight broadcasts while its
pairs retain their multiply/add order, rounding, repair and publication.
A schedule model checks pair and single tails for widths 1–4,096. Exact
pixels, bounds, ownership and guards pass. The complete three-pair screen is
**891.07 → 902.47 µs** at **112 B/op**, interval **0.919240–1.115862**.
The point result is adverse and the interval is inconclusive.
The candidate stops after one private build with the original actor.
[Trial evidence](bicubic-cross16-prefix-quartet-evidence.json)

## Single-round word publication trial

The trial clamps each finite quotient to the integer output endpoints, rounds
once, and uses that result in both the half-step certificate and publication.
The clipped endpoints are half a sample from an ambiguity, beyond the existing
quarter-sample margin guard. Near a half-step, the distance computed from the
nearest integer equals that computed from the floor. The original word
quotients, margins and exact repair remain. This targets the enlarged-height
full-row prefix. The strip tail and identity-height route keep the current
word publisher.

| Complete TightCross16 operation | Baseline or zimg µs | Candidate µs | Ratio interval | GC B/op, candidate |
| --- | ---: | ---: | ---: | ---: |
| Private build, three managed pairs | 889.79 | 873.73 | 0.913745–1.055267 | 112 |
| Private build, twelve independent managed pairs | 822.91 | 808.28 | 0.979465–0.984986 | 112 |
| Private build, standalone versus prepared zimg | 829.10 | 811.04 | 0.946155–0.995390 | 113 |
| Scoped Release build, standalone versus prepared zimg | 891.96 | 883.71 | 0.972930–1.008999 | 114 |
| Scoped Release build, twelve managed pairs | 890.11 | 885.50 | 0.983994–1.005709 | 112 |

The private build confirms a **1.78%** paired gain. The scoped build leaves the
byte publisher's source intact. Its standalone comparison remains uncertain
and its matched pairs leave the gain unconfirmed. Absolute clocks move on both
sides. These results establish no cause for the transfer difference. Keep the
qualified package and TightCross16 costs.

The first scoped-build collector expected 256 operations per actual. The
scoped build completed twelve actuals of 240 under the driver's adaptive
calibration. The guard records and managed log survive, but the temporary
native receipt was retired before export. The single corrected capture keeps
the original generated delegates and accepts their consistent actual count.
All retained scored runs pass exact managed pixels, native bounds, source,
ownership and quiet-VM guards.

The private variant agrees with the current certificate on **3,145,776**
inputs spanning word half-steps, boundary margins, clipped endpoints and
accepted/refused writes on the AVX-512 VM. Windows and Linux each pass 32
focused tests, including the named storage suites. Two private core builds
reuse the same actor, worker, fixtures and complete operation.

[Trial evidence](bicubic-cross16-clipped-rounding-evidence.json) retains both
producer identities, all archived runs, the lost-receipt diagnosis and
reproducible focused checks.
