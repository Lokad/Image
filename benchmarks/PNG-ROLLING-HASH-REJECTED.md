# Rejected PNG rolling dictionary hash

The existing Fastest RGBA8 control screens6290.82µs baseline versus6246.02µs
candidate:only **0.7% less elapsed**, with overlapping confidence bounds. This
misses the predeclared5% screen gate. Reject and remove it without confirmation,
retuning, adjacent-case scoring or another variant.

Both quiet single-CPU jobs retain three actuals and both native brackets. Ratios
are1.445/upper1.519 baseline and1.423/upper1.926 candidate; native~4.35–4.39ms.
Allocation1344453→1344469B/op adds16bytes. Complete packets remain exact236828B,
with exact RGBA/alpha/hidden RGB, input and independent output ownership. These
are bounded short observations, not independent gain/parity confirmations.

An untimed replay finds276572of310782insertions consecutive (88.99%) and zero
mismatches for a rolling15bit hash. The candidate replaces two hash-byte reads
on those calls with previous-position/key state, preserving jump fallback,
ring indexing, dictionary chains and strict match order. Counts alone do not
establish a substantial complete-operation gain.

Both systems pass18focused checks normal/scalar, plus176complete-byte/flush and
32failure/cancellation comparisons per mode against the accepted own source.
Windows also passes16public reference-packet checks normal/scalar. No new
production API,buffer,scope,admission or cancellation boundary is introduced.

The accepted12dd485source and matching DLL/PDB/XML are restored on both systems;
linked internal-test assemblies are rebuilt once from restored source. The actual
private0.2.1 package is unchanged. Existing full5052checks and package/consumer
proofs remain applicable; neither is repeated for this rejected change.
[Evidence](png-rolling-hash-rejected-evidence.json) retains observations, counts,
proofs and the mirrored/member-verified88-member raw master. The prior independent
Fastest cost remains valid in the current benchmark summary. Stop hash variants;
resolve the already named uncertain reader comparisons next.
