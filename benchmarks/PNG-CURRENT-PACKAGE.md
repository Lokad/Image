# Current-package PNG cost

The original resident RGBA8 PNG decode passes on the unchanged actual 0.2.1
package. This is one required path outside the 165 displayed comparisons.

| Control | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Original 513×259 RGBA8 PNG, resident decode | 1,192.53 | 1,396.53 | 0.854 / 0.862908 | 4,472 | Pass; stop |

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
The other 46 decoder costs remain historical after the shared-storage change;
no passing kernel is retuned. The full 289-path objective stays open.

[Evidence](png-current-package-cost-evidence.json) retains the measurement,
source reconciliation, all gates and immutable identities. Mirrored/member-verified
`png-current-package-cost.tar.xz`: **887,060 B / 48 members**, SHA256
`6aead4ef3b42d12f858306cd6dba16f97b5a3570efca09e7ca2e2c54c26e1245`.
Archived `frp1272-*` recipes reuse the frozen runtime and reference dependencies.
A missing producer receipt stopped preparation before any benchmark; only that
receipt was supplied, with the failure retained and no capture restarted.
