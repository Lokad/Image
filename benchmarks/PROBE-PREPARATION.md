# Probe preparation

[JPEG marker preparation](JPEG-MARKER-PREPARATION.md) supersedes the two JPEG
timings below. The PNG/WebP probes, PNG headers and allocation proof remain current.

Four dimension probes now allocate **632 rather than 688 B/op**, saving 56 B per
complete operation. Default values are normalized without an options allocation
or cache; probe facts share their reader owner. Fixed 512 B admission/256 B scratch,
32-byte field bounds, source/result lifetimes and zero retained scratch remain.
Allocation, logical owned capacity and unmeasured physical/native/JIT peaks are
distinct. No causal CPU gain is inferred from historical clocks.

| Complete original operation / FFmpeg AVIO + bounded header adapter | Lokad / reference µs | Ratio / upper | GC B/op | Status |
| --- | ---: | ---: | ---: | --- |
| PNG dimensions | 0.132 / 0.123 | 1.077 / 1.116 | 632 | gap |
| WebP dimensions | 0.133 / 0.124 | 1.073 / 1.105 | 632 | gap |
| JPEG dimensions | 0.301 / 0.198 | 1.518 / 1.555 | 632 | gap |
| JPEG dimensions, immediate async | 0.377 / 0.200 | 1.880 / 1.925 | 632 | gap |
| Complete PNG headers | 0.304 / 0.569 | 0.534 / 0.546 | 592 | pass |

Original inputs, references, independent descriptors and fresh source acquisition/
disposal remain timed. This is the labeled header adapter, not FFmpeg's codec
parser. Probes stop at the required prefix; complete headers scan framing/EXIF/EOF
without PNG CRC or pixel validation. Async uses immediate resident callbacks;
held-read tests establish ownership during genuinely suspended reads separately.
Four probes remain gaps; the full 289-path performance objective is open.

Each probe has a three-actual short screen followed by an independent twelve-actual
confirmation on the same immutable DLL, retaining all actuals/native brackets and
quiet CPU 0 idle-v2 checks. The passing complete-header guard receives one confirmation.
No new image/settings grid, cached result or skipped public work. Existing 12 read
and 21 header controls/preparation, descriptors, stream state, limits and source/native
identities remain exact, including managed/native scalar header controls.

ProbeReader derives the acquisition reader and owns its existing facts; the
separate ProbeState is removed. Plain header readers gain no fields, virtual
dispatch or second cleanup. Existing grammar and acquisition/disposal bodies are
unchanged after the private type rename. Explicit-null limits and invalid-policy/
cancellation precedence remain; limits/root/inputs stay borrowed. Reservations
remain until actual asynchronous users finish, including refusal/cancellation/
failure cleanup and successful recovery. No new memory or quota abstraction.

Windows/Linux each pass **6,692 full/live checks** (3,263 internal/2,481 public/948 live),
plus focused 2 internal/139 public normal/scalar per OS. The **same actual Release
package/all three usage examples** pass both platforms. One core/embedded build
per OS, zero public/harness/native/helper/fixture builds, one pack, no post-timing
producer rebuild. Full/package work follows affected performance reconciliation;
unchanged codec/edit/resize/text and arithmetic/ISA qualification is reused.
Published releases remain immutable and publication stays postponed.

Timed DLL `4451852468da3823d6187dba4f7495e61a69926cc5895be60f9b30925c7ff16d`; actual private package `fd090f96b98dd4ded3fef95870b5ce38796eb6b4d103a72133029694b0ad9304`,
461,574 B. [Evidence](probe-preparation-evidence.json) pins all five current
costs, limits, lifetimes and actual-package proofs. Raw `probe-preparation-qualified.tar.xz`,
3,078,112 B/142 members, SHA256 `9f8e74a1f287ab06a7e8462d1d688981a4543ec87b490edbfb5452296fc6714e`, mirrored and
member-verified. Dependencies retain original fixtures/tools and the preceding
qualified package. The short console's twelve-actual label and initial record-
adapter syntax error were corrected without retiming or rebuilding production.
