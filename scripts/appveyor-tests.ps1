$files = Get-ChildItem .\test\**\*.csproj
foreach ($file in $files) {
  dotnet test $file -c Release
}