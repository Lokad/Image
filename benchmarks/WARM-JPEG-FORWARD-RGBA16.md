# Forward-only JPEG from RGBA16

The complete forward-only RGBA16 encode independently confirms **1.948× FFmpeg /
conservative upper 1.954×**, with **263,812 allocated B/op**. Production is
unchanged from the [fixed geometric AC round](WARM-JPEG-FIXED-GEOMETRIC.md).
This distinct public boundary clears the target; uniform current-source coverage
remains open.

| Measurement | Lokad.Image µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Three-actual screen | 3,553.03 | 1.936 / 2.106 | 263,812 |
| Independent confirmation | 3,572.45 | 1.948 / 1.954 | 263,812 |

All twelve confirmation actuals are retained and selected. Both versioned quiet
guards pass for both runs. The conservative bound includes filtered/all-actual
uncertainty against the fastest native bracket's lower bound. The explicit
`Encode420Stream16` case, depth-specific recipe and separate anchor select the
actual word source and forward-only path.

The existing authored 513×259 RGBA16 raster, quality 69/native qscale 5 and
background RGB 0x204060 are fixed. Full 16-bit alpha flattening and normalization
to eight bits execute inside every operation on both subjects. An independent
entropy recoder preserves every quantized coefficient and amplitude from the
original fixed packet; the integrated **81,275-byte** geometric packet equals
that reference byte for byte. Public/native decoded hashes and the **0.105942 dB**
source-quality gap remain unchanged within the existing 0.25 dB gate. Alpha,
geometry, source and simultaneously held independent outputs pass. No additional
original raster or quality sweep is introduced. This is source-quality timing;
compression efficiency parity is not established.

Each operation creates a fresh forward-only Stream wrapper and fresh owned
MemoryStream storage, including wrapper creation/disposal, storage growth,
complete conversion/encoding and release. Native normalization, flattening,
conversion, encode and independent packet acquisition/release are timed with
prepared wrappers reused. Sequential Release runs use one pinned CPU, one native
codec/conversion thread, normal SIMD, tiering off and workstation nonconcurrent
GC. Startup, IPC, I/O, preparation and checks are excluded. Native allocation is
unmeasured; managed figures include harness overhead.

The same immutable production/harness serves screen and confirmation. Only the
case-selection harness and generated drivers build; verified assemblies are
preloaded before correctness-only preparation. Retained source inventories prove
the two harness files are the only changed build inputs, and the timed DLL is
the previously qualified DLL. Its **3,281 tests per OS**, **896 live comparisons
per OS**, and consumer/three examples through the **same actual dependency-free
Release package** remain authoritative. No full suite, package proof or method
audit is repeated. The [numeric evidence](warm-jpeg-stream16-evidence.json)
retains both runs, the coefficient proof and reused qualification identities.
Stop this fixed pair; remaining material public/codec/current-source coverage
stays open.
