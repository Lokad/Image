# Warm Clone: complete-overwrite storage

The fixed RGBA16 Clone case improves 95.21%
by screened mean, from 22.55x FFmpeg to
1.04624x. Both baseline quiet guards pass. Failed later guards remain failed.
Both quiet guards pass for the independent twelve-sample repeat, certifying this fixed pair.
One original 513x259 raster is measured; uniform warm parity remains open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Fresh-array Clone baseline | 374.67 | 22.54836x | 24.47991x | Pass |
| Complete-overwrite Clone, short screen | 17.95 | 1.04624x | 1.30391x | Pass |
| Independent twelve-sample repeat | 17.86 | 1.07626x | 1.08886x | Pass |

One Clone primitive replaces fresh full-array allocation and zeroing with the
existing admitted complete-overwrite allocator. Every operation still copies
all source pixels. Its original row loop, exact sample work count and at most
16,384 bytes between work/cancellation checks are unchanged. Both complete
buffers remain admitted; complete channel bytes, alpha, hidden RGB, geometry,
source orientation and independent live ownership are preserved. A private
partial result is disposed on failure. The existing single idle raster slot
retains at most 4 MiB; storage policy and zero-initialized Create remain unchanged.
Released storage may be reused; processed outputs are never cached.

Fourteen new controls cover both depths, long rows, exact memory/work limits,
cancellation, hidden RGB and independent live owners. They poison returned
storage before partial failure, verify EXIF retention and check identity Resize
and ResizeToFit results, which call Clone. Focused checks pass on both OSes with
intrinsics enabled and disabled: 507 baseline, 521 final. All 2,251 Release tests
per OS pass (533 internal, 822 public API, 896 live differential), along with all
three examples through the same actual dependency-free Release package. Timed
and fresh production methods/flags match that package. Only existing Clone's
body changes, with no added or removed methods (555 retained). All other bodies
and flags are equivalent with metadata references resolved.

The original public-API FFmpeg adapter runs a null graph, which emits a shared
view. Every timed operation allocates independent storage with av_frame_get_buffer
and copies the view with av_frame_copy, then releases the result. Graph/source/
frame wrappers are reused. Padded rows need no redundant packing copy. Fourteen
negative protocol/input/affinity controls pass. Every channel byte matches the
original raster and pinned normal/scalar CLI output; two held outputs prove
mutation isolation and the source remains unchanged.

Runs execute sequentially in Release on the dedicated VM, CPU0, one filter thread,
thread type zero, normal SIMD, tiering off and non-concurrent workstation GC.
Native batches use the declared 50 ms calibration target, three pre/post samples
for screens and twelve for confirmation. Managed samples retain the 200 ms target. The repeat has twelve actual managed
iterations and ten filtered results under the declared BDN outlier policy;
all original measurements remain in the evidence.
Prepared generated children avoid builds during statistical windows. Preparation
executes the actual fixed job for correctness only, with one native operation
before/after and null ratios. Cooldown scheduling precedes unchanged five-second
quiet guards; a later cooldown never certifies a failed interval. Startup, IPC,
I/O, generation, hashing and validation are excluded.

The [numeric evidence](warm-clone-reuse-evidence.json) retains managed/GC/threading
results, native samples, guards, source and actual-child identities, plus immutable
archives. Conservative bounds divide managed confidence upper by the smaller
native confidence lower using the managed critical value. Regenerate with
`python benchmarks/render-warm-clone-reuse.py`; add `--check` to verify.
See the [worker/replay recipe](../tools/WarmReference/README.md).
