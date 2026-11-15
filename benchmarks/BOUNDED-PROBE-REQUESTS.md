# Bounded probe refill requests

Bounded refill callers already fit their requested field/chunk at the supplied
offset. The shared reader uses that request before the input/acquisition clamps,
removing a redundant capacity minimum. Unbounded read-ahead retains its original
calculation. Actual request sizes, partial assembly, error precedence, cancellation
checkpoints, pending-user lifetimes and ownership remain exact. No API, owner,
buffer, cache or retained state is added.

Independent complete async JPEG pairs save **2.08% / 5.7 ns**:
**0.274392 →0.268685 µs**, interval **.971930–.986548**. Both producers allocate
**568 B/op**. This is a scoped DLL effect, not FFmpeg parity or a sum of historical
gains. The original three-pair sync/async screens and every actual remain.

| Complete boundary / labeled FFmpeg adapter | Lokad / reference µs | Conservative upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| ProbeDimensionsPng | 0.122 / 0.125 | 1.005403 / uncertain | 568 |
| ProbeDimensionsWebp | 0.124 / 0.122 | 1.049541 / uncertain | 568 |
| ProbeDimensionsJpeg | 0.234 / 0.194 | 1.249963 / gap | 568 |
| ProbeDimensionsJpegAsync | 0.271 / 0.202 | 1.393297 / gap | 568 |
| PngHeaders | 0.297 / 0.572 | 0.527285 / pass | 568 |

All five current costs use the same actual immutable timed/package DLL, twelve
actuals, both original native brackets and quiet guards. References are FFmpeg
AVIO/header adapters, not pixel decoders. Inputs, acquisition, exact independent
descriptors and disposal match their frozen boundaries. Async callbacks complete
immediately; suspended-user behavior is covered separately. JPEG probes remain
gaps; PNG/WebP confidence stays open. PNG headers pass/stop. A separate three-pair
complete-JPEG-header guard is inconclusive (ratio.997838,interval.919204–1.083198),
with unchanged8776B/op; it establishes no header speedup.

Every private refill caller was inspected: fields are at most32B, marker reads
need1/2B, and scan/skip chunks fit capacity. Only field assembly uses an offset;
adaptive growth precedes request calculation. Restoring this calculation alone
reconstructs the whole original source. No parser or validation duplication.

**Windows/Linux each pass6,721full/live checks**,10internal/148public focused
normal/scalar and the **SAME actual Release-package consumer/all three examples**.
One core/embedded build perOS,one pack,no public/harness/native/fixture build or
post-timing rebuild. Performance reconciliation precedes the single broad/package
milestone. Unchanged Identify,codec,editing,resize and font/text paths reuse valid
evidence; this is not a release-wide timing refresh.

Admission remains **512B dimension /8448B bulk maximum**, zero retained operation
workspace. Allocation, logical charged capacity and unmeasured physical/native/JIT
peaks are distinct. Safe managed execution, terminal refusal and all existing
resource and correctness contracts remain. Published packages are immutable;
publication and the full289-path objective remain open.

[Evidence](bounded-probe-requests-evidence.json) pins current producers, complete
boundaries, samples and qualification. Raw `bounded-probe-requests-qualified.tar.xz`:
8,036,872B/167members, SHA256
`c67739464f88e8e929351c2f00b4323edd5429607a8cb5320ae92fdefd4e5fe4`, mirrored/member-verified. Dependencies retain the previous
qualified producer and frozen tooling; [earlier publication](PROBE-RESULT-PUBLICATION.md)
still owns the unchanged complete Identify comparisons.
