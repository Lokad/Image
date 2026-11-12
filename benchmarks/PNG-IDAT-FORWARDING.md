# PNG IDAT publication and ownership

Historical stage: [current block Adler sums](PNG-BLOCK-ADLER.md) supersedes
the affected CPU costs. Its format, ownership and non-checksum implementation
changes remain qualified; raw evidence and original boundaries are retained.

The encoder now forwards each already prepared IDAT payload synchronously from
its deflater to the caller output. The private sink no longer copies it through
another4KiB array or retains count/flush/disposal state. Deflate emits full4KiB
buffers and one final tail, so complete packets and chunk boundaries are unchanged.
The original producer owns and admits its buffer until header, payload and CRC
callbacks finish. An output failure abandons this operation-private sink; disposal
never writes or retries. Async encoding retains its existing admitted staging until
the actual asynchronous delivery finishes. No pool, cache, session or API is added.

All17existing complete fresh-state encodes refresh:14pass/1gap/2uncertain.
The two retained-native stored controls, predictable16 and natural64word refresh
separately. No shape/depth/quality grid is added. CPU0, one thread, warm optimized
JIT, workstation nonconcurrent GC, quiet idle-v2 guards, original inputs/settings,
independent output ownership and exact complete packets stay fixed. Every actual
and native bracket is retained, including adverse and uncertain evidence.

| Complete operation / sampling | Lokad / FFmpeg µs | Ratio / conservative upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Fresh Fastest RGBA8 | 4,232.01 / 4,350.13 | 0.973 / 0.992 | 816,780 | pass |
| Fresh Optimal gray8 | 2,035.27 / 1,977.36 | 1.029 / 1.065 | 547,228 | gap |
| Fresh Optimal gray-alpha8 | 4,523.06 / 4,704.58 | 0.961 / 1.003 | 548,196 | uncertain |
| Fresh Optimal gray-alpha16 | 10,789.20 / 10,718.65 | 1.007 / 1.051 | 1,356,764 | uncertain |
| Predictable RGBA16 3000², native retained | 253,653.62 / 402,643.73 | 0.630 / 0.656 | 2,491,852 | pass |
| Fresh stored RGBA8 | 88.62 / 562.79 | 0.157 / 0.174 | 609,708 | pass |
| Fresh stored RGBA16 | 519.81 / 1,457.84 | 0.357 / 0.367 | 1,147,860 | pass |
| Stored RGBA16, native retained, post-GC4 | 528.10 / 433.88 | 1.217 / 1.246 | 1,147,452 | gap |
| Same stored RGBA16, natural-GC64 adjacent | 355.50 / 431.83 | 0.823 / 0.857 | 1,147,393.875 direct | pass |

Fresh-state encodes include codec/options/zlib/wrappers creation and disposal on
both sides; the word reference also allocates/converts/releases its frame. Managed
stream creation/growth/disposal stays timed. Twelve global actuals contain four
managed operations each, with forced GC outside each actual and adaptive50ms
native brackets. The retained-native workers retain codec/frame/wrapper storage;
their two stored and predictable views remain asymmetric. Natural-word uses twelve
alternating64-operation pairs and native brackets, without forced collection.
These views are not summed or subtracted to estimate preparation time. No general
word pass or CPU gain is claimed. Predictable's exact packet is573,041B; stored
byte/word packets remain533,444/1,066,552B. All other frozen packet identities are
in the evidence. Alpha/hidden RGB and managed pixels remain exact.

Storage improves independently of CPU: one4,096B payload and its object/state are
removed. All15nonzero complete encodes allocate4,136B less than their frozen
providers. Stored byte also falls4,136B; word counter scopes differ by408B, so its
post-GC/fresh/natural totals remain separately reported. The obsolete64KiB
transport reservation becomes4KiB coarse minor codec allowance, reducing admission
by61,440B. This is reservation rightsizing, not a61KiB physical saving. Deflater
arrays/reservation remain69,632B/72KiB for stored and384KiB reservation for nonzero.
Retained operation scratch is zero; source/output capacity and borrower ownership
are unchanged. Physical/JIT/native peaks are not remeasured.

There is no uniform timing improvement. Two historical normalized adverse results
prompt only three contemporary unchanged-baseline confirmations, without rebuilds:
Cancelable16 remains passing and its raw/all-actual/normalized intervals overlap.
RGB16 candidate raw time is1,204.14µs/7.76%higher while native rises6.18%; normalized
1.48%adversity is not separated. Gray8 is now a small demonstrated gap; baseline
is uncertain, candidate filtered time rises18.23µs, but all-actual/normalized
intervals overlap. These results do not prove equivalence or a causal regression.
Both gray-alpha controls become uncertain; their historical passing CPU costs are
superseded for the changed sink. Keep them open without favorable retiming.
All other historical/raw clock shifts remain in the JSON, including RGB/word
adverse filtered and all-actual captures. The simpler ownership/storage result is
accepted without inventing a CPU win. Stop the14confirmed fresh passes.

Twelve focused byte/word cases inject cancellation or the original writer exception
at the first IDAT header, payload and CRC. They verify caller prefix, active charges
through every callback, borrowed stream/source/root, cleanup, fresh recovery and
independent survivors. Existing scalar/all-depth/compression/admission/async tests
remain. Windows/Linux each pass6,509full/live checks (3,180internal+2,381public+
948live), plus702internal/246public PNG/admission cases per normal/scalar/
AVX512-off/AVX2-off mode. Unchanged resize/other-family ISA evidence is reused.
The SAMEactual Windows Release package and all three usage examples pass bothOSes.
One core/embedded build perOS is reused throughout. Two public builds correct
stale threshold tests; an actual-package consumer exposes one further old threshold
and is corrected/repeated against the same packed DLL, without repacking. These
initial failures remain archived. No production/native/harness/fixture rebuild is
performed during measurement or qualification. Published packages are unchanged.

The preceding IDC872 full-block fast path retained a tail buffer and had an
inconclusive three-actual word screen. Its sources, immutable DLLs and diagnostics
are retained here; the accepted design instead removes that redundant sink state.
The [stored deflater workspace](PNG-STORED-WORKSPACE.md),
[one-shot reference boundaries](PNG-STORED-ONE-SHOT-BOUNDARIES.md),
[length classification](PNG-LENGTH-CLASSIFICATION.md),
[predictable/conditioning study](PNG-HISTORICAL-CURRENT-CONFIRMATION.md), and
[adjacent Fastest](PNG-FASTEST-ADJACENT-CONFIDENCE.md)/
[gray](PNG-GRAY-ADJACENT-CONFIDENCE.md) reports preserve history and raw evidence.
Their affected CPU costs no longer prove the current sink's costs. Other production
sources, numerical/ISA contracts and source-valid costs are unchanged. All289paths
remain required and publication is postponed.

[Evidence](png-idat-forwarding-evidence.json) pins source/packet/module identities,
complete receipts, reconciliation and package proof. Raw `png-idat-forwarding-qualified.tar.xz`,
SHA256`7412c5dcc4c026fbf4eac35cbc3f480c40114271d2a75148826ab2569c22ad71` (3,128,304B/314members), is mirrored and every
member verified. It includes the baseline/candidate modules, both ownership stages,
originals, helper code, actual package, failed checks and final qualification.
