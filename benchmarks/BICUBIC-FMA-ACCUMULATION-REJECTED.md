# Bicubic strip FMA accumulation: rejected

The existing TightCross16 screen measures 1,472.146 µs, 1.578× (upper 1.956),
1,063,404 B/op. It fails the 5% gate against the applicable fresh 1,263.401 µs
baseline. Exact managed rational/frozen pixels, native RGB/alpha maximum1/
RMSE ≤0.25, input, owners and quiet guards pass. No gain/regression claim follows
from unmatched short screens. No variants or independent confirmation follow.

The trial replaces three weighted multiply/add loops with fused operations.
Before edits, actual 256/512-bit FMA controls compare against exact dyadic
BigInteger sums. With at most 10×5 taps and stored weights bounded by 2, expanded
absolute term sums are at most 200U. The conservative γ200 bound gives less than
2⁻³⁷U error per evaluation, so the difference from original order is less than
2⁻³⁶U. The existing guard budget is 2⁻³²U, for U=65535² (RGB), 65535 (alpha),
1 (normalizer). Stored coefficients, tap order, guards, exact replay and 4 KiB
admission stay fixed.
These numerical controls establish safety preparation, not a performance gain.

The complete-operation screen finds no useful gain. Production FMA calls and
its hardware gate are removed; source and qualified actual DLLs/package are
restored exactly. The dedicated unadopted-algorithm test is removed after archival.
Candidate/restored 16 affected public cases pass normal/scalar per platform;
proof execution is Linux-only because Windows lacks AVX512. Unchanged full and
same-actual-package qualification is reused. No published package is modified.

[Evidence](bicubic-fma-accumulation-rejected-evidence.json) retains samples,
uncertainty, pins, proof and restoration. Mirrored/member-verified raw
bicubic-fma-accumulation-rejected.tar.xz, SHA256
d95c9f1d6843c1bba0f1379dd2d49d17826d1681f1259c503433c49abbc07876 (143 members),
retains the removed numerical test and candidate source along with producers,
selectors, checks, guards and receipts. The accepted library and full 268-path
performance objective remain unchanged.
