namespace Lokad.Image.Internal;

internal static class MemoryAdmission
{
    // Establish a surviving owner before its storage exists. The operation scope
    // must not be this owner's parent: returned storage outlives that operation.
    internal static IMemoryReservation? Own(IMemoryBudget? budget, long bytes)
    {
        if (budget is null) return null;
        if (!budget.TryReserve(bytes, out var reservation)) throw Refused();
        return reservation;
    }

    internal static ImageLimitException Refused() => new(ImageLimitKind.WorkingBytes,
        "Cooperative memory admission refused.", ImageLimitScope.SharedBudget);
}
