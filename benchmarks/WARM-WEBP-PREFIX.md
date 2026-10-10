# Warm VP8L short-prefix lookup

One production primitive replaces repeated short-code bit traversal with an
independently generated canonical lookup. The same frozen VP8L input and
original raster are reused. The retained screens do not certify the 2x target
or uniform parity across codec and editing paths.

The [original numeric evidence](warm-webp-prefix-evidence.json) retains
actual managed samples, native brackets, uncertainty, allocation diagnostics,
source/binary identities, exact input/output hashes, controls and quietness.
Regenerate with `python benchmarks/render-warm-webp-prefix.py`.

| Stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper | Allocated bytes/op |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Baseline | 1 | 16520.21 | 2147.51 / 2148.84 | 7.69x | 10.86x | 1,069,577 |
| Baseline | 2 | 16023.08 | 2151.31 / 2150.33 | 7.45x | 8.28x | 1,069,577 |
| Short prefix | 1 | 4438.84 | 2205.94 / 2166.36 | 2.05x | 4.42x | 1,074,292 |
| Short prefix | 2 | 4400.77 | 2149.63 / 2157.03 | 2.05x | 2.42x | 1,074,292 |

Measured 2026-10-10 UTC, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). One pinned CPU,
one codec/conversion thread, normal SIMD, tiering disabled and non-concurrent
workstation GC. The managed mean spread across the candidate repeats is
0.865%. All candidate actual samples below all baseline actual samples:
true.

A separate accepted-baseline CPU profile attributes 69.65% of workload CPU
time to Huffman decoding, including 56.70% in bit reading. Method matching
excludes parameter types. Those baseline workload loops include warmup and
calibration; the profile is separate from parity timing and does not report
candidate percentages.

Canonical codes follow the [public lossless specification](https://developers.google.com/speed/webp/docs/webp_lossless_bitstream_specification).
The lookup covers codes of at most eight bits, with packed symbol/length entries.
Smaller complete trees use their maximum code width. Single leaves remain
zero-bit codes; trees with only longer codes allocate no lookup. All entries
are generated from the encoded lengths. No upstream code or table is copied.
Required storage and bounded construction work are admitted before allocation.
Complete code reads retain the original logical bit charge and commit only
required bytes. Long codes, incomplete input and tight work budgets use the
original canonical path without speculative reader mutation.

Thirty-seven additional cases cover 240 uniform cached symbols, 72 mixed
short symbols, eight cached one-bit controls, 64 partial short-code budgets,
128 long-code budget controls and five truncated/budget precedence cases.
Code/field values, exact required bytes, source integrity, construction limits
and cancellation remain checked. All 68 lossless decode and 54 lossy
alpha/orientation memory profiles retain independently derived exact peaks
and rejection one byte below. Prefix storage raises some small-image peaks;
single leaves and wholly longer trees retain their original tree charges.

All 1,967 tests pass on Windows and Linux: 387 internal, 686 public and
894 live FFmpeg comparisons. The actual dependency-free Release package
passes both platforms and all three usage-guide examples. Package/measured
DLL method-body fingerprints match; both reproduce every original RGBA byte
of the frozen VP8L input. Package SHA256:
`7a92c4a74392017847207641931130db8c7eb2adca9e0811946044cf7719fe67`.

Inclusive BDN allocation changes are [4715, 4715] bytes/op.
Four main eight-bit lookups and the smaller distance lookup admit 4,512
additional live bytes in this input; temporary code-length lookups admit
40 bytes each and are released before the corresponding main tree. The
[resource contract](../docs/resource-admission.md) specifies per-tree charges.

One existing 513x259 raster, one frozen 533,100-byte VP8L input and exact
531,468-byte RGBA output are used. No preparation or quality enumeration is
repeated. Alpha and zero-alpha hidden RGB remain exact. Full public decode,
independent output acquisition and disposal are timed; native software webp
state reset, ARGB-to-RGBA conversion and independent output allocation/release
are included. Codec/conversion contexts and wrappers are reused; no decoded
pixel cache replaces work. Startup, IPC, I/O, generation, hashing and checks
are excluded. Other VP8L strategies and VP8 decode remain unmeasured.

Failed quietness samples and later passing cooldowns are retained. Reproduce
sequentially on the quiet VM with the
[worker instructions](../tools/WarmReference/README.md#still-vp8l-webp-decode).
