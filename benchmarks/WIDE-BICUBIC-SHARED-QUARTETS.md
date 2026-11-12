# Shared wide bicubic vertical quartets

[Contiguous source views](CONTIGUOUS-WIDE-SOURCE-VIEWS.md) own the current byte/
word costs and qualification. This report retains the earlier traversal change,
its independently verified gain and raw evidence.

BWF794 reuses the shared exact four-pixel vertical accumulator in bounded wide
byte/word resizing. It keeps one publication/repair loop: the second pair waits
in a vector while the first is published in its original order. The shared helper,
horizontal filtering, geometry, pair/scalar tails, cancellation and ownership stay
unchanged. No new kernel, buffer, owner, cache, pool or public API is added.

The existing RGBA8 control improves **415.97→371.62 µs**, saving **44.34 µs /
10.66% raw / 5.35% reference-normalized**. Filtered/all-actual raw and conservative
normalized confidence intervals separate. Native time moves −5.61%, so the raw
reduction includes clock movement. Three-actual paired screens precede independent
twelve-actual confirmations on unchanged immutable builds. All actuals remain;
no unchanged capture is repeated for a favorable result.

| Complete existing 4 KiB resize | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| TightWide8 | 371.62 | 351.70 | 1.057 / 1.069 | 113 | gap |
| TightWide16 | 407.47 | 417.67 | 0.976 / 0.988 | 113 | pass |

Byte still has a measured gap. Word passes and stops, but its CPU effect is
**inconclusive**: predecessor 408.34/native413.48 µs versus candidate 407.47/
native417.67 µs; raw/all-actual and normalized intervals overlap. The word short
screen's promising point is not independent proof. No word gain is claimed.

These are the frozen 513×259→128×64 controls. Managed timing includes preparation,
normalization, exact filtering/repair, independent output and disposal. **zimg 3.0.6,
FFmpeg's zscale backend**, retains graph/input/scratch as declared; halo, filtering,
packing/normalization and independent outputs remain timed. This is not a fresh
native-graph comparison. Source/output hashes, exact rational/qualified managed
pixels, approved native float bounds, normal/scalar frames and CPU0 idle-v2 gates
pass. No image, shape, quality, filter or depth grid is added.

Primary vertical loop iterations 66,304→33,152; pixel loads/arithmetic stay the
same. These are logical counts, not a timing forecast. The source reverse proof
reconstructs the entire wide file after restoring only vertical construction.
The three-column layout retains pairs/scalar; performance claims cover only the
two displayed controls. Other resize/codec/text paths reuse unchanged evidence.

Working admission remains 4 KiB. Explicit stack-array payloads remain 3,296/4,068/
3,776 B, with one extra 64 B vector lifetime; physical/JIT-stack peaks are unmeasured.
Total warm allocation stays 113 B/op and operation scratch retention is zero.
The primary borrows 8,704 B cache+2,176 B weights inside 11,264 B of unpublished
result rows. Top 42 rows remain disjoint from the final 22; users retire before tail
metadata restoration and publication. Result payloads 32,768/65,536 B remain owned
until disposal. Existing default pixel-pool retention and governed bypass are separate.

Windows/Linux each pass **6,248 full/live checks** (2,939 internal, 2,361 public and
948 live). Affected 363 internal/776 public resize checks pass normal, scalar,
disabled-AVX2 and disabled-AVX512, including all three named storage suites.
The actual shared helper matches 81,920 ordered lanes through twenty taps with
poisoned unused storage. Existing row-loan/tail, odd stride, partial strips,
low-alpha, refusal, cancellation/recovery and independent-survivor checks remain.
Hardware kernels execute where supported. One main/embedded build per OS, no
public/benchmark/native rebuild or profile, and one pack serve the round.
Affected performance reconciliation precedes broad/package qualification.
The SAME actual Release-package consumer and all three examples pass both OSes.
Private 0.2.1 package `a88e69e45e371daaab3f0695a95e4012aefd83e3463068aa3860db76fb7063df` (449,031 B),
DLL `55b93e0028ad9916d2a02fc1ddedd5cee5b749e4d38c28b3ade038af3d363fc8`; Linux qualified DLL
`51186488f9c7f49a608e8f2aa7e8b2017419ef90997dc624a5b39c57408badb7`. Published releases remain immutable;
publication stays postponed.

[Evidence](wide-bicubic-shared-quartets-evidence.json) retains actuals, guards,
source proof, capacities, qualification and reproduction. Mirrored/member-verified
raw `wide-bicubic-shared-quartets.tar.xz`: 9,547,844 B / 664 members,
SHA256 `e955203ad4e4102b11a89bd392b835a30289b18d6cfabc6eafcc3ac456c14736`. Its pinned predecessor archive
retains unchanged helpers/reference tools, fixtures and qualification.
[Original row-loan history](BICUBIC-RESULT-ROW-LOAN.md) remains separately retained.
All 289 required paths remain; this is not a whole-release refresh.
