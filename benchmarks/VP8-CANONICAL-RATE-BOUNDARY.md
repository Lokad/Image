# Canonical VP8 rate core: standalone boundary regression

Reject this implementation and restore the qualified
[luma preparation](https://github.com/lokad/image/blob/bf9d192f/benchmarks/VP8-CANDIDATE-SCORE-PUBLICATION.md).
A scoped paired-build gain fails at the actual standalone photograph boundary:
the candidate costs **326.46 µs / 2.96% more**, with unchanged allocation and
exact packets. The broader canonical-core design is not permanently excluded.

The prototype extends search-local rate views to luma16/chroma and passes the
existing luma4 view to one canonical static scalar/vector residual core. Remove
the old instance entry, change existing tests to bind the actual core, and retain
every native cost/table/packet oracle. No new heap buffer, table, owner, pool,
cache, session, quota or numerical policy. Source projection preserves all rate
math, validation, pruning, strict ties, cancellation and owned lifetimes.

## Complete-operation evidence

Original513×259 inputs, Q75/effort4 photograph and frozen opaque settings stay
unchanged. Source/JIT/setup are outside timing on both sides; public normalization,
preparation/admission, fresh output growth, independent ownership and disposal
remain timed on quiet pinned CPU0. Photograph/opaque complete packets remain
17,688/72,380 B. No new workload/settings grid or candidate repeat for favorable
numbers. All points, including filtered-out actuals, remain available.

| Complete boundary | Original µs | Candidate µs | Candidate/original interval | Allocated B/op | Result |
| --- | ---: | ---: | ---: | ---: | --- |
| Photograph, paired assembly contexts | 10,969.58 | 10,830.10 | 0.98258–0.99202 | 809,072, both | Scoped gain |
| Photograph, actual standalone | 11,016.20 | 11,342.67 | 1.02314–1.03552 | 809,225, both | Loss; reject |
| Opaque RGBA8, actual standalone | 15,006.47 | 15,911.61 | 1.05069–1.06997 | 1,201,552, both | Uncertain |

The independent paired photograph comparison uses12alternating pairs ×16complete
encodes in isolated immutable assembly contexts. It establishes a scoped139.48µs/
1.27% lead. TransparentRGBA16/ordinarydiagram/trellis short effects remain
uncertain; no gain, loss or equivalence is claimed there.

Seven actual standalone candidate comparisons then expose a contradiction.
Two contemporary original controls reuse the SAME standalone/default-context
worker, native reference and original immutable DLL. Each has12actuals and native
before/after brackets. The table's standalone interval conservatively combines
filtered/all-actual raw endpoints; source/packet/quality/alpha/owner/quiet gates pass.

Photograph's reference-normalized candidate/original interval is
**1.01644–1.03965**. Native time moves only
0.086%; raw and normalized evidence confirm the actual caller loss.
Opaque8 remains uncertain despite its adverse point; no causal loss or equivalence
claim. Original parser-path failure was recovered from completed logs using the
existing parser; no measurement or build rerun.

This comparison resolves acceptance, not the precise JIT/layout cause. Do not
promote the paired-harness effect to a general production gain, infer component
times from static loops, or blindly replay the implementation. A revised design
needs its own actual-caller evidence.

## Qualification, restoration and retention

The candidate passes5,724full/live checks perOS (2485internal/2291public/948live),
affected normal/scalar/AVX2-disabled/AVX512-disabled checks and the SAME actual
private Release package/all3usage examples. These functional checks completed
while the timing contradiction was being reconciled; they do not establish
performance acceptance and are retained as candidate history.

Restore source, main/embedded modules, public/live/probe bindings, source manifest
and actual package byte for byte on Windows/Linux. Reuse the qualified milestone's
5,724checks and actual-package proof without rebuilding or repeating unchanged
qualification. Current benchmark rows and their evidence owner remain unchanged;
this rejected study is not a renderer dependency. Published packages are immutable.

Allocation is unchanged at each matched boundary, separately reported from peak
and retention. No new owned heap capacity; existing16KiB macroblock scratch remains.
Retained operation scratch is zero. Complete physical/admitted peaks and native
allocations are not newly measured. No CPU claim follows from allocation.

[Evidence](vp8-canonical-rate-boundary-evidence.json) retains every screen,
independent comparison, candidate/original standalone observation, source proof,
qualification and exact restoration. Raw `vp8-canonical-rate-execution-rejected.tar.xz`:
2,632,584B/268members, `4f4ca7060e11e52a74ddf4e323270504cf87d2a02daa85e96e69538bbbc4ae1e`.
Restoration `vp8-canonical-rate-restoration.tar.xz`:
5,244B/8members, `fd73eedb3c7bf7ad2981b157f31320cb7482ffe1ec72f25205b3c5807cb801f7`.
Both mirrors and all members are verified, with qualified inputs reused by hash.

Continue independent substantial codec/resize work. One rejected implementation
neither closes its family nor blocks the remaining282-path objective. Publication
stays postponed.
