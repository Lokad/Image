# Asynchronous nonseekable PNG inspection

The current qualified producer has a **1.246× gap** for the previously
unmeasured complete async inspection boundary. This establishes evidence; it
does not change production or claim an independent parity confirmation.

| Fresh nonseekable resident PNG → owned metadata | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| IdentifyAsync, PNG RGBA8 513×259 | 24.10 | 19.34 | 1.246 / 1.571 | 8,408 |

Both sides acquire/release a fresh nonseekable source, privately acquire/grow/copy
encoded storage, read through EOF and perform complete structural/EXIF/CRC
inspection into independently owned six-field metadata. No pixel inflation,
input view, source length/seek or cached descriptor. Resident callbacks complete
immediately on both sides; managed code performs actual ReadAsync calls.
Startup, real I/O, IPC, preparation and checks are excluded. Native parser/context
and released private storage may be reused. Reference: explicitly labeled FFmpeg
nonseekable AVIO/public PNG parser/CRC plus original structural/metadata adapter.
Native allocations are unmeasured. Encoded packet218,775B; descriptor24B.

All21 existing structural/CRC/EXIF controls revalidate on the current producer,
including late CRC, truncation and valid-CRC invalid Deflate accepted without
inflation. Unchanged worker, source/build and input hashes reuse the21 normal/
scalar native controls from `h11-equivalence.tar.gz`
(`f58c256f48a44aaa11d231bcf36c7cf2c33d2d38ff4b29bfd81bd570125a59fe`). Source preservation,
independent live descriptors, actual async EOF, caller left open, resource refusal
and cancellation pass. The setup and cleanup repeat these public boundary gates.

One quiet three-actual short, three native samples before/after, both idle-v2
guards. No long run above one, new image/settings grid, producer/harness/selector/
native build or repeated full/package qualification. The immutable producer and
[current qualification](WARM-LANCZOS3-COEFFICIENT-PERMUTATIONS.md) remain unchanged:
4,448 tests/OS, affected hardware-off checks, same actual private package and all
three examples on Windows/Linux. Existing JPEG/Lanczos runtimes are preserved.

One already-named case moves to numeric: **254 total =232 measured+22 unmeasured**;
70 historical independent passes,161 mean gaps,one confidence-open. Earlier
codec/edit confirmations retain their identified historical builds. Full parity
and replacement readiness remain open. Next: qualify the separately named async
decode boundary with exact pixels and equivalent nonseekable native input.

[Evidence](warm-png-async-identify-evidence.json). Raw `h11b-short-linux.tar.gz`:
`93019dd3699d4a0ea8afd785688a31e31c190f71c330595ae7de108aab0b9f2d`. Producer `c12fbeebea4edb6b4959837183a4b12379756be3139bd46b5e3c1798ff97671d`.
Selector `0075d31dbce00be8712baf02cfa8b40b1767007078e0048d7e98b4d7649c755b`
is reused from its identified original source archive, with extracted bytes retained
in this round. No duplicate full runtime is added.
