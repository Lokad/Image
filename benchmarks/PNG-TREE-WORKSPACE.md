# Operation-owned PNG tree workspace

One encoder now owns the seven Huffman preparation arrays reused by successive
literal/distance/code-length trees. Counts, heap, active parents and internal
depths overwrite their used ranges; only leaf depths, lengths and bit counts
are cleared, with an explicit zero root. Returned trees remain independently
owned. Candidate selection, tie-breaking, dummy symbols, overflow repair,
packet bytes, cancellation and source/output ownership are unchanged.

At identical batching, complete Smallest9 encodes allocate **834,544 B less
(24.4%) for RGBA16** and **94,960 B less (10.2%) for RGBA8**. Independent CPU
intervals overlap: this is an allocation milestone, without a speedup,
equivalence or causal regression claim. Byte raw elapsed rises8.77% while its
native clock rises8.46%; word raw changes−0.16% with native+0.85%. All actuals,
filtered/unfiltered estimators and native brackets remain in the evidence.

| Existing complete513×259 one-shot control | Lokad.Image µs | FFmpeg µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 Smallest9 | 22,770.12 | 21,242.05 | 1.072 / 1.104 | 2,585,904 | 963,202 |
| RGBA8 Smallest9 | 19,603.10 | 18,690.15 | 1.049 / 1.061 | 837,556 | 210,973 |
| RGBA16 Optimal6 | 21,775.28 | 20,741.61 | 1.050 / 1.064 | 2,585,972 | 963,202 |
| RGBA8 Optimal6 | 17,820.47 | 17,246.94 | 1.033 / 1.055 | 837,556 | 210,973 |
| RGBA16 Fastest1 | 22,154.97 | 20,225.47 | 1.095 / 1.124 | 2,585,972 | 968,216 |
| RGBA8 Fastest1 | 5,956.95 | 4,745.88 | 1.255 / 1.282 | 840,844 | 236,828 |

The four existing level1/6 controls refresh costs on the same immutable selected
build, without expanding the workload grid. Their native clock movements and
raw costs are recorded separately; they establish no incremental CPU claim.
Complete fresh codec/zlib/packet preparation, encoding and independent output
disposal stay timed on both sides. Native word endian-frame allocation,
conversion and disposal are also timed. Five have confirmed parity gaps;
byte Optimal6 is uncertain (lower0.984, upper1.055), to resolve before tuning.

The workspace retains **12,648 B array payload during the operation**, zero
scratch after disposal. It is admitted before allocation within the existing
**384 KiB** encoder reservation: fixed arrays266,236 + conservative tree/code/
metadata/stack32,768 + fixed metadata4,096 =303,100 B, below393,216 B. This is
a capacity bound, not a newly measured complete physical/admitted peak. No new
allowance, shared root, pool, session, finalizer or fallback is introduced.

Before mutation, ten original frequency recipes freeze actual qualified outputs
on both OSes: ties, sparse/dummy alphabets and length overflow. Three new tests
check forward/reverse reuse, independently surviving trees, immutable counts,
poisoned scratch including stale roots, and cancellation followed by reuse.
All-clear stage251 and selective stage252 shorts remain, including their adverse
and noisy observations. An initial test setup canceled before constructing its
context; only internal tests were rebuilt to correct the order. Stage252 then
passes fresh **576 internal/211 public PNG checks per normal/scalar mode and OS**.

Selected production passes **5,569 full/live checks per OS**:2,346 internal,
2,275 public,948 differential. The SAME actual Release0.2.1 package and all three
examples pass Windows/Linux. Published packages are unchanged. Only dynamic PNG
tree paths invalidate earlier costs; six are current, other affected H costs
remain historical. Unchanged codecs/editing/text reuse their valid proof.
The full282-path objective stays open; publication is postponed.

[Evidence](png-tree-workspace-evidence.json) pins both stages, frozen producers,
source identities, current costs, ownership bounds, qualification and adverse
results. Mirrored/member-verified raw `png-tree-workspace-qualified.tar.xz`: 5,679,252 B / 346 members,
SHA256 `ac86ee3d5fd76da7608e47f5d256812aeb748f20e5938e0557bba72900b27321`. Sources, producer/test snapshots, actual packages,
guards, actuals and reproduction are retained. Pinned dependencies preserve the
storage audit, [prior token output](PNG-TOKEN-OUTPUT.md) gain and equivalent
[fresh-operation reference](PNG-SMALLEST-ONE-SHOT-CURRENT.md).
