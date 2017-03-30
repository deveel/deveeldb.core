#!/bin/bash
dotnet restore

VERSION = $(printf "v%50g" $TRAVIS_BUILD_NUMBER)

dotnet build -c Release --version-suffix $VERSION