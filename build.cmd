@echo off
dotnet build ClassifyBot.sln /p:Platform=x64 /p:Configuration=Debug %*
:end