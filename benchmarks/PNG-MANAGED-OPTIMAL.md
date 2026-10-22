# E21 managed PNG Optimal packet qualification

This records E21. [E24 qualification](PNG-MANAGED-POLICIES.md) extends the managed
encoder to every compression policy and records current binaries/tests/packages.

At E21, the default encoder used original safe managed level6 Deflate, generated
Huffman trees, filterNone and4096-byte IDAT chunks. These are the same eight
513×259 original controls and frozen FFmpeg inputs/settings used previously.
Complete packets match raw-exact; no metadata exception is used.

| Source representation | 8-bit packet B, managed = reference | 16-bit packet B, managed = reference |
| --- | ---: | ---: |
| Grayscale | 72,586 | 265,433 |
| RGB | 168,729 | 739,472 |
| Grayscale-alpha | 120,366 | 506,887 |
| RGBA | 210,973 | 963,202 |

Production keeps a bounded history/dictionary and one token block. Its conservative
1MiB workspace is admitted before allocation through the borrowed shared context;
no full-frame filtered/token copy or encoded-output cache is retained. Refusal is
terminal. Cancellation/writer failure cleanup releases workspace without further
output. Async encoded bytes keep their separate owner until actual writes finish.

Windows/Linux qualify the same immutable core,5,181 unique Release checks each including
live FFmpeg,21 affected scalar checks each, and the same actual private0.2.0
package with all three examples. Public tests check exact packet hashes, pixels,
source and surviving outputs; primitive tests cover chunked delivery, stored/fixed/
dynamic data, shared refusal, cancellation/failure cleanup and subsequent success.

A later resource-document correction changed no executable or NuGet metadata
content; the identical payload's consumer evidence is reused after explicit ZIP
comparison. Both package identities are recorded in the evidence.

CPU time and allocated B/op for this new encoder are **unmeasured**. All17prior
BCL PNG timing rows predate the changed shared entry/framing code; nine also have
the new default algorithm. At that checkpoint, other policies retained runtime backends;
E24 subsequently completes the six named level0/1/9 controls. Unchanged decoders
and transforms retain evidence on their recorded frozen inputs. These controls
do not establish general-image, other-policy or performance parity.

[Numeric evidence](png-byte-parity-evidence.json) pins binaries, source/reference
hashes, qualification and raw records. The initial token/tree prototype remains
development tooling; production has no native bindings or dependencies.
