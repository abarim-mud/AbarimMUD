#!/bin/bash
pkill AbarimMUD
dotnet publish AbarimMUD.sln -c release -r ubuntu.20.04-x64
nohup AbarimMUD.Launcher/bin/Release/net6.0/ubuntu.20.04-x64/AbarimMUD.Launcher Data &
