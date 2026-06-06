; ============================================
; 微信自动化 - 排序小助手 NSIS 安装脚本
; 猪猪工作室出品
; ============================================

!include "MUI2.nsh"

; ---------- 基本信息 ----------
Name "微信自动化 - 排序小助手"
OutFile "C:\Users\Administrator\Desktop\微信自动化-排序小助手-v1.0.0-Setup.exe"
InstallDir "$PROGRAMFILES64\猪猪工作室\微信自动化"
InstallDirRegKey HKLM "Software\猪猪工作室\微信自动化" "InstallDir"
RequestExecutionLevel admin
Unicode True

; ---------- 版本信息 ----------
VIProductVersion "1.0.0.0"
VIAddVersionKey "ProductName" "微信自动化 - 排序小助手"
VIAddVersionKey "CompanyName" "猪猪工作室"
VIAddVersionKey "FileVersion" "1.0.0"
VIAddVersionKey "FileDescription" "基于图像识别的微信自动化工具"
VIAddVersionKey "LegalCopyright" "猪猪工作室"

; ---------- 图标 ----------
!define MUI_ICON "installer\app.ico"
!define MUI_UNICON "installer\app.ico"

; ---------- 界面 ----------
!define MUI_ABORTWARNING

; ---------- 页面 ----------
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

; ---------- 语言 ----------
!insertmacro MUI_LANGUAGE "SimpChinese"

; ---------- 安装区段 ----------
Section "安装" SecInstall
    SetOutPath "$INSTDIR"

    ; 主程序及所有依赖
    File /r "bin\publish\*.*"

    ; 模板文件（覆盖 publish 中可能没有的）
    SetOutPath "$INSTDIR\templates"
    File /nonfatal /r "installer\templates\*.png"

    ; 图标
    SetOutPath "$INSTDIR"
    File /nonfatal "installer\app.ico"

    ; 创建桌面快捷方式
    CreateShortCut "$DESKTOP\微信自动化.lnk" "$INSTDIR\WechatBot.exe" "" "$INSTDIR\app.ico" 0

    ; 创建开始菜单
    CreateDirectory "$SMPROGRAMS\猪猪工作室"
    CreateShortCut "$SMPROGRAMS\猪猪工作室\微信自动化.lnk" "$INSTDIR\WechatBot.exe" "" "$INSTDIR\app.ico" 0
    CreateShortCut "$SMPROGRAMS\猪猪工作室\卸载微信自动化.lnk" "$INSTDIR\uninstall.exe"

    ; 写入注册表（卸载信息）
    WriteRegStr HKLM "Software\猪猪工作室\微信自动化" "InstallDir" "$INSTDIR"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\微信自动化" "DisplayName" "微信自动化 - 排序小助手"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\微信自动化" "UninstallString" "$INSTDIR\uninstall.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\微信自动化" "DisplayIcon" "$INSTDIR\app.ico"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\微信自动化" "Publisher" "猪猪工作室"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\微信自动化" "DisplayVersion" "1.0.0"

    ; 写入卸载程序
    WriteUninstaller "$INSTDIR\uninstall.exe"
SectionEnd

; ---------- 卸载区段 ----------
Section "Uninstall"
    ; 结束进程
    nsExec::ExecToLog 'taskkill /f /im WechatBot.exe'

    ; 删除所有文件
    RMDir /r "$INSTDIR"
    RMDir "$PROGRAMFILES64\猪猪工作室"

    ; 删除快捷方式
    Delete "$DESKTOP\微信自动化.lnk"
    RMDir /r "$SMPROGRAMS\猪猪工作室"

    ; 删除注册表
    DeleteRegKey HKLM "Software\猪猪工作室\微信自动化"
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\微信自动化"
SectionEnd
