# Shared full-raster copying

The original RGBA16 clone independently saves **1.46 µs / 7.66% raw**,
**7.41% reference-normalized**, from a contemporary 19.12 / FFmpeg 17.15 µs
baseline. Both confidence views separate; the native clock moves −0.26%.
Two byte copy controls now pass. Word copies retain small gaps.

| Existing complete control | Lokad / FFmpeg µs | Upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| Clone RGBA8 | 6.02 / 9.02 | 0.695 / pass | 112 |
| Clone RGBA16 | 17.65 / 17.10 | 1.045 / gap | 112 |
| Normal orientation RGBA8 | 6.25 / 8.52 | 0.758 / pass | 112 |
| Normal orientation RGBA16 | 19.55 / 18.90 | 1.051 / gap | 112 |
| PNG8 decode/orient/crop/fit | 1,369.97 / 1,619.91 | 0.857 / pass | 169,574 |
| PNG16 decode/orient/crop/fit | 3,168.24 / 3,161.97 | 1.012 / uncertain | 338,268 |

Clone copies the tightly packed raster without repeated row setup. Calls with
no cancelable token use one complete Span.CopyTo; cancelable calls retain bounded
16 KiB checkpoints across rows. Normal orientation uses Clone, removing the
duplicate 32-line implementation. Allocation/admission before writes, exception
cleanup, metadata, complete word/hidden-RGB samples and independent owners remain.
Identity Resize and identity Lanczos plans already use this copy boundary.

One cheap original twelve-pair screen precedes independent word confirmation.
All twelve alternating 64-operation pairs and three native brackets remain.
Companion costs are current measurements; historical clock changes do not establish
causal companion gains or regressions. The two PNG compositions are refreshed
because their timed AutoOrient(Normal) changes. Both filtered and all-actual
confidence are retained; no pipeline gain is inferred by adding stage timings.
Every original pixel/source/owner/native-identity and CPU0 idle-v2 gate passes.

Complete public operations include admission, output acquisition, materialization
and disposal. The FFmpeg public null graph retains graph/source/frame wrappers,
then allocates and copies an independent pixel payload inside each operation.
Composition includes complete stream decode, normal orientation, crop and nearest
fit, with managed intermediate owners materialized/disposed; the native graph
may retain intermediate views. Original inputs/settings and the same reference
builds remain. Startup, I/O, hashing and validation are excluded as declared.

Copy allocation stays **112 B/op**. No owned/admitted capacity, operation scratch
or retained state is added. The two-slot/4 MiB idle pool and caller-budget bypass
are unchanged. Byte/word copies borrow 531,468 / 1,062,936 B and own the same-sized
result. Word composition still has 2,455,088 B simultaneous image payload and
2,125,872 B source-derived idle storage. Physical/native/JIT peaks are unmeasured.

**6,592 full/live checks per OS** pass, with **520 internal / 1,057 public** focused
normal/scalar checks and all three named resize-storage suites. Existing row/chunk,
tiny-size, hidden-RGB, EXIF, identity, aggregate-admission and cancellation-cleanup
tests are reused. The **same actual Release package**, containing the timed Linux
DLL, passes both OS consumers and all three examples. Four added consumer scenarios
cover both copy boundaries/depths, terminal refusal, cancellation and surviving
owners. Unchanged codec/resize/text/explicit-ISA evidence is reused.

The orientation dispatcher is 243 native bytes on both immutable modules;
instructions/constants match except its intentional normal-copy call target.
Other branches reuse valid costs. One core/embedded build per OS, zero public,
harness/native/fixture builds, one Release/--no-build pack; no production rebuild
after timing. Encoder packets, decoder pixels and managed editing stay correct.
Publication is postponed; published packages remain immutable.

Current image views: **168 pass / 6 gaps / 17 uncertain** across 191 comparisons;
all **289 material paths** remain required. Stop passing byte copies; retain the
small word gaps and composition uncertainty. This is the declared control scope.

[Evidence](shared-raster-copy-evidence.json) records identities, boundaries, all
samples/confidence, source scope, ownership and qualification. Raw
`shared-raster-copy-qualified.tar.xz`, SHA256
`3e1b130aa06d03373804c547a140f7afa5b701ad3745794962c207bf529207af` (2,687,624 B / 157 members), is mirrored
and member-verified. Its dependency retains the preceding qualified package,
the [bounded horizontal stage](BOUNDED-WIDE-HORIZONTAL-QUARTET.md) and original
reference inputs/recipes. No new benchmark workload or settings grid.
