# PNG byte channel packing

Updated 2026-10-17. The three existing gray8/RGB8/gray-alpha8 encodes independently
meet warm single-CPU parity. Managed time falls **47.3%/16.6%/29.3%**, with exact
PNG packets and frozen compression settings. Stop these cases; uniform parity
on the fixed 207-case checklist remains open.

| Existing encode | Managed µs | Reference µs | Ratio / upper | Managed B/op | PNG bytes managed/native | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| gray8, Optimal managed9/native6 / None | 1,871.37 | 1,934.46 | 0.967 / 0.974 | 263,699 | 72,373/72,586 | Parity confirmed |
| RGB8, Optimal managed9/native6 / None | 8,712.51 | 9,507.55 | 0.916 / 0.922 | 527,032 | 163,358/168,729 | Parity confirmed |
| gray-alpha8, Optimal managed9/native6 / None | 4,174.10 | 4,289.79 | 0.973 / 0.980 | 264,179 | 120,036/120,366 | Parity confirmed |

All inputs are the established projections of the original 513×259 RGBA8 raster.
Complete public EncodePng includes per-operation options, fresh MemoryStream
creation/growth and disposal, without redundant ToArray. Native complete
conversion/packing, encode and independent owned packet acquisition/release stay
timed. Managed Optimal9/native6, None filter, exact samples/alpha/hidden RGB,
packet hashes, IDAT sizes and the unchanged 5% IDAT gate pass. Compression policy
is unchanged. Native allocations remain unmeasured; managed B/op includes harness
amortization. No image, workload or quality/settings grid was added.

One current six-second profile places 21.0% of 5,933 userspace samples in Stride,
6.9% in Pixels, 20.0% in Encode and 50.5% in runtime compression. These are
attribution fractions, not wall-time timing. The first candidate replaces four
per-pixel accessors with one source span per original 4,096-pixel cancellation
block. Its three short ratios are gray1.042, RGB0.931 and gray-alpha1.003; gray
already improves 43%. The retained second candidate adds bounded AVX512F/BW/VBMI
packing within that same primitive. Sixteen validated RGBA pixels select 16/48/32
output bytes. Invalid groups retain the scalar prefix and first failing pixel,
including RGB-before-opacity validation. Other hardware keeps the improved span
path. No workspace, per-operation allocation, work/admission, write or ownership change.

Removing the byte0/2/4 dispatch and two packing helpers restores the entire
predecessor PngCodec file byte for byte. RGBA8, all word encodes, PNG decode/
metadata, JPEG/WebP and editing retain their executed source. **204 comparisons
and all 54 prior passing proofs are reused.** Only three existing rows are replaced.

Each candidate reuses one immutable production/harness/worker/fixture build.
The first three screens are archived before mutation; all three final means are
read before one independent twelve-actual confirmation each. All six screens,
three confirmations, every actual/native bracket and both exclusive-vm-idle-v2
guards remain retained, including broad short bounds. No selector/harness/native
build or speculative long confirmation was needed.

All **3,646 Release cases per OS** pass (1,340 internal, 1,408 public, 898 live
FFmpeg), plus 36 scalar packing/admission controls per OS. Four new public cases
exercise block tails, exact independent BCL inflation, source preservation,
invalid vector groups, first-pixel error order and header cancellation. The same
actual Release package passes its consumer and all three examples on Windows/Linux;
symbols match the qualified DLL, and Linux production bytes remain identical
through qualification. Unaffected scalar proofs and released 0.1.0 artifacts/tag
are preserved.

[Numeric evidence and archive hashes](warm-png-byte-packing-evidence.json) retain
both candidates, all statistics/receipts, compressed sizes, profile, identities
and qualification. Main raw archive `parity-p22-qualified-evidence.tar.gz`, SHA256
`d9946895951d8b144576b6d202238f9cc7d629abbbd9b89641e3d4eccb028fb0`.
