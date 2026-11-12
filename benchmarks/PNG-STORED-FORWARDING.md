# Stored PNG output forwarding

Complete 4 KiB stored payloads now pass synchronously from admitted history to
the IDAT writer. The partial prefix and final tail still use the original output
buffer. This removes an intervening copy while preserving the exact packet/chunk
boundaries and existing history, fields, admission and output ownership. On writer
or pre-write cancellation failure, the helper restores the original pending
buffer before rethrowing; post-write cancellation sees the original empty state.

Independent retained-native RGBA16 confirmation improves **426.33→417.77 µs**:
**8.55 µs / 2.01% filtered raw**, with separated confidence. All 12 actuals give
426.34→418.65 µs (7.70 µs / 1.81%), also separated. Native time moves −0.36%.
The normalized 1.65% point has overlapping confidence: no normalized or general
PNG gain is claimed. This control remains a **gap**, range1.003740–1.051470.

| Complete stored encode | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| RGBA8, retained native context, post-GC4 | 58.43 / 189.61 | 0.308 / 0.350 | 609,708 | pass |
| RGBA16, retained native context, post-GC4 | 417.77 / 404.34 | 1.033 / 1.051 | 1,147,452 | gap |
| RGBA8, equivalent fresh contexts | 58.64 / 537.90 | 0.109 / 0.127 | 609,708 | pass |
| RGBA16, equivalent fresh contexts | 427.83 / 1,380.52 | 0.310 / 0.319 | 1,147,452 | pass |
| RGBA16, retained native context, natural-GC64 | 275.24 / 406.09 | 0.678 / 0.708 | 1,147,393.875 direct | pass |

Each final capture retains 12 actuals and 12 native samples per bracket; all
originals/outliers remain. One CPU0, warm optimized JIT, nonconcurrent workstation
GC and idle-v2 quiet guards pass. Managed complete EncodePng includes fresh
MemoryStream capacity/growth and output disposal, without ToArray. Retained-native
codec/frame/packet wrappers remain prepared; word endian conversion is timed.
Fresh-native views include codec/zlib/frame/conversion/output setup and disposal.
Natural-GC64 and post-GC4 are separate conditioning scopes, not additive phases.

The frozen original513×259 RGBA samples, stored0/filter None settings, native
identities, full-depth/hidden-RGB/alpha pixels and independent output gates are
unchanged. Complete packets remain exactly533,444/1,066,552B. Word packet SHA256:
`9b31604447d61d5ab7a30aad3cfde4cd73fef6d85ff506e74baeba807e54574d`.
Allocations are unchanged per instrumentation boundary; there is no new owned
capacity or retention. Stored-deflater admission remains72KiB, operation scratch
retention zero; output capacities and prompt cleanup remain. Physical/native/JIT/
pool peaks are unmeasured. No allocation or physical-peak saving is inferred.

The first layout's short screen428.98/native405.09µs versus contemporary
434.80/404.16µs has wide confidence. Its enlarged710B StoredBlock motivated a
coherent revision: isolate scheduling in a private noninlined240B helper, retaining
the original455B header/state body and217B borrowed-write helper. Code size is a
diagnostic, not a cause proof. No independent A gain/regression is claimed; its
source/modules/all evidence remain. The [earlier direct-stream trial](PNG-STORED-DIRECT-STREAM.md)
removed history and added format state; this design retains all existing storage.

Eight common generated bodies match instructions/labels/code size after only
absolute-address/method-number normalization: constructor582B, Write558B,
WriteStored395B, stored-Adler651B, generic OutputBytes244B, Flush171B, Complete656B,
shared Adler548B. Original source reconstructs outside StoredBlock/new helpers;
compressed emission never calls StoredBlock. Reuse15compressed encode/51PNG read
costs and unrelated codec/resize/text paths. Current fresh PNG scope stays
16passes/one Gray8uncertainty; this is incremental qualification, not a release refresh.

Four new authored tests cover second/ninth chunk failure and cancellation with an
independent RFC1950/1951 prefix oracle, pending Flush bytes, same error/token,
admission through writer callbacks, nonwriting cleanup and subsequent operation
recovery. Both designs pass709internal/246public focused checks in normal/scalar/
AVX512-off/AVX2-off on both OSes. After all affected costs reconcile, selected code
passes **6517full/live checks per OS**, then the **SAME actual package consumer and
all three examples** on Windows/Linux. The existing19-wide stored consumer's
height13→131 makes both depths exercise forwarding, sync/async output, exact pixels
and independent owners. One core/embedded build per design/OS, zero public/harness/
native/fixture builds, one full milestone, one pack/consumer per OS.

The renderer now selects one current qualification instead of repeatedly replacing
it with20historical aliases. Alias-only whole-module AST equivalence, every
assertion and identical generated Markdown are verified separately from the new
cost update. Historical evidence remains; no framework, schema or production
qualification campaign is added for that maintenance change.

Actual0.2.1 development package456,416B, SHA256
`ef218829ea19644bb5badb3e39d064d81defc87b96a147b0d3c1184108bcc82e`;
Windows/timed DLL `e4927a8eacfbc26f4d18527da3e46d2e13e5baef43de58c6b9117d5386c11a38`,
Linux qualification DLL `9a4ee933cefd44e1a6cb7d8cb3e5326536c53c89b6ce8b14d12a456010e05fb3`.
Published packages remain unchanged; publication is postponed.

[Evidence](png-stored-forwarding-evidence.json) pins both designs, native workers,
all exact packets, actuals, source/owner/quiet gates and package proof. Raw
`png-stored-forwarding-qualified.tar.xz`, SHA256
`0cbf99026e7bf67e311d197d7ada66714f972f8bb5d4311af8eae8dc5ffa7165`,
2,383,464B/214members, is mirrored/member-verified. Dependencies retain the
[checksum predecessor](PNG-STORED-CHECKSUM.md), former direct-stream trial,
unchanged confidence controls and frozen harnesses. Stop passing cases; retain
the scoped word gap and uncertainties across the complete289-path objective.
