# PNG RGBA16 Fastest entropy recoding

Historical implementation, retired by the subsequently qualified
[streaming fast-backend round](WARM-PNG-FAST2.md). Its original results below remain
valid for the retained source and workload; they are not current backend figures.

The complete warm RGBA16 Fastest encode is independently confirmed at **0.787x
FFmpeg / conservative upper 0.791x**, with **5,478,765 managed B/op**. The original
size-gate refusal is resolved: IDAT decreases from 1,023,460 to **974,122 bytes**,
against native **965,318**, a **0.904%** difference within the unchanged 5% gate.
The earlier refusal has no scored timing, so it establishes no speedup ratio.

| Complete operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA16 Fastest, short | 13,906.36 | 17,679.48 | 0.787 / 0.803 | 5,478,765 | Quiet screen |
| RGBA16 Fastest, independent 12 | 13,911.38 | 17,666.27 | 0.787 / 0.791 | 5,478,765 | Confirmed |
| RGBA16 Optimal | 14,704.00 | 20,081.83 | 0.732 / 0.769 | 2,103,624 | Regression screen; prior confirmed |
| RGBA8 Optimal | 5,439.91 | 17,231.30 | 0.316 / 0.353 | 527,627 | Regression screen; prior confirmed |

An independent safe RFC1951 primitive recodes private BCL fixed-Huffman output
into a smaller dynamic block. Two scans retain all **878,280 literals and 32,323
length/distance pairs**, including extra bits, with no token array. Independent
token parsing verifies their exact sequence. Match finding, level1, filter None,
zlib fast-class header and checksum stay unchanged. Unsupported, overlong or
nonsaving streams retain their original payload. No upstream implementation,
unsafe code, dependency or output cache is introduced.

The optional stage admits only bounded, noncancelable RGBA Fastest output on a
stock MemoryStream with complete memory/work/output admission. Its additional
workspace is 2C+32,768 bytes, C=2F+65,536 for filtered F in 64KiB..4MiB. Two scans
charge 16N+131,072 work for compressed N. Insufficient bounds, custom streams,
cancelable calls and insufficient nonexpandable outputs retain original failure
paths. Array capacity limits are checked before mutation. Allocation includes
staging and output capacity; native allocations are **unmeasured**.

Both subjects time complete public original-raster encoding, row/endian packing,
output acquisition and release. Native wrappers are reused. Managed staging and
recoding are timed. Startup, I/O, IPC and validation are excluded. Exact words,
alpha, hidden RGB, CRCs, source preservation and two live owners pass, including
normal/scalar native decoding. Only the original two deterministic 513x259 rasters
and three workload types are used; no compression-setting sweep is involved.

All final runs use the same immutable production/harness and existing generated
drivers; all twelve actuals are retained. Quiet guards pass. Regression means
precede full **3,371 Release tests per OS**, including **898 live comparisons**,
plus 137 focused scalar checks per OS. One actual Release package passes its
consumer and all three examples on Windows and Linux. Measured and qualified
owned inputs are identical.

[Numeric evidence](warm-pngfast16-recode-evidence.json) retains actuals, guards,
identities, source inventories, token proof, package proof and raw archive hashes.
It also retains a superseded candidate and an unscored failed I/O-wait guard.
The [original refusal and compression diagnostic](WARM-PNG-FAST16-REFUSAL.md)
remain historical evidence. Stop this confirmed pair; caller/cancelable/constrained
policies and full material/current-source warm parity remain open.
