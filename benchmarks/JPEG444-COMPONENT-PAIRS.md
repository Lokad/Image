# JPEG444 adjacent component pairs

C150 adds one bounded component-pair sweep after the existing source-constrained
AC trellis. A simultaneous pair can improve coupled RGB error/rate where separate
moves cannot. The bank is current/minus-one/plus-one, visited in stable order for
Y/Cb, Y/Cr and Cb/Cr at frequencies 63..1. Accepted moves strictly improve the
existing objective, never increase combined estimated AC price and retain the
original-nearest MCU error cap. Source price training, lambda, DQT, DC and four
Huffman tables stay unchanged. No reference image or quality search enters the codec.

| Existing 513×259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 142,580 | 141,528 / 133,041 | 0.215790 dB |
| Original 4:4:4, RGBA16 | 142,544 | 141,529 / 133,043 | 0.220860 dB |

Savings are 1,052/1,015 B. Both controls pass the unchanged 0.25 dB matched-quality
gate, exact alpha and decoder agreement (maximum 3, RMSE 0.227378/0.225911).
Source PSNR decreases 0.026858/0.025594 dB from C142 while remaining inside that
band; this is a compression gain at the declared matched-quality tolerance,
without a quality-improvement claim. Inputs, managed quality 69/native qscale 5,
sampling and 0x204060 flattening remain frozen. Numerical quality settings do not
establish equivalence. The complete-packet gap remains about 6.4%. These controls
make no photographic, diagram or general JPEG compression claim. Optional
CompactColor stays separate; all passing 4:2:0/4:2:2/gray/WebP controls stay stopped.

Actual packets exactly reproduce the independent C149 candidates on both
platforms and normal/scalar paths. All five affected ledger boundaries agree:
RGBA8/16, forward-only, live cancellation and async RGBA. Public goldens also
cover caller samples, fixed/adaptive entropy, async samples, the fixed 8 KiB shared
root, terminal refusal, successful subsequent use and independent output lifetime.
The source prototype independently reproduces all 411,840 original coefficients
per depth and replays each final scan exactly.

The tail keeps three prices (12 safe stack bytes), with no heap matrix, frame
copy or cache. Its scratch follows the preceding DAG call within the same
admitted 8 KiB. Work is bounded by 196,002 per MCU:384 error setup,192 price setup,
then 63×3 pairs×(8 setup+8×128 trial+2 commit). Combined decisions cost at most
863,442 per MCU or 287,814/block. Adaptive/async estimates include the bound;
the existing control's planning estimate 1,893,600,773 fits the unchanged default
2,000,000,000 units. Trials only read coefficients; checked commit cannot leak an
unselected pair after refusal or cancellation. Shared admission and asynchronous
ownership contracts remain intact.

Each platform passes 4,979 Release tests (2,066 internal,1,977 public,936 live
FFmpeg), plus 155 internal and 57 public scalar checks. Fourteen focused pair tests
cover coupled moves, amplitude limits, exact work bounds, refusal before commit,
retained selected pairs, cancellation and subsequent success. The same actual
private 0.2.0 package and all three examples pass on Windows/Linux. One core and
test build/project/platform are frozen throughout qualification. C149 quality
evidence is reused by exact packet identity; no benchmark/native/selector/differential
build, reference encode or quality search is repeated.

One quiet-VM three-actual forward444 screen records **1,282,590.681 µs / reference
3,766.823 µs**, ratio 340.497/upper 366.211, **564,192 B/op**. Both idle-v2 guards pass;
native allocations are unmeasured. The previous C142 screen was 796,455.405 µs
with the same allocations. This CPU increase is accepted for the verified byte
saving under compression-first ordering. No CPU-parity, speed-improvement or
independent confirmation is claimed. Full normalization, source analysis, encode,
fresh forward-only wrapper/output growth and ownership/release are timed on one
CPU. The unchanged compiled `fixed-geometric-v1` selector is historical tooling
text; current production uses source-trained prices, trellis/pair decisions and
admitted adaptive Huffman coding. The unaffected caller420 cost retains C146's producer.

[Numeric evidence](jpeg444-component-pair-evidence.json) and the
[19-row ledger](JPEG-COMPLETE-PACKET-AUDIT.md) retain scope and tested producers.
Five default444 size gates and two PNG gates remain open; full compression,
eventual CPU parity and replacement readiness are not complete.

Mirrored raw archives: `c150-qualified-linux.tar.gz` (2,473,243 B; SHA256
`055786e2b141cbd8d1d7cb9675bc371955f28c341f78e57e3ea176fc1bc31739`),
`c150-qualified-windows.tar.gz` (2,463,059 B; SHA256
`34a63b2480b381019ccaf7f939fbaf188a84d8575264600c4dc1564b6d8022f3`).
