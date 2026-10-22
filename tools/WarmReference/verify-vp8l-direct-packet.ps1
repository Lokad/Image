param(
    [Parameter(Mandatory)][string]$NativeArchivePath,
    [Parameter(Mandatory)][string]$DependencyDirectory,
    [Parameter(Mandatory)][string]$ModelAssembly
)
$ErrorActionPreference='Stop'
foreach($name in 'Probe.dll','AuxiliaryProbe.dll','MainParityProbe.dll'){
    [void][Reflection.Assembly]::LoadFrom((Resolve-Path (Join-Path $DependencyDirectory $name)).Path)
}
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
Add-Type -TypeDefinition @'
public static class Vp8lDirectControlInput {
    public static byte[] Create() {
        var source=new byte[129*129*4];uint state=0x9e3779b9u;
        for(var at=0;at<source.Length;at+=4){state^=state<<13;state^=state>>17;state^=state<<5;source[at]=(byte)state;source[at+1]=(byte)(state>>8);source[at+2]=(byte)(state>>16);source[at+3]=255;}
        return source;
    }
}
'@
function Hash([byte[]]$bytes){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes))}
$source=[Vp8lDirectControlInput]::Create()
$sourceHash=Hash $source
if($sourceHash -ne '8d7970238f86775153ef39a123c7dfa67b7c1afc5040fb7fab2685357d4ff5a0'){throw 'Frozen source differs.'}
# Model inputs are source samples only; read the oracle after both operations.
$first=[Vp8lDirectPacketParity]::Encode($source,129,129)
$second=[Vp8lDirectPacketParity]::Encode($source,129,129)
$archiveStream=[IO.File]::OpenRead((Resolve-Path $NativeArchivePath).Path)
$gzip=[IO.Compression.GZipStream]::new($archiveStream,[IO.Compression.CompressionMode]::Decompress)
$tar=[System.Formats.Tar.TarReader]::new($gzip)
$expected=$null
try{
    while($entry=$tar.GetNextEntry()){
        if($entry.Name -eq 'tmp/e36-noise-native.webp'){
            $copy=[IO.MemoryStream]::new()
            try{$entry.DataStream.CopyTo($copy);$expected=$copy.ToArray()}finally{$copy.Dispose()}
            break
        }
    }
}finally{$tar.Dispose();$gzip.Dispose();$archiveStream.Dispose()}
if($null -eq $expected -or $expected.Length -ne 50010 -or
   (Hash $expected) -ne '18b6120553762b78d4ca5df0fbf34b45d9ee3a3eaaca02462636c17e795e9ec5'){throw 'Frozen reference differs.'}
if(-not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
   [Object]::ReferenceEquals($first.Packet,$second.Packet)){throw 'Complete packet or output ownership differs.'}
$second.Packet[20]=$second.Packet[20] -bxor 1
if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
   -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or (Hash $source) -ne $sourceHash){throw 'Negative mutation/source preservation differs.'}
[ordered]@{
    existingPublicCase='WebpEncoderPredictionTests.NonbeneficialNoiseRetainsTheExactPredecessorPacket'
    width=129;height=129;sourceSha256=$sourceHash;packetBytes=50010;packetSha256=(Hash $first.Packet)
    rawExact=$true;metadataExemptions=0;independentOutputs=$true;sourceUnchanged=$true
    negativeMutationRejected=$true;goldenPacketReadAfterModel=$true;details=$first.Details
    productionChanged=$false;nativeBuilds=0;nativeEncodes=0;timingRuns=0
    scope='Existing129x129 direct-coding input only; dimension-aware source model, not admitted production or general shape qualification.'
}|ConvertTo-Json -Depth 8
