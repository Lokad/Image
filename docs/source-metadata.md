# Source metadata

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

Sync/async Identify and InspectHeaders agree for valid inputs, without pixel
decoding. Header inspection uses fixed workspace and omits pixel, PNG CRC and
JPEG table validation; Identify validates fuller containers and Decode also
validates pixels. Input/metadata/item limits, cancellation, shared admission and
borrowed-stream ownership apply. See [the contract](spec.md#inspection-streams-and-failures).
