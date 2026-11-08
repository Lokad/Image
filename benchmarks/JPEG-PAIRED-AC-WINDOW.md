# Paired JPEG AC windows

Twelve completed-plane costs are superseded by
[completed output storage](JPEG-COMPLETED-OUTPUT.md). Seventeen unchanged controls
and the paired gains below remain valid.

JGA556–JPE562, 2026-11-08. Accepted scoped decoder improvement; all 283 required
paths and the warm parity objective remain open.
[Evidence](jpeg-paired-ac-window-evidence.json) owns all 29 current JPEG read costs.

Two complete AC fields share one 12-bit descriptor lookup and reservoir advance.
The encoded canonical tables produce 4,096 UInt32 entries per used AC table.
Descriptors contain signed amplitudes and positions; zero second amplitude means
EOB. The second position is checked before either store, including the case where
coefficient 63 must finish without consuming a following EOB. Unsupported pairs,
long codes, stuffing, tails and syntax failures retain the original traversal.
Live and progressive entropy kernels, DC setup, normal cursor publication, inverse
transforms and pixel contracts remain unchanged.

Preparation belongs to a single ordinary pixel scan and stays inside timing.
Scans need at least 64 MCUs and sufficient existing local working allowance;
selection precedes shared admission. Each 16,408-byte capacity is reserved before
allocation. Shared refusal terminates without retry or lower-memory fallback.
Canonical tables that cannot contain any pair allocate nothing. A scan `finally`
clears and releases the tables after their actual users, including failure.
There is no new pool, session, cache or independent allowance.

| Frozen complete decode | Original → current µs | Reference µs | Current upper | GC B/op, original → current |
| --- | ---: | ---: | ---: | ---: |
| RGB444 | 1,644.16 → 1,530.34 | 1,450.52 | 1.065 | 10,088 → 42,936 |
| Sparse Adobe CMYK | 527.63 → 511.72 | 436.56 | 1.178 | 4,520 → 20,944 |
| Owned grayscale | 651.46 → 679.83 | 679.12 | 1.009 | 672,202 → 672,218 |
| Library-encoded JPEG8 | 1,589.10 → 1,642.49 | 1,896.25 | 0.873 | 4,304 → 4,320 |

Independent RGB confirmation saves **113.83 µs / 6.92% raw / 6.75% normalized**;
sparse CMYK saves **15.91 µs / 3.02% raw / 2.64% normalized**. Filtered/all-actual
raw and normalized intervals separate for both. Native clocks move −0.184% and
−0.390%. Inputs, settings, exact managed samples and declared native bounds,
complete preparation/decode/conversion/output/disposal and one CPU are preserved.
Native codec/input/frame wrappers retain their documented contexts; this is not
fresh-context equivalence.

All 29 costs refresh because the Huffman object gains one pointer, including on
progressive/unprepared paths: **14 pass, 14 gap, 1 uncertain**. Native444 now passes;
caller CMYK is a demonstrated gap. Owned grayscale moved from passing to uncertain,
and JPEG8 has an adverse point while remaining passing. These results remain
visible; no independent causal regression or broad JPEG nonregression claim is
made. Resolve grayscale uncertainty before assuming it needs optimization.

RGB's measured admitted peak rises **541,253 → 574,069 B** (+32,816 B, 6.06%). Its
retained output stays 531,468 B; live-token peak remains 541,253 B. Default idle
pixel-pool capacity stays 531,468 B within the existing 4 MiB cap; governed calls
bypass that pool. Pair scratch retained after the scan is zero. GC allocation is
above, separate from admitted live capacity; physical/native peaks are unmeasured.
The existing generic capacity adapter is identified by actual JPEG input/module
hashes; its old VP8 banner is not a workload description.

The independent canonical model verifies 6,630 blocks, 1,105 MCUs and final
padding/EOI on the original RGB input. A 12-bit pair can cover 90.16% of its AC
symbols. UInt32 descriptors pass 516,096 independent prefix/position checks. A
bounded 10-bit revision retains 80.43% coverage and quarters table storage, but its
RGB screen gives up most of the CPU lead. Both source/module snapshots, all
adverse results and compact sparse-CMYK gains remain as unselected evidence.
Eligibility and short screens alone establish no CPU gain. Final eligibility
checks exclude provably empty plans before allocation.

Windows/Linux each pass **5,883 full/live checks**, plus 407 internal / 375 public
checks in each normal/scalar mode. Four new facts cover actual prepared prefixes,
blocks/following reads/malformed tails, admission/cancellation recovery, and empty
plans. The SAME actual Release package/all three examples pass both OSes. A new
original T.81 RGB fixture freezes pre-change pixels and verifies actual grouped
decoding, sync/async ownership, terminal aggregate refusal and independent survivors.

One immutable final build per OS serves current measurements and qualification.
The first completed batch measurement was recovered after a script initialization
error, with all original actuals and guards reused; it was not rerun. Metadata was
restored exactly. Postprocessing corrections did not repeat measurements. No new
performance fixture, worker, harness, selector or public-test assembly was built.

Private 0.2.1 package SHA-256:
`d09babd7ae99bf50a0e906549d34535d47ef2a0f6df167bf33ef5892ab87d178`
(429,278 B). Publication is postponed; published packages remain unchanged.
The ignored `jpeg-paired-ac-window-qualified.tar.xz` is mirrored and member-verified;
its identity and dependencies are in the evidence. Source snapshots, models,
actuals, commands, guards, storage observations and actual package are retained.

Next prioritize the larger live opaque VP8 and mixed-word resizing gaps. Preserve
the grayscale uncertainty and adverse points; do not tune passing cases or replay
table widths to seek favorable samples.
