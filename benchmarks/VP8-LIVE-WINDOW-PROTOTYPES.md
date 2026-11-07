# VP8 live window prototypes

LWA466/LWS467, 2026-11-07. Neither implementation is selected.
[Evidence](vp8-live-window-prototypes-evidence.json) retains the bounded experiment;
current performance and production remain the qualified
[overwrite-output stage](VP8-OVERWRITE-OUTPUT.md).

The hypothesis was to retain one borrowed arithmetic window across a macroblock's
24/25coefficient blocks, removing repeated construction/refill and scalar publication.
The actual live token loop moved onto the existing BlockReader. A shared scheduler
finally published the completed prefix; entry and every token/category/sign poll,
strict272B/block partition guard and one-block/short-tail fallback remained.
No geometry-loop duplication, new heap storage, owner, pool or numerical policy.

| Complete live opaque decode | Actuals | Managed µs | FFmpeg µs | Managed GC B/op | Interpretation |
| --- | ---: | ---: | ---: | ---: | --- |
| Qualified original, reused | 12 | 5,162.06 | 4,790.47 | 228,864 | Current accepted cost |
| A: shared window | 12 | 5,311.80 | 4,794.63 | 228,864 | Confirmed regression |
| B: captured input span | 3 | 5,287.07 | 4,792.51 | 228,864 | Unpromising short; not selected |

A loses149.74µs/2.90%raw and2.81%native-normalized; both filtered/all-actual raw and
normalized intervals separate. Native movement is+0.087%. Its preceding live short
5323.83µs and ordinary short5021.39µs remain. Kernel size falls3789→3631B and stack
232→184B, while its caller grows1411→1912B/136→200B. Smaller code is not a gain.
One bounded revision captures the readonly input span before the token loop,
avoiding repeated delivery from the receiver without copying input bytes. B's live
short remains unpromising; ordinary short5015.93µs is inconclusive. B establishes
neither regression, equivalence nor improvement; no independent confirmation or
further sampling follows. This is an implementation result, not a permanent ban
on a materially different window design.

All runs use the quiet VM, Release/oneCPU, frozen72380B opaque input/exact531468B
RGBA, equivalent complete parsing/reconstruction/filter/color/output/disposal and
original source/alpha/token/owner/idle-v2 gates. Native contexts/private scratch
and the existing default result pool retain their documented boundaries.
There is no new admission, retention or allocation saving to trade against CPU.

Both versions pass36internal/260public checks per normal/scalar mode on both OSs.
Independent bitwise oracles cover Pixel/Validation windows, every completed cursor,
following reads, cancellation after0/1/23/24blocks and within-block failure prefixes.
All token/category/sign helpers remain source-identical. Two immutable core/embedded
versions per OS; no public/harness/native/selector/fixture build. A JIT collector
assertion expected the old wrapper body after inlining; its completed logs were
recovered without repeating any tests or measurements.

Qualified original sources, main/embedded DLLs and dependent bindings are restored
and byte-verified on both OSs. Experiment-only production hooks/tests are removed;
their sources/modules remain in raw evidence. The actual package is unchanged.
Reuse the5801full/live/scalar/Windows/Linux/package proof; no redundant full or
package qualification. No displayed comparison changes or renderer dependency.

Raw`vp8-live-window-prototypes.tar.xz`,1466120B/146members,
SHA256`bd3315834f13eb7ba6b3b098f37b6ac9dd9a3816570f68500b1583c710c80ee1`,
is mirrored/member-verified, with prior-archive providers for unchanged inputs.
Every stage, screen, confirmation, generated-code observation and restoration is
retained. The next independent task is current progressive JPEG attribution.
