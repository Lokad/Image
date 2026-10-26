# Cancellable VP8 coefficient blocks

The live-token helper polls before every arithmetic decision and publishes
the completed reader prefix in finally. Initial cancellation consumes no input;
coefficient writes retain their original order, including partially written
blocks. Cancellation is cooperative, with no cumulative work count or quota.

RFC6386§7.2/13.2 bounds16coefficients by seven token decisions, eleven category
bits and one sign each:304decisions. A decision normalizes by at most7bits.
The32-bit window needs at most269physical bytes for this bound; selection
requires272bytes within the supplied partition plus valid scalar state. Short
or invalid states use the scalar reader. Constructor lookahead is not exposed;
finally projects position/shift/range/value without an extra cancellation poll.

The decoder supplies private probabilities, a16-sample block and valid plane/
context. Small stack state stays inside existing coarse workspace, without
new heap storage or a separate admission owner. Tests stop at every decision
ordinal0..304, checking token identity, scalar cursor, coefficients and subsequent
reads. Original-reader composition compares1024states; maximum blocks exercise
every starting phase. [Cleanup qualification](cleanup-qualification.md) covers
current runtime/package results; old timing reports retain their producer scope.
