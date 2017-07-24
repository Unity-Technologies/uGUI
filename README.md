# README #

The Unity UI system is open and available for use in your games and applications. This document details how to get it up and running on your computer and start modifying the code!

### What license is the UI system shipped under? ###
The UI system is released under an MIT/X11 license; see the LICENSE file.

This means that you pretty much can customize and embed it in any software under any license without any other constraints than preserving the copyright and license information while adding your own copyright and license information.

You can keep the source to yourself or share your customized version under the same MIT license or a compatible license.

If you want to contribute patches back, please keep it under the unmodified MIT license so it can be integrated in future versions and shared under the same license.

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
        * {UNITY_VERSION} is either Editor or Standalone. Editor version is used inside the Editor where Standalone is used by ALL players.

* If you want the dll's to copy automatically on build
    * For each visual studio project file
        * Open the file in a text editor
        * Locate the section: <Target Name="AfterBuild">
        * Follow the instructions in the comments

### Unity is at version X but the repo is only at version Y why isn't it updated already? ###

Unity's release schedule doesn't always happen at convenient times for us to drop everything and update our public repo. We will try to always have the repo updated within a week of a public release but sometimes things happen and more time is needed. Please note that we will NOT be updating the repo for patch releases due to the time it takes to update the repo at this time.

### Will you be taking pull requests? ###
At this stage the UI system is undergoing further development internally with many design decisions still being debated and implemented. We will be accepting pull requests when the team has time to process them. Due to the current set up where development actually happens on a internal repo, changes are pushed in a batch when public releases are done. Whether a pull request is accepted or not will be declined locally with a comment of Accepted. This makes for less merge conflicted during the batch pushes as well as ensuring a fix will make it out in the official unity release for everyone.