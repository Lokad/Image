# Bicubic word-guard vector construction: rejected

The existing TightCross16 control keeps its original513×259RGBA16 source,
256×519 output and input+output+4KiB working limit. Complete public Resize,
independent owned output and disposal are timed on quiet CPU0. The unchanged
reference is zimg, FFmpeg's zscale backend. Exact managed rational/frozen pixels,
native RGB/alpha maximum1/RMSE≤0.25, source and ownership checks pass.

| Three-actual screen | Managed µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Rejected vector guard | 1,297.224 | 1.457 / 1.566 | 1,063,410 |
| Fresh qualified baseline | 1,263.401 | 1.522 / 1.652 | 1,063,404 |

The native bracket shifted from the earlier release screen, motivating one fresh
baseline before deciding. The resulting normalized ratio difference is only4.3%,
and candidate elapsed is higher. These unmatched short screens establish no
useful5% gain, speedup or regression. The six-byte allocation difference is
harness amortization; owned storage is unchanged. No variants or independent
confirmation follow. Production source and actual DLLs/package are restored.

One six-second current-core trace has5,929 samples: CrossWord TryApply87.2% and
coefficient generation7.6%. All5,168 kernel instruction pointers match an identical
5,059-byte replay. The largest normalization block contains1,263 samples. These
are diagnostic shares, not phase timings. One tiny host is built; no production/
harness/native build is needed for attribution.

The rejected primitive constructs paired divisors and power-of-two rounding
margins with vector operations, retaining identical divisions, bits, guards and
publication. Useful regression coverage is retained:1,008 original arithmetic
controls freeze68 accepted results and every refusal/output byte against the
actual pre-adoption qualified DLL, with independent scalar rounding and output
canaries. The initial all-refusal fixture is corrected before production edits;
its failed coverage assertion remains. Windows lacks AVX512, so SIMD-oracle
execution is claimed only on Linux; portable public checks pass on both.

Candidate/restored checks pass37 affected public controls normal/scalar perOS,
plus the new internal test. Unchanged full and same-actual-package evidence is
reused. Postprocessing initially compared a minimal older native witness with
the newer full metadata object; every original source/build/worker/archive/flag
pin was unchanged. Added provenance fields were reconciled without repeating
the completed benchmark. All actuals, brackets, guards and failures are retained.
The accepted summary and full268-path objective stay unchanged.

[Numeric evidence](bicubic-word-guard-vector-rejected-evidence.json) retains
producers, complete samples, uncertainty, oracle and restoration. Mirrored/member-
verified raw bicubic-word-guard-vector-rejected.tar.xz, SHA256
1827c7bb8245ec80eb8aa527f7dd2aa183751e9e7271235955231382cf4fcff0 (206members),
retains trace, matching JIT, source, selectors, checks and receipts.
Use existing WarmShort/TightCross16 with the [reference tooling](../tools/WarmReference/README.md).
