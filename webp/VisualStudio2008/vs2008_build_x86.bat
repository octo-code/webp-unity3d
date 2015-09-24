%comspec% /k "C:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\vcvarsall.bat" x86 < "
cd ..\libwebp\
"C:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\bin\nmake.exe" /f Makefile.vc CFG=release-dynamic RTLIBCFG=dynamic ARCH=x86 OBJDIR=output
@pause
"