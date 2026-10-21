# Source-trained JPEG AC prices

C137 prices the existing coupled 4:4:4 coefficient decisions using a bounded
source sketch. At most 64 evenly spaced MCUs supply two smoothed AC histograms
(Y and shared Cb/Cr). Deterministic integer Q8 prices replace geometric estimates;
the source DCT, Original DQT/DC, distortion cap, lambda and stable ties stay fixed.

| Existing 513×259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 150,479 | 149,650 / 133,041 | 0.225614 dB |
| Original 4:4:4, RGBA16 | 150,395 | 149,528 / 133,043 | 0.233669 dB |

The incremental savings are 829/867 B. Both satisfy the unchanged 0.25 dB gate,
exact alpha and decoder agreement (maximum 3, RMSE below 0.229). Production
packets exactly match the independent C133 candidates on both platforms and
normal/scalar paths. The full-packet gap remains about 12.5%; these controls do
not establish photographic, diagram or general JPEG compression claims. Optional
CompactColor stays separate. Passing grayscale, 4:2:2 and WebP controls stay stopped.

A 512-int stack histogram becomes the price model in place: 2,048 B alongside
the existing 2,304 B MCU, within the admitted 8 KiB workspace. There is no new
heap or frame copy. Work includes at most 136,129 units per sampled MCU plus
3,632 to finish the models; adaptive and async staging estimates include it.
Shared refusal, work exhaustion and cancellation remain terminal. Existing root,
stream-failure, quota and independent-output checks pass.

Each platform passes 4,935 Release tests (2,026 internal, 1,973 public, 936 live
FFmpeg), plus 115 internal and 53 public scalar checks. The same actual private
0.2.0 package and all three examples pass on Windows/Linux. One production
build/platform is reused. Eighteen focused cases cover integer prices, smoothing,
wide sample positions, work refusal and cancellation; public goldens cover RGBA,
caller samples, fixed/adaptive, live cancellation and async ownership. Unchanged
reference/quality evidence is reused by exact packet identity.

One quiet-VM three-actual forward-only screen records **54,822.079 µs / reference
3,761.904 µs**, ratio 14.573/upper 14.972, **563,802 B/op**. Both idle-v2 guards
pass; native allocations are unmeasured. Normalization, source analysis, encoding
and fresh output ownership/release are included on one CPU. This records the cost
of compression; no CPU-parity, speed improvement or independent confirmation is
claimed. The reused harness's `fixed-geometric-v1` selector text is historical;
the actual producer uses source-trained prices and adaptive Huffman coding.

The bounded audit found little opportunity in headers/stuffing or separate chroma
tables. A joint-triplet candidate saved 570 B but was larger than C133; coupled
nearest rounding added 1,326 B and was stopped. Their raw evidence is retained.

[Numeric evidence](jpeg-source-ac-price-evidence.json) and the
[19-row ledger](JPEG-COMPLETE-PACKET-AUDIT.md) retain the tested scope and producers.
Mirrored raw archives: `c137-qualified-linux.tar.gz` (3,084,754 B; SHA256
`ba30b2c0c858b5d1984366eda4f983ddc655427fdf798f3ea7f962e4ebcae9ed`),
`c137-qualified-windows.tar.gz` (2,745,793 B; SHA256
`178852980dcf99ba747da5b1b5cad98384fbab91dbc17cc562be68c446e65fab`).
