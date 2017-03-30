#!/bin/bash
if [ ! -d ./packages/$TRAVIS_BUILD_NUMBER ]; then
  mkdir -p ./packages/$TRAVIS_BUILD_NUMBER;
fi

FILES = ./test/**/*.csproj
for f in $FILES
do
	dotnet pack f -c release --version-suffix $TRAVIS_BUILD_NUMBER -o ./packages/$TRAVIS_BUILD_NUMBER
done