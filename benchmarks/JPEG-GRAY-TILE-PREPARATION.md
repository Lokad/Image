# Grayscale JPEG tile preparation

JGT702 prepares an8-row/64-pixel grayscale tile, sharing row acquisition and
normalization setup across up to eight adjacent MCUs. The existing normalization,
separate binary64 grayscale operations, transform, quantization, MCU order,
histograms and entropy replay remain exact. There is one actual block-processing
body, with no new SIMD normalizer, owner, pool, session or public API.

| Existing complete control | Original µs | Current µs | SIMD libjpeg-turbo µs | Ratio / upper | Managed B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| RGBA8→grayscale JPEG | 1,460.11 | 1,160.95 | 1,403.16 | 0.827 / 0.840 | 155,435 | 58,910 |
| Caller GRAY8 | — | 1,059.14 | 1,112.88 | 0.952 / 0.969 | 155,438 | 58,910 |

The independent reversed original/candidate pair saves **299.16µs /20.49% raw /
19.96% reference-normalized**, with separated filtered and all-actual confidence.
Native movement is−0.66%. The three-actual screen and twelve-actual confirmation
agree. Caller-GRAY8 remains a passing regression control; no causal companion gain
is claimed. Both passing controls stop, with no favorable repeat.

The frozen513×259 input, quality69, background0x204060 and complete58910B packet
SHA256 `444e39e6126dd3077d22c0b4f60e38d8a22611fa5721b226c5168a63b7e94423`
are unchanged. This is exact compression, not quality-number matching. All
normalization, encoding, fresh output acquisition/ownership and release stay
timed. SIMD-enabled standalone libjpeg-turbo3.1.4.1 is the labeled approved tooling
reference, with its existing retained codec/gray-scratch boundary. The intended
Windows package DLL supplies costs on the quiet Linux VM; Linux-native production
is separately qualified. No reference, selector, harness or fixture build occurs.

Prefetching runs only in the recorded image-input pass, whose data stays private
until the complete pass succeeds and the header is written. Tight/restart
recomputation and caller samples retain their original preparation. Per-MCU
cancellation remains, with another checkpoint before each bounded tile row.
Invalid-alpha failures preserve the borrowed stream prefix, source and shared-root
cleanup; subsequent valid operations succeed. Right/bottom padding remains exact.

The tile512B and packed-RGB256B add **768B stack storage** within the already
admitted8KiB fixed workspace. A conservative source bound counts1,988B of explicit
stack arrays, including exclusive/ended callees together, plus2,816B of fixed
heap/reference/metadata allowance:4,804B. This is not a JIT-stack or physical peak.
Entropy-record capacity keeps its own charge; replay storage begins after the
tile pass ends. No heap capacity or retained tile storage is added. Total managed
allocation is separate; its1B rounded difference is not an allocation-gain claim.
Full admitted/physical peaks are not remeasured; existing coefficient pools and
their governed bypass are unchanged.

Nine new controls compare tiled8/16-bit samples with the unchanged qualified block
path across edges and tile boundaries, guard borrowed storage, verify zero warm
allocation/existing admission, and check cancellation, failure cleanup and recovery.
Existing frozen packets, all-RGB rounding and decoder/editing contracts remain.
Windows/Linux each pass **6,073 full/live checks**, plus444internal/375public per
normal/scalar/disabled-AVX2/disabled-AVX512 mode. The SAME actual private Release
package and all three examples pass both OSes. One production build per OS serves
the round; a test-only scratch-guard correction requires a second Windows embedded
build. Initial logs and the corrected expectation are retained, with no production
rebuild or repeated timing.

Only two gray costs change. Fifteen unchanged color JPEG costs retain
[prepared Huffman evidence](JPEG-PREPARED-HUFFMAN-BANK.md), giving16pass/0gap/
1uncertain across those17 boundaries. No broad content, depth or release-refresh
performance claim follows. The full283-path objective remains open; publication
is postponed.

[Evidence](jpeg-gray-tile-preparation-evidence.json) pins sources, confidence,
ownership and package. Raw `jpeg-gray-tile-preparation-qualified.tar.xz` contains
5,938,608B /146members, SHA256
`5355192b13417330513082d3f8d06dee70ac3bf208abaeab9b42c8255b29069d`,
mirrored and member-verified. The actual private package is438,804B, SHA256
`05074b8f42120252df42a06ae4243a0687c203f53945b8e9a8237720da74154f`.
