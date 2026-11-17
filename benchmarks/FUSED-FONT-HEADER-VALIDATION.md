# Fused font-header validation

Accepted final-DLL controlled loads improve **16.566 → 15.844 µs**, saving
**0.722 µs / 4.36%**, with ratio interval **0.951273–0.961513** and separated raw
mean intervals. Allocation remains **176 B/op**. All twelve reversed alternating
16,384-operation pairs and quiet guards pass. This is an incremental implementation
effect, not standalone reference parity or a cumulative historical gain.

The noncancelable long-location path classifies eight adjacent record extents in
SIMD, then feeds their decoded offsets directly to two shared four-header bounds
checks. This removes repeated scalar structural checks and offset decoding.
The intermediate two-pass implementation adds a table traversal and shows no
useful gain; its original screen is retained. Fusion independently confirms on
the prototype, then on the actual final Linux DLL. Only the final effect above is
the accepted current gain.

All reads remain bounded spans. Constructor-owned private bytes remain stable
between classification and gathering. A failed classification returns to the
original ordered validator; a scalar tail reports errors only after earlier
headers pass. Signed bounds, eager checks, short-location/cancelable/scalar paths,
admission, output ownership and cleanup remain. No buffer, pool, cache, public API,
unsafe code or dependency is added.

| Complete current boundary / labeled reference | Managed / reference µs | Conservative upper / status | Managed GC B/op |
| --- | ---: | ---: | ---: |
| Owned font load / FreeType 2.13.2 | 15.872 / 13.152 | 1.219689 / scoped gap | 176 |
| Text one-shot / ImageSharp public integration | 1140.099 / 6243.338 | 0.208898 / pass | 306,168 |
| Preparation + eight draws / ImageSharp public integration | 1253.421 / 6880.332 | 0.204495 / pass | 315,892.17 |

The frozen DejaVu Sans 2.37 Regular face is 757,076 B with 6,253 glyphs. Font
loading includes copying, admission, eager validation and disposal; FreeType's
validation is lazier. This establishes no general parser equivalence. Text uses
stock ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12, every layout call,
all thirty fitting trials, fresh cleared independent RGBA16 canvases and disposal.
Preparation and prepared drawing remain scoped diagnostics; stage times are not
added into end-to-end claims. All native identities, frozen outputs and gates stay.

Both OSes pass **6,790 full/live checks**: 3,330 internal, 2,512 public and 948 live
differential. Focused checks pass **283 internal/166 public** per normal, AVX2-off
and scalar mode; the actual final DLLs each pass the **16,384-case ordered oracle**
in all three modes. Fourteen new public-operation regressions cover eight-record
boundaries/tails, empty/zero-contour records, unsigned extremes, first-error order,
borrowed-root charges and successful recovery. The **same actual Release package**
and three usage examples pass both OSes. Published packages remain unchanged.

The final timed Linux DLL is built once and never rebuilt. One final core/embedded
build per OS, two isolated Windows prototypes, zero existing public/harness/
reference/fixture builds and one NoBuild Release pack. Live font admission remains
757,332 B; idle font payload remains 757,076 B. No allocation/retention reduction
or physical/native/JIT peak is claimed.

[Evidence](fused-font-header-validation-evidence.json) pins sources, every stage,
all actuals, final artifacts and qualification. Mirrored/member-verified raw
`fused-font-header-validation-qualified.tar.xz`: **2,203,304 B / 200 members**,
SHA256 `96ccf53f147e5458a32ee4dc1931c7c577192031ffc8c4ce0418881c5bb19167`.
Archived `lhv1174-*` / `fhv1175-*` recipes and declared dependencies reproduce the
round. Unaffected ordinary/prepared/authored drawing and codec/resize/copy paths
reuse identified evidence. The finite 289-path parity objective remains open;
publication stays postponed.
