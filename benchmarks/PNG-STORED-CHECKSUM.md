# Stored-only PNG checksum

The existing stored-block encoder now uses a private guarded 64-byte AVX512BW/F
Adler loop. Its original 32-byte/scalar helper handles tails and portable paths.
The shared Adler helper is unchanged; compressed encodes and decoding do not
enter the new body. Exact complete packets, input/settings, memory admission,
cancellation and independently owned output are preserved.

The retained-native RGBA16 control independently improves 436.87→422.80 µs:
**14.06 µs / 3.22% raw**, with separated confidence. All-actual means improve
437.78→424.22 µs (13.57 µs / 3.10%), also separated. Native changes +0.16%;
normalized gain is 3.37%, with separated confidence. These are distinct estimates,
not an overall PNG speedup. This control remains a demonstrated parity gap.

| Complete stored encode | Lokad / reference µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| RGBA8, retained native context, post-GC4 | 62.92 / 189.19 | 0.333 / 0.375 | 609,708 | pass |
| RGBA16, retained native context, post-GC4 | 422.80 / 405.96 | 1.041 / 1.063 | 1,147,452 | gap |
| RGBA8, equivalent fresh contexts | 62.15 / 538.45 | 0.115 / 0.131 | 609,708 | pass |
| RGBA16, equivalent fresh contexts | 430.41 / 1,378.65 | 0.312 / 0.320 | 1,147,452 | pass |
| RGBA16, retained native context, natural-GC64 | 284.59 / 405.76 | 0.701 / 0.736 | 1,147,393.875 direct | pass |

Each confirmation retains 12 actuals and 12 native samples per bracket, pinned
CPU0 on the quiet VM with idle-v2 guards. Independent baseline/candidate capture
follows the three-actual screen. Filtered and all-actual distributions, including
outliers, remain in raw evidence. No extra sampling seeks a passing result.

Both sides encode the frozen original 513×259 RGBA samples at stored0/filter None,
with complete independently owned packets and output cleanup. Managed timing
includes fresh MemoryStream growth/public EncodePng/disposal and no ToArray copy.
Retained-native timing reuses codec/source/packet wrappers; word sample conversion
into the prepared frame is timed every operation. Fresh-native timing also includes
codec/options/zlib/frame/conversion/packet preparation and disposal. Post-GC4 and
natural-GC64 conditioning are separate scopes. They do not measure additive phases.
Packets remain exactly 533,444/1,066,552 B; word SHA256 is
`9b31604447d61d5ab7a30aad3cfde4cd73fef6d85ff506e74baeba807e54574d`.

Allocations are unchanged at each counter boundary. There are no new arrays,
fields, owners, caches or retained capacities. Existing stored-deflater admission
is 72 KiB (64 KiB history plus 4 KiB output, rounded allowance); operation scratch
retention is zero. Physical/native/JIT/pool peaks are unmeasured. The existing
codec allowance, output capacities and prompt owner cleanup remain unchanged.

The design isolates a useful lead from the [earlier global Adler trial](PNG-WIDE-ADLER-TRIAL.md),
whose compressed workload adversity prevents retaining it. Independent RFC1950
coefficient/overflow proof is reused for the identical 64-byte prefix. Normalize
prefix state before the original helper consumes each ≤63-byte tail; publish
checksum state only after success, with direct cancellation at 4096-byte blocks.
A new production-path test independently checks literal trailers, partitioning
and guard bytes for 54 deterministic zero/all-255/affine boundary controls.

Common generated Adler/Write/constructor bodies remain exactly 548/558/582 B after
normalizing addresses and method numbers only. All other original source is
reconstructed outside the stored call and new private body. Therefore reuse 15
compressed encode costs, 51 PNG read/validation controls, shared-root reads and
unrelated codec/resize/text evidence. Current [RGB16/gray confidence](PNG-CURRENT-ENCODE-CONFIDENCE.md)
remains source-valid: 17 fresh PNG controls have 16 passes/one gray uncertainty.
These are reused scoped costs, not a release-wide refresh on the new DLL.

A coherent follow-up fused source copy and checksum into the same loop. Its
three-actual screen measured 434.48/native404.57 µs versus the first design's
424.04/native404.51 µs screen, with wide overlapping confidence. It establishes no
benefit and no causal regression or permanent copy-fusion ban. Both variants pass
focused correctness. The checksum-only source and verified immutable modules
were restored byte-for-byte without rebuilding; the unretained source/modules
and complete adverse/inconclusive evidence are archived.

Only two core/embedded builds per OS cover those two designs; no public, harness,
native or fixture rebuild. All affected costs reconcile before one full/package
milestone: **6513 full/live checks on each OS**, plus **705 internal/246 public**
focused checks in normal/scalar/AVX512-off/AVX2-off modes on both OSes. Unchanged
resize ISA/storage and other codec/text qualification are reused. One pack and
one actual-package consumer per OS pass, including every existing owner/pixel/
admission check and all three usage examples. Published packages are unchanged.

The actual 0.2.1 development package is 456,266 B, SHA256
`a10d19929c0f7bfe18543488e97e60c31f6b4611d60d2b04036e353cb5e47792`;
its Windows/timed DLL is `4f86581e92b0fed87ce970a7358d495644a1676ab109f2868c21c9e100949f19`.
The Linux qualification build is `2d968192894c7ac9423ef596e70ba4a20a411206d094769cda3fc8048f31bf96`.

[Evidence](png-stored-checksum-evidence.json) pins sources, build manifests,
guards, exact packets, native identities, all actuals and ownership gates. Raw
`png-stored-checksum-qualified.tar.xz`, SHA256
`c032fe9bafa6b7f6c823692285602ef92af5222659cf2b8f519d60852a92325a`,
is 2,288,868 B/198 members, mirrored and member-verified, including the actual
package, both designs and unchanged harness dependencies. The initial clean-tree
staging guard finished after the patch and failed before backup/build/timing;
pinned original bytes were recovered from Git before continuing. No successful
build or capture was repeated for that preflight correction.

Stop confirmed passing controls. Preserve the retained-context gap and gray
uncertainty; continue justified independent codec/resize work across the full
289-path objective. Publication remains postponed.
