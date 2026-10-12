# Warm PNG16 encode: bulk chunk CRC

Replacing eight reflected polynomial steps per payload byte with bulk CRC
reduces the fixed PNG16 encode screen by 60.40%.
The complete encoded PNG remains byte-for-byte unchanged. The independent
twelve-actual repeat is 0.82552x FFmpeg, conservative upper
0.83753x. Both independent quiet guards pass, certifying this fixed pair only.
Uniform warm parity and other material cases remain open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Bitwise chunk CRC baseline | 41640.80 | 2.10245x | 2.73744x | FAIL |
| Bulk chunk CRC, short screen | 16488.73 | 0.83297x | 0.84829x | Pass |
| Independent twelve-sample repeat | 16470.31 | 0.82552x | 0.83753x | Pass |

One original RGBA16 513x259 raster is measured. Together with the retained RGBA8
input, there are two original performance rasters. This round adds one explicit
Encode16 selection; the three-case default and hard three-pair cap remain.
There is no image, shape, compression or codec-strategy sweep.

One production primitive computes chunk CRC across type and payload without
joining or copying buffers. It continues the existing reflected polynomial state
through independently generated tables, safe PCLMUL folding where supported,
eight-byte slicing and scalar tails. Reversing the type-seeded state starts the
normal-polynomial fold correctly. The existing decoder CRC body is unchanged.
No new table, constant, workspace or dependency is introduced; the public PNG
specification supplies the polynomial. No upstream implementation is copied.
WriteChunk is the only changed existing method; ComputeChunk is the only added
method (555 to 556). All other bodies and flags match with metadata resolved.
Chunk order, bytes, compression effort, work, cancellation and error behavior
remain unchanged. The old encoder CRC had no extra cancellation checks.

Fifteen new cases compare four chunk types and unaligned payloads around scalar,
eight-byte, vector and long-tail boundaries against an independent literal
polynomial recurrence. Focused CRC/pixel and public PNG checks pass with intrinsics
enabled and disabled on both OSes (49 and 74 tests respectively). All 2,266
Release tests per OS pass: 548 internal, 822 public API and 896 live differential.
All three examples pass through the same actual dependency-free Release package
on both OSes. Fresh and actual timed production methods/flags match that package.

Both subjects use level 6, filter None and noninterlaced RGBA16. Managed encoding
times a fresh MemoryStream including creation, growth, encoding and disposal;
ToArray is outside timing. Native encoding includes little-to-big-endian sample
conversion and owned packet encoding/acquisition/release. Its prepared big-endian
frame, source, codec context and wrappers are reused. Managed and native PNGs
contain 960,340 and 960,316 IDAT bytes, within the declared 5% effort gate.
Different chunk overheads account for total lengths 960,565 and 963,202 bytes.
Exact inflated rows, every decoded word, alpha, hidden RGB, source and independent
held outputs are verified, including pinned normal/scalar FFmpeg CLI outputs.
The existing native worker and protocol controls are unchanged.

Sequential Release runs use CPU0, one codec/conversion thread, thread type zero,
normal SIMD, tiering off and non-concurrent workstation GC. Native before/after
batches target 50 ms; managed actual iterations target 200 ms. Prepared generated
children validate outside scored windows with one native operation per bracket
and null ratios. The repeat retains twelve actual and
10 filtered managed results. Cooldown scheduling
precedes unchanged five-second quiet guards and cannot certify a failed interval.
Startup, IPC, I/O, generation, hashing and validation are excluded.

The [numeric evidence](warm-png16-encode-crc-evidence.json) retains all managed,
GC/threading and native measurements, guards, identities and immutable archives.
Conservative bounds divide managed confidence upper by the smaller native lower
using the managed critical value. Regenerate with
`python benchmarks/render-warm-png16-encode-crc.py`; add `--check` to verify.
See the [reference recipe](../tools/WarmReference/README.md).
