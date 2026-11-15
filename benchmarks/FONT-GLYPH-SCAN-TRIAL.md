# Glyph-scan helper trial

GVS1054 moved the unchanged eager glyph-header scan into one private helper to
shorten constructor live ranges. Reads, error order, every cancellation poll,
ownership and admission stayed unchanged. It added no storage, pool or cache.

The existing quiet-VM short complete owned-font load/dispose screen measured
**21.048 µs**, **176 B/op**, versus explicitly labeled
FreeType 2.13.2 **13.458 µs**. All three actuals, native brackets,
source/metrics/ownership checks and idle guards remain. Earlier qualified
**21.136 µs** is context, not a contemporary baseline.
This screen did not justify further confirmation. No gain, regression or parity
is established; this implementation is not retained.

Constructor/helper code is 2,508 + 626 B, versus the former 3,094 B constructor.
Code size and FPA1053's 1,998 sample locations are diagnostic, not CPU forecasts.
The raw archive retains the actual-module profile, JIT and discarded instrumented
timings. FreeType validates differently/lazily; loading remains a scoped diagnostic.

Both OSes passed 265 internal /161 public focused checks in normal/scalar modes.
The exact qualified source, core and embedded-test snapshots were restored;
unchanged tests and source-valid full/package evidence are reused. One candidate
core/embedded build per OS; no worker, fixture, broad or package campaign.
The [current complete one-shot and eight-draw integrations](FONT-FIXED-HEADER-VIEWS.md)
remain passing on the restored package. Never add historical stage timings.

[Evidence](font-glyph-scan-trial-evidence.json) records hashes and reproduction.
Mirrored/member-verified raw `tmp/font-glyph-scan-trial.tar.xz`: 959,608 B /
92 members, SHA-256 `927f3a18cc5d08746cb43b7f2c15d8b0495de29bb3dc7fb673e6df57ccc656ae`. It depends on the retained fixed-header raw
archive listed in the evidence. This history supplies no current renderer result.
Publication remains postponed; no broader font/parser/overall text claim.
