#!/bin/bash
VERSION = $(printf "v%05d" $TRAVIS_BUILD_NUMBER)

dotnet restore
dotnet build -c Release --version-suffix $VERSION