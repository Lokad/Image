# Current lossless WebP depth and async boundaries

The two largest displayed apparent lossless gaps were historical after accepted
workspace and scoring changes. Current immutable WSP152 passes both required
boundaries. This refresh resolves missing confirmation; it is not a new CPU gain.

| Complete encode / libwebp exact=1 | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA16 normalization → VP8L8 | 119,050.51 | 131,006.84 | 0.909 / 0.921 | 7,476,640 | 183,812 |
| RGBA8 → fresh async nonseekable output | 124,488.45 | 130,150.20 | 0.956 / 0.967 | 7,477,024 | 183,812 |

Each has one twelve-actual confirmation, twelve native samples before/after,
CPU0 and passing versioned quiet guards. Frozen inputs/settings, complete packet
equality, normal/scalar native decoding, exact normalized RGBA/hidden RGB and
independent output ownership remain required. RGBA16 includes actual 16→8
normalization on both sides. Async includes fresh nonseekable output acquisition,
growth, complete delivery and disposal. Neither workload adds redundant ToArray.

The async job initially stopped before timing because its prepared correctness
proof identified an older library. The existing managed probe, native worker and
normal/scalar CLI checks regenerated that proof on the current immutable DLL.
The completed RGBA16 measurement was reused; no scored job was repeated. No
production, harness, worker, selector or package rebuild was needed.

Full Windows/Linux correctness, affected scalar and same actual Release-package
consumer/three-example proof from [WSP152](WEBP-SCORING-PREPARATION.md) remain
valid. The package is unchanged. Total allocations are measured; physical/native
heap peaks are unmeasured. Stop tuning these passing controls. Other required
paths and full warm parity remain open; publication is postponed.

[Evidence](current-lossless-webp-boundaries-evidence.json) pins current producers,
all actuals, confidence, gates and qualification reuse. Prior depth and async
reports remain historical, rather than current gap evidence. Raw
`webp-current-lossless-boundaries.tar.xz`: 971,892 B / 42 members, SHA256
`f6e0253b14cafb1ccf07a5ea1788bb18b273251c0892897749e0ea82e7b0c7c4`,
mirrored/member-verified. The retained WSP152 archive supplies unchanged baseline
qualification and dependencies.
