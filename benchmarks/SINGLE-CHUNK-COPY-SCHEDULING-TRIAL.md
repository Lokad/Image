# Single-chunk copy scheduling trial

**Not retained; inconclusive.** Preparing the one-chunk row extent does not
demonstrate a useful complete public rectangle-copy gain:
**1.988141 → 1.989948 µs**, ratio interval
**0.999303–1.002516**, **0 B/op** on both subjects.
The approximately0.091% adverse point establishes neither regression nor FFmpeg parity.

The candidate prepares byte extent and the single-chunk decision once for rows
up to the existing 4,096-pixel cancellation chunk. It avoids repeated chunk-size,
progress and direction arithmetic while retaining the same slices, copy kernels
and poll per row. Wider rows keep their original loop. Shared geometry, row
advance, clipped/aliased samples, validation, admission and ownership remain.
No new pixel kernel, storage, cache, pool, owner or public API.

One original quiet-CPU0 screen retains all twelve alternating 8,192-call pairs,
exact source/canvas and independent-subject checks. No independent confirmation,
other-case timing, generated-code capture or broad/package campaign follows.
Linux passes **25 internal/190 public** checks in normal/scalar modes, including
both depths, clipping, small rows, 8,202-pixel multi-chunk aliases, cancellation,
limits and recovery. Exact qualified sources/core/embedded/public modules and
source inventory are restored without rebuilding. Windows modules and package
never change; reuse **6,721 full/live checks per OS** and SAME actual-package/
all-three-example qualification. One Linux core/embedded build; no other builds.

Added owned/admitted/retained capacity is zero; physical/native/JIT peaks are
unmeasured. [Current copy costs](DRAWING-ROW-PREPARATION.md),
[word-copy uncertainty](RECTANGLE-COPY-ADJACENT.md) and the
[word-flip pass](OWNED-FLIP-CURRENT-CONFIDENCE.md) remain separate.
No rejected-result renderer dependency or scheduling-family ban.

[Evidence](single-chunk-copy-scheduling-trial-evidence.json) retains all samples,
source/module identities, proof and restoration. Raw `single-chunk-copy-scheduling-trial.tar.xz`
(1,751,700 B/54 members), SHA256
`7405019cca28e600670f45f2c7b495be4c06d69e4be24475b564fc3482d7b2ef`, is mirrored/member-verified with original helper and
qualified-package dependencies. Full289-path completion remains open;
publication is postponed.
