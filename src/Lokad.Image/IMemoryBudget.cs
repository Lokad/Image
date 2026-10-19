using System.Diagnostics.CodeAnalysis;

namespace Lokad.Image;

/// <summary>Immediate, thread-safe admission against a caller-owned shared memory allowance.</summary>
/// <remarks>The library borrows this object. Implementations forward to the caller's
/// root allowance; child scopes have no independent allowance. Estimates count live
/// capacity, not cumulative allocations. Callers serialize disposal with active users.</remarks>
public interface IMemoryBudget
{
    /// <summary>Create an empty owned scope sharing the root allowance.</summary>
    IMemoryScope CreateScope();

    /// <summary>Reserve nonnegative bytes; refusal leaves accounting unchanged and returns false/null.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The estimate is negative.</exception>
    /// <exception cref="ObjectDisposedException">The owner has been disposed.</exception>
    bool TryReserve(long bytes, [NotNullWhen(true)] out IMemoryReservation? reservation);
}

/// <summary>An owned subtree of a shared allowance. Creation reserves no bytes.</summary>
/// <remarks>Idempotent, nonthrowing disposal refunds the subtree and invalidates
/// descendants; it does not cancel work or dispose storage. Complete all users
/// before disposal. Admission through a disposed owner throws ObjectDisposedException.</remarks>
public interface IMemoryScope : IMemoryBudget, IDisposable { }

/// <summary>An owned estimate, refunded by idempotent, nonthrowing disposal.</summary>
public interface IMemoryReservation : IDisposable
{
    /// <summary>The current estimate; zero after this reservation or an ancestor is disposed.</summary>
    long Bytes { get; }

    /// <summary>Replace the estimate. Refusal leaves it unchanged; zero is valid.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The estimate is negative.</exception>
    /// <exception cref="ObjectDisposedException">This reservation or an ancestor has been disposed.</exception>
    bool TryResize(long bytes);
}
