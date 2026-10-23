param(
    [Parameter(Mandatory)][string]$ModelAssembly,
    [Parameter(Mandatory)][string]$SourcePath,
    [Parameter(Mandatory)][string]$PhotographSourcePath,
    [Parameter(Mandatory)][string]$NativePrefixDirectory,
    [Parameter(Mandatory)][string]$PriorNativeArchivePath
)
$ErrorActionPreference='Stop'
[void][Reflection.Assembly]::LoadFrom((Resolve-Path $ModelAssembly).Path)
function Hash([byte[]]$bytes){[Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData($bytes))}
function Member([string]$path,[string]$name){
    $stream=[IO.File]::OpenRead((Resolve-Path $path).Path)
    $gzip=[IO.Compression.GZipStream]::new($stream,[IO.Compression.CompressionMode]::Decompress)
    $tar=[System.Formats.Tar.TarReader]::new($gzip)
    try{while($entry=$tar.GetNextEntry()){if($entry.Name -eq $name){
        $copy=[IO.MemoryStream]::new();try{$entry.DataStream.CopyTo($copy);return ,($copy.ToArray())}finally{$copy.Dispose()}
    }}}finally{$tar.Dispose();$gzip.Dispose();$stream.Dispose()}
    throw 'Missing frozen packet member.'
}
$controls=@(
    @('asymmetric',$SourcePath,'70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80','1c9c7469eb5b9f3a898704bb5e6848fc3e6d8224e9f4d3fe9964d9c7eff6ded9',3),
    @('photograph',$PhotographSourcePath,'2b970f3ddee2a651ca381aab10b90db78a2ce08db8c7f70154bec92c1ec33090','28932bb732389e34054bd2cd044fcfc7e44de98b89e65bc4dff6ef576fbbd575',2)
)
$rows=@()
foreach($control in $controls){
    $name=$control[0];$source=[IO.File]::ReadAllBytes((Resolve-Path $control[1]).Path)
    if((Hash $source) -ne $control[2]){throw 'Frozen source differs.'}
    $prefix=Join-Path $NativePrefixDirectory "e48-${name}"
    $native=Get-Content "${prefix}.json" -Raw|ConvertFrom-Json
    $first=[Vp8lEffortParity]::EncodeAdaptiveSampling($source,513,259)
    $second=[Vp8lEffortParity]::EncodeAdaptiveSampling($source,513,259)
    $levels=@()
    foreach($level in 0,1){
        $bits=$level+2;$expected=[IO.File]::ReadAllBytes((Resolve-Path "${prefix}-${bits}.argb").Path)
        $modes=[byte[]]::new($expected.Length/4)
        for($i=0;$i -lt $modes.Length;$i++){$modes[$i]=$expected[4*$i+1]}
        if(-not [Linq.Enumerable]::SequenceEqual[byte]($first.Predictor.CandidateModes[$level],$modes) -or
           -not [Linq.Enumerable]::SequenceEqual[byte]($second.Predictor.CandidateModes[$level],$modes) -or
           $first.Predictor.EntropyCosts[$level] -ne $native.levels[$level].entropyCost){throw 'Candidate modes/costs differ.'}
        $levels+=[ordered]@{bits=$bits;modeSamples=$modes.Length;modeSha256=(Hash $modes);entropyCost=$first.Predictor.EntropyCosts[$level];allModesExact=$true;costExact=$true}
    }
    [byte[]]$expected=if($name -eq 'asymmetric'){Member $PriorNativeArchivePath 'tmp/e46-method5-native.webp'}else{[IO.File]::ReadAllBytes((Resolve-Path "${prefix}-native.webp").Path)}
    if((Hash $expected) -ne $control[3] -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       $first.Predictor.SelectedBits -ne $control[4] -or $first.Predictor.StoredBits -ne $control[4] -or
       $native.selectedBits -ne $control[4] -or
       [Object]::ReferenceEquals($first.Packet,$second.Packet) -or
       [Object]::ReferenceEquals($first.Predictor.Modes,$second.Predictor.Modes) -or
       [Object]::ReferenceEquals($first.Predictor.Residual,$second.Predictor.Residual)){throw 'Complete packet, sampling or ownership differs.'}
    $residualHash=Hash $first.Predictor.Residual
    $second.Packet[20]=$second.Packet[20] -bxor 1
    $second.Predictor.Modes[0]=$second.Predictor.Modes[0] -bxor 1
    $second.Predictor.Residual[0]=$second.Predictor.Residual[0] -bxor 1
    if([Linq.Enumerable]::SequenceEqual[byte]($second.Packet,$expected) -or
       [Linq.Enumerable]::SequenceEqual[byte]($first.Predictor.Modes,$second.Predictor.Modes) -or
       (Hash $first.Predictor.Residual) -ne $residualHash -or
       -not [Linq.Enumerable]::SequenceEqual[byte]($first.Packet,$expected) -or (Hash $source) -ne $control[2]){throw 'Mutation/source preservation differs.'}
    $rows+=[ordered]@{name=$name;sourceSha256=$control[2];selectedBits=$first.Predictor.SelectedBits;storedBits=$first.Predictor.StoredBits;levels=$levels;
        residualSha256=$residualHash;packetBytes=$first.Packet.Length;packetSha256=$control[3];rawExact=$true;cachedBytes=$first.CachedBytes;uncachedBytes=$first.UncachedBytes;
        independentOutputs=$true;sourceUnchanged=$true;negativeMutationRejected=$true;details=$first.Details}
}
[ordered]@{sourceOnly=$true;cases=$rows;productionChanged=$false;timingRuns=0;scope='Existing513x259 asymmetric/photograph RGBA8 atmethod5, quality75/exact=1. Both adaptive2..3 mode maps/costs and complete packets; coarse and fine winners. Other method/settings and admitted production/public boundaries remain open.'}|ConvertTo-Json -Depth 10
