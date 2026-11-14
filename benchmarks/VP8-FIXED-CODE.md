# Fixed-position VP8 arithmetic

**Accepted. All twelve declared WebP reading comparisons pass and stop.** The
remaining live-token opaque gap closes. These are the named frozen controls,
not a performance guarantee for every image or an overall-library parity claim.

The shared wide reader keeps its active code byte at bits63..56 of the same
UInt64. Probability comparisons/subtractions use that fixed position; normalization
shifts the code and refill inserts input into zero low bits. Range, future-bit
count, byte positions, every read address and successful scalar cursor remain
identical. The 272-byte declared-partition guard, short-input fallback, mode/
coefficient trees and every cancellation check are unchanged. No new owner/storage.

Independent complete live pairs improve **4,824.96 → 4,783.43 µs**:
**41.53 µs / .86%**, ratio interval **.987162–.995641**. The earlier three-pair
screen was inconclusive (.922586–1.060780); all captures remain. This managed-DLL
effect is scoped to its ALC execution, separate from actual reference costs.

| Complete existing control / FFmpeg | Lokad / reference µs | Conservative ratio interval | Managed B/op |
| --- | ---: | ---: | ---: |
| Opaque8 | 4,682.08 / 4,789.44 | 0.973927–0.979945 | 230,880 |
| OpaqueCancelable8 | 4,767.34 / 4,784.37 | 0.991113–0.999450 | 230,880 |
| SimpleOpaque8 | 4,794.68 / 4,874.87 | 0.979072–0.986309 | 230,880 |
| Native8 | 5,701.59 / 5,953.90 | 0.953229–0.960727 | 240,400 |
| StreamNative8 | 5,701.59 / 5,982.33 | 0.949625–0.955934 | 240,496 |
| ValidateVp8Alpha | 4,331.36 / 5,454.37 | 0.788699–0.796935 | 15,600 |

All six affected costs refresh on the timed immutable actual-package DLL. Five
separate standalone points are higher than the preceding round: ordinary,
simple-filter, transparent, stream and coding-only validation. Validation is
4,331.36 versus the prior4,236.13 µs; stream5,701.59 versus5,664.40 µs. All still
pass. These captures do not establish a universal speedup, clock-causal regression
or equivalence. Keep the current costs and adverse points visible. The independently
paired gain is for live decoding, whose actual standalone gap now closes.

Managed Decode includes parsing/admission, reconstruction/filtering, chroma/RGBA
normalization, independent output acquisition and disposal. Stream includes fresh
non-exposed MemoryStream acquisition/copies/EOF. Coding validation decodes complete
coding but returns source metadata, without RGBA normalization. FFmpeg retains its
declared codec/conversion wrappers and scratch/input boundaries; every packet is
decoded. Exact frozen pixels/alpha or descriptors, native/settings identities,
source independence, public CLI and owner gates pass unchanged. CPU0, sequential
Release, optimized JIT, natural nonconcurrent workstation GC, idle-v2 and all twelve
actuals/native brackets remain. Coding validation keeps filtered/all-actual bounds;
its scoped proof overlay is restored byte-for-byte after measurement.

The representation invariant is Fixed=Low<<(56-Bits). Shifting the code during
normalization and inserting the same input word below known bits preserves it.
Final cursor publication extracts the same normalized byte without another read.
Source reversal reconstructs the entire preceding file byte-exactly. Existing
independent scalar/coefficient/window/mode tests and every cancellation ordinal
are reused. Two new normal/live controls force304maximum-normalization decisions
at all8phases, compare every completed cursor and31following reads, and preserve
a nonzero unread tail without admission charges or source mutation.

Allocations remain unchanged; each independent RGBA result owns531,468 B. The same
8192Bcoarse header allowance, value fields and output owners remain. GC allocation,
admitted capacity, active results and default two-slot/4MiB idle storage are distinct;
caller-root calls bypass idle storage. No buffer/cache/pool/session or new reserved
capacity. Physical/native/JIT peaks are unmeasured. Unaffected encoders, lossless
reading, resizing and text reuse their source-valid evidence.

Both OSes pass **6,692 full/live checks** (3,263 internal/2,481 public/948 live),
plus **1,042 internal/402 public normal and scalar checks**. The SAME actual
461,523 B Release package and all three usage examples pass both OSes. One core/
embedded build per OS, zero tool/harness/native/fixture builds, zero producer
rebuilds after timing, one pack. Published releases are unchanged; no publication.

Current191image views: **185 pass/two gaps/four uncertain**. The73resize views keep
71passes/two uncertainties. All289material paths remain required. Next inspect
owned word copy/flip boundaries; do not retune the twelve passing WebP reads or
repeat unchanged uncertain resize captures.

[Evidence](vp8-fixed-code-evidence.json) pins representation algebra, source reversal,
every capture/adverse point and actual package. Source-specific older header/window
profiles remain history. Raw `vp8-fixed-code-qualified.tar.xz`, 3041424 B / 163 members, SHA256
`b8c44a7151a5855abec730b73e3d9cc4a3c1a93b8cdda670e538ea150f791092`, is mirrored/member-verified. Dependencies retain the original
package, scalar oracles, reference/tool identities and current resize evidence.
