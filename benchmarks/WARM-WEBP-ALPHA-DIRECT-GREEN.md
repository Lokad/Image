# Direct compressed WebP alpha output

Complete stream decoding uses **2.89% less filtered / 3.16% less all elapsed**
in independent confirmation, with **531,756 fewer allocated B/op**. Both raw
99.9% intervals and conservative normalized intervals separate. The existing
resident companion also has current independent costs, without a paired gain
claim. Both remain gaps to warm single-CPU parity.

| Complete existing operation | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Fresh seekable stream | 7,068.765 | 1.181 / 1.184 | 1,434,827 |
| Resident input | 7,198.364 | 1.208 / 1.212 | 1,434,731 |

Both subjects use the same original 98,590-byte, 513×259 VP8+ALPH input. Every
RGBA/alpha sample matches the frozen native output exactly. Stream costs include
fresh non-exposed acquisition/copies/EOF, full decoding/normalization, independent
output acquisition and release. Reference settings and native boundaries stay
fixed; native allocations are unmeasured. No new performance input/settings grid.

The decoder still reconstructs and validates the complete headerless VP8L image.
It now copies green directly from its transformed uint raster into the existing
alpha array, removing a full RGBA intermediate and its owner/conversion. All
preexisting lossless methods remain source-identical. Ordinary VP8L output,
pixel validation, raw ALPH, opaque VP8 and encoding are unchanged. No new buffer,
cache, admission mechanism or owner is added; exact pixels and cancellation stay.

The separate existing governed 513×259 memory control retains its 115,542-byte
quality85/effort4 packet. Its shared peak falls **1,411,931→884,699 B (37.34%)**;
allocations fall **1,966,392→1,434,800 B**. Both OSes and normal/scalar modes agree.
The initial private prediction overstated peak reduction by 4,236 B: removing a
frame does not imply an equal fall in the maximum overlap. Before CPU timing,
the check was corrected to full-frame allocation removal and a peak below two
frames; all CPU/pixel/native/quiet gates stayed fixed. Raw evidence preserves
the original prediction and correction. No candidate or timing was repeated.

A new public regression proves two-frame admission, exact alpha, retained-result
charges, terminal aggregate refusal, survivor preservation, release and recovery.
It fails the original implementation and passes the candidate. The actual package
consumer also proves exact alpha and two-frame admission on both OSes.

Both OSes pass **5,197 checks each** (2,097 internal, 2,152 public, 948 live),
35 internal/270 public focused checks in normal/scalar modes and 54 additional
scalar admission checks. Eight prior admission formulas are tightened to remove
the deleted intermediate; one-byte-below rejection remains. Completed internal
checks are reused after this test-only correction. The same actual Release
package, governed async consumer and three usage examples pass both platforms.

One immutable core per OS, cached workers/harnesses/selectors, one fresh three-
actual pair, one candidate twelve against valid original twelve, and one resident
twelve are used. The predeclared 2% continuation allowance is justified by the
substantial storage removal. No profiling, variants or successful timing repeats.

[Evidence](warm-webp-alpha-direct-green-evidence.json) pins producers and the
mirrored/member-verified webp-alpha-direct-green-qualified.tar.xz (226 members),
SHA256 976766208f13643e247ef2fbfd1c04fff1c96bb3bbc0c8321206529c4b793ad1.
Other affected compressed-alpha boundaries need applicable current costs;
unchanged paths reuse qualified evidence. The full 268-path objective stays open.
