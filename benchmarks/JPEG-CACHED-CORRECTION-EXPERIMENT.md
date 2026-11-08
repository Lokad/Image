# Cached JPEG correction preparation

TMB609 is unselected. The prototype prepares SIMD correction values from an
already cached field for a dense eight-coefficient group. It keeps every per-bit
poll, consumption, overflow check and immediate commit, with the original sparse/
short-cache loop. No new input delivery, heap storage, cache, owner or retained
capacity. The helper returns pending bits without exposing the caller's locals.

The complete live progressive444 three-actual screen costs **3,637.86/native
3,356.27µs**, versus source-valid **3,553.06/native 3,351.49µs**. Allocation remains
**169,664B/op**. The adverse point is +84.80µs/2.39%; wide intervals are inconclusive,
so no causal regression is established. There is no useful point lead or allocation
benefit. No confirmation, further variant, full-suite or package repeat follows.
Call/eligibility/lane extraction and code generation remain possible costs, not
isolated causes. The implementation does not close this design family.

Windows/Linux each pass **423 internal/375 public checks per normal/scalar mode**.
New core tests cover all 6,560 eight-lane mask/field pairs, extreme overflow and
following-bit state, and cancellation precedence. SIMD tests execute on supported
hardware; existing scalar fallback and concurrent bitwise-prefix models remain.
One initial Windows compile needed a `scoped` span annotation and produced no
executable. One final main/embedded build per OS; no worker/native/fixture builds.

The frozen 132,961B packet still produces the exact independent 531,468B frame.
Complete public preparation, decoding, conversion, output and disposal remain
timed. Original native context/cancellation boundaries, source/ownership gates and
idle-v2 single-CPU guards remain. Instrument counts are not elapsed evidence.

Sources, main/embedded modules, public/differential/probe bindings and the Linux
manifest restore byte for byte. Prototype-only helper/tests are removed. Reuse
the qualified 5,924 checks per OS and SAME actual package/all-three-example proof;
numeric summaries and release packages are unchanged.

[Evidence](jpeg-cached-correction-experiment-evidence.json) retains design/model,
all actuals, checks and restoration. Mirrored/member-verified raw
`jpeg-cached-correction-experiment.tar.xz`, SHA256
`81d633ad48554eea28dcfb7f62698767f98e951517b36c583b9f14cb07656e9e`,
5,207,088B/81 members, preserves candidate/original modules, source, failed compile
and reproduction, with [current attribution](JPEG-LIVE-CURRENT-ATTRIBUTION.md)
and qualified worker/input dependencies. No rejected study becomes a renderer input.
