# Current resident JPEG and WebP Identify

Both existing named controls pass independently:JPEG **0.420×/upper0.426** and
WebP **0.00436×/upper0.00483**, each96B/op. This qualifies current complete metadata
inspection on these two frozen packets; it is not a production speedup claim.

| Existing control | Lokad µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Jpeg8, short | 0.501412 | 1.185941 | 0.422796 / 0.440621 | 96 |
| Jpeg8, confirm | 0.501202 | 1.192712 | 0.420220 / 0.425530 | 96 |
| Webp8, short | 0.021812 | 4.936416 | 0.004419 / 0.004993 | 96 |
| Webp8, confirm | 0.021757 | 4.984580 | 0.004365 / 0.004825 | 96 |

Inputs remain original513×259:52093B JPEG420 and533100B VP8L+alpha. All nine
metadata fields agree, including encoded source color/alpha/components. Each
codec's existing21same-packet controls passes with managed intrinsics on/off.
Opaque entropy/payload remains accepted without pixel decoding; changed width,
strict EXIF, source integrity, exact admission/cancellation and independent
surviving descriptors retain their gates. No new image/settings grid.

JPEG uses the approved original FFmpeg MJPEG discard-all adapter and timed17B
canonical SOF2 grayscale header projection, excluding stronger quantizer/sampling/
pixel checks outside Identify. Original source color and components are retained
independently of the projected grayscale representation. Projection ownership,
decoder reset/header extraction and fresh AVCodecParameters acquisition/release
remain inside each operation; pixel-buffer requests remain zero.

WebP uses the original full RIFF/metadata compatibility scan plus public FFmpeg
parser, including its required packet buffering and EOF flush and fresh owned
descriptor acquisition/release. That buffer processing is material on this large
fixed input; this is not a general WebP performance claim. Neither side decodes
pixels. All nine native facts feed the observed accumulator, preventing their
elimination. Obsolete native work counters are removed; meaningful bounds remain.

All idle-v2 guards and native brackets pass. JPEG retains12actual/10filtered and
WebP12/12; conservative bounds include unfiltered results. Allocations measure
managed bytes, not native allocation or admitted peaks. Both named cases stop.
One shared harness and two native builds reuse four cached selectors. Production
source/DLL/package and prior5052checks perOS/same-package consumers are unchanged;
no production build, full-suite or package proof is repeated for tooling.

[Numeric evidence](current-jpeg-webp-resident-identify-evidence.json) retains all
observations, controls, identities and the mirrored/member-verified314-member raw
master. Earlier six-field comparisons are superseded. ResidentPNG's current
screen is unchanged; stream/async Identify, other formats/settings and default
decoder boundaries require their own valid evidence. Full267-path warm single-
CPU ≤1× remains open.
