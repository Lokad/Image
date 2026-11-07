# VP8 four-mode trellis execution

Four independent Luma4 prediction modes now share coefficient preparation and SIMD
score execution. Each lane preserves the original serial recurrence, integer
arithmetic, strict ties and traceback. Ordered mode scoring and winner publication
remain unchanged. Portable and cancellable calls retain the production single-mode
kernel. No encoder settings, packet, quality or metadata contract changes.

The fixed opaque diagram is 513×259, Q75/effort6, with the complete 6,818-byte
FFmpeg/libwebp packet and exact reconstructed pixels/alpha. All work includes source
processing, admission, preparation, fresh independent output and disposal, on one
quiet VM CPU. Native contexts/private scratch retain their disclosed reuse.

**Four predetermined counterbalanced standalone launch pairs save 662µs/6.4% raw**:
10,291.93→9,629.92µs. Each launch has three actuals. The paired launch means, rather
than twelve pooled within-process samples, are the inferential units. The 99.9%
candidate/original ratio interval is **[0.903483,0.968913]**; the all-actual view agrees.
The normalized interval [0.844134,1.017614] remains inconclusive. A separate isolated
DLL comparison, twelve pairs of sixteen complete encodes, shows 5.63% scoped gain,
interval [0.940505,0.946852]. Its magnitude and 754,344B accounting are not substituted
for the standalone boundary's 754,497B.

The balanced experiment resolves a material contradiction: the original independent
twelve-actual comparison measured 10,117.31→10,409.98µs, a 2.89% adverse raw point.
Filtered raw intervals separate adversely; all-actual intervals overlap. Both normalized
views overlap. Those observations remain retained. The follow-up protocol fixed all
four launch pairs and their order before execution, kept every result and prescribed
no further timing this round. Between-launch movement is observed without assigning
an unproven cause.

| Complete diagram/Q75 operation | Managed µs | Native µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Current effort6, independent twelve | 10,409.98 | 10,694.42 | 0.973 / 1.711 | 754,497 |
| Unchanged effort5, retained confirmation | 7,144.08 | 7,000.96 | 1.020 / 1.031 | 741,065 |

Effort6's isolated twelve passes its own gate (upper0.978), but the displayed upper
also envelopes the balanced study's launch-level and native-bracket uncertainty.
The last native bracket has a wide interval; all observations remain retained.
**Overall parity remains unconfirmed.** The normalized interval above uses bracket
means and cannot replace these bounds. Do not reinterpret short launches as twelve
independent launch samples.
Effort5 and seven other VP8 costs reuse precise unchanged-path evidence: PlainMode
projection and the selected single-mode kernel are unchanged. The nine cases now
have three passes, two gaps and four uncertain results. One displayed cost refreshes;
124 numerical rows reuse unchanged paths. This is a tested diagram/settings gain,
not broader compression or CPU parity.

Initial stage A reused four geometry arrays, adding1,408B explicit stack capacity.
Stage B computes geometry at each coefficient and packs traceback nodes in lane
order, removing1,296B of preparation arrays. The final kernel uses304B, plus128B
caller output, versus320B old single geometry: **112B additional explicit capacity**.
Total GC allocation is unchanged; added owned heap and retained buffers are zero.
The observed active stack subtractions total5,888B; a512B saved-register/return margin
fits existing16KiB macroblock admission. The standalone2KiB allowance is not claimed
for this kernel. Whole-operation physical peak and native allocation are unmeasured;
the20,192B rate plan and128B borrowed DP storage remain unchanged.

Original current-build attribution resolves504of623 Search leaf samples to Luma4,
119to Luma16AC and none to Y2. Samples motivate the design; they do not predict elapsed
gains. Stage A/B source, code, short screens and every adverse result remain archived.

Windows/Linux each pass **5,764 full/live checks** (2,508internal/2,308public/948differential),
82internal/140public per normal/scalar mode, and28internal/22public per AVX2-disabled
and AVX512-disabled mode. Thirteen added cases compare four lanes against actual
single-mode production execution, including signed fragments, quantizer/context
boundaries, zero tails, borrowed guards, cancellation and disposed-owner prefixes.
All22 complete sync/async packets, including the existing asymmetric effort6 input,
pass. One immutable main/embedded build per OS is reused throughout stage B.

The SAME actual private0.2.1 Release package and all three examples pass both OSes:
SHA256`49d032ada873156f9165fa9fabe6a267dff3e2c7d582b7c06005ae7f0291ce63`,420,975B.
Published releases are unchanged; publication and the full actual-package release
refresh remain postponed. All283 required material paths remain in scope.

[Evidence](vp8-four-mode-trellis-evidence.json) pins proofs, observations, qualified
modules, the package and reproducible dependencies. Mirrored/member-verified raw
`vp8-four-mode-trellis-qualified.tar.xz`:3,895,236B/322members,
SHA256`29190b53f424f2498b3872f98876fdee93766ac940d8812f481053606bf1b939`.
The preceding [coefficient-local kernel](VP8-COEFFICIENT-LOCAL-TRELLIS.md) retains
its own evidence and links earlier experiments.
