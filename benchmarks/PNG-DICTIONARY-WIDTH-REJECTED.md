# Rejected PNG dictionary-width CPU trial

The existing Fastest RGBA8 control screens **6,233.21 µs** versus the unchanged
**6,318.22 µs** baseline: 1.3% less elapsed, below the predeclared 5% CPU gate.
Allocation drops from 1,344,453 to 1,082,242 B/op (19.5%). Preserve that allocation
evidence for later work, but reject this CPU trial without confirmation or variants.

Two dictionaries used checked unsigned 32-bit entries while keeping long counters,
chains and ties. Their owned capacity and admitted workspace fell by 256 KiB.
Public PNG input, including filters, is bounded below 2³² bytes by the existing
RGBA array limit; source depth remains unchanged. No new public quota was added.

The exact 236,828-byte packet, pixels, ownership and quiet guards pass. Each OS
passes 18 focused tests normally and with SIMD disabled, plus 176 complete-byte/
flush, 32 failure/cancellation and 12 high-index comparisons per mode against
frozen own source. High indices use translated small streams, without giant images.

Qualified source, matched DLL/PDB/XML and linked tests are restored; the actual
package and full qualification remain unchanged. [Evidence](png-dictionary-width-rejected-evidence.json)
retains all samples, proofs and the mirrored, member-verified raw archive. The
current benchmark summary and full performance objective remain unchanged.
