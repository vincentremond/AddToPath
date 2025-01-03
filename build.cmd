@ECHO OFF

dotnet tool restore
dotnet build -- %*

dotnet run --project ./AddToPath/AddToPath.fsproj -- add ./AddToPath/bin/Debug/
