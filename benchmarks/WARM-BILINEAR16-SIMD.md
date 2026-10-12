# Warm RGBA16 bilinear SIMD

RGBA16 bilinear resize remains **OPEN** above 2x; independent confirmation is also open. Latest screen mean is
936.46 microseconds, 2.232x FFmpeg;
conservative upper 3.203x. Observed managed mean time
falls 45.2% from the original screen. Full required warm coverage stays open.

| Run | Lokad.Image us | FFmpeg us | Ratio / upper | Managed allocated B/op | Actual/filtered | Quiet guards |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Original RGBA16 screen | 1710.31 | 417.76 | 4.09399 / 4.68748 | 290,089 | 3/3 | Pass |
| Paired-row SIMD screen | 1074.38 | 419.47 | 2.56126 / 2.84419 | 290,113 | 3/3 | Pass |
| Vector normalization/rounding screen | 936.46 | 419.61 | 2.23172 / 3.20258 | 290,111 | 3/3 | Pass |

One retained original 513x259 RGBA16 raster is resized to 256x129. Managed output
matches the exact rational reference before and after timing. The approved
native-only alpha bound is maximum 1 / RMSE <=0.25 in full 16-bit sample units;
native alpha differs at the same 62 pixels, RMSE 0.04332923. Native RGB keeps
maximum 1 / RMSE <=0.25, and zero-alpha RGB stays zero. Normal/scalar public CLI
output exactly matches the worker. Source integrity and two live owned outputs
pass. The library's pixel contract and RGBA8 exact native alpha remain unchanged.

One six-second diagnostic profile attributes 84.0% to short-support filtering
and 15.3% to scalar rounding. The first primitive extends paired-row SIMD to
RGBA16, placing independent pixels in four-channel lanes with the original
tap and separate multiply/add order. The second primitive normalizes and rounds
RGBA16 channels together. Fractional-remainder comparisons preserve binary64
values adjacent to halves; exact tiny-alpha rounding remains authoritative.
An unpaired last row and CPUs without AVX-512 retain scalar filtering.
No new arithmetic scratch, admission change or cached output is introduced.

A separate four-tap preparation prototype yields only about 1% observed mean
reduction and is reverted. Its source, rejected package, raw screen and failed
initial preguard remain retained. That preguard launched no benchmark; one
bounded fresh screen passed. Quiet thresholds are unchanged. Accepted final
source is `59026241084478dd42901fbd156af10187b7c336ec156a7e7905f9043716340d` at recorded revision
`08833bcfae5db60bd8f7e0bd86e8e0aded3a0792`; intermediate sources remain individually bound.

Timing includes complete public Resize, geometry preparation, admission,
independent output acquisition, filtering and disposal. Released managed pixel
storage may be reused; coefficient arrays remain per operation. Native public
FFmpeg filtering includes packed/planar conversion, widened triangle filtering,
encoded-space premultiplication/unpremultiplication, final RGBA64 conversion
and independent pixel acquisition/release. Graph/source/frame wrappers and
geometry contexts are reused; padded output rows are exposed directly.
Startup, I/O, IPC, generation, hashing and validation are excluded. CPU0,
one codec/filter/conversion thread, normal SIMD, tiering off and nonconcurrent
workstation GC remain fixed. Native allocation is unmeasured.

Every actual is retained. Upper uses the larger filtered/all-actual conservative
bound; native 50ms pre/post brackets surround managed 200ms iterations. Each
screen retains three actuals and three samples per native bracket. Both screens
of accepted primitives pass both quiet guards. The attempted independent
confirmation failed its preguard and launched no benchmark. Its one bounded
fresh attempt also failed before launch; both failures are archived and remain
failures. No further repetition occurs in this round.
`exclusive-vm-idle-v2` checks live overlap plus five-second aggregate/CPU0 busy
<=5%, I/O wait <=1% and steal <=1%. Four child identities and original receipts
are frozen before any source mutation or subsequent benchmark preparation.
Managed allocation is raw GC bytes / operations rounded up, including harness.

Both OSes pass 2,396 Release tests each (648 internal, 852 public API, 896 live
FFmpeg comparisons), 145 scalar resize/admission controls each and all three
usage examples through the same actual dependency-free Release package.
All 571 production method bodies and flags in the accepted candidate's fresh
and actual timed child match its package; earlier packages remain separately bound.
RGBA8 validation-only BenchmarkDotNet smoke and native rejection controls pass.

[Numeric evidence](warm-bilinear16-simd-evidence.json) retains original and
intermediate screens, all actuals, brackets, source/input/output/child/package
identities, guards, profile attribution, QA and immutable archive digests.
See the [current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md). Regenerate with
`python benchmarks/render-warm-bilinear16-simd.py`; `--check` verifies this report.
