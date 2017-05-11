@echo off
cd %~dp0
cd assets
..\..\Protobuild.exe --execute ProtogameAssetTool -o ..\compiled -p Windows -p Android -p MacOSX -p Linux
pause