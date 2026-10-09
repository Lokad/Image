using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class ResizeSamplingTests
{
    // Original rational controls: (2*x+1)*source/(2*target) is exactly an integer
    // minus 1/(2*target). Binary64 rounds that position up to the next cell.
    // The source/target lengths fit RGBA8 array admission with explicitly raised
    // dimensions/pixels/memory limits. Test coordinates without allocating GiB.
    [Theory]
    [InlineData(123919244, 358964423, 329768846, 134890243)]
    [InlineData(420664975, 443772109, 463656806, 402624054)]
    [InlineData(96544623, 378670093, 220776274, 165590988)]
    [InlineData(94086296, 369597527, 133764754, 259964313)]
    [InlineData(325047379, 312680771, 432073781, 235228494)]
    [InlineData(61394483, 314342115, 325779689, 59239026)]
    public void LargeAdmittedAxesSelectTheExactCell(int index, int source, int target, int expected)
        => Assert.Equal(expected, ResizeSampling.Nearest(index, source, target));

    [Theory]
    [InlineData(0, 3, 2, 0)] [InlineData(1, 3, 2, 2)]
    [InlineData(0, 2, 3, 0)] [InlineData(1, 2, 3, 1)] [InlineData(2, 2, 3, 1)]
    [InlineData(0, 2147483647, 1, 1073741823)]
    [InlineData(0, 1, 2147483647, 0)]
    [InlineData(2147483646, 2147483647, 2147483647, 2147483646)]
    public void CentersEdgesAndIntegerEndpointsHaveLiteralCells(int index, int source, int target, int expected)
        => Assert.Equal(expected, ResizeSampling.Nearest(index, source, target));
}
