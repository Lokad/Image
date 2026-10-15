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

On the unchanged qualified .NET10.0.8 build, separate filter/row writes, combined
rows and one contiguous write produce the same 1,023,460-byte IDAT payload,
byte-for-byte. Exact filtered samples and chunk CRCs pass. Write boundaries do
not explain this refusal. Production/package qualification is reused; no timing
or allocation result is scored.

Independent Deflate traversal finds two fixed-Huffman blocks in the BCL payload
and fifty-five dynamic blocks in the native payload. An original correctness-only
[RFC1951](https://www.rfc-editor.org/rfc/rfc1951#section-3.2.7) prototype recodes the
same 878,280 literal bytes and 32,323 length/distance pairs into one dynamic block.
It changes neither level1 match finding nor filter/sample data. IDAT becomes
**974,122 bytes**, versus native **965,318**, a **0.904%** difference inside the
unchanged5% gate. The zlib fast-class header is preserved; maximum literal/distance
code widths are13/12 bits. Independent token-sequence comparison, both managed
decoder paths and both normal/scalar pinned FFmpeg decoders preserve every word,
alpha and hidden-RGB sample. Geometry, chunk CRCs, source and live-owner independence
also pass.

This prototype is not production-integrated or performance-qualified. Current
Lokad.Image output still fails the original size gate, and the row remains
unscored. The numeric refusal now includes the bounded diagnostic, exact token/
packet hashes, actual decoder qualification reuse and ignored raw archive. Next:
one safe C# entropy-recoding primitive with full admission/work/failure controls,
followed by equivalent complete-operation comparisons; no compression-level sweep
or broader fixture set.
