# VP8 search termination at zero error

The first prediction with exact zero squared error is optimal. Returning that
mode preserves the existing earliest-tie rule and its already written prediction,
while skipping later candidates and the redundant final rewrite. Diagram encode
time improves **10.8%**, from6,442.53 to **5,744.66µs**, independently reaching
**0.984x / conservative upper0.991x** the reference. All encoded packets and
quality results remain unchanged; this is a time improvement, not a size claim.

| Complete VP8 encode | Managed / reference µs | Ratio / upper | Managed allocated B/op | Managed / reference bytes | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Photograph RGBA8 | 7,521.25 / 9,720.38 | 0.774 / 0.778 | 541,256 | 21,826 / 17,688 | Independent parity |
| Text/diagram RGBA8 | 5,744.66 / 5,839.81 | 0.984 / 0.991 | 490,812 | 8,958 / 7,100 | Independent parity |
| Original alpha RGBA8 | 22,670.74 / 53,618.15 | 0.423 / 0.425 | 2,616,016 | 471,436 / 98,590 | Independent parity |
| Original alpha RGBA16 | 23,330.59 / 53,790.68 | 0.434 / 0.437 | 2,615,835 | 471,436 / 98,590 | Independent parity |
| Original opaque RGBA8 | 22,160.39 / 15,568.56 | 1.423 / 1.521 | 2,536,280 | 333,580 / 72,380 | Short gap |
| Original opaque RGBA16 | 22,889.11 / 15,634.31 | 1.464 / 1.599 | 2,536,099 | 333,580 / 72,380 | Short gap |

Both luma and chroma searches stop only at exact integer zero. Candidate order,
mode, reconstructed pixels and packets are identical. Every performed trial
retains its original work/cancellation charge; skipped trials consume no work.
No new heap state, table, buffer or admission requirement is introduced. Measured
allocation includes the complete output owner and harness, with raw-GC rounding.
The remaining five controls show no material time improvement; the two opaque
gaps remain open. Lossy compressed-size/rate-distortion work also remains open.

Use the same two original513×259 rasters/opacity projections and the two fixed
[photograph/diagram controls](Fixtures/Compression/manifest.json), with no new
settings or workload grid. Existing FFmpeg libwebp workers, FF58/7dd harnesses,
prepared short/confirmation drivers and one verified immutable candidate are
reused. No native/harness/driver build or new profile. Managed settings remain100
for the original controls,78 for the photograph,75 for the diagram; native75 and
effort4 remain fixed. Packet hashes, source-error/decoder/alpha metrics and native
build identities reproduce the frozen anchors exactly. No quality retuning or
assumption that equal quality numbers are equivalent.

Complete public operations include admission, all required source normalization/
conversion, encoding, fresh MemoryStream growth/output ownership and disposal.
Native normalization/repacking/import/encode/packet ownership are included.
Wrappers and released scratch may be reused, but no result is cached. Startup,
I/O, IPC, preparation, validation and hashing are outside both timers. Native
allocations are unmeasured, not zero. One pinned CPU, normal SIMD, tiering off,
nonconcurrent workstation GC; EPYC9V45, Ubuntu24.04.4, .NET10.0.8/SDK10.0.204,
FFmpeg n8.1 9047fa1b084f.

Six3actual screens precede four independent12actual confirmations, only for
promising cases. All66 actuals/outliers, native pre/post50ms brackets and both
idle-v2 guards remain. The conservative upper is max(selected/all-actual bound)
against the faster native bracket's lower bound. A completed opaque screen was
reused after correcting its older receipt field names in post-processing; no
timed run was repeated. [Numeric evidence](warm-vp8-zero-error-evidence.json)
retains exact statistics, allocation, ownership, packets, guards and identities.

All **4,161 tests/OS** (1,412 internal,1,823 public,926 liveFFmpeg), **179 affected
hardware-off checks/OS** (147 public,32 internal), and the **same actual Windows
private package** consumer/all three examples pass on Windows/Linux. Production
stays safe managed dependency-free .NET10. Lossless encoding, other codecs and
decoding/Identify, inspection, edits, drawing and text are unchanged; their valid
evidence is reused, including all five latest lossless passing proofs. Stop the
four VP8 passing cases until a relevant production change. Full parity is open.

Linux producer31859b5843f07a9bbb86154b6f08abf160bfc92188962fed3c95253426a02eab;
Windowsc1199becfe6716c12d268a97444c34c7ccf460864fb2754656095e5d37e02217.
Private package9a3b11d4c82e248c2ba4c04ddf52efbfb5555c80e274d15cee40b0b3bc898acb.
The first screen uses sourcecb05aae935747a844bf7af38a4975c3a9b8ef8b096d4d73188241e75478205cb;
subsequent runs0c489809a58a550f8a254b618ec96a079877beec687c478bb858d536b76be187,
2406files. Only SPEC's explanation of skipped work changed; production/harness
bytes remained identical throughout. Later edits are reporting only.

Raw archives on both hosts: `h9e-measurements.tar.gz`
31c56cb7cb1973290f21beadbe94e34261c51aa9873934e353d7e25bd6cf3075;
`h9e-qualified-windows.tar.gz`
794bb9bc0d6222190a3c9da4139b5cc6f51c11a3488d65a4b4089021b8de2de8;
`h9e-qualified-linux.tar.gz`
047b965d1576c4d013b87da7677d3b1c07df2231062c48c86a48f6a95d3d833d.
The published0.1.0 package/tag and original fixtures remain unchanged.
