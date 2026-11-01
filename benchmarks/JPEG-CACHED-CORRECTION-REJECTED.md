# Rejected live JPEG cached-bit traversal

The existing live-token progressive444 complete decode is **5.51% slower** in
one short screen. Reject the cached-bit correction loop and restore production
and linked binaries byte-exactly. Allocations and exact pixels are unchanged.

| Existing complete decode | Lokad µs | Ratio / upper | Managed B/op |
|---|---:|---:|---:|
| Qualified original, independent12 | 4,788.190 | 1.421 / 1.451 | 984,212 |
| Rejected candidate, short3 | 5,052.099 | 1.504 / 2.285 | 984,212 |

All-actual elapsed increases **5.34%**; native-normalized point cost increases
**5.86%**. No independent confirmation, variant or speedup/parity claim.
JCR1 completed-row reuse supplies the applicable original cost; it supersedes
the earlier affected refinement-window costs. Inputs/settings remain frozen.

One two-second actual-IP capture first assesses scalar progressive coefficient
permutation: **69/1,921 samples (3.59%)**, versus **101 (5.26%)** in the entire
materialization method. This gives insufficient demonstrated headroom; no SIMD
permutation prototype is built. The same capture has **1,031 (53.67%)** in live
buffered refinement. An exact-host untimed JIT dump maps its instruction offsets.
These fractions are diagnostic, not component elapsed times or predicted gains;
all instrumented timings are discarded. No additional sampled profile.

The single candidate moves refill checks outside a cached-bit loop and skips
coefficient addressing for zero bits. Each logical read retains its cancellation
checkpoint, immediate coefficient update, overflow-consumed bit and logical
cursor. No ordinary/scalar, refill-width, arithmetic or ownership change.

The same **47 internal / 388 public checks pass normal/scalar Linux** on both
candidate and restored original, including independent traversal, pending phases,
consecutive blocks, malformed/stuffed/marker/truncated tails, partial overflow,
cancellation and cooperative-admission cleanup. Screen pixels equal current
normal/scalar/native frames; source, independent owners and quiet-v2 pass.

One Linux core build, one tiny cached-job selector build, zero test/harness/native
or Windows builds. Zero restoration builds; no test/limit changes. Unchanged
5,342 checks per OS, three additional probe contracts and SAME actual Release
package/three-example proof carry forward. Published packages remain immutable.
All current costs and **268 required paths** remain; both primitives are closed.

[Numeric evidence](jpeg-cached-correction-rejected-evidence.json) retains all
actuals, brackets, confidence, attribution, producers and restoration. Raw
`jpeg-cached-correction-rejected.tar.xz`, SHA256
573fb99f7e5b05a371564dc21e2304ad92b475b4b858dca406b2a4f4501d3129 (2,219,048 B / 123 members),
is mirrored and member-verified. Linked J4N31/JCR1/JRW1 archives retain full
package, original reference/frame and immutable-selector provenance.
