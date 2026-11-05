# JPEG scan header preparation

Compile a small canonical Huffman/sign/EOB program once per eligible progressive
AC scan and reuse it across blocks. One scan executor also prepares row geometry,
removing repeated generic component/MCU traversal. Ordinary and live-token
correction kernels keep their arithmetic and failure/cancellation behavior.
Long codes, invalid symbols, escaped bytes, markers and scalar execution retain
established paths. All preparation stays inside each timed complete operation.

Independent ordinary/live progressive444 comparisons save **229.58/230.38µs**,
**4.84%/4.57% raw**. Both filtered/all-actual raw and normalized confidence
separate. Ordinary goes 4,745.49→4,515.91µs; live
5,035.77→4,805.39µs. Normalized gains are
5.04%/3.68%; the faster native brackets move
+0.22%/-0.93%. This attributes the combined preparation/geometry
stage, without separate phase-time or machine-instruction savings claims.

## Current scope

Six existing frozen controls, dedicated quiet VM CPU0, warm sequential Release,
normal SIMD, tiering off and nonconcurrent workstation GC. Twelve actuals each;
all raw/filtered results and native brackets retained. Decode includes public
admission, normalization, independently owned RGBA8 output and disposal. Native
retains its documented input/decoder/conversion/wrapper scratch, resets decoder
state per operation and decodes/converts every packet. Live-token mode is managed
only. Coding validation returns source metadata with complete coded-pixel
validation and cleanup; it constructs no RGBA output.

| Existing control | Lokad µs | FFmpeg µs | Ratio / upper | Lokad B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| progressive grayscale, resident decode | 2,289.86 | 1,851.17 | 1.237 / 1.245 | 683,126 | gap |
| progressive420, resident decode | 2,142.28 | 2,115.00 | 1.013 / 1.022 | 775,318 | gap |
| progressive422, resident decode | 2,743.54 | 2,424.01 | 1.132 / 1.148 | 847,116 | gap |
| ordinary progressive444, resident decode | 4,515.91 | 3,660.11 | 1.234 / 1.243 | 984,212 | gap |
| progressive444, live-token resident decode | 4,805.39 | 3,616.72 | 1.329 / 1.340 | 984,212 | gap |
| Validate JPEG progressive420, span | 287.38 | 291.59 | 0.986 / 0.992 | 4,072 | pass |

All five decode controls keep every frozen native sample, source and output
ownership. Their input packets/settings are unchanged; the 444 input is 132,961B
and output 531,468B. Coding validation retains its exact 36B descriptor and complete
boundary. Five CPU gaps remain; the passing validation control stops. The three
other decode refreshes establish current costs and correctness generalization,
without independently attributed broader speedup claims. JPEG encoding, complete
encoder packets and metadata exceptions are unchanged.

## Storage and qualification

The plan uses **2,048 admitted stack bytes per eligible scan**, released in
`finally` after its last actual user. It adds no heap array, cross-operation
cache/session or retained storage. Allocation is unchanged, including 984,212B/op
on both 444 controls. Whole-operation physical/admitted peak and native allocations
are not newly measured; added scan capacity is distinct from total allocation.

Preparation requires at least 32blocks to amortize 512 entries. Small scans use
direct parsing and retain their exact admission bounds. Geometry selects the path
before reserving storage, independently of available capacity. Refusal terminates
without fallback/retry/waiting. An authored 32 block authored control verifies refusal,
cancellation cleanup, retained result ownership and successful root reuse.

Windows/Linux qualify **5,711 full/live checks** (2,474 internal, 2,289 public,
948 differential) and **373 internal/333 public JPEG checks per normal/scalar
mode**. Independent traversal covers every nine-bit prefix, bit phases and partial
failures for ordinary/live preparation. The concurrent-prefix oracle also covers
both direct/prepared headers; all 65,536 correction fields remain covered. Only
internal test assemblies rebuild for that strengthening; production DLLs stay
immutable, reusing the Windows full run and supplementing it with both focused
facts normally/scalar. Linux full qualification includes the strengthened fact.

The **same actual Release 0.2.1 package**, `3874820ab5c095e8…` (411,981B), and
all three usage examples pass both OSes. Its actual consumer checks both the existing
direct control and a 32 block prepared control against every frozen original PGA335
sample, with async delivery, input mutation and independently retained outputs.
The frozen oracle reuses original library/fixture assemblies without a tool build.
Published releases remain immutable; publication is postponed.

## Retained experiments and reproducibility

Stage A qualifies a useful live-only lead. Reference time moves about 8.7% between
its short and confirmation; the adverse raw comparison to the older original is
retained. One fresh immutable original resolves the missing contemporary
comparison; no candidate repeats follow solely for favorable samples. Stage C
reuses that valid original for live mode and measures one contemporary original
for ordinary decoding, then independently confirms both completed kernels.

Stage B shares preparation with ordinary execution but fails 15 existing tiny-image
admission cases. It receives no timing run. Stage C's geometry decision resolves
that functional failure before measurement. All failed source/modules/logs remain.
One validation setup stops before timing on an old exact producer pin: frozen
expected descriptors are reused with explicit original qualifier/current selector
identity; existing `ValidateManaged` checks the actual current operation before
timing. Payload/settings/gates stay unchanged and metadata restores byte for byte.
This supplies no broader parser-equivalence claim. A Linux package preflight
catches an unsynchronized packaged document; syncing that exact document resumes
the unchanged package without repacking or rebuilding the library.

One final production version per OS is reused for confirmations/qualification;
Linux retains its two earlier experimental versions. No harness/native/selector
or fixture-tool build. One package, twelve successful timing jobs. The retained
[scan-state](JPEG-SCAN-REFINEMENT-INCONCLUSIVE.md),
[correction-execution](JPEG-BUFFERED-CORRECTION-EXECUTION-INCONCLUSIVE.md) and
[instruction attribution](JPEG-REFINEMENT-STATE-ATTRIBUTION.md) remain source-level
leads/history, without a new profile or permanent family ban.

[Numeric evidence](jpeg-prepared-scan-headers-evidence.json) pins sources,
producers, confidence, all boundaries/gates, qualification, failures and package.
Mirrored/member-verified raw `jpeg-prepared-scan-headers-qualified.tar.xz`:
13,746,996B/901members, SHA256
`b3641e93dba174a591b71fd001f3d936aca4e8d79281655d35fc5885cc5983ab`. Original/selected source/modules/tests/package, workers,
selectors, all observations and reproducible scripts are retained; unchanged
reference-source/fixture provenance stays in pinned dependencies. Six displayed
comparisons refresh; other 119 reuse unchanged-path evidence. The full 282-path
objective remains open.
