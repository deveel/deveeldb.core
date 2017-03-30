#!/bin/bash
if [ ! -d ./packages/$TRAVIS_BUILD_NUMBER ]; then
  mkdir -p ./packages/$TRAVIS_BUILD_NUMBER;
fi

for f in ./test/**/*.csproj;
do
	dotnet pack $f -c release --version-suffix $TRAVIS_BUILD_NUMBER -o ./packages/$TRAVIS_BUILD_NUMBER
done