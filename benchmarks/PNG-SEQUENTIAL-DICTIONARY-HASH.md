# Sequential PNG dictionary hash preparation

The lazy Deflate parser reuses its three-byte dictionary hash across consecutive
insertions. The old leading byte shifts outside the 15-bit mask; inserting one new
byte produces exactly the original next hash. Discontinuities and history slides
reseed from acquired bytes. Cache state is local to one immutable input sweep;
dictionary links/order, search limits, lazy decisions/ties and packets stay exact.

This removes two repeated input-byte reads per sequential insertion without a
session, pool, new buffer or persistent cache. Greedy/stored paths are unchanged.

| Existing 513×259 Optimal6 / native6, filter None | Before µs | Selected µs | FFmpeg µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| RGBA16 | 23,406.99 | 22,845.90 | 20,758.37 | 1.101 / 1.113 | 3,420,516 | 963,202 |
| RGBA8 | 18,312.90 | 18,064.02 | 17,190.81 | 1.051 / 1.057 | 932,516 | 210,973 |

Independent word confirmation saves **561.09 µs / 2.40% raw / 2.55% normalized**.
Filtered/all-actual raw and conservative normalized intervals separate: before
normalized lower 1.117299 exceeds selected upper 1.113123. The native bracket clock
is 0.155% slower, disclosed alongside the raw gain. Byte points improve 1.36% raw/
1.02% normalized; normalized intervals overlap, so no byte incremental gain or
regression is claimed. Both controls retain CPU gaps to the full parity target.

All 12 actuals per confirmation, normal estimator exclusions, native brackets and
both versioned quiet guards are retained. Four complete sequential operations per
actual; one CPU. Both sides prepare/dispose fresh encoder/zlib/packet state and
independent output; the word reference also allocates/converts/releases its endian
frame inside timing. Frozen input/settings, full 16-bit samples, hidden RGB, source
immutability, raw packets and output ownership pass. No new workload grid or
sampling to obtain favorable results. This is not a full release refresh.

Each OS passes **5,549 full/live checks** (2,326 internal + 2,275 public + 948 live),
plus 563 internal/211 public PNG checks per normal/scalar mode. Existing generic Deflate
oracles cover input-delivery boundaries, origins 0/2^40, slides, overlapping runs,
real cancellation/writer failures and later root reuse. An independent algebraic
check validates discarded leading bytes and 327,680 recurrence cases. The SAME
actual Release package and all three usage examples pass Windows/Linux, including
retained/async outputs. Published packages remain unchanged.

One immutable core/internal build per OS serves the entire bounded experiment and
qualification. No harness/native/selector/public-test builds; one package build.
Allocation is unchanged; admitted Deflate workspace remains **384 KiB**, retained
operation scratch zero. Two scalar locals fit the existing stack allowance. New heap
arrays/reservations zero; complete admitted/physical peaks and native allocation are
not newly measured or inferred from B/op.

Only lazy 6/9 insertion changes. Other affected color/depth/async/SmallestSize costs
remain historical; Fastest1/stored0, other codecs, decode/inspection, editing and
text reuse applicable unchanged-path proof. The full 282-path objective stays OPEN.
Earlier [lazy execution state](PNG-LOCAL-EXECUTION-STATE.md),
[greedy execution state](PNG-FAST-LOCAL-EXECUTION-STATE.md) and
[one-shot boundary](PNG-WORD-ONE-SHOT-BOUNDARY.md) evidence remain linked history.

[Evidence](png-sequential-dictionary-hash-evidence.json) pins sources, both producers,
all observations/gates, proof and actual-package qualification. Mirrored/member-
verified raw `png-sequential-dictionary-hash-qualified.tar.xz`: 3,593,664 B / 164 members,
SHA256 `1af6c5b1db1cc0f911293c62438f870a4157bee97b051b70754a4f55563fe661`.
Source/core/test snapshots, actuals, guards, packages and reproduction are retained.
Qualified baseline, reference and [current attribution](PNG-CURRENT-OPTIMAL-WORD-ATTRIBUTION.md)
archives are pinned dependencies. Publication is postponed.
