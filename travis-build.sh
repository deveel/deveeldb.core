#!/bin/bash

num=$TRAVIS_BUILD_NUMBER
VERSION=$(printf "v%05d\n" $num)

echo "version suffix to" $VERSION

dotnet restore
dotnet build -c Release --version-suffix $VERSION