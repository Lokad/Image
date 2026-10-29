# PNG retained-root cost before row selection

Superseded for current CPU cost by [caller-root row selection](WARM-PNG-CALLER-ROOT-ROWS.md).
This retained record supplies its unchanged original independent baseline.

The existing two-decode operation independently confirms **3,525.226 µs,
1.237× FFmpeg / upper 1.244**, with **2,574,629 allocated B/op**. It remains a
CPU gap. All 12 actuals/native brackets, both confidence views and idle-v2 guards
are retained. No causal gain or regression is inferred from older short screens.

The first independent RGBA8 output stays alive throughout the second decode;
both are then disposed. Both subjects process the identical original 513×259 PNG,
including normalization, fresh owned outputs and release. Managed admission is
included; FFmpeg has no matching governor API. Native wrappers are reused,
decoded content is not cached, and each operation performs two complete decodes.

The 32 MiB root and its 218,775-byte caller-input reservation are prepared outside
timing. Observed admitted peak is 2,306,560 bytes, including overlapping storage.
Exact pixels, source/root ownership, retained charges/refunds, refusal/cancellation
cleanup and surviving-output independence pass. Allocations are cumulative per
two-decode operation, distinct from peak admission; native allocations are unmeasured.

Zero builds or new controls. Current Windows/Linux 5,082 checks each, affected
scalar checks and the same actual package/three examples remain valid. This
resolves one of the 52 costs needing refresh after scan-CRC reuse; 51 remain.

[Evidence](current-png-retained-root-cost-evidence.json) pins boundaries,
identities and reuse. Mirrored/member-verified archive: current-png-retained-root-confirmation.tar.xz,
47 members, SHA256
a204c46f19cf5e6965c9cfa7eb7efa123c5655eb2261f566fa870ca99a895967.
