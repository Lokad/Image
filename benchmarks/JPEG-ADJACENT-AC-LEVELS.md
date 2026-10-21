# Adjacent JPEG AC choices

C139 extends the final source-priced 4:4:4 pass with each original AC value
minus/plus one, including initially zero values. These follow the existing
smaller-category choices, skip duplicates and retain stable ties. The geometric
source trainer, Original DQT/DC, source-DCT RGB error cap and lambda stay unchanged.

| Existing 513×259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 149,650 | 149,037 / 133,041 | 0.224518 dB |
| Original 4:4:4, RGBA16 | 149,528 | 148,972 / 133,043 | 0.232120 dB |

Savings are 613/556 B. Both remain within the unchanged 0.25 dB gate, with exact
alpha and decoder agreement (maximum 3, RMSE below 0.230). Actual packets exactly
reproduce the independent C138 candidates on both platforms and normal/scalar
paths. The complete-packet gap remains about 12%; these controls do not establish
photographic, diagram or general JPEG compression claims. Optional CompactColor
remains separate; passing grayscale, 4:2:2 and WebP controls stay stopped.

Candidate levels use 48 stack bytes once per decision call, within the existing
admitted 8 KiB workspace alongside the 2,048 B model and 2,304 B MCU. There is no
new heap or frame copy. Final decisions charge at most 145,536 work units per MCU
(48,512/block); the geometric trainer and its work bound remain unchanged.
Adaptive and async staging estimates include the new bound. Refusal, work
exhaustion and cancellation remain terminal; root, stream and ownership gates pass.

Windows and Linux each pass 4,943 Release tests (2,034 internal, 1,973 public,
936 live FFmpeg), plus 123 internal and 53 public scalar checks. Eight focused
cases cover zero/same-category neighbors, signs, geometric isolation, work refusal
and restoration, cancellation and the exact maximum work bound. Public goldens
cover RGBA/caller samples, fixed/adaptive, live cancellation and async boundaries.
The same actual private 0.2.0 package and all three examples pass on both platforms.
One production and one test build/project/platform are reused; reference/quality
evidence is reused by exact packet identity, with no new reference preparation.

One quiet-VM three-actual forward-only screen records **106,257.837 µs / reference
3,780.694 µs**, ratio 28.105/upper 36.492, **563,924 B/op**. Both idle-v2 guards
pass; native allocations are unmeasured. The previous producer recorded 54,822.079
µs and 563,802 B/op. The substantial CPU cost is accepted for compression under
the current priority; no CPU-parity, speed improvement or confirmation is claimed.
Normalization, source analysis, complete encode and fresh output ownership/release
are included on one CPU. The unchanged harness's `fixed-geometric-v1` selector
text is historical; actual coding uses source-trained prices and adaptive Huffman.

[Numeric evidence](jpeg-adjacent-ac-level-evidence.json) and the
[19-row ledger](JPEG-COMPLETE-PACKET-AUDIT.md) retain scope and tested producers.
Mirrored raw archives: `c139-qualified-linux.tar.gz` (2,766,168 B; SHA256
`eb875ce52e6a146d15372b07e202eb4f8a254505e2c45797e0c655d09f305521`),
`c139-qualified-windows.tar.gz` (2,749,538 B; SHA256
`d106f52bb39f57d394f92bdfacf8852cfcbdab8217f5739f83ae8b3af155130d`).
