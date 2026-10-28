# Shared bicubic word publication

The fixed Up16 control falls from 4,141.053 to 3,513.798 µs: 15.1% lower elapsed
in fresh independent twelve-actual baseline/candidate confirmations. Filtered
and unfiltered confidence intervals separate; all actuals remain. The ratio is
1.221× / upper 1.234, so this control still has a CPU gap. Measured allocation
is 5,016,602 versus 5,016,594 B/op (+8); no owned buffer or admitted capacity is
added. One short screen precedes the confirmations, without candidate variants.

All five affected existing default-workspace controls now have current costs.
Inputs remain the original 513×259 RGBA16 raster; outputs appear below. Operations
remain complete, warm, single-CPU and independently owned. Reference: the frozen
zimg workers, FFmpeg's zscale backend, with approved unchanged boundaries/settings.

| Existing control | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Up16 → 1027×519 | 3,513.798 | 1.221 / 1.234 | 5,016,602 | Confirmed gap |
| Down16 → 256×129 | 597.246 | 1.044 / 1.050 | 464,333 | Confirmed gap |
| Horizontal16 → 256×259 | 705.104 | 1.216 / 1.316 | 730,645 | Screen gap |
| Vertical16 → 513×129 | 696.107 | 1.176 / 1.258 | 913,653 | Screen gap |
| VerticalUp16 → 513×519 | 1,614.391 | 0.964 / 0.973 | 2,516,728 | Confirmed pass; stop |

The current six-second profile captures 5,958 samples: normalization blocks account
for 45.1%, and the out-of-line reciprocal helper another 6.3%, forcing vector
spills. All 5,154 Cached instruction samples map to the same process's 8,316-byte
JIT body. No separate replay is needed. The initially suggested four-tap reuse
is not tried: its eligible subset of the ~14% horizontal share does not establish
a useful whole-operation gain.

The accepted change removes 43 duplicated lines and uses the existing qualified
word-pair helper. Divisors, division, margins, clipping and packing match; its
additional conservative refusals use existing Original replay. The established
16×16-tap error bound covers this caller. No helper, tap order, pixel contract,
buffer, admission, cancellation, ownership or public API changes. The pre-change
argument and unchanged helper's frozen 1,008-control oracle are retained.

Windows/Linux each pass 5,074 checks and 89 focused cases normal/scalar. Windows
tests the portable path; Linux executes AVX512. The SAME actual private 0.2.1
package passes both platforms and three examples, including frozen 8/16-bit
cached pixels, word enlargement, shared refusal/recovery and survivor ownership.
Published packages remain immutable. VerticalUp16 retains its approved native
RGB error ≤2 only where reference alpha ≤16; other RGB/alpha bounds and RMSE stay
fixed, and every managed sample is exact. Its independent upper 0.973 passes:
stop this unchanged case. Byte and mixed-word paths retain applicable evidence.

[Evidence](warm-bicubic-shared-word-publication-evidence.json) includes samples,
confidence, allocation, producer and qualification pins. Mirrored/member-verified
bicubic-shared-word-publication-qualified.tar.xz: 230 members, SHA256
a969ecba9ec8af4f17b12b34f60e542a2a9299ef0b05544dc109c92a012327d4.
The diagnostic archive is linked there. This does not complete the full 268-path
objective or establish general replacement readiness.
