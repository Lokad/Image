# Warm PNG framing costs

The current retained PNG16 profile identifies strict Deflate framing as the
largest sampled cost. A separate literal output-admission branch gave no
useful complete-operation gain in one screen and was discarded. Production
codec behavior and the retained 2.36/2.33x decode gap are unchanged.

The [numeric evidence](warm-png-framing-costs-evidence.json) retains original
timing samples, source/input/output identities, the profile, assembly controls
and raw archive hashes. Regenerate with
`python benchmarks/render-warm-png-framing-costs.py`.

| PNG16 stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Retained filter None | 7592.82 | 3228.33 / 3223.98 | 2.36x | 2.57x |
| Retained filter None | 6827.11 | 2927.41 / 2929.16 | 2.33x | 2.84x |
| Rejected literal admission | 6874.83 | 2921.58 / 2926.50 | 2.35x | 2.41x |

The trial and separate CPU profile use the same original 513x259 RGBA16
raster and identical resident PNG bytes, including zero-alpha hidden RGB.
No additional benchmark image or quality enumeration. Complete decode and
independent output acquisition/release are timed, including required native
endian conversion. Native contexts/scratch are reused. Startup, IPC, I/O,
generation, hashes and validation remain outside the equivalent boundary.

Runs are sequential in Release on CPU 0 of the dedicated AMD EPYC 9V45 VM,
one codec thread, normal SIMD, .NET 10.0.8 / SDK 10.0.204, disabled tiering
and non-concurrent workstation GC. The separate profile includes generated
warmup/calibration workload loops and is not native parity timing.

| Current method | Inclusive sampled percent |
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
The next primitive will try a wider prefix-only refill. Stored 16-bit reads
retain four-byte refill so SkipBytes sees the exact original cursor. Preserve
padding rewind, truncated tails, work/cancellation and all framing guards.

The restored code passes all 2,028 tests on Windows and Linux: 448 internal,
686 public and 894 live FFmpeg comparisons. All production method bodies
match the same existing actual dependency-free Release package, previously
verified with all three usage examples on both platforms. The built profile
project also matches that package. Source inventory and original child build
log accompany the trace; BDN cleans its child directory, so the assembly
comparison covers the surviving profile project. Package SHA256:
`ec685ba2d21a0e7fb2fe2f1bd6ddb2e20ac7f9a391436ee7345322036d8ff7d2`.

The screen and profile pass pre-run quietness and fail their post-run checks.
All original samples, failed quietness and later cooldowns remain in ignored
evidence. One screen supplies no parity certification. PNG remains the largest
retained measured gap; other measured, unknown and unmatched directions stay
open.
