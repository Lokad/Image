# JPEG stuffed-pair acquisition

Complete header inspection now keeps entropy `FF00` pairs inside one marker
acquisition instead of returning each pair through the async builder and outer
grammar. Available suffix bytes use the existing buffer directly; exhausted
buffers still await the original reader. Controlled complete-operation pairs save
**31.743% / 1.754 µs**, with unchanged allocation. This accepts a measured
**2.698 ns / 1.255% async dimension-probe regression**. No uniform speedup is claimed.

| Current complete operation | Lokad / reference µs | Ratio interval / status | Managed GC B/op |
| --- | ---: | --- | ---: |
| JPEG headers, fresh source | 3.768640 / 2.835402 | 1.259025–1.367175 / gap | 8,768 |
| WebP headers, fresh seekable source | .142106 / .310231 | .431685–.471884 / pass-stop | 552 |
| JPEG dimensions, async nonseekable | .220388 / .201119 | upper1.133673 / gap | 568 |

Headers use explicitly labeled **FFmpeg n8.1 AVIO/owned descriptor APIs plus an
original bounded structural/EXIF adapter**, not public packet-parser performance.
Both include fresh opaque seekable source, workspace and independent descriptor
acquisition/release, complete late metadata and EOF, and exact nine-field outputs.
The async dimension probe retains its distinct immediate-callback prefix boundary.
No pixels, whole-input copy, cached descriptor or input view is substituted.
Original packets/settings and [reference identities](BOUNDED-HEADERS-CURRENT-REFERENCE.md)
remain fixed; two original images, zero new required paths or workload grid.

The separate controlled DLL comparison measures5.524754→3.771005µs, ratio interval
.672586–.692631, with8,776B on both sides. Its wrapper differs from the standalone
worker's8,768B; that8B is not an allocation improvement. Sync dimension pairs are
inconclusive, interval.989159–1.011918; unchanged prefix-path evidence is reused.
Async pairs measure.214894→.217592µs, interval1.006314–1.018844. Its current native
comparison was refreshed independently; the2.7ns cost is retained, not retuned.

One short complete-JPEG screen precedes independent12-actual confirmation.
All actuals, three header-native brackets, alternating order and99.9%Student-t /
paired-log confidence remain. Controlled pairs and the existing async-reference
worker each retain12actuals. CPU0, natural GC and idle-v2 guards apply. No profile
clocks or historical stage sums establish a gain; no favorable capture is repeated.

The change adds no fields, storage, cache, scope or public API. Byte consumption,
fill/request order, pending users, cancellation, failures and owner cleanup remain;
restart/real markers still count and stuffed pairs still do not. Failed private
grammar state is abandoned; only completed metadata is published. Two new public
guards force4,097stuffed pairs across one-byte reads, genuinely pending async input,
late EXIF and restart-item limits. Both pass original/candidate Linux normal/scalar
and current Windows normal/scalar. Twenty existing same-packet controls also pass.

Affected complete costs reconcile before **6,747 full/live checks per OS**
(3,294internal/2,505public/948differential), focused10internal/156public normal/scalar,
and the **SAME actual Release-package consumer/all three examples on both OSes**.
One core/embedded/public build per OS, zero helper/native/harness/fixture builds,
one NoBuild pack; no timed Linux rebuild. Unchanged arithmetic/ISA paths reuse
qualification. Encoded packets, decoded pixels and other editing contracts remain.

Managed admission stays512B initial/8,448B bulk maximum; new allocated/admitted/
retained capacity is zero. Native scratch16,416B excludes contexts/descriptors.
GC totals, admission, retained results and unmeasured physical/native/JIT peaks
remain separate. Reader storage ends with its actual users.

[Evidence](jpeg-stuffed-pairs-evidence.json) pins both cores, actual package,
sources, worker/reference, all costs and qualification. Raw
`tmp/jpeg-stuffed-pairs-qualified.tar.xz`, SHA-256
`3828e4155a1ad79b0c81fd9d8a9ff1d952429315ef5b583cf52587c3577fdffe`,
4,588,880B/155members, is mirrored/member-verified with four pinned dependencies.
It retains three pre-timing harness preparation failures and their recovery;
no completed measurement was restarted. Reproduce using its `smp1130-*` scripts
and unchanged bounded-header/ROI/probe workers. Publication remains postponed.

JPEG header and async-prefix gaps remain visible. Stop passing WebP; reconcile
the remaining original named boundaries before another tiny optimization.
