# Shared self-row execution state

Separate-word costs now use [interleaved reference groups](WARM-CADENCE-AND-BRACKETS.md)
on this same immutable artifact; the original samples and accepted self-row gain remain.

The original RGBA16 overlap gap closes on the final timed/package DLL:
**5.186 /FFmpeg 5.602 µs**, upper **0.930762**, **pass**.
Controlled complete forward/reverse pairs save **1.110 µs /20.26%**:
**5.476772 →4.366922 µs**, ratio interval
**0.795688–0.799020**, with separated raw mean intervals and **0 B/op**.
This scopes the final-artifact effect; private/historical gains are never summed.

| Current complete operation / existing FFmpeg reference | Managed /reference µs | Ratio upper /status | Managed GC B/op |
| --- | ---: | --- | ---: |
| Original RGBA16 overlap + reverse, 257×129 | 5.186 /5.602 | 0.930762 /pass | 0 |
| Separate RGBA8, 256×129 | 2.172 /2.080 | 1.059073 /gap | 0 |
| Clipped RGBA8, 254×129 effective | 2.145 /2.091 | 1.034826 /gap | 0 |
| Separate RGBA16, 256×129 | 3.903 /3.902 | 1.091016 /uncertain | 0 |
| RGBA8 overlap + reverse | 3.885 /4.183 | 0.938291 /pass | 0 |

Known self-copies pass one bounded packed span and one row step instead of two
identical spans/steps. The caller's same-object/different-row/matched-depth checks
prove this substitution. The original grouped four loads before stores, bounded
vector ends, two cursors, snapshot Y order, row checkpoints and final cursor rule
remain. Exact source reconstruction verifies the unchanged general/portable,
byte, same-row and separate-word kernels, public validation and admission.
No pin, cache, workspace, owner or public API is added. The useful general kernel
remains alongside this measured specialization; no pixel-loop abstraction is added.

Each private candidate and final artifact passes six internal/190 public focused
checks per normal, AVX-512-disabled and scalar mode on both OSes, plus the original
worker's full-word snapshots, hidden RGB/untouched pixels, cancellation/refusal,
independent ownership and recovery. Two new controls cover 775 row lengths in
both directions using an independent snapshot oracle. Actual helper code is
401 B versus 438 B; three machine-argument words disappear. Code size alone is
not CPU attribution. Private screen/confirmation lead by12.90/17.48%; only the
final actual-DLL effect above is accepted.

All five frozen references reconcile on that immutable final DLL before broad
qualification. CPU0 Release, natural nonconcurrent GC, fixed original batches,
every actual, native bracket and idle-v2 guard stay. Native public plane arguments
are prevalidated; managed public validation/clipping/cancellation remain timed.
Both sides borrow resident source/canvas, without per-operation output allocation
or disposal. Source/canvas preparation, IPC, checks and final disposal stay outside.
Separate-word keeps its262144-operation cadence and uncertainty; do not infer its
kernel needs optimization. Passing original word/byte overlap paths stop.

Windows/Linux each pass **6,805 full/live checks** (3,345 internal,2,512 public,
948 differential), plus the SAME actual Release package/all three examples.
Each final core/embedded assembly builds once; existing public/harness/native/
fixture builds are reused. One NoBuild Release pack; no timed DLL rebuild occurs.
Managed allocation/new admitted/retained capacity stay zero. Standing caller
pixels and unmeasured physical/native/JIT peaks are separate. Unchanged fill,
composite, font/text, codec, resizing and shared single-row-copy paths reuse evidence.

An old minimal reference root lacks the byte-overlap-only assets. Exact original
binary/identity/source/script restore before any WorkloadActual; the initial
failure and guards remain. A local resumed-log collision is corrected before SSH.
No completed measurement repeats, reference upgrades or gate relaxations occur.
The [alignment experiments](COPY-DESTINATION-ALIGNMENT-TRIALS.md) remain unpromoted;
their pin/value-plan machinery is absent from production.

Public CopyRegion selection changes, so all five copy costs refresh. Earlier
[prepared-row](RECTANGLE-COPY-PREPARED-ROWS.md) and [separate-word](COPY-WORD-CURRENT-PACKAGE.md)
captures retain history. Two byte gaps, separate-word uncertainty and other named
requirements keep the full289 objective/final165comparison package refresh open.
Publication remains postponed; published packages remain immutable.

[Evidence](self-row-storage-evidence.json) pins sources, actuals and package.
Mirrored/member-verified `self-row-storage-qualified.tar.xz`: **5,034,580 B /206 members**,
SHA256 `32a9632881c333c26d4f44af679d869f0377832c687cbdd1571cc92baf464ea9`. Archived `srs1210-*` recipes and declared dependencies reproduce it.
