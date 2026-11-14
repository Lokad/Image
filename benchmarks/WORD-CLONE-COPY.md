# Word clone copying

[Current longer integration](OWNED-EDIT-CURRENT-CONFIDENCE.md) supersedes the
normal-word/clone-word confidence rows below. Their original measurements and
scoped incremental gains remain history; no production path changed.


Uncancelable RGBA16 clones now reuse the existing safe disjoint vector copier.
One checked destination slice establishes its known-equal extent before the
loop. This removes repeated destination checks in the four-vector loop without
another copy kernel, buffer, owner, cache or public API. The byte bulk body,
live-token16KiB checkpoints and all admission/failure/disposal behavior remain
verbatim. Portable fallback remains the existing BCL copy.

Independent complete clone comparisons against the preceding immutable DLL:
word **17.3896→17.0929µs**, **1.71% /0.297µs**, paired interval **.96940–.99665**;
byte guard **5.5791→5.3306µs**, **4.45% /0.249µs**, interval **.94791–.96305**.
Both retain56B/op. The byte body is unchanged; this does not identify a word-vector
cause for its gain. Do not add these effects to earlier admission-stage gains.

The first vector-dispatch implementation confirms only0.776%word/1.466%byte
increments; its adverse byte screen remains. Its actual761BClone repeats bound
checks, motivating the revised equal-span preparation (640B; original314B).
Both candidates compare against the original0669DLL, not each other. Neither
code size nor those separate timings establish a measured A-to-B increment.
Only the final implementation receives broad/package qualification.

| Existing complete513×259 control | Lokad µs | FFmpeg µs | Upper / status | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Clone RGBA16 | 17.34 | 17.32 | 1.009 / uncertain | 56.0 |
| Normal orientation copy RGBA16 | 17.07 | 16.64 | 1.037 / gap | 56.0 |
| Clone RGBA8 | 5.47 | 7.72 | 0.716 / pass | 56.0 |
| Normal orientation copy RGBA8 | 5.56 | 8.18 | 0.688 / pass | 56.0 |
| PNG decode/orient/crop/fit RGBA8 | 1,236.40 | 1,477.26 | 0.846 / pass | 169,454.8 |
| PNG decode/orient/crop/fit RGBA16 | 2,874.75 | 2,937.59 | 0.984 / pass | 338,159.1 |

These six complete references use the same immutable packaged DLL. Fourteen
unaffected owned-edit views reuse their identified
[copy-result admission](COPY-RESULT-ADMISSION.md) producers: the only changed
ImageBuffer body is the uncancelable Clone dispatch; every other byte and the
shared copy primitive are unchanged. Combined20views: **16pass,two gaps,two
uncertain**. Normal word copy and unchanged word vertical flip remain gaps;
word clone and unchanged byte flip are uncertain. Stop passing controls.
This is an incremental update, not full289-path parity or a release-wide refresh.

All source/output sample bytes, alpha/hiddenRGB, borrowed source and independent
surviving owners pass. Public admission, output acquisition/materialization and
disposal remain timed. FFmpeg retains graph/source/frame wrappers, then owns
and materializes the result; native padded rows need no redundant packing copy.
PNG boundaries remain complete decode/orient/crop/fit with all intermediates.
Startup/JIT/IPC/preparation/checks stay outside both clocks. CPU0,sequential
Release,natural nonconcurrent GC,idle-v2 guards,all12adjacent pairs/native brackets
are retained;256calls per edit,64per composition. Paired confirmations use
12alternating1024call pairs and do not prove FFmpeg parity.

Metadata allocation remains56B/op for all18owned edits. Source/result capacity,
retained storage and the two-slot/4MiB default ceiling are unchanged; no retained
operation workspace is added. Borrowed caller-root storage bypasses idle storage.
Allocation, admitted capacity and retention are separate; physical/native/JIT
peaks are unmeasured. Existing source/result overlap, escaping ownership,
terminal refusal and cancellation cleanup remain exact.

Windows/Linux each pass **6,683full/live checks**, plus24internal/221public focused
checks in normal/scalar modes, including all three named resize-storage suites.
The **same actual Release package and all three examples pass both OSes**.
One final core/embedded build perOS; one earlier Linux candidate build. No
public/harness/native/fixture rebuild or production rebuild after timing.
All six affected costs reconcile before broad/package checks. Published packages
remain immutable; publication is postponed.

[Evidence](word-clone-copy-evidence.json) pins every producer, sample, unchanged
path, codegen observation and qualification. Mirrored/member-verified raw:
word-clone-copy-qualified.tar.xz, **4175872B/230members**, SHA256
9596f8228d0e67e0e9973d78f2e4fe571a86a8ea59be76204bd19640b81e03f9. Its dependency preserves the original baseline, public module,
reused harness/native/fixture identities and all14unchanged current edit views.
