# Current-package PNG cost

On the actual 0.2.1 package, 46 of 47 original resident PNG decoder controls pass.
The tight working limit RGBA16 control remains open.

| Control | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Original 513×259 RGBA8 PNG, resident decode | 1,192.53 | 1,396.53 | 0.854 / 0.862908 | 4,472 | Pass; stop |
| Original RGBA8, live token | 1,334.01 | 1,395.37 | 0.956 / 0.962890 | 4,472 | Pass |
| Original RGBA8, tight working limit | 1,240.93 | 1,505.84 | 0.824 / 0.829439 | 68,224 | Pass |
| Original RGBA16, tight working limit | 2,958.97 | 2,900.14 | 1.020 / 1.058694 | 70,272 | Uncertain |

The resumed batch captured 46 controls sequentially using one runtime and the
same generated loops as the first resident RGBA8 confirmation. All 552 managed
actuals and both native brackets per control are retained. In the open case,
the managed batches move from 2.786–2.793 ms for the first two to 2.932–3.031 ms
for the remaining ten. Native bracket means move from 2.900 to 3.118 ms.
The conservative ratio interval is 0.905898–1.058694. Interleaved native sampling
can test the effect of measuring the two sides at different times.

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
