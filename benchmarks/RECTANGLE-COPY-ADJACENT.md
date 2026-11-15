# Adjacent complete rectangle-copy comparison

The existing RGBA16 rectangle copy remains **uncertain**:
**3.776210 / FFmpeg 3.768393 µs**, ratio
**1.002074**, conservative interval **0.993778–1.006153**,
**0 managed B/op**. No production change or speedup is claimed.

One prespecified capture uses twelve alternating adjacent pairs of **65,536
complete calls per subject** (about 248 ms), plus twelve equal native batches
before/after. This replaces separate managed 200 ms/native 50 ms blocks for this
case to reduce drift. Every actual, adverse observation and the original
paired/all-actual/fastest-native-bracket confidence rule remains. The earlier
[3.783/3.780 µs uncertainty](DRAWING-ROW-PREPARATION.md) stays in history.
No identical favorable retry follows; uncertainty does not establish a need
for optimization.

Both sides borrow the original 513×259 RGBA16 source and separate canvas, copying
the 256×129 rectangle from (127,65) to (17,19). Complete public managed validation,
clipping, cancellation and mutation remain timed. The reference is FFmpeg's public
`av_image_copy_plane` with prevalidated resident arguments. Neither side resets or
allocates output per call; identical overwrites are idempotent. Original full
canvas/source, hidden RGB, alpha, untouched-pixel and ownership checks pass.
Setup, hashing and IPC stay outside both clocks. CPU0, natural nonconcurrent
workstation GC, optimized JIT and idle-v2 guards remain.

The immutable timed DLL is the current actual qualified package DLL. Reuse
**6,721 full/live checks per OS**, normal/scalar evidence and the same package's
consumer/all three examples. One small adjacent-driver build only; its measurement
and statistical bodies are unchanged. No production, embedded, harness, native,
fixture or package build, broader campaign or new workload grid.

Borrowed source/canvas capacity is **2,125,872 B**; no new admission, owned storage
or retained operation workspace. Native allocations and physical/JIT peaks are
unmeasured. Unchanged byte copying remains a gap; passing controls stop.

[Evidence](rectangle-copy-adjacent-evidence.json) retains identities, all samples,
current costs and qualification reuse. Raw `rectangle-copy-adjacent.tar.xz`
(67,628 B/51 members), SHA256
`df5c5abb3b57248b7c74cd5779f188be099e8562abc5f67ac279c619239576e4`, is mirrored/member-verified. Dependencies retain the original
harness/worker and actual package. All 289 paths remain required; publication is
postponed.
