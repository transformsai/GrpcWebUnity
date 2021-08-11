
SETLOCAL

CALL :SetupPythonEnv Server
CALL :SetupNodeEnv Client

EXIT /B %ERRORLEVEL%

rem
:SetupPythonEnv
    set rootname=%~1
    cd %rootname%\
    if not exist venv\ (
        python -m venv --clear venv --prompt %rootname%
    )
    call .\venv\scripts\activate
    @echo on
    pip install wheel
    pip install -r requirements.txt
    call deactivate
    @echo on
    cd ..
    EXIT /B %ERRORLEVEL%

:SetupNodeEnv
    set rootname=%~1
    cd %rootname%\
    npm install
    npm run build
    cd ..
    EXIT /B %ERRORLEVEL%
