# JPEG prefix acquisition trial

**Rejected; qualified production restored.** Joining an opaque segment's last
read with the next required JPEG marker reduces callbacks, but its incremental
asynchronous gain does not justify the synchronous standalone regression.
Current benchmark costs and package identities remain unchanged.

| Complete operation | Baseline / candidate µs | Ratio interval | Result |
| --- | --- | --- | --- |
| Private asynchronous screen, three pairs | .223999 /.213037 | .538571–1.678361 | Promising point |
| Independent asynchronous confirmation, twelve pairs | .217520 /.199010 | .887579–.943362 | Scoped gain |
| Repository-built asynchronous confirmation, twelve pairs | .216657 /.196622 | .899407–.915694 | Scoped gain transfers |
| Repository-built synchronous guard, twelve pairs | .186170 /.187959 | 1.003596–1.015643 | Small regression |
| Candidate asynchronous / native reference | .202877 /.201771 | .988604–1.018979 | Uncertain |
| Candidate synchronous / native reference | .244653 /.198646 | 1.191109–1.269886 | Gap |
| Restored baseline, same synchronous loop / reference | .195589 /.193287 | .986491–1.035572 | Uncertain diagnostic |

All probe operations allocate568B. Paired DLLs establish scoped incremental
effects, not standalone parity. The synchronous reference-boundary control uses
the same new orchestration executable, original compiled BenchmarkDotNet loop,
input and operation counts as the candidate. It confirms a material discrepancy
with the small paired regression. Its cause is unproved; neither a historical
clock difference nor the paired result excuses the candidate's measured cost.
There is no favorable replay or promotion.

Only forward-only asynchronous dimension probing before SOF joins the known
opaque body and next natural marker pair, using the existing256B acquisition
buffer. The frozen513×259 JPEG needs11rather than14reads, still308acquired and
traversed bytes. Synchronous/seekable request traces stay exact. Additional bytes
never exceed the valid input allowance; the limit sentinel and pixel tail are not
prefetched. Existing cancellation order, pending-user lifetime, structured
limits, borrowed input/root and independent result ownership remain.

Five retained public tests pass the original implementation before the trial:
partial reads across workspace boundaries, no pixel-tail requests, independent
results, genuinely pending reads, shared-root refusal/cleanup/recovery, and
cancellation versus invalid-marker/input/acquisition-limit precedence. Private
and repository candidates pass11internal plus177existing and5new public checks
on Windows/Linux in normal/scalar modes. After exact restoration,182public
checks pass again in both modes on each OS. Existing20bounded-header grammar
controls and all original descriptor/source/owner gates also pass.

Complete candidate JPEG/WebP header guards pass at2.067117/2.830804µs
(upper.744375,8768B) and.133483/.298689µs (upper.461057,552B). These unpromoted
costs do not replace current passing evidence. No codec packet, pixel kernel,
font/text/resize/copy implementation or fixture changes. No new required path.

The quiet VM runs sequentially on CPU0, optimized JIT, natural nonconcurrent
workstation GC and idle-v2 guards. Every observation is retained. Original
generated constructor/consumer/unrolled/remainder delegates include real16/7
invocation-allocation checks. Synchronous974960/asynchronous855040managed and
262144native operations per unit remain;16subdivisions aggregate into the
original12statistical units for each of three groups. Extra clocks/loop overhead
remain included. Native reference is explicitly FFmpeg AVIO/owned descriptors
with the existing bounded header adapter, not a public packet-parser benchmark.

Acquisition capacity/admission stays256/512B, with no new fields, owned buffers,
cache or retained workspace. Returned metadata survives source disposal; physical
and JIT peaks are unmeasured. Windows487a0f7f/Linux801f7d09and packagea54dda1c,
their timestamps and embedded assemblies are restored exactly. Reuse6839full/live
checks each OS and the same actual package/three examples. No new broad or
package qualification follows a rejected implementation. One private Windows
core/embedded/public build, one final core/embedded/public build per OS and one
small orchestration build; no harness/native/fixture/package rebuild.

Mirrored, member-verified raw archive: `jpeg-prefix-acquisition-trial.tar.xz`,
13,386,880B,313members, SHA256
`913df27debff916ab921f06d5ade49096a0d9e7a8bbbd17e0dbbcc950125be1b`.
The archived `jsm1250-*` recipes retain exact sources/modules, every actual,
checks, identities and restoration proof, reusing the original prepared workers.
Archiving initially exhausted VM disk;36,413,440B of byte-verified extracted
transfer duplicates and the incomplete uncited archive were removed before the
successful capture. Cited archives, release packages and reference sources remain.
This report supplies no displayed result or renderer dependency.
