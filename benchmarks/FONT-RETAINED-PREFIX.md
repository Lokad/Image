# Retained font prefixes

Complete owned-font loads save **1.376 µs /8.29%** on the final timed/package
DLL: **16.603153 →15.227349 µs**, paired ratio interval
**0.911165–0.923164**, with separated raw mean intervals and **176 B/op**.
This is a scoped incremental effect; private/historical clocks are never added.

| Complete operation / explicitly labeled reference | Managed /reference µs | Ratio upper /status | Managed GC B/op |
| --- | ---: | --- | ---: |
| Owned font load /FreeType 2.13.2 | 14.982 /14.459 | 1.047861 /gap | 176 |
| Text one-shot /ImageSharp public integration | 1226.782 /6894.192 | 0.199701 /pass | 298,786 |
| Text preparation +eight draws /ImageSharp public integration | 1392.804 /7695.722 | 0.198924 /pass | 308,518 |

The same original 757,076-byte DejaVu Sans 2.37 Regular face stays borrowed.
A directory-driven prefix covering all thirteen supported table tags owns
**678,016 B**, **79,060 B less**. Original offsets/relative bytes, table validation,
eager glyph/mapping checks and independent disposal remain. No glyph subsetting,
rebasing, second array, parsed-font cache or caller-data retention is introduced.
Every original directory range is checked, including unretained tables; retained
ranges are also checked against the private copy. Invalid classification selects
the original parser before any owned admission, never after a refused reservation.
Encoded-input limits still cover all supplied bytes. Actual owned capacity is
admitted before allocation; cancellation/refusal/failure release unused charges.

Managed live font admission is **678,272 B**, including the 256-byte descriptor.
The original complete-fit pipeline measures **742,840 B governed peak**, down
**79,060 B**; its retained layout remains **4,160 B**. Serial default idle font
backing is **678,016 B**, separately bounded by the unchanged four-array/4 MiB pool.
Caller-budgeted fonts bypass that pool. Allocation, live admission and idle storage
are different quantities; physical/native/JIT peaks are unmeasured.

FreeType still owns the full 757,076-byte copy and validates more lazily.
Its complete copy/open/retain/dispose boundary stays unchanged; this is not
general parser equivalence or proof of an overall text slowdown. Text retains stock
Drawing 2.1.7 /Fonts 2.1.3 /ImageSharp 3.1.12: every public layout call, all thirty
fitting trials, fresh explicitly cleared RGBA16 canvases, independent outputs and
disposal. Full16-bit samples and original exact/visual gates pass. Native one-shot/
batch GC allocations remain **9,256,953 /9,494,633 B**. Prepared state differs.
Preparation-only is diagnostic; measured preparation+eight amortizes to
**174.10 /961.97 µs/draw**. Historical stages are not summed.

Fourteen new controls cover actual aggregate admission, optional lazy tables,
unknown range error order, limits, cancellation, recovery and layouts surviving
font disposal/prefix reuse. Existing tests change only retained-capacity assertions
and their limits; all glyph/layout/pixel/error gates remain. Windows/Linux each
pass **6,819 full/live checks** (3,359 internal,2,512 public,948 differential),
**306 internal/222 public** per normal/AVX2-disabled/scalar mode, and the SAME
actual Release package/all three examples. Each final core/embedded/public build
runs once per OS. Existing harness/native/fixture builds are reused. The timed DLL
is never rebuilt; affected performance reconciles before broad qualification.

The completed pipeline capture initially hits a historical fixed-peak assertion.
Postprocessing reconciles the current baseline minus the exact retained-prefix
reduction; every other check stays exact, and no samples repeat. A missing uploaded
qualification receipt fails before Linux tests start. Packaged documentation is
synchronized, including repository LF endings, after an initial package document
mismatch; three NoBuild packs retain
the same DLL, and the final package is qualified on both OSes. Initial logs/packs
remain raw evidence. No reference identity, fixture or correctness gate changes.

Only owned loading and complete integrations refresh. Borrowed font/text operations
prepare fonts outside timing and read identical retained bytes through unchanged
methods; their identified costs remain valid. Codec/resize/drawing/copy paths stay
unchanged. The scoped font gap and other named uncertainties keep the full289
objective/final165comparison refresh open. Publication remains postponed.

[Evidence](font-retained-prefix-evidence.json) records source/artifact identities,
actuals, capacity boundaries and qualification. Mirrored/member-verified
`font-retained-prefix-qualified.tar.xz`: **5,726,108 B /208 members**,
SHA256 `708287b5b0670dbbe243fb523217eb9e98fa48ee72ef5bdc93e48b3b0aabd70e`. Archived `fsp1212-*` recipes and pinned
runtime/reference dependencies reproduce the round. Prior
[storage audit](FONT-OWNED-TABLE-STORAGE.md),
[header normalization](FONT-HEADER-NORMALIZATION.md) and
[polygon scratch](TEXT-CONDITIONAL-POLYGON-SCRATCH.md) retain their evidence.
