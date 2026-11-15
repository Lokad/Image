# In-bounds clipping trial

**Not retained.** A guarded return for wholly contained nonnegative rectangles
does not demonstrate a useful complete byte-copy gain. Its twelve-pair screen is
**1.968338 → 1.963435 µs**, ratio interval **0.994106–1.000928**, **0 B/op** on
both sides. The 0.249% favorable point is inconclusive. This is a scoped managed-DLL
comparison, not a new FFmpeg cost or evidence of a gain or regression.

Only the common Clip helper changed. Public validation, admission, general
clipping, pixel kernels, aliases and cancellation stayed verbatim. Existing
513×259 inputs, twelve alternating 8,192-call pairs, full source/canvas checks,
borrowed ownership, natural GC and idle-v2 guards remained. No confirmation,
neighboring variant or word/reference retiming follows.

Linux passes **25 internal/190 public** checks normally and with intrinsics off,
including clipping, extreme coordinates, empty regions and ownership. One
core/embedded build, zero other builds. Exact qualified sources, core/embedded/
public modules and source inventory are restored without rebuilding. Windows
modules and the actual package never changed. Reuse **6,721 full/live checks per
OS** and the same package consumer/all three examples; no broad campaign.

No new owned/admitted/retained capacity. Physical/native/JIT peaks are unmeasured.
Current costs remain in [prepared copy rows](DRAWING-ROW-PREPARATION.md) and
[adjacent word confidence](RECTANGLE-COPY-ADJACENT.md). No rejected-result renderer
dependency or permanent algorithm-family ban.

[Evidence](in-bounds-clipping-trial-evidence.json) retains all samples, identities,
checks and restoration. Raw `in-bounds-clipping-trial.tar.xz` (1,751,440 B/54 members),
SHA256 `00c63036d09a61ffea73b07da42746c3cd7654de31524bdaf2cfae0d92150a67`,
is mirrored/member-verified with the original helper and qualified-package
dependencies. A report encoding error was repaired after archiving without any
recapture or rebuild. Full 289-path completion remains open; publication postponed.
