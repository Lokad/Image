# Current predictable and stored PNG costs

PHC832 refreshes three historical controls on the **actual verified package DLL**,
without production changes. PredictableRGBA16 and storedRGBA8 pass/stop.
StoredRGBA16 remains **sampling-sensitive**: retain both current views.

| Complete existing encode / sampling | Lokad / FFmpeg µs | Ratio / conservative upper | GC B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: |
| Predictable3000² RGBA16,4op actuals | 276,996.48 / 407,420.77 | 0.680 / 0.706 | 2,495,988 | 573,041 |
| Stored513×259 RGBA8,4op actuals | 94.11 / 190.82 | 0.493 / 0.674 | 810,524 | 533,444 |
| Stored513×259 RGBA16,post-GC4op actuals | 507.69 / 405.03 | 1.253 / 1.343 | 1,348,268 | 1,066,552 |
| Same RGBA16,natural-GC64op adjacent pairs | 351.54 / 405.42 | 0.867 / 0.902 | 1,348,209.88 direct | 1,066,552 |

Three independent twelve-actual global-bracket confirmations preserve original
selectors and inputs/settings. The matched word view has twelve alternating pairs
of64complete operations per subject and twelve native batches before/after. All
actuals/pairs/brackets are retained; bounds include all-actual/filtered and, where
applicable, paired log-ratio confidence. No causal gain is claimed from older costs.

The pinned selector fixes **four managed operations per actual** for every case,
while native stored-word samples use128operations. Those managed word actuals last
about2ms. Inspection of the actual selector finds no GC-force override and its
actual BenchmarkDotNet0.15.6 resolver returns true. The versioned
[engine](https://github.com/dotnet/BenchmarkDotNet/blob/v0.15.6/src/BenchmarkDotNet/Engines/Engine.cs)
performs GC cleanup outside iteration clocks; [GC configuration](https://github.com/dotnet/BenchmarkDotNet/blob/v0.15.6/src/BenchmarkDotNet/Jobs/GcMode.cs)
documents that default. The owned matched driver uses natural GC. Batching,
conditioning and adjacency differ; do not attribute the entire156µs difference
to GC alone or select the favorable view as general warm parity. The64op view
passes its precise repeated-use scope; broader stored-word confirmation remains
open. Do not presume a production kernel gap from the post-GC diagnostic.

Complete public EncodePng includes packing/prediction/Deflate, fresh MemoryStream
creation/growth, independent output and disposal, with no final snapshot copy.
The original FFmpegn8.1 references retain codec/frame/wrappers and time independent
packet acquisition/encode/release; word endian conversion remains timed. This
retained-state boundary is distinct from the [fresh-context controls](PNG-LENGTH-CLASSIFICATION.md).
Frozen full packets, every sample/alpha/hiddenRGB, cancellation/source/owner checks,
CPU0/one thread and idle-v2 guards pass. No image, quality, shape or settings grid.

Allocations are total per encode; the small direct/BDN word difference is
instrumentation, not a storage saving. Operation-owned scratch is released.
Borrowed source payloads retained by the harness are72,000,000/531,468/1,062,936B;
validation packets and native prepared state are separate. Governed/physical/
native peaks and default pool retention are not remeasured. Allocation totals
do not establish peak live capacity or retained storage.

The immutable private0.2.1 package and its[6,417full/live checks perOS, scalar/ISA,
SAMEactualpackage/all3examples](PACKED-HORIZONTAL-WINDOW-REUSE.md) are reused.
No production/embedded/public/native/fixture build or broad/package repeat. One
owned-driver build adds WarmPng to the existing allowlist only. An initial
reference-source identity failure stops before any actual; matching ignored
source/worker views restore provenance without weakening checks or rebuilding.
Global reference tools remain unchanged.

The next independent item is the existing historical Tight32x16_8 bicubic control;
measure its actual-package boundary before assuming its old gap remains.
[Evidence](png-historical-current-confirmation-evidence.json) records producers,
receipts, GC inspection and the mirrored/member-verified raw archive
`png-historical-current-confirmation.tar.xz` — SHA256`30179fa480b53a2822e535b2872f95e5ba261e8c29ac75af1777f8dfc37ebc04`
(252,776B/86members). Dependencies retain native workers,
frozen harnesses/fixtures, the actual package and qualification. All289paths remain;
publication is postponed.
