# Warm JPEG signed amplitude extension

Signed amplitude extension uses an exact sign mask instead of a
payload-dependent branch. Complete warm decode remains above the 2x target.

The [numeric evidence](warm-jpeg-signed-evidence.json) retains actual samples,
uncertainty, native brackets, allocations, source/input/output hashes and
terminal receipts. Regenerate with `python benchmarks/render-warm-jpeg-signed.py`.

| Input / stage | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: | ---: |
| Native / branching extension | 1 | 4438.51 | 1718.89 / 1733.11 | 2.58x | 4.40x |
| Native / branching extension | 2 | 4415.58 | 1723.10 / 1720.61 | 2.57x | 3.01x |
| Native / sign mask | 1 | 3657.04 | 1718.52 / 1719.48 | 2.13x | 2.25x |
| Native / sign mask | 2 | 3694.14 | 1717.73 / 1726.43 | 2.15x | 2.54x |
| Owned / branching extension | 1 | 4805.26 | 1907.07 / 1899.05 | 2.53x | 2.96x |
| Owned / affected screen | 1 | 3826.16 | 1899.11 / 1910.50 | 2.01x | 2.58x |

T.81 F.2.2.1 subtracts `2^count - 1` when the amplitude has no leading one.
After the unchanged bit read, `threshold = 1 << (count - 1)` lies in
[1,32768] and `value` lies in [0,65535]. The difference fits Int32; its
arithmetic sign mask is zero for a positive amplitude and minus one for a
negative amplitude. Applying `1 - 2*threshold` yields exactly the original
signed value without overflow. Count zero keeps its early return. The JIT
inlines this arithmetic into baseline coefficient decoding and emits sign
shifts at the former conditional extension sites. Other guards remain.

Huffman traversal, byte stuffing, padding/restarts, categories, predictor and
run bounds, input positions, partial writes, cancellation and logical work
are unchanged. No new table, buffer, image cache, dependency or unsafe code.

The exhaustive original primitive test enumerates all 131,070 signed payloads
at widths 1 through 16. It constructs T.81 payloads from signed magnitudes,
including stuffed bytes, one padding and RST0, and checks decoded values,
positions and exact work. Existing category endpoints, zero/invalid widths,
partial budget failures, cancellation and malformed coefficient controls
remain. These payloads are correctness controls, not benchmark images.

Both platforms pass all 2,009 tests: 429 internal, 686 public and 894 live
FFmpeg comparisons. All thirty original JPEG fixture outputs match the prior
accepted assembly. The same dependency-free Release package passes verification
and all three usage examples on both platforms; its method bodies and fixture
pixels match the actual measured DLL. Package SHA256:
`800ae9abb157936ebb3dcf19c0cd5a921c864dce67625152d51900e866e60959`.

The same two frozen JPEGs use one existing original 513x259 RGBA8 raster.
No new image, preparation or quality enumeration is used. Complete public
decode, conversion, independently owned output acquisition and disposal are
timed on resident compressed bytes. Native reset and pixel work stay timed.
Startup, IPC, I/O, hashing and validation are excluded. Runs are sequential
on CPU 0 of the dedicated AMD EPYC 9V45 VM: one codec/conversion thread,
normal SIMD, .NET 10.0.8 / SDK 10.0.204, tiering disabled and
non-concurrent workstation GC.

Inclusive allocation changes: [-8, -8] bytes/op native,
17 bytes/op owned. Managed mean spread:
1.014%.

An untimed allocation diagnostic compares complete public Stream decode with
a fresh caller stream and owned output disposal. After equal warmup, a bounded
no-GC region isolates the sixteen calls in each comparison. All
sixteen per-call allocations and totals match exactly on each frozen JPEG.
The numeric evidence preserves its distinct boundary, assemblies and output
hashes. The initial diagnostic with normal GC had variable owned-input calls;
its full observations are retained alongside the controlled comparison.
The small benchmark allocation estimates remain visible; this primitive
introduces no allocation or change to decode construction.

The retained baseline CPU profile attributes 50.72% inclusive to coefficient
and entropy methods, with 28.51% at the baseline coefficient leaf. It precedes
this change and supplies no signed-extension-only or candidate percentage.
Short screens guide ranking; the owned input has one affected screen. Native
and managed uncertainty, failed post-run quietness samples and later passing
cooldowns remain visible. Uniform parity, other gaps and unknown or unmatched
directions remain open.
