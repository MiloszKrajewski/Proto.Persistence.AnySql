@echo off

setlocal
set fake=%~dp0\.fake\fake.exe

if not exist %fake% (
    pushd %~dp0
    rmdir /q /s %temp%\nuget\fake 2> nul
    nuget install -out %temp%\nuget -excludeversion fake
    xcopy %temp%\nuget\fake\tools\* .\.fake\
    popd
)

%fake% %*
endlocal