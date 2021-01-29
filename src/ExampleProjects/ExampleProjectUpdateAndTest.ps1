Push-Location $PSScriptRoot
. ..\OMyEF\publishLocal.ps1
Pop-Location

Push-Location "$PSScriptRoot\OMyEFDbContext"

dotnet add package OMyEF.Db
dotnet restore

Pop-Location

Push-Location "$PSScriptRoot\OMyWebAPI"

dotnet add package OMyEF
dotnet restore

Pop-Location

Push-Location "$PSScriptRoot\OMyWebAPITests"

dotnet test

Pop-Location