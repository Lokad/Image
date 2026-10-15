# Opaque VP8L encoding

Complete opaque-byte VP8L encoding independently confirms **0.102× libwebp
exact=1 (FFmpeg encoder backend) / upper 0.103×**, at **2,100,170 managed B/op**.
Both outputs decode to the exact same pixels. Production is unchanged.

| Same immutable build | Lokad.Image µs | Backend µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 7,857.52 | 76,895.49 | 0.102 / 0.153 | 2,100,170 |
| Independent twelve-actual confirmation | 7,195.24 | 70,637.23 | 0.102 / 0.103 | 2,100,170 |

The existing original 513×259 RGBA8 raster is reused with RGB unchanged and
alpha set to255. Both original and projected hashes are recorded. This is one
explicit projection, with no new original image or depth/quality/effort grid.
Both encoders use effort4; native lossless quality75 is a compression control.
The explicitly approved backend uses its public API with `exact=1`.

Lokad.Image emits **533,100 bytes**, versus **121,896 bytes** for the adaptive
native encoder. This establishes exact-pixel operation timing, **not compression
efficiency parity or an algorithm-for-algorithm speedup**. No production tuning
is justified by this fixed timing comparison.

Managed timing includes complete public EncodeWebp, admission, source processing,
fresh MemoryStream creation/growth and disposal, without redundant ToArray.
Native timing includes full RGBA-to-BGRA repacking, fresh pixel import/allocation,
encoding and independent owned output acquisition/release. Config/picture/writer
wrappers and BGRA workspace are reused; pixels and packets are not cached.
Startup, I/O, IPC, projection, fixture retention and validation are excluded.
Native allocations are **unmeasured**; managed figures include harness overhead.

Managed decoding and normal/scalar FFmpeg decoding of both packets reproduce
every projected RGB/alpha byte. Syntax, geometry/orientation, source preservation,
deterministic packets and independent owners pass before and after timing.
Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass on each run. All twelve confirmation actuals are retained;
BenchmarkDotNet selects eleven. The upper includes selected and all-actual
intervals against the faster native bracket's lower bound.

Only one benchmark input changes. One harness build and cached generated children
suffice, with verified current DLLs preloaded into each child. Existing **3,432
Release tests per OS**, including **898 live comparisons**, and the same actual
package's consumer plus all three examples on Windows/Linux are reused from the
[JPEG qualification](WARM-JPEG-BASELINE-AC-RUN.md). No production/native build,
full suite, package proof, method audit or profile is repeated.

[Numeric evidence](warm-vp8l-opaque8-evidence.json) retains every actual,
allocation, guard, input/output/source/assembly/job/native identity and qualification
reuse. Stop this confirmed pair; full required warm coverage remains open.
