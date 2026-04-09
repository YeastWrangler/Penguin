#!/bin/zsh
# Double-click this file in Finder to launch the game
export PATH="/usr/local/share/dotnet:$PATH"
cd "$(dirname "$0")"
dotnet run
