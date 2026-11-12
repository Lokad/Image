# Stored PNG streaming experiment

This implementation was not retained. It removed the64KiB input staging array
and reduced its proposed scratch admission from72KiB to8KiB, preserving exact
stored packets. The extra declared-length/row/final state did not demonstrate a
CPU improvement. Production, tests, binaries and the actual package are restored
to the qualified [Cross-prefix milestone](CROSS-BICUBIC-OUTPUT-PREFIX.md).

| Existing513x259 complete encode | Candidate / reference us | Conservative upper | Candidate managed B/op |
| --- | ---: | ---: | ---: |
| RGBA8,post-GC4operations | 87.63 / 188.87 | 0.498 | 548,308 |
| RGBA16,post-GC4operations | 518.14 / 406.17 | 1.299 | 1,086,052 |
| SameRGBA16,natural-GC64operations | 354.60 / 426.91 | 0.868 | 1,085,993.875 direct |

The contemporary word baseline is508.25/405.36us,1,151,588B/op.
The candidate saves65,536B/op but costs9.89us/1.95% more raw time; filtered and
all-actual raw intervals separate while reference-normalized intervals overlap.
The natural64-operation scope passes; it does not establish general warm parity
or explain the conditioning difference solely through GC. Byte passes. No causal
CPU gain or physical-peak reduction is claimed. Scratch retention remains zero.

Added scalar state also changes compressed object layout. Fourteen existing
complete compressed controls retained exact packets:11pass/3uncertain.
FastCancelable16 and RGB16 have adverse separated historical normalized
comparisons; these are concerns, not causal regression proofs. The async runner
failed case selection before timing and was not retried after abandonment. This
is a tooling setup failure, not evidence of a production defect.

The same frozen inputs, settings, public stream creation/growth/encode/disposal,
independent outputs, singleCPU and idle-v2 guards were preserved. Stored native
codec/frame/wrappers remain retained and word endian conversion stays timed;
compressed references include fresh codec/zlib state. No output snapshot copy,
pool, session or production dependency was added. New RFC1950/1951 packet controls
cover row endings,65,531-byte hard splits and empty final blocks. BothOSes pass
701internal/246public focused checks normally and with intrinsics disabled.
Two public tests initially retained the old admission threshold; their corrected
checks pass. No broad/package qualification was launched for this discarded code.

The finite289-path inventory, current benchmark rows and published packages are
unchanged. Existing full6497checks perOS and actual-package qualification remain
valid after exact restoration. A revised implementation may reuse existing
format state and avoid perturbing compressed layout; this is a lead, not merge
authorization or a reason to replay this implementation. The next demonstrated
resize gap has about59.46us excess, larger than this uncertain tradeoff.

Reproduction, source/module identities, every actual/bracket, failures and stored
packet checks are indexed in [the evidence](png-stored-direct-stream-evidence.json).
The mirrored, member-verified raw archive is
png-stored-direct-stream-inconclusive.tar.xz (1,511,648B/207members), SHA256
f16622f9856fceee31ad554ee1ff27b505cea42f099a958d63687f04215343d6.
