# Warm JPEG shared forward products

Sharing the first horizontal SIMD product improves complete warm JPEG encoding
while retaining every original binary64 coefficient bit and encoded byte.
The AVX-512 branch updates eight independent vertical-frequency accumulators
in the original y/x addition order, with two separate rounded multiplications.
Logical work, admission/cancellation, the scalar/AVX fallbacks and quality stay
unchanged. There is no new heap allocation, explicit scratch or output cache.

The [numeric evidence](warm-jpeg-forward-shared-evidence.json) retains original
timing samples, brackets, memory, quality/output/source identities, JIT contexts,
validation and raw archive hashes. Regenerate with
`python benchmarks/render-warm-jpeg-forward-shared.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Retained component rows | 9485.54 | 3794.72 / 3773.77 | 2.51x | 3.33x |
| Retained component rows | 9431.87 | 3775.93 / 3787.21 | 2.50x | 3.03x |
| Before basis hoist, not retained | 8037.77 | 3782.96 / 3822.84 | 2.12x | 2.38x |
| Before basis hoist, not retained | 9417.72 | 3781.18 / 3773.32 | 2.50x | 3.03x |
| Shared forward products | 8035.26 | 3807.72 / 3790.85 | 2.12x | 2.51x |
| Shared forward products | 8038.71 | 3798.74 / 3782.47 | 2.13x | 2.58x |

Both screens use one frozen matched pair on the existing original 513x259
RGBA8 raster: owned quality 69, native qscale 5, explicit alpha flattening
onto RGB 0x204060. Selected bitstreams and the fixed source-error band are
unchanged. No preparation, additional image or quality enumeration was run.
Complete encoding/conversion and fresh MemoryStream creation/growth/disposal
are timed; native reset and all pixel work are timed with reusable contexts
and scratch. Startup, IPC, I/O, generation, hashes and validation are excluded.

Runs are sequential in Release on CPU 0 of the dedicated AMD EPYC 9V45 VM,
one codec/conversion thread, normal SIMD, .NET 10.0.8 / SDK 10.0.204, disabled
tiering and non-concurrent workstation GC. Managed allocation remains
1,051,907 bytes/op, exactly the retained baseline.

Original transform tests pin all 64 coefficient bits for eight analytic blocks
and retain argument guards, exact work admission and cancellation before output.
All thirteen cases pass in default AVX-512, forced AVX and scalar modes; the
original 44 public JPEG admission cases pass too. Generated code contains no
fused multiply-add. A bounded basis span is acquired before accumulator
initialization; both observed compilation contexts keep the accumulators in
registers. The prior version had two compilation forms with different spills
and an unstable repeat; both earlier screens and JIT forms remain visible.
They motivated the hoist, without proving which JIT form ran in each timed
child. All controls remain original; they add no benchmark images.

All 2,026 tests pass on Windows and Linux: 446 internal, 686 public and 894
live FFmpeg comparisons. The same actual dependency-free Release package
passes verification and all three usage examples on both platforms. Its
production method bodies and thirty existing original JPEG fixture pixels
match the measured library. Package SHA256:
`ec685ba2d21a0e7fb2fe2f1bd6ddb2e20ac7f9a391436ee7345322036d8ff7d2`.

The prior version had one repeat attempt fail quietness before any benchmark
child started. The final version has two complete narrow screens.
Completed runs pass their pre-run checks; post-run failures and later
cooldowns remain in the ignored evidence. These short screens establish a
useful gain; their means remain above 2x and do not certify parity.
PNG decode at 2.36/2.33x now ranks ahead. Other measured, unknown and
unmatched directions remain open.
