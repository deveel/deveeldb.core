#!/bin/bash

for projFile in ./test/**/*.csproj;
do
	dotnet test "$projFile" --no-build
done