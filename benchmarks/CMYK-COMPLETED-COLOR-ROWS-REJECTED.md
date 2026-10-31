# Completed CMYK color rows: rejected

The complete Ac decode short changes **556.403 → 546.505 µs**: **1.78%**
filtered/all-actual and **4.69%** native-normalized. Raw improvement misses the
retention gate and intervals overlap. No causal gain claim, independent candidate
confirmation or variant. Allocation is unchanged within noise: 1,085,759 →
1,085,760 B/op. The accepted Ac cost remains 535.551 µs, 1.366× / upper 1.396.

The candidate compares all four component rows, copying the previous completed
RGBA row only when every visible sample agrees. It adds no storage or owner.
Exact managed/native/scalar frames, frozen inputs/settings, ownership and both
quiet-v2 guards pass. Candidate 353 internal/354 public controls pass normal and
scalar on both OSes, plus Linux without AVX512. One core/internal/public build
per OS; no benchmark/native/selector builds or new benchmark inputs.

Production, core copies, linked internal assemblies and temporary fixture
overlays are restored byte-exactly. Eight public controls remain: individual
component changes including black, both inversion modes, right edges, exact Q16
pixels, independent rows and retained survivors. Restored public suites pass
**2,193 checks per OS**, plus eight scalar controls and Linux AVX512-off.
Unchanged 2,162 internal + 948 live checks and the same actual package/three
examples are reused: **5,303 applicable checks per OS**. No package regeneration.

One bounded two-second diagnostic of restored production collects 1,931 samples:
20.8% inverse kernel, 18.7% baseline block dispatch, 17.9% entropy block decoding,
9.5% dequantization and only 3.1% color-row conversion. Native/unresolved is
18.7%; it is not attributed to GC or clearing. These are sampled shares, not
component elapsed costs. One diagnostic-host build; exact pixels and guards
pass. Further color packing/row-reuse variants are closed.

[Evidence](cmyk-completed-color-rows-rejected-evidence.json) retains actuals,
brackets, decisions, restoration and diagnostic shares. Mirrored/member-verified
raw cmyk-completed-color-rows-rejected.tar.xz SHA256
61f7bf2497e869fbca74d020cb9ccf20a6af58072f70fc3f7e0c667541bfacb0
(3,279,800 bytes, 109 members); restored proof/diagnostic archive
cmyk-completed-color-rows-restored-proof.tar.xz SHA256
3dad4914ccd2a9061bf50dc0808883d56796743371cca114b80c7bc8251196a2
(522,212 bytes, 38 members). Current 76 costs and all 268 required paths remain.
