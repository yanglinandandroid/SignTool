
@ECHO OFF
Echo ********************SignTool Created By LKZ **************************
if exist "moons-xst-app-track.apk" (goto a) else (goto b)
:a
(Echo ��Ҫǩ�����ļ���moons-xst-app-track.apk��
 Echo ���ɵ�ǩ���ļ���moons-xst-app-track_signed.apk
 Echo ǩ���С���
 java -jar signapk.jar platform.x509.pem platform.pk8 moons-xst-app-track.apk moons-xst-app-track_signed.apk
 Echo ǩ�����
 Pause
EXIT
)
:b
Echo ��ǰĿ¼δ�ҵ���ǩ�����ļ��� moons-xst-app-track.apk
Pause
EXIT