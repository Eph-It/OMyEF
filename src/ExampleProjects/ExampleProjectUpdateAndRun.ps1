Push-Location "$PSScriptRoot\OMyEFDbContext"

dotnet add package OMyEF.Db
dotnet build

Pop-Location

Push-Location "$PSScriptRoot\OMyWebAPI"

dotnet add package OMyEF
dotnet build
dotnet run
Pop-Location