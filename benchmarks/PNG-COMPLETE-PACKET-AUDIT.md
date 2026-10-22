# PNG complete-packet audit

All17 existing encode/profile/delivery rows now match the complete frozen
reference packets **raw-exact**. These are14 distinct input/policy controls:
all four color types at8/16bits for Optimal, plus RGBA8/16 NoCompression,
Fastest and SmallestSize. Three additional rows exercise async RGBA8 and
live-token/forward RGBA16 delivery. No metadata exception applies.

[Managed policy qualification](PNG-MANAGED-POLICIES.md) gives the14 packet sizes,
resource boundaries and both-platform actual-package evidence. The
[numeric ledger](png-complete-packet-audit-evidence.json) maps every row to its
executed current control and retains superseded producer observations separately.
Historical5% IDAT tolerances and smaller-packet passes are not byte-exact proofs.

CPU/allocation rows remain historical and require current-producer confirmation.
No timings or new image/settings controls accompanied this integration.
