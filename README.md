# HoloArt

## Build Settings
The following settings worked with Unity Version: 2018.4.24f1:
 1. Go to File > Build Settings.
 2. Target Device: HoloLens 
 3. Architecture: x86
 4. Build Type: D3D
 5. Target SDK Version: Latest installed
 6. Minimum Platform Version: 10.0.10240.0
 7. Visual Studio Version: Visual Studio 2019
 8. Build and Run on: Local Machine
 9. Build configuration: Release
 10. Copy References: not checked
 11. Unity C# Projects: not checked
 12. Development Method: default

## Build Instructions

 1. Go to File > Build Settings
 2. Click Build
 3. The Unity editor will open a dialog box asking where you want the project to built be built. Select a folder. I use a folder named 'VS' in the Unity project directory (HoloArt/VS).
 4. Open the solution file (HoloArt/VS/HoloArt.sln) in Visual Studio 2019 (Community Edition).
 5. Set the Solutions Platforms to x86.
 6. Optional: Double click on Package.appxmanifest in the Solution Explorer, switch to the Capabilities tab, and check that the following are enabled:
    - Gaze Input
    - Internet (Client)
    - Microphone
    - Spatial Perception
    - Webcam
7. Build the solution: Build > Build Solution.
8. Deploy to Remote Machine

> **Note: To set the deployment target to HoloLens:**

> - Ensure that the HoloLens is switched on.
> - Right click the HoloArt project in the Solution Explorer.
> - Select Properties.
> - Go to Debugging.
> - Select Machine and then click Locate from the dropdown menu.
> - Select the HoloLens device
    
## To Do List
- [ ] Tidy up source code.
- [ ] Document source code.
- [x] TextureScale class is currently disabled. This is called by DisplayPanel.DownloadThumbnailImage(). TextureScale will not compile when the Scripting Backend (PlayerSettings > Other Settings) is set to .NET. Could try switching to IL2CPP or find some other way to rescale the thumbnail image.
- [x] Create a close box for the holographic display panel. Look at examples from HoloToolkit. It should be possible to close a display panel by selecting the close box.
- [x] Display panels are closed using a "Close" voice command, which invokes AppManager.CloseWindows(). Consider destroying the display panel game object rather than setting the game object to false.
- [x] AppManager.CloseWindows() uses GameObject.FindGameObjectsWithTag(), which is resource intensive. Implement alternative solution to manage open display panels.
- [ ] ImageCapture.GetImage() attempts to calculate a target position for the display position (this.CalculateDisplayPanelPosition()). Exceptions are thrown when this code is run, so the routine is disabled. Investigate the source of the exceptions and attempt to resolve.  
