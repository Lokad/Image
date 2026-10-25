# Current PNG SmallestSize8 cost

The existing513×259 RGBA8 control keeps SmallestSize9, nativelevel9 and filterNone.
Complete public encoding includes fresh MemoryStream creation/growth, output
ownership and release. Both subjects run on one CPU; startup and validation are
excluded. All pixels, alpha and hiddenRGB remain exact; independent outputs and
unchanged source are checked. Complete PNG packets are identical:210,973B,
SHA256`4d8f15d9fdb8f415544b831c8f0845c1532adc8696f2707264bced25fc424c3c`.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Short | 25,360.59 | 17,299.67 | 1.466/1.601 | 1,325,678 |

All three actual/selected iterations and both idle-v2 guards pass. This resolves
one current-producer uncertainty; no independent confirmation or speedup claim
against the superseded BCL encoder follows. VP8 remains the larger measured gap.

The accepted build, benchmark, native worker and fixtures are reused, with an
archived host verified to bind this exact case. No rebuild or new control was
needed. E249's5,133 checks per platform and same actual Release package/three
usage examples remain valid. The full257-case/1× objective remains open.
See [evidence](warm-current-png-smallest8-evidence.json) for receipts and reuse.
