# Lanczos word product reuse

Accepted LZB916D shares one paired-output schedule across byte and word reduction. Word
execution expands exact source products once per needed row pair, then reuses
them across overlapping horizontal windows. The original UInt32 producer remains
for suffixes and geometries where an output loan is unavailable. No public API,
owner, pool, plan field or retained capacity is added.

| Existing 513×259 → 256×129 operation / zimg | Managed / native µs | Ratio / upper | GC B/op | Retained plan B |
| --- | ---: | ---: | ---: | ---: |
| Prepared RGBA16 | 485.36 / 512.07 | 0.948 / 0.958 | 138 | 156,248 |
| One-shot RGBA16 | 661.63 / 1,809.24 | 0.366 / 0.382 | 155,078 | 0 |
| Prepared RGBA8 | 462.40 / 488.52 | 0.947 / 0.953 | 138 | 139,800 |
| One-shot RGBA8 | 596.90 / 1,576.30 | 0.379 / 0.382 | 138,647 | 0 |

All four pass and stop. The prepared-word independent comparison saves
**74.63 µs / 13.33% raw** and **6.18% reference-normalized**, with separated
confidence intervals. The native clock moved **−7.62%**; retaining every actual
gives 74.18 µs / 13.25% raw, also separated. Companion rows qualify current costs,
not causal gains. One additional read-only confirmation of unchanged prepared word
enlargement remains uncertain: 3,989.78/native3,911.18 µs, ratio1.020/upper1.035,
4,265,332 B/op and333,396 B retained plan. It adds no speedup or gap claim and is
not repeated for a favorable bound. Other three enlargement controls reuse their
source-valid evidence. Current Lanczos scope: **seven passes / one word-enlargement uncertainty**;
the full **289-path objective remains open**.

The byte one-shot snapshot is 5.43 µs /0.92% above the prior report, with separated
raw intervals, while prepared byte intervals overlap. These historical snapshots
are not a contemporary paired-DLL causal proof. Keep that small adverse difference
visible; the current complete byte boundaries remain confirmed passes.

Prepared calls include fresh source reads, row-ID reset, exact filtering,
certification/repair, independent output and disposal. Their caller-owned plans
are prepared outside timing on both sides. One-shot calls include preparation
and release on both sides. The explicitly labeled **zimg 3.0.6 FFmpeg scaler
backend** includes packed/planar normalization, halo, filtering, unpremultiplication,
quantization, fresh output and disposal. Its retained graph/input capacity is
unmeasured; scratch is 367,168 B. This is not FFmpeg CLI performance. Native float
RGB/alpha error ≤1 and RMSE ≤0.25 stay separate from exact managed Q30/full-word
pixels. Inputs, output hashes, settings and operation boundaries remain frozen.

The existing 16,416-byte two-row UInt32 plan array holds one exact double product
row. Another 16,416 bytes borrow unpublished output before the active 8,192-byte
second sum. On the frozen word control the temporary region begins at row116;
its users retire before the current pair is published, including outputs116/117.
Later pairs cannot overwrite completed loan rows. The ordinary producer and tail
materialize the rest. Charges remain attached to the original plan/result owners.
Prepared word plan plus one independent result is **420,440 B**; the 1,062,936-byte
caller input is separate. No owned/admitted/retained capacity decreases. Reported
138–139 B/op does not establish an intrinsic one-byte saving. Physical/native/JIT
peaks are unmeasured.

Every UInt32 product converts exactly to binary64. Both producers keep the
original four horizontal FMA chains, reductions and tails; the ascending vertical
union preserves each output's nonzero-tap order. Certificates, exact repair,
scalar/long-support paths, admission, terminal refusal, cancellation and independent
survivors remain. Source reconstruction verifies all other production bodies;
actual byte costs are refreshed rather than inferred from constant folding.

The first paired-output stage was inconclusive: 509.80/native510.64 µs,
upper1.026, despite separated raw gain. Its normalized gain was only a1.18% point
lead with overlapping intervals. No favorable repeat or broad qualification was
run for it. A two-second profile located paired horizontal filtering at1,115 of
1,995 sampled locations; those are leads, not phase costs or removable shares.
The revised preparation differs from the earlier
[single-row conversion experiment](LANCZOS-SOURCE-CONVERSION-EXPERIMENT.md): it
shares coefficients across two rows and uses a separate output loan while partial
sums remain live. The original generic staging/prewarm implementation is not replayed.

One bounded short and one independent confirmation qualify the revision; the
same immutable prior producer supplies the baseline. Each affected companion has
one twelve-actual current-cost capture, plus that one read-only enlargement check.
CPU0, sequential Release, idle-v2 guards,
all actuals and adverse/inconclusive views remain. An initial staging-identity
assertion and writable-span compile error precede timing and are retained.
No worker, selector, harness, fixture or benchmark matrix expansion occurs.

Focused normal/scalar checks cover exact integer frames, scalar-FMA bits and
BigInteger envelopes, guards, producer allocation, minimum/straddling loan extents,
root refusal, cancellation and reuse. Both OSes pass **6,573 full/live checks**,
focused **152 internal / 15 public normal/scalar checks**, the three named
resize-storage suites and the **same actual timed-DLL package consumer / three
usage examples**, including frozen word pixels and surviving owners.
[Evidence](lanczos-word-product-reuse-evidence.json) records source identities and mirrored/member-checked
raw archive retain reproduction and the earlier stage. Published releases are
immutable; publication remains postponed.

[Current adjacent enlargement confidence](LANCZOS-ENLARGEMENT-CURRENT-CONFIDENCE.md)
supersedes only the earlier word-enlargement uncertainty above. It passes on the
current actual-package DLL; all eight declared Lanczos comparisons now pass and
stop. The earlier costs remain history, without a causal speedup claim.
