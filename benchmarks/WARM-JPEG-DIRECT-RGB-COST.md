# Current direct-RGB JPEG cost diagnostic

One completed one-second diagnostic of the existing DirectRgb444 decode collects
934 samples. Both official idle-v2 guards pass. These are sampled CPU locations,
not comparative timing or a new performance confirmation.

- Plain baseline AC reader: 445 samples (47.6%).
- Separable inverse transform: 192 (20.6%).
- Container parsing: 49 (5.2%); baseline block reader: 38 (4.1%).
- SIMD RGB row packing: 12 (1.3%); further packing work has little headroom.

The unchanged current core, prepared harness/selector, frozen input, exact managed
frame, native normal/scalar quality gates and independent owners pass setup.
No production, harness, selector or native builds, full-suite/package repeats or
new fixtures. Existing selected observations and coverage counts remain unchanged.
Separately, the frozen 22,708-byte Sparse2D packet contains zero stuffed FF00
pairs; a byte-stuffing optimization cannot explain its remaining gap.

The preceding [quadrant trial](WARM-JPEG-SPARSE-QUADRANT-TRIAL.md) is parked and
accepted production remains restored. JPEG entropy work is deferred while
lossless WebP compression size takes priority on the fixed photograph/diagram
controls. No JPEG implementation is pending. Compression efficiency, uniform warm
parity and replacement readiness remain open.

[Evidence](warm-jpeg-direct-rgb-cost-evidence.json) records sanitized symbols,
quiet guards, actual producer identities and the mirrored raw archive hash.
