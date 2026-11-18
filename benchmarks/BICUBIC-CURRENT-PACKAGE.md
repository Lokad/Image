# Bicubic storage on the current package

Four original bicubic cases use the changed shared idle storage. The tight
RGBA8 and default horizontal RGBA16 enlargements pass on the actual 0.2.1
package. Both RGBA8 reductions remain uncertain.

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

Thirteen of fourteen additional cases pass on the same actual package. The tight 64×32
RGBA8 reduction remains uncertain.

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

The 16×8 word case retains its original twelve alternating 256-call pairs
and twelve native samples in each outer bracket. The earlier uncertain
capture and the later streaming-column pass remain in the evidence.
The new result confirms the current package. The compact-weight change
does not execute on this shape. [Streaming-column results](STREAMING-COLUMN-REUSE.md)

The other thirteen cases use two sequential batches with the same 127 verified
non-core runtime files and original generated delegates. Each retains three warmups, twelve measured
actuals with natural GC and twelve samples per native bracket. The conservative
interval includes both the filtered and all-actual views. The 64×32 byte
interval spans parity. Its point difference is 0.30 µs.

Every case preserves the original input and output hashes, exact managed
pixels, native bounds, source and independent output checks. The reference
retains zimg preparation. Both idle guards pass with untimed files in memory.
No new build or production change was needed. Allocations are reported
separately from owned capacity and idle retention. Five existing summary
rows use the refreshed measurements.

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
