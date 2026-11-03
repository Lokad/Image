# Current WebP cache-history capacity

The prior qualified immutable RSP186 DLL's admitted encoder peaks occur during cache-
history preparation on both existing controls. A caller-root callback records
growth snapshots; a separate count-only source projection observes the live
token prefixes at that boundary. Production and the actual package are unchanged.

| Complete control | Current admitted peak B | Unused standard-token tail B | History workspace payload B |
| --- | ---: | ---: | ---: |
| Lossless photograph, Q85 / effort4 | 3,470,059 | 85,352 | 238,068 |
| Transparent VP8, Q75 / effort4 | 3,847,888 | 584,024 / 927,968 across its two filters | 238,068 each |

Both alpha tails fit the temporary minimum-hit, cache, histogram and prefix-mass
storage. The photograph's standard/repeat tails (85,352 / 21,616 B) do not fit.
The current history reservation is 238,227 B per preparation. These are observed
capacity and derived layout facts, not verified allocation, peak-reduction or CPU
gains. Growth callbacks do not expose every release or prove all storage lifetimes.
Caller-owned source/output and diagnostic storage are excluded; physical peaks
are unmeasured. Encoder scratch returns to zero without disposing the caller root.

This supports a bounded operation-owned reuse design: lend only the token tail
after its live prefix, end the borrow before cache application or later token
writes, and keep both candidates independently owned. Prove disjointness and
all future users before mutation. Select by actual capacity, preserve candidate
order/ties and terminal admission refusal, and keep ordinary allocation where
capacity cannot fit. No cross-operation pool, cache or new owner abstraction.
The earlier [cache-application trial](WEBP-CACHE-HISTORY-REUSE-INCONCLUSIVE.md)
saved only 8,672 B/op and is distinct from lending this full temporary workspace.

Each diagnostic executes one warmup and one observed complete encode per control.
All original 160,032 / 98,590 B packets, source pixels, borrowed roots and quiet
guards pass. One actual-DLL audit build and one private count-projection build;
zero production, worker, selector, native or package builds. No timing or total-
allocation measurement. Reuse unchanged Windows/Linux/scalar and actual-package
qualification; all three CI jobs on 52d0bf8f are green. Current benchmark costs
remain valid and the complete performance objective remains open.

[Evidence](webp-cache-history-capacity-evidence.json) pins source/core/tool
identities, peaks and prefix counts. Raw `webp-cache-history-capacity-audit.tar.xz`:
1,155,040 B / 49 members, SHA256
`dde8c47e21aaa6b15904cb2344a4152d6ad8330270c37fd4796a45b1b8dc81ab`,
mirrored and member-verified. Full growth events and both tools are retained.

The subsequent [token-tail workspace qualification](WEBP-TOKEN-TAIL-HISTORY.md)
records the implemented borrow, its current costs and measured storage savings.
