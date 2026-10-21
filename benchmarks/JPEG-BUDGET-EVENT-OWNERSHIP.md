# JPEG adaptive-event ownership

C126 repairs the lifetime of substantial adaptive JPEG event storage when a caller
supplies `ImageLimits.MemoryBudget`. Those operations allocate under their existing
reservation and neither borrow nor populate the unowned thread-local cache. The
ordinary root-null cache and encoded packets are unchanged. No encoding decision,
quantizer, pixel, quality, reference setting or complete-operation boundary changed.

Twelve regressions fail before the repair and pass after it. They cover RGBA,
caller RGB and caller grayscale; matching/mismatched cached capacity; charged
capacity before allocation; output-quota failure; terminal shared refusal; prompt
cleanup, independent surviving outputs and subsequent success. The preexisting
cache's identity and contents remain unchanged. No garbage collection is required
to restore admission capacity.

Windows and Linux each pass 4,876 Release tests (1,969 internal, 1,971 public API,
936 live FFmpeg), plus 58 internal and 46 public scalar checks. The same actual
private 0.2.0 package and all three examples pass on both platforms. Each platform
uses one production and one accepted internal-test build; unchanged public and
differential assemblies are reused. No timing, native encode, quality sweep or
independent performance confirmation was repeated. Existing byte/quality evidence
and the original producer identities for historical time/allocation observations
remain valid; no new cost or CPU-parity claim is made.

Qualification source: `5d04af1bd09db0c738f4a21a53656c8afeeb02e9a8df170ac06d6ea8b3f89422`.
Windows producer: `8610cd1a15de8add999419ddb83927bed1c2b9536f53f3203d0c904d5b136912`.
Linux producer: `9f089fc5213ce59a56d8a62ac6639d9ee1112a8c6097c6082f07f7959b51dbc4`.
Actual package: `2fa0ee5f9f54d1d4dd02d7fb1e86a58947173f4d851d57386621a00645a472b8`.

Immutable raw qualification is mirrored as `c126-qualified-windows.tar.gz`
(2,052,546 B, SHA256 `6f90fa350943195f65da409a1c01ef709b8249426c61303c141ef8f194fd5ba2`)
and `c126-qualified-linux.tar.gz` (2,035,963 B,
SHA256 `f5cf25693eb02e3a6f2152ad33eda6e0bb354714817620747d5ae3cb92ea5b60`).
