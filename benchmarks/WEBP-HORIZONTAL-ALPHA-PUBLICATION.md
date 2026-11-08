# Fused horizontal WebP alpha publication

WAF542 combines final VP8L green extraction with horizontal alpha inversion.
This removes a second frame walk and generic per-pixel coordinate/filter
dispatch. Four-sample SSE2 byte prefixes and a portable scalar tail preserve
exact modulo-256 results, row borders and prepared RGBA's zero RGB lanes.
Palette-backed compact alpha uses the same phase with independent destination
storage. Raw alpha and other filters retain their existing paths.

Independent complete resident decoding improves **6,116.01 → 5,919.97 µs**, saving
**196.04 µs / 3.21% raw / 3.25% reference-normalized**. Filtered/all-actual raw and
normalized intervals separate; native movement is +0.048%. Older 6,237.83 µs
baseline evidence is retained but does not supply this causal gain. The faster
contemporary unchanged baseline is also a gap; that historical difference remains
unexplained.

| Complete existing decode / FFmpeg | Lokad µs | Reference µs | Ratio / upper | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Resident Native8 | 5,919.97 | 5,962.52 | 0.993 / 1.002 | 238,384 | Uncertain |
| Fresh seekable Stream | 5,939.22 | 6,011.78 | 0.988 / 0.992 | 238,480 | Pass/stop |

Each cost has twelve actuals and both native brackets. Resident parity remains
uncertain; no favorable repeat follows. The stream row is current standalone
parity evidence, without a causal historical stream-gain claim. One displayed
row refreshes; 124 reuse unchanged paths. Together with four unchanged VP8
model/validation controls, the current scope has two passes, three gaps and one
uncertain case. This is not full-library parity or a release-wide refresh.

The frozen 98,590-byte input has a 24,588-byte ALPH chunk with compression 1,
horizontal filter 1 and no preprocessing. Exact 531,468-byte RGBA output, source,
native/settings and independent-owner gates remain. Complete parse/preparation,
inverse transforms, alpha/color publication, output ownership and disposal stay
timed; native context retention remains declared. A preceding two-second capture
finds 72 of 1,988 samples in alpha inversion. Samples and instrumented elapsed do
not establish phase costs; only the complete confirmations establish the gain.

No buffer, owner, pool, cache, session, dependency or public API is added. Existing
allocation/admission capacities and cleanup statements remain. Output stays
charged for its 531,468-byte ownership lifetime; no scratch survives the operation.
GC is unchanged. Physical/admitted peaks and native allocations are not newly
measured or inferred. Two bounded cancellation checks per row now surround the
fused private phase; no partial public result escapes and cleanup remains intact.

Windows/Linux each pass **5,877 full/live checks**, plus **25 internal / 402 public
checks per normal/scalar mode**. Two new facts cover 66 width/border/wrap/tail/
destination controls against an independent byte recurrence and precancellation.
Existing palette tests extend actual compressed-horizontal integration. One
Windows cancellation-test setup initially canceled before context construction;
its failure/source/module are retained. Correct only the test and rebuild the
embedded assembly; production is reused unchanged. Each OS also builds the
changed public test assembly once; no harness/native/selector/fixture rebuild.

The **same actual Release package/all three examples** passes both OSes, including
an assertion that the package's alpha control selects compression 1/filter 1.
Private 0.2.1 package `5a7cd5db…`, 428,013 bytes, is in `artifacts/nugets/`.
Windows/Linux producers are `e6bc25c6…` / `589a9034…`. Published releases remain
unchanged; publication is postponed. Unchanged coefficients, opaque decoding,
coding validation, standalone VP8L, encoders and other families reuse their evidence.

[Evidence](webp-horizontal-alpha-publication-evidence.json) pins source projections,
byte proof, original/current actuals, guards, allocation, package and qualification.
Raw `webp-horizontal-alpha-publication-qualified.tar.xz`, SHA256
`cfbfd93f72c02d9094452726314d8268d493719efe2d281e276ba1834516b9fe`,
15,884,220 bytes / 261 members, is mirrored/member-verified. It retains the current
profile, original/candidate modules/sources, test correction, frozen input,
actual package and reproducible tooling with its qualified provider. The full
283-path objective remains open.
