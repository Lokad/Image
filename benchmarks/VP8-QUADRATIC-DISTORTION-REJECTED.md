# Rejected VP8 quadratic distortion

The frozen 513×259 diagram, Q75/effort6, takes **17,163.08 µs** with the trial
versus the applicable retained **14,921.39 µs** screen: 15.0% more elapsed.
Its ratio is 1.658× / upper 1.963×. It fails the required 5% reduction; reject
the change without confirmation or arithmetic variants. These short observations
do not establish independent gain or parity.

The trial shared sign, step and weighted scale across the two candidate levels,
and used the equivalent quadratic distortion identity. States, rates and ties
were unchanged. Both screens allocate 754,344 B/op and produce the exact same
6,818-byte packet. Complete normalization/output ownership, all three actuals,
native brackets, filtered/unfiltered statistics and idle-v2 guards are retained.

The preceding diagnostic reuses the current trellis trace: all 2,408 sampled
instruction addresses map to instruction starts in a same-sized 3,434-byte JIT
replay. The DP loop accounts for 1,357 of 5,912 total samples (23.0%); domain and
weight validation accounts for 279 (4.7%). This is attribution, not phase timing
or proof that replayed native bytes are identical.

Candidate and restored producers pass 21 focused tests normally and with SIMD
disabled on each OS, including frozen native levels and rolling states. Qualified
source and matching DLL/PDB/XML are restored; the actual package is unchanged.
Existing full Windows/Linux and package qualification remains applicable.

[Evidence](vp8-quadratic-distortion-rejected-evidence.json) identifies the mirrored,
member-verified raw archive with candidate source/binaries, JIT attribution,
actual samples and restoration proofs. The current benchmark summary is unchanged.
Next resolve the photograph's stale attribution before choosing a distinct
substantial opportunity; the [267-path objective](WARM-COVERAGE.md) remains open.
