# Current tight PNG decode costs

Two existing resident-input controls now have independent current-code costs.
Byte decoding remains a demonstrated gap. The 16-bit case is confidence-open:
its evidence establishes neither a parity pass nor a performance gap.

| Existing 513×259 decode | Managed µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| DecodeTight8 | 1,642.611 | 1.096 / 1.119 | 68,224 | Gap |
| DecodeTight16 | 3,120.336 | 0.997 / 1.016 | 70,272 | Confidence open |

Both retain all twelve actual iterations, including the one omitted from each
eleven-result BDN selection, both native brackets and idle-v2 guards. Original
packets, full-precision RGBA/alpha/hidden RGB, source and independent output owners
pass. Local working limits stay 1,235,173 / 3,254,193 B; input hashes and exact
samples are recorded in the evidence. Native RGBA16 BE-to-LE conversion remains
inside the complete decode/output/disposal boundary. Native allocation is unmeasured.

The older policy/reconciliation costs predate shared GFNI CRC changes. That
invalidates their executed CRC timing, while preserving the frozen packets,
pixels, reference settings, limits and ownership contract. Current costs make no
causal gain/regression claim against older producers. WPC31 encoding changes
bypass these decoder paths. No image/depth/filter/settings grid is added.

**Zero builds, profiles, new inputs/settings or repeated full/package checks.**
The same verified immutable qualified core, cached selector/current harness,
worker and fixtures serve both jobs. Unchanged **5,342 checks per OS**, 948 live
comparisons and the SAME actual Release package/three examples are reused.

Keep the word case explicitly unresolved; do not infer optimization is needed
or repeat valid measurements without a new reason. Prioritize a distinct
substantial opportunity among demonstrated gaps. The full **268-path warm
conservative-upper ≤1× objective** remains open.

[Numeric evidence](current-tight-png-decode-costs-evidence.json) retains actuals,
confidence, pins, original boundaries and qualification reuse. Mirrored/member-
verified raw current-tight-png-decode-costs.tar.xz, SHA256
e5319918f7ce7bbaaafd940384bf191d76cebb5931199b01381d9e377d869824 (4,559,012 B / 49 members);
WPC31/PMR31/APC31/PCB2 dependencies remain retained, including original descriptors.
