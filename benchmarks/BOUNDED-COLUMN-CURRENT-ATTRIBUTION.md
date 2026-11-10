# Current bounded-column attribution

BCA753 reuses the qualified PRR752 actual-package DLL for one two-second CPU0
capture, with sampling onCPU1. No production/test change, build, new workload or
timing confirmation. All instrumented elapsed is discarded. The existing complete
513×259→256×129 RGBA8 cost **504.20/zimg419.02µs**, upper1.208,113B/op, remains
source-valid under [wide overlap](BICUBIC-WIDE-OVERLAP.md). The column scheduler,
geometry and relevant byte math bodies are unchanged; unrelated whole-file edits
do not invalidate those paths. Earlier [bounded attribution](BICUBIC-BOUNDED-CURRENT-ATTRIBUTION.md)
profiles the implementation before shared overlap and stays historical.

| Actual sampled body | Code bytes | Locations out of1,996 |
| --- | ---: | ---: |
| Large byte strip | 5,922 | 680 |
| Small byte strip | 6,046 | 1,160 |
| Geometry Build/Read/Prepare | — | 59 |
| Exact original-order byte repair | — | 3 |

Absolute addresses and unique emitted sizes distinguish the identically named
strip instantiations. Hot selected instructions include indexed horizontal
fetch/broadcast and vertical row-offset/bounds/weight traversal. Geometry setup
and exact replay are not the dominant sampled locations. Inlining and sampling
skid prohibit interpreting counts as phase times, removable percentages or a
speedup forecast. Frozen exact managed/native frames, ownership, sources and
both quiet idle-v2 guards pass.

The next hypothesis traverses two adjacent output-column pairs together at one
y, sharing vertical row-address and coefficient preparation while retaining two
independent original-order sums. A small leaf can also reduce pressure in the
large scheduler. Keep original pair/single tails and word execution, publishers,
certificates, exact repair and the existing4KiB/result-loan lifetime. No array,
cache/session, owner/root, numerical-policy change or general framework.

A bounded address/order model on the existing control reduces logical row/weight
visits132,736→66,368 while keeping132,736pair sample loads. The existing151×83→75×41
functional control checks clipped/odd tails,12,321→6,327visits and unchanged loads.
This is neither actual SIMD nor a CPU result. Next prove actual helper bits,
guards, no allocation and complete scheduler/root/cancellation/loan behavior;
inspect emitted code and word-mode erasure before the existing short screen.
Use a current original/final confirmation when useful and refresh affected
TightDown16 status. Revise or abandon based on complete costs and simplification;
no fixed threshold or family ban. Do not expand the benchmark matrix.

Managed preparation,4096B admission,4080/3232B explicit stack payloads and owned
output/disposal stay timed. Geometry and cache loans retire before publishing
those output pixels. Retained operation scratch stays zero; existing output-pool
retention is separate. Physical/native/JIT-stack peaks remain unmeasured. Native
zimg retains its declared graph/input/scratch and halo/filter/packing boundary;
this does not establish equivalent fresh-graph performance or relaxed pixels.

Unchanged6155full/live perOS,329internal/729public normal/scalar/ISA and SAME
actual-package/allthree-example qualification are reused. PRR752 commit06c3768a
CI42773274478 is green in allthree jobs. The full283-path objective remains open;
text/drawing and independent codec work remain eligible. Publication is postponed.

[Evidence](bounded-column-current-attribution-evidence.json) pins source identities,
reused costs, producer, code addresses, model and next step. Mirrored/member-verified
raw `tmp/bounded-column-current-attribution.tar.xz`, SHA256
`e9ccaa68f36aab3dc3104c745d0d43ca20c3e10b8809ba55e6cf307bd8068d46`,128,764bytes/27members, retains the one
capture, source snapshots, scripts and annotation. No renderer dependency is added.
