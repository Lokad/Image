# Wider baseline JPEG AC cache

Current-build full-resolution RGB JPEG decoding improves from **2.040× FFmpeg**
to independently confirmed **1.695× / upper 1.709×**, reducing managed time
**16.8%**. Frozen managed pixels remain exact; final allocation is
**2,664,366 B/op**. Two existing cases also confirm on the same immutable build.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Current-build RGB screen | 2,957.91 | 1,450.05 | 2.040 / 3.322 | 2,664,373 | Above target |
| Four-byte cache, three actuals | 2,455.66 | 1,450.62 | 1.693 / 1.825 | 2,664,366 | Screen |
| RGB independent twelve | 2,460.96 | 1,451.53 | 1.695 / 1.709 | 2,664,366 | Confirmed |
| Progressive444 regression | 6,310.11 | 3,356.77 | 1.880 / 2.093 | 2,682,364 | Mean passes |
| Fresh Stream regression | 1,625.25 | 993.60 | 1.636 / 1.728 | 1,813,266 | Mean passes |
| Progressive444 independent twelve | 6,282.99 | 3,348.11 | 1.877 / 1.896 | 2,682,364 | Confirmed |
| Fresh Stream independent twelve | 1,627.42 | 997.29 | 1.632 / 1.673 | 1,813,266 | Confirmed |

Only three existing workloads and their frozen packets are used, reusing the
same original 513×259 byte raster: full-resolution direct RGB, progressive 4:4:4
and fresh seekable 4:2:0 restart Stream decoding. The wider Stream input proposal
was dropped: the public ReadInput loop never inspects CanSeek, Length, Position
or Seek, so both capabilities already use identical bounded 8-KiB buffering,
growth/copies and EOF checks. Existing qualified nonseekable-input bounds and
caller-lifetime controls remain applicable. No image, shape or settings grid is
added.

The current-source screen exposes RGB above 2×; it receives no twelve-sample run
before optimization. One six-second profile records 5,213 samples, **56.5% in
ReadPlainBaselineAc**. The accepted primitive retains a local 64-bit cache for
noncancelable ordinary short AC symbols. It checks four ordinary bytes before
refilling and decodes several coefficients before another refill. Each symbol
keeps its existing atomic prefix/amplitude work charge. Every exit returns
speculative whole bytes before publishing the logical minimum byte position,
pending bits, partial coefficients and budget. Stuffing, markers, long/invalid
codes, short tails and tight budgets use the original fallback. The cancelable
path is unchanged. No table, heap workspace or decoded-pixel cache is added.

Twenty focused checks pass normally and with intrinsics disabled per OS,
including every cached phase, budget/truncation boundaries, signed endpoints,
partial writes, predictor state, consecutive blocks and final control bits.
Managed whole-image samples, source preservation and simultaneous independent
owned outputs are exact. The native decoder gate remains RGB max 4 / RMSE 1.5
and exact opaque alpha; no tolerance is widened.

All subjects include complete public decode, conversion/packing, independent
output acquisition and disposal. The Stream case includes fresh input/AVIO,
owned packet acquisition, buffering/copies, length/EOF checks and release.
The native Stream worker's resident-source preservation comparison is moved
from the timed operation to its existing untimed source check, preserving the
check and excluding validation from timing. Actual packet/source/EOF/ownership
contracts are unchanged. Native context/frame wrappers are reused; decoder state resets each operation. Startup, I/O, IPC, hashes, fixture creation
and validation are excluded; no decoded output is cached.

Sequential Release measurements use CPU0, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Both versioned quiet
guards pass on every run. Each confirmation retains all twelve actuals; RGB
selects twelve, progressive ten and Stream eleven. The upper includes selected
and all-actual intervals against the faster native bracket's lower bound.
Allocation figures include harness overhead; native totals are **unmeasured**.

Both regression means pass before **3,483 Release tests per OS**: 1,199 internal,
1,386 public API and **898 live comparisons**. The same actual Release package's
consumer and all three examples pass on Windows/Linux. The two additional
independent case confirmations reuse that qualified immutable core/harness/
native build without another suite/package proof. One changed Stream-worker
build, one production candidate build and one profile suffice; the harness is
unchanged, with current DLLs preloaded into cached jobs.

[Numeric evidence](warm-jpeg-ac-cache-evidence.json) retains every actual,
allocation, guard, source/build/fixture/native identity, profile and qualification.
Stop these confirmed pairs; full required warm coverage remains open.
