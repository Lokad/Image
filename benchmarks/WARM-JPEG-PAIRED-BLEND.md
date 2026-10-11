# Warm JPEG paired alpha-blending trial

An exact paired Int16 multiply-add trial gives no useful complete JPEG encode
gain and is discarded. Managed time is 7.635 ms versus the retained 7.638 ms
screen; the ratio remains near 2.02x FFmpeg. One original mixed-lane alpha
control and the current CPU-cost evidence are retained. Uniform parity is open.

The [numeric evidence](warm-jpeg-paired-blend-evidence.json) preserves samples,
confidence bounds, allocations, frozen quality/input/output/source/library
identities, quietness and immutable raw/source archive hashes. Regenerate with
`python benchmarks/render-warm-jpeg-paired-blend.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Retained baseline | 7638.30 | 3784.38 / 3778.61 | 2.0215x | 2.1956x |
| Rejected paired blend | 7635.14 | 3791.20 / 3784.08 | 2.0177x | 2.1497x |

One current retained-code EventPipe profile shows a 68.55% visible encoder leaf,
18.62% inclusive entropy writing, 14.66% limited byte writing, 10.34% limited
span writing, 9.90% forward transform, 7.38% MemoryStream writing and 1.20%
quantization. Inclusive costs overlap and cannot be added. The encoder leaf
includes inlined arithmetic and traversal; it is not a component-only cost.
The 2,447.34 ms weighted workload and 1,232 segments include warmup/calibration,
so this supplies attribution, not parity timing. Its actual retained child DLL
and both encode correctness receipts match the accepted actual package in every
production method body and implementation flag. A receipt-field error is repaired
without another profile. Immediate postprofile quietness is unavailable; a later
passing cooldown and original failed checker log remain archived.

The rejected primitive packs each pixel/background and alpha/inverse-alpha pair
into positive adjacent Int16 values. An exact Int32 multiply-add replaces two
Int32 multiplications and an addition. All inputs lie in 0..255, product sums
are at most 65,025, and the original +127/divide-255 rounding and binary64
component matrix remain. No table, buffer, cache or heap allocation is added.
Frozen bytes, work/cancellation, memory/output admission and bounds remain exact.
The complete-operation screen does not support retention; no repeat is run.
All production source bytes are restored to the accepted pre-trial version.

The new original control gives adjacent lanes different alpha values, independent
channel values and eight backgrounds, covering every alpha phase. It checks
original scalar blending/component rounding, unaligned spans, output sentinels
and unchanged input bytes. It passes against original, candidate and retained
production. All 17 component/admission controls pass with normal and disabled
intrinsics on Windows and Linux. Primitive inputs add no benchmark image.

The one-pair screen uses the same original 513x259 RGBA8 raster, selected owned
quality 69/native qscale 5, RGB background 0x204060 and 4:4:4 sampling. Frozen
encoded bytes, source-error match within 0.25 dB, decoder/alpha agreement, source
ownership and all seven historical quality settings remain exact. No preparation,
new image, effort change or quality enumeration is run. Complete public encoding
and fresh MemoryStream creation/growth/disposal are timed. Native flattening,
conversion, encoding and owned-output acquisition/release are included; codec,
conversion and wrapper state reuse is declared. Startup, IPC, I/O, generation,
hashing, validation and quality search are excluded.

Sequential Release jobs use CPU 0, one codec/conversion thread, thread type zero,
normal SIMD, .NET 10.0.8 / SDK 10.0.204, disabled tiering and non-concurrent
workstation GC on the dedicated AMD EPYC 9V45 VM. Statistical prequiet passes,
postquiet fails; passing/failing cooldowns remain recorded. These short screens
do not certify passing status.

All 2,050 retained tests pass on Windows and Linux: 470 internal, 686 public and
894 live FFmpeg comparisons. Freshly rebuilt retained assemblies from both
platforms match the previously verified actual dependency-free Release package
in all production method bodies and implementation flags. Its existing three
usage-example results on both platforms remain applicable; no repack is needed.
The rejected actual statistical DLL instead matches the freshly built candidate.
Retained package SHA256:
`108a4bb6845cd4b04d22489c3ade62eda5409c9e2aaf42b7c515935a3dccdcf4`.

JPEG encode stays the largest retained measured point gap near 2.0215x. A distinct
encoder-loop primitive comes next. Unknown, unmatched, uncovered and unconfirmed
required paths remain open.
