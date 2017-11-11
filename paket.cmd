@echo off

setlocal
set target=%~dp0\.paket\
set paket=%target%\paket.exe

if not exist %paket% (
    mkdir %target% 2> nul
    pushd %target%
    rmdir /q /s %temp%\nuget\paket.bootstrapper 2> nul
    nuget install -out %temp%\nuget -excludeversion paket.bootstrapper
    xcopy %temp%\nuget\paket.bootstrapper\tools\paket.bootstrapper.exe .
    rem paket.bootstrapper.exe --self
    paket.bootstrapper.exe
    popd
)

%paket% %*
endlocal