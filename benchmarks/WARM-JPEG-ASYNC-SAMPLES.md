# Asynchronous caller-RGB420 JPEG encoding

Historical producer and short screen. [Current cost and packet evidence](CURRENT-ASYNC-JPEG-RGB420-COST.md)
supersede the measurements and compression claims below; their raw evidence is retained.

Updated 2026-10-20. The current qualified producer has a **1.276× gap**
for the already-named complete caller-sample async operation. Production is
unchanged. This is a short screen; no independent parity confirmation is claimed.

| Complete EncodeJpegAsync /513×259 packed RGB24 | Lokad µs | FFmpeg µs | Ratio / conservative upper | Lokad allocated B/op | JPEG B Lokad / native |
| --- | ---: | ---: | ---: | ---: | ---: |
| Quality69 /nativeqscale5 /420 | 2,076.73 | 1,628.02 | 1.276 / 1.430 | 232,614 | 74,040 /52,093 |

Both sides borrow the same existing398,601B RGB24 projection, stride1539, and
perform complete RGB-to-YCbCr420 conversion/sampling, transforms, adaptive/optimal
Huffman planning, entropy, independent staging/output acquisition, actual output
copies/growth and release. **Neither side creates an RGBA intermediate or flattens
alpha inside the operation.** The original projection is prepared outside both.

Managed public EncodeJpegAsync(ReadOnlyMemory) includes validation/borrow,8192B
sample workspace, its separate adaptive admission/contiguous staging selection,
tail clearing and awaited WriteAsync completion. Native uses public FFmpeg RGB24
conversion/encoding, then fresh nonseekable AVIO/caller output and internal flush
to finish buffered writes. Caller storage survives staging release and is disposed
inside each operation. No final snapshot copy, source/output view, seek/length,
real I/O, forced scheduling or cached encoded content. Callbacks complete immediately;
resident input/context/wrappers and released private storage may be reused. Native
allocations are unmeasured. No managed caller FlushAsync is required.

Both packets are byte-identical to the frozen RGB420 anchor. Current public/native
quality samples and per-encoder decoded pixels/alpha revalidate once; source
PSNR15.363091854/15.256982294dB gives **0.106109561dB** gap inside0.25. Numeric
quality values are encoder-specific. Setup/cleanup reuse exact hashes, and normal/
scalar native decoder evidence remains valid for unchanged packets and checked
FFmpeg executable. No calibration, image, depth or sampling grid; compression-
efficiency parity is not established.

The shared JPEG/output controls check actual WriteAsync-only delivery, caller
left open, input unchanged, independent simultaneous/surviving outputs after
peer/staging release and caller-input mutation, output/memory/work refusal,
pre-cancellation, cancellation after the first write, second-write sink failure
and subsequent success. Existing deferred-await/cooperative-memory qualification
is reused because production is unchanged.

One quiet three-actual short with three native samples before/after, both idle-v2
guards and conservative selected/all-actual bounds. No long run above parity.
One successful immutable tooling build/OS and one native build; an initial Windows
constructor-overload collision failed compilation before execution and was corrected.
The original generic WarmJpeg selector is reused from its qualified archive and
later reuse attestation, with the common verified.NET10 runtime configuration.
Its actual receipt confirms the new RGB420 public boundary. No selector build or
source reconstruction. Earlier prepared runtimes stay intact.

Reuse [current qualification](WARM-LANCZOS3-COEFFICIENT-PERMUTATIONS.md):4,448 tests/OS,
affected hardware-off checks, same actual private package and all three examples
on Windows/Linux. New tooling compiles on both OS; Linux supplies controls/timing.
No producer build or repeated full/package qualification. The finite checklist
becomes **254 =236 numeric+18 unmeasured**:71 independent passes,164 mean gaps,
one confidence-open. Other235 observations retain their identified builds. Full
parity/release qualification remains open. Next: the named lossless WebP async
output boundary, preserving the approved exact-RGB reference.

[Evidence](warm-jpeg-async-samples-evidence.json). Raw `h15-qualified-linux.tar.gz`:
`12804476fc85e1af9e3eb7ffd1d0af9af9167371d86a91904e9fa04b2a10ae4b`. Windows tooling `h15-tooling-windows.tar.gz`:
`15d09ce4971ce998166213403facd6a6981f02cb874161541b6a5ea173f320d3`. Producer
`c12fbeebea4edb6b4959837183a4b12379756be3139bd46b5e3c1798ff97671d`; Linux harness
`620ef3f645c32345905ab1983e57fbae2a038755a8386dee048aa96b11bb598a`. Source/reference/release evidence remains
protected. A241,499,801B reinstallable Linux SDK archive was removed from the
Windows download cache after path/type/hash verification; installed SDKs and
reference sources remain. Old VM diagnostics were losslessly compressed with
round-trip hashes/modes. Both checkouts remain below5,000,000,000B.
