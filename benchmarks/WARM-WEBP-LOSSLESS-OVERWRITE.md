# Complete-overwrite lossless WebP results

Resident lossless WebP decoding uses **5.55% less filtered / 5.49% less all
elapsed time** in independent confirmation, with **531,802 fewer allocated
B/op**. Raw 99.9% intervals and conservative normalized intervals both separate.
The existing stream companion also has current independent costs, without a
paired gain claim. Both remain gaps to warm single-CPU parity.

| Complete existing operation | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Resident input | 3,843.472 | 1.272 / 1.285 | 684,377 |
| Fresh seekable stream | 3,859.762 | 1.274 / 1.287 | 684,422 |

Both subjects retain the frozen 183,812-byte, 513×259 exact=1 reference packet.
Every RGBA sample, alpha and hidden RGB matches the original and native result.
Complete operations include normalization, independent output ownership and
release; stream acquisition/copies/EOF and native boundaries remain equivalent.
Native allocations are unmeasured. No performance input/settings grid is added.

One allocation call adopts the existing complete-overwrite result policy. Every
entropy/transform method, pixel store, cancellation checkpoint and cleanup path
remains unchanged. All bytes are overwritten before publication, and failure
disposes the private result. Ungoverned warm calls may use the established bounded
pool; governed calls use fresh storage and never borrow or populate that pool.
No new owner, cache, admission model, threshold or fallback is introduced.

The separate existing governed control remains **1,217,192 allocated B/op** and
**1,063,480 B shared peak**, unchanged on both OSes and in normal/scalar modes.
There is no shared-peak savings or startup-cost claim. Two new public cases prove
complete overwrite after dirty disposal, independent survivors, retained charges,
prompt cleanup and borrowed caller-root/input ownership.

Both OSes pass **5,199 checks each** (2,097 internal, 2,154 public, 948 live), plus
49 internal/272 public focused checks in normal/scalar modes. The same actual
Release package, governed async consumer, compressed-alpha two-frame consumer
and three usage examples pass both platforms. Existing exact encoder packets
and decoding/editing coverage remain intact.

One immutable core per OS and cached workers/harnesses/selectors are used. The
fresh three-actual pair passes; one fresh original twelve resolves a shift in
both raw managed/native rates before one candidate twelve. No successful timing
is repeated. One stream twelve supplies current costs. No profiling, variants
or pool-capacity tuning. Unchanged paths reuse applicable qualification.

[Evidence](warm-webp-lossless-overwrite-evidence.json) pins producers, decisions
and the mirrored/member-verified webp-lossless-overwrite-qualified.tar.xz.
Other affected ordinary VP8L boundaries need applicable current costs before
CPU-pass claims; compressed-alpha and other unchanged paths retain proof.
The full 268-path parity objective remains open.
