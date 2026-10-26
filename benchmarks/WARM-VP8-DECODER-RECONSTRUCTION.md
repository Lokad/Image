# Fused VP8 decoder reconstruction

One existing 513×259 VP8/ALPH control decodes a frozen 98,590-byte packet from a
fresh seekable Stream to independently owned RGBA8. Complete input buffering,
copies, EOF reading, decoding, chroma expansion, exact alpha and output release
are timed on one CPU for both subjects.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Short | 10,051.04 | 5,991.99 | 1.677/1.750 | 1,966,591 |
| Independent | 10,046.91 | 5,991.89 | 1.677/1.684 | 1,966,591 |

Independent confirmation uses 4.53% less elapsed than the retained prior short.
All 12 actual/11 selected iterations and both idle-v2 guards pass. Allocations
are unchanged; the 1× objective remains open. Managed/native pixels are identical:
SHA256 `55cdb9d8ce5e4dbaf16967129b299f6f900aa1c2fa8dad554d26d91b02295dcd`.

The decoder reuses the qualified fused inverse-transform/add/clamp helper.
Its arithmetic is unchanged; residual scratch is dead after each block while the
separate Y2 DCs remain live. Work, cancellation, admission, ownership and the
8,192-byte fixed workspace are unchanged; no owned storage is added.

Both platforms pass 5,133 checks, including 947 live comparisons, plus affected
normal/scalar controls. The same actual private Release package and all three
usage examples pass both platforms. A stale Windows consumer DLL was caught
before acceptance; only affected external checks were repeated, retaining valid
internal results. Final verification reused the package without rebuilding.
No benchmark, native worker, host or fixture rebuild was needed.

Other VP8 decoder CPU results require confirmation; unchanged paths retain their
applicable evidence. See [evidence](warm-vp8-decoder-reconstruction-evidence.json).
