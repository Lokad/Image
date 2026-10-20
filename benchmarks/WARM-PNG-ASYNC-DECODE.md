# Asynchronous nonseekable PNG decoding

This baseline is superseded by the
[materialized Deflate cursor](WARM-DEFLATE-MATERIALIZED-CURSOR.md); the original
native controls and complete async boundary are retained.

The current qualified producer has a **1.450× gap** for the already-named
complete async decode boundary. Production is unchanged; this is a short screen,
with no independent parity confirmation claimed.

| Fresh nonseekable resident PNG → owned RGBA8 | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| DecodeAsync, PNG RGBA8 513×259 | 2,049.34 | 1,412.89 | 1.450 / 1.683 | 12,784 |

Both sides include fresh nonseekable source acquisition, bounded private encoded
storage growth/copies/EOF, complete container/EXIF/CRC validation, full pixel
decode into independent RGBA8 and all input/output release. Managed code performs
actual ReadAsync calls. Native reference is explicitly **FFmpeg nonseekable AVIO
and PNG decoder plus the original structural/CRC compatibility adapter**. Its
acquired padded packet is handed off without a final full-input copy. Neither
side exposes source length, seeking or an encoded input view. Callbacks complete
immediately on both sides; no forced scheduling or real I/O. Native wrappers and
released private storage may be reused; pixels/content are not cached. Native
allocations are unmeasured. PNG218,775B; RGBA531,468B.

All21 existing original container controls agree, including late CRC, truncation,
EXIF orientation, invalid Deflate and mismatched raster width. Only original and
orientation6 are valid complete images; exact pixels/metadata agree in normal
and scalar workers. Normal/scalar public CLI pixels are also exact. This differs
from Identify, which deliberately accepts valid-CRC invalid compressed content.
No new image, shape, quality or scheduling grid is introduced.

The first native adapter returned a partial frame while emitting a decoder
error. It was rejected before timing. The public FFmpeg error-severity callback
now makes such an operation fail, without parsing diagnostic text. Full
structural/CRC validation remains timed. The initial bytes/control failure are
retained in `h12-native-first-attempt.tar.gz`
(`3c70ea475f3d32d53871a25ccabce7007ed05563cfebe036980c6a56cd6adb79`). No invalid reference is scored.

Setup and cleanup check exact alpha/hidden RGB, unchanged borrowed source,
caller stream left open, independent simultaneous and surviving outputs,
input/memory/work refusal, cancellation, late-error cleanup and subsequent
successful decoding. Existing cooperative-memory and deferred-async ownership
qualification is reused because production is unchanged.

One quiet three-actual short, three native samples before/after and both idle-v2
guards. No long confirmation above parity. One harness build/OS, two native
builds to establish the reference, no production or selector build. The archived
typed StreamDecode selector and unchanged runtime dependencies are reused in an
isolated runtime; earlier prepared runtimes remain intact. The executable harness
payload lives with its runtime configuration, with a frozen alias; no second
payload copy or rebuild is needed for probes.

Reuse [current qualification](WARM-LANCZOS3-COEFFICIENT-PERMUTATIONS.md):4,448 tests/OS,
affected hardware-off controls, same actual private package and all three examples
on Windows/Linux. New tooling builds pass on both OS; the dedicated Linux VM
supplies all21 controls and timing. No full/package rerun for unchanged production.
The numeric checklist becomes **254 total =233 measured+21 unmeasured**:
70 historical independent passes,162 gaps,one confidence-open. Older confirmations
retain their identified builds; full parity/replacement readiness remain open.

[Evidence](warm-png-async-decode-evidence.json). Raw `h12-short-linux.tar.gz`:
`ad7cabbd707afbb6d77a8bbaac642ff6980f4b814e01e9fe7101d5ca889f67cc`. Windows tooling `h12-tooling-windows.tar.gz`:
`cc98aa7d051247e37c919c3eda616112a05abc8b05e4264dda5a4db97d47ff24`. Producer
`c12fbeebea4edb6b4959837183a4b12379756be3139bd46b5e3c1798ff97671d`; Linux harness
`23640f6e89b37574dbb5b281346178e23db6615661f2412c9b8a7424a094ff68`. Old diagnostics were losslessly compressed
with round-trip hashes/modes before building; duplicate host dependency copies
were pruned after equality checks. Both checkouts remain below5,000,000,000B.
