# Empty copy-tail trial

**Not retained.** Skipping zero-length tail spans/Memmove improves complete byte
copying, but a controlled word-flip comparison is adverse. All captures remain.

| Existing complete managed-DLL comparison | Baseline / candidate µs | Ratio interval | GC B/op |
| --- | ---: | ---: | ---: |
| Separate byte copy, independent confirmation | 1.986021 / 1.970827 | 0.991359–0.993341 | 0 / 0 |
| Word vertical flip, targeted control | 17.447331 / 17.528318 | 1.000884–1.008409 | 56 / 56 |

The first saves **15.19 ns /0.765%**; the second is adverse by **80.99 ns /0.464%**.
These are scoped effects on distinct workloads, not FFmpeg costs, a universal
regression, summed gains or a workload-frequency assumption. All twelve alternating
8,192/4,096-call pairs, exact source/output/owner checks and idle-v2 guards remain.

Five actual-reference captures reconcile before rejection: two pass, two gaps and
one uncertainty on the rejected producer. They are not current costs. Candidate
byte copying is 1.988 µs, above its historical 1.969 µs despite the isolated gain;
that direction conflict remains unexplained. A planned baseline-reference control
is not run after the word comparison already rejects the shared candidate. No
candidate/native replay or broad qualification follows.

Only an empty-tail condition changes. Whole-block snapshots, nonempty tails,
portable fallback, callers, cancellation, admission and ownership stay exact.
Actual code adds a branch over tail slicing/Memmove, with identical vector
instructions; CopyRegion grows 1,357→1,363 B. Instrumented times are discarded.
No new owner, array, pool, cache, API or fixture-specific dispatch.

Linux passes **25 internal/190 public** normal/scalar checks. Exact qualified
source/core/embedded/public modules and source inventory are restored without
rebuilding; Windows modules and package never changed. Reuse **6,721 full/live
checks per OS** and the same actual package consumer/all three examples. One
Linux core/embedded build; zero other builds or package/broad repeats.

Allocation stays 0 B borrowed /56 B owned; no new admitted or retained storage.
Physical/native/JIT peaks are unmeasured. [Current copy](DRAWING-ROW-PREPARATION.md),
[word confidence](RECTANGLE-COPY-ADJACENT.md) and [flip](SNAPSHOT-BLOCK-COPY.md)
results remain unchanged. No rejected-result renderer dependency or family ban.

[Evidence](empty-copy-tail-trial-evidence.json) retains all data and restoration.
Raw `empty-copy-tail-trial.tar.xz` (1,847,176 B/138 members), SHA256
`76135cd402e1f45312d3e337b6879f134358cc8eb7f2f5c418f610a0307d42e1`, is mirrored/member-verified with original tools and
qualified-package dependencies. Full 289-path completion remains open;
publication is postponed.
