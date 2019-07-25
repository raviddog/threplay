Touhou Replay Manager README

https://github.com/raviddog/threplay

You need to have the .NET framework 4.6.1 or later installed, because that's the version that Visual Studio picked for me.
Attempting to open the program without it will prompt you to go to the download page.

How to use:
===========
Should be pretty self explanatory.
Select the game on the left, use the scroll wheel to scroll. Sometimes you get your mouse in the position where it keeps opening and closing the panel, nothing I can do about that unfortunately.
Click the folder icon next to the replay data section to show the directories, click each field to browse for either the game exe or each respective folder.
The program can auto-detect the appdata replay folders, or the older ones based off of the game exe location.
Replays from the game are shown on the left, replays in your backup folder are on the right.
All replays are shown, irrespective of which game they're from. Functions to clean up stuff like this are planned, but aren't currently implemented.

Click on a replay to show details. Click on the file name to rename it, press enter when done. Toggle the Move and Copy options to change how the Restore and Backup buttons work. Enabling Multiselect lets you move multiple replays at once, there's no Ctrl-click yet.

The date last modified is read from your score.dat files, click the Backup button under it to copy your score file to the backup directory. There's no restore button because the program isn't designed for that.

The big settings button in the bottom left allows you to toggle which games are visible, in case you only play some of them. There is also a function to select a master folder for all your backup folders to be created under.

You can drag and drop replays onto either the current or backup replay lists to copy them into the folders.

The program checks for updates silently at startup. If an update is found it will show an install button.