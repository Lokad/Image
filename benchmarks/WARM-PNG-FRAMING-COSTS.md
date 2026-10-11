# Warm PNG framing costs

Before Work inlining, the retained PNG16 profile identified strict framing as the
largest sampled cost. Separate literal output-admission and wider prefix-refill
trials gave no useful complete-operation gain and were discarded. Production
codec behavior was unchanged during those trials. Their historical 2.36/2.33x
baseline is below; the accepted [work-admission round](WARM-WORK-ADMISSION.md)
records the subsequent improvement.

The [numeric evidence](warm-png-framing-costs-evidence.json) retains original
timing samples, source/input/output identities, the profile, assembly controls
and raw archive hashes. Regenerate with
`python benchmarks/render-warm-png-framing-costs.py`.

| PNG16 stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Retained filter None | 7592.82 | 3228.33 / 3223.98 | 2.36x | 2.57x |
| Retained filter None | 6827.11 | 2927.41 / 2929.16 | 2.33x | 2.84x |
| Rejected literal admission | 6874.83 | 2921.58 / 2926.50 | 2.35x | 2.41x |
| Rejected wider prefix refill | 6762.55 | 2923.59 / 2923.72 | 2.31x | 2.44x |

Both trials and the separate CPU profile use the same original 513x259 RGBA16
raster and identical resident PNG bytes, including zero-alpha hidden RGB.
No additional benchmark image or quality enumeration. Complete decode and
independent output acquisition/release are timed, including required native
endian conversion. Native contexts/scratch are reused. Startup, IPC, I/O,
generation, hashes and validation remain outside the equivalent boundary.

Runs are sequential in Release on CPU 0 of the dedicated AMD EPYC 9V45 VM,
one codec thread, normal SIMD, .NET 10.0.8 / SDK 10.0.204, disabled tiering
and non-concurrent workstation GC. The separate profile includes generated
warmup/calibration workload loops and is not native parity timing.

| Method before Work inlining | Inclusive sampled percent |
| --- | ---: |
| `ZlibFraming.Validate` | 56.51% |
| `PngCodec.DecodeRaster` | 32.20% |
| `Inflater.Inflate` | 29.61% |
| `Crc32.Compute` | 9.44% |
| `PngCodec.CopyRgbaRow` | 1.61% |
| `Adler32.Append` | 0.43% |

Match exact method names before parameter lists. Inclusive costs overlap and
cannot be added. The last visible frame may include inlined work or native
transitions without their own symbols. Framing is about 56.5% inclusive;
row copy and Adler are small in this case. No pure managed-only inflation
cost or literal-loop-only percentage is inferred.

The rejected branch kept Huffman decoding, bit consumption, cancellation/work,
history/window checks and errors. All 78 original framing controls pass on
both platforms and all 2,026 candidate Windows tests pass. No repeat, affected
timing screen or full Linux candidate suite was run after the unhelpful result.

Two new original fixed-then-stored streams pin prefix lookahead across LEN/NLEN
and payload, checksum cursor with optional unused trailing bytes, exact 9,522
work units, atomic three-unit stored-payload work and scratch release on both
success and failure. They agree with BCL inflation and add no benchmark image.
The wider prefix-only trial safely read eight bytes, admitting eight to an
empty cache or seven masked bytes when one to eight bits remained. Stored
16-bit reads kept four-byte refill and their exact SkipBytes cursor. All 80
framing controls pass on both platforms and all 2,028 candidate Windows tests
pass. The 2.31x screen, conservative upper 2.44x, supplies no useful gain over
the retained results; allocation matches the latest retained screen. No repeat, affected timing
screen or full Linux candidate suite. Two pre-run quiet checks failed before
any samples were collected. Keep every failure and original sample.

The restored code passes all 2,028 tests on Windows and Linux: 448 internal,
686 public and 894 live FFmpeg comparisons. All production method bodies
match the same existing actual dependency-free Release package, previously
verified with all three usage examples on both platforms. The built profile
project also matches that package. Source inventory and original child build
log accompany the trace; BDN cleans its child directory, so the assembly
comparison covers the surviving profile project. Package SHA256:
`ec685ba2d21a0e7fb2fe2f1bd6ddb2e20ac7f9a391436ee7345322036d8ff7d2`.

Both screens and the profile pass pre-run quietness and fail post-run checks.
All original samples, failed quietness and later cooldowns remain in ignored
evidence. One screen supplies no parity certification. PNG remains the largest
retained measured gap; other measured, unknown and unmatched directions stay
open. One retained-code JIT inspection on the same Decode16 input shows
separate OperationContext.Work calls in the consumed-prefix path. Its optimized
Validate listing is 7,441 bytes; this inspection supplies no parity timing.
Both quiet checks pass. Next, try inlining only Work with its original method
body and admission/cancellation semantics. The subsequent work-admission
report records that trial; avoid repeating refill and literal-admission trials.
