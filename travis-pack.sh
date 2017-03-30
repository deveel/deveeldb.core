#!/bin/bash
num=$TRAVIS_BUILD_NUMBER
VERSION=$(printf "v%05d\n" $num)

echo "version suffix to $VERSION"

if [ ! -d ./packages/$TRAVIS_BUILD_NUMBER ]; then
  mkdir -p ./packages/$TRAVIS_BUILD_NUMBER;
fi

for f in ./test/**/*.csproj;
do
	dotnet pack $f -c release --version-suffix $VERSION -o ./packages/$TRAVIS_BUILD_NUMBER
done