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

Development 0.2.1 adds `Probe`/`ProbeAsync` for selected `ImageProbeFacts`.
The default reads dimensions before pixel payloads. `EstablishedFacts` identifies
known results; nullable fields stay unknown until examined. Normal orientation
includes established absence of orientation. SourceRepresentation covers color,
sample precision and component count; Transparency separately covers the source
alpha representation and header hint. Animation can be detected without decoding;
animation decoding remains unsupported.

Probing starts at the current stream position and leaves it open after the acquired
prefix. Header reads do not read ahead; searching JPEG entropy for late metadata
may read ahead by at most 8,192 bytes. `BytesRead` counts actual acquisition;
`BytesTraversed` also includes seeks/read-ahead and equals the relative final stream
position. There is no rewind. Requested late or absent facts can require traversal
of pixel data; JPEG color declarations may occur after scans.

`ImageProbeOptions.MaxAcquiredBytes` defaults to 1 MiB and caps actual reads,
excluding seeks; exhaustion reports `ImageLimitKind.AcquiredBytes` without reading
past the cap. `ImageLimits.MaxInputBytes` separately bounds declared/traversed
encoded extents; unvisited tails have unknown length. Metadata/item/dimension/pixel
limits apply to the examined prefix. The fixed 8,448-byte workspace uses shared
admission; no hypothetical raster allocation or managed raster-size ceiling applies.
No encoded or metadata buffer survives the call. Probing does not validate CRCs,
pixel syntax, complete containers or unvisited conflicting metadata.
