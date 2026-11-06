# VP8 live coefficient-model preparation

Borrow fixed eleven-byte rows from the decoder's complete private coefficient
model instead of delivering dynamic spans at every token. Complete live opaque
decoding independently improves **48.60µs /0.83%raw**, with unchanged760,564B/op.
Reference-normalized confidence overlaps: this is a scoped raw reduction, not a
confirmed normalized gain or FFmpeg parity.

The existing1,056Bmodel contains96rows in RFC6386section13.2order. A safe borrowed
`MemoryMarshal.Cast` view and eleven-byte inline row give token decisions a fixed
model boundary without copying or allocating. Existing header/model validation,
read order, arithmetic/normalization/refill, category/sign, every per-bit cancellation
checkpoint and finally publication of the completed prefix remain exact. No owner,
pool, cache, session, admission or numerical policy changes. Unchanged normal/tail
and noncancelled kernels retain their measured specialization.

One preceding two-second current live capture has1,961samples:874in token
decisions,611in coefficient execution and31in large prediction. Repeated model
delivery/checks are an investigation lead; constructor/publication is not isolated
as dominant. Samples/skid are instruction locations, not phase times or predicted
savings. Qualification disassembly is Tier0 functional evidence and supplies no
optimized benchmark code-size claim. Prior failed generic arithmetic-state sharing
is distinct from this model representation change.

| Complete513×259live opaque decode | Original µs | Current µs | Managed B/op |
| --- | ---: | ---: | ---: |
| Independent12actuals each | 5,846.28 | 5,797.69 | 760,564both |
| Current FFmpeg reference, faster native bracket | — | 4,787.47 | Unmeasured |

Both filtered/all-actual raw intervals separate. Native point changes−0.202%;
normalized point improves0.631%, with candidate/original interval.986083–1.002342.
The current ratio1.211/upper1.215 remains a gap. The short's384µsraw point coincides
with4.33%native clock movement; retain it without claiming that gain. An initially
reused original build's materially different native clock justified a contemporary
original measurement. No unchanged candidate is repeated for favorable numbers.

Measurements retain the actual standalone/default-context operation, frozen72,380B
input, exact531,468Bmanaged/native output, prepared live cancellation source, input
preservation and independently owned result/disposal. Native codec/conversion
contexts remain retained as labeled. Setup/JIT are excluded. No new image/settings
matrix or relaxed pixel/reference gate. Five other reader costs reuse unchanged
paths; all125displayed numerical comparisons remain valid. This report supplies
the updated named live control and summary status.

New owned capacity and retained operation scratch are zero. The existing8KiBcoarse
scratch allowance is unchanged; borrowed model users finish before their operation
ends. Total allocation, complete peak live capacity and retained storage remain
separate. Complete physical/admitted peak and native allocation are not newly measured.

Windows/Linux each pass5,735full/live checks (2,488internal/2,299public/948differential)
and39internal/61public per normal/scalar mode. Existing4,096token/cursor cases,
305cancellation ordinals,1,024live-block contexts and164native frame controls retain
their exact gates. No ISA-specific kernel changed; unchanged vector-path ISA proof
is reused. One immutable main/embedded build perOS is used throughout.

The SAME actual private0.2.1Releasepackage and allthreeexamples pass bothOSes.
An added consumer decodes the existing frozen opaque packet through live sync/async
APIs, compares the unchanged ordinary reader, and checks surviving output admission
after input mutation/disposal. Package
`e570f4caf8eaded7845517de5baf2542a1f5213bc077a01e037afd96b1e51a3c`
is418,744B. Published releases remain unchanged. This is development qualification,
not a full125-row actual-package release refresh or282-path completion.

[Evidence](vp8-live-model-preparation-evidence.json) owns the current reader costs,
precise unchanged-path reuse, source projection, all points and qualification.
Raw archive identity is recorded there; dependencies preserve source/module/worker
identities and reproducibility. The current attribution raw is
`vp8-current-live-model-attribution.tar.xz`,221,928B/36members,
SHA256`9ee1e678dfbc2bd19b3476126e494384a769fce619d7d8e16a0b0ae4d749f5c4`,
mirrored/member-verified. [Earlier token preparation](VP8-PREPARED-TOKEN-STATE.md)
retains its440/304µsvalidation/stream gains and adverse history; do not sum gains.

Keep the small raw reduction and fixed-model boundary with normalized uncertainty
explicit. Stop small reader tuning here; select larger independent codec work from
current attribution. Publication remains postponed.
