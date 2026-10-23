param([Parameter(Mandatory)][string]$ModelAssembly,[Parameter(Mandatory)][string]$SourcePath,[Parameter(Mandatory)][string]$NativeArchivePath)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
function Hash([byte[]]$b){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($b))}
$source=[IO.File]::ReadAllBytes((Resolve-Path $SourcePath).Path)
$sourceHash=Hash $source
if($sourceHash -ne '70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'){throw 'Frozen asymmetric source differs.'}
$targets=@{0='a83e601e8d0fd35d256dd92d682db09d6868a7f401466d8f13cb95b1fd6d150b';1='f5a66ebfaff467b0f2409663753b3387b02b4f872e711c034869c328317b956d';5='1c9c7469eb5b9f3a898704bb5e6848fc3e6d8224e9f4d3fe9964d9c7eff6ded9'}
$rows=@()
foreach($method in 0,1,5){
    if($method -eq 5){
        $firstComparison=[Vp8lEffortParity]::CompareCacheCandidates($source)
        $secondComparison=[Vp8lEffortParity]::CompareCacheCandidates($source)
        $first=$firstComparison.Selected;$second=$secondComparison.Selected
        $details=[ordered]@{cachedBytes=$firstComparison.CachedBytes;uncachedBytes=$firstComparison.UncachedBytes;framing=$first.Framing}
    }else{
        $first=[Vp8lEffortParity]::EncodeFixedSampling($source,513,259,$method)
        $second=[Vp8lEffortParity]::EncodeFixedSampling($source,513,259,$method)
        $details=$first.Details
    }
    $stream=[IO.File]::OpenRead((Resolve-Path $NativeArchivePath).Path)
    $gzip=[IO.Compression.GZipStream]::new($stream,[IO.Compression.CompressionMode]::Decompress)
    $tar=[System.Formats.Tar.TarReader]::new($gzip);$expected=$null
    try{while($entry=$tar.GetNextEntry()){if($entry.Name -eq "tmp/e46-method${method}-native.webp"){
        $copy=[IO.MemoryStream]::new();try{$entry.DataStream.CopyTo($copy);$expected=$copy.ToArray()}finally{$copy.Dispose()};break
    }}}finally{$tar.Dispose();$gzip.Dispose();$stream.Dispose()}
    if($null -eq $expected -or (Hash $expected) -ne $targets[$method]){throw 'Native packet identity differs.'}
    if(-not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       [Object]::ReferenceEquals($first.Packet,$second.Packet)){throw "Method${method} packet or output ownership differs."}
    $second.Packet[20]=$second.Packet[20] -bxor 1
    if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or (Hash $source) -ne $sourceHash){throw 'Mutation/source preservation differs.'}
    $rows+=[ordered]@{method=$method;packetBytes=$first.Packet.Length;packetSha256=$targets[$method];rawExact=$true;independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;details=$details}
}
[ordered]@{sourceOnly=$true;sourceSha256=$sourceHash;cases=$rows;productionChanged=$false;nativeEncodes=0;timingRuns=0;scope='Existing asymmetric513x259 RGBA8 input: fixed method0/1 algorithms and method5 cache/no-cache comparison on its confirmed shared default prefix. General adaptive sampling and production/public boundaries remain open.'}|ConvertTo-Json -Depth 11
