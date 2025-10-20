@echo off
echo =================================
echo   Сборка RVN Майнера
echo =================================
echo.

echo Восстановление пакетов...
dotnet restore

if %errorlevel% neq 0 (
    echo Ошибка при восстановлении пакетов
    pause
    exit /b 1
)

echo Сборка проекта...
dotnet build -c Release --no-restore

if %errorlevel% neq 0 (
    echo Ошибка при сборке проекта
    pause
    exit /b 1
)

echo Публикация приложения...
dotnet publish -c Release --no-build -o publish --self-contained false

if %errorlevel% neq 0 (
    echo Ошибка при публикации
    pause
    exit /b 1
)

echo.
echo =================================
echo Сборка завершена успешно!
echo.
echo Исполняемый файл: publish\rvn-miner.exe
echo Размер:

if exist "publish\rvn-miner.exe" (
    for %%A in ("publish\rvn-miner.exe") do echo %%~zA байт
)

echo.
echo Для запуска используйте:
echo   publish\rvn-miner.exe
echo или
echo   dotnet publish\rvn-miner.dll
echo.
pause