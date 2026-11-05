# Lanczos horizontal publication investigation

Two stages remove the separate exact-Int64-row reread/conversion/rewrite pass.
Shared typed byte/word kernels retain every exact product/sum and change only
publication. The first converts four samples per store; the revision keeps one
private 32-byte pending pixel and converts two together at the original 512-bit
width. Both preserve conversion bits, certified rounding/repair and ownership.
Neither implementation is retained; production restores byte-exactly.

One **counterbalanced comparison confirms a 195.89 µs / 5.07% regression** for the
first stage on complete prepared RGBA16 enlargement: 3,864.81→4,060.69 µs. Twelve
alternating pairs × sixteen operations per subject, all retained; ratio confidence
**1.04491–1.05650**. Both subjects allocate 4,264,240 B/op. Exact pixels, independent
owners, source/loaded-module identities and quiet guards pass. Setup/JIT/validation
are excluded; source reads, row reset, independent output allocation/disposal and
the complete public prepared resize remain timed. This is an incremental managed
comparison, not new zimg parity evidence.

| Existing prepared control, three-actual screens | First stage µs / zimg µs | Revised stage µs / zimg µs | Revised ratio / upper | Allocated B/op | Retained plan B |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 513×259→1027×519 | 3,993.13 / 3,894.40 | 4,069.66 / 3,904.88 | 1.042 / 1.168 | 4,265,332 | 333,396 |
| RGBA8 513×259→256×129 | 553.77 / 493.06 | 569.54 / 491.55 | 1.159 / 1.533 | 139 | 139,800 |

The first short's apparent word raw gain accompanies material native clock
movement; the paired result resolves that uncertainty adversely. The revised
stage's shorts support no useful lead, so no second independent comparison or
width/inlining campaign follows. All actuals and native brackets remain. Shorts
do not establish causal gains/regressions or replace current qualified costs.

Actual production FullOpts listings confirm closed typed stores and no runtime
type dispatch. First-stage horizontal byte/word bodies are 681/609 B; revised
bodies are 798/727 B with 512-bit pair conversion and 256-bit tails. This verifies
the intended implementation change, not its cost allocation. No new heap owner,
pool, cache or admitted capacity. The revision's small register/stack value fits
existing coarse allowance; physical/admitted peaks are unmeasured. Standalone and
paired enlargement allocations use different batching; no saving is inferred.
The zimg float reference never relaxes exact managed Q30/rational results.

Each Linux candidate passes 78 internal/15 public Lanczos checks normal/scalar.
An initial cancellation test canceled before context construction; fixing its
setup needs one internal-only rebuild, with unchanged production. A relative JIT
output path is corrected for testhost's working directory. Failed logs and both
code epochs remain. No completed timing is repeated.

Two useful tests remain against the **restored actual implementation**, qualified
normal/scalar on Windows/Linux. Independent BigInteger rows cover clamped/long/
odd/even geometry, zero/one/full alpha, signed exact sums and non-exact Int64-to-
double rounding. Canaries, source immutability, zero allocation, retained charges,
cleanup and early cancellation stay checked. Integer fallback executes scalar;
floating conversion executes only under the real fast-path hardware guard.
No obsolete implementation or forwarding API is retained for tests.

Production, consumers and the actual package restore to the qualified
[luma4 checkpoint](VP8-LUMA4-PREPARATION.md). Its 5,639 full/live checks per OS and
same-package/all-three-example proof remain valid, plus the two separately
qualified additions: **5,641 applicable checks**, not a repeated full-suite run.
Two Linux core/four internal builds, one Windows internal-only build and one
private paired-driver build; no benchmark/native/selector/package rebuild.
All 125 current comparison rows and the full 282-path objective remain unchanged.
Publication is postponed.

[Evidence](lanczos-row-publication-evidence.json) pins every source/producer epoch,
actual, JIT listing, restored module and retained test. Reproduce with archived
`lrp304*.py`, the parameterized private paired driver and original prepared resize
harness/workers/fixtures on the quiet single-CPU VM. The revised publication is
a concrete implementation change, not unchanged resampling.

Raw `lanczos-row-publication-investigation.tar.xz`: 12,009,872 B / 274 members,
SHA256 `0517bac13b2037f9efbb9318fd44ac2a8bd6906706961b1e15d69241b8f4f8e3`, mirrored and every member verified locally/VM.
Dependencies preserve qualified source/package, frozen controls and framework.
