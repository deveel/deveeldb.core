#!/bin/bash
FILES = ./test/**/*.csproj
for f in $FILES
do
	dotnet test f
done