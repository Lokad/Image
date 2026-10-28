# Rejected PNG insertion inlining

The existing Fastest RGBA8 control screens **7,039.46 µs baseline** versus
**7,037.84 µs candidate**: effectively flat (0.023%), below the 5% gate.
Both allocate 1,344,453 B/op and retain the exact 236,828-byte packet.

The trial added one `AggressiveInlining` hint to dictionary insertion. Its hash,
storage, limits and ownership were unchanged. The retained trace placed 12.1%
of samples in insertion, including its actual hash work. A matching 936-byte
caller replay mapped all 2,843 sampled instruction addresses and showed two
indirect insertion calls. That lead did not produce a material screen gain.

One fresh baseline resolves native-reference drift from the reused baseline.
All actuals, native brackets, pixels, hidden RGB, ownership and quiet guards remain.
Each OS passes 18 focused tests normally/scalar, plus 176 byte/flush and 32
failure/cancellation comparisons per mode against frozen own source.

Qualified source, matched DLL/PDB/XML and linked tests are restored. The actual
package and existing full qualification are reused. [Evidence](png-insertion-inlining-rejected-evidence.json)
retains candidate sources/binaries, diagnostics, samples and restoration proofs.
Stop this family without variants or confirmation; current benchmark costs remain.
