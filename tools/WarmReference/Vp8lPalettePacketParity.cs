// Source-only palette packet composition. Palette/index syntax: RFC 9649.
// Production admission and general palette ordering remain unqualified.
#nullable enable
using System;
using System.Linq;
using System.Buffers.Binary;
public sealed record Vp8lPalettePacketResult(byte[] Packet,object Details);
public static class Vp8lPalettePacketParity
{
    public static Vp8lPalettePacketResult Encode(byte[] source,int width,int height,int method=4)
    {
        if(width<1||width>16384||height<1||height>16384||source.Length!=checked(width*height*4))throw new ArgumentException("Expected RGBA8 dimensions.");
        var choice=Vp8lInitialChoice.Select(source,width,height,method);
        if(choice.Mode!=4)throw new NotSupportedException("This stage composes palette coding only.");
        var palette=Vp8lPaletteParity.Build(source,width,height);
        var bits=new Vp8lParityBits();
        bits.Put(0x2f,8);bits.Put(width-1,14);bits.Put(height-1,14);
        bits.Put(Enumerable.Range(0,width*height).Any(i=>source[4*i+3]!=255)?1:0,1);bits.Put(0,3);
        bits.Put(1,1);bits.Put(3,2);bits.Put(palette.Colors.Length-1,8);
        var auxiliary=Vp8lAuxiliaryParity.Encode(palette.Deltas,palette.Deltas.Length,75);
        for(var bit=0;bit<auxiliary.BitCount;bit++)bits.Put(auxiliary.Bytes[bit>>3]>>(bit&7)&1,1);
        var coded=new byte[palette.Packed.Length*4];
        for(var i=0;i<palette.Packed.Length;i++)BinaryPrimitives.WriteUInt32LittleEndian(coded.AsSpan(i*4),palette.Packed[i]);
        var stage=new Vp8lInputParity(bits.Finish(),bits.Count,coded,new{palette=palette.Colors.Length});
        var frame=Vp8lDirectPacketParity.Compose(stage,palette.CodedWidth,height,choice.HistogramBits,choice,method==0);
        return new(frame.Packet,new{colors=palette.Colors.Length,packingBits=palette.PackingBits,codedWidth=palette.CodedWidth,composition=frame.Details});
    }
}
