# Warm still WebP lossless decode

One frozen VP8L input from the existing original 513x259 RGBA8 raster
establishes a missing warm decode comparison. No production code changes
in this round. This path exceeds the 2x target and becomes the largest
measured gap; uniform parity remains open.

The [original numeric evidence](warm-webp-decode-evidence.json) retains
actual managed samples, native brackets, uncertainty, allocations, source
and binary identities, output checks, controls and quietness observations.
Regenerate with `python benchmarks/render-warm-webp-decode.py`.

| Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper | Allocated bytes/op |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 16520.21 | 2147.51 / 2148.84 | 7.69x | 10.86x | 1,069,577 |
| 2 | 16023.08 | 2151.31 / 2150.33 | 7.45x | 8.28x | 1,069,577 |

Measured 2026-10-10 UTC, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec and conversion thread, normal SIMD, tiering disabled and
non-concurrent workstation GC. Managed mean spread across the two screens is
3.103%.

The frozen input is 533,100 bytes; exact RGBA output is 531,468 bytes.
It is prepared once by the original Lokad.Image lossless encoder at effort 4.
No new raster or quality enumeration is used. This literal/run VP8L profile
has no transforms, color cache or spatial entropy groups, as defined by the
[public lossless specification](https://developers.google.com/speed/webp/docs/webp_lossless_bitstream_specification).
These results do not cover other VP8L strategies or lossy VP8 decode.

Both subjects receive identical resident bytes. Managed timing covers the
complete public Decode call, independent output acquisition and disposal.
Native timing covers software webp decoding, state reset, ARGB-to-RGBA
conversion, independent owned output allocation and release. Codec/conversion
contexts and packet/frame wrappers are reused; every call decodes and converts
every pixel. Native padded rows need no extra packing copy. Startup, IPC,
I/O, encoding, hashing and validation remain outside both timing boundaries.

All RGBA bytes, alpha and zero-alpha hidden RGB match the original source
and public FFmpeg CLI. Setup and cleanup verify source integrity and
independent live managed/native outputs. Twenty native negative controls
cover affinity, arguments, protocol, dimensions and container framing.
The JPEG decode smoke and eleven unmatched-VP8 preparation controls pass.

Windows offline tests pass 1,036 cases. Linux passes all 1,930 cases:
350 internal, 686 public and 894 live FFmpeg comparisons. Production source
bytes are unchanged. The existing actual dependency-free Release package
is reverified on Linux, including all three usage-guide examples. Its
production method-body fingerprint matches the actual measured DLL; both
decode this frozen VP8L input to every original RGBA byte. Package SHA256:
`5746329831d92e68a5e8e8d40de9f62dadd39f3a03b072e21313a19da313e695`.

Failed quietness samples and later passing cooldowns are retained. Short-run
uncertainty stays visible; this report does not certify other paths or
future hypervisor isolation. Reproduce sequentially on the quiet VM using
the [worker instructions](../tools/WarmReference/README.md#still-vp8l-webp-decode).
