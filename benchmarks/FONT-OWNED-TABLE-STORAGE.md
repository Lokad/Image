# Owned-font table storage

A bounded source/storage audit finds **81,472 B /10.76%** of modeled copying
headroom on the frozen 757,076-byte DejaVu Sans 2.37 Regular face. This is a byte
and lifetime calculation, **not a measured CPU gain**. Current font-loading costs
and qualification remain unchanged; no prototype or production change is accepted.

The four `TrueTypeFont` files and `TextShaper` consume thirteen named table tags:
`head`, `hhea`, `maxp`, `glyf`, `loca`, `hmtx`, `cmap`, `kern`, `vhea`, `vmtx`,
`GDEF`, `GPOS`, `GSUB`. Eleven occur in this face; vertical metrics are absent.
The eleven payloads total **675,262 B**. All twenty original directory entries
still require range/ordering validation, including the nine unconsumed tables.
There is already exactly one full input copy; no redundant copy was discovered.

An aligned compact representation retaining the original directory size would
use **675,604 B**, versus the current **757,076 B** backing. A simpler retained
prefix through the final consumed table uses **678,016 B**, saving **79,060 B**
without rebasing offsets. Full compaction saves only **2,412 B** beyond that
prefix. These are logical capacities; allocator/physical/native peaks are unmeasured.
The current coarse owned charge is **757,332 B**, including its 256-byte descriptor.

Next, design a private directory-driven retained-prefix experiment before changing
canonical source. Use all current consumers rather than a face-specific table
order or glyph subset. Preserve validation of the **original** directory and its
first errors; copying less must not turn an ignored valid table into truncation.
Preserve checks on all retained offsets after the private copy, including malformed
inputs. Do not first copy the full face and then replace it: that repeats copying
and adds overlapping ownership. Do not add a parsed-font cache or retain caller data.
Reserve actual owned capacity before allocation; keep borrowed-input limits,
terminal refusal, cancellation, independent survivors and disposal explicit.

Only a complete original owned-load screen can establish useful elapsed savings.
If promising, independently confirm and reconcile the affected font/text operations
on the final immutable artifact before broad/package qualification. Retain the same
FreeType 2.13.2 owned copy/open/retain/dispose boundary and disclose eager/lazy
validation differences. Neither the byte reduction nor historical sampled libc
shares predicts CPU savings. Existing one-shot/repeated text passes stay closed
unless their executed production path changes. No expanded font/settings grid.

This audit reads only the licensed public frozen face and the seven pinned
implementation/ownership files. No private face, new fixture, build, VM timing,
package proof or renderer dependency is added. The full 289-path objective remains
open. [Evidence](font-owned-table-storage-evidence.json) records every table extent,
consumer tag, source identity and the capacity arithmetic. Provenance remains in
`tools/TextReference/font-provenance.json`; external font bytes are not tracked.

Mirrored/member-verified `font-owned-table-storage-audit.tar.xz`: **22,724 B /14 members**,
SHA256 `562532ea697b823cfe37da7554cc9f0731be376876afa6277b2d4f105bd1d023`.
The archived `fsa1211-audit.py` reproduces the calculation using the frozen face;
`fsa1211-record.py` records the source/fixture identities and archive verification.
