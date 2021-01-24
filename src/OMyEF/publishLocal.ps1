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

$Version = Get-Content "$PSScriptRoot\publishLocalVersion.txt" -Raw
$splitVersion = $Version.Split(".")
$IncreaseVersion = ([int]$splitVersion[-1]) + 1
$NewVersion = $splitVersion[0] + "." + $splitVersion[1] + "." + $IncreaseVersion
$NewVersion > "$PSScriptRoot\publishLocalVersion.txt"

$LocalNugetFolder = "$env:USERPROFILE\LocalNugetFiles"
if(-not ( Test-Path $LocalNugetFolder )){
    $null = New-Item $LocalNugetFolder -ItemType Directory
}

$FoldersToBuild = @('OMyEF', 'OMyEF.Db', 'OMyEF.Server')
foreach($folder in $FoldersToBuild){
    Get-ChildItem "$env:USERPROFILE\LocalNugetFiles\$Folder" | Sort-Object CreationTime -desc | Select-Object -Skip 3 | Remove-Item -Force -ErrorAction SilentlyContinue -Recurse
    Get-ChildItem "$PSScriptRoot\$Folder\bin\Release" -Filter '*.nupkg' -ErrorAction SilentlyContinue | ForEach-Object { Remove-Item $_.FullName -Force }
    $ProjectLocation = "$PSScriptRoot\$Folder"
    Push-Location $ProjectLocation
    dotnet build -c Release 
    dotnet pack -c Release -p:NuspecFile=".\$($Folder).nuspec" -p:NuspecBasePath=.\bin\release -p:NuspecProperties=OMyEfVersion=$NewVersion
    $nupkgFiles = Get-ChildItem "$PSScriptRoot\$Folder\bin\Release" -Filter '*.nupkg'

    foreach($file in $nupkgFiles){
        cmd /c $NugetLocation add $file.FullName -source $LocalNugetFolder
    }
    Pop-Location    
}
