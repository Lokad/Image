# Rejected PNG word bit buffer

The existing 513×259 RGBA8 Fastest control screens **6,318.22 µs baseline** versus
**6,085.02 µs candidate**: 3.7% less elapsed, below the predeclared 5% gate.
Allocation rises from 1,344,453 to 1,344,578 B/op. Reject without confirmation or
bit-buffer variants; these short observations establish no independent gain.

The candidate emitted 32-bit words from a wider accumulator while preserving the
exact 236,828-byte packet, pixels, alpha, hidden RGB, source and output ownership.
All actuals, native brackets and passing quiet guards remain retained.

Each OS passes 18 focused tests normally and with SIMD disabled, plus 176
complete-byte/flush and 32 cancellation/writer-failure comparisons per mode
against frozen own production source. Shared ownership and cleanup remain exact.

Qualified source, matching DLL/PDB/XML and linked test assemblies are restored.
The actual package and existing full Windows/Linux/package proofs are reused.
[Evidence](png-word-bit-buffer-rejected-evidence.json) identifies the mirrored,
member-verified raw archive with candidate source, binaries, samples and proofs.
The current benchmark summary remains unchanged. The full parity goal stays open.
