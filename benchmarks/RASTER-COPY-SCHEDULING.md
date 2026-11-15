# Raster copy scheduling

[Wide scan CRC](WIDE-PNG-SCAN-CRC.md) now owns the two PNG composition costs.
The four unchanged basic copy/clone comparisons retain this producer.

Current borrowed-copy costs are in [prepared copy rows](DRAWING-ROW-PREPARATION.md); current owned flips are in [grouped copying](SNAPSHOT-BLOCK-COPY.md). This report retains its original evidence/history.

Normal RGBA16 copying and cloning now pass the original FFmpeg comparisons.
Uncancelable word clones reuse the existing 16 KiB BCL strip loop; byte bulk
copying, live-token checkpoints, admission and cleanup remain. The shared row
copier is unchanged. No new kernel, buffer, owner, cache or public API.

| Existing complete 513×259 operation | Lokad / FFmpeg µs | Upper / pass | GC B/op |
| --- | ---: | ---: | ---: |
| Normal orientation RGBA16 | 15.48 / 16.65 | 0.936 | 56.0 |
| Clone RGBA16 | 15.45 / 16.74 | 0.926 | 56.0 |
| Clone RGBA8 | 5.67 / 7.73 | 0.750 | 56.0 |
| Normal orientation RGBA8 | 5.56 / 7.66 | 0.739 | 56.0 |
| Complete PNG8 decode/orient/crop/fit | 1,236.76 / 1,478.65 | 0.845 | 169,454.8 |
| Complete PNG16 decode/orient/crop/fit | 2,867.64 / 2,934.06 | 0.982 | 338,159.2 |

Independent complete normal-word pairs show **17.048 → 15.441 µs**, ratio
**.906 (.900–.911)**. Reversed producer assignment gives **17.408 → 15.405 µs**,
mapped ratio **.885 (.878–.892)**. The scoped gain is about **9–12%** across those
assignments, not a universal percentage. Each retains twelve alternating 4096-call
pairs and 56 B/op. Original pixels, hidden RGB, source and independent owners match.

The first 64-call PNG16 capture remained uncertain. One paired pipeline run was
adverse, **1.009 (1.002–1.016)**; reversing producers maps to **.993 (.986–1.001)**.
A same-DLL control is **.993 (.984–1.002)**. Attribution changes sign with assignment;
no fixed slot correction, AA subtraction or causal pipeline/byte gain is claimed.
All controls remain. One prescribed **64 → 256** standalone integration passes
at upper **.982** on the same immutable DLL. No source or successful timing was
repeated to repair a tool error.

Complete public admission, output acquisition/materialization and disposal stay
timed. FFmpeg retains graph/source/frame wrappers; null views are independently
materialized inside each operation, with padded rows exposed directly. PNG controls
retain full decode/orient/crop/fit and every managed intermediate. Setup/JIT/IPC/
verification are excluded equally. Quiet CPU0, Release, natural nonconcurrent GC,
all twelve actuals/native brackets and original reference identities remain.

All 18 basic owned edits still allocate **56 B/op**. Active byte/word result payload
is **531,468/1,062,936 B**, separately from borrowed input, the unchanged two-slot/
4 MiB idle store and zero retained operation scratch. Caller-root results bypass
idle storage. Physical/native/JIT peaks are unmeasured.

Existing tests cover 16 KiB crossings/tails, exact words/hidden alpha, identity
resize, retained siblings, refusal/cancellation and recovery. Each OS passes
**6,696 full/live checks**, plus **25 internal/118 public** focused normal/scalar.
The **same actual Release package and all three examples pass Windows/Linux**.
One core/embedded build per OS, no public/harness/native/helper/fixture builds,
one pack and no post-timing producer rebuild. Unchanged arithmetic/ISA proof is reused.

The 20 owned-edit/composition views now have **18 passes, one gap, one uncertainty**;
191 existing image views have **187 passes, one gap, three uncertain**. Stop the
new passes. Byte vertical flip remains a gap; word flip and two resize views remain
uncertain. Drawing/inspection gaps and the full 289-path objective stay open.

[Evidence](raster-copy-scheduling-evidence.json) pins every current comparison and
control. Timed DLL b9822a4946a02e99b7f1918f644d3e359dc4c7ac7631ae5a0084891147f5d2a1; actual private package
2e38138311d4983c901179849099b074ab2e89a1a7dea5d5d3b5cdd54bfc53de, 461,903 B. Raw raster-copy-scheduling-qualified.tar.xz,
5,468,044 B/296 members, SHA256 254540f22edaae5451829452f7b349c3029fed4ef8fa49a38206a4cab9f9372f,
is mirrored/member-verified; dependencies retain attribution and preceding package.
Publication is postponed.
