# Cached bicubic preparation attribution

CBA779 attributes the existing RGBA8 513×259→128×64 cached-wide operation in
an isolated copy of current owned sources. Three coarse timestamp insertions,
outside pixel loops, separate preparation and execution. Removing them reconstructs
the original source exactly. Production, its qualified DLLs and package are unchanged.

| Instrumented phase, µs/op | Batch 1 | Batch 2 | Batch 3 |
| --- | ---: | ---: | ---: |
| Admission, allocation and horizontal geometry preparation | 33.32 | 33.62 | 33.41 |
| Row decoding/filtering, vertical geometry, publication and repair | 368.09 | 367.44 | 367.64 |
| Outer dispatch, output ownership/disposal and loop overhead | 0.43 | 0.41 | 0.42 |

Each batch runs for200 ms after warmup,498/499/499 complete operations on CPU0.
The instrumented total is401.46–401.84 µs. These are diagnostic phase observations,
not new package timings, independent performance confirmations or a native-parity
claim. Instrumentation and compiling into another assembly can perturb generated
code. Do not subtract33 µs from the qualified395.96/native345.54 µs comparison.
That comparison still includes fresh managed preparation versus a retained zimg graph.

Preparation is material enough to investigate caller-owned reuse. It includes
admitted row/index allocation, horizontal kernel coefficients, normalizers,
windows/pairs and cache-ID initialization. Vertical geometry remains inside the
execution phase. The next design should share existing preparation/execution in
a small disposable bicubic plan, still reading every new source and resetting
row IDs. Keep current one-shot costs visible and establish equivalent prepared
and complete one-shot boundaries. No fixed-fixture cache or general caching
framework is justified.

Retention is a cost: a prepared plan must keep all its capacity charged between
calls until disposal. This is not an inferred peak-memory saving. The separate
governed operation measures231,200 B peak, excluding borrowed input; its32,768 B
result stays charged independently, with zero scratch after operation completion.
The diagnostic allocates about198,350 B/op; qualified198,450 B/op stays unchanged
because the diagnostic and benchmark wrappers differ. Physical/native peaks are
unmeasured. Plan reset/ownership/dispatch overhead could offset preparation gains.

Frozen input/output SHA256, source immutability, independent output and borrowed
root cleanup pass, as do both idle-v2 guards. An initial input gate caught two
omitted zero-alpha samples before any resize or phase measurement; the frozen
recipe was corrected in the ignored diagnostic and rebuilt once. Both versions
and failed preflight logs remain. No completed measurement was repeated.

[Evidence](cached-bicubic-preparation-attribution-evidence.json) retains exact
source pins, diagnostic inputs/binaries, phase boundaries and reproduction.
Existing [LWP776C](LANCZOS-PAIRED-WORD-ROWS.md) correctness/package qualification
and separately retained literal-run tests are reused. Displayed timings and
renderer dependencies stay unchanged; no new shape/depth/settings grid is added.
Raw `cached-bicubic-preparation-attribution.tar.xz`, SHA256
`01c07ea8f6f4a437464e57ad036cd8e0351d6dbebfcbe3cb8653d19e03049fb9`,
1,174,732 B/284 members, is mirrored and member-verified. Publication remains postponed.
