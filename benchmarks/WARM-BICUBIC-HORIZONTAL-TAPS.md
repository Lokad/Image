# Bicubic horizontal tap views

The existing Down8 control (513×259 RGBA8 → 256×129) falls from 632.921 to
560.306 µs in independent quiet-VM confirmations: 11.5% lower filtered elapsed,
13.1% using all actual samples. Both confidence-interval forms separate. The
faster native bracket shifts by 2.8%; the worst-bracket ratio improves by 9.0%.
The current 1.339× ratio / upper 1.347 remains a CPU gap. Allocation is unchanged:
332,186 versus 332,187 B/op reflects invocation amortization, without a storage gain.

One three-actual screen precedes one twelve-actual confirmation; eleven managed
samples survive filtering, with all twelve retained. The unchanged-production
baseline has twelve actuals/nine filtered. The affected Up16 enlargement is
refreshed once: 4,182.047 µs, 1.453× / upper 1.654, 5,016,594 B/op. This screen
supports no gain or parity claim.

The retained current profile maps 2,887 of 5,936 samples to one horizontal tap
loop. The change bounds paired weight/offset spans once and borrows a vector view
of decoded pixels. Offsets are multiples of four doubles, so shifting by two
selects identical vectors. Tap/accumulator order, zero-padded unused taps, guarded
quantization, exact replay, cancellation, capacity and shared admission stay fixed.
No owned buffer, cache, native binding, dependency or specialization is added.

Exact managed rational/frozen pixels, approved native RGB/alpha bounds, input,
independent output and idle/process guards pass. Windows/Linux each pass 5,074
checks, including live comparisons, plus 89 focused cases normal/scalar. Windows
tests the portable path; Linux exercises AVX512. The same actual private 0.2.1
package passes both platforms and all three examples, including 8/16-bit frozen
bicubic pixels, admission refusal/recovery and retained-output ownership. Published
packages remain unchanged.

Nine existing Cached controls execute the changed general horizontal loop:
Down8/16, Up8/16, Cross8/16, Horizontal8/16 and HorizontalUp8. Down8 has current
confirmation and Up16 a current screen; the other seven need current cost evidence.
Earlier mixed-axis column kernels, vertical copy branches, tight-workspace/wider
support paths and other algorithms retain applicable unchanged evidence.

[Evidence](warm-bicubic-horizontal-taps-evidence.json) retains samples, uncertainty,
pins, qualification, scope and baseline links. Mirrored/member-verified raw
bicubic-horizontal-tap-views-qualified.tar.xz: 129 members, SHA256
47d2e63475d436b06d8daafea0f697581b6ba01ddb5a915dab3d75833c118ee1.
The finite 268-path CPU objective remains open.
