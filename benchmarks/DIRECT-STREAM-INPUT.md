# Direct complete-input acquisition

One shared synchronous reader and its async counterpart read into admitted input
capacity directly. This removes the8KiB staging copy/async array and the obsolete
final-copy admission gate. Full container/CRC/metadata and decoding contracts stay
unchanged; this is complete input acquisition, not header-only inspection.

Independent quiet-VM confirmation on frozen builds saves2.288µs/11.57%seekable and
2.147µs/10.47%async for the existing218,775-byte513×259PNG Identify packet. Filtered,
all-actual and native-normalized confidence intervals separate. Async allocation
falls8,400→216B/op; seekable stays192B/op. Nine affected companion costs are current
complete-operation observations, without causal claims from older timings.

| Complete operation, one CPU | Lokad µs | FFmpeg µs | Ratio / upper | GC B/op | Status |
|---|---:|---:|---:|---:|---|
| PNG Identify, fresh seekable | 17.492 | 15.921 | 1.099 / 1.103 | 192 | gap |
| PNG Identify, async nonseekable | 18.367 | 19.282 | 0.953 / 0.956 | 216 | pass |
| PNG8 decode, fresh seekable | 1,193.409 | 1,399.372 | 0.853 / 0.859 | 4,568 | pass |
| PNG8 decode/crop/fit | 1,334.927 | 1,480.342 | 0.902 / 0.910 | 701,270 | pass |
| PNG16 decode, fresh seekable | 2,820.595 | 3,094.182 | 0.912 / 0.915 | 8,664 | pass |
| PNG16 decode/crop/fit | 3,259.109 | 2,947.165 | 1.106 / 1.115 | 1,401,346 | gap |
| PNG8 decode, async nonseekable | 1,182.096 | 1,415.052 | 0.835 / 0.839 | 4,592 | pass |
| PNG16 pixel validation, async | 2,785.658 | 3,607.668 | 0.772 / 0.775 | 360 | pass |
| JPEG420 restart decode, fresh seekable | 801.987 | 997.535 | 0.804 / 0.820 | 262,927 | pass |
| VP8L decode, fresh seekable | 2,725.370 | 3,031.581 | 0.899 / 0.908 | 152,824 | pass |
| VP8 + alpha decode, fresh seekable | 5,809.554 | 6,000.485 | 0.968 / 0.973 | 240,496 | pass |

The existing native workers/inputs/settings and complete output ownership remain.
Native stream wrappers/packet storage acquisition, EOF, parser/CRC, decoding and
output release are included as declared by each boundary. Native codec contexts
may remain prepared; independently owned input/output payloads are reacquired.
JPEG uses its existing frozen reference view; its initial unfrozen setup failed
before measurement. One async collector schema mismatch was recovered from its
completed capture without resampling. Both diagnostics remain in raw evidence.

Storage starts at up to256bytes and grows geometrically, clamped by input/array
bounds. A one-byte read checks EOF before replacing a full buffer. Every substantial
allocation is admitted first, including old/new overlap. Actual async users finish
before release on cancellation/failure. Refusal is terminal. Streams/root budgets
stay borrowed; returned images retain independent charges until disposal.

For the Identify control, retained input is262,144B and growth peak393,216B. Decode/
async acquisition previously needed401,408B including scratch. Async's optional
one-byte probe allocates32B; removing the8,216B scratch object saves8,184B/op.
The unchanged default pool may retain7,936additional idle bytes for this workload's
small growth buckets. Governed calls bypass it. Total allocation, admitted peak and
idle retention are separate; physical/native/JIT/RSS peaks were not measured.

Windows/Linux each pass6,530full/live checks and157internal/429public focused checks
in normal/scalar modes. Unchanged kernel ISA evidence is reused. The same actual
Release package and all three usage examples pass both platforms, including exact
capacity, replacement refusal, cleanup and subsequent reuse checks. Resident span
paths, encoders, editing/resizing and text/drawing are unchanged.

[Evidence](direct-stream-input-evidence.json) records all samples, source identities,
ownership and boundaries. Nine passing controls stop. Seekable Identify and word
crop/fit retain their measured gaps. The full289-path objective remains open.
