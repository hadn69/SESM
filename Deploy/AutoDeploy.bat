if %1 == acceptEULA goto :deploy
start "" notepad "%~dp0\SESM EULA.txt"
set /p accept=Do you accept the SESM End User Licese Agreement (EULA) ? [Y/N] %=%

if %accept% == y goto :deploy
if %accept% == Y goto :deploy
goto exit

:deploy
"%~dp0\SESM.deploy.cmd" /Y "-skip:objectName=filePath,absolutePath=SESM\\SESM.config$" "-skip:objectName=dirPath,absolutePath=SESM\\Logs" >> "%~dp0\Deploy.log"

:exit