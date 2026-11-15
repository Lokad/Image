# Refill-call inlining

Inlining the shared refill at its read/skip/marker callers saves **15.99 ns /
6.32%** in an independent complete async JPEG probe comparison:
**0.253131 → 0.237141 µs**, paired ratio interval **0.926273–0.947561**,
still **568 B/op**. The all-actual mean intervals also separate. This scoped
managed-DLL effect is not FFmpeg parity or a gain added to earlier stages.

Removing one annotation reconstructs the complete original source. Every body,
read size, check, counter, cancellation point, pending helper and owner is verbatim.
No new buffer, cache, scope, public API or production dependency.

| Original complete boundary / labeled FFmpeg AVIO-header adapter | Lokad / reference µs | Conservative upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| ProbeDimensionsPng | 0.115 / 0.122 | 0.965695 / pass | 568 |
| ProbeDimensionsWebp | 0.114 / 0.123 | 0.957043 / pass | 568 |
| ProbeDimensionsJpeg | 0.212 / 0.193 | 1.126636 / gap | 568 |
| ProbeDimensionsJpegAsync | 0.240 / 0.206 | 1.201432 / gap | 568 |
| PngHeaders | 0.291 / 0.580 | 0.509395 / pass | 568 |

All five use the SAME actual immutable timed/package DLL, twelve actuals, both
original native brackets, exact independent descriptors and quiet guards. PNG/WebP
dimensions and complete PNG headers pass/stop; JPEG sync/async remain gaps. No
whole-library parity or general codec-parser comparison is claimed.

The complete JPEG-header guard first has an adverse but uncertain three-pair point.
One targeted independent confirmation gives **5.990315 → 6.010557 µs**, ratio
interval **0.998398–1.008379**, **8,776 B/op** unchanged: no regression or speedup
established. Sync-JPEG's guard is also uncertain. Retain every sample; no favorable
retry follows. Historical standalone clock differences are not causal gains.

Actual code verifies refill calls fall from one to zero at each selected caller:
ReadAsync **690 → 1,977 B**, JpegHeaderMarkerAsync **516 → 1,706 B**, SkipAsync
**1,041 → 2,247 B**. Their former shared refill was **1,181 B**; selected combined
native code grows **2,502 B**. The JPEG parser stays **7,451 B**. These are code sizes,
not stack/physical/JIT peaks or CPU-phase costs. Instrumented elapsed is discarded.

One bounded current-process two-second profile supplies **1,993 locations**,
including **324 in FillAsync**. Same-process map/JIT matches locate leads; sample
counts, inlining and skid are not removable-time forecasts. A preceding Boolean-
completion redesign is inconclusive (**0.251812 → 0.251658 µs**, interval
**0.900262–1.109443**, **568 B/op**) and restored exactly before this hint trial.
It receives no confirmation or broad/package campaign; all source/module evidence remains.

Windows/Linux each pass **6,721 full/live checks**, focused **10 internal/148 public**
normal/scalar and the **SAME actual Release-package consumer/all three examples**.
Affected performance reconciles before this single broad/package milestone. One
accepted core/embedded build per OS and one pack, plus one earlier unretained Linux
core/embedded build. Zero public/harness/native/helper/fixture builds; the timed
accepted Linux DLL is never rebuilt. Unchanged production paths reuse valid evidence.

Managed allocation remains **568 B/probe or PNG-header operation**, **8,776 B** for
the complete-JPEG-header guard. Admission stays **512 B dimension /8,448 B bulk maximum**,
zero added owned capacity/cache/retained operation storage. Allocation, logical
capacity and unmeasured physical/native/JIT peaks remain separate. Original input,
reference settings, exact encoder packets, managed pixels and ownership are preserved.

[Evidence](refill-call-inlining-evidence.json) pins all samples, code, producers,
restoration, actual package and qualification. Private development package
`fac9093764c7a6ebc1003c574bfe06a79a9b993a072d03de4d0e0bb4be1f150d` (465,932 B) contains timed DLL
`9351a52aa369275a795176dc2c7e26a84ac70d6f7d3d6254d500aaf943ebdccd`. Raw `refill-call-inlining-qualified.tar.xz`
(6,597,504 B/254 members), SHA256
`30c3060d4515c28faf7f48f3241b95b7c872b774c63393aa91e16ee35467e82c`, is mirrored/member-verified with original helper/reference
and baseline-package dependencies. [Completion-leaf inlining](REFILL-COMPLETION-INLINING.md)
retains the preceding implementation/history. The full **289-path** objective remains
open; published releases are immutable and publication is postponed.
