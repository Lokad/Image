// Source-only first palette primitive: the existing single-pixel public input.
// RFC 9649 defines palette deltas, index packing and the auxiliary/main syntax.
#nullable enable
using System;
using System.Buffers.Binary;
public static class Vp8lSingletonPaletteParity
{
    public static byte[] Encode(byte[] rgba)
    {
        if(rgba.Length!=4)throw new ArgumentException("Expected one RGBA8 pixel.");
        var color=(uint)rgba[3]<<24|(uint)rgba[0]<<16|(uint)rgba[1]<<8|rgba[2];
        var palette=Vp8lAuxiliaryParity.Encode(new[]{color},1,75);
        // Palette-index rasters use an opaque carrier alpha; the inverse
        // palette transform obtains the real alpha from the palette itself.
        var indices=Vp8lAuxiliaryParity.Encode(new[]{0xff000000u},1,75);
        var bits=new Vp8lParityBits();bits.Put(0x2f,8);bits.Put(0,14);bits.Put(0,14);
        bits.Put(rgba[3]==255?0:1,1);bits.Put(0,3);
        bits.Put(1,1);bits.Put(3,2);bits.Put(0,8);Append(palette.Bytes,0,palette.BitCount);
        bits.Put(0,1); // End transforms.
        bits.Put(0,1);bits.Put(0,1); // No color cache, one main prefix group.
        Append(indices.Bytes,1,indices.BitCount);
        var payload=bits.Finish();var packet=new byte[20+payload.Length+(payload.Length&1)];
        "RIFF"u8.CopyTo(packet);BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(4),(uint)(packet.Length-8));
        "WEBPVP8L"u8.CopyTo(packet.AsSpan(8));BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(16),(uint)payload.Length);payload.CopyTo(packet,20);
        return packet;

        void Append(byte[] data,int start,int stop)
        {for(var at=start;at<stop;at++)bits.Put(data[at>>3]>>(at&7)&1,1);}
    }
}
