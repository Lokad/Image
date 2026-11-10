# Narrow bicubic result-row reuse

Complete RGBA8 bicubic 513×259→256×129 with 4 KiB working admission saves
**16.28 µs / 3.33% raw**, 488.72→472.44 µs. Filtered and all-actual raw
confidence intervals separate. The normalized point gain is 3.40%; its wider
confidence intervals overlap. This is a retained incremental gain, not parity.

| Complete frozen control | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8, 4 KiB | 472.44 | 417.11 | 1.133 / 1.165 | 113 | gap |
| RGBA16, 4 KiB | 545.94 | 569.04 | 0.959 / 0.971 | 113 | pass |

The initial byte screen is 458.59/native419.01 µs. Independent confirmation
changes the reference clock regime: original 494.05/native 418.55 versus
candidate 499.42/native 450.74 µs. One targeted unchanged-build original/candidate
check resolves the raw-direction contradiction: native 416.82/417.11 µs, a 0.069%
movement. The table uses that check, with all twelve actuals retained on each
side. No additional unchanged sampling seeks a favorable ratio. The initial
confirmation and its adverse raw direction remain in the evidence.

## Preparation, execution and storage

The existing executor adds closed top/tail modes, keeping one pixel loop.
Byte/word preparation shares a closed generic helper. Thirty-two columns run
over the first 117 rows, borrowing 9,216 B cache and 2,560 B coefficients from the
last 12 unpublished result rows (12,288 B). The top frame returns before tail-only
metadata restoration and the existing eight-column execution publishes those
rows. Existing 16-column/eight-or-six-column and word fallbacks remain.

The [model](NARROW-BICUBIC-ROW-CACHE-ATTRIBUTION.md) counts row visits
3,354→1,320, source conversions 6,734→2,768 and decoded pixels 171,976→151,228.
Repeated tail work increases horizontal column-rows by 1,536 and horizontal
weight evaluations by 2,052. These are logical counts, not phase timings or a CPU
forecast. Complete timing includes preparation, restoration and repeated work.

There is **no new owned capacity**, pool, root, owner, session or public API.
Admission stays 4 KiB; explicit top stack payload 3,424 B versus old 3,232/4,080 B
modes. Independent result capacity 132,096 B (word 264,192 B) remains owned until
disposal; operation scratch retention is zero. Total warm allocation stays 113 B/op.
Existing output-pool retention and governed bypass are separate. Physical/native/
JIT-stack peaks are unmeasured; explicit payload is not a physical peak claim.

Ordered coefficient/pixel arithmetic and helpers are verbatim; only execution
row bounds change inside the loop. Geometry/math sources are unchanged. Exact
managed rational pixels, paired coefficients, certified repair, tiny-alpha unwind,
terminal refusal and cancellation remain. The preparation/entry change invalidates
both narrow complete costs, so word refreshes instead of substituting its old
passing result. Other resize/codec/text paths reuse unchanged evidence.

Eight new checks cover the actual top/tail phases, poisoned retired storage,
tail metadata, cancellation before publication, zero-allocation execution, shared
root refusal/release/recovery, low alpha and independent surviving outputs.
Functional boundaries include ten row slots, odd stride/partial 32-column strips,
and no retained geometry footer. No benchmark input/settings grid is added.

## Qualification and boundaries

Windows/Linux each pass **6,180 full/live checks** and 349 internal/729 public resize
checks per normal/scalar/disabled-AVX2/AVX512 mode. The **same actual Release
package/all three usage examples** pass both OSes, including existing bounded
narrow resize ownership/refusal/cancellation checks. One immutable core and one
successful embedded-test build per OS serve the round. An initial internal
compile requires a scoped Span lifetime annotation and produces no executable;
its failure and exact attempted source are retained. No harness/native/selector/
fixture rebuild; broad and package qualification run once per OS.

Managed timing includes complete public Resize, storage preparation, filtering,
fresh independent output and disposal. The labeled FFmpeg zscale **zimg backend**
retains graph/input/scratch; halo, packing, filtering, output allocation/release
and approved native float bounds remain timed. This is not equivalent fresh-graph
work. Exact managed results remain distinct from native tolerances. Quiet idle-v2,
one CPU, sequential Release and frozen source/output/ownership checks pass.

[Evidence](narrow-bicubic-row-loan-evidence.json) pins all actuals, sources,
comparison caveats, allocation/capacity and qualification. Mirrored/member-verified
raw `narrow-bicubic-row-loan.tar.xz`: 6,804,148 B / 161 members, SHA256
`c6496efc3e8c001f0dc0d83e89a1a01ccf09e3b380758065209ba9aaae8e473f`.
Private 0.2.1 package: 443,743 B, SHA256
`963fcbb548709ee103adb1732bf3d2a21ff72e53549fc477625c81f57fe7872f`.
Published releases stay immutable; publication is postponed. All 283 required paths
remain required; this scoped stage does not establish whole-library parity.
