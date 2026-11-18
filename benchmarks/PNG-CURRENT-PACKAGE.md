# Current-package PNG cost

All 47 original resident PNG decoder controls pass on the actual 0.2.1 package.

| Control | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Original 513×259 RGBA8 PNG, resident decode | 1,192.53 | 1,396.53 | 0.854 / 0.862908 | 4,472 | Pass; stop |
| Original RGBA8, live token | 1,334.01 | 1,395.37 | 0.956 / 0.962890 | 4,472 | Pass |
| Original RGBA8, tight working limit | 1,240.93 | 1,505.84 | 0.824 / 0.829439 | 68,224 | Pass |
| Original RGBA16, tight working limit | 2,796.05 | 2,904.45 | 0.963 / 0.979774 | 70,293 | Pass |

The resumed batch captured 46 controls sequentially using one runtime and the
same generated loops as the first resident RGBA8 confirmation. All 552 managed
actuals and both native brackets per control are retained. In the open case,
the managed batches move from 2.786–2.793 ms for the first two to 2.932–3.031 ms
for the remaining ten. Native bracket means move from 2.900 to 3.118 ms.
The first conservative ratio interval is 0.905898–1.058694.

The independent confirmation places a 32-operation FFmpeg sample before and
after every managed actual, outside the managed clock. Original setup and
cleanup brackets remain, as do managed warmups, natural GC and generated loops.
The adjacent bracket means are 2.904452 and 2.904544 ms. The conservative
0.935299–0.979774 interval includes the two paired log-ratio intervals and the
filtered and all-actual confidence bounds against all four native brackets.
The case passes and stops. The first capture remains available, and the cause
of its drift remains unresolved. Both runs pass the exact working limit, pixel,
cancellation, ownership and quietness gates.

The mirrored confirmation archive `png-tight-word-interleaved-confirmation.tar.xz`
contains 15 verified members and 28,588 B, SHA-256
`66d796a6240d66e526e7a442c9c7f1e8197ccb3ade26e40dc41e6f27f5d323c1`.
It retains all actuals, native observations and the single temporary driver.

The scored iterations use natural GC. BenchmarkDotNet reports allocation in a
separate diagnostic pass after the actuals, with its own forced collection.
The VM source guard initially rejected the run because a prior archive step
had overwritten the pinned log parser source. Restoring the exact manifest
version resolved the rejection before timing began. The VM parser then failed
to load a dependency after the first capture. The local parser scored that
capture and the rest of the batch from their retained logs.

The batch evidence includes every case and the 26 verified input origins for the
other original PNG controls. All cases retain their individual public boundaries.
[Batch evidence](png-decoder-current-package-evidence.json) and the mirrored,
member-verified `png-decoder-current-package-confirmations.tar.xz` preserve
commands, actuals, validation and the failed setup and parser logs. The archive
contains 395 members and 423,200 B, SHA-256
`6e0bb4e060760f1c42b870b6e9f984623ad50dc195266fc6296aed4ac01464c3`.

The original generated BenchmarkDotNet delegates, consumer, unroll/remainder
loops, setup/cleanup, three warmups, twelve actuals and 200 ms target are reused.
An explicit job disables forced collections between iterations; normal collections
inside operations remain timed. The historical generated job's display label
`WarmShort` becomes `WarmConfirm`; its iteration counts already described a
confirmation. Every actual is retained. The conservative bound includes the
eleven selected results and all twelve unfiltered actuals against both native
brackets. No production gain or regression is inferred from historical clocks.

Both sides process identical frozen encoded input and acquire/release independent
output on one CPU. FFmpeg retains its declared codec/input/frame wrappers.
Exact pixels, alpha, hidden RGB, dimensions, source integrity, owners, native
identity and both idle-v2 guards pass. The actual package DLL is `7b59eabe`,
package `f4c5b5db`; neither is rebuilt. Prior 6,844 full/live checks per OS,
the same actual-package consumer and three usage examples remain applicable.
One temporary driver build; no production/harness/reference/fixture/package builds.

GC allocation is separate from live capacity and retention. Their actual peaks
are not remeasured. Existing shared image/font storage retains at most four
arrays under its 4 MiB ceiling; private PNG workspace pools keep two-slot policies.
Physical/native/JIT peaks remain unmeasured. Admission and ownership contracts
are unchanged.

The original renderer reproduces all 240 numeric rows from 75 pinned Git data
files. All 47 original PNG decoder controls match later records by case, encoded
input hash, output hash, dimensions and depth. This establishes scope provenance.
The batch refresh above supplies the current costs after the shared-storage
change. The full 289-path objective stays open.

[Evidence](png-current-package-cost-evidence.json) retains the measurement,
source reconciliation, all gates and immutable identities. Mirrored/member-verified
`png-current-package-cost.tar.xz`: **887,060 B / 48 members**, SHA256
`6aead4ef3b42d12f858306cd6dba16f97b5a3570efca09e7ca2e2c54c26e1245`.
Archived `frp1272-*` recipes reuse the frozen runtime and reference dependencies.
A missing producer receipt stopped preparation before any benchmark; only that
receipt was supplied, with the failure retained and no capture restarted.

Fresh seekable RGBA16 PNG decode also passes on the same actual package at
**2,876.67 / FFmpeg 3,104.71 µs**, upper **0.991093**, **8,664 B/op**. The
existing adjacent driver retains twelve 64-operation pairs and both native
brackets. Both sides acquire the full packet and release independently owned
output. Fresh stream creation, read and disposal remain timed, and all exact
pixel, source, ownership and quietness checks pass. The archived nine members
retain all observations and commands. The mirrored
`png-word-stream-current-package.tar.xz` is 48,200 B, SHA-256
`091e3bda93ffd8abeace20de69d33c98bb1762f90d4a7c909e113f18354614cf`.

## Current package storage controls

| Control | Lokad.Image µs | FFmpeg µs | Conservative interval | Managed GC B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 async nonseekable, one-shot Optimal6 encode | 17,544.42 | 18,428.84 | 0.931125–0.978328 | 877,476 |
| PNG8 resident decode, two retained outputs | 2,652.54 | 2,849.73 | 0.817536–0.972152 | 1,638,214 |
| RGBA8 fresh async nonseekable decode | 1,166.55 | 1,419.42 | 0.808696–0.833486 | 4,602 |
| RGBA8 fresh seekable decode | 1,195.40 | 1,397.69 | 0.796255–0.874818 | 4,578 |

All four pass the conservative upper-ratio gate on the same package and stop.
Each capture retains twelve managed actuals and both twelve-sample reference
brackets. The original generated loops, warmups, cleanup and natural GC remain.
The async encoder retains four operations per batch and produces exact
210,973-byte packets on both sides. Both streams include fresh input delivery
through the original complete decode boundaries. The async input proof rechecks
all 21 original controls, including exact pixels for its two accepted fixtures.

The caller-budget operation retains the first owned result through the second
complete decode. The shared root admitted peak was 1,766,641 B, including
218,775 B of caller input. This is separate from GC allocation and gives no
physical peak or idle retention measurement. Cancellation, refusal, resource
cleanup and independent output checks all pass. The collector initially searched under the generated PNG class name. The receipt
uses the memory-budget benchmark name.
The completed capture was recovered from its original receipt without retiming.

The mirrored `png-storage-current-package-confirmations.tar.xz` has 160 verified members and 65,432 B,
SHA-256 `cb6d7706ccb396127a8e478225c4b7ba9f08e91fba01f157e3e59722febe7880`. It includes capture and collection
recipes, all original validation records and quietness guards. The temporary
async proof was removed and the original bytes restored. No production,
package, reference, fixture or generated benchmark rebuild was needed.
[Evidence](png-decoder-current-package-evidence.json)
