for %%F in (%APPVEYOR_BUILD_FOLDER%\test\**\*.csproj) do (
	dotnet test %%F -c Release
)