# Current PNG Fastest word attribution

One bounded two-second sampler window uses the existing complete 513×259
RGBA16 Fastest1 control on immutable VTP306. All twelve actuals complete; exact
968,216 B packet, source/settings, independent output and both quiet guards pass.
Both sides include fresh codec/zlib/packet state; native word endian-frame
preparation/disposal remains timed. CPU0 executes, CPU1 samples. Zero builds or
new controls. Profiled elapsed is discarded; current comparison rows are unchanged.

| Exclusive sampled location | Samples, out of 884 |
| --- | ---: |
| Greedy parser, including inlined search | 590 |
| Token emission | 84 |
| Huffman ordering | 43 |
| Block construction | 42 |
| Huffman heap repair | 20 |

There are 813 mapped managed samples. The 1,246 B parser has hot locations in
candidate search, dictionary preparation and token publication. Samples and
instruction skid do not establish phase times or available gains. Context setup
is not this capture's dominant location; a prepared session would leave greedy
work in place. Earlier [local execution state](PNG-FAST-LOCAL-EXECUTION-STATE.md)
and [prepared probes](PNG-PREPARED-PROBES.md) remain present. The
[long-match transfer](PNG-GREEDY-MATCH-REUSE-REJECTED.md) and
[inner-poll removal](PNG-FAST-CANCELLATION-POLLS-REJECTED.md) are different,
unsuccessful implementations and are not replayed unchanged.

Next, prepare the cancellation policy once per greedy pass. A shared generic
worker can specialize an immutable uncancelable token while retaining **every**
checkpoint for live tokens, including publication, callbacks and cleanup. No
buffer, pool, owner or quota is proposed. Measure existing complete word/byte
Fastest controls; fewer token reads alone establish no gain. This is an eligible
hypothesis, not merge approval or a fixed continuation threshold.

Unchanged production reuses VTP306's 5,642 full/live checks per OS, affected
normal/scalar checks and the same actual package/all three examples. No broad
suite, rebuild, package proof or comparative confirmation follows attribution.
The 282-path objective remains open; publication is postponed.

[Evidence](png-fast-word-current-attribution-evidence.json) pins producer, source,
code, packet and guard identities. Reproduce with retained `pfa307-profile*.py`.
Trace/map/samples/JIT, sources, loaded runtime and receipts are retained; native
worker provenance is pinned through qualified dependencies.

Raw `png-fast-word-current-attribution.tar.xz`: 5,611,960 B / 113 members,
SHA256 `8dccc1386e6295bdeb09837c65323ee2408319ccb1ecbe0df97fe5aeefad242b`; mirrored/member-verified.
