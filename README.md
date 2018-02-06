# README #

This document details how to get it up and running on your computer and start modifying the code!

### What license is the UI system shipped under? ###
Please see the LICENSE file.

### How do I get started? ###
* Clone this repository onto a location on your computer.
* Configure your IDE for the Unity coding standard, look in the .editorconfig file for more information
* Open the project in Visual Studio or MonoDevelop
    * If you are using MonoDevelop
        * Ensure you enable XBuild (Preferences -> Projects -> Build ->"Compile projects using MSBuild/XBuild")
        * You may need to restart MonoDevelop
    * Build the solution

* A folder will be created in the root directory called "Output", the generated dll's will output here in the correct folder structure
    * If you wish to use these dll's
    * Locate your Unity install location
    * Windows: Copy the contents of Output folder to: `Data\UnityExtensions\Unity\GUISystem\{UNITY_VERSION}`
    * OSX: Copy the contents of Output folder to: `Unity.app/Contents/UnityExtensions/Unity/GUISystem/{UNITY_VERSION}`

* If you want the dll's to copy automatically on build
    * For each visual studio project file
        * Open the file in a text editor
        * Locate the section: <Target Name="AfterBuild">
        * Follow the instructions in the comments

### Will you be taking pull requests? ###
At this stage the UI system is undergoing further development internally with many design decisions still being debated and implemented. Also, we are waiting to see the volume of pull requests and the time it will take the process them. As such, we are prioritizing first bug fix pull requests and will iterate on this process going forward.
