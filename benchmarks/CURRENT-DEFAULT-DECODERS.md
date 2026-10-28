# Current default JPEG420 and VP8L decode

Three existing controls have current costs on the qualified0.2.1 core. All are
screen gaps, with exact pixels and passing quiet-VM guards; no production
optimization, independent parity confirmation or before/after gain is claimed.

| Existing control | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Jpeg420 | 6,191.127 | 1,028.722 | 6.018 / 20.175 | 757,166 |
| WebpResident | 4,314.077 | 3,028.299 | 1.425 / 3.692 | 1,216,162 |
| WebpStream | 4,462.612 | 3,422.650 | 1.304 / 1.578 | 1,216,190 |

All reuse the original513×259RGBA8 source. JPEG uses the frozen52,093B baseline
4:2:0 packet; VP8L uses the frozen183,812B exact=1 reference packet. JPEG managed
pixels now equal normal/scalar FFmpeg exactly, including opaque alpha; the old
pre-parity managed hash is refreshed after four-way exact comparison. Input,
reference, quantizers and sampling remain fixed. VP8L reproduces every original
RGBA sample, including alpha and hiddenRGB, with managed/native intrinsics on/off.

Each operation includes complete Decode, independently owned output and release.
Native software decoders reset state and normalize every pixel: JPEG includes
centered chroma expansion/crop and BT.601 conversion; WebP includes ARGB→RGBA.
The stream case includes a fresh non-exposed seekable input, all reads/copies,
EOF and caller-open checks, native AVIO/input/scratch acquisition and release.
Startup, generation, files, IPC and correctness hashing are excluded. CPU0,
one codec/conversion thread, fixedJIT and nonconcurrent workstationGC apply.

Three actual iterations and both three-sample native brackets are retained per
case, including wide confidence bounds. Ordinary clear gaps need no independent
confirmation. Allocation includes harness overhead; native allocation and live
admission peaks are unmeasured. These are tested packet/boundary costs, not
general codec speed claims. Source integrity and independent outputs pass before
and after scoring. Reused native workers also pass fresh untimed exact/protocol
and stream-ownership checks.

No production, harness, worker or selector build is needed. The verified immutable
TDB1 core and IR3 harness reuse five cached selectors. Prior5052checks perOS,
same actual private0.2.1 package and three consumer examples remain applicable.
Earlier selected decoder timings are historical: a811236/fd8e9df changed JPEG
inverse/color, b62fe39 removed work/overlapping admission, and3e4ec8b changed shared
decoder validation/render paths. Current horizontalJPEG/PPC1/TDB1 changes affect
encoding only; unrelated current evidence remains valid where unchanged.
[Evidence](current-default-decoders-evidence.json) retains every actual, native
bracket, filtered/all-actual bound, guard, source/input/output/worker/child identity
and the mirrored176-member raw master. Preparation/retention failures caused no
measurement repeat and are retained.

JPEG420's6.02× gap takes priority over the smaller encoder ratios. One six-second
current-core CPU profile (5930samples) places40.4% in inverse transformation,
24.5% in chroma interpolation and17.0% in its surrounding pixel loop; entropy
accounts for6.4% and color conversion5.8%. Diagnostic shares are not timing/gain
evidence. Exact pixels and guards pass. One profile-host build reuses the core,
harness and worker; postprocessing corrections reuse the captured trace.

The next primitive is SIMD evaluation of the existing exact inverse matrix,
retaining signed overflow, saturation, biased columns, rounding, cancellation,
scratch admission and the portable fallback. The mirrored35-member diagnostic
archive is linked in the numeric evidence; no profile repeat or new raster is
needed. Both WebP gaps remain recorded. The full267-path warm single-CPU
conservative≤1× objective stays open; no content/sampling/settings grid is added.
