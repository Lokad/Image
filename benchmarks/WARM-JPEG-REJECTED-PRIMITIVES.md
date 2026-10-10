# Warm JPEG rejected primitives

Three narrow candidates provided no useful speedup on the retained native JPEG.
All production changes were discarded. Twenty-seven original boundary tests
remain. The retained JPEG decode gap is still about 2.91x; uniform parity is open.

The [numeric evidence](warm-jpeg-rejected-primitives-evidence.json) retains
actual managed samples, confidence intervals, native brackets, allocation
diagnostics, frozen input/output hashes, source identities and quietness checks.
Regenerate with `python benchmarks/render-warm-jpeg-rejected-primitives.py`.

| Candidate | Repeat | Managed mean, µs | Worst bracket ratio | Conservative upper | Allocation change, bytes/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| extended-prefix | 1 | 5124.96 | 2.98x | 3.28x | 12,320 |
| extended-prefix | 2 | 5132.63 | 2.98x | 3.96x | 12,320 |
| combined-prefix | 1 | 5314.70 | 3.10x | 3.52x | 0 |
| combined-prefix | 2 | 5297.04 | 3.08x | 3.41x | 0 |
| sparse-inverse | 1 | 5744.28 | 3.33x | 3.69x | 0 |

The adaptive twelve-bit lookup replaced the ten-bit table for definitions
containing eleven/twelve-bit codes. Its added admission/allocation had no useful
benefit. The combined ten-bit lookup placed short codes in the existing long
table and selected it directly; it was also slower. Skipping zero coefficients
in the binary64 inverse transform was slower in one screen, so its repeat and
affected-input screen were omitted. These are observations, not CPU attribution
of the rejected code or certified regression bounds.

All five screens ran sequentially on CPU 0 of the dedicated AMD EPYC 9V45 VM,
with .NET 10.0.8 / SDK 10.0.204, normal SIMD, disabled tiering and background
GC, one codec/conversion thread and the same frozen 133,041-byte JPEG. It comes
from one existing original 513x259 RGBA8 raster; no new image, preparation or
quality enumeration was used. Each pair receives identical resident bytes.
Full public decode, conversion, independent owned output allocation and disposal
are timed; native reset remains included. Startup, IPC, I/O, generation, hashing
and validation are excluded. All output identities match the retained baseline.

Before each sample, the five-second quietness check passed. Post-run rolling
load checks failed; all failures and later passing cooldowns are retained.
Short screens guide rejection and ranking; they do not certify uniform parity.

After reverting all candidates, one current CPU profile uses the same JPEG.
Method matching excludes parameter types. The following inclusive proportions
are sampled workload CPU, including warmup/calibration, and are not parity timing.

| Current primitive / group | Inclusive sampled CPU |
| --- | ---: |
| JpegDecoder.ReadBaselineBlock | 49.36% |
| coefficientEntropyGroup | 49.36% |
| JpegHuffman.Read | 28.81% |
| JpegDecoder.Render | 27.10% |
| JpegTransform.Inverse | 23.43% |
| JpegEntropyReader.ReadBits | 11.34% |
| JpegEntropyReader.TryReadHuffman | 5.34% |
| JpegEntropyReader.TryReadLongHuffman | 0.45% |

All 1,994 tests pass on Windows and Linux: 414 internal, 686 public and
894 live FFmpeg comparisons. The retained controls cover sixteen mixed-code
cached phases, 288 partial short-code budgets with a long table, 184 partial
eleven/twelve-bit budgets, stuffing/padding/restarts, marker/truncation versus
work precedence, invalid-prefix failures, exact table admission and cancellation.
Canonical codes are independently derived under
[T.81 C.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf). Fixture inputs remain original.

The final production library is the previous accepted implementation. The same
actual dependency-free Release package passes verification and all three usage
examples on both platforms. SHA256:
`fb77f78c640d0571e8916486aa70bfa1b74207a3e30af30bfbf77b5bc40c323f`.

Reproduce sequentially with the [worker instructions](../tools/WarmReference/README.md#jpeg-decode).
