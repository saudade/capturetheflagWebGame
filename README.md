# tagpro-clone
An open source recreation of the popular online webgame Tagpro, using Unity, a free game engine, and uLink, a networking library for Unity. Play Tagpro at http://www.tagpro.gg/

Play the clone at http://saudade.github.io/tagpro-clone/releaseWeb.html (Need to have unity web plugin and be using a compatible browser)

Controls: Arrow keys to move. Hit enter to chat. F or space to honk. F9 to pause game (if server allows clients to pause game). Tab to show/hide network debug information. ESC to show/hide audio icon that when pressed allows you to play/mute in game audio.

Windows Client: https://github.com/saudade/tagpro-clone/releases/download/v0.1.0-windows/windowsTagpro.rar

Mac Client: https://github.com/saudade/tagpro-clone/releases/download/v0.1.0-mac/macTagpro.rar

Android Client APK (with on-screen joystick): https://github.com/saudade/tagpro-clone/blob/master/debugBuilds/clientAndroidAlpha.apk?raw=true

Windows Server: https://github.com/saudade/tagpro-clone/releases/download/v0.1.0-windows/windowsTagpro.rar
(Must have port 52602 open on your router and unblocked by firewall for other players to join. If you don't, no one will be able to see the server or join it, apart from LAN.)

## Prerequisites
1. Download and install the latest version of Unity3d. Get it for free at https://unity3d.com/
2. Download the latest **beta** of uLink and import the unity package when the project is opened with Unity. Get uLink here http://download.muchdifferent.com/ulink/uLink_1.6.0-beta39_Birgit_(2015-03-22).unitypackage (direct link)
3. Tiled2Unity (Included in project). Tiled2Unity is created by Sean Barton (https://github.com/Seanba/Tiled2Unity) and is used under the MIT license.
4. CNControls (Included in project). CNControls, "a Unity3D high performance mobile joystick" is used under the MIT license. https://github.com/KumoKairo/CNJoystick/


## Features/Changes
* Game Mode: It's real capture the flag. The one where if you pass the middle line and get tagged, you go to jail. The one with a safe zone in the flag area. Here's an image of a modified The Holy See map that better explains how this game mode plays. http://i.imgur.com/G4Rmgyi.png?1

* Powerups: Only rolling bomb and juke juice. Wouldn't really make sense to have tagpro in a real capture the flag game mode. Also, a different mechanism of picking up powerups, if two players are on top of a powerup when it spawns, one player will need to obtain at least 50% of the powerup area to pick it up. If only one player spawns on top of powerup, he will automatically pick it up.

* Pausing: If a server has pausing enabled (the one I'm hosting does), you can pause the game by pressing F9. A chat message and ping sound will be sent to everyone and three seconds later the game will be paused (no one can move, everything is still.) Anyone can press F9 again to unpause the game. Each player get 3 pauses per game right now.

* Honking: Added by default, press space or F to activate. Some keyboards will not register the keys if you have space and 2 other arrow keys held down b/c of how they're wired, so I added F as a honk key as well.

* Replays: Still a work in progress, but I'm aiming for the system in DOTA 2 where the server automatically saves a replay of the whole game and allows you to download and view it.

## Future Ideas

Apart from fixing bugs within the code that cause servers/clients to crash or gameplay to be messed up in some way, I will not be spending more time on tagpro-clone. However, tagpro-clone is still incomplete when compared to Tagpro. Below are some major features that are found in Tagpro but are missing from tagpro-clone.

* Support for webGL. Currently Unity supports webGL, though its much slower than the unity web plugin. However, uLink does not support webGL as of right now. MuchDifferent (the company behind uLink) had stated it will be webGL compatible by August 2015.

* Support for a tile system. The current process of creating maps in tagpro-clone requires significant manual actions - creating the maps in Tiled, exporting to Unity, manually attaching scripts, hardcoded spawn values, etc. Parsing of tagpro maps using the upcoming Unity2D tile system would be one of the main things I would focus on if I had more time.

* Player accessibility, persistent data management, private/public groups, account creation, player statistics, and automated game server initializations. Perhaps would be "easiest" if tagpro-clone was integrated into the full UnityPark Suite.


## License

GPLv3
