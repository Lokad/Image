# JPEG 4:4:4 row normalization

Reusing the existing exact row kernels reduces the original forward-only control's
native-normalized cost by **18.92%** in its independent confirmation. Raw elapsed
reductions are 25.90% filtered / 25.67% all-actual; the native point is 8.61% faster
between runs, so the raw change alone is not a code-only gain. Both raw confidence
forms and the conservative native-normalized confidence separate.

The tested scope is five existing 513×259 image-input 4:4:4 controls, original
RGBA8/16 sources, Q85/native qscale 5 and background 0x204060. Complete operations
include normalization, admission, fresh output acquisition/growth and release.
Packets remain exactly 133,039/133,041 B (word 133,041/133,043 B), solely the approved
bounded attribution COM difference. Settings and reconstruction are unchanged.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Original forward-only12 | 5,038.943 | 4,130.456 | 1.220 / 1.229 | 820,123 |
| Candidate forward-only3 | 3,750.156 | 3,769.446 | 0.995 / 1.091 | 820,150 |
| Candidate forward-only12 | 3,733.671 | 3,774.819 | 0.989 / 1.009 | 820,150 |
| Encode8 | 3,720.747 | 3,774.692 | 0.986 / 0.995 | 820,118 |
| Encode16 | 3,780.622 | 3,853.815 | 0.981 / 0.992 | 820,088 |
| Encode444Cancelable8 | 3,762.030 | 3,783.042 | 0.994 / 1.001 | 820,118 |
| Jpeg8AsyncNonSeekable | 3,727.716 | 3,877.520 | 0.961 / 0.971 | 820,531 |

Resident 8-bit, resident 16-bit and async independently pass the ≤1× upper-bound target.
Forward-only 0.989/upper 1.009 and live-token 0.994/upper 1.001 remain confidence-open;
neither is a parity pass or a demonstrated gap. Stop confirmed passing cases;
do not repeat the two near-parity jobs to chase significance. This does not
establish full JPEG or 268-path parity.

Each MCU row now obtains its source row once and reuses the existing exact
byte/word normalization helpers. Right-edge repetition, YCbCr conversion,
transform, entropy, cancellation and error behavior remain. The extra 32 stack
bytes fit the existing admitted 8-KiB fixed owner; no new heap buffer/cache/charge.
The measured primary +27 B/op includes harness amortization. Native allocation is
unmeasured. Grayscale/4:2:0/4:2:2 branch before this code; all decoders and unrelated
codecs/editing/text retain applicable unchanged evidence.

Windows and Linux each pass 5,342 checks (948 live comparisons), plus 12 internal/
388 public focused checks normal/scalar; Linux also passes without AVX512. The
same actual Windows Release 0.2.1 development package and three examples pass both
OSes. The package consumer independently normalizes byte/word inputs and compares
owned-image/sync/async packets with callerRGB packets, source/stream ownership and
shared admission. Published packages remain unchanged; this is not a release.

One core/internal build per OS; no public/differential/harness/selector/native
rebuilds. Cached drivers and frozen controls are reused throughout the bounded
round. One completed async result was recovered after a sync-only post-check
field mismatch, without rescoring. The initial analysis helper also mistakenly
added a cross-run 5% native cutoff absent from the planned/established protocol;
it was corrected before independent confirmation, with all point/confidence,
pixel, resource and quietness gates preserved. Native drift remains disclosed.

The 0.2.0 byte 4:4:4 short was still applicable: both entropy-record and paired-writer
changes predated that release. The current original twelve resolves a missing
independent confirmation, not a falsely invalidated production path.

The [evidence](jpeg444-row-normalization-evidence.json) contains all actuals,
native brackets, filtered/unfiltered statistics, exact hashes, qualification,
corrections and the mirrored/member-verified 3,803,532 B raw archive:
`jpeg444-row-normalization-qualified.tar.xz`, SHA256
`baece509ccf51f64d4a915aac4040fa6201ae8f498d1c9f3d7751ff6fa827ae7`.
Its retained dependencies preserve the preceding immutable producer and async
worker/fixture provenance. No broader content/settings claim follows.
