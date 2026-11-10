# Cached-wide certified publication

Complete RGBA8 bicubic513x259->128x64 saves **32.48us /
7.58% raw /6.66% reference-normalized**,
428.44->395.96us. Filtered/all-actual raw and normalized confidence
separate; native movement is-0.990%. All twelve actuals remain,
including candidate variation; no unchanged primary repeat. Upper1.206 remains a
gap. This is a scoped incremental gain, not full parity or a whole-release refresh.

| Existing complete bicubic control | Lokad us | zimg us | Ratio /upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Byte ->128x64 | 395.96 | 345.54 | 1.146 /1.206 | 198,450 | gap |
| Word ->128x64 | 374.01 | 413.94 | 0.904 /0.912 | 198,447 | pass |
| Byte ->64x32 | 340.60 | 358.12 | 0.951 /0.959 | 197,681 | pass |
| Word ->64x32 | 357.18 | 388.11 | 0.920 /0.924 | 197,677 | pass |
| Byte ->32x16 | 329.13 | 376.50 | 0.874 /0.898 | 197,298 | pass |
| Word ->32x16 | 325.51 | 411.86 | 0.790 /0.806 | 197,296 | pass |
| Byte ->16x8 | 316.76 | 496.20 | 0.638 /0.648 | 201,203 | pass |
| Word ->16x8 | 288.93 | 497.39 | 0.581 /0.584 | 201,202 | pass |

The eight existing support/depth controls have **seven passes /one gap** on the
actual package DLL. Other fresh costs establish no causal companion gain or
equivalence. The first word64x32 confirmation was uncertain (upper1.025).
One unchanged-build countercheck found387.27us versus candidate385.91us, with
material native movement395/393->416.65us. One targeted candidate confirmation
resolves that uncertainty at357.18/native388.11us, upper.924. The original uncertain
run, countercheck and every actual remain; no production tuning followed it.

## Shared publisher and exactness

The cached-wide row writer selects byte/word mode at entry. Byte calls reuse the
existing ordinary-alpha reciprocal/Newton certificate only when the public tap
upper bounds are each<=20. The prior400-term/2^-40 proof and2^-16 margin apply.
Ineligible supports, alpha<16, ambiguity and exact halves retain the original
wide division certificate and individual original-order repair. Managed pixels,
coefficients, accumulation order, normalizers, cache geometry/scheduling and odd
tails remain exact. A source undo reconstructs both original files exactly.

One focused prepared-row test exercises the actual writer for byte/word and full/
tiny alpha, a sample refused by the old broad margin but certified by the shared
helper, true-half away-from-zero repair, an independently certified neighbor,
odd-tail replay, NaN canaries, unchanged source, zero allocations/shared admission,
cancellation before writes and recovery. Existing helper/rational/resource tests
remain. Initial fixture namespace/stackalloc and context-construction errors were
corrected in tests only; failed logs remain. The production DLLs never rebuilt.

Generated code contains byte reciprocal publication and its original division
fallback. Word excludes byte reciprocal imports. Parent code8952->9550B and its
static frame1144->1560B; the word publisher2641->2092B and both row frames536B.
These observations are not elapsed-time or physical-stack-peak measurements.
No arrays, heap capacity, owner, pool, session or public API are added. Primary
admitted workspace198432B, independently retained result32768B and retained scratch0
remain. GC198450B/op versus198448B/op is amortization, not a new intrinsic allocation.
Physical/native/total JIT-stack peaks remain unmeasured. Refusal stays terminal.

## Boundaries, generalization and qualification

Complete public Resize includes admission, geometry/workspace preparation,
filtering, independent output and disposal. The explicitly labeled FFmpeg zscale
zimg backend retains graph/input/scratch; declared conversion, clamped halo,
filtering, packing, output allocation/release and native float bounds stay timed.
This is not equivalent fresh-graph timing. Exact managed pixels remain distinct
from approved native-only pixel bounds. One CPU, sequential Release, frozen
original inputs/settings/output identities and idle-v2 guards all pass.

The existing managed-only4097x2051->1024x512 diagnostic refreshes once:
default**21.61ms**/compact**31.36ms**, three actuals each.
Its short intervals separate within this pair; there is no independent generalized
or historical gain, native-parity or default-promotion claim. No new worker,
image or settings grid. Normal/scalar/prior complete frames and independent owners
match. Default/compact GC1585372/153Bop, workspace1585440/4096B, governed peak
3682592/2101248B and retained result2097152B are separate measurements.

Windows/Linux each pass **6158 full/live checks**,332internal/729public resize
per normal/scalar/AVX2-disabled/AVX512-disabled mode, and the SAME actual Release
package/all three usage examples. One production build per OS is reused; three
successful embedded-test versions include test-only corrections. Harnesses,
selectors, native workers and fixtures require zero builds. The unchanged package
consumer already covers the primary default byte/word frame and shared ownership.
Encoder packets, strict metadata exceptions and published packages are unchanged.

[Evidence](cached-wide-certified-publication-evidence.json) owns current costs,
source/code proof, qualification and invalidation. Mirrored/member-verified raw
`cached-wide-certified-publication.tar.xz`:6961832B/266members, SHA256
`5ba34c4cad295dcf819ea9a4e8699667db7b60feaa74e3a75603d45f67cbc824`. Private0.2.1 package442264B, SHA256
`33268e22dbb0e90f9a89ee5b8b51c3d5519ed26ea5f812b656b021152aebd70c`. Publication stays postponed.
[Attribution](CACHED-WIDE-PUBLICATION-ATTRIBUTION.md) and
[shared windows](CACHED-WIDE-SHARED-WINDOWS.md) retain earlier design, results and raw
history. Larger bounded-byte and independent JPEG leads remain eligible;
all283material paths remain required.
