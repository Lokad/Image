# Current opaque VP8 decode confidence

The three existing opaque VP8 decode controls remain **uncertain** on the current
immutable package DLL. They do not establish optimization gaps or parity.

| Complete original 513×259 decode / FFmpeg | Managed / reference µs | Conservative ratio interval | Managed allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Opaque8 | 5,130.47 / 5,201.43 | 0.965–1.006 | 230,880 | Uncertain |
| OpaqueCancelable8 | 5,206.57 / 5,092.25 | 0.972–1.042 | 230,880 | Uncertain |
| SimpleOpaque8 | 5,128.59 / 5,143.61 | 0.983–1.008 | 230,880 | Uncertain |

Opaque8 exercises ordinary filtering; SimpleOpaque8 uses the separate frozen
nonzero simple-filter packet. OpaqueCancelable8 uses the ordinary packet with a
live unsignaled token; native has no equivalent cancellation setting. No image,
shape, quality or filter grid is added. Every managed/native pixel is exact,
including alpha; source, independently surviving outputs, original reference
identities and normal/scalar CLI gates pass unchanged.

Each case has twelve fixed alternating pairs of 16 complete operations, with
twelve native batches before and after. All actuals and the unchanged conservative
worse-of paired/three-bracket confidence rule remain. Original global observations
stay history; different clocks do not establish regression or speedup. No
unchanged favorable retry or production tuning follows these uncertainties.

Public managed Decode includes input parsing, admission, decoding/filtering,
normalization, independent result acquisition and disposal. FFmpeg retains its
declared decoder/frame/conversion contexts and padded input; full reset, decode,
packing and independent output acquisition/release remain timed. CPU0, sequential
Release, optimized JIT, nonconcurrent workstation GC and both idle-v2 guards pass.

One diagnostic build adds only the harness name to the existing adjacent driver's
allowlist. All numerical/confidence loops and the original harness/worker remain
unchanged. One runtime serves all three after their original bindings match.
Production, embedded tests, references and fixtures are reused without builds;
6,600 full/live checks per OS, scalar/ISA and SAME-package consumer/all three
examples are reused. No broad/package qualification campaign occurs.

Allocation stays 230,880 B/op. An independent result owns 531,468 B; allocation,
admitted capacity and retention are separate. No pool/capacity/ownership change
occurs; the two-slot/4 MiB default pixel store and governed bypass remain.
Physical/native/JIT peaks are unmeasured. WebP reading retains nine passing views
and these three uncertainties; all 289 material paths remain required.

[Evidence](vp8-decode-current-confidence-evidence.json) preserves full captures,
frozen inputs, reference identities and qualification reuse. Raw `vp8-decode-current-adjacent.tar.xz`:
378,456 B / 36 members,
SHA256 `e447d9cb4496d58e4991acd9c0bf8d9cdbbf19f0bfb02f0fc142ac8d0bd42cad`, mirrored and member-verified.
Dependencies retain original sources, tools, fixtures and the actual qualified
package. Publication stays postponed.
