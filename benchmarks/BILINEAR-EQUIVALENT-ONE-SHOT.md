# Equivalent one-shot bilinear enlargement

BFB1055 qualifies the existing 513×259→1027×519 RGBA16 public bilinear Resize
against an equivalent fresh FFmpeg graph. No production code or pixel contract
changes. The earlier [fresh-managed/prepared-native diagnostic](BILINEAR-WORD-CURRENT-CONFIDENCE.md)
remains uncertain; its cited eight equivalent controls cover bicubic, not bilinear.

Independent confirmation: **1,973.05 / FFmpeg 5,139.83 µs**,
ratio **0.383875**, conservative interval **0.340508–0.434451**,
**4,498,574 managed B/op**. This complete one-shot boundary passes/stops.
All twelve alternating 256-operation pairs, every native before/after bracket,
natural GC and quiet CPU0 guards remain. The separate eight-operation screen also
passes. No historical clock subtraction, production speedup or phase-cost claim.

Both sides borrow caller-resident source rasters. Native graph/context/geometry,
lazy first-use storage, result wrapper, packed/planar conversion, triangle filtering,
unpremultiplication, quantization and independent output/disposal are timed each
operation. The graph is destroyed before output publication; outputs survive other
operations and graph disposal. The input frame is retained caller state, matching
the managed input ImageBuffer boundary. Managed geometry, working storage, exact
filtering/repair, admission, result and disposal all remain timed.

The original graph preparation body and filtering/pixel-oracle bodies are preserved.
Frozen input, rational managed pixels and native frame hashes match the original;
normal/scalar FFmpeg CLI checks remain exact. Native-only RGB/alpha maximum1 and
RMSE≤.25, zero-alpha RGB, source immutability and independent owners pass unchanged.
No codec correctness or production dependency is relaxed.

Managed capacity remains 4,264,104 B output +102,892 B geometry +131,520 B rows;
operation scratch retention is zero. Caller-root admission/refusal/cancellation
qualification is reused. Native graph/result state is released each operation;
its padded borrowed source is 4,128×259 B. Native allocation and physical/native/JIT
peaks are unmeasured. Allocation, admitted capacity and retention stay separate.

The actual immutable package DLL is `289e66f6523429d8e033f16f5e88941da830ea44c9d846ff5263bed2bb83cf1e`; package
`14ac5ffd20e8c8cddeea54ab534cd8cfdd41e4b5cd6c5db0236b4fe02b988af5` retains **6,721 full/live checks per OS**,
normal/scalar/ISA evidence and the SAME-package consumer/all three examples.
One harness and one native-worker build reuse the driver, fixtures and library.
No production/internal/public-test build, full-suite repeat, repack or package-proof
repeat. A wrapper syntax error occurred before workload execution and is retained;
no completed capture was restarted. The driver’s historical generic retained-state
wording is superseded by the fresh-specific harness boundary and native identity.

[Evidence](bilinear-equivalent-one-shot-evidence.json) records complete scope and
reproduction. Raw `tmp/bilinear-equivalent-one-shot.tar.xz`: 2,161,056 B /61 members,
SHA-256 `9f382146d3cc106f3f2fa5644cfe3f1717397c2e7949daf4c3d996e3c34e7572`, mirrored/member-verified. It retains sources, frozen producers,
both captures and source proofs; dependencies retain the prior package/reference.
All 289 material paths remain in scope; this adds one equivalent view, not a new
image/settings grid. The original asymmetric uncertainty stays visible. Publication
is postponed. Use `build-bilinear-linux.sh --one-shot` and the frozen Up16 harness
with `LOKAD_IMAGE_WARM_BILINEAR_ONESHOT=1` to reproduce the lifetime boundary.
