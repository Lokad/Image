# Warm JPEG 4:2:0 decode: eight-pixel rendering

The uncovered baseline 4:2:0 decode case screens at 4.39x FFmpeg.
One bulk render primitive reduces complete decode by
61.17% in the short screen, preserving every owned
output byte. The independent twelve-actual repeat is 1.70860x,
conservative upper 1.71924x. Both independent quiet guards pass, certifying this fixed pair only.
Uniform warm parity and other material cases remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Scalar geometry/color baseline | 4421.18 | 4.38593x | 7.49980x | Pass |
| Eight-pixel 4:2:0 render, short screen | 1716.54 | 1.70020x | 2.13731x | Pass |
| Independent twelve-sample repeat | 1721.81 | 1.70860x | 1.71924x | Pass |

One existing original RGBA8 513x259 raster is encoded outside timing at fixed
native qscale 5, optimal Huffman coding and centered area chroma reduction,
with alpha flattened onto RGB 0x204060. The resulting baseline JPEG has 52,093
bytes. Both decoders consume those identical resident bytes; encode quality
matching is unnecessary for this decode comparison. The explicit NativeDecode420
case selects one pair. There are two retained original performance rasters;
no new image, shape, quality enumeration or profile matrix is added.

The general renderer repeats interpolation geometry and scalar color conversion
for each pixel. The new 4:2:0 path shares row geometry and renders eight interior
pixels together. Centered 2:1 interpolation has exact quarter fractions; bounded
integer numerators (at most 16*255) reproduce the binary64 interpolation exactly.
Safe AVX2 permutations and AVX512 binary64 conversion/color arithmetic preserve
separate multiply/add, rounding, clamping and opaque alpha. First pixels, edges,
tails, other sampling and unsupported CPUs retain the general scalar path.
Existing 64-byte inverse-block stack storage supplies the 24-byte rounded-row
scratch; there is no new workspace, pass, cache, dependency or upstream content.
Coefficient/inverse logic, exact work charge, cancellation, memory admission,
orientation, ownership and errors remain unchanged. Render is the only changed
existing body (1,109 to 1,697 IL bytes), with two added row helpers (556 to 558
methods); all other bodies and flags are equivalent with metadata resolved.

Sixteen new controls test 4,096 seeded centered rows against an independent
binary64 coordinate recurrence, including clipped colors, fractional samples,
unaligned spans, unchanged inputs and output/scratch guards. Focused color checks
pass on both OSes with intrinsics enabled and disabled (28 tests per mode).
The original JPEG420 managed hash is identical before and after the change and
on Windows with intrinsics enabled/disabled. Normal/scalar CLI comparisons retain
RGB maximum 3 and RMSE 0.579363, within the fixed maximum 4/RMSE 1.5 bounds;
alpha is exactly opaque. All 2,282 Release tests per OS pass (564 internal,
822 public API, 896 live differential), along with all three examples through
the same actual dependency-free Release package. Actual timed/fresh production
methods and flags match that package.

The original public-API FFmpeg worker resets the software mjpeg decoder every
operation. Its 4:2:0 path expands both ceil-sized chroma planes by two, then uses
a crop view of the original bounds for full-range BT.601 conversion. This keeps
the declared chroma grid at odd dimensions; direct stretch to the image width
would change the workload. Intermediate and independent RGBA acquisition and
release are timed. Codec/conversion contexts, resident packet and frame wrappers
are reused; padded output rows need no redundant packing copy. Native output
matches the pinned normal CLI exactly. Both sampling modes pass fifteen negative
protocol/input/affinity controls, including rejection of wrong declared sampling.
The existing 4:4:4 setup also passes a validation-only smoke. Source and two
simultaneously held outputs remain independent and exact.

Sequential Release runs use CPU0, one codec/conversion thread, thread type zero,
normal SIMD, tiering off and non-concurrent workstation GC. Native pre/post
batches target 50 ms; managed actual iterations target 200 ms. Generated children
are prepared outside scored windows with one native operation per bracket and
null ratios. The repeat retains twelve actual and
11 filtered managed results. Cooldown scheduling
precedes unchanged five-second quiet guards; later cooldown never certifies a
failed interval. Startup, IPC, I/O, generation, hashing and validation are excluded.

The [numeric evidence](warm-jpeg420-bulk-evidence.json) retains all managed,
GC/threading and native measurements, guards, input provenance, package/source/
actual-child identities and immutable archives. Conservative bounds divide the
managed confidence upper by the smaller native confidence lower using the same
critical value. Regenerate with `python benchmarks/render-warm-jpeg420-bulk.py`;
add `--check` to verify. See the [reference recipe](../tools/WarmReference/README.md).
