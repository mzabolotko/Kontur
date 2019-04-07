@echo off
SET TOOL_PATH=.fake

IF NOT EXIST "%TOOL_PATH%\fake.exe" (
  dotnet tool install fake-cli --tool-path ./%TOOL_PATH%
)
@echo on

dotnet restore
"%TOOL_PATH%/fake.exe" %*
