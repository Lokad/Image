# JPEG scan-state experiment

The scan-level refinement redesign did not establish a useful complete-decode
speedup. All production stages are restored; current benchmark costs and the
qualified package remain unchanged. This is an inconclusive timing result, not
a demonstrated regression or a permanent exclusion of scan-state designs.

| Stage, live progressive444 | Managed µs | Managed/native | Conservative upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| 170 baseline short | 5,021.204 | 1.411353 | 1.601280 | 984,212 |
| 170 candidate short | 4,958.035 | 1.400303 | 1.513773 | 984,212 |
| 171 candidate short | 4,987.317 | 1.382117 | 1.541387 | 984,212 |
| 172 baseline confirm | 4,699.299 | 1.397394 | 1.415099 | 984,212 |
| 172 candidate confirm | 4,655.933 | 1.380355 | 1.408084 | 984,212 |
| 173 candidate short | 4,676.220 | 1.393596 | 2.416964 | 984,212 |

Stages170–171 retain lookahead/invariant state across the scan and move block
execution into its stack-only cursor. Stage172 removes unused setup and confirms
the completed design independently: before/candidate raw and normalized intervals
overlap. Stage173 materially revises correction execution into bounded atomic
groups; its short screen has no useful normalized lead and receives no retiming.
Its internal cancellation cadence changes explicitly to checkpoints before groups
while preserving the public cooperative contract and valid original prefixes.

All stages preserve exact complete pixels, partial/error cursors, marker/stuffing/
restart/EOB/overflow behavior, independent output and shared admission. Each OS
passes normal/scalar JPEG checks:364/331 internal/public initially,365/331 after
scan scheduling,366/331 after grouping. Two useful retained Facts exercise all
65,536 correction fields and concurrent cancellation against the actual restored
production reader. Restoration passes37 focused checks per OS and mode.

The complete operation includes public Decode, normalization and owned output
disposal on the frozen513×259,132,961-byte progressive444 packet. Native context
reuse stays explicit. Managed/native pixels match the frozen531,468-byte frame.
Both before/after quiet guards, source/output identity and ownership gates pass;
the existing expected-frame metadata projection is restored byte for byte.

No production/package rebuild is required for restoration. One retained-test
build per OS supplements unchanged WSP152 full/live/scalar and same actual
Release-package consumer qualification. No accepted allocation/CPU gain. Next,
inspect instruction-level attribution: method shares and reacquired-byte counts
did not establish which inlined work costs time.

[Evidence](jpeg-scan-refinement-inconclusive-evidence.json) records exact producers,
all stages, gates, restoration and dependent archives. All source variants, actual
Windows/Linux binaries, failures, samples and reproduction are mirrored/member-
verified in `jpeg-scan-refinement-inconclusive.tar.xz`:2,191,356B/295members,
SHA256 `f5940984229f2e0a74c5c8e6462c82011dec9729e42868e982503b9f588609e2`.
