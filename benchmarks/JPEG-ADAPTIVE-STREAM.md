# JPEG adaptive tables for stream output

C127 uses the existing admitted event/histogram plan for forward-only and
live-cancellable encoding. Checked emission retains each code/amplitude work
charge, cancellation check and output quota. Only noncancellable stock
`MemoryStream` with the conservative packet allowance uses direct admitted writes.
Planning makes no caller length/position/seek queries. Tight local limits keep
fixed coding; shared admission refusal remains terminal. Caller-budget event
storage stays owned until its last user finishes, including nested callbacks.

Five existing controls improve, without changing any reconstructed pixel:

| Complete operation | Previous B | Current B | Frozen reference B |
| --- | ---: | ---: | ---: |
| RGBA8 4:4:4, forward-only / live-cancellable | 170,978 | 158,812 | 133,041 |
| RGBA8 4:2:0, forward-only | 60,987 | 55,797 | 52,093 |
| RGBA16 4:2:0, forward-only / live-cancellable | 61,039 | 55,834 | 52,117 |

These are exact previously qualified adaptive packets, so existing matched-quality
evidence applies unchanged. No source/reference/settings, quantizer, coefficient,
reconstruction or alpha changed. Nine existing boundary replays pass normal/scalar;
other byte/quality facts are reused. All five size gates remain open. The
[19-row ledger](JPEG-COMPLETE-PACKET-AUDIT.md) still has five default passes and
thirteen open cases; optional CompactColor passes separately. There is no broader
photographic/diagram compression claim. Passing controls stay stopped.

Both Windows and Linux pass 4,890 Release tests (1,983 internal, 1,971 public,
936 live FFmpeg), plus 72 internal and 51 public scalar checks. Fifteen focused
stream cases cover exact quota/prefix failure, callback and cancellation cleanup,
terminal admission refusal, nested aggregate peaks, later success and independent
surviving output. Existing fixed-packet tests explicitly constrain local workspace.
The same actual private 0.2.0 package and all three examples pass on both platforms.
One production build/platform is reused throughout; no worker/native/selector
rebuild or independent timing confirmation is performed.

One quiet-VM three-actual screen records forward-only RGBA8 4:4:4 at
**5,777.779 µs / reference 4,066.607 µs**, ratio1.421, allocated **559,443 B/op**.
Both idle-v2 guards pass. The conservative upper is4.529 with this short sample;
this is a cost observation, not a CPU-parity claim. Native allocations are
unmeasured. Complete normalization, event planning, encoding, fresh wrapper/output
ownership and release are timed on one CPU. The unchanged compiled harness retains
its historical `fixed-geometric-v1` selector/receipt text; the recorded C127
producer emits adaptive tables. Original receipts are preserved with that label
limitation explicit. No timing repeat is needed for compression acceptance.

[Numeric evidence](jpeg-adaptive-stream-evidence.json) retains producer identities,
raw statistics, allocations, quality reuse and precise invalidation. Immutable raw
is mirrored as `c127-qualified-linux.tar.gz` (2,740,729 B; SHA256
`8e3e8204460828b51ea39a87ef020bc3ce80693e47a6573bbcbfde033c5aba91`) and
`c127-qualified-windows.tar.gz` (2,777,254 B; SHA256
`9c98d314bbff92e7b90b1457a27f3c08b715d17f9befedd69af6da46c3e25e15`).
