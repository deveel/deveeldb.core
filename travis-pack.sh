#!/bin/bash
FILES = ./test/**/*.csproj
for f in $FILES
do
	dotnet pack f --version-suffix $TRAVIS_BUILD_NUMBER
done