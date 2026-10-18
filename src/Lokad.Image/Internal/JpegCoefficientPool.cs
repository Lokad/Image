namespace Lokad.Image.Internal;

/// <summary>Bounded exact-size private storage; every lease starts completely zeroed.</summary>
internal sealed class JpegCoefficientPool
{
    internal const int MaxRetainedBytes=4*1024*1024;
    internal static JpegCoefficientPool Shared {get;}=new();
    private readonly object gate=new();
    private readonly int[]?[] idle=new int[3][];
    private readonly int maximumBytes;
    private int retainedBytes;

    internal JpegCoefficientPool(int maximumBytes=MaxRetainedBytes)
    {
        if(maximumBytes<0)throw new ArgumentOutOfRangeException(nameof(maximumBytes));
        this.maximumBytes=maximumBytes;
    }

    internal int[] Rent(int length)
    {
        int[]? storage=null;
        if(length>0 && length<=maximumBytes/sizeof(int) && Monitor.TryEnter(gate))
        {
            try
            {
                for(var i=0;i<idle.Length;i++)
                    if(idle[i]?.Length==length)
                    {
                        storage=idle[i];idle[i]=null;retainedBytes-=length*sizeof(int);break;
                    }
                // A new image shape must not leave all slots occupied by an
                // unrelated old shape. Live leases are never in this array.
                if(storage is null){Array.Clear(idle);retainedBytes=0;}
            }
            finally {Monitor.Exit(gate);}
        }
        if(storage is null)return new int[length];
        Array.Clear(storage);
        return storage;
    }

    internal void Return(int[] storage)
    {
        if(storage.Length==0 || storage.Length>maximumBytes/sizeof(int) || !Monitor.TryEnter(gate))return;
        try
        {
            for(var i=0;i<idle.Length;i++)if(ReferenceEquals(idle[i],storage))return;
            var bytes=storage.Length*sizeof(int);
            if(bytes>maximumBytes-retainedBytes)return;
            for(var i=0;i<idle.Length;i++)
                if(idle[i] is null){idle[i]=storage;retainedBytes+=bytes;return;}
        }
        finally {Monitor.Exit(gate);}
    }
}
