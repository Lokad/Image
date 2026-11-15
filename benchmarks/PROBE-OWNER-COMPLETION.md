# Probe owner completion

The public probe wrapper completes immediate parsing directly and transfers
genuinely pending work to an out-of-line owner. Validation order,shared grammar,
reads,limits,polls,metadata and independent results remain. The owner survives
actual I/O users,including cancellation/failure. An async failure helper preserves
cancellation status/token and disposal-exception precedence;no cache,pool,array,
new admission model or public API.

Independent complete-operation pairs confirm **0.225342→0.217335 µs**,
**3.55%/8.01 ns** saved,
interval **0.951896–0.977259**, **568 B/op** unchanged.
Both all-actual mean intervals also separate. All twelve pairs and the original
uncertain three-pair screen remain. This scoped effect is not FFmpeg parity or
a sum of historical stage gains. Earlier [completion trial](PROBE-COMPLETED-PATH-REJECTED.md)
used different parser/result/ownership machinery and a superseded fixed threshold.

| Complete dimension probe / labeled FFmpeg AVIO-header adapter | Lokad / reference µs | Conservative upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| ProbeDimensionsPng | 0.109 / 0.122 | 0.921810 / pass | 568 |
| ProbeDimensionsWebp | 0.103 / 0.121 | 0.884141 / pass | 568 |
| ProbeDimensionsJpeg | 0.199 / 0.195 | 1.078226 / uncertain | 568 |
| ProbeDimensionsJpegAsync | 0.220 / 0.202 | 1.123853 / gap | 568 |

All four affected original references use the SAME actual immutable timed/package
DLL,twelve actuals and original callback,prefix,descriptor and owner gates.
Filtered results and all-actual statistics remain separate;upper takes the worse
view. PNG/WebP pass and stop;JPEGsync remains uncertain,async a gap. Unchanged
PNG-header and complete JPEG-header evidence is reused;no release-wide refresh.

One-gated-read allocation diagnostic adds **8 B/probe**:1952→1960B.
The unchanged complete-header guard stays9984B. Two alternating128-operation
pairs follow128warmups per subject/mode. Absolute totals include equal input,
gate,typed bridge and batch overhead;not standalone production GC or native
performance. Immediate allocation568B,admission512Bdimensions/8448Bbulk,
zero added arrays/cache/retained operation storage. Physical/native/JIT peaks
are unmeasured. No CPU gain is inferred from allocation counts.

Windows/Linux each pass **6,727 full/live checks**, **10 internal/154 public**
normal/scalar and the **SAME actual Release-package consumer/all three examples**.
Two stream-cancellation cases also pass the original baseline on both OSes:
thrown and faulted reads preserve an unsignaled token/canceled status,release
admission,leave caller input/root open and permit successful subsequent use.
Existing actual-pending/EOF/failure/refusal/cleanup checks remain.
One core/embedded/public build per OS,zero harness/native/fixture/helper builds,
one pack and no timed Linux rebuild. Qualification follows affected performance
reconciliation;exact encoder packets and managed pixels remain.

[Evidence](probe-owner-completion-evidence.json) pins sources,both observations,
memory tradeoff and package. Raw `probe-owner-completion-qualified.tar.xz` (6,219,776 B/194
members),SHA256 `847a2839ebb2a466b4fcbbdd3abca92fdeece2c07130d8d969503a91d6b32a2f`, is mirrored/member-verified with retained
dependencies. Private package `436c80d44cdce72acfcf006226aacd55f5c257e0f82b6d28e931972150cfbf5c` (466,614 B)
contains timed DLL `fb17f12e3153953b7bbd9687d3294ffd9b30f1eec5167eec1776a501d5007018`.
The full289-path objective remains open;publication is postponed and published
packages remain immutable.
