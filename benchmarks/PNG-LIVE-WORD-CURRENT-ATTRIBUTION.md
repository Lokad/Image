# Live RGBA16 PNG attribution

PWA777 captures the complete existing live-token513×259 RGBA16 decode using the
immutable DLL in the qualified LWP776C package. It changes no production source,
build, fixture, worker or timing result. Existing51 PNG decode/validation controls
remain49pass/1gap/1uncertain; the source-valid word gap is2919.94/native2894.87 µs,
upper1.012,8568 B/op. The older live-loop profile predates the selected cursor.

One2-second capture, CPU0 operation/CPU1 sampler, has1994 sample locations:

| Method | Locations |
| --- | ---: |
| CopyCancelableLiteralRun | 1373 |
| InflateLiveCursor | 291 |
| Canonical Decode / tree construction | 51 / 46 |
| Adler / PNG CRC | 39 / 38 |

The305-byte literal-run body holds bit state locally. Its source still repeats
lookup bounds/classification, cancellation polls and refill checks per literal.
This is not evidence of per-literal tree construction or caller dispatch. Sample
locations, inlining and skid do not establish phase time or removable work. All
instrumented elapsed values are discarded; no CPU saving is claimed.

The next bounded hypothesis groups already-cached short prefixes to reduce loop,
refill and view setup, retaining each symbol's two cancellation checks, exact
partial stores and completed cursor. Preserve short/incomplete-prefix behavior,
EOB/match/overflow fallback and error ordering. No speculative input padding,
extra table, owner, pool, session or fixture-specific state is proposed. Wider
live-body restructuring is justified only by complete-operation evidence.

Use the existing word primary/byte companion, independent actual-kernel prefix/
tail/cancellation checks, then complete-operation screens. Extra spans, unrolling
and register pressure may outweigh sharing. Confirm a useful coherent stage and
reconcile companions before broad/package qualification; retain adverse evidence.
Ordinary, rolling-history and counting paths stay stopped unless affected.

Both idle-v2 guards, frozen input/settings, full16-bit samples, alpha/hidden RGB,
native normal/scalar pixels, live cancellation and independent output gates pass.
Before capture, preflight found the known package-consumer-only manifest change;
that pin and canonical identity were reconciled without restarting a capture.
No production or package input changed, so existing6197 full/live checks per OS,
affected ISA and SAME actual package/all3examples qualification is reused.

[Evidence](png-live-word-current-attribution-evidence.json) pins sources, package,
receipt, generated code, raw map and reproduction. The renderer gains no dependency
and displayed costs stay unchanged. Raw `png-live-word-current-attribution-pwa777.tar.xz`,
SHA256 `1bd1a3117265854b35b36cc02b5c6193db4c14a366d9db27fc07641b9e880ed2`,
344008 B/30members, is mirrored and member-verified. Its dependencies retain the
[live cursor](PNG-LIVE-PREPARED-CURSOR.md), [current51 controls](PNG-CURRENT-FRAMING-COSTS.md)
and [qualified build](LANCZOS-PAIRED-WORD-ROWS.md). Publication remains postponed.
