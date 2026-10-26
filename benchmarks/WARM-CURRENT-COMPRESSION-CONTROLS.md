# Current photograph and diagram WebP controls

Two existing 513×259 opaque RGBA8 controls reconcile CPU/allocation uncertainty on
the qualified production build. Effort 4, native quality 75 and exact=1 remain
frozen. Managed lossless Quality is ignored; its old value 85 is retained. Complete
packets match the reference byte for byte; equal quality numbers are not the gate.

| Quiet short | Lokad µs | libwebp µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| PhotographLossless8 | 133,724.16 | 82,618.55 | 1.619 / 2.098 | 29,610,520 | 160,032 |
| DiagramLossless8 | 21,976.22 | 9,631.04 | 2.282 / 3.413 | 45,404,827 | 7,234 |

Both idle-v2 guards and all three actual iterations pass per case. These are
current gaps, not independent parity confirmations or measured gains over the
superseded encoders. No timing repeat is justified by these results. B/op measures
managed allocation, independently of peak shared admission and native storage.

The complete public operation includes source processing and fresh MemoryStream
growth, independent packet ownership and release, without a redundant ToArray.
The native libwebp exact=1 reference includes normalization/repacking, import,
encoding and independently owned packets. Preparation, validation, I/O, IPC and
startup are excluded. One CPU per operation; no packet cache or concurrency.

The current harness now uses the original fixture hashes and frozen complete
packet oracles instead of a superseded quality-search catalog tied to an older
build. All four existing smoke controls pass: lossless photograph/diagram
160,032/7,234 B and lossy photograph/diagram 17,688/7,100 B. Normal/scalar native
reconstructed pixels, source preservation and independent output lifetimes pass.
Lossy checks retain the approved normalized VP8 decoder graph.

Production source and selected Windows/Linux binaries are unchanged. The existing
4,922 Release checks and 601 affected scalar checks per OS, same actual private
0.2.0 package and three examples are reused. Only affected benchmark tooling is
built; archived selectors and native workers are reused, preserving the shared
benchmark binary. Historical 7.330×/2.291× rows remain evidence of their own
producers rather than current cost claims.

[Evidence](warm-current-compression-controls-evidence.json) records samples,
allocations, tool/core identities, reused qualification and the mirrored,
member-verified raw archive. The diagram path now warrants one bounded dominant
primitive investigation. The finite 253-case warm upper≤1× objective remains open.
