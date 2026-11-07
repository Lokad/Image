# VP8 shared live block execution

Cancellable coefficient decoding now borrows the production block reader's unshifted
reservoir, normalization, refill and publication. Arithmetic state stays local across
the coefficient block. Explicit live token/category operations inline into that block;
every decision still checks the borrowed cancellation token before consuming input.
A finally publishes the exact completed scalar prefix, including on cancellation.
Duplicate reader fields, construction, normalization, refill and publication are removed.

The independent complete-operation confirmation saves **284.78µs/5.11% raw** and
**5.17% native-normalized**. Both filtered and all-actual confidence views separate:
candidate/original raw interval[0.945420,0.952406], normalized[0.941023,0.957210].
Native movement is+0.067%. This is a complete live opaque control, not a broader
decoder or throughput claim. FFmpeg parity remains open.

| Frozen513×259 live opaque decode | Managed µs | Native µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Contemporary qualified original,12actuals | 5,573.57 | 4,789.26 | 1.164 / 1.168 | 760,564 |
| Current independent,12actuals | 5,288.78 | 4,792.47 | 1.104 / 1.106 | 760,564 |

Both sides use the same72,380B packet, exact531,468B RGBA output, frozen source/native
settings and one quiet VM CPU. Parsing, reconstruction, filtering, color conversion,
independent output ownership and disposal remain timed. Caller cancellation-source
preparation is outside timing; native codec/conversion contexts remain retained and
explicitly labeled. All original pixel/alpha/source/ownership and idle-v2 gates pass.

Stage A simplified storage but left separate token/category calls: short5,664.10µs,
no gain established. Actual optimized code exposed their eight-argument/ref-state
boundary. Stage B inlines those calls, matching the existing noncancellable block
schedule: short5,307.35µs followed by the contemporary original and independent
confirmation above. The final3,789B body has a232B stack subtraction; no separate
token/category body remains. Code shape motivates the change but does not establish
the measured gain. All stage A/B source, modules, JIT, failures and observations are
retained. Earlier adverse generic sharing experiments remain distinct evidence.

Total managed allocation stays760,564B/op. Added owned heap and retained buffers are
zero. Existing8KiB decoder workspace admission covers local state; no owner, pool,
cache or session is added. Whole-operation physical/admitted peak and native
allocation are not remeasured. The earlier ALPH peak/output figures apply only to
that unchanged ownership path and are not live opaque measurements.

Only cancellable coefficient execution changes. Noncancellable arithmetic and
execution are unchanged; its shared token primitive only changes visibility. Token
order, typed eleven-byte model rows, category values and sample publication remain
exact. Five other reader controls reuse valid unchanged-path costs; all six remain
gaps. All125 displayed numerical comparisons remain source-valid and unchanged;
the concise benchmark summary records this named material control.

Windows/Linux each pass **5,764 full/live checks** (2,508internal/2,308public/948differential)
and65internal/64public per normal/scalar mode. Existing4,096token cases,1,024live
blocks,305cancellation ordinals,273B maximum partitions across eight initial phases,
native frames, refusal/cleanup and independent survivors remain exact. Direct tests
exercise the actual shared production state/helpers; obsolete instance wrappers are
removed. No ISA guard/kernel changes; unchanged disabled-ISA proof is reused.

The SAME actual private0.2.1 Release package and all three examples pass both OSes.
Package SHA256`aba6be82e035e1db42b01ccf6220490394eccc66fa7f115f77987271f271e500`,420,831B.
Published releases remain immutable. Publication, a full actual-package release
refresh and all283-path parity remain open.

[Evidence](vp8-live-block-execution-evidence.json) pins source applicability,
observations, qualified modules, package and mirrored/member-verified raw inputs.
Raw`vp8-live-block-execution-qualified.tar.xz`:3,876,928B/194members,
SHA256`0996b584a9c36e468d1cb9579161db0bff3855960d21bba6670821c8b16510f9`.
[Earlier block execution](VP8-BLOCK-STATE-RESERVOIR.md) and the
[unshifted live reservoir](VP8-UNSHIFTED-LIVE-RESERVOIR.md) retain historical results.
