:: ���ÿ���̨����ҳΪUTF-8
:: CHCP 65001 >nul

@ECHO OFF
SETLOCAL EnableDelayedExpansion
CLS

:: ============================================================================================
:: SET "BASE_DIR=D:\Design\2025.Work\" ��β��Ҫ����б��

:: ���û���·������
SET "BASE_DIR=%~dp0"
SET "PUBLISH_DIR=D:\Design\~Publish"
SET "CACHE_DIR=T:\Design"

:: ��Ҫ�ų����ɵ�Ŀ¼��ʹ�÷ֺż��
SET "EXCLUDE_DIRS=.git;.vs;DaLi.Utils.Test"

:: ������������Ŀ¼
ECHO ����׼����������...
rd /s /q "%CACHE_DIR%" 2>nul


IF NOT EXIST "%PUBLISH_DIR%" (
    MD "%PUBLISH_DIR%"
)

:: �� BASE_DIR ��ȡ�̷����л�
FOR /F "tokens=1 delims=:" %%i IN ("%BASE_DIR%") DO %%i:

:: ============================================================================================
:: ���빫����
CALL :CompileProjects "������Ŀ" "%BASE_DIR%"

:: ��������nugetĿ¼
rd /s /q "%PUBLISH_DIR%\nuget\utils" 2>nul
MD "%PUBLISH_DIR%\nuget\utils"

:: ���ƹ����ⷢ�а�
ECHO ���ƹ����ⷢ�а�...
@FOR /r "%CACHE_DIR%" %%i in (*.nupkg) DO (
    @COPY "%%i" "%PUBLISH_DIR%\nuget\utils"
    @COPY "%%i" "%PUBLISH_DIR%"
)

:: ============================================================================================
ECHO ������ʱĿ¼...
rd /s /q %CACHE_DIR%

ECHO.
ECHO ����������ɣ����飡
PAUSE
GOTO :EOF

:: ===== �������� =====

:CompileProjects
:: ����ָ��Ŀ¼�µ�������Ŀ
:: %~1: ��ʾ�ı���
:: %~2: ��Ŀ����Ŀ¼
ECHO %~1...
ECHO %~2...

IF NOT EXIST "%~2" (
    ECHO ����: Ŀ¼ "%~2" �����ڣ���������
    GOTO :EOF
)

CD %~2
:: ����Ŀ¼���ų�EXCLUDE_DIRS��ָ����Ŀ¼
@FOR /f %%i in ('dir /b /ad') DO (
    SET "SKIP_DIR=0"
    
    :: ��鵱ǰĿ¼�Ƿ����ų��б���
    FOR %%j IN (%EXCLUDE_DIRS:;= %) DO (
        IF "%%i"=="%%j" (
            ECHO �����ų���Ŀ¼: %%i
            SET "SKIP_DIR=1"
        )
    )
    
    :: ��������ų��б��У���ִ�б���
    IF !SKIP_DIR! EQU 0 (
        ECHO ���ڱ���: %%i
        dotnet publish %%i -c Release
        ECHO.
    )
)
GOTO :EOF