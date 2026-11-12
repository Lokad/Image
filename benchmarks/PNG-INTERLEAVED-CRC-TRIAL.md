# PNG interleaved CRC trial

The eight-residue encoder CRC trial is **not retained**. The qualified four-residue
encoder, production DLLs and actual development package are restored byte-for-byte.
Current costs remain in [wide chunk CRC](PNG-WIDE-CHUNK-CRC.md).

| Complete quiet-VM operation | Baseline / trial µs | Result |
| --- | ---: | --- |
| Stored RGBA16, retained-native post-GC4 | 439.40 / 429.00 | 10.40 µs / 2.37% raw gain; normalized confidence overlaps |
| SmallestSize RGBA16, fresh state | 20,310.66 / 21,564.66 | 1,253.99 µs / 6.17% raw adverse capture; normalized confidence overlaps |

Both filtered and all-actual raw intervals separate in these comparisons. Native
mean moves −0.10% for the stored comparison and −0.32% for the compressed comparison.
The compressed result is a material unresolved concern, not a proven uniform causal
regression. It prevents accepting this implementation for the smaller stored gain.
No further sampling is taken to obtain favorable bounds.

A polynomial model checks 5,752 intermediate states across 160 input/seed controls.
Two independent wide accumulators advance eight residues by x^1024, then join by
x^512 before the existing serial combination/reduction/tails. Exactness passes.
All 20 trial encoder views retain exact complete packets, frozen inputs/settings,
normalization, output ownership, native identities and idle-v2 guards. Total
allocations and image admission/retention are unchanged; one 16-byte static format
factor was added only in the unretained implementation. Physical/native peaks are
unmeasured. Current costs and statuses are not replaced by these trial results.

A second implementation makes three-input XOR explicit. Its JIT introduces a move
in place of the removed XOR, and its complete-operation evidence establishes no
additional benefit. Preserve both sources, modules, screens, confirmations and
adverse results. These are implementation results; materially revised designs
remain possible. No CPU claim follows from instruction or sample counts.

The retained change is one independent correctness test: 896 nonperiodic payload,
type-seed, unaligned and stride/tail comparisons against the literal bitwise PNG
polynomial. Windows/Linux each pass 3,182 internal checks on restored production
sources; the new test also passes scalar/AVX512-off/AVX2-off modes. Public/live/
actual-package qualification is reused from the unchanged producer: 2,381 public
and 948 live checks, same actual package and all three usage examples per OS.
There is no production rebuild or package proof after restoration. Two core and
three embedded builds per OS cover both prototypes and the final retained test;
public, benchmark, native and fixture builds are reused.

[Evidence](png-interleaved-crc-trial-evidence.json) preserves the full measurements,
module/source identities, scoped source/JIT proofs, selection and restoration.
Retained-native captures identify their frozen harness root; fresh compressed
controls identify current experiment sources. Both identities and the exact timed
producer pins are retained. Rejected results are not renderer dependencies.
Raw `png-interleaved-crc-trial.tar.xz`, SHA256 `f29e33af687835b6656a51e42886d6d7ceb9751b7d75790db73ed6d8f2f7bca3`
(3,144,040 B / 342 members), is mirrored/member-verified with
both source contexts. Published packages remain unchanged; publication postponed.

Next: investigate the existing byte bilinear reduction gap using current source
and one bounded complete-operation attribution. Keep every required path and its
pixel, ownership, admission and confidence contracts; this trial closes none.
