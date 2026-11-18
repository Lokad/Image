# JPEG pending publication

**Accepted, JSP1255.** Three small, non-inlined helpers construct the existing
typed pending values only after the existing completion checks. This removes
repeated cold construction from immediate JPEG grammar advancement. Final
complete asynchronous-probe pairs save **2.56% / 5.57 ns**, with unchanged
**568 B/op**. This is a scoped incremental effect, not a uniform speedup.

| Complete operation | Baseline / candidate or managed / reference µs | Ratio interval | Result |
| --- | --- | --- | --- |
| Private asynchronous screen, three pairs | .217017 / .211696 | .907452–1.048602 | Promising point |
| Independent private confirmation, twelve pairs | .215350 / .214013 | .989844–.997748 | Scoped gain |
| Final repository DLL confirmation, twelve pairs | .218092 / .212519 | .968341–.980598 | Gain transfers |
| Final synchronous probe / FFmpeg adapter | .184243 / .194156 | .936097–.961768 | Pass; stop |
| Final asynchronous probe / FFmpeg adapter | .216719 / .215312 | .983597–1.028464 | Uncertain |
| Final complete JPEG headers / FFmpeg adapter | 2.187728 / 2.813302 | .710874–.844795 | Pass; stop |
| Matched complete-header DLL guard, twelve pairs | 2.025630 / 2.025875 | .926781–1.076026 | Neutral/inconclusive |

The smaller private confirmation remains evidence of its own magnitude; gains
are not summed. The final header point is slower than the historical
1.999/2.799 µs observation. One matched check with the same original HeaderWorker
does not demonstrate a causal regression or gain. It does not replace the
standalone cost, and no favorable recapture follows.

## Boundary and correctness

There is one grammar and the same `JpegPending` representation. Completion tests,
`GetResult`, assignment/constructor ordering, resume state, reads, cancellation
polls, metadata policy, limits, failure precedence and actual pending-user
lifetimes are unchanged. Inlining the helper bodies recovers the original source
exactly. There is no sentinel shortcut, new immediate condition, buffer, field,
cache, API or input coalescing. This differs from the earlier completed-task
sentinel experiment and the rejected prefix-acquisition trial.

Probe measurements retain the original compiled BenchmarkDotNet invocation,
consumer, unrolled and remainder loops; synchronous974960/asynchronous855040
managed and262144native operations per statistical unit, twelve units per group,
and all sixteen subdivisions. All observations and added clock/loop overhead
remain included. The conservative gate considers every original confidence
view. Frozen input, descriptor, prefix, source, ownership and disposal checks
pass. Complete headers include fresh delivery, full entropy, late metadata,
EOF, independent owned descriptors and disposal. The reference is explicitly
FFmpeg AVIO/owned descriptors with the existing bounded header adapter, not a
public packet-parser comparison. Twenty original grammar controls remain valid.

All VM work is sequential Release CPU0 with optimized non-tiered JIT, natural
nonconcurrent workstation GC and idle-v2 guards. Reference, fixture, harness,
driver and compiled-loop identities are retained in the evidence.

## Storage and qualification

Actual immediate `AdvanceJpeg` code shrinks8264→7793B and its entry stack
reservation1080→472B. Cold helper calls have separate24B argument adjustments;
neither figure measures physical or complete-operation peak storage. The final
and private code shapes agree. Probe/header allocation stays568/8768B and shared
admission512/8448B. No owned, admitted or retained heap capacity is added.

The reused genuinely pending-delivery diagnostic reports unchanged1960B for
asynchronous probing and9984B for complete asynchronous headers. These figures
include equal source, gate, bridge and batch overhead, with whole-process GC;
they are not standalone production allocation or native timing claims. Actual
suspension, exact descriptors, independent surviving outputs, caller input
ownership and cleanup pass. Native, physical and JIT peaks remain unmeasured.

Windows and Linux each pass **6844 full/live checks** (3379 internal,2517 public,
948 differential), plus11internal/182public focused checks in normal/scalar
modes. The **same actual Release package** passes both OS consumers and all three
usage examples. Affected performance reconciles before broad qualification.
Each OS builds the final core and embedded tests once; unchanged public,
reference, harness and fixture builds are reused. There is one NoBuild Release
pack and no rebuild of the measured DLL.

Only synchronous/asynchronous JPEG stream probing and complete JPEG headers
replace current costs. PNG/WebP, resident identification, pixel codecs,
editing/resizing, font/text and ownership implementations retain unchanged-path
evidence. The three copy gaps, asynchronous-probe uncertainty,289-path objective
and final165-comparison immutable-package refresh remain open. Publication is
postponed.

## Evidence and reproduction

[Machine-readable evidence](jpeg-pending-publication-evidence.json) pins:

- Windows DLL `30bf6a15b32dbde2d875b7e7c9b6e9ef08026dcf50c51b9de7f84d38776a47f8`.
- Timed Linux/package DLL `7b59eabed152f4c05331d957dad515b41aee8e278d026943de40e7202e270bd0`.
- Private0.2.1 package `f4c5b5dbaeb6f7d8d1782c9f2d1100dccbcdfa7994750c1a041fed3662b3ccf0`.

Mirrored/member-verified `tmp/jpeg-pending-publication.tar.xz` contains189members,
4057960B; SHA-256
`e878a9b7d86b022ac131f6ec0f60c8f515e7a85c835492f1077614758f3e4250`.
It retains exact sources/modules/symbols/package, source proof, all observations,
guards, JIT, pending diagnostics and platform/package logs. Recipes include
`jsp1255-prepare.py`, `jsp1255-final-build.py`, `jsp1255-final-linux.py`,
`jsp1255-final-probe-measure.py`, `jsp1255-headers.py`,
`jsp1255-header-guard.py`, `jsp1255-pending.py`, `jsp1255-full.py`,
`jsp1255-package-linux.py`, `jsp1255-package-windows.py` and
`jsp1255-record.py`. Existing script outputs are immutable: reproduce in a fresh
directory with the pinned prerequisites, rather than overwrite the archive.

[Original-loop reference](JPEG-PROBE-ORIGINAL-LOOP-REFERENCE.md),
[prefix acquisition](JPEG-PREFIX-ACQUISITION-TRIAL.md) and
[mapping-validation qualification](FONT-MAPPING-GROUP-VALIDATION.md) retain
their independent histories and prerequisite evidence.
