#!/bin/bash
pkill AbarimMUD
rm -rf AbarimMUD.Launcher/bin/Release
dotnet publish AbarimMUD.sln -c release -r linux-x64
nohup AbarimMUD.Launcher/bin/Release/net8.0/linux-x64/AbarimMUD.Launcher Data &
