# Font-header normalization

Accepted complete owned-font loads improve **15.893 → 14.922 µs**, saving
**0.971 µs / 6.11%**, with paired ratio interval **0.937376–0.940380** and
separated raw mean intervals. Allocation remains **176 B/op**. All twelve reversed
alternating 16,384-operation pairs and quiet-VM guards pass. This is an incremental
effect on the frozen DejaVu Sans 2.37 Regular face, not standalone reference parity
or a cumulative historical gain.

The shared four-header gather now reverses coordinate bytes with one lane-local
AVX2 permutation. Contour words used only for equality to zero are read directly:
endianness and sign cannot change that predicate. The permutation alone has an
inconclusive screen (**15.864 → 15.802 µs**, interval **0.960344–1.033147**).
Completing normalization produces a promising screen and independent prototype
confirmation; only the final actual-DLL effect above is accepted. Stages are not
added together. Actual generated long-location validation shrinks from 1,847 to
1,731 bytes; instruction size alone is not performance evidence.

Exact textual reconstruction proves that all other source is unchanged. Bounded
ten-byte slices, coordinate bits, signed comparisons, ordered eager errors, scalar
fallbacks, cancelable loops, private input ownership and shared admission remain.
No buffer, pool, cache, public API, unsafe code or dependency is added. The
permutation has a fixed constant; total native/JIT/code retention is unmeasured.

| Complete current operation / labeled reference | Managed / reference µs | Conservative upper / status | Managed GC B/op |
| --- | ---: | ---: | ---: |
| Owned font load / FreeType 2.13.2 | 15.241 / 13.188 | 1.165467 / scoped gap | 176 |
| Text one-shot / ImageSharp public integration | 1130.261 / 6248.029 | 0.210404 / pass | 306,162 |
| Preparation + eight draws / ImageSharp public integration | 1283.201 / 6898.687 | 0.217156 / pass | 315,888 |

The font boundary includes copy, admission, validation, retained ownership and
disposal of the original 757,076-byte / 6,253-glyph face. FreeType validates more
lazily; this is not general parser equivalence. Standalone measurements retain all
twelve actuals, both reference brackets and the conservative all-actual/filtered
confidence envelope. Their clocks are not interchangeable with paired actor clocks.

Text uses stock Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12, every public layout
call, all thirty fitting trials, fresh owned explicitly cleared RGBA16 canvases
and disposal. Prepared state differs: reference positioned paths versus managed
full-precision coverage. Font validation and GC-driven reference font release also
differ. Preparation/drawing diagnostics are measured separately, never added into
end-to-end claims. Exact glyph/cluster/line/fitting checks and existing position,
bounds and full-16-bit visual gates pass. No text-pipeline speedup is claimed.

Live font admission remains **757,332 B**, idle payload **757,076 B**. Complete-fit
retained capacity remains **4,160 B**, governed peak **822,460 B**. Approximate
prepared heap deltas **4,336 / 22,504 B** exclude idle pools and physical/native/JIT
peaks. Allocation, owned capacity and retention are separate quantities.

Windows and Linux each pass **6,790 full/live checks** (3,330 internal, 2,512
public, 948 differential), focused **283 internal / 166 public** per normal,
AVX2-disabled and scalar mode, and **16,384 ordered oracle cases** in every mode.
Existing tests cover batch/tail bounds, first-error order, zero contours, unsigned
extremes, cancellation, admission refusal, retained ownership and recovery. No new
test-only production path is introduced. The same actual Release package and all
three usage examples pass both OSes. Published packages remain unchanged.

Two isolated prototype core builds, then one final core/embedded build per OS;
zero existing public/harness/reference/fixture builds and one NoBuild Release pack.
Affected performance was reconciled before broad/package qualification. The timed
Linux DLL was not rebuilt. [Evidence](font-header-normalization-evidence.json)
pins source, artifacts, all actuals, qualification and reproduction recipes.
Mirrored/member-verified `font-header-normalization-qualified.tar.xz` contains
**2,712,044 B / 211 members**, SHA256
`64aa1ab63fcb1fb889d2846cc02e7cb512b5aaefb06168853ad53056e0548e95`.

[Fused validation](FUSED-FONT-HEADER-VALIDATION.md) and
[post-fusion attribution](FONT-POST-FUSION-ATTRIBUTION.md) retain prior evidence;
their header-instruction profiles describe the previous implementation. Unchanged
borrowed text/drawing and codec/resize/copy paths reuse identified evidence.
The finite 289-path objective and final-package refresh remain open. Publication
stays postponed.

[Subsequent coordinate-reduction trials](FONT-COORDINATE-REDUCTION-TRIALS.md)
are inconclusive and leave this accepted implementation unchanged.
