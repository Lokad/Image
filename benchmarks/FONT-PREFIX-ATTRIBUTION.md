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
