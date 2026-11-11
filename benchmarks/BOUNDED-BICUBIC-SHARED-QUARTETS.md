# Shared bounded bicubic vertical quartets

BQF793 shares the existing ordered four-pixel accumulator between enlargement
and bounded narrow byte/word resizing. Four pixels share one vertical traversal; the second pair waits in a vector
while the existing publisher writes the first. The publication/repair loop and
original pair/scalar tails remain.
Horizontal filtering, geometry, normalizer order and all ownership remain unchanged.

| Complete existing 4 KiB resize | Before µs | Current µs | Saved µs | Raw / normalized reduction |
| --- | ---: | ---: | ---: | ---: |
| TightDown8 | 505.98 | 459.65 | 46.34 | 9.16% / 9.04% |
| TightDown16 | 580.98 | 506.31 | 74.66 | 12.85% / 5.71% |

Both filtered/all-actual raw and conservative normalized confidence intervals
separate. Three-actual paired screens precede independent twelve-actual confirmations
on unchanged immutable builds. Native point movement is −0.129% for byte and
−7.573% for word: the word raw reduction includes clock movement; normalized
reduction is 5.71%. No unchanged capture is repeated for a favorable bound.

| Existing complete boundary | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| TightDown8 | 459.65 | 451.35 | 1.018 / 1.027 | 113 | gap |
| TightDown16 | 506.31 | 574.04 | 0.882 / 0.888 | 113 | pass |

The byte case still has a small measured gap; word passes and stops. These are
existing 513×259→256×129 controls, with complete public preparation, normalization,
exact filtering/repair, independent output and disposal. **zimg 3.0.6, FFmpeg's
zscale backend**, retains graph/input/scratch as declared; halo, filtering,
packing/normalization and independent outputs remain timed. This is not a fresh
native-graph comparison. Frozen source/output hashes, rational/qualified managed
pixels, approved native float bounds, normal/scalar frames and idle-v2 gates pass.
No image, shape, depth, quality or filter grid is added.

The [current attribution](NARROW-BICUBIC-CURRENT-ATTRIBUTION.md) identifies repeated
vertical traversal. Logical primary loop iterations 132,736→66,368; sample loads
and arithmetic remain. Every pixel retains its original multiply/add order,
normalizers, byte/word certificate and original repair, including tiny-alpha unwind.
A five-file reverse proof reconstructs all production sources. The moved helper
and both enlargement callers have identical resolved IL, locals, max stack and
inline flags after canonicalizing only the helper owner. That scoped check supports
reusing unchanged enlargement costs; it is not JIT identity or a new timing claim.
Other math bodies, cached/wide/packed/Lanczos/codec/text paths are unchanged.

No heap buffer, owner, pool, cache, API or retained scratch is added. Working
admission stays 4 KiB; explicit array payloads remain 3,424/3,232/4,080 B. The pending
vector adds a 64 B value lifetime; physical/JIT-stack peaks are unmeasured. Total
warm allocation stays 113 B/op. Independently owned result payloads remain 132,096 B
and 264,192 B until disposal. Existing default pixel-pool retention and governed
bypass remain separate. Cache/weight loans remain disjoint from top publication;
the frame retires before tail metadata restoration and final output writes.

Windows/Linux each pass **6,248 full/live checks** (2,939 internal, 2,361 public and 948 live).
Affected 363 internal/776 public resize checks pass normal, scalar, disabled-AVX2
and disabled-AVX512, including all three named storage suites. The actual shared
helper matches 40,960 ordered lanes through ten taps, with poisoned unused storage.
Existing row-loan/tail, odd stride, partial 32-column strips, low alpha, refusal,
cancellation/recovery, no-allocation and surviving-output tests remain. Hardware
kernels execute where supported.

One main/embedded build per OS, no public/benchmark/native rebuild, one scoped
three-method IL diagnostic and one pack. Linux's first line-ending preflight stops
before mutation/build; normalized unchanged test source matches, and the failure
is retained. Affected performance reconciliation precedes broad qualification.
The SAME actual Release-package consumer and all three examples pass Windows/Linux.
Private 0.2.1 package `fd594e53525db2c62f2f276df768487929cb72900296d6a2fa8ad3358effea1c` (448,983 B),
DLL `94481e592c408aa914c818fb96438a39eec8aa45dadb366aa67dcc07588364e0`; Linux qualified DLL
`6fb9f3ed61bcc6c68d362967321544eb14025db0491f7a5567640c66a4b6ed5f`. Published releases stay immutable;
publication is postponed.

[Evidence](bounded-bicubic-shared-quartets-evidence.json) retains all actuals, source/
IL proofs, modules, capacity, qualification and reproduction. Mirrored/member-
verified raw `bounded-bicubic-shared-quartets.tar.xz`: 9,459,608 B / 682 members,
SHA256 `f6d85f6a6b402094b61341fcc550e7ed77be4daffd10ff5564ce66fbf6cb5340`. CNP789 and NCA792 dependencies
retain predecessor/reference inputs and attribution. Historical/adverse screens
remain separate. All 289 required paths remain; this is no whole-release refresh.
