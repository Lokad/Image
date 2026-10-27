using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
namespace Lokad.Image.Internal;
internal sealed partial class Vp8RatePlan
{
    private static Vector256<ushort> CreateResidualBandOffsets()
    {
        Span<ushort> values=stackalloc ushort[16];
        for(var p=0;p<16;p++)values[p]=(ushort)(Band(p)*3*68);
        return Vector256.Create(values);
    }
    private static readonly Vector256<ushort> ResidualBandOffsets=CreateResidualBandOffsets();
    // RFC6386 coefficient bands and the existing generated rate tables. Prepare
    // magnitudes, contexts and complete lookup addresses in sixteen safe lanes.
    // The64-byte stack owner fits the outer admitted macroblock workspace.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ResidualVectors(ReadOnlySpan<ushort> values,ReadOnlySpan<short> levels,int type,int first,int initialContext,out bool nonzero)
    {
        if(levels.Length<16||(uint)type>3||(uint)first>1||(uint)initialContext>2||first==1&&levels[0]!=0)throw new ArgumentException("Invalid VP8 residual layout or context.");
        var absolute=Avx2.Abs(Vector256.Create(levels)).AsUInt16();
        if(!Vector256.EqualsAll(Avx2.Min(absolute,Vector256.Create((ushort)2047)),absolute))throw new ArgumentException("VP8 residual level exceeds 2,047.");
        var mask=~(uint)Avx2.MoveMask(Avx2.CompareEqual(absolute,Vector256<ushort>.Zero).AsByte());
        if(first!=0)mask&=~3U;
        nonzero=mask!=0;
        var index=(type*8+Band(first))*3+initialContext;
        if(!nonzero)return values[EobAt+index*2];
        var last=(31-BitOperations.LeadingZeroCount(mask))/2;
        var clamped=Avx2.Min(absolute,Vector256.Create((ushort)67));
        var contexts=Avx2.Min(absolute,Vector256.Create((ushort)2));
        // Context at coefficient p is determined only by coefficient p-1.
        var previous=Vector256.Shuffle(contexts,Vector256.Create((ushort)0,0,1,2,3,4,5,6,7,8,9,10,11,12,13,14));
        previous=previous.WithElement(first,(ushort)initialContext);
        var indices=Avx2.Add(clamped,Avx2.Add(Avx2.MultiplyLow(previous,Vector256.Create((ushort)68)),
            Avx2.Add(ResidualBandOffsets,Vector256.Create((ushort)(LevelsAt+type*8*3*68)))));
        // These initialized local vectors each contain exactly sixteen words.
        // Views remain within this call; avoid two separate localloc arrays.
        ReadOnlySpan<ushort> magnitudes=MemoryMarshal.Cast<Vector256<ushort>,ushort>(MemoryMarshal.CreateReadOnlySpan(ref absolute,1));
        ReadOnlySpan<ushort> addresses=MemoryMarshal.Cast<Vector256<ushort>,ushort>(MemoryMarshal.CreateReadOnlySpan(ref indices,1));
        var cost=initialContext==0?values[EobAt+index*2+1]:0;
        for(var p=first;p<=last;p++)cost+=values[FixedAt+magnitudes[p]]+values[addresses[p]];
        if(last<15)
        {
            var next=(type*8+Band(last+1))*3+Math.Min((int)magnitudes[last],2);
            cost+=values[EobAt+next*2];
        }
        return cost;
    }
}
