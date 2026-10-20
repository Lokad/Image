# Cancellable VP8 coefficient blocks

The live-token coefficient helper preserves the original cancellation check before
every arithmetic decision. It borrows the token, uses the existing block window
arithmetic and settles completed work once when leaving the block. The existing
noncancelable window remains unchanged.

RFC 6386 sections 7.2 and 13.2 bound one block by 16 coefficients, each requiring
at most seven token decisions, eleven category bits and one sign: 304 decisions.
Selection requires that much remaining local work and at least 272 partition bytes
also present in the supplied span. Each decision normalizes by at most seven bits;
the original 32-bit window proof covers every prefix and at most 269 physical bytes
for 304 decisions, within the conservative bound. Short input, tight work and
invalid scalar states keep the original reader before any optimized work.

The decoder supplies its complete private probability model, 16-sample block and
valid plane/context. The helper adds no allocation, table, owner, shared root or
reservation; its small stack state remains within the existing coarse allowance.
It never recovers from admission refusal or snapshots shared available memory.

Each successful token poll precedes the existing decision's work increment and
arithmetic. A throwing poll consumes neither. A `finally` settles exactly the
completed count through `CommitAdmittedWork`, which performs no additional poll,
then projects the original scalar position, shift, range and value. Partition end
is unchanged. Constructor lookahead is not exposed; initial cancellation leaves
zero work and the original cursor. Coefficients retain the original write order,
including partially written blocks. No cancellation check is batched or removed.

Tests deterministically stop before every decision ordinal from 0 through 304,
checking token identity, exact completed work, all scalar fields and subsequent
reads. Independent original-reader block composition compares token/category/sign/
dequantized output, EOB, guards and work across 1,024 states. Maximum 304-decision
blocks exercise every initial cursor phase; eligibility and borrowed-root tests
cover tight/short/invalid states and initial cancellation. Runtime/package and
quiet-VM qualification are reported separately in the benchmark evidence.
