# Current live-cancelable VP8L decoding

The existing live-token lossless decode control has a **demonstrated gap** on
the current actual package DLL. Its first adjacent capture resolves the earlier
uncertainty, without a production change or CPU-gain claim.

| Complete 513×259 RGBA8 decode | Lokad / FFmpeg µs | Ratio interval | Allocated B/op | Input / output B |
| --- | ---: | ---: | ---: | ---: |
| Frozen native VP8L, live token | 3,206.92 / 3,025.66 | 1.048–1.070 | 152,728 | 183,812 / 531,468 |

Twelve alternating pairs of four complete operations and twelve native batches
before/after remain. Confidence includes paired log ratios and every actual/
native bracket. Native bracket means are 3,028.58/3,029.12/3,025.66 µs.
The quiet VM uses CPU0/one thread, optimized JIT, natural nonconcurrent workstation
GC and idle-v2 guards. The old global-bracket observation remains in
[palette-output history](WEBP-PALETTE-OUTPUT.md); do not infer a causal change from it.

Managed timing includes complete public Decode and independent output acquisition/
disposal. Native retains codec/conversion contexts and packet/frame wrappers;
state reset, complete ARGB→RGBA conversion and independent output allocation/
release are timed. Padded rows need no extra packing copy. Source delivery is the
same borrowed resident frozen packet. Managed uses a live token that is never
signaled; native has no matching cancellation setting. Token/source creation and
disposal, preparation, startup, I/O, IPC, hashes and validation remain outside clocks.

All original encoded/raster identities, native settings/build, exact RGBA/alpha/
hidden RGB, normal/scalar CLI pixels, already-canceled rejection, source preservation
and independent-owner gates pass. No reference or pixel/cancellation adjustment.
Total allocation remains 152,728 B/op; operation scratch retention is zero.
Result payload is 531,468 B until disposal. Default pool capacity/retention and
governed/physical/native/JIT peaks are unmeasured; no allocation saving is claimed.

Only the existing driver's harness allowlist adds `WarmWebpDecode`; its measurement
and statistical bodies are byte-identical. One tooling build, zero production,
embedded/public test, harness, native, fixture or package builds. The unchanged
[6,449 full/live checks per OS and SAME actual package/all three examples](PNG-STORED-WORKSPACE.md)
are reused, with no broad qualification repeat. The decoder source matches its
prior owner; the measurement uses the actual current immutable package DLL.

Six lossless read/validation controls now have three passes, one gap and two
uncertain results. Stop passing controls; [current attribution](WEBP-LOSSLESS-LIVE-CURRENT-ATTRIBUTION.md) investigates this
approximately 181 µs gap before a substantial decoder change. Do not assume the
two other uncertainties are gaps or add a settings/profile grid.

[Evidence](webp-lossless-live-current-confidence-evidence.json) records identities,
full samples, actual boundary and qualification. Raw
`webp-lossless-live-current-confidence.tar.xz`, SHA256
`cabe120fd49fdcba00ef4d4c6bf901b5aa4ebea60ebcb6fb7d82729453e47942`
(466,560 B / 27 members), is mirrored/member-verified. Dependencies preserve the
original reference, inputs, driver and current package. All 289 paths remain;
publication is postponed.
