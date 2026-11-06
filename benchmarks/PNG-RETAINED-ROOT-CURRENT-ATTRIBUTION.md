# Shared-root PNG decoding attribution

The existing two-retained-output PNG8 control remains a demonstrated gap:
3,292.45 µs versus FFmpeg 2,846.74 µs, ratio 1.157 / upper 1.168,
1,637,520 allocated bytes per complete two-decode operation. Relevant current
Scan/Decode/row helpers, rolling inflater, Deflate cursor, checksums and admission
paths are unchanged from that confirmation. Reuse its timing; no new CPU claim.

One bounded two-second capture on qualified `e69516f7` contains 1,885 process
samples: 1,208 in InflateCursor, 263 in DecodeLongLiteral and 96 in canonical
Decode. Adler/CRC have 26/21 and row Consume two. Samples and instruction skid
identify locations, not component times or projected savings. Preparation/root
accounting is not the dominant sampled location. Instrumented elapsed is discarded.

An untimed independent RFC1951 parser reconstructs all 531,727 filtered bytes
exactly against zlib, consuming the complete original Deflate stream. Across its
eight blocks, 28,816 literal/length tokens exceed nine bits; 24,024 use exactly ten.
This supports testing bounded prepared prefix decoding, including preparation in
complete operation costs. It does not establish a gain or justify fixture caching.

The candidate design would replace repeated long-code reversal/canonical search
and state publication with a prepared ten-bit lookup. Preserve the original full
maximum-depth lookahead on a root miss, exact bit/cursor/error/cancellation behavior
and canonical incomplete/short fallbacks. Byte-sized validated code-length storage
could offset the larger table within the current 4 KiB bound: nominal nested array
payload 4,056 + 1,024 − 1,017 = 4,063 bytes. Actual layout and affected callers still
need qualification. Counts alone establish neither correctness nor a speed gain.

The capture retains the frozen 218,775-byte packet, exact hidden RGB, first output
through the second decode, independent surviving owners, borrowed shared root,
terminal refusal/cancellation cleanup and 1,766,641-byte admitted peak. Both complete
decodes, preparation, allocations and releases remain in the workload. Native
wrappers retain their declared preparation; FFmpeg has no matching shared-budget
API. No new image, setting, harness/native/production build or statistical repeat.

Two preflights failed before timing/capture: the old retained worker's shared source
identity differs after unrelated encoder changes; an isolated root then required
the existing ImageProbe dependency. Logs remain retained. The successful workspace
uses the exact frozen worker/source/identity and cached adaptive complete-operation
driver with the current immutable DLL. Current source, worker and metadata remain
unchanged. All original native/pixel/ownership gates and idle-v2 guards pass.

[Evidence](png-retained-root-current-attribution-evidence.json) reuses the current
[block-state qualification](VP8-BLOCK-STATE-RESERVOIR.md): 5,750 full/live checks per
OS, affected scalar checks and the same actual package/all three examples. CI
42287140890 is green on all three jobs. No repeated package proof is needed for
this unchanged module. The [retained cost report](CURRENT-RETAINED-PNG-DECODE-COST.md)
preserves its original points and boundaries. Displayed numbers remain unchanged.

Raw `png-retained-root-current-attribution.tar.xz`: 331,936 bytes / 49 members,
SHA256 `5f30795b21030f904942a4f0b2679283a241ebfb8f4e7dda34fe70dfb1be7ced`,
mirrored and member-verified with pinned hash-addressed dependencies. Publication
and all 283 required warm paths remain open.
