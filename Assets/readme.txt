Summary
=======
This project is a "Bootcamp Demo" modification.
It shows how to add multiplayer functionality to an existing project using Exit Games' Photon technology. 
This bundle is compatible with our Photon Cloud Service and the Photon v3 Server SDK.


Notes
=======
- This project is not mobile compatible most likely. We only use Web and Standalone.
- This demo makes use of Exit Games' Photon Unity Client SDK. This is not the same as Photon Unity Networking.
- The client SDK can be downloaded here (registration required but free): 
  http://exitgames.com/download
- Feedback: 
  http://forum.unity3d.com/threads/131968-Photon-Bootcamp-Multiplayer-Demo


Hosting: Photon Cloud
=======
The Photon Cloud is a fully managed service at affordable prices. With it, you won't have to setup or update servers. 
Register for a free, one month trial. More about this service:
http://cloud.exitgames.com

To use the Photon Cloud: 
Register online, get your AppId and edit the field "Photon Cloud App Id" in the UsePhoton gameobject.


Hosting: Self Hosted
=======
To run your own server, download the latest v3 Photon Server SDK and a free license with it:
http://www.exitgames.com/Download/Photon
In PhotonControl, use "Photon InstanceLoadbalancing" to setup a service or run as application.


Setup
======
1) Import package into empty unity project
2) Open Unity3d scene BootCampLiteLobby.unity
3) Edit server setup in "UsePhoton" game object
	a) Register online, get your AppId and edit the field "Photon Cloud App Id"
	b) Enter your (self-hosted) server's address in "Custom Server Address" (leave the "Photon Cloud App Id" blank)
3) Optional: Setup your own Photon Server (requires Windows XP or later with .NET 3.5 SP1)
      a) Download the latest v3.0 Photon Server SDK and extract it parallel to \Assets folder (not into the Assets folder!)
         http://www.exitgames.com/Download/Photon
      b) Go to the extracted folder, find a the binaries-folder fitting your system and start PhotonControl.exe
      c) Right-click the PhotonControl tray icon and in "Photon InstanceLoadbalancing" setup a service or "run as application"
      d) Allow PhotonSocketServer.exe through the Windows Firewall. Detailed instructions:
         http://doc.exitgames.com/photon-server/FirewallSettings
   OR 
      follow instructions at http://doc.exitgames.com/photon-server/PhotonIn5Min
5) Press "Play" in Editor, or build and run


Game Controls
=============
WASD    - Movement
R       - Reload
1       - Machine Gun
2       - Granade Launcher
Shift   - Run
Ctrl    - Crouch
Space   - Jump
Enter   - Chat
Esc     - Menu / Exit Chat
Mouse 1 - Fire


Documentation
=============
http://doc.exitgames.com/photon-cloud/BCDemo_Objective
http://forum.unity3d.com/threads/131968-Photon-Bootcamp-Multiplayer-Demo


Known Issues
============
Currently none.


Authors
=======
Exit Games GmbH, Hongkongstr. 7, 20457 Hamburg, Germany
EU Headquarters: +49 40 413596-0
Exit Games Inc., One World Trade Center, 121 SW Salmon St, Portland, OR 97204, USA
US Headquarters: +1 503 383 9484
www.exitgames.com

King's Mound, Mina 1014, Barrio Antiguo, Monterrey, NL, México
T. + 52 818 340 4826
www.kingsmound.com


Questions?
==========
http://forum.exitgames.com
