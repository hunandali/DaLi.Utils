:: 设置控制台代码页为UTF-8
:: CHCP 65001 >nul

@ECHO OFF
SETLOCAL EnableDelayedExpansion
CLS

:: ============================================================================================
:: SET "BASE_DIR=D:\Design\2025.Work\" 结尾需要带反斜杠

:: 设置基础路径变量
SET "BASE_DIR=%~dp0"
SET "PUBLISH_DIR=D:\Design\~Publish"
SET "CACHE_DIR=T:\Design"

:: 需要排除生成的目录，使用分号间隔
SET "EXCLUDE_DIRS=.git;.vs;DaLi.Utils.Test"

:: 清理并创建发布目录
ECHO 正在准备发布环境...
rd /s /q "%CACHE_DIR%" 2>nul


IF NOT EXIST "%PUBLISH_DIR%" (
    MD "%PUBLISH_DIR%"
)

:: 从 BASE_DIR 获取盘符并切换
FOR /F "tokens=1 delims=:" %%i IN ("%BASE_DIR%") DO %%i:

:: ============================================================================================
:: 编译公共库
CALL :CompileProjects "编译项目" "%BASE_DIR%"

:: 清理并创建nuget目录
rd /s /q "%PUBLISH_DIR%\nuget\utils" 2>nul
MD "%PUBLISH_DIR%\nuget\utils"

:: 复制公共库发行包
ECHO 复制公共库发行包...
@FOR /r "%CACHE_DIR%" %%i in (*.nupkg) DO (
    @COPY "%%i" "%PUBLISH_DIR%\nuget\utils"
    @COPY "%%i" "%PUBLISH_DIR%"
)

:: ============================================================================================
ECHO 清理临时目录...
rd /s /q %CACHE_DIR%

ECHO.
ECHO 发布过程完成，请检查！
PAUSE
GOTO :EOF

:: ===== 函数定义 =====

:CompileProjects
:: 编译指定目录下的所有项目
:: %~1: 显示的标题
:: %~2: 项目所在目录
ECHO %~1...
ECHO %~2...

IF NOT EXIST "%~2" (
    ECHO 错误: 目录 "%~2" 不存在，跳过编译
    GOTO :EOF
)

CD %~2
:: 遍历目录并排除EXCLUDE_DIRS中指定的目录
@FOR /f %%i in ('dir /b /ad') DO (
    SET "SKIP_DIR=0"
    
    :: 检查当前目录是否在排除列表中
    FOR %%j IN (%EXCLUDE_DIRS:;= %) DO (
        IF "%%i"=="%%j" (
            ECHO 跳过排除的目录: %%i
            SET "SKIP_DIR=1"
        )
    )
    
    :: 如果不在排除列表中，则执行编译
    IF !SKIP_DIR! EQU 0 (
        ECHO 正在编译: %%i
        dotnet publish %%i -c Release
        ECHO.
    )
)
GOTO :EOF