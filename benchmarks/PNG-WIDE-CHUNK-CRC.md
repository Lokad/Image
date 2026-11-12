# Wide PNG chunk CRC

PNG encoding now advances its four existing 128-bit CRC residues together using
guarded 512-bit carryless multiplication, byte reversal and lane shuffling. The
same polynomial powers, seed, lane order, serial combination, reduction and tails
remain. Byte order and fold factors are prepared once per chunk. The original
128-bit and portable fallbacks remain. No packet, metadata or pixel contract changes.

The existing retained-native stored RGBA16 control improves from
473.40 to 443.92 µs:
**29.48 µs / 6.23% raw**.
Filtered and all-actual confidence intervals separate; the all-actual gain is
29.70 µs / 6.26%.
Native time moves +0.68%; normalized gain
6.86% also has separated intervals. This is a scoped
complete-operation gain, not a uniform encoder improvement or full parity.

| Existing complete encode view | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| EncodeNoCompression8 | 64.50 / 541.91 | 0.119 / 0.136 | 609,708 | pass |
| EncodeNoCompression16 | 439.04 / 1,380.79 | 0.318 / 0.328 | 1,147,452 | pass |
| EncodeNoCompression8Retained | 64.34 / 190.26 | 0.338 / 0.382 | 609,708 | pass |
| EncodeNoCompression16Retained | 443.92 / 406.92 | 1.091 / 1.105 | 1,147,452 | gap |
| EncodeGray8 | 2,035.51 / 1,988.73 | 1.024 / 1.059 | 547,228 | uncertain |
| EncodeRgb16 | 15,921.31 / 16,071.65 | 0.991 / 1.005 | 2,427,356 | uncertain |
| EncodePredictable16 | 257,701.58 / 387,124.92 | 0.666 / 0.684 | 2,491,852 | pass |

All 20 existing encoder views refresh on the same immutable module; their exact
complete packets, frozen inputs/settings, source guards, independent outputs and
native identities pass. The 17 equivalent fresh-state controls have
15 passes / 0 gaps / 2 uncertain.
Uncertainty is not proof of a slowdown. Stored packets remain 533,444/1,066,552 B;
predictable16 remains 573,041 B. Retained-native post-GC4 word is still a gap.
Fresh-state native preparation includes codec/options/zlib/packet creation and,
for words, frame allocation/conversion/disposal. It is a separate comparison.
Natural-GC64 word is 290.60/405.18 µs,
upper 0.749, 1,147,393.875 direct B/op,
and passes only that scope. Never sum or subtract historical means into phases.

Total allocations, peak capacity and retention remain distinct. No heap array,
owner, pool, cache, field or substantial owned/retained storage is added. Existing
4 KiB codec allowance and 72/384 KiB deflater admission remain; operation-scratch
retention is zero. Some counters differ by 408 B across captures; that is not an
allocation-saving claim. Physical JIT/native peaks are unmeasured. Cancellation,
writer failure cleanup, terminal admission refusal and output ownership are unchanged.

Only encoder ComputeChunk changes. The standalone scan/identify Compute and Reduce
generated bodies are identical (1,045/293 code bytes), after normalizing only
process addresses and method-number prefixes. Shared fields/initialization and
read/fold/reduce source remain verbatim. Thus the [51 read controls and shared-root
evidence](PNG-CURRENT-FRAMING-COSTS.md), other codecs, resize and text costs remain
source-valid; no broad timing refresh is performed.

The independent scalar polynomial model compares 1,206 intermediate states across
90 input/seed controls. Existing bitwise, impulse, type, unaligned and tail tests
exercise the actual implementation. Untimed prototype A rebuilt byte order inside
the loop; final B hoists it after inspecting generated code. Both are retained;
A has no performance claim. Actual B wide instructions are observed in the JIT.

Quiet-VM CPU0, one thread, warm optimized JIT, workstation nonconcurrent GC and
idle-v2 guards remain. Three-actual screen precedes independent 12-actual primary
baseline/candidate confirmation. All actuals and native brackets are retained.
Affected performance reconciles before one accepted qualification milestone:
Windows/Linux each pass 6,510 full/live checks and 703 internal / 246 public PNG
checks per normal/scalar/AVX512-off/AVX2-off mode, then the same actual Release
package consumer and all three usage examples. Unchanged resize ISA/storage
qualification is reused. Two core/embedded builds per OS cover A+B; public,
benchmark, native and fixture builds are reused. Published releases remain unchanged.

[Evidence](png-wide-chunk-crc-evidence.json) identifies modules, sources, all current
costs, adverse comparisons, validation and qualification. Raw `png-wide-chunk-crc-qualified.tar.xz`,
SHA256 `eff9069bde7a59f90b484eaaa31b2d756ebf0ef756423a4b0da96138a189909e` (3,158,872 B / 306 members), is mirrored
and member-verified. [Prior Adler work](PNG-BLOCK-ADLER.md) and its cited raw evidence
remain history and current proof for unchanged reading paths. All 289 required
paths remain in scope; publication is postponed.
