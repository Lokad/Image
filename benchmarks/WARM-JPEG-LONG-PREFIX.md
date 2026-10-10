# Warm JPEG adaptive long-prefix lookup

One production primitive adds an input-derived lookup for nine/ten-bit
Huffman codes after the existing eight-bit lookup. Other lengths retain
the scalar reader. Tables without those codes keep their original lookup
payload and construction work. The selected native-input gap improves
but remains above 2x; this does not establish uniform parity.

The [original numeric evidence](warm-jpeg-long-prefix-evidence.json) retains
managed actual samples, statistics and allocations, native brackets, source
and binary identities, exact input/output hashes, syntax, controls and
quietness. Regenerate with `python benchmarks/render-warm-jpeg-long-prefix.py`.

| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native / baseline | 1 | 7882.95 | 1717.65 / 1732.39 | 4.59x | 5.70x |
| Native / baseline | 2 | 8779.15 | 1726.26 / 1717.73 | 5.11x | 5.98x |
| Native / additional lookup | 1 | 6979.76 | 1719.74 / 1725.72 | 4.06x | 4.52x |
| Native / additional lookup | 2 | 7043.52 | 1722.65 / 1720.48 | 4.09x | 4.82x |
| Owned / affected screen | 1 | 7326.89 | 1903.70 / 1906.35 | 3.85x | 4.43x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled
and non-concurrent workstation GC. Native-input managed mean spread is
0.9%; the earlier baseline spread remains recorded.
The owned input has one affected screen, not a new confirmation. These
short screens guide priority and retain uncertainty.

Both frozen JPEGs come from one existing original 513x259 RGBA8 raster,
flattened once on RGB 0x204060. No additional source raster, upstream corpus,
preparation or quality enumeration is used. Each decoder pair receives
identical resident bytes. Managed pixels remain exactly frozen; native pixels
match the pinned public CLI with normal SIMD. RGB maximum 4/RMSE 1.5 and
exact opaque alpha remain required.

Both intervals include full decode/conversion, independent owned output
allocation and release. Native context/conversion/input packet/frame wrappers
are reused, with state reset and every pixel decoded/converted each call.
Padded native rows need no tight-row copy. Managed public Decode and disposal
are timed. Startup, IPC, I/O, generation, hashing, checks and file packing
are excluded.

The additional lookup is generated from each encoded canonical table under
T.81 C.2. It admits 2,048 payload bytes and 1,024 construction-work units
before allocation. The native input has two such AC tables, adding 4,096
admitted bytes and 2,048 construction units. The fixed-code owned input has
none. Observed x64 allocation increases by 4,176 bytes: the 4,096-byte
payload, 48 bytes of array headers and 32 bytes of additional table fields.
The owned input adds only 16 bytes for its two table fields. Every symbol
retains its original code-bit work. Non-mutating lookahead
commits only through the last byte required by a complete code; cached
padding, markers, truncation and tight budgets keep scalar failure precedence.

All 1,861 tests pass on Windows and Linux: 281 internal, 686 public and
894 live FFmpeg comparisons. On the VM, 61 internal and 70 public JPEG
controls pass separately with normal SIMD, AVX-512 disabled, AVX disabled
and all intrinsics disabled. The 30 existing original JPEG fixtures retain
exact owned pixels against the previous library. New controls cover nine/
ten-bit code padding/positions/work, additional storage admission, partial
budgets and marker failure precedence. BDN output smoke passes.

The same actual dependency-free Release package passes verification and
all three usage-guide examples on both platforms. Package SHA256:
`b34b24f8737b2e6de6abafe62c9eb06c583c6aa7cab9e5211c405325674f756a`.

Package and benchmark builds have separate DLL identities. On the VM, their
method-body fingerprints match and the same 30 existing fixtures reproduce
identical pixels through both actual binaries. These are correctness checks
without timing; both DLL hashes remain in the evidence.

Native-input managed allocation is 2,664,536 bytes per operation.
This includes the additional lookups; allocation observations remain in the
numeric evidence. Failed rolling-load samples and later passing cooldowns
are retained without relaxing the quietness gate. The baseline workload
profile attributed 53.04% to entropy/Huffman work;
its timings are excluded from the parity rows.

Reproduce sequentially on the quiet dedicated Linux VM following the
[worker instructions](../tools/WarmReference/README.md#jpeg-decode).
