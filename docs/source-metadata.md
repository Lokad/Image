# Source metadata qualification

`ImageInfo.SourceColor`, `SourceAlpha`, `SourceComponentCount` and `BitDepth`
describe encoded samples, independently of decoded RGBA. Six-argument construction
and deconstruction remain available; manually constructed records have unknown
descriptors and zero component count. Equality and serialization include the new fields.

| Encoding | Source color / alpha | Precision / components |
| --- | --- | --- |
| PNG gray or RGB | Grayscale/Rgb; None, Channel or color-key tRNS | Encoded sample depth; alpha is a component |
| PNG indexed | Indexed; None or palette tRNS | Index depth; one coded component |
| JPEG | Grayscale or declared/conventionally named RGB, YCbCr, CMYK, YCCK; no alpha | 8-bit; SOF component count |
| JPEG without a meaningful color declaration | Unknown; no alpha | Known 8-bit precision and component count |
| WebP VP8 | YCbCr; optional separate ALPH | 8-bit; three or four components |
| WebP VP8L | Rgb; coded alpha channel | 8-bit; four components, even with an unset alpha hint |

`HasAlpha` retains header semantics, rather than proving a nonopaque pixel exists.
Opaque tRNS entries still count; WebP's lossless alpha hint remains nonbinding.
JPEG declarations use Adobe, then JFIF, then conventional RGB/CMYK component names;
ambiguous three/four-component input stays unknown. Inspection does not apply
ICC conversion, inversion, caller color overrides or EXIF reorientation.

The 36 new original-fixture controls cover every inspection API form, packed and
16-bit PNG, all transparency representations, optional RGB palettes, JPEG model
declarations and ambiguity, WebP alpha hints, no-pixel-decoding behavior, shared
refusal/cancellation cleanup and recovery. Existing 32MiB sparse-header and EXIF
tests preserve fixed workspace, seek/drain behavior, limits, validation boundaries
and borrowed streams. Authored fixtures reuse the repository's independent PNG,
JPEG and pixel recipes; no external corpus or font set is added.

All **4,571 tests/OS** (1,683 internal,1,956 public,932 liveFFmpeg), **259 affected
scalar checks/OS**, and the **same actual private0.2.0 package** consumer/all three
examples pass on Windows/Linux. Metadata remains a40-byte escaped record on both
platforms; byte descriptors use existing padding, without a new retained buffer.
Production freezes are reused throughout qualification. No benchmark, native
worker, selector, codec setting or fixture packet is rebuilt or retuned.

This is functional qualification. Metadata factories/preflight change31 previously
current decode/inspection cases, which require timing confirmation. The named cases
are listed in [qualification evidence](../benchmarks/source-metadata-qualification-evidence.json).
The metadata-returning PNG reference must also expose equivalent typed fields.
Pixel/quality/resource proofs and unaffected encoder/edit timings retain their
identified scope. Full warm parity, compression-size gates, additional required
supplied-face compatibility and replacement readiness remain open.

Linux producer `aae514fc5f04195ba920314ca473af3cbe999e7b7ff6957e877f6b12b2729d42`;
Windows `73dfaf82df696b4b1c5bddf6af481aa7bebc1092f766325b3bc03c157c43dde1`;
actual package `b05504b3cd8c0feaae805ca17e4e78cb0abc87cff36df3168f72240d6a5f208e`.
Raw archives retained on both hosts: `c43-qualified-windows.tar.gz`
SHA256 `b30bf7a3a86998557f99403a34984c741e92ee4593a1aa326705f02ebbd0d580`,
and `c43-qualified-linux.tar.gz`
`4b12dc27f088cbca7a7310a359077f12d01d95e6be2e5da70a8872fb4e9fd5c2`.
