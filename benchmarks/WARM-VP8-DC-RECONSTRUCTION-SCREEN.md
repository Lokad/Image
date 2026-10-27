# Rejected DC-only encoder reconstruction shortcut

The existing effort-6 diagram short measures 19,839.74 µs, 2.029× reference time
(upper 2.099×), 754,328 B/op and an exact 6,818-byte packet. Its 0.488% elapsed
difference from the preceding short is within uncertainty. The candidate is
rejected; no confirmation, full qualification or package repeat is warranted.

The candidate detected zero AC after the existing complete coefficient snapshot,
using the original saturated row writer for DC or returning for zero. The frozen
229,504-case inverse oracle, every signed DC value, 90 aliases and full native
packet remained exact. Both systems passed 25 focused checks.

Source and selected production binaries are restored byte-for-byte. Linked tests
were rebuilt, then all 25 focused checks passed normally and with intrinsics
disabled on each system. The prior full/scalar/actual-package qualification stays
valid. [Evidence](warm-vp8-dc-reconstruction-screen-evidence.json) retains the
candidate, raw samples, guards, restoration and mirrored, member-verified archive.
Stop reconstruction variants.

The next bounded lead is repeated zigzag construction inside coefficient trellis.
Reuse the sixteen-position specification geometry, preserving all arithmetic,
validation, bounds, cancellation and ownership. No new kernel, table policy,
profile, image or settings grid is needed.
