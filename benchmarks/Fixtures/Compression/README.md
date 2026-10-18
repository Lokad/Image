# Compression controls

Exactly two fixed 513×259 opaque RGBA8 images exercise photographic texture and
text/diagram content. [manifest.json](manifest.json) records encoded/raster hashes
and preparation. These are inputs; [measured results](../../WARM-WEBP-COMPRESSION.md) are recorded separately.

`photograph.png` derives from **Grand Tetons**, Scott Horvath, USGS, published
October 4, 2016. The [specific original publication](https://www.usgs.gov/media/images/grand-tetons)
identifies it as public domain and provides a [CC0 dedication](https://creativecommons.org/publicdomain/zero/1.0/).
FFmpeg 8.1 performs one Lanczos resize and centre crop; optional metadata is
stripped. The original JPEG/source page stay in ignored `external/`; neither is
a build input. This image comes from the photographer's publication.

`diagram.png` is original Lokad.Image content: fixed bar-chart data, rectangles
and supplied-font text. Its generation is defined by
[CompressionFixtures.cs](../../../tools/Fixtures/CompressionFixtures.cs), under
the repository's MIT license. To reproduce it, supply the matching font identified
by name/hash in the manifest and run `dotnet run -c Release --project tools/Fixtures
-- <empty-output> --compression-diagram <font.ttf>`. Font bytes are not included;
using the frozen PNGs requires no fonts or platform renderer.

Fixture preparation and hashing are outside codec measurements on both sides.
Use equivalent complete encoding, source-quality checks and independent output
owners. Equal quality settings are not equivalent quality. Keep this pair fixed;
record later codec results separately.
