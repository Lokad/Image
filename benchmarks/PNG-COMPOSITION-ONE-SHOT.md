# Complete one-shot RGBA16 PNG composition

[Current retained-native precision](PNG-WORD-COMPOSITION-PRECISION.md) remains
uncertain on the latest actual package. The scoped milestone observations below
are retained; the independent equivalent one-shot pass stays valid.

The existing frozen 513×259 RGBA16 seekable-PNG control now has an equivalent
fresh-state comparison: Decode → AutoOrient(Normal) → centered 256×129 crop →
nearest fit inside 128×64, producing an independently owned 127×64 result.
The managed method and every intermediate owner remain unchanged.

| Complete one-shot operation | Managed / FFmpeg µs | Ratio / upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | --- |
| PNG16 decode/orient/crop/fit | 2,856.938 / 3,301.306 | 0.865 / 0.871 | 338,155.875–338,159.750 | pass/stop |

Native PNG codec/options, filter graph and packet/decoded/result wrappers are fresh
per operation. Timed work includes configuration, fresh AVIO/8 KiB scratch,
length query, acquired encoded packet/EOF, full decoding, crop/point filtering,
full-word conversion, independent result and disposal. Graph/decoder preparation
is released before output publication; surviving results retain their own storage.
Both sides borrow resident encoded input; startup/JIT, file I/O, IPC, hashes and
verification are outside clocks. Native may retain intermediate views within an
operation; all managed public intermediates remain independently materialized.
The frozen harness's generic retained-context wording is superseded by the
explicit fresh-state source/ready proof; every original gate remains.

The original graph configuration, pixel validation and Apply input/decode/filter
body reconstruct exactly except preparation/release calls. The original worker
and sources are untouched. Two-live-output mutation/independence, unchanged input,
all 16-bit samples, normal/scalar FFmpeg CLI pixels, caller stream lifetime and EOF
pass before and after timing. The complete input packet is frozen at 960,565 B,
SHA256 `3e4b64d778c395392451d5331d2a11b541d262e2f39826e1322d22d6f03bb4a8`.

The short 12-pair/8-call screen is retained: 2,867.211/3,924.820 µs, upper 0.763.
Independent confirmation uses 12 alternating 64-call pairs and 12 native batches
before/after, natural nonconcurrent GC, one CPU0 operation and idle-v2 guards.
Every actual and the existing conservative paired/all-actual/fastest-bracket
confidence rule remain. Native costs differ between those batch sizes; neither
cross-run subtraction nor a causal CPU improvement is claimed.

One controlled native-only 64-call comparison directly tests the state lifetime:
retained decoder/graph **2,942.746 µs** versus fresh
**3,292.073 µs**, extra **349.328 µs**;
fresh/retained interval **1.113372–1.124057** over 12 alternating
pairs. It includes preparation/disposal and internal buffer/cache reuse differences,
not a pure setup-phase duration or a managed speedup. The original
[asymmetric pipeline comparison](WIDE-PNG-SCAN-CRC.md) remains uncertain at
2,871.463/2,943.036 µs, interval 0.928688–1.021958. This fresh view does not relabel it.

GC allocation, retained storage and peak capacity stay separate. Unchanged
source-derived simultaneous image payload is 2,455,088 B; default idle payload can
retain 2,125,872 B within two slots/4 MiB. Caller-governed storage bypasses that pool.
This measurement does not newly measure admitted peak, native allocations/allocator
retention or physical/JIT peaks. No production capacity or retention is added.

Reuse the immutable actual package `e58932e09d4a7c82d0a4bd4e5e4b0a56a6c1621b12f7d0379b5b1b3461d8e416` and DLL
`843fa82f57827a4a5c4f0fe38061db9da92723207d9e0733a3f4d6c70613e045`. Each OS's 6,742 full/live checks, affected PNG normal/scalar/
AVX512-off evidence and same-package consumer/all three examples remain valid.
Zero production, managed harness/driver, fixture or package builds; one isolated
original native adapter build uses the same pinned FFmpeg/static archives.
Untimed projection/source-guard corrections and the local controller syntax
rejection are retained; no completed capture is restarted or broad suite repeated.

[Evidence](png-composition-one-shot-evidence.json) pins scope, confidence, boundaries,
all exact/owner gates, source proof and qualification reuse. Raw `png-composition-one-shot.tar.xz`
(10,759,100 B/191 members), SHA256
`08cf04d9c811bb00398dc738e29ae2cb974e503e2a8ac0fdcede5bc2f45532b8`, is mirrored and every member verified.
It retains original/modified sources, both workers, unchanged managed runtime,
all captures and corrections, frozen input/oracle and native lifetime attribution.
Dependencies retain the actual package and prior qualification. Reproduce with
archived `tmp/pcf1117-prepare.py`, then `tmp/pcf1117-run.py short`/`confirm` and
`tmp/pcf1117-native-attribution.py` in a fresh ignored projection with pinned
producer/reference inputs. Existing completed capture names are never overwritten.
This adds one view of an existing required public boundary, no material path or
shape/depth/settings grid. The full 289-path objective stays open; publication
remains postponed.
