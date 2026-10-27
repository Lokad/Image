# Shared Adler-32 for PNG encoding

PNG16 SmallestSize independently measures 26.83 ms, 1.336× FFmpeg (upper 1.360×),
with the exact complete 963,202 B packet and unchanged 3,813,679 B/op. All twelve
actual/filtered samples and both native brackets remain. Its preceding short was
28.45 ms; separate jobs are not a paired speedup claim. Full warm parity is open.

| Existing quiet one-CPU control | Lokad µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: |
| RGBA16 SmallestSize short | 27,349.07 | 1.351 / 2.396 | 3,813,679 | 963,202 |
| RGBA16 SmallestSize independent | 26,830.88 | 1.336 / 1.360 | 3,813,679 | 963,202 |
| RGBA8 SmallestSize short | 24,339.75 | 1.396 / 1.938 | 1,325,670 | 210,973 |
| RGBA8 Fastest short | 7,950.16 | 1.760 / 2.620 | 1,344,453 | 236,828 |
| RGBA16 NoCompression short | 1,158.17 | 2.754 / 2.935 | 1,741,683 | 1,066,552 |

The two encoder checksum loops now call the existing shared `Adler32.Append`.
Its code, SIMD weighted sums, bounded 4 KiB reductions and portable fallback are
unchanged. The [RFC 1950 checksum recurrence](https://www.rfc-editor.org/rfc/rfc1950.html#section-2.2)
remains exact. Direct comparisons cover 459,712 seed/boundary/split cases with
SIMD enabled and disabled. No new production kernel, storage, cache, owner or
admission mechanism. Cancellation is forwarded; failure cleanup still cannot flush.

Both systems pass 4,959 Release checks (1,977 internal, 2,035 public, 947 live),
499 internal and 180 public scalar PNG checks, including the fourteen frozen
complete native PNG packets and synchronous/asynchronous ownership. Existing
checksum, all-policy chunking, refusal/recovery and cancellation/writer-failure
tests are reused. The same actual private 0.2.0 package and three usage examples
pass both systems. Published 0.1.0 is unchanged.

One production build per OS serves the round; workers, harness and recovered
selectors are reused. Existing complete normalization, compression, fresh output
ownership and release stay timed. No new image/settings grid or profile.
[Evidence](warm-png-shared-adler-evidence.json) retains samples, producer/package
identities and the mirrored, member-verified raw master. Other PNG encode costs
are historical; unchanged decoders, other codecs and editing/text retain evidence.

The newly measured NoCompression gap is the largest current observed multiple.
Prioritize bulk stored-payload emission next, preserving row/block choice,
bit alignment, 4 KiB flush/failure boundaries, exact packets and ownership.
