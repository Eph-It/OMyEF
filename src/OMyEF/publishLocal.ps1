$nugetFile = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
$NugetLocation = "nuget.exe";
try{
    $output = cmd /c $NugetLocation /? *>&1
    if($output[0].GetType().Name -eq 'ErrorRecord'){ throw }
}
catch{
    $FileLocation = "$env:USERPROFILE\nuget.exe"
    if(-not (Test-Path $FileLocation)){
        $Answer = Read-Host "Can't find nuget.exe - download from $nugetFile ?(y/n)"
        if($Answer.ToLower() -eq 'y'){
            
            Write-Host "using $FileLocation"
            $WebClient = New-Object System.Net.WebClient
            $WebClient.DownloadFile($nugetFile,$FileLocation)
        }
    }
    $NugetLocation = $FileLocation
}

$ProjectLocation = "$PSScriptRoot\OMyEF"
Push-Location $ProjectLocation
dotnet build -c Release 
dotnet pack -c Release -p:NuspecFile=.\OMyEF.nuspec -p:NuspecBasePath=.\bin\release


$nupkgFiles = Get-ChildItem "$PSScriptRoot\OMyEF\bin\Release" -Filter '*.nupkg'

$LocalNugetFolder = "$env:USERPROFILE\LocalNugetFiles"

if(-not ( Test-Path $LocalNugetFolder )){
    $null = New-Item $LocalNugetFolder -ItemType Directory
}

foreach($file in $nupkgFiles){
    cmd /c $NugetLocation add $file.FullName -source $LocalNugetFolder
}

Pop-Location