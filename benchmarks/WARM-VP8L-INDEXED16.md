# Indexed VP8L decoding

Historical timing checkpoint: [borrowed VP8L lookup](WARM-VP8L-BORROWED-GREEN.md)
records the current qualified short measurement. Its selected decode timing is superseded; original packets and boundaries remain.

Complete sixteen-color VP8L decode independently confirms **1.519× FFmpeg /
upper 1.550×**, at **1,351,961 managed B/op**. Production is unchanged.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Short screen | 850.30 | 553.57 | 1.536 / 2.550 | 1,351,968 |
| Independent confirmation | 825.07 | 543.26 | 1.519 / 1.550 | 1,351,961 |

One original 11,810-byte native packet uses the existing PNG palette4 projection
of the original 513×259 byte raster: j=red>>4, i=17*j,
RGBA=(i,255-i,(73*i+19)%256,i). Header inspection pins first transform type3 and
sixteen palette entries, packing two indices into each coded green byte. This
exercises inverse palette expansion before final RGBA packing. No new original
raster, palette-size/depth/settings grid or quality enumeration is added.

The existing public libwebp exact=1 encoder worker authors the fixture outside
timing at method4/quality75, with deterministic packets and simultaneous
independent owners checked. Source/build/version identity and readiness are
retained. It is not rebuilt. Managed and normal/scalar FFmpeg CLI outputs match
the frozen projected RGBA exactly, including alpha and nonzero hidden RGB at
alpha0. The unchanged **FFmpeg software decoder** remains the performance
baseline; libwebp only authors the untimed input.

Both subjects time complete decoding into independently owned RGBA8 and release.
Managed framing, entropy decoding, inverse transforms/palette expansion, packing
and ownership are included. Native state reset, software VP8L decode, ARGB-to-RGBA
conversion and real output allocation/release are timed. Codec/conversion and
frame/packet wrappers are reused, processing every pixel each operation. No
output cache. Startup, IPC, I/O, generation, hashing and validation are excluded.

Sequential Release measurements use CPU0, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned pre/post
quiet guards pass on both runs. The short mean passes; its three-actual upper
does not certify the target. All twelve independent actuals are selected and
retained; the upper includes selected/all-actual intervals against the faster
native bracket's lower bound. Allocations include harness overhead; native
total allocations are unmeasured.

One changed harness build serves both runs, reusing the same verified immutable
production core and native encoder/decoder workers. Existing 3,483-tests-per-OS/
live/same-actual-package qualification is reused without production/native
rebuilds, a profile or repeated suite/package proofs. Frozen preparation cannot
regenerate during measured jobs; reference generation is an explicit command.

[Numeric evidence](warm-vp8l-indexed16-evidence.json) retains every actual,
allocation, guard, original fixture provenance and source/build/pixel identity.
Stop this pair; remaining material/current-source coverage stays open.
