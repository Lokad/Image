# Decoder exactness

[The finite checklist and numeric evidence](decoder-exactness-evidence.json) reuse
original controls and pinned references. Exact normalized samples and typed source
metadata are required. Remaining error bounds are transitional functional checks.

| Required path | Current evidence and next action |
|---|---|
| PNG normalization, Adam7, entropy, packed depths and tRNS | Existing frozen/live exact assertions pass; stop at that scope. |
| VP8L transforms, palette, cache and copies | Existing60-fixture/authored/live exact assertions pass; stop at that scope. |
| JPEG inverse and progressive coefficients | Integer primitive and all30 frozen grayscale/YCbCr444/422/420 controls pass; stop passing cases. |
| JPEG444/422/420 YCbCr color | Exact frozen/live samples, odd edges, restarts and progressive scans pass on both OSes. |
| JPEG direct-RGB422/420 | Reuse the two named existing warm packets and approved reference normalization; distinct expansion/caller branches need confirmation. |
| JPEG CMYK/YCCK and411/410 | E106 named CMYK/YCCK RGB/gray controls pass. Varying three-component411/410 remain pending. |
| JPEG caller grayscale/RGB/CMYK | Native Y/RGB444/422/420 and named CMYK/YCCK RGB/gray, padding/orientation/ownership pass. Canonical ink/source/inversion contracts retained. |
| VP8 reconstruction and deblocking | Measure planar reconstruction before changing a primitive. |
| VP8 chroma/RGB and ALPH | RGB exactness uncertain; retain passing exact-alpha controls. |
| Typed source metadata, orientation and delivery | Existing sync/async, caller-buffer and lifetime checks pass; retain them. |

The fixed-point inverse and Q13 color matrix preserve the pinned numerical policy,
including the recorded native C/normal AVX distinction. All30 existing frozen
JPEG grayscale/YCbCr controls require exact samples. Three31x23 anchors cover
arrays, synchronous/asynchronous streams, caller RGB, padding and orientation.
Existing live baseline/progressive/restart/caller controls require exact samples.

E106 adopts the demonstrated CMYK/YCCK integer product and black-before-color
precision for owned RGBA and caller RGB/grayscale. Original constant, varying,
saturated, AC and progressive controls have native pixel gates. The two65x31
varying packets additionally cover grayscale, streams, oriented/padded caller
outputs and retained shared-root lifetime. Canonical CMYK keeps its explicitly
requested ink representation and source/inversion controls.

YCCK uses three stack bytes, with no added owned heap storage or frame copy.
Existing4096 block/32-per-pixel row work, admission, cancellation and ownership
boundaries remain. Superseded floating YCCK output paths and their implementation-
only tests are removed.

Windows and Linux each pass5,027 qualified checks, including942 live comparisons,
plus33 internal/113 public affected scalar checks. Valid1977-check internal results
are reused after a public expectation correction; production/internal-test DLLs
stayed unchanged. The SAME actual private0.2.0 package passes both consumers and
all3 usage examples. Earlier JPEG decode timing/allocation rows describe prior
producers; no new CPU/GC measurement is claimed.

Next, resolve exactness on the named existing direct-RGB422/420 warm controls and
67x19 varying three-component411/410 controls. These are distinct expansion/output
branches. Preserve approved reference normalization/settings and input hashes;
no new workload grid or passing-case retuning. VP8, broader decoder equality,
warm single-CPU parity and replacement readiness remain open.
