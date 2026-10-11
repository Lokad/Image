# Warm JPEG encoder costs

This checkpoint precedes the [shared forward-product round](WARM-JPEG-FORWARD-SHARED.md).
Its profile describes the encoder before that accepted change.

The restored encoder profile at this checkpoint points to entropy writing and the forward
transform. An exact signed-amplitude writer mask and a guarded stock
MemoryStream byte path gave no useful improvement in their separate complete
warm encode screens and were discarded. Production codec behavior
and the retained encoder gap of about 2.5x remain unchanged.

The [numeric evidence](warm-jpeg-encoder-costs-evidence.json) retains the
original timing samples, quality/output identities, source inventories, current
profile, assembly controls and raw archive hashes. Regenerate with
`python benchmarks/render-warm-jpeg-encoder-costs.py`.

| Operation / stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Retained component rows | 9485.54 | 3794.72 / 3773.77 | 2.51x | 3.33x |
| Retained component rows | 9431.87 | 3775.93 / 3787.21 | 2.50x | 3.03x |
| Rejected signed writer | 9523.51 | 3785.72 / 3781.55 | 2.52x | 2.63x |
| Rejected MemoryStream byte path | 9967.10 | 3810.63 / 3780.88 | 2.64x | 3.41x |

Each trial used one frozen matched pair on the existing original 513x259
RGBA8 raster. The same alpha flattening, quality settings, compressed bytes
and source-error band were retained. No new benchmark input or preparation.
Complete encoding, conversion, fresh MemoryStream creation/growth/disposal
and native timed reset/pixel work stay inside the equivalent boundaries.
Startup, IPC, I/O, generation, checks and quality enumeration are excluded.

The separate CPU profile uses the restored source and the same pair. It
includes warmup/calibration workload loops; its duration is not parity timing.
Runs are sequential on CPU 0 of the dedicated AMD EPYC 9V45 VM, with one
codec/conversion thread, normal SIMD, .NET 10.0.8 / SDK 10.0.204, disabled
tiering and non-concurrent workstation GC.

| Current method | Inclusive sampled percent |
| --- | ---: |
| `JpegEntropyWriter.Write` | 25.59% |
| `JpegTransform.Forward` | 20.97% |
| `LimitedWriteStream.WriteByte` | 13.68% |
| `LimitedWriteStream.Write` | 10.97% |
| `MemoryStream.Write` | 7.87% |
| `JpegEncoder.Quantize` | 0.69% |

Inclusive methods overlap and cannot be added. Exact method matching excludes
parameter types and separates Write from WriteByte. The encoder leaf includes
inlined work; no component-construction-only or signed-writer-only percentage
is inferred. The stock MemoryStream trial retained per-byte cancellation and
output admission, subclass/generic span dispatch and the original span path
at the internal integer-overflow boundary. The guarded path gave no
complete-operation gain in the screen, with identical
output bytes, capacity behavior and managed allocation. No repeat was run.

Fourteen added original stream cases retain growth, overwrite, gaps, fixed
and read-only buffers, disposal, nonzero origins, overflow, subclass callbacks
and failed-write quota/cancellation precedence. They compare the original BCL
span output and failure state; they add no benchmark raster.

The next forward-transform candidate will share the first horizontal SIMD
product across independent vertical-frequency accumulators. Every coefficient
must retain its original binary64 bits and y/x addition order, with the same
logical work and admission. No reassociation or new heap scratch.

Three original writer facts remain as correctness controls. They enumerate
all 131,070 canonical signed amplitudes at widths 1 through 16 and check
stuffed bytes, one padding, restart markers, caller ownership and exact work.
Zero width retains work/cancellation; invalid widths and payloads retain
their error argument before work/output. These are primitive payloads, not
benchmark images or a copied upstream corpus.

The restored code passes all 2,026 tests on Windows and Linux: 446 internal,
686 public and 894 live FFmpeg comparisons. Its built profile-project method
bodies and all thirty original JPEG fixture pixels match the existing actual
dependency-free Release package. That same package previously passed
verification and all three usage examples on both platforms. The source
inventory and completed child build log accompany the trace; BenchmarkDotNet
cleans its child build directory. Package SHA256:
`800ae9abb157936ebb3dcf19c0cd5a921c864dce67625152d51900e866e60959`.

The rejected production change remains discarded. One short screen supplies
no parity certification. Two byte-path attempts failed quietness before any
benchmark child started. The completed screen passed pre-run quietness and
failed post-run quietness; it is retained as a rejection observation. All
failed quietness samples and later passing cooldowns
remain in the ignored evidence. Uniform warm parity, other gaps and all
unknown or unmatched directions remain open.
