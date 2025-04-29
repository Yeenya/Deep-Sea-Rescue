Github repository of my master thesis - Audio-based navigation for visually impaired play in open-world games. Created by Jan Borecký at the Faculty of Mathemathics and Physics, Charles University in Prague.

Clone or download the repository / attachment to your computer. If you want to only play the Windows build of the game, navigate to the folder containing the repository and open the Builds folder. Inside the folder is a folder named Standard. Open it and you will see two folders and three files, one of them being Deep Sea Rescue.exe. Click on this file and start playing. In order to open the Unity project itself, you need to have Unity Editor version 2022.3.8f1 installed. This version can be installed in the Unity Hub app which can be downloaded here: https://unity.com/download Newer subversions of the 2022 editor should still work, but I cannot guarantee that. Open Unity Hub and click on the Add button in the upper right corner. Choose Add project from disk and locate the directory with the downloaded repository. Select the folder and the Deep Sea Rescue project will appear in the Projects list. Click on the project and wait for the Unity Editor to load up. There are two scenes in the Scenes folder. MainScene is where the game is happening. DataScene is a scene that is used only for data processing. If you want to play the game in the Unity Editor, open the MainScene and hit the play button at the top of the editor window. If you want to build the game, click on the File button in the left top corner and select Build Settings. In the Build Settings window, check that only MainScene is contained in the Scenes In Build list and select your preferred platform. The game was developed on Windows and is meant to be played on Windows. If you need a different target platform, add the platform support to your version of Unity Engine. In Unity Hub, click on Installs on the left), locate the correct version, and click on the settings wheel on the right. Choose Add modules and add build support for any platform you want. Keep in mind that the game is not meant to be played on mobile phones due to missing screen controls.)

Controls:
• W - Move forward
• S - Move backward
• A - Turn left
• D - Turn right
• Left Shift - Tilt up
• Left Control - Tilt down
• Left / Right arrow keys - Reset tilt to neutral orientation
• Space - Dock / Undock
• R - Attempt to rescue a nearby diver
• E - Turn on / off the base sound signal
• F - Turn on / off the front light
• C - Turn on / off the left light
• V - Turn on / off the right light
• Tab - Switch the camera view between the first-person cabin view and third-person view behind the submarine
• Mouse - Rotate the first-person view in the cabin
• Escape - Pause the game and open the pause menu
• + - Increase replay speed
• - - Decrease replay speed
