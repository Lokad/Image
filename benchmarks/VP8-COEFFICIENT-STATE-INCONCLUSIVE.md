# VP8 coefficient-state investigation

Neither bounded implementation establishes a useful complete-encode lead.
Production is restored exactly; current costs and the qualified package remain
unchanged. These screens are not independent gain/regression or parity proofs.

Current opaque16 attribution records 1,969 process samples: coefficient emission 378,
luma4 decisions 258 and token statistics 178. The 3,931 B coefficient body repeatedly
accesses byref writer fields. Both quiet guards and the exact 72,380 B packet pass;
one two-second CPU0 capture with sampler CPU1, no builds. Samples/skid are not method times
or predicted savings; instrumented elapsed is discarded.

Stage 220 prepares each coefficient block's symbols in one frame-owned 608 B stack
span, then drains them with local arithmetic state. It adds 640 B admitted scratch
and no heap storage. Stage 221 removes staging, keeps syntax/arithmetic interleaved,
copies the cursor locally, inlines word publication and returns carry by value.
It adds no storage. The revised implementation is a distinct hypothesis, not
repeated sampling of the same candidate.

| Quiet one-CPU screen, three actuals | Managed µs | Managed/FFmpeg ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: |
| 220 baseline, opaque16 | 20,220.96 | 1.294 / 1.331 | 1,201,552 | 72,380 |
| 220 candidate, opaque16 | 22,302.52 | 1.436 / 1.478 | 1,201,552 | 72,380 |
| 220 baseline, photograph | 13,851.27 | 1.403 / 1.445 | 809,072 | 17,688 |
| 220 candidate, photograph | 14,181.64 | 1.461 / 1.517 | 809,072 | 17,688 |
| 221 baseline, opaque16 | 20,274.19 | 1.300 / 1.336 | 1,201,552 | 72,380 |
| 221 candidate, opaque16 | 20,313.06 | 1.302 / 1.345 | 1,201,552 | 72,380 |
| 221 baseline, photograph | 13,618.63 | 1.403 / 1.488 | 809,072 | 17,688 |
| 221 candidate, photograph | 13,811.35 | 1.424 / 1.545 | 809,072 | 17,688 |

Existing 513×259 controls/settings, full normalization, preparation, fresh output
acquisition and disposal remain timed. FFmpeg retains codec/frame/packet wrappers;
its repacking and complete encode execute each operation. This retained native
setup is not silently attributed to algorithm speed. All original source, packet,
reconstruction, ownership and before/after quiet gates pass. No confirmation runs
follow these points. Allocation is unchanged; complete live peaks are unmeasured.

Stage 220 passes 816 internal/357 public checks per OS and normal/scalar mode, including
both writer entry points against 8,192 frozen symbol sequences, native carry/exact
limits and terminal-refusal/cancellation recovery. Stage 221 passes 814/357, with
staging-only tests removed. Both selected cores/PDB/XML, original tests and sources
are restored byte-for-byte; 47 writer/frame checks pass per OS/mode. Reuse PFE218's
5,547 full/live checks per OS and the same actual Release-package/all-three-example
qualification. No broad/package rebuild or repetition; no production change.
Two successful core/internal builds per OS; two corrected pre-timing Windows
compile errors; zero harness/native/selector builds and one untimed diagnostic build.

The follow-up ownership audit counts actual final-frame events using unchanged source
assemblies and verifies all three frozen packets, coefficient immutability and
borrowed-root cleanup. Counts describe logical storage, not measured allocations
or a new performance control. A two-byte event stream also needs 2,248 B of offsets
on each control, plus any real chunk capacity/metadata.

| Existing content | Ordered events | Current coefficient payload B | Hypothetical two-byte event payload B |
| --- | ---: | ---: | ---: |
| opaque | 778,189 | 448,800 | 1,556,378 |
| photograph | 148,033 | 448,800 | 296,066 |
| diagram | 65,626 | 448,800 | 131,252 |

Moving event production into existing statistics recording could remove a second
classification walk. It must preserve final probability references, fixed symbols,
the node 9/10 statistics distinction, skip contexts and header retries. Opaque
storage would grow materially with a simple two-byte design. Explicit ownership
and removal of full-frame duplication must precede completed-stage timing; these
counts neither authorize a merge nor prove a CPU benefit. No family is closed.

[Evidence](vp8-coefficient-state-inconclusive-evidence.json) pins all samples,
producers, gates, source variants and restoration. Mirrored/member-verified raw:

- `vp8-current-opaque-word-attribution.tar.xz`: 1,102,116 B / 37 members,
  SHA256 `9d58d701126cce125b134e1f19f53850b625078a36700886137a4918aef5ad5c`.
- `vp8-coefficient-state-inconclusive.tar.xz`: 3,215,920 B / 258 members,
  SHA256 `d95158429339d46a09219746acd816e82099418b3ad2e42bb1c461f99a6a3f49`.

Qualified-package/reference dependencies remain pinned in the manifests.
Published releases are unchanged; publication is postponed.
