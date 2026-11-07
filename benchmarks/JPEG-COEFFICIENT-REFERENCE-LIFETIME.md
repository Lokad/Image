# JPEG coefficient-reference lifetimes

The buffered live AC-refinement loop now delivers the correction bit before loading
its coefficient. Integer cursors cross fallible input work; the coefficient is loaded
and immediately updated afterward. A once-prepared64-sample span and a proven0..63
index remove per-correction bounds work and the interior reference held across bit
delivery. No arithmetic, read order, cancellation or pixel policy changes.

Independent complete decoding saves **102.83µs/2.69% raw and2.77% native-normalized**.
Both filtered and all-actual confidence views separate: raw candidate/original
interval[0.964030,0.982292], normalized[0.960537,0.984965]. Native movement is+0.088%.
FFmpeg parity remains open.

| Frozen513×259 live progressive444 decode | Managed µs | Native µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Contemporary qualified original,12actuals | 3,824.93 | 3,358.38 | 1.139 / 1.146 | 701,233 |
| Current independent,12actuals | 3,722.09 | 3,361.34 | 1.107 / 1.114 | 701,233 |

Both builds retain the132,961B packet, exact531,468B frame and frozen source/native
settings. Complete public decode, preparation/admission, reconstruction/conversion,
independent output and disposal remain timed, on one quiet VM CPU. Caller token
preparation is outside timing. Native decoder/conversion contexts remain retained;
its cancellation boundary difference remains disclosed. This is the tested live444
control, not broader JPEG or compression parity.

The initial short3,750.78µs/native3,361.04µs could not establish a gain against older
4,180.69µs/native3,666.00µs evidence: both clocks moved. A contemporary original and
one independent candidate confirmation resolve that comparison. Every original
actual/bracket is retained; expectation metadata restores byte-exactly after each job.
No favorable unchanged repeats or expanded workload grid.

The selected path requires64 samples. A nonzero UInt64 correction mask yields an
index0..63, including for malformed encoded input; masking by63 is an identity.
Its original pre-read bounds access therefore could not fail. The original first
value read also followed bit delivery. Immediate stores and all arithmetic exceptions
stay in order: Int32.MinValue still fails only after a set correction bit is consumed.
Cancellation and failed reads preserve the exact completed prefix and following cursor.

Current attribution retains942buffered-live samples of1,926total/1,785managed. Exact
JIT/map evidence identified the spilled reference before input work. The final body
shrinks2,989→2,921B, stack200→168B, with no per-correction range check or reference
across bit delivery. Samples/skid and code shape are investigation evidence, not phase
times or predicted gains.

Added owned heap/retained storage is zero; admission, coefficients, planes and outputs
are unchanged. Both current64-operation GC samples report701,233B/op. The older
48-operation diagnostic population reports701,182B/op and remains recorded separately;
no causal production allocation change is claimed. Physical/admitted peak and native
allocation are not newly measured. Existing retained result/cache bounds stay explicit.

Windows/Linux each pass **5,767 full/live checks** (2,511internal/2,308public/948differential)
and60internal/341public per normal/scalar mode. Three added guarded-position cases
compare the actual primitive with the existing independent original traversal across
signed/extreme values, pending phases, truncation, markers, cancellation and following
reads. The fast path requires existing AVX2/BMI2 guards; the scalar public suite checks
the unchanged portable fallback. No ISA selection changed. One immutable main/embedded
build per OS, no public/harness/native/selector/fixture rebuilds.

The SAME actual private0.2.1 Release package and all three examples pass both OSes.
Package SHA256`ce9b830a5d874d7210d704e1b47361cc2f1f38b0044940bdc07ed2af1c4125e4`,420,863B.
Four ordinary decoder controls and progressive420coding-validation reuse unchanged
paths: six total, one pass/three gaps/two uncertain. One displayed cost refreshes;
124 numerical rows remain source-valid. Published packages are immutable; publication,
the full actual-package release refresh and all283-path parity remain open.

[Evidence](jpeg-coefficient-reference-lifetime-evidence.json) pins exact source undo,
measurements, qualified modules, the package and mirrored/member-verified raw inputs.
Raw`jpeg-coefficient-reference-lifetime-qualified.tar.xz`:2,728,108B/134members,
SHA256`b7b81f8eae46e659d692425cefe11f35da8c57503e009eb56f084c5fa143ff06`.
[Earlier correction-bit selection](JPEG-CORRECTION-BIT.md) retains prior results and
links the preceding ownership and refinement stages.
