@echo off
rem Compila o Stack Quest usando o compilador C# que ja vem com o Windows (.NET Framework 4.x)
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
cd /d "%~dp0"
"%CSC%" /nologo /codepage:65001 /optimize+ /target:winexe /win32icon:src\icon.ico /out:StackQuest.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll src\*.cs
if errorlevel 1 (echo ERRO na compilacao & pause & exit /b 1)
echo Pronto: StackQuest.exe