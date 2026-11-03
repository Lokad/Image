# VP8 quantized scoring investigation

The actual quantizer forms zigzag levels before luma4 scoring reloads them for
flatness, magnitudes and nonzero/rate preparation. Stage 1 passes those exact
byproducts directly and borrows the rate view once. Generated code duplicates
the two residual-scoring paths, growing luma4 from 6,685 to 7,590 B. One diagnostic
capture motivates stage 2: both bounded quantizer producers feed one scoring body.
Trellis arithmetic, general-consumer validation, mode order/ties and packets stay
unchanged. This is a revised implementation, not favorable resampling.

| Complete control, three actuals | Before µs / ratio | Stage 1 µs / ratio | Stage 2 µs / ratio | Allocated B/op, all | Packet B, all/reference |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque photograph / FFmpeg | 14,518.59 / 1.4051 | 14,718.57 / 1.4473 | 13,861.96 / 1.4273 | 809,072 | 17,688 |
| Transparent RGBA8 / FFmpeg | 67,393.57 / 1.2083 | 65,889.75 / 1.2340 | 65,073.43 / 1.2170 | 8,622,560 | 98,590 |

Neither stage has a useful normalized point lead. The revised stage's raw lead
tracks a faster native clock. These short screens establish neither a gain nor
a causal regression; no independent confirmation is justified and none is run.
The two-second diagnostic uses the same frozen complete photograph, CPU0 with
sampler CPU1; instrumented elapsed is discarded. Samples/skid are not phase times.

Every original source, exact complete packet/reconstruction, ownership and quiet
gate passes. Full preparation, normalization, independent output and disposal
remain timed. Both stages pass 804 internal / 194 public WebP checks in each normal/
scalar mode on each OS. Four new helper controls use independent scalar quantizer/
rate oracles, signed/threshold/tail guards and zero-allocation checks. No extra
heap, owner or admission; additional vectors fit the existing 16 KiB stack envelope.
Allocation is unchanged; physical/admitted peaks and retention are not remeasured.

Qualified RSP186 source/core/PDB/XML and internal assemblies are restored exactly
on both OSes; obsolete helper-only tests are removed. Reuse 5,529 full/live per OS,
affected scalar and same actual Release-package/three-example qualification.
Two candidate core/internal builds per OS; no worker, native, selector, package,
restoration or broad-suite builds. Current costs and published releases are unchanged.

[Evidence](vp8-quantized-scoring-inconclusive-evidence.json) pins both stages,
all actuals/identities, diagnostic code and restoration. Raw
`vp8-quantized-scoring-inconclusive.tar.xz`: 2,655,844 B / 243 members, SHA256
`a504406c93ad1c9ff328c2502468612f7502250d226a3d88a9c40ccf11beca47`,
mirrored and member-verified. Its manifest pins unchanged reference dependencies.
Materially revised designs remain eligible; publication stays postponed.
