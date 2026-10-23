# Decoder exactness

[The finite checklist and numeric evidence](decoder-exactness-evidence.json)
reuse existing controls and pinned references. Exact normalized samples and typed
source metadata are required. Remaining lossy bounds are transitional checks.

| Required path | Current evidence and next action |
|---|---|
| PNG normalization, Adam7, entropy, packed depths and tRNS | Existing exact frozen/live assertions pass; stop at that scope. |
| VP8L transforms, palette, cache and copies | Existing60-fixture/authored/live exact assertions pass; stop at that scope. |
| JPEG inverse and progressive coefficients | Integer primitive qualified; all30 frozen grayscale/444/422/420 controls and named live cases exact. Stop passing cases. |
| JPEG444/422/420 color and direct RGB | Exact frozen/live samples pass on both OSes, including odd edges, restarts and progressive scans. Rounded chroma precedes the integer matrix. |
| JPEG CMYK/YCCK and411/410 | Windows diagnostic CMYK/constant411 pass; constant YCCK444/420 expose a one-sample conversion difference. Resolve it before varying-chroma confirmation. |
| JPEG caller grayscale/RGB/CMYK | Native Y/RGB444/422/420, padding and orientation pass. Windows direct-RGB grayscale diagnostic passes; durable assertion and CMYK exact confirmation remain. |
| VP8 reconstruction and deblocking | Measure planar reconstruction before changing a primitive. |
| VP8 chroma/RGB and ALPH | RGB exactness uncertain; retain passing exact-alpha controls. |
| Typed source metadata, orientation and delivery | Existing sync/async, caller-buffer and lifetime checks pass; retain them. |

E104 routes half-resolution YCbCr through rounded eight-bit chroma and the
qualified Q13 BT.601 matrix. Centered interpolation geometry is unchanged.
Owned RGBA and caller RGB share the conversion, including opt-in orientation.
Fixed32-sample chunks add64 stack bytes, with no new owned heap storage or frame
copy. Existing4096-per-block and32-per-pixel row work, cancellation, terminal
refusal and retained ownership remain. Superseded floating H2 implementations
and their implementation-only tests are removed.

All30 existing frozen grayscale/444/422/420 controls now require exact complete
RGBA samples. The three31x23 anchors additionally cover arrays, synchronous and
asynchronous streams, caller RGB, padding and orientation. Existing live baseline,
progressive, restart and caller controls require exact samples; the three anchors
also compare normal native execution. Original numeric inverse controls retain
their explicit pinned normal x64 policy, including native C/AVX differences.

Windows and Linux each pass5,023 checks, including940 live comparisons, plus33
internal and77 public affected scalar checks. The SAME actual private0.2.0 package
passes both consumers and all3 usage examples. Earlier JPEG decode timing and
allocation rows describe the replaced producer; no new CPU/GC measurement is
claimed. Broader decoder equality, warm single-CPU parity and readiness remain open.

The next bounded screen reuses the current immutable build. Existing constant
CMYK and direct-RGB grayscale controls match normal/scalar native on Windows.
Existing constant YCCK444/420 differ by one sample and identify the next conversion
boundary. Constant411 agreement does not qualify varying-chroma interpolation.
Keep those distinctions; no new quality/shape grid or passing-case retuning.
