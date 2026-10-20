# Cooperative memory qualification

The 0.2.0 development implementation exposes caller-owned admission through
`ImageLimits.MemoryBudget`. The production package contains only the interfaces
and integration hooks; a governor implementation is supplied by the caller.

The bounded qualification passed **4,248 tests per OS** on Windows and Linux:
1,439 internal, 1,881 public API and 928 live FFmpeg comparisons. Another
**36 cooperative-memory tests per OS** passed with intrinsics disabled. The same
actual dependency-free Release package passed an isolated consumer and all three
usage-guide examples on both systems, including the new interfaces, asynchronous
codecs, retained images, and independent font/layout disposal.

Focused tests cover concurrent aggregate admission, borrowed caller scopes and
samples, independently retained results, clone/replacement peaks, overlapping
encoded-input growth, nested output callbacks/orientation, local work quotas,
terminal refusal, cancellation during actual reads/writes, cleanup and subsequent
reuse. Forced refusal at every growth admission of twelve fixed tiny operations
covers PNG/JPEG/VP8L/VP8 decode/encode, fonts, layouts, resizing and inspection.
No quality/shape grid was added. Allocation sites release unused admission on
allocation failure; qualification does not induce process-wide exhaustion.

Governed operations allocate uncached storage so their releases leave no unowned
pixel, coefficient, encoded-input or encoded-output pool capacity. Minor metadata
and image-independent format constants remain approximate runtime headroom.
This qualifies the implemented memory contract, not text shaping, Lanczos3,
future paths, compression-efficiency parity or warm performance of these changes.

Retained evidence identities (SHA-256):

- Source inventory: `408797434bdd1abab04827568d6ab43c252cc496ec9f0b5b97b3b6b4be1c2fe7`.
- Windows producer: `4c79daf9ab28932dc0874e64685550be1345da522d91d2ee184bfc10868796d0`.
- Linux producer: `7833889a33a722681d6ea08b2c8702a61a6c2fa47ac91eabc707cfb17b32c449`.
- Actual `Lokad.Image.0.2.0.nupkg`: `e38c5cf43a3a48a6ba397313b1b789d6206040b0ba7d611b7971dedf22343255`.
- `p1-qualified-windows.tar.gz`: `35cd2ed337283056c8d99e93ae201e430e13a69ceabf09b105ed4043ac66e542`.
- `p1-qualified-linux.tar.gz`: `42be8d4e9542891ed145833230e79c16c0afbac04dd573b2bc328c9f82c53f7e`.

Published 0.1.0 artifacts are preserved. No package was published by this round.

Four additional four-component JPEG owner/peak cases now pass within29 focused
CMYK/YCCK tests normally and with intrinsics off on both OS. They cover retained
output charges, poisoned-buffer overwrite and survivor independence, every growth
refusal, cancellation at output admission, subsequent success and the unchanged
caller-plane peak bound. Borrowed roots remain usable. These tests run against
the restored qualified producer after the [storage trials](../benchmarks/WARM-JPEG-STORAGE-TRIALS.md);
production sources/DLLs and the actual private package are unchanged. Full-test
and package proofs are reused. This adds functional memory evidence, not a
performance or broader compatibility claim.
