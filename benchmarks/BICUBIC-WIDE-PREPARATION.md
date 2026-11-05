# Shared wide bicubic preparation

Wide byte/word column drivers now reuse the existing small-column geometry
helper. Vertical coefficients and their ordered normalizer are prepared once in
already owned, unpublished output-row tails. Prefix strips finish before those
tails retire; final strips reconstruct geometry directly. Horizontal preparation,
typed filtering kernels, tap order, certified rounding/repair and portable paths
remain unchanged. There is no new owner, pool, cache, session or public API.

Independent quiet-VM confirmations on the existing **513×259 →128×64 bicubic,
input+output+4 KiB** controls support both incremental gains. Raw and native-bracket
normalized confidence separate in filtered and all-actual views. Both controls
remain performance gaps; no broader resizing claim follows.

| Complete resize | Before µs | Current µs | Saved µs / raw gain | Current zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| RGBA8 | 504.83 | 480.55 | 24.28 /4.81% | 347.12 | 1.384 /1.394 | 113 |
| RGBA16 | 627.22 | 532.73 | 94.49 /15.07% | 404.07 | 1.318 /1.329 | 113 |

Each baseline and selected confirmation has twelve actuals. The short screen is
474.48/529.55 µs. Native clocks move −0.65%/−3.85%, so normalized reductions
4.18%/11.67% differ from the raw gains. Every actual, bracket, guard and adverse
test result is retained; no completed timing job repeats or additional variant
follows. These are actual standalone public-API comparisons, not private paired
DLL effects substituted for current costs.

For byte four-column strips, geometry construction falls from2,048 to704 row
builds; word three-column strips fall2,752→576. These are logical counts, not phase
timings. Borrowed metadata is9,728/11,264 B within32,768/65,536 B owned outputs.
Added owned capacity and retained operation scratch are zero. Fixed-array payloads
4,068/4,056 B retain their4,096 B admission. Total allocation113 B/op is unchanged;
physical/complete admitted peaks and native allocations are not remeasured.

Horizontal geometry is already constructed once per strip. Sharing its source
alone would not remove per-row work, so no generic offset/filter abstraction is
added. The existing packed/deep controls are too narrow for useful result-tail
preparation; their distinct measured kernels and storage layouts remain. Small
columns and Lanczos reuse their unchanged source and qualification.

The explicitly labeled **zimg3.0.6, FFmpeg zscale backend** retains graph/input
workspace/scratch before timing. Halo/deinterleaving, filtering, independent
output allocation/repacking/release remain timed. Managed complete public Resize
includes preparation, admission and independent output ownership/disposal. This
is the established retained-state reference boundary, not one-shot setup parity.
Managed rational pixels stay exact; native RGB/alpha maximum1 and RMSE≤0.25 remain.

Both OSes pass **5,696 full/live checks** (2,461internal/2,287public/948live), plus
45internal/521public focused checks per normal/scalar mode. Linux normal exercises
the AVX512 kernels; Windows without AVX512 and scalar runs cover public portable
behavior. Eleven new cases cover prepared-tail lifetime, odd three-column and
narrow direct geometry, exact independent direct pixels, terminal refusal and
admission-triggered cancellation/recovery. The first Linux run catches four test
setup errors: a narrow fixture exceeds the wide support guard, and cancellation
occurs during context construction. Corrected tests reuse the unchanged production
modules; original failures remain. One production/two embedded-test versions per
OS; no public-test, benchmark, native or selector builds.

The **same actual Release package/all three examples** pass Windows/Linux,
including frozen wide byte/word pixels with4 KiB admission, shared refusal,
cancellation, independently surviving outputs and subsequent recovery. One broad/
package milestone follows selection; published packages remain unchanged.

[Evidence](bicubic-wide-preparation-evidence.json) pins source/producers, exact
input/output hashes, every actual/confidence decision, actual package and mirrored,
member-verified raw archive with dependencies and reproduction. Only the two
displayed wide4 KiB costs change; other123 comparisons reuse unchanged-path proof.
All282 named paths remain required; full CPU parity and publication are separate.

Raw `bicubic-wide-preparation-qualified.tar.xz`:3,104,088 B/196members, SHA256
`98dd9c3b03a41e09459c548311257a77f921479d412630a4a7c6182b7e921d25`.
An archive postcheck initially compares resolved hard-link bytes with zero-sized
link headers. Hashing every resolved member verifies the unchanged archive;
production, qualification and completed measurements are not repeated.
