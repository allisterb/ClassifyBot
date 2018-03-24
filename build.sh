#!/bin/bash

set -e 

dotnet build ClassifyBot.sln /p:Platform=x64 /p:Configuration=DebugLinux $*
