
@ECHO OFF
Echo ********************SignTool Created By LKZ **************************
if exist "moons-xst-app-track.apk" (goto a) else (goto b)
:a
(Echo 将要签名的文件：moons-xst-app-track.apk，
 Echo 生成的签名文件：moons-xst-app-track_signed.apk
 Echo 签名中……
 java -jar signapk.jar platform.x509.pem platform.pk8 moons-xst-app-track.apk moons-xst-app-track_signed.apk
 Echo 签名完成
 Pause
EXIT
)
:b
Echo 当前目录未找到被签名的文件： moons-xst-app-track.apk
Pause
EXIT