@echo off
REM This is called as a Post Build Event by Visual Studio in HintTech.eXtensions
REM Copy from HintTech.eXtensions in ASP.NET source
xcopy "C:\Source Code\HintTech.eXtensions\HintTech.eXtensions\App_CodeFolder\*.cs" "C:\Tridion\eXtensions\WordPress Publishing\WPP.Editor\App_CodeFolder" /D /Q /Y
xcopy "C:\Source Code\HintTech.eXtensions\HintTech.eXtensions\Configuration\*.config" "C:\Tridion\eXtensions\WordPress Publishing\WPP.Editor\Configuration" /D /Q /Y
xcopy "C:\Source Code\HintTech.eXtensions\HintTech.eXtensions\Images\*.*" "C:\Tridion\eXtensions\WordPress Publishing\WPP.Editor\Images" /D /Q /Y
xcopy "C:\Source Code\HintTech.eXtensions\HintTech.eXtensions\Scripts\*.js" "C:\Tridion\eXtensions\WordPress Publishing\WPP.Editor\Scripts" /D /Q /Y
xcopy "C:\Source Code\HintTech.eXtensions\HintTech.eXtensions\Styles\*.css" "C:\Tridion\eXtensions\WordPress Publishing\WPP.Editor\Styles" /D /Q /Y
xcopy "C:\Source Code\HintTech.eXtensions\HintTech.eXtensions\Web.*" "C:\Tridion\eXtensions\WordPress Publishing\WPP.Editor" /D /Q /Y
xcopy "C:\Source Code\HintTech.eXtensions\HintTech.eXtensions\WordPressPublish.*" "C:\Tridion\eXtensions\WordPress Publishing\WPP.Editor" /D /Q /Y
xcopy "C:\Source Code\HintTech.eXtensions\HintTech.eXtensions\bin\*.dll" "C:\Tridion\web\WebUI\WebRoot\bin" /D /Q /Y
REM Exit with a zero return code, so Visual Studio can report success on build
exit /b 0