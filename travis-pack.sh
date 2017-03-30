#!/bin/bash
num=$TRAVIS_BUILD_NUMBER
VERSION=$(printf "v%05d\n" $num)
PACKAGES=./packages/$TRAVIS_BUILD_NUMBER

echo "version suffix to $VERSION"

if [ ! -d $PACKAGES ]; then
  mkdir -p $PACKAGES;
fi

for f in ./test/**/*.csproj;
do
	dotnet pack $f -c release --version-suffix $VERSION -o $PACKAGES
done