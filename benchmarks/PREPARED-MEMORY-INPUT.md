# Prepared complete stream input

[Current PNG16 stream confidence](PNG-STREAM-WORD-CURRENT-CONFIDENCE.md) supersedes
only the earlier standalone PNG16 decode uncertainty below. The original capture
and inconclusive two-DLL comparison remain history. The complete operation now
passes on the current actual-package DLL under alternating managed/native batches;
this is a confidence update, not a production speedup. Other costs below retain
their identified scope.

Complete PNG `Identify(Stream)` independently saves **2.75 µs / 15.40% raw**,
**15.01% reference-normalized**, against a contemporary 17.829 µs baseline.
Filtered and all-actual raw confidence separate, as does normalized confidence;
the native clock moves -0.46%. This is the original 218,775 B, 513×259 input,
complete container/CRC/nine-field inspection, without pixel inflation.

| Existing complete operation, one CPU | Lokad / reference µs | Upper / status | GC B/op |
| --- | ---: | ---: | ---: |
| PNG Identify, fresh seekable / FFmpeg metadata adapter | 15.083 / 15.882 | 0.953 / pass | 192 |
| PNG8 stream decode | 1,188.348 / 1,394.836 | 0.858 / pass | 4,568 |
| PNG8 stream decode/crop/fit | 1,257.220 / 1,478.183 | 0.857 / pass | 169,574 |
| PNG16 stream decode | 3,180.060 / 3,084.967 | 1.121 / uncertain | 8,664 |
| PNG16 stream decode/crop/fit | 2,926.976 / 2,945.113 | 1.002 / uncertain | 338,268 |
| JPEG420 restart stream decode | 801.486 / 991.575 | 0.812 / pass | 262,927 |
| VP8L stream decode | 2,805.412 / 3,016.872 | 0.935 / pass | 152,824 |
| VP8 + alpha stream decode | 6,147.316 / 6,261.604 | 1.004 / uncertain | 240,496 |

Ordinary synchronous `MemoryStream` inputs with 257 B–4 MiB remaining now prepare
one power-of-two owned capacity, clamped by input/array limits. The existing pool
ceiling bounds this optimization. No caller backing storage is borrowed and real
reads, EOF and excess-input probes remain mandatory. Derived/custom streams,
larger inputs and all async acquisition retain their original incremental behavior.
Reserve before allocation, terminal refusal, cancellation cleanup and independent
result lifetimes remain. No new cache, owner, field, public API or budget.

For the Identify control, **11 → 1 capacity acquisitions**, **261,888 → 0 growth-copy
bytes**, and **393,216 → 262,144 B acquisition peak**. Final capacity stays 262,144 B;
GC stays **192 B/op**. Shared-root calls bypass the unchanged default input pool
(largest bucket 4 MiB, ceiling about 8 MiB). No actual idle-retention saving, universal
complete-decode peak reduction or physical/native/JIT peak is claimed. Existing
pixel-result ownership and the separate two-slot / 4 MiB image pool are unchanged.

The PNG16 standalone is bimodal and uncertain, **3,180.06 / 3,084.97 µs**, interval
.941–1.121. One contemporary baseline is 2,906.53 µs with overlapping normalized
confidence. One controlled alternating immutable-DLL pair gives **2,834.26 →
2,828.93 µs**, ratio interval .944–1.057: inconclusive, with no demonstrated
regression or speedup. These paired costs do not replace the standalone comparison.
VP8 stream is now uncertain, upper 1.004; word PNG composition stays uncertain.
All original adverse samples remain. No favorable standalone repeat follows.

Timing includes fresh non-exposed input streams, owned input acquisition/copies,
EOF, parsing or full decoding/composition, independent outputs and disposal.
FFmpeg retains the existing codec/parser context; its fresh AVIO/input/output owners
and release remain timed. Identify uses the explicitly labeled complete public PNG
parser plus CRC/metadata adapter. Original 21 controls, native settings/identities,
exact managed pixels/metadata/packets and output independence remain unchanged.
One three-actual screen precedes independent 12-actual primary confirmations;
seven existing synchronous companion costs refresh once, on the quiet CPU0 VM
with idle-v2 guards. No new images, settings or material paths.

Windows/Linux each pass **6,614 full/live checks**: 3,248 internal / 2,418 public /
948 differential. Focused **28 internal / 153 public** pass normal/scalar on each OS.
Five new internal and four public cases cover owned storage, derived read failures,
relative cursors, bounded large inputs, EOF, refusal/cancellation cleanup and recovery.
The **same actual Release package**, containing the timed Linux DLL, passes both
OS consumers and all three examples, including the new preparation checks.
One core/embedded/public build per OS; one untimed-gate harness build and one
diagnostic-only pair-driver build. No production rebuild after timing. Valid async,
resident, encoder, edit/resize/font/text and kernel ISA evidence is reused.

Current image views: **173 pass / 3 gaps / 15 uncertain**, 191 existing comparisons.
This Identify pass is outside those views. All **289 material paths** remain required.
Publication is postponed; published packages stay immutable.

[Evidence](prepared-memory-input-evidence.json) owns current costs, identities,
boundaries and qualification. Raw `prepared-memory-input-qualified.tar.xz`: 8,844,792 B /
263 members, SHA256 `5dd5497bcac99999a01af73db6e1508f277edcd068d789b34df455fd0a6ceaf4`, mirrored and member-verified.
Dependencies preserve original fixtures, reference tools, earlier proofs and package.
