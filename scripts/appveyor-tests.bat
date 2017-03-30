echo "scanning tests in %APPVEYOR_BUILD_FOLDER%\test\"

for %%f in (%APPVEYOR_BUILD_FOLDER%\test\**\*.csproj) do (
	echo "testing %%~nf"
	dotnet test %%~nf -c Release
)