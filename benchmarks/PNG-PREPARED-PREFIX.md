# Prepared PNG literal prefixes

PNG framing prepares ten-bit literal prefixes and stores validated code lengths
as bytes. The inflate cursor resolves those prefixes without repeated canonical
reversal/search or state publication. Nine-bit literal-run paths stop at wider
entries. Deeper/incomplete prefixes retain the active general canonical fallback.
No second obsolete implementation, cache, pool, session or public API is added.

Keep the initial nine-bit peek and the original maximum-depth lookahead on its
miss, even when ten bits suffice. Unknown tail bits cannot become zero-padded
codes. Scalar/live polling, exact partial output and bit-cache state, checksum,
container/filter errors and all owned admission remain unchanged. This changes
preparation/representation, not pixel precision or encoded packets.

| Complete two-image shared-root operation | Managed µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Unchanged original confirmation, reused | 3,292.45 | 2,846.74 | 1.157 / 1.168 | 1,637,520 |
| Current independent confirmation | 2,870.80 | 2,845.40 | 1.009 / 1.014 | 1,638,214 |

The independent saving is **421.65 µs / 12.81% raw**, 12.77% native-normalized;
both filtered/all-actual confidence views separate. Native timing moves −0.047%.
The three-actual screen measures 2,852.10 µs; every point is retained, with no
unchanged favorable repeat. The current lower bound 1.0029 still establishes a
small parity gap. There is no broader PNG or whole-library speed/pass claim.

Both subjects use the same frozen 218,775-byte packet and exact 1,062,936-byte
RGBA pair including hidden RGB. First output survives the second complete decode;
both releases, normalization, table construction, admission and storage remain
timed. The borrowed 32 MiB root/input reservation is prepared outside timing.
FFmpeg retains declared decoder/packet/frame wrappers and has no shared-budget
API. Frozen source/worker/settings stay exact in the prepared isolated workspace.
The older driver groups four complete operations per actual, the current adaptive
driver 80; each is normalized once. Source-valid original timing is reused.

Nominal nested array payload is **4,056→4,063 bytes**, within the unchanged 4 KiB
admission: wider lookup +1,024 bytes, byte code lengths −1,017 bytes. No production
heap payload or retained workspace is added. Measured total GC rises **694 bytes /
0.0424%**; do not call it unchanged. Current MemoryDiagnoser has 34 Gen2 collections
over 80 operations, versus zero over four originally. Its extra 55,488 bytes equals
1,632×34; that is a diagnostic lead, not a proven cause. Coarse admitted peak stays
1,766,641 bytes, including caller input and two retained 531,468-byte results.
Physical peak and native allocation are not newly measured. These quantities differ.

Windows/Linux each pass **5,751 full/live checks** (2,495 internal / 2,308 public /
948 differential), with 704 internal / 246 public normal/scalar coverage. The new
actual prepared/cursor oracle compares against the still-used general canonical
decoder: ten/fifteen-bit codes, maximum-depth lookahead, every tail cut, all eight
phases, incomplete/unassigned paths, output overflow and exact partial cache/output.
Existing independent zlib/full-frame/refusal/cancellation/async/owner checks remain.
No new ISA operation/guard; unchanged PNG vector/filter/CRC disabled-ISA proof is
reused. One immutable main per OS is reused throughout. Test-only additions require
a second embedded build; no public/harness/native/selector/fixture build occurs.

The SAME actual private Release package/all three usage examples pass both OSes.
Package `98c5409fb690ca9febc9ded679267ca45ca84202669b0141f8ec0a0be707f08e`
is 419,349 bytes. Windows main
`84c68084fb76ed0e900456dfcbfffcb370c62eea8f5ef26d8f185bb0ba4638fe`;
Linux main `05f302755073e4711cdecd7158ce5aa37165bcbc2bf0fe6f9c67eaedf825befb`.
Published packages stay immutable; publication is postponed.

This shared framing/preparation change invalidates other PNG decode/coding costs,
including stored callers of the changed workspace method. Seven other displayed
costs are explicitly historical pending confirmation: live RGBA8, tight RGBA8/16,
ValidatePng8/16, indexed Adam7 and async ValidatePng16. Those are the next bounded
refresh; no new workload grid. Encoding and header-only Identify/Inspect/Probe are
unchanged. One displayed cost refreshes, seven become H, 117 reuse unchanged paths.
Full 283-path parity and a full actual-package release refresh remain open.

[Evidence](png-prepared-prefix-evidence.json) records exact source/cursor/storage
proof, all observations, scope and same-package qualification. Raw
`png-prepared-prefix-qualified.tar.xz`: 3,621,012 bytes / 118 members, SHA256
`3a57e9ffd77680fe3dda7a121bbd4fc41ed7ca83756828eccdc129543ed3a699`,
mirrored and member-verified with pinned dependencies. The benchmark-time source
manifest preceding the test addition is reconstructed byte-exactly and retained;
no sampling is replayed. [Attribution](PNG-RETAINED-ROOT-CURRENT-ATTRIBUTION.md) and
[original cost](CURRENT-RETAINED-PNG-DECODE-COST.md) retain prior evidence.
