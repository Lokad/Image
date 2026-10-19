# Adobe CMYK JPEG decode

The named `JpegDecode/CmykAdobeResident` case is the largest demonstrated warm
parity gap. It fills one missing measurement, using unchanged qualified production.

| Complete public operation | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op | RGBA bytes, both |
| --- | ---: | ---: | ---: | ---: |
| Resident Adobe CMYK baseline JPEG decode | 2,754.84 / 378.20 | 7.284 / 7.565 | 1,635,074 | 531,468 |

The513×259 input is an original CMYK projection of the existing RGBA8 raster,
flattened over0x204060. Device-CMYK samples are stored Adobe-inverted; each coded
8×8 block uses its component's clamped mean, rounded ties-up. The authored T.81
baseline packet has unit quantizers, DC-only coefficients, four full-resolution
planes and Adobe transform0. It is10,910bytes. This tests the distinct four-plane
color/black path without a second image or sampling/quality grid; it does not
extrapolate performance to arbitrary CMYK photographs or progressive coding.
Shared entropy/IDCT paths retain their separate named representatives.

Expected canonical CMYK and opaque RGBA are computed independently from the
component means. Default Adobe inversion and explicit non-inversion controls
pass; managed RGBA is byte-exact. Native software MJPEG applies K/device-color
conversion during decode and exposes GBRAP with opaque alpha. The timed adapter
packs that into fresh zeroed owned RGBA, releasing decoder planes and output
inside every operation. No black/color step or packing copy is omitted. Native
input padding and empty codec/frame/conversion contexts are reused; decoded
content is never cached. Startup, projection/fixture construction, I/O, IPC and
checks are outside timing. Both complete operations use the same resident packet,
one CPU and independently surviving outputs. ICC conversion is off and EXIF
reorientation remains opt-in. K is black, never alpha.

Normal native pixels exactly match public FFmpeg CLI output. The existing normal/
scalar JPEG maximum4/RMSE1.5 gate passes: normal maximum1/RMSE0.71044, exact alpha.
Input stability and two live outputs, including mutation/release of another owner,
pass. All three actuals, pre/post50ms native brackets and both idle-v2 guards pass.
The7.284x mean needs no long confirmation. Next optimize full-resolution CMYK
conversion, preserving exact device arithmetic, admission and ownership.

One narrow harness and one separately named native adapter are compiled. The
existing generic short selector, qualified production and pinned FFmpeg libraries
are reused. No older production evidence is invalidated. Full4,176tests/OS,
73affected hardware-off checks/OS and the same actual private Windows package
consumer/all three examples on both OS remain valid from
[the accepted RGB420 round](WARM-JPEG-CALLER-RGB420.md); no repeated package proof.
Current219numeric rows contain70passes/148gaps/one confidence-open;24named paths
remain unmeasured. Full warm parity is open; the70passing paths stay stopped.
[Numeric evidence](warm-jpeg-cmyk-adobe-evidence.json) retains the fixture recipe,
hashes, quality/ownership receipts, allocations, timings, guards and identities.
Raw archive retained on both hosts: `h10h-measurements.tar.gz`
`0f404a564d5d4f5b7b6869c80111a448da3ddf56ce2abe256fea09cb6981f8f0`.
