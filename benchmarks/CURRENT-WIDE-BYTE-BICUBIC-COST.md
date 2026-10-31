# Current wider-support byte bicubic cost

These timings precede the [exact-alpha producer](WARM-EXACT-ALPHA-INT128.md), which supplies the latest costs for these paths. Original evidence is retained.

The existing513×259 RGBA8→128×64 control confirms **1.521× / upper1.532**,
**528.948 µs** and **231,239 allocated B/op**, against the existing zimg backend
used by FFmpeg zscale. It remains a gap. Its greater-than16tap support exercises
WideCachedBicubicResize,distinct from the default and4KiB Down8 paths.

One independent twelve retains all actuals,filtered/all confidence,native
brackets and passing quiet-v2 guards. Exact rational/frozen pixels,input/output/
native hashes,native max1/RMSE≤0.25,normal/scalar bounds,source immutability and
independent output ownership pass. Complete public/native normalization,
filtering,output allocation and release stay timed;native allocation is unmeasured.
No new input,shape,depth,filter,settings or workload grid is introduced.

**Zero builds/profiles**: reuse the accepted immutable production,harness,
completed selector,fixtures and source-verified worker. Unchanged full5,245/live
checks per OS and the same actual Release-package/three-example proof are reused
from the [accepted certificate change](WARM-BICUBIC-BYTE-FIXED-CERTIFICATE.md).
The old independent producer predates wide-kernel/admission/cancellation changes;
this is not a causal speedup claim against it.

A postcheck encounters the old receipt omitting the default/null limit field;
its boundary description also gains “Default managed working limit” and one
capital letter. All other characters,operation and correctness gates match.
Original successful samples are recovered without retiming. The current native
worker retains compiler/flags/buildscript/zimg archive and complete processing;
shared-source differences only concern an inactive1×1 branch,default halo alias
and readiness metadata. No comparison gate is weakened.

[Numeric evidence](current-wide-byte-bicubic-cost-evidence.json) records complete
samples,producers,bounds and qualification reuse. Raw
current-wide-byte-bicubic-cost.tar.xz,SHA256
7f3648b449e667d88cafef190b1e0a669f41312f087404cfa34b9616518361b0,
is mirrored/member-verified with sources,actual runtime/worker,selector and recovery.
Use existing WarmBicubicResize/Wide8 with the
[reference tooling](../tools/WarmReference/README.md). All268required paths remain;
the larger tight-memory gaps take priority. The summary now has68comparisons.
