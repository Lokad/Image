# Warm nearest resize baseline

The first complete nearest-downsample screen reaches 1.3121x
FFmpeg, conservative upper 1.4969x. No production
optimization is needed for this case. Independent quiet confirmation, other
directions/depths and bilinear/bicubic coverage remain open.

The [numeric evidence](warm-nearest-baseline-evidence.json) retains raw samples,
uncertainty and source/output/tool identities. Regenerate with
`python benchmarks/render-warm-nearest-baseline.py`.

| Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| ---: | ---: | ---: | ---: |
| 313.08 | 238.62 / 239.16 | 1.3121x | 1.4969x |

`Nearest/Down8` uses the same one original 513x259 RGBA8 raster and one 256x129
output. Both subjects reproduce every original selected sample, including alpha
and 68 zero-alpha pixels with nonzero hidden RGB. Independent integer center
coordinates use `(2*destination+1)*sourceSize/(2*targetSize)`. Source integrity and
two simultaneously live outputs are checked, including mutation of one output
while the other and source remain exact.

The original development-only adapter calls FFmpeg's public filter APIs. It
includes packed/planar conversion, plane extraction, separate RGB and replicated
alpha zscale point kernels, merging, output pixel acquisition and release. The
native graph, immutable source, frame wrappers and geometry contexts are reused;
each call filters every selected sample. Native padded rows are exposed directly.
Managed timing covers the complete public Resize call, admission, allocation,
materialization and disposal. Startup, IPC, I/O, hashing, generation, independent
references and validation are excluded. Neither subject caches output pixels.

The pinned CLI reproduces every output byte. A direct packed swscale-neighbor
CLI check differs in 16,368 of 132,096 bytes on this profile; its timing is refused.
The selected zscale route matches the declared coordinates and hidden RGB.
Fourteen negative controls cover affinity, operation, geometry, precision,
input framing and bounded protocol. Native counters and independent live owners
also pass. The nearest BDN smoke and a compatibility JPEG decode smoke pass.
Existing file/process nearest comparisons supply correctness recipes but do not
establish these warm operation boundaries.

Production methods/flags are unchanged and match the retained actual package in
the saved statistical child and fresh Windows build. Thirty existing original
JPEG outputs remain exact as correctness controls, not extra benchmark images.
The same production already passed all 2,113 tests, and the retained actual
package passed all three usage examples on both OSes. No production change warrants a repeated full suite or
package examples in this benchmark-only round.

One short screen runs sequentially in Release on CPU0, one filter thread, normal
SIMD, disabled tiering and non-concurrent workstation GC on the dedicated VM.
Prequiet passes and postquiet fails under unchanged thresholds; observations and
later cooldowns remain. This case adds material warm coverage, without certifying
uniform parity or the uncovered resize paths.
