# PNG RGBA16 Fastest comparison refusal

The original 513×259 RGBA16 raster encoded with managed `Fastest` and FFmpeg
`compression_level=1`, filter None, failed the existing 5% IDAT-size gate during
preparation. Managed IDAT contains 1,023,460 bytes; native contains 965,318 bytes:
**5.681% difference**. No timing was scored and allocation figures are unavailable.

Every decompressed filtered sample, including full word precision, alpha and
hidden RGB, matches the original raster. All chunk CRCs pass. Both streams declare
the fast compression class. Equal numeric backend levels do not establish
equivalent compression effort; this result remains unscored without relaxing
the size gate or searching compression settings.

The original native adapter includes complete little-to-big-endian conversion,
encoding and independent packet acquisition/release in each operation. Preparation,
I/O and validation are excluded. Production inputs were unchanged at refusal,
so existing cross-platform and package qualification was reused. The subsequent
[RGBA16 packing round](WARM-PNG-STORED16-ENCODE.md) has its own qualification;
this refusal has no scored performance claim at either source.

[Machine-readable refusal](warm-pngfast16-refusal.json) records packet identities,
measured source and native build. The ignored raw archive retains the failed
preparation log, both packets, original samples, source inventory, native worker
and four actual child identities. Continue other material workloads; the uniform
warm target remains open.
