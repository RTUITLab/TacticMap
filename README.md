# TacticMap #

***
##### What was used: #####
***
  - [![Unity Version](https://img.shields.io/badge/unity-2018.4.21-blue.svg)](https://unity3d.com/get-unity/download)
  - [![MRTK Version](https://img.shields.io/badge/Microsoft-MRTK%202.4.0-green)](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.4.0)
  - Hololens 1st gen.
  - Magic.

##### Instruction: #####
***
- Open the project in Unity. 
- Configurete Photon settings.
>Windows > Photon Unity Networking > Highlight Server Settings (Ctrl + Shift + Alt + P).
>If the photon server will be used, then you need to enter App id realtime and check the "Use name server" box. Else uncheck box and insert ip into "Server" and default port 5055.
- Bild visual studio solution.
- Deploy on Hololens.
> If you get "Windows Mobile" error while building your project, in the solution folder, open HoloMapOnline> HoloMapOnline.vcxproj. At the end of the file, cut out the mention of Windows Mobile.


##### Photon server Instructoins: #####
***
- Get ur server [license here.](https://dashboard.photonengine.com/en-US/selfhosted)
- Download [server here.](https://www.photonengine.com/en-us/sdks#server-sdkserverserver)
- Move license to deploy/bin_Win64/
- Open PhotonControl.exe.
- Start ur Photon On Premises Server.
