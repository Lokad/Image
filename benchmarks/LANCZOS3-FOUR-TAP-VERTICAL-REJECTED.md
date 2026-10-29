# Four-tap Lanczos3 vertical trial

One four-tap streaming trial is rejected. The existing complete RGBA8 reduction
screens **809.961 µs,1.460× / upper1.801,138,454B/op**. Native point costs ran
13.1% above the unmatched retained confirmation; the ratio point difference is
only2.68%, missing the5% triage gate. No causal absolute gain/regression or
independent speedup claim follows. No variants or longer confirmation.

The candidate retains sequential per-channel FMA order and zero-weight skips,
writing sums once per four taps. Consecutive rows occupy distinct cache slots;
support above64 keeps streaming, with the original one-to-three-tap tail. Plans,
certificate, exact fallback, storage, cancellation and admission/ownership stay
fixed. A static frozen-plan estimate cuts sum-buffer traffic25.1→6.5MB; that
estimate does not establish a complete-operation performance improvement.

All58 focused controls pass Linux normal/scalar/AVX512-off and Windows
normal/scalar, including wrapped cache slots, guarded spans, exact FMA bits and
the existing >64-support oracle. Exact frozen managed/native frames, independent
owners and both quiet guards pass. Windows lacks AVX512; that arithmetic executes
on Linux. One Linux core build; cached harness/native/selector reused.

Source/tests and qualified core/PDB/XML are restored.54restored focused checks
pass per OS. Current full5,065-per-OS/live and same actual package proof carry
forward; no full/package repetition for rejected code. Current summaries and
the268-path objective are unchanged.

[Evidence](lanczos3-four-tap-vertical-rejected-evidence.json) retains actuals,
brackets, confidence views, proof and restoration. Mirrored/member-verified raw:
lanczos3-four-tap-vertical-rejected.tar.xz,55members,SHA256
2043f3881fb42368cbcc9a9edf8685ac62e75ae49a2b20b563098e64934dc748.
