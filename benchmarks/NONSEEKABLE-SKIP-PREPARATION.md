# Nonseekable skip preparation

The complete immediate-async JPEG dimension probe improves **10.76%** in a controlled
comparison. Already-completed payload reads stay in a direct loop; only a genuinely
pending/faulted/canceled read enters the awaited continuation. This follows the
existing header-read pattern. Parser/marker grammar, seekable skipping, acquisition,
polls, admission and disposal remain unchanged. No new storage, cache or public API.

| Existing complete operation, scoped DLL pair | Prior → current µs | Ratio interval | GC B/op |
| --- | ---: | ---: | ---: |
| Immediate-async JPEG dimensions | 0.334 → 0.298 | 0.857088–0.929125 | 632 |
| Seekable complete JPEG headers, unchanged branch | 6.014 → 6.049 | 0.999088–1.012529 | 8,800 |

The first is an independent twelve-pair confirmation after the short screen,
262,144 complete public calls per subject. The header guard retains twelve
16,384-call pairs after 4,096 warmups. Every actual, natural GC, quiet CPU0, original
65163-byte JPEG, source/prefix/complete descriptor and independent-output check
remains. The header guard is inconclusive around a 0.6% adverse point: no header
gain, causal regression or universal non-regression claim. Its unchanged seekable
branch is distinct from the target. No timing is repeated for a favorable result.

The same immutable DLL's standalone confirmation is **0.299 /
0.206 µs**, conservative interval **1.391118–1.508613**:
still a gap to the explicitly labeled **FFmpeg AVIO + bounded header adapter**,
not FFmpeg's public codec parser. Immediate resident callbacks, fresh input
adapter, independent descriptor and cleanup are timed on one CPU. This is not a
general asynchronous-I/O or header performance claim. Scoped gains are not added
to [earlier marker preparation](JPEG-MARKER-PREPARATION.md).

Allocations remain **632 B/op**; dimension admission **512 B**, actual scratch
**256 B**, borrowed encoded input **65,163 B**. Retained operation scratch is zero;
returned metadata remains independently usable and is counted in allocation.
No new owned capacity, idle retention or shared-budget allowance. Full-header
admission stays at most **8,448 B**. Physical/native/JIT peaks remain unmeasured.

Four authored APP2 controls suspend a nonseekable drain at its first read or
after five successful bytes. Cancellation and read failure keep storage charged
until the actual uncooperative read finishes, then prove cleanup, borrowed-root/
input survival and subsequent recovery. Mixed immediate/pending one-byte reads,
limits, metadata, truncation and full inspection coverage remain.

Both Windows and Linux pass **6,700 full/live checks** (3,263 internal/2,489 public/
948 live), plus focused **2 internal/147 public** normal/scalar checks. The **same
actual Release package and all three usage examples pass both OSes**, including
four external-consumer suspended-payload cleanup/recovery cases. One core/embedded/
public build per OS, no harness/native/helper/fixture builds, one NoBuild pack and
no post-timing producer rebuild. Unchanged arithmetic/ISA evidence is reused.

Only the displayed immediate-async JPEG probe executes the changed skip path.
Other three probe costs, seekable headers, codecs, editing, resizing and complete
text retain source-valid evidence on their original producers. This is not a
release-wide refresh. All 289 material paths remain required; publication is postponed.

[Evidence](nonseekable-skip-preparation-evidence.json) pins identities and all
observations. Timed DLL 6f8a065c3c906438eec71df50fdfd2f2f314fc6fdd66c13ca35182a8548c14cf; actual private package
bc2da6874fec465157d65b73eeeb99d7e740008ea35b23cb783ae16e43c70229, 462,028 B. Raw nonseekable-skip-preparation-qualified.tar.xz,
4,876,204 B/178 members, SHA256 571d81060acec9a6a38de9930fbb17a18bbe2a2d2831ceb9ce6f88d5c140d1ba,
is mirrored/member-verified. It retains attribution, sources, both producers,
checks, original tools/inputs, package and preceding private package.
