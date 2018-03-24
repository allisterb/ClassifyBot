#!/bin/bash

set -e 

donet build ClassifyBot.sln /p:Platform=x64 /p:Configuration=DebugLinux $*
