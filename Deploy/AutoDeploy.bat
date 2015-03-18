@echo off
if "%1"=="" goto :question
if %1 == acceptEULA goto :deploy
:question
start "" notepad "%~dp0\MITLicense.txt"
set /p accept=SESM is Released under MIT License. You must agree to it to install/use SESM.Do you agree to it ? [Y/N] %=%

if %accept% == y goto :deploy
if %accept% == Y goto :deploy
goto exit

:deploy
"%~dp0\SESM.deploy.cmd" /Y "-skip:objectName=filePath,absolutePath=SESM\\SESM.config$" "-skip:objectName=dirPath,absolutePath=SESM\\Logs" >> "%~dp0\Deploy.log"

:exit