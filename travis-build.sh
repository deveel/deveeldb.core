#!/bin/bash
dotnet restore

dotnet build -c Release --version-suffix $TRAVIS_BUILD_NUMBER