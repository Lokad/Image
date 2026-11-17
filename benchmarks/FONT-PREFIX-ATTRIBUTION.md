# Font attribution after prefix ownership

One two-second diagnostic on the immutable timed/package DLL **e1ae64a3…**
records **1,992 leaf samples**: **1,141** in libc, **707** in
`TryLongGlyphHeaders`, **63** in mapping validation, **16** in directory lookup,
**10** in retained-length classification and **5** in tag selection. These locate
work; they are not elapsed phase costs, unavoidable lower bounds or a speed forecast.
No historical sample-count subtraction establishes a gain.

The new preparation loop is not the next target. A more substantial hypothesis is
copy/validation scheduling: the full required prefix is copied before header
validation reads its glyph records. Establish a bounded ownership/error-order
proof for shortening that reuse distance before implementing a prototype. Preserve
all table bytes/offsets and independently owned storage, including glyphs outside
the frozen text. Handle table overlap and records crossing copy boundaries; keep
the complete current fallback. Do not assume a cache size or infer CPU savings.
The original complete-load screen must decide any eventual implementation.

The original DejaVu face/FreeType tooling, exact source/metrics/cleanup checks,
CPU0 optimized JIT, natural nonconcurrent GC and idle-v2 guards pass; collector
runs on CPU1. Every instrumented clock is discarded. The first guard stops before
worker launch because two packaged-document entries have stale sizes in the VM
source manifest. Only those entries update to the qualified package-document
hashes; code, DLL and package remain unchanged. Existing verified runtime files
are reused, and the initial failure remains. One actual profile, zero builds.

Reuse **6,819 full/live checks per OS**, the SAME actual package/all three examples
and current timings. No broad/package repetition, new font/settings grid, renderer
dependency or production change. Passing paths stay stopped; named uncertainties
and the full 289-path objective remain open.

[Evidence](font-prefix-attribution-evidence.json) records exact sources, trace,
leaf parsing, manifest correction and qualification reuse. Mirrored/member-verified
`font-prefix-attribution.tar.xz`: **54,752 B /26 members**,
SHA256 `299f48515a2c2f09457af0098f7d6ad5015bf33d3dcd06627c71799f24163383`. Archived `ppa1213-*` recipes and the
identified [prefix package/runtime](FONT-RETAINED-PREFIX.md) reproduce it.

## Directory-reuse review

The current source remains `9eb17e74…`. Its two directory scans straddle admission
and the owned copy: borrowed classification selects capacity; private validation
checks the bytes actually copied. Removing the latter without a directory-equality
proof would trust an earlier input state across caller callbacks. A bounded saved
directory could prove reuse for unchanged, noncancelable input, but adds copying,
comparison, fallback and stack lifetime; malformed/order/cancellation/refusal
behavior must remain. This is distinct from simply deleting duplicate work.

Reuse the retained trace: the entire constructor has12/1,992 leaves, retained
classification10 and directory lookup16, versus707 in glyph validation and1,141
in libc. Counts neither measure a directory phase nor establish an elapsed bound.
They provide little support for selecting this additional mechanism now. No
prototype, cache, fresh profile, timing, build or qualification repeat follows.
Keep the source-valid loading gap open; this review is not an impossibility claim
or a permanent ban. Independent uncertain comparisons can proceed without
reopening passing text paths.
