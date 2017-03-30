for %%F in (.\test\**\*.csproj) do (
	dotnet test %%F -c Release
)