# Immutable JPEG scan-order data and block bounds

JZB603, unselected. The prototype replaces the generated mutable Int32 zigzag
array with the same 64 entries in immutable compiler byte data. A complete block
view and explicit 0..63 domains make repeated lookup/store bounds available to
the JIT. Pair order, coefficient-63/EOB guards, cursor publication, cancellation,
inverse transforms and numerical contracts remain.

The complete RGB444 three-actual screen costs **1,530.74/native 1,448.43µs**,
versus the source-valid original **1,530.34/native 1,450.52µs**. Allocation stays
**42,936B/op**. Wide bounds are inconclusive and there is no useful point lead;
immutable data and fewer checks do not establish elapsed gains. No confirmation,
further variant or remaining affected-cost refresh follows.

All 64 values match the original alternating-diagonal geometry. Independent
position enumeration verifies 12,032 accepted pair positions and 888 single-field
positions: masks are identities after existing grammar guards. Actual production
callers admit full 64-sample blocks. No new workspace, owner, per-operation
allocation or retained state is added; removing startup data is outside warm timing.
Physical/native peaks are unmeasured.

Windows/Linux each pass **407 internal/375 public checks per normal/scalar mode**,
including actual pair/prefix/partial-cursor models, inverse oracles and frozen
pixels/packets. Frozen input/settings, exact independent 531,468B output,
native/source/alpha/ownership checks and idle-v2 single-CPU guards remain.
Complete decoding/preparation/output/disposal and declared native context
retention remain; this is not fresh-context equivalence.

Sources, tests, main/embedded modules, bindings and manifest are restored byte for
byte. Reuse existing 5,909 full/live checks per OS and SAME actual package/three
examples without rebuild or repeated qualification. Current numeric summaries and
published packages are unchanged. This result does not ban materially revised
format-data or execution designs.

[Evidence](jpeg-immutable-map-experiment-evidence.json) retains identities, proof,
all actuals/guards and restoration. Mirrored/member-verified raw
`jpeg-immutable-map-experiment.tar.xz`, SHA256
`e5b8449539961add8c2c1e41ce4efe61a7d8fff8c56764a40f5ba472c97a26ac`,
11,016,312B/173 members, retains reproduction and current attribution/provider
dependencies. No rejected experiment becomes a renderer dependency.
