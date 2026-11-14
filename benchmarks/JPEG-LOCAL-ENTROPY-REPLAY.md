# Local JPEG entropy replay

Complete paired confirmations save **167.72 µs / 10.49%** for caller-RGB420 and
**196.77 µs / 16.68%** for RGBA8→gray. Each follows a short screen; twelve alternating
original/candidate pairs run 64 complete operations per subject, retaining every
pair. Their ratio intervals are 0.882–0.909 and 0.829–0.837. Packets and allocation
behavior are unchanged. These are scoped incremental effects, not historical
stage sums or claims about other content/settings.

Adaptive replay keeps its bit reservoir local across records, publishing it once
on success. Both encoder callers own the writer privately and abandon failed
state. One shared packing kernel serves replay and direct/restart writing. The
unused WritePair method and two tests solely preserving it are removed; exhaustive
packing tests now exercise actual replay. Code/amplitude validation, per-record/
MCU cancellation, stuffing, first 256/subsequent 512-byte flushes, published stream
prefixes, failure disposal and lazy admission retain their observable behavior.

The same immutable package DLL supplies these 18 existing complete 513×259 controls:

| Complete control | Lokad / reference µs | Ratio / upper | GC B/op | Packet B, Lokad / reference | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8 → 422 | 1,802.71 / 2,347.63 | 0.768 / 0.771 | 529,871 | 71,166 / 71,168 | pass |
| 420, forward output8 | 1,405.32 / 1,761.88 | 0.798 / 0.803 | 388,247 | 52,091 / 52,093 | pass |
| RGBA16 → 422 | 1,823.24 / 2,418.01 | 0.754 / 0.758 | 529,871 | 71,208 / 71,210 | pass |
| 420, forward output16 | 1,491.95 / 1,838.09 | 0.812 / 0.816 | 388,247 | 52,115 / 52,117 | pass |
| 420, live-token16 | 1,533.15 / 1,836.72 | 0.835 / 0.840 | 388,214 | 52,115 / 52,117 | pass |
| RGBA8 → 420 | 1,409.87 / 1,767.03 | 0.798 / 0.802 | 388,215 | 52,091 / 52,093 | pass |
| RGBA16 → 420 | 1,478.82 / 1,835.61 | 0.806 / 0.809 | 388,215 | 52,115 / 52,117 | pass |
| RGBA16 → 420 / shared admission | 1,525.81 / 1,841.57 | 0.829 / 0.832 | 819,475 | 52,115 / 52,117 | pass |
| Caller RGB24, async nonseekable420 | 1,422.33 / 1,626.70 | 0.874 / 0.878 | 309,523 | 52,091 / 52,093 | pass |
| Caller RGB24 → 420 | 1,431.80 / 1,578.72 | 0.907 / 0.912 | 388,222 | 52,091 / 52,093 | pass |
| RGBA8 → 444 | 3,057.75 / 3,776.90 | 0.810 / 0.821 | 820,088 | 133,039 / 133,041 | pass |
| RGBA16 → 444 | 3,157.28 / 3,847.23 | 0.821 / 0.827 | 820,088 | 133,041 / 133,043 | pass |
| 444, live-token8 | 3,105.10 / 3,777.72 | 0.822 / 0.829 | 820,088 | 133,039 / 133,041 | pass |
| RGBA8, async nonseekable444 | 3,106.93 / 3,858.03 | 0.805 / 0.810 | 820,502 | 133,039 / 133,041 | pass |
| Caller GRAY8 / SIMD libjpeg-turbo | 889.23 / 1,077.61 | 0.825 / 0.834 | 155,439 | 58,910 / 58,910 | pass |
| 444, forward output8 | 3,083.86 / 3,772.61 | 0.817 / 0.825 | 820,120 | 133,039 / 133,041 | pass |
| RGBA8 → grayscale / SIMD libjpeg-turbo | 962.45 / 1,408.06 | 0.684 / 0.688 | 155,435 | 58,910 / 58,910 | pass |
| RGBA8 → 420, row restart33 | 2,112.08 / 1,610.79 | 1.311 / 1.317 | 151,179 | 58,010 / 58,012 | gap |

**17 pass / 1 gap.** All replay controls pass and stop. The row-restart path remains
**1.31× FFmpeg**. The unchanged contemporary original also has a confirmed gap
(2,188.66 / 1,642.50 µs, interval 1.326–1.337); the candidate interval is 1.306–1.317.
This resolves the suspected new regression without a favorable candidate repeat.
It is not a paired causal restart-speedup claim. Keep this required gap explicit
and profile its fixed-table/direct entropy path next. No whole-library parity claim.

Color references are FFmpeg n8.1. Grayscale uses explicitly labeled SIMD-enabled
standalone libjpeg-turbo 3.1.4.1. Native contexts/private conversion storage remain
prepared as declared; each operation still converts/encodes the original input
and owns/releases an independent packet. Managed timing includes validation,
normalization, all preparation/encoding, fresh output growth/staging and disposal.
Frozen settings/inputs, exact packets and quality sanity gates pass. Color differs
only by the approved strict 2-byte attribution exception; grayscale is raw-exact.
Equal quality numbers are not the equivalence proof. CPU0, sequential Release,
idle-v2 guards, every actual and both native brackets remain.

No new heap capacity, pool, cache or retained state. Writer admission stays 640 B,
its buffer 512 B and retained writer storage after disposal zero. The existing 4 KiB
stack bank fits its 16 KiB table owner. Governed RGBA16 peak remains 1,704,856 B.
The existing 4 MiB coefficient-pool ceiling and shared-root bypass are unchanged.
GC allocation, admitted capacity and idle retention are distinct; physical/native/
JIT peaks are unmeasured. Small GC rounding differences are not allocation gains.

Windows/Linux each pass **6,640 full/live checks** plus 464 internal/379 public focused
cases in each normal/scalar mode. Exact replay corpora, output/cancellation/refusal
cleanup and independent owners pass. Unchanged arithmetic/ISA evidence is reused.
The SAME actual Release package and all three usage examples pass both platforms.
One core/internal build per OS remains frozen through timings and qualification;
no post-timing production rebuild. Published releases remain immutable.

Tooling failures retained in raw: a helper caught wrong input before timing;
the repaired helper used the same production DLL. A missing VM log-parser
dependency and a missing archive import occurred after successful captures;
completed evidence was recovered without repeating measurements. One pre-run
idle guard failed narrowly before any workload and is retained beside its retry.

Unchanged 191 image-owner views and decoder/PNG/WebP/resize/text/inspection evidence
are reused. Previous [Huffman bank](JPEG-PREPARED-HUFFMAN-BANK.md),
[gray preparation](JPEG-GRAY-TILE-PREPARATION.md) and
[caller confidence](JPEG-CALLER-CURRENT-CONFIDENCE.md) are retained history.
All 289 material paths remain required; publication is postponed.

[Evidence](jpeg-local-entropy-replay-evidence.json) pins producers, source and
reference identities, all costs/actuals and the same-package proof. Raw `jpeg-local-entropy-replay-qualified.tar.xz`: 45,981,144 B / 989 members,
SHA256 `027a47771b282a9fadcad54cfdb989b9cd58a723974d90dd23477a84d2f554b0`,
mirrored and member-verified. Dependencies retain earlier tools/fixtures and attribution.
