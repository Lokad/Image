# Shared VP8 residual preparation

Statistics and final partition emission now share one exact sixteen-level
preparation kernel. Signed vector comparisons validate before counter mutation;
an exact nonzero mask finds the final coefficient, excluding split DC where
required. The scalar fallback preserves the previous scan. Coefficient grammar,
counter rescaling, candidate order, all cancellation checkpoints and complete
packets are unchanged. No per-frame extent table, full-event stream or owner.

One counterbalanced comparison confirms **475.19 µs / 3.80% elapsed improvement**
on the frozen opaque photographic RGBA8 encode, Q75/effort4:
12,491.36→12,016.17 µs. Twelve alternating pairs, sixteen complete operations per
subject, all pairs retained; ratio confidence **0.95691–0.96703**. Immutable
isolated builds own identical resident input. Fresh output construction/growth,
complete public encoding, observation and disposal stay timed; fixture decode,
setup, JIT and validation are excluded equally. Exact 17,688-byte packets,
independent owners and source/producer/quiet guards pass. This is a scoped
incremental managed-build result, not full FFmpeg parity or an alpha gain claim.

| Existing complete 513×259 control | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Managed B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque photograph, effort4 | 12,273.06 | 9,714.77 | 1.263 / 1.276 | 809,072 | 17,688 |
| RGBA8 + ALPH | 61,821.92 | 53,387.59 | 1.158 / 1.168 | 7,165,880 | 98,590 |
| RGBA16 + ALPH / normalization | 62,712.50 | 53,763.68 | 1.166 / 1.182 | 7,165,472 | 98,590 |
| Opaque RGBA8 | 17,090.79 | 15,514.93 | 1.102 / 1.115 | 1,201,552 | 72,380 |
| Opaque text/diagram, effort4 | 6,776.22 | 5,820.65 | 1.164 / 1.182 | 741,137 | 7,100 |
| Diagram, effort2 / distortion | 3,037.56 | 2,505.76 | 1.212 / 1.218 | 741,386 | 7,330 |
| Diagram, effort6 / trellis | 12,979.43 | 9,828.25 | 1.321 / 1.325 | 754,344 | 6,818 |
| Opaque RGBA16 / normalization | 18,397.96 | 15,576.92 | 1.181 / 1.187 | 1,201,552 | 72,380 |

Opaque16's separate original/current comparison saves 894.59 µs
(4.64% raw / 4.52% normalized); native clock
movement is -0.12%. Both filtered/all-actual raw and normalized
confidence separate. This reuses the applicable original twelve-actual capture,
not a paired opaque16 result. The evidence retains every confidence view.

All eight affected costs are current and remain gaps. The refresh supplies costs,
without additional incremental claims about alpha or every setting. VP8L,
PNG/JPEG, decoders, resizing and text/drawing retain unchanged-path evidence;
other affected VP8 boundaries marked H remain historical.

Paired allocation is unchanged at 809,072 B/op. Seven current controls retain
their previous B/op; effort2 is 741,386 versus 741,375 B/op at 80 versus 64 operations
per harness batch. That small unequal-batch difference is not a causal allocation
regression or saving claim. No buffer, cache, pool, owner, retained extent or admitted capacity is
added. Admitted/physical peaks are not remeasured; native allocation is unmeasured.
Earlier [full-block quantization](VP8-FULL-BLOCK-QUANTIZATION.md) and
[ALPH storage reuse](WEBP-ALPHA-COEFFICIENT-STORAGE.md) retain their scoped evidence.

Windows/Linux each pass **5,636 full/live checks** (2,401 internal, 2,287 public,
948 differential) and 849 internal/357 public focused checks in normal/scalar
modes. Two new theory cases exercise every coefficient position, first0/1,
signed range edges, rescaling, tails and refusal before counter mutation against
the actual statistics implementation. Existing signed-domain/native-fragment and
complete-packet coverage remains. Unchanged quantization reuses its AVX512-off
proof. One core/internal build per OS; no public-test, harness, native-worker,
selector or paired-tool rebuild. The same actual private 0.2.1 Release package
passes its consumer and all three examples on both OSes. Published packages stay
unchanged; publication is postponed.

Reproduce with Release/differential checks, affected scalar checks and the actual
package consumer. Retained `vrs298-*.py` scripts pin original/selected modules,
existing controls/selectors/workers, all actuals/brackets/guards and paired inputs.
Use the dedicated quiet single-CPU VM. [Evidence](vp8-residual-preparation-evidence.json)
contains identities, all gates, confidence, qualification and raw manifests.

Raw `vp8-residual-preparation-qualified.tar.xz`: 5,989,600 B / 314 members,
SHA256 `6b33714fbe4c8bd0a1ae6cbd59a32c0881205ce59ace99f88ac8c97d04c2ccb1`. Original/selected sources/modules,
tests, paired driver, actual package and all full/scalar/package logs are retained
and member-verified locally and on the VM. Dependencies preserve unchanged
framework, workers, controls and qualified source history.
