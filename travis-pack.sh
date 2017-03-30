#!/bin/bash
if [ ! -d ./packages/$TRAVIS_BUILD_NUMBER ]; then
  mkdir -p ./packages/$TRAVIS_BUILD_NUMBER;
fi

VERSION = $(printf "v%50g" $TRAVIS_BUILD_NUMBER)

for f in ./test/**/*.csproj;
do
	dotnet pack $f -c release --version-suffix $VERSION -o ./packages/$TRAVIS_BUILD_NUMBER
done