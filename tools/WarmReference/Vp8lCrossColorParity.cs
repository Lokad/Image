#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Reflection;

// Original source-only fixed-control model. The eight search directions are
// generated geometrically; mathematical entropy is shared with the frozen E26 model.
public static class Vp8lCrossColorParity
{
    private const long Scale = 1L << 23;
    private static readonly Func<int,long> SLog = typeof(Vp8lPredictorParity).GetMethod("SLog",BindingFlags.Static|BindingFlags.NonPublic)!.CreateDelegate<Func<int,long>>();
    private static int Signed(int value) => unchecked((sbyte)value);
    private static long Cost(ReadOnlySpan<int> current, ReadOnlySpan<int> history)
    {
        long cost = 0; var total = 0; var combinedTotal = 0;
        for (var i = 0; i < 256; i++)
        {
            var count = current[i]; var combined = count + history[i];
            total += count; combinedTotal += combined;
            cost -= SLog(count) + SLog(combined);
        }
        cost += SLog(total) + SLog(combinedTotal);
        long weighted = current[0] * 3 * Scale; long weight = 240 * Scale;
        for (var magnitude = 1; magnitude < 16; magnitude++)
        {
            weighted += (weight * (current[magnitude] + current[256-magnitude]) + 50) / 100;
            weight = (weight * 6 + 5) / 10;
        }
        return cost - (weighted + 5) / 10;
    }
    private static uint Apply(uint pixel, int gr, int gb, int rb)
    {
        var green = Signed((int)(pixel >> 8)); var red = Signed((int)(pixel >> 16));
        var transformedRed = ((int)(pixel >> 16) - (Signed(gr) * green >> 5)) & 255;
        var transformedBlue = ((int)pixel - (Signed(gb) * green >> 5) - (Signed(rb) * red >> 5)) & 255;
        return pixel & 0xff00ff00u | (uint)transformedRed << 16 | (uint)transformedBlue;
    }
    public static (byte[] Map, byte[] Coded) Select(byte[] residual, int width, int height, int bits, int quality)
    {
        var columns = (width + (1 << bits) - 1) >> bits; var rows = (height + (1 << bits) - 1) >> bits;
        var map = new uint[columns * rows]; var pixels = new uint[width * height];
        for (var i = 0; i < pixels.Length; i++) pixels[i] = BinaryPrimitives.ReadUInt32LittleEndian(residual.AsSpan(i*4));
        var redHistory = new int[256]; var blueHistory = new int[256];
        var histogram = new int[256]; var tilePixels = new uint[1 << (bits*2)];
        uint previousX = 0, previousY = 0;
        for (var ty = 0; ty < rows; ty++)
        for (var tx = 0; tx < columns; tx++)
        {
            var x0 = tx << bits; var y0 = ty << bits; var x1 = Math.Min(width,x0+(1<<bits)); var y1 = Math.Min(height,y0+(1<<bits));
            var at = ty * columns + tx;
            if (ty > 0) previousY = map[at-columns];
            var count = 0;
            for (var y = y0; y < y1; y++) for (var x = x0; x < x1; x++) tilePixels[count++] = pixels[y*width+x];
            long RedScore(int gr)
            {
                Array.Clear(histogram);
                for (var i = 0; i < count; i++) histogram[(Apply(tilePixels[i],gr,0,0)>>16)&255]++;
                var cost = Cost(histogram,redHistory);
                if ((byte)gr == (byte)previousX) cost -= 3*Scale;
                if ((byte)gr == (byte)previousY) cost -= 3*Scale;
                if (gr == 0) cost -= 3*Scale;
                return cost;
            }
            long BlueScore(int gb, int rb)
            {
                Array.Clear(histogram);
                for (var i = 0; i < count; i++) histogram[Apply(tilePixels[i],0,gb,rb)&255]++;
                var cost = Cost(histogram,blueHistory);
                if ((byte)gb == (byte)(previousX>>8)) cost -= 3*Scale;
                if ((byte)gb == (byte)(previousY>>8)) cost -= 3*Scale;
                if ((byte)rb == (byte)(previousX>>16)) cost -= 3*Scale;
                if ((byte)rb == (byte)(previousY>>16)) cost -= 3*Scale;
                if (gb == 0) cost -= 3*Scale;
                if (rb == 0) cost -= 3*Scale;
                return cost;
            }
            var bestGr = 0; var bestRedCost = RedScore(0);
            for (var level = 0; level < 4+((7*quality)>>8); level++)
            {
                var step = 32 >> level;
                foreach (var sign in new[] {-1,1})
                {
                    var candidate = bestGr + sign*step; var cost = RedScore(candidate);
                    if (cost < bestRedCost) { bestRedCost=cost; bestGr=candidate; }
                }
            }
            var bestGb = 0; var bestRb = 0; var bestBlueCost = BlueScore(0,0);
            var iterations = quality < 25 ? 1 : quality > 50 ? 7 : 4;
            for (var level = 0; level < iterations; level++)
            {
                var step = 16 >> Math.Clamp(level-1,0,3);
                for (var axis = 0; axis < 8; axis++)
                {
                    var dx = axis < 2 ? 0 : axis < 4 ? axis==2 ? -1 : 1 : axis<6 ? -1 : 1;
                    var dy = axis < 2 ? axis==0 ? -1 : 1 : axis<4 ? 0 : axis%2==0 ? -1 : 1;
                    var gb = bestGb + step*dx; var rb = bestRb + step*dy; var cost = BlueScore(gb,rb);
                    if (cost < bestBlueCost) { bestBlueCost=cost; bestGb=gb; bestRb=rb; }
                }
                if (step==2 && bestGb==0 && bestRb==0) break;
            }
            previousX = 0xff000000u | (uint)(byte)bestRb<<16 | (uint)(byte)bestGb<<8 | (byte)bestGr; map[at]=previousX;
            for (var y = y0; y < y1; y++) for (var x = x0; x < x1; x++) pixels[y*width+x]=Apply(pixels[y*width+x],bestGr,bestGb,bestRb);
            for (var y = y0; y < y1; y++) for (var x = x0; x < x1; x++)
            {
                var index=y*width+x; var pixel=pixels[index];
                if(index>=2 && pixel==pixels[index-1] && pixel==pixels[index-2]) continue;
                if(index>=width+2 && pixel==pixels[index-width] && pixels[index-1]==pixels[index-width-1] && pixels[index-2]==pixels[index-width-2])continue;
                redHistory[(pixel>>16)&255]++; blueHistory[pixel&255]++;
            }
        }
        var mapBytes = new byte[map.Length*4]; var coded = new byte[residual.Length];
        for (var i = 0; i < map.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(mapBytes.AsSpan(i*4),map[i]);
        for (var i = 0; i < pixels.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(coded.AsSpan(i*4),pixels[i]);
        return (mapBytes,coded);
    }
}
