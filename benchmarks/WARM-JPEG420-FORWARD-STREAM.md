# JPEG 4:2:0: forward-only output

The complete forward-only RGBA8 encode screens at **2.098× FFmpeg** and remains
open. A byte-admission shortcut does not improve it and is discarded; production
is restored. Neither screen receives a twelve-actual confirmation.

| Three-actual screen | Lokad.Image µs | Ratio / upper | Allocated B/op | Decision |
| --- | ---: | ---: | ---: | --- |
| Qualified original | 3,703.74 | 2.098 / 2.370 | 527,367 | Open |
| Direct byte admission | 3,744.01 | 2.116 / 3.115 | 527,367 | Reject |

Both screens pass the versioned quiet guards. The explicit `Encode420Stream8`
case creates a forward-only Stream wrapper over fresh owned MemoryStream
storage for every operation. The wrapper exposes no seeking, Length or Position.
Wrapper creation/disposal, storage growth, complete encoding and output release
are timed. The native operation includes flattening/conversion, encoding and
independent packet acquisition/release, with prepared wrappers reused. Runs are
sequential Release on one pinned CPU, one codec/conversion thread, normal SIMD,
tiering off and workstation nonconcurrent GC. Startup, IPC, I/O, preparation and
checks are excluded. Native allocation is unmeasured; managed allocation includes
harness overhead.

The same authored 513×259 RGBA8 raster, quality 69/native qscale 5 and background
RGB 0x204060 preserve a **0.106110 dB** source-quality gap within the unchanged
0.25 dB gate. Both managed screens retain the exact original **147,635-byte**
fixed-Huffman packet, decoded pixels, alpha, geometry, source and independent
owners. Native optimal Huffman coding is explicit: this is source-quality
timing, and compression efficiency parity is not established. There is one
quality evaluation, no sweep and no additional original raster.

A single six-second profile records 5,891 samples: **41.3%** in the encoder leaf
(including inlined work), **18.3%** in entropy Write, **5.0%** in byte admission
and **2.8%** in span admission. Removing the byte-to-span admission call preserves
exact callbacks, work, quota/cancellation/failure order and packet bytes, but
shows no useful speed gain. The candidate passes 39 focused checks in normal
and scalar modes; 39 restoration checks pass. Exact original production source
and the qualified Linux DLL are restored. Its **3,267 tests per OS**, **896 live
comparisons per OS**, and consumer/three examples through the same actual package
remain authoritative. No repeated full suite, package proof or method audit is
needed for this rejected change.

The first preparation refuses an obsolete native-worker anchor before timing;
one current-worker fixed quality setting is then validated in a separate anchor.
A supervisor's post-run JSON-key casing error is independently corrected from
the retained receipt and guards; the successful timing is not repeated. Both
incidents remain in the [numeric evidence](warm-jpeg420stream-evidence.json).

Next investigate a substantial fallback entropy strategy. Any deliberate packet
change must be explicit and preserve every quantized coefficient, amplitude,
decoded pixel, quality comparison and public operation guarantee. Do not repeat
this byte shortcut or the earlier rejected entropy inlining, cached MemoryStream
and symbol-batching trials unchanged. Uniform current-source parity stays open.
