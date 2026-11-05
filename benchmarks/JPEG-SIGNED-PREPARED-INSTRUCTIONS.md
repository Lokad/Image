# Signed prepared JPEG refinement instructions

The existing scan-owned canonical Huffman plan now stores signed insertion
factors−1/+1, rather than raw sign bits. Both ordinary and live-token prepared
refinement consumers multiply by their refinement increment. The positive EOB
count, remaining flags/run/width, fallback eligibility, input consumption and
correction traversal stay exact. One512-word/2KiB plan remains admitted before
allocation and released after its scan; no buffer, owner, cache or session added.

Contemporary original confirmations, separate candidate screens and independent
candidate confirmations retain every actual and both reference brackets:

- **Ordinary progressive444: 461.82 µs / 11.16%**, 4,137.21→3,675.39 µs; reference-normalized reduction 11.33%.
- **Live-token progressive444: 491.58 µs / 11.06%**, 4,443.01→3,951.43 µs; reference-normalized reduction 10.95%.

Filtered and all-actual raw/reference-normalized confidence separates for both.
Each confirmation has12 actuals. The faster native bracket moves+0.19% ordinary
and−0.13% live; the ordinary short has a larger native-clock movement and remains
retained, without supplying the final gain claim. These are scoped complete-decode
effects, not attributed branch times or a general JPEG speed claim.

All operations use the frozen513×259/132,961B progressive444 packet and exact
531,468B output. Source/token setup is excluded symmetrically; public Decode,
admission, required normalization, independent output allocation and disposal
remain timed on quiet pinned CPU0. The live token is unsignaled; native has no
equivalent cancellation switch. FFmpeg retains documented wrappers but resets
decoder state and decodes/converts every pixel per operation.

| Current existing control | Managed µs | FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| progressive grayscale, resident decode | 2,042.18 | 1,844.71 | 1.107 / 1.120 | 683,136 | gap |
| progressive420, resident decode | 1,710.92 | 1,953.57 | 0.876 / 0.886 | 775,314 | pass |
| progressive422, resident decode | 2,385.30 | 2,264.20 | 1.053 / 1.079 | 847,116 | uncertain |
| ordinary progressive444, resident decode | 3,675.39 | 3,361.81 | 1.093 / 1.112 | 984,238 | gap |
| progressive444, live-token resident decode | 3,951.43 | 3,353.74 | 1.178 / 1.181 | 984,238 | gap |
| Validate JPEG progressive420, span | 286.24 | 289.05 | 0.990 / 1.001 | 4,072 | uncertain |

Current status: 1 pass, 3 gaps, 2 uncertain. Original input/settings/pixel or descriptor, native/
scalar CLI, source/independent output and quiet gates pass. The three color-layout
and affected coding-validation costs are current observations, not independently
proved gains. No new content/settings grid; other119 displayed comparisons reuse
unchanged production paths. Passing cases stop. The full282-path objective remains
open; this is not a full actual-package125-row release refresh.

The initiating current profile has1,085 refinement samples among1,935 process
samples in a3066B method. Broad header/sign/run and correction regions motivate
preparing interpretation; sampling skid prevents phase-time or savings claims.
One finite audit checks17,864 eligible length/symbol/field/increment interpretations.
All flag/run/EOB/width/consumption/sign results agree. Existing independent scalar
bit traversal checks all512 prefixes, phases/tails, malformed/long-code/marker/
restart/EOB, overflow and logical error/cancellation prefixes. No golden pixel
expectation is changed. Profile postprocessing corrections reuse the same capture;
instrumented elapsed is discarded and original metadata restores byte for byte.

Measured complete-operation allocation is984,238 versus984,212B/op on ordinary/
live confirmations. Preserve that26B difference and raw GC/batch evidence; do not
claim total allocation equivalence or causally assign it to library storage.
Production changes add no allocation site or owned capacity. Complete physical/
admitted peaks and native allocations are not newly measured; retained scan
scratch after disposal is zero.

One immutable main/embedded build perOS passes5,716 full/live checks
(2,479 internal/2,289 public/948 differential),56 internal/333 public focused
checks per normal/scalar mode, including concurrent cancellation prefixes.
The SAME actual private0.2.1 Release package and all3 usage examples pass both
OSes, including frozen32-block prepared refinement, async/shared admission and
independent survivors. Unchanged54 AVX512-off proofs are reused. Published
packages remain immutable; publication is postponed.

[Evidence](jpeg-signed-prepared-instructions-evidence.json) pins source/modules,
all actuals/brackets/gates, finite audit and package proof. Previous
[scan preparation](JPEG-PREPARED-SCAN-HEADERS.md) and adverse experiments remain
linked history. Raw `jpeg-signed-prepared-instructions-qualified.tar.xz`: 4,435,092B/248members,
SHA256 `6a11454755e51d36c928763973c0d043306d4d50674277054650742b053fa127`, mirrored and member-verified; dependencies retain the
current original profile and previous qualified build.
