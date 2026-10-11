# Warm VP8 branchless interval selection

Selecting arithmetic intervals with integer masks reduces the retained complete
VP8 decode mean about 2.2%, from 2.43x to 2.38x FFmpeg.
This case remains above 2x; uniform warm parity and material coverage are open.

The [numeric evidence](warm-vp8-branchless-evidence.json) preserves raw samples,
uncertainty, identities, allocations, quietness and immutable source/raw archive
hashes. Regenerate with `python benchmarks/render-warm-vp8-branchless.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Read inline 12-sample baseline | 14761.39 | 6077.10 / 6070.94 | 2.4315x | 2.4574x |
| Branchless interval screen | 14445.47 | 6083.42 / 6072.91 | 2.3787x | 2.6814x |
| Branchless interval 12-sample repeat | 14439.30 | 6078.13 / 6078.59 | 2.3756x | 2.4027x |

One fresh CPU profile of the accepted inlined reader attributes 44.96% of
managed workload time exclusively to Coefficients, including the inlined
reader. Method-name matching excludes parameter types, isolates workload
stacks and includes warmup/calibration. Its actual diagnostic library matches
the validated baseline package in every production method body/flag. This is
attribution, not parity timing. A postprocessing archive-name error was repaired
from the existing trace/output; the completed profile was not rerun.

The only production method body changed is `Vp8BooleanReader.Read`; all other
bodies and all implementation flags across 530 methods remain exact. After
the existing 0/1 result is computed, its negation is 0 or -1. For original range
r, boundary b and value v, the new range is `b + ((r - 2*b) & mask)` and the new
value is `v - (b & mask)`. The left interval is therefore exactly `(b,v)` and
the right exactly `(r-b,v-b)`. Valid intermediate arithmetic stays within
32-bit bounds. The original bit-by-bit normalization loop remains unchanged.
This follows the existing RFC 6386 interval recurrence; no implementation
table, buffer, cache, unsafe code or dependency is added.

Work is still charged before probability/interval validation and extraction.
Successful/failing bit cursor, range/value/shift, cancellation, work/memory/output
admission, error precedence, precision, quality and output ownership remain.
The existing 21 scalar-state controls cover normalization widths, byte cursors,
crossings/truncation, probability/interval boundaries, shorter-span index failures,
literal/tree reads and work/cancel rejection. All 53 VP8 controls pass with normal
and disabled intrinsics on both OSes. All 2,084 Release tests pass on Windows and
Linux: 504 internal, 686 public and 894 live FFmpeg comparisons.

An optimized-JIT probe of the saved actual short-screen DLL decodes the frozen
input exactly and shows negation/masks for both inlined interval selections.
Coefficients grows from 1443 to 1486 code bytes. Diagnostic pre/post quietness
passes. This establishes code shape, not speed attribution.

One existing original 513x259 RGBA8 raster and the same frozen native quality-75
VP8/ALPH input are reused. No image, quality search, regeneration, native-adapter
change or broad matrix occurs. Decode consumes identical bytes; unmatched VP8
encode is not timed. Native RGBA matches the pinned CLI exactly. Managed pixels
retain their prior hash; alpha is exact and RGB max1/RMSE0.132879, including
hidden RGB, remains within the specified max4/RMSE1.5 bounds. Source integrity,
geometry/orientation, independent live owners and mutation isolation pass.

Both subjects perform complete resident-input decode and owned output
acquisition/release. Native timing includes state reset, centered bilinear
chroma expansion, limited BT.601 conversion through RGBA16, no-dither RGBA8
reduction, exact alpha merge and scratch/output allocation/release. Contexts
and packet/frame wrappers are reused; padded output rows are directly exposed.
Startup, I/O, IPC, encoding, hashing and validation are excluded. Sequential
Release measurement uses one CPU/thread, normal SIMD, CPU 0 on the dedicated
EPYC 9V45 VM, .NET 10.0.8 / SDK 10.0.204, disabled tiering and non-concurrent
workstation GC.

The same actual dependency-free Release package passes all three usage examples
on both OSes. Both actual statistical DLLs and the fresh Linux assembly match it
in all method bodies/flags; fresh Windows library bytes match the pack. Three
fixed-count repeats of sixteen complete public decode/disposal operations have
exactly equal allocations between the old/new actual packages.

The initial measurement launch fails its quiet guard before starting a child;
its observation remains archived. One actual short screen and one twelve-sample
repeat are executed. Statistical prequiet passes and postquiet fails under
unchanged thresholds. Original failures and cooldowns remain; independent quiet
confirmation and required/material coverage stay open. No universal <=2x
result is claimed.
