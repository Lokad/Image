# CMYK output acquisition: rejected

The complete sparse-CMYK short changes **626.368 → 605.012 µs**: **3.41%**
filtered/all-actual and **1.09%** native-normalized, below the declared 4.1%
CPU screen. Confidence overlaps. No causal elapsed gain, independent candidate
confirmation or variant. Default warm allocations fall **1,085,762 → 553,950
B/op** (49.0%); that observation does not establish a CPU gain. Current accepted
sparse-CMYK cost remains 638.035 µs, 1.442× / upper 1.481, 1,085,758 B/op.

One production call uses the existing complete-overwrite allocator. The full-
resolution CMYK row, half-resolution YCCK kernel and general fallback write all
RGBA bytes before publication; failures dispose private storage. Ungoverned
calls reuse the existing bounded exact-size pool. Shared-budget calls retain
separately admitted exact-capacity allocation and lifetime charges, with no
unowned pool. No new cache, owner or admission abstraction. Governed allocations
are outside this timing observation.

Exact managed/native/scalar frames, original packet/settings, independent output
and both quiet-v2 guards pass. Candidate 353 internal/354 public focused controls
pass normal/scalar on both OSes and Linux without AVX512. One core/internal build
per OS; public assemblies, native workers and selectors reused. One Linux
benchmark assembly build clarifies setup-only allocation descriptors; timed
operation bodies are unchanged. No new benchmark input or profile.

Candidate source, binaries, receipts and failed decision are retained. Original
production, core copies, internal assemblies and benchmark source/assembly are
restored byte-exactly; public assemblies, fixtures and package are unchanged.
Reuse 2,162 internal + 2,193 public + 948 live checks per OS (**5,303**) and the
same actual package/three examples. No full-suite/package repeat for rejected
production. This allocation primitive is closed without variants.

[Evidence](cmyk-overwrite-allocation-rejected-evidence.json) retains actuals,
brackets, allocations, descriptor proof, qualification and restoration. Mirrored,
member-verified raw cmyk-overwrite-allocation-rejected.tar.xz SHA256
bc2965fe510d4e3ae23f48e6838bf8b4626c190a0fff1974db6c1d85ec11d0f6
(1,872,136 bytes, 103 members). All 76 displayed costs and 268 paths remain.
