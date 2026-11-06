# JPEG correction-bit selection

Live progressive AC refinement selects each coefficient update through exact
integer masks. A set correction bit changes only a magnitude whose refinement
bit is missing; zero correction leaves the coefficient unchanged. The existing
signed-update rule, every cancellation poll, immediate updates and logical cursor
remain exact. Int32.MinValue fails only for a set correction, after publishing the
already-consumed bit. No new storage, owner, pool, session or numerical policy.

| Complete513×259live progressive444 decode | Elapsed µs | Managed B/op |
| --- | ---: | ---: |
| Contemporary original,12actuals | 4,289.15 | 701,182 |
| Current, independent12actuals | 4,180.69 | 701,182 |
| Current FFmpeg, faster native bracket | 3,666.00 | Unmeasured |

Independent complete decoding saves **108.47µs/2.53%raw and2.07%normalized**.
Both filtered/all-actual confidence views separate. Raw candidate/original interval
is.967580–.981877; normalized.958899–.995813. Native point changes−0.466%.
The currentratio1.140/upper1.151 remains a gap. Five other required
controls retain unchanged paths: six total,1pass/3gaps/2uncertain. No broader JPEG,
sampling, quality or compression claim; encoded inputs and reconstructed pixels stay.

Both subjects retain the frozen132,961Bpacket and exact531,468BRGBA frame. Complete
public decode, preparation/admission, reconstruction/conversion, independent output
and disposal remain timed. The caller's live token is prepared outside timing;
native cancellation remains its declared boundary difference. Native decoder/
converter wrappers remain retained as labeled. Input preservation, native exact
pixels, original settings, both native brackets and idle-v2guards pass. Temporary
decoder expectation metadata restores byte-exactly after every job.

One preceding short each supplies121.19µs/2.83%raw and3.34%normalized points with
wide overlapping intervals. Follow with one independent original/candidate
confirmation. Retain all12actuals on each side, including the original's one
statistically excluded point. No unchanged favorable repeat or expanded matrix.

The existing current signed-refinement profile remains a useful source-valid
lead: entropy/Huffman/refinement files and ReadBlock/ApplyCorrection bodies are
unchanged. Later plane scheduling invalidates old whole-rendering attribution,
not those source identities. Reuse the949hot traversal samples without another
capture; samples/skid are locations, not phase times or projected gains.
Earlier buffered-eligibility experiments changed a different part of the loop;
their inconclusive/adverse evidence stays in
[the correction-preparation report](JPEG-BUFFERED-CORRECTION-PREPARATION.md).

Bounded arithmetic exploration covers29,148boundary/random value-bit-depth cases,
including zero and both Int32extremes. It is not production/performance proof.
Existing independent header/bit/run/marker/truncation/cancellation/overflow/logical-
tail oracles qualify behavior. The existing overflow test additionally checks that
zero correction on MinValue neither throws nor changes the logical tail.

Windows/Linux each pass **5,742full/live checks** (2,489internal/2,305public/948
differential), plus57internal/341public pernormal/scalar. One immutable core/embedded
build perOS is reused throughout. No public/harness/native/selector/fixture build or
new ISA operation/guard; unchanged vector-path disabled-ISA proof is reused.
The SAME actual private0.2.1Releasepackage/allthreeexamples passes bothOSes. Its
existing AC-refinement consumer covers live sync/async frames, shared refusal/
cancellation cleanup and independent survivors. Package
`aa71a6da413ccc3c2a9669d7d57e30c3a60a1d975ea856c253e02e64286b9417` is418,496B.
Published releases remain immutable; publication is separate and postponed.

Allocation stays701,182B/op; new owned capacity and retained operation workspace
are zero. Existing [plane scheduling](JPEG-PROGRESSIVE-PLANE-SCHEDULING.md) keeps its
capacity/ownership proof. The existing1,697,280Bungoverned coefficient cache remains;
governed calls bypass it. No complete physical/admitted peak or native allocation
is newly measured. Separate GC totals, live capacity and retained storage.

[Evidence](jpeg-correction-bit-evidence.json) retains sources, source projection,
all complete points, unchanged-case reuse and actual-package qualification. Raw
`jpeg-correction-bit-qualified.tar.xz`: 2,607,260B/129members, SHA256
`624f0b847f29b7465f96a7c2dbaecf2ed5f64f85359cecb5c9d1364bd893a1d1`, mirrored/member-verified with pinned reused inputs. One displayed
live row refreshes;124reuse unchanged paths. This is not a125-row actual-package
release refresh or completion of the282-path objective.

Stop this stage and select the next substantial justified codec/resize task.
