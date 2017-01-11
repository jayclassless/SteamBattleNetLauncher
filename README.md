# Steam Battle.Net Launcher

This application allows you to launch Battle.Net games through Steam so that:
- Your friends can see in your status what game you're playing.
- You can receive Steam notification while playing.
- You can access the Steam overlay so you can chat with your friends, etc.

## Requirements
- Windows
- .NET runtime v4.5+
- Battle.Net
- Steam

## Installation
1. Download the most recent release of this application from this repository's
   [Releases](https://github.com/jayclassless/SteamBattleNetLauncher/releases).
2. Unzip the file you downloaded and put its contents in a directory on your computer. E.g.: `C:\SBNL`
3. In Steam, go to the "Games" menu and choose "Add a Non-Steam Game to My Library".
4. In the window that pops up, click "Browse".
5. Navigate to the directory you unzipped this application into, and choose the file "SteamBattleNetLauncher.exe".
6. Click the "Add Selected Programs" button.
7. In your Steam Library, a new game should appear called "SteamBattleNetLauncher". Right click on it and choose "Properties".
8. Change the name from "SteamBattleNetLauncher" to whatever game you intend to launch. E.g.: `Overwatch`
9. In the "Target" box, add a space and one of the codes below to the end of the path that is already in the box.
   Following the examples in these instructions, to launch Overwatch, the "Target" box would contain:
   `"C:\SBNL\SteamBattleNetLauncher.exe" pro`
10. Click "Close".
11. You're done! Launch your game through Steam like you would any other.

## Caveats
This was slapped together rather quick based on various Visual Basic Scripts I've seen floating around the internet. It's
likely buggy, so any bug reports or patches you can provide to improve this application, the better! See the following
table for the current known support for the launcher.

## Games
Game | Code | Launcher Support
---- | ---- | ----------------
Diablo 3 | D3 | Game launches, Status updated, Overlay works
Heroes of the Storm | HERO | Game launches, No Status, No Overlay
Overwatch | PRO | Game launches, Status updated, Overlay works
Starcraft 2 | S2 | Untested
World of Warcraft | WOW | Untested
Hearthstone | WTCG | Game launches, Status updated, Overlay works

## License and Copyrights
The code in this repository is released under the MIT license.

The Battle.Net name and logo are owned/copyrighted/trademarked by [Blizzard Entertainment](http://www.blizzard.com).

The Steam name and logo are owned/copyrighted/trademarked by [Valve Corporation](http://www.valvesoftware.com).
