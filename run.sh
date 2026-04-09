#!/bin/zsh
# Run the Penguin Platformer game
export PATH="/usr/local/share/dotnet:$PATH"
cd "$(dirname "$0")"
dotnet run
