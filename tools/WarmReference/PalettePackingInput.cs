// Original deterministic palette fixtures: xorshift32 seed0x6d2b79f5,
// scalar RGBA formula; the first N indices guarantee every palette entry.
public static class PalettePackingInput {
    public static byte[] Create(int width,int height,int colors) {
        var bytes=new byte[width*height*4];uint state=0x6d2b79f5u;
        for(var i=0;i<width*height;i++) {
            state^=state<<13;state^=state>>17;state^=state<<5;
            var c=i<colors?i:(int)(state%(uint)colors);
            if(c==0)continue;
            bytes[4*i]=(byte)(c*47+11);bytes[4*i+1]=(byte)(c*89+7);bytes[4*i+2]=(byte)(c*131+23);bytes[4*i+3]=c%7==0?(byte)0:(byte)255;
        }
        return bytes;
    }
}
