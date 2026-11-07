# Owned font-load preparation

Font loading prepares its validated `loca` and glyph-table views once for the glyph
scan. Glyph headers retain their original ten-byte minimum and explicit fixed
extent. Fresh, exact-length managed storage is completely overwritten by the caller's
font bytes before parsing or publication; admission precedes allocation. No cache,
pool, lazy-validation path, parser dependency or ownership policy is added.

Independent complete loading saves **14.84 µs / 4.80% raw** and **8.03%
reference-normalized**. Both filtered and all-actual confidence intervals separate:
candidate/original [0.943068, 0.960914] raw and [0.891759, 0.948324] normalized.
The reference moved +3.51%; the raw gain and current reference cost remain distinct.

| Frozen DejaVu Sans Regular load / FreeType 2.13.2 | Lokad µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Contemporary qualified original, twelve actuals | 308.95 | 13.47 | 22.930 / 23.312 | 757,598 |
| Independent current, twelve actuals | 294.11 | 13.95 | 21.089 / 21.246 | 757,597 |

The single-CPU operation includes the complete 757,076-byte copy, face creation,
validation, glyph-count consumption, owned result and disposal. FreeType includes
owned copy/open/retain/dispose; library/input setup, I/O, verification and IPC are
excluded equally. Eager/lazy validation differs: this is not general parser
equivalence. All frozen source, metrics, ownership and idle-v2 gates pass.
The large reference gap remains open. Claims cover this 6,253-glyph Regular face.

The short screen was followed by one contemporary original and one independent
candidate confirmation, with twelve samples per reference bracket. Historical
339.68/14.88 µs is superseded current cost, not a paired baseline or causal gain.
No unchanged favorable repeats or expanded font/settings matrix.

Owned array capacity stays 757,076 B, plus the existing coarse 256 B descriptor
charge retained until font disposal. Allocation diagnostics use 656 original and
688 candidate operations; their one-byte rounded difference is not a causal
production reduction. Added payload/retained state is zero. Whole frozen-face
admission/physical peak and native allocations are not newly measured. Existing
shared-root, refusal/cancellation and independently surviving layout tests pass.

The attribution capture retained 473 total / 210 managed leaves: table spans65,
headers56, locations40 and constructor43. Instrumented elapsed is discarded.
Current generated scanning no longer calls `Location` per glyph; code size does
not explain the gain: Load168→201 B, constructor2,750→2,894 B and Header357→384 B.
The fixed extent does not establish generated bounds-check elimination. No
per-component causal gain is claimed; only the complete-operation result above.

Exact source undo isolates copy and validation preparation. Directory bounds,
short/long location values, ordering, every glyph header, cmap checks, limits and
per-glyph cancellation stay required. Every newly allocated byte is copied before
any read or escape. Failed parsing releases the operation owner; retained faces
keep their charge. Metric, outline, layout, shaping and rendering policies remain.

Windows/Linux each pass **5,774 full/live checks** (2,515 internal / 2,311 public /
948 differential), plus204 internal /144 public each normal/scalar. Two added
public rows cover malformed short/long location tables, short headers, reversed
bounds, unchanged caller buffers, owned-copy cleanup and subsequent successful
loads. Existing authored/supplied-font, shaping and ownership coverage remains.

The SAME actual private 0.2.1 Release package/all three examples pass both OSes:
SHA256 `6b5e4fe6e87ab64fbbbaabced04ea7bd741341436e422b2b32286dbeba09be53`,
421,317 B. One immutable core/embedded/public build per OS is reused; zero benchmark,
reference-worker, selector or fixture builds. One displayed cost refreshes; 124
numbers reuse unchanged paths. Font loading remains outside other prepared text
timings. Published packages are immutable; publication and all283-path parity remain open.

[Evidence](font-load-preparation-evidence.json) pins source proofs, every measurement,
qualification and mirrored/member-verified raw inputs. Raw
`font-load-preparation-qualified.tar.xz`:4,248,636 B /141 members,
SHA256 `3253da301972a8468b23753d4794d50d1f6169713d4c613deec4b03b0a520ccc`.
Its pinned attribution dependency retains the original capture and generated code.
