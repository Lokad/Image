# Bicubic result-row workspace reuse

Complete RGBA8 bicubic 513×259→128×64 with 4 KiB working admission saves
**58.90 µs / 12.66% raw / 8.94% reference-normalized**,
465.06→406.16 µs. Filtered/all-actual raw and normalized confidence
intervals separate. Native movement is -4.09%; all observations remain.

| Complete frozen control | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8, 4 KiB | 406.16 | 367.01 | 1.107 / 1.119 | 113 | gap |
| RGBA16, 4 KiB | 433.70 | 436.46 | 0.994 / 1.003 | 113 | uncertain |

One contemporary original and one independent final byte confirmation retain
12 actuals each. Earlier 419.13/native349.29 µs costs belong to a different clock
regime; the gain above uses 465.06/native382.65 µs. The initial candidate confirms
427.29 µs before the geometry-return temporary is removed. Screens establish no
independent parity. There is no further unchanged sampling for a favorable pass.

## Shared execution and ownership

The existing closed-mode executor processes sixteen columns over the top rows,
then returns before restoring geometry and processing the tail with four columns.
Cache/weights borrow 8,704+2,176 B from unpublished result rows: 10,880 B inside
11,264 B in the primary control. Top 42 rows never write the final 22 loan rows.
Only tail metadata is restored after all loan users retire. The complete operation
times this restoration and repeated filtering; no setup is moved outside timing.

Logical row visits fall 2,048→1,040, source conversions 8,288→4,448, decoded pixels
229,474→189,418 and geometry builds 704→390. Horizontal filtering increases by
1,536 column-rows. These are work counts, not phase timings or a CPU forecast.
The corrected 152 B geometry footer leaves an 88-column prepared prefix; the
[attribution](WIDE-COLUMN-ROW-STORAGE-ATTRIBUTION.md) retains the earlier model error.

No heap capacity, pool, root, owner, session or public API is added. Admission stays
4 KiB; explicit top stack payload is 3,296 B versus original 4,068/3,776 B modes.
The independently owned result retains 32,768 B until disposal; operation scratch
retention is zero. Total allocation remains 113 B/op. Static JIT frame adjustments
are recorded separately; complete physical/native/JIT-stack peaks are unmeasured.
Existing output-pool retention and governed bypass remain separate.

Original geometry Prepare/Read/Build and ordered coefficient/pixel arithmetic are
verbatim. Scalar repair, tiny-alpha unwind, cancellation and terminal refusal stay.
The word kernel remains identical after process-address/security-cookie
normalization: 520 B frame / 5,712 B body. The entry driver changes 24→40 B frame,
so both complete costs refresh. The first candidate's conditional geometry return
created an extra 48 B temporary; explicit branches remove it. Current word upper
1.00267 remains uncertain. The unchanged original countercheck passes upper
0.99691; overlapping raw intervals do not prove a word regression. The older
passing cost is not substituted for this changed complete path.

Six new checks exercise actual top/tail kernels, poison the retired loan before
restoration, compare every pixel, test cancellation before tail writes, zero
allocation, borrowed-root release and recovery. Functional controls include the
existing 528×264 boundary plus odd stride/partial sixteen-column strips and a
narrow image with no geometry footer. No benchmark image/settings grid is added.
The actual package's existing frozen bounded-wide consumer covers aggregate
admission, refusal, cancellation, source preservation and surviving outputs.

## Qualification and boundaries

Windows/Linux each pass **6,167 full/live checks**, 341 internal/729 public resize
checks per normal/scalar/disabled-AVX2/AVX512 mode, and the **same actual Release
package/all three usage examples**. Two coherent core and four embedded-test
versions per OS include two test-only coverage corrections; final broad/package
qualification runs once per OS. No harness/native/selector/fixture rebuilds.

The existing 4097×2051→1024×512 default/compact diagnostic matches scalar pixels
on the actual package DLL. Three actuals each screen 20.53/21.64 ms and allocate
1,585,369/140 B per operation. This is a managed-only generalization check, without
independent historical-gain or native-parity claims; its intervals overlap.

Complete public Resize includes conversion, geometry/storage preparation,
filtering, fresh independent output and disposal. The labeled FFmpeg zscale zimg
backend retains graph/input/scratch; halo, packing, filtering, fresh output/release
and approved native float bounds remain timed. This is not equivalent fresh-graph
work. Exact managed pixels remain distinct from native bounds. Quiet idle-v2,
one CPU, sequential Release and frozen input/output/ownership gates pass.

[Evidence](bicubic-result-row-loan-evidence.json) records current costs,
source/code checks, all stages and qualification. Mirrored/member-verified raw
`bicubic-result-row-loan.tar.xz`: 9,584,036 B / 295 members, SHA256
`37b964975f355e6c837fbd2db07f90a02797cb0479299bca5da2080ba71cbef6`. Private 0.2.1 package: 443,312 B, SHA256
`361dd2578ecb2c6fc77ceea66acb0073101d05128c53f16a745adb52734d9587`. Published packages remain immutable; publication is postponed.
All 283 paths remain required. Narrow/packed byte and JPEG gaps remain eligible;
uncertain word timing is not evidence that its arithmetic needs tuning.
