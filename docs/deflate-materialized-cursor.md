# Materialized Deflate block cursor

PNG decoding can use a local bit cursor for a complete-Huffman, noncancelable
block written into an already admitted private output span. Tight work limits,
cancelable calls, counting and rolling-row inflation retain their original paths.
No new heap buffer, table, cache or memory owner is introduced.

RFC 1951 sections 3.2.5–3.2.7 bound a prefix by 15 bits, length extras by 5 and
distance extras by 13. A literal therefore charges at most 15 bits per output
byte. A match charges at most 48 bits and produces at least three bytes, hence
at most 16 bits per output byte. EOB or the first offending/truncated/excess
token charges at most another 48 bits, including an extra field charged before
its failed refill. For `R` remaining output bytes, selecting only when remaining
logical work is at least `16*R+48` admits every possible block prefix. Tree and
block-header work is paid before selection; subsequent blocks select afresh.
This precondition fits `long` throughout the `int` output domain. It neither
charges the bound nor reserves a separate allowance: only consumed bits settle.

Root-prefix lookahead precedes its matched-code charge. Extra fields charge before
refill, matching the original reader. A request of at most 16 bits refills while
fewer than 16 are available, so a 32-bit load leaves at most 47 meaningful bits
in the `ulong` cache. Short tails keep byte refills. Long prefixes use the original
canonical decoder, with cursor/work state published before the call and restored
even if it throws. A final `finally` publishes all state on every block exit;
the existing outer settlement records exact completed work without replacing the
original failure with a cancellation poll.

Reserved codes, history/window bounds and excess output are checked in their
original order. History copies retain the original overlap-safe prefix doubling.
The exact checksum position and unused trailing-IDAT behavior are unchanged.
Selection never queries shared available memory or recovers from admission refusal.
Caller-owned storage, local memory limits and asynchronous ownership are unchanged.

Authored tests compare the original per-symbol materializer across block phases,
length/distance extras, overlapping history, deep canonical codes, every truncation,
malformed tokens, guarded output, remaining work and checksum positions. Existing
independent BCL, exact-budget, cancellation and cooperative-owner controls remain.
The [benchmark summary](benchmark.md) states the separate performance qualification.
