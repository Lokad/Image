# Baseline JPEG AC publication screen

One existing 513×259 resident baseline420 JPEG decode, frozen 52,093-byte packet.
Complete public decode, independently owned RGBA8 output and release remain.
All 531,468 output bytes, alpha, source and ownership gates pass.

The candidate short mean is **1,278.441 µs**, 1.49% below the applicable
[JCP1 confirmation](WARM-JPEG-PAIRED-COLUMNS.md), below the planned 5% gate.
It remains 1.267× / upper 1.421, with unchanged 757,319 B/op. Three actual/filtered
samples, both native brackets and idle-v2 guards are retained; they establish
neither an independent gain nor parity. No confirmation or variants followed.

The candidate validated table/block/coefficient layouts and replaced expected
loop exits plus noncancellable try/finally with one explicit cursor publication.
The bit algorithm, storage, ownership, cancellable and progressive paths stayed
unchanged. Existing attribution maps all 1,678 method samples to matching 556-byte
instructions. Its hot loop includes required decoding and repeated stack access;
its 23.6% whole-profile share is not purely removable exception overhead.

Both platforms pass 34 focused checks in normal and scalar modes for candidate
and restored code, including exhaustive byte pairs, phases, truncation, stuffing,
markers, EOB, partial state and independent ordinary-reader comparisons. Qualified
JCP1 source, matched DLL/PDB/XML and linked tests are restored; its actual private
package is unchanged. Applicable full/package qualification is reused.

[Evidence](jpeg-baseline-ac-publication-rejected-evidence.json) records all samples,
identities, restoration and the mirrored member-verified raw archive. The next
named item resolves PNG8 span-validation uncertainty; full 267-path parity is open.
