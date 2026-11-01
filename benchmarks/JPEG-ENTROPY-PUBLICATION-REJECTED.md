# Rejected baseline entropy publication

Single-exit publication in the fused baseline JPEG reader misses 4.1% triage on
the existing 513×259 direct-RGB444 complete decode. Reject the candidate; current
production and qualified costs remain unchanged.

| Complete decode | Lokad µs | Ratio / upper | Observed managed B/op |
|---|---:|---:|---:|
| Qualified original, independent12 | 2,098.407 | 1.449 / 1.458 | 966,367 |
| Rejected candidate, short3 | 2,046.509 | 1.409 / 1.644 | 966,370 |

Point gains are **2.47% filtered /
2.54% all-actual /
2.81% native-normalized**. One active short,
no independent confirmation or guard/layout/refill-width/inlining variants.
These short results establish no speedup or parity claim.

The retained original actual-IP/JIT evidence has 127 of 744 method samples at
reader-state stack operands. This motivated one control-flow change, rather than
a new lookup or arithmetic policy. Those samples are diagnostic, not phase times
or predicted gains. One untimed candidate method dump adds no sampled profile.

Bounded spans and the generated scan map cover the noncancelable loop's accesses.
Every normal marker/tail/code/run/EOB exit publishes the same minimal logical
cursor and fractional bits, then uses the original fallback. The candidate adds
no capacity, owner, charge or explicit allocation. Observed B/op includes harness
costs and differs by 3 bytes; no allocation-saving claim follows.

Original 133,057-byte input/settings, exact normal/scalar/native pixels, source,
independent owned outputs, all actuals/native brackets and both idle-v2 guards
pass. Fresh output, normalization and release remain timed on one CPU; startup,
IPC and checks are excluded. Native allocation is unmeasured.

All 47 internal/388 public checks pass normal/scalar Linux, including malformed
input, truncations, cursors/partial coefficients, cancellation/admission and
cleanup. One core build; **zero test, harness, native or selector builds**.
Source/core/PDB/XML/linked tests restore byte-exactly and the same regression
checks pass. Windows production/package remain unchanged. Applicable full
Windows/Linux, the same actual Release package and three examples are reused;
no full/package repeat follows rejection. No tests were changed or removed.

[Evidence](jpeg-entropy-publication-rejected-evidence.json) retains the decision,
samples, diagnostics and restoration. Mirrored/member-verified raw
`jpeg-entropy-publication-rejected.tar.xz`: 1,823,608 B / 95 members, SHA256
`994aa264c3d94b7faca1dcd1167d1acc7180e84096ad19fdf12d85bf2de72d23`. J4N31/DRGB31 dependencies remain retained.
The full 268-path objective stays open; close this primitive.
