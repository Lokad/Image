# Full-block VP8 quantization

The ordinary quantization boundary now processes one complete sixteen-coefficient
block with AVX512F. It removes the two-half matrix loads, repeated widened
arithmetic and lane reassembly. Q17 products, signs, thresholds, level cap, raster
reconstruction and diagonal levels remain exact. The diagonal constant is generated
from existing RFC6386 scan geometry. AVX2 and scalar paths are unchanged.

One counterbalanced comparison confirms **733.52 µs / 5.43% elapsed improvement**
on the frozen opaque photographic RGBA8 encode, Q75/effort4:
13,517.56→12,784.05 µs. Twelve alternating pairs, sixteen complete operations per
subject, all pairs retained; ratio confidence **0.94357–0.94790**. Isolated immutable
builds own identical resident input. Fresh output construction/growth, complete
public encoding, output observation and disposal stay timed; fixture decode,
setup, JIT and validation are excluded equally. Exact 17,688-byte packets,
independent owners and source/producer/quiet guards pass. This is an incremental
managed-build comparison, not full FFmpeg parity or an alpha CPU-gain claim.

The photograph short has a 12.20% raw point lead against older evidence while its
native bracket is 7.64% faster. The paired experiment resolves that clock mismatch
without a before/after sampling cascade. Alpha's short is nearly flat. All actuals,
adverse points and confidence views are retained.

| Existing complete 513×259 control | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Managed B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque photograph, effort4 | 12,495.75 | 9,711.95 | 1.287 / 1.295 | 809,072 | 17,688 |
| RGBA8 + ALPH | 62,978.73 | 53,594.45 | 1.175 / 1.185 | 7,165,880 | 98,590 |
| RGBA16 + ALPH / normalization | 63,809.97 | 53,547.84 | 1.192 / 1.200 | 7,165,472 | 98,590 |
| Opaque RGBA8 | 18,282.69 | 15,499.96 | 1.180 / 1.186 | 1,201,552 | 72,380 |
| Opaque text/diagram, effort4 | 6,988.34 | 5,816.29 | 1.202 / 1.208 | 741,137 | 7,100 |
| Diagram, effort2 / distortion | 3,247.97 | 2,514.89 | 1.291 / 1.297 | 741,375 | 7,330 |
| Diagram, effort6 / trellis | 13,227.51 | 9,824.29 | 1.346 / 1.349 | 754,344 | 6,818 |
| Opaque RGBA16 / normalization | 19,292.55 | 15,595.81 | 1.237 / 1.242 | 1,201,552 | 72,380 |

All eight affected costs are refreshed once on the selected immutable build;
all remain gaps. Ordinary quantization also serves trellis chroma/Walsh and
distortion-only selection. This refresh supplies current costs, without additional
incremental-gain claims. VP8L, PNG/JPEG, decoders, resizing and text/drawing reuse
unchanged-path evidence; other affected VP8 boundaries marked H remain historical.

Per-operation allocation is unchanged, including 809,072 B/op in every paired
subject. No operand array, pool, cache, owner or admitted capacity is added. One
64-byte immutable format-geometry vector is minor static storage, distinct from
operation-owned capacity. Admitted and physical peaks are not remeasured; native
allocation is unmeasured. Earlier [ALPH capacity reuse](WEBP-ALPHA-COEFFICIENT-STORAGE.md)
and [packed reconstruction](VP8-PACKED-RECONSTRUCTION.md) remain linked history.

Windows and Linux each pass **5,634 full/live checks** (2,399 internal, 2,287 public,
948 differential), 847 internal/357 public focused checks in normal and scalar
modes, and eleven direct quantization checks with AVX512 disabled. Existing tests
cover the actual kernel's coefficient/int32 domain, native fragments, sign/rounding,
tails and zero allocation. One core/internal build per OS; no public-test, harness,
native-worker or selector rebuild. The same actual private 0.2.1 Release package
passes its consumer and all three examples on both OSes. Published packages remain
unchanged; publication is postponed.

Reproduce the selected source with Release tests/differential checks, affected
scalar/AVX512-disabled checks and the actual-package consumer. The retained
`vqf295-*.py` scripts pin original/selected modules, frozen existing selectors,
workers, inputs and outputs. The reusable private paired driver accepts explicit
producer and packet identities and batch size; it is not a production dependency.
Use the dedicated quiet single-CPU VM; profiled/local times are not performance
proof. [Machine-readable evidence](vp8-full-block-quantization-evidence.json)
contains identities, all eight costs, exact gates, qualification and raw manifests.

Raw `vp8-full-block-quantization-qualified.tar.xz`: 5,548,916 B, 302 members, SHA256
`02129e641cb197971c3ade5232f06cd6862c14c520f2e4bffd978ab4efb71ef6`. Original/selected modules, changed source, all
actuals/brackets/guards, paired driver, fallback/full/package logs and the actual
package are retained and member-verified locally and on the VM. Dependency
archives in the evidence retain unchanged framework, workers, controls and source
history.
