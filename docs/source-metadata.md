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
limits apply to the examined prefix. Dimension-only probes use 512 bytes of
admitted workspace; other fact selections use 8,448 bytes. Both use shared
admission; no hypothetical raster allocation or managed raster-size ceiling applies.
No encoded or metadata buffer survives the call. Probing does not validate CRCs,
pixel syntax, complete containers or unvisited conflicting metadata.

`ValidatePixels`/`ValidatePixelsAsync` establish pixel-coding validity and return
the same coded `ImageInfo` as complete identification, without retaining RGBA.
They still require codec-dependent storage and preserve metadata policy, resource
limits and cancellation. Stream routes acquire through EOF and stay open; async
acquisition is awaited before releasing storage. Validation omits final sample
conversion/reorientation and does not replace Decode when pixels are needed.

All read routes default to `ImageMetadataPolicy.Strict`: EXIF header/IFD0 orientation
errors, duplicate chunks/fields and misplaced PNG EXIF fail. Opt into `IgnoreInvalid`
on DecodeOptions, JpegDecodeOptions or ImageProbeOptions. Identify/InspectHeaders
and their async counterparts have explicit `(input, limits, cancellationToken,
metadataPolicy)` overloads; existing overloads remain strict.

Permissive mode ignores unusable EXIF directories/fields, accepts duplicate and
misplaced EXIF, and uses the last usable orientation. Missing/invalid fields do not
erase a previous usable value; no usable value means Normal. All eight valid
orientations retain their meaning; reorientation remains opt-in. Requested permissive
orientation probing traverses the container to resolve later overrides. Both policies
ignore unrelated tags/links without recursion or aliased-payload copies and skip unused
text/ICC without decompression. Outer lengths/framing, required coding, promised
checksums, WebP feature flags, resource refusal and cancellation remain mandatory.
