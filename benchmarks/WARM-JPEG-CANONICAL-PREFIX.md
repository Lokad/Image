# Warm JPEG canonical fallback prefix

One production primitive consumes eight Huffman prefix bits together when
no short code is assigned, then continues canonical traversal. Pixels and failure boundaries
remain unchanged. Both retained gaps exceed 2x; these short screens do not
establish uniform parity.

The [original numeric evidence](warm-jpeg-canonical-prefix-evidence.json)
retains samples, statistics, allocation diagnostics, native brackets, source
and binary identities, frozen input/output hashes, controls and quietness.
Regenerate with `python benchmarks/render-warm-jpeg-canonical-prefix.py`.

| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native / baseline | 1 | 5112.00 | 1726.01 / 1716.48 | 2.98x | 4.61x |
| Native / baseline | 2 | 5134.22 | 1723.64 / 1731.19 | 2.98x | 3.54x |
| Native / canonical prefix | 1 | 5019.99 | 1734.75 / 1726.15 | 2.91x | 4.25x |
| Native / canonical prefix | 2 | 5001.46 | 1723.06 / 1716.30 | 2.91x | 3.31x |
| Owned / baseline screen | 1 | 5477.23 | 1925.25 / 1898.57 | 2.88x | 5.37x |
| Owned / affected screen | 1 | 5400.91 | 1899.30 / 1897.88 | 2.85x | 2.93x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). CPU 0 only,
one codec/conversion thread, normal SIMD, optimized JIT with tiering disabled
and non-concurrent workstation GC. Native-input managed mean spread is
0.371%. The owned input has one affected screen only.

Two frozen JPEGs come from one existing original 513x259 RGBA8 raster.
No additional image, preparation or quality enumeration is used. Each pair
receives identical resident JPEG bytes and includes complete public decode,
conversion, independent owned output allocation and disposal. Native context,
conversion and input wrappers are reused; reset and every decoded/converted
pixel remain timed. Startup, IPC, I/O, generation, hashing, correctness checks
and packing are excluded.

After existing short/long lookup misses, a complete eight-bit prefix is checked
against the existing short-code table. A zero entry proves no assigned code
of length eight or less. Admit its original eight logical work units, commit
only its required bytes and continue canonical traversal at length nine.
The canonical codes follow [T.81 C.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf).
Assigned short codes, markers, truncation and tight budgets retain the original
one-bit fallback without speculative state mutation. Padding, restart markers,
symbols/amplitudes, error/source/work positions, cancellation, decoded pixels
and independent ownership remain unchanged. No storage, table, cache,
allocation or dependency is added.

All 1,930 tests pass on Windows and Linux: 350 internal, 686 public and
894 live FFmpeg comparisons. Four VM SIMD/scalar modes each pass 130 internal
and 70 public JPEG controls. Fifteen new cases cover 48 cached phases of
11 to 16-bit canonical codes (144 exact symbols), padding and restart positions.
All 128 partial long-code work budgets and 64 short-code budget/phase pairs
retain exact byte positions. Six marker/truncation inputs and 16 cached
marker/budget phases retain failure messages, source position and work.
Thirty original fixtures retain exact pixels; all 160 frozen encoder bitstreams
remain unchanged. Output smoke and FFmpeg CLI pixel controls pass.

The same actual dependency-free Release package passes verification and all
three usage-guide examples on both platforms. Package SHA256:
`5746329831d92e68a5e8e8d40de9f62dadd39f3a03b072e21313a19da313e695`.
Package and measured DLL identities differ; their method-body fingerprints
and all 30 original fixture pixels match on the VM.

Inclusive BDN allocation changes are [0, 0] bytes/op for the native-input repeats
and 0 bytes/op for the owned screen. Raw diagnostics remain visible.
Separate strict controls perform 16 complete public Stream decodes per frozen
input through baseline and measured DLLs. Candidate totals do not increase;
owned pixel hashes match. Fresh caller streams and disposal are included.
Native-input totals: 53,135,488 / 53,135,488 bytes (baseline / candidate).
Owned-input totals: 63,575,344 / 63,544,264 bytes (baseline / candidate).
These controls make no timing claim; warm parity uses resident span inputs.

Failed quietness samples and later passing cooldowns are retained. A separate
current baseline profile ranks Huffman.Read at 23.04% leaf, inverse DCT at
20.37% and ReadBits at 13.01%. The coefficient/entropy group spans several
methods (52.73% inclusive). Method matching excludes parameter types.
These are baseline CPU samples, not candidate percentages or parity timings;
profile workload includes warmup/calibration.

Reproduce sequentially on the quiet dedicated VM using the
[worker instructions](../tools/WarmReference/README.md#jpeg-decode).
