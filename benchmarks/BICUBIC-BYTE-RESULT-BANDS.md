# Result-backed byte bicubic bands

Mixed-axis RGBA8 bicubic resizing reuses unmaterialized output capacity for a
64-row band, then retires it before the original small-band suffix. A shared
numeric range implementation serves both capacities. Four-row publication
prepares two independent pair quotients before certification; a width-identity
specialization removes zero horizontal taps. Useful word/cached kernels and
portable fallbacks remain. No new public API, heap capacity, owner, cache or pool.

On the existing tight 513×259 → 1027×129 control, contemporary Windows-produced
original/candidate DLLs measure **1130.05 → 980.90 µs**:
**149.15 µs / 13.20% raw /
13.02% normalized** saved. Filtered and all-actual
raw/normalized confidence intervals separate. This is a scoped improvement;
both mixed controls still miss parity.

| Existing complete control | Lokad µs | zimg µs | Ratio upper / status | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Mixed8 | 960.04 | 866.82 | 1.116 / gap | 114 |
| TightMixed8 | 980.90 | 865.74 | 1.142 / gap | 114 |
| TightVertical8 | 503.50 | 587.18 | 0.862 / pass | 113 |

These current costs use the exact Windows-produced DLL **69e4a0bc732403633d4c4254d516eac8fd2fc0095b31d14edea7c133bbbc85f6**
subsequently packed and verified on both systems. Twelve actuals per independent
confirmation and twelve native samples in each bracket, sequential CPU0 on the
quiet VM, idle-v2 guard, tiering off and nonconcurrent workstation GC. Input
generation, validation, startup and IPC are excluded. There is no causal gain
claim for the other two controls. Passing vertical stops.

Managed timing includes complete public resize, preparation, working storage,
admission, allocation/materialization of an independent result and disposal.
The explicitly labeled FFmpeg zscale **zimg backend** retains its graph, input
workspace and scratch outside timing; packed input conversion, clamped halo,
filtering, independent output, repacking and release are timed. This is a
prepared backend comparison, not FFmpeg CLI or equivalent one-shot native setup.
Frozen inputs, settings and exact managed pixels are unchanged: full rational
references for mixed controls; the qualified original vertical byte frame with
25 certified rational alpha-half decisions for vertical. These scopes remain
explicit in the harness receipts.
Approved native-only RGB/alpha maximum1 and RMSE≤0.25 are distinct from that exact
managed contract. Q30/rational and certified rounding guarantees are preserved.

## Storage and correctness

The primary result lends **28,136 B** at [415488,443624), before geometry starting
443664; safe prefix rows0–100 use bands64+37, then suffix8+8+8+4 replaces17bands
with6. The deterministic model reduces main-filter source conversions
182115→148770 and drivercolumns17459→6162. These are applicability counts, not
phase timings or a predicted CPU gain. The65728 paired-output operations are
maximum quotient opportunities; alpha guards may skip divisions and changed
band parity changes companions. No claim of identical issued-division counts.

Total allocation stays114B/op for mixed and113B/op for vertical, with no added
owned capacity or retained scratch. The existing4KiB reservation remains.
Primary local working ceiling is1,065,496B: borrowed source531,468, owned
result529,932 and scratch4,096. Output capacity, including its borrowed workspace,
is admitted once and remains charged through its independent lifetime.
Physical/native peaks are unmeasured. Bank views retire before overlapping suffix
writes; the NoInlining core retires stack workspace before exact tiny-alpha replay.
The primary has zero deferred pixels; real tiny-alpha tests exercise replay.
Direct cancellation checkpoints remain bounded. Refusal is terminal.

All old BicubicMath methods, OriginalByte and exact replay are source-exact;
word and shared geometry sources are unchanged. Nine added checks cover189
quartet combinations/allfour acceptance masks/refused stores, five bounded shapes,
poisoned output/suffix, real tiny alpha, source lifetime, refusal/cancellation/
recovery and aggregate independently retained outputs. Both OSes pass **5,964
full/live checks** (2699internal+2317public+948differential),73internal/556public
per normal/scalar mode, and197internal/703public per disabled-AVX512/AVX2 mode.
One main and embedded-test build per OS is reused throughout.

The SAME actual private Release0.2.1 package **b43fbef4daaf7ea44db700deead2fb4b0d48d4c965c5d69152dfd5cbc6db227b**
(434764B), symbols **5ebdb0a897de7a4af5aabc82a75e57942ac21c93b2f1a8a7bef52daf03742baa**,
passes Windows/Linux actual consumers, byte-band frozen/admission/ownership
checks and allthree usage examples. Published0.2.0 is unchanged. Publication is
postponed. Other codecs and source-valid unchanged paths retain their evidence;
unrefreshed affected pipeline costs remain historical. No full release refresh
or all283-path parity claim.

## Artifact reconciliation and reproducibility

The previous [package observation](BICUBIC-MIXED-BYTE-CURRENT.md) was1319.86µs.
It is **not** the causal baseline. The contemporary Linux-produced original
measures1073.89µs and candidate977.82µs (96.08µs/8.95%raw gain), independently
supporting direction but not the package-artifact magnitude. The contemporary
Windows-produced original is1130.05µs and candidate980.90µs.

Only the library DLL/PDB/XML differ in the harness file identities. All248
compiler inputs, order and evaluated Release properties agree. Four targeted
normalized IL bodies and implementation flags agree across the original
artifacts. The timing discrepancy's root cause remains unknown. Do not attribute
it to a compiler/JIT explanation or use cross-artifact/historical pairs to claim
a larger gain. One primary package-artifact confirmation per side resolves the
actionable scope; adverse and diagnostic observations are retained without
favorable repeats. No additional production builds were made for this inquiry.

[Compact current evidence](bicubic-byte-result-bands-evidence.json) owns the
three costs and exact identities. Raw **bicubic-byte-result-bands-qualified.tar.xz**, SHA256 **c376dfbdbfacede3b61bf10b10f81972dec59f7acadde04ad96fdf66859fa05a**,
13722916B/363members, is mirrored and member-verified. Retained
MBA633/MBM634 probe/model raw19d449d2 and MXC632/VQP630 archives pin prior inputs.

Reproduce from the raw archive: MBQ635 build/focused/source-proof scripts pin
the one build per OS; MBW637 measure/observe/compare selects the exact intended
package DLL, existing MGP inputs and original RPA/zimg worker. The MBQ635 primary
short precedes independent Linux diagnostics. ABI636 inspects only metadata.
MBQ635 qualify/ISA/package scripts run full/live, portable/ISA and actual-package
checks, with --no-build production reuse and all original gates. Logs, receipts,
guards, manifests, binaries and scripts remain together; no extra workload grid.
