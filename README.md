# tagpro-clone
An open source recreation of the popular online webgame Tagpro, using Unity, a free game engine, and uLink, a networking library for Unity. Play Tagpro at http://www.tagpro.gg/

Play the clone at (Need to be using unity web plugin and a compatible browser)

Windows Client:

Mac Client:

Android Client APK:


## Pre-requisties
1. Download and install the latest version of Unity3d. Get it for free at https://unity3d.com/
2. Download the latest **beta** of uLink and import the unity package when the project is opened with Unity. Get uLink here http://download.muchdifferent.com/ulink/uLink_1.6.0-beta39_Birgit_(2015-03-22).unitypackage (direct link)
3. Tiled2Unity (Included in project). Tiled2Unity is created by Sean Barton (https://github.com/Seanba/Tiled2Unity) and is used under the MIT license.
4. CNControls (Included in project). CNControls, "a Unity3D high performance mobile joystick" is used under the MIT license. https://github.com/KumoKairo/CNJoystick/

## Future Ideas

Apart from fixing bugs within the code that cause servers/clients to crash or gameplay to be messed up in some way, I will not be spending more time on tagpro-clone. However, tagpro-clone is still incomplete when compared to Tagpro. Below are some major features that are found in Tagpro but are missing from tagpro-clone.

* Support for webGL. Currently Unity supports webGL, though its much slower than the unity web plugin. However, uLink does not support webGL as of right now. MuchDifferent (the company behind uLink) had stated it will be webGL compatible by August 2015.

* Support for a tile system. The current process of creating maps in tagpro-clone requires significant manual actions - creating the maps in Tiled, exporting to Unity, manually attaching scripts, hardcoded spawn values, etc. Parsing of tagpro maps using the upcoming Unity2D tile system would be one of the main things I would focus on if I had more time.

* Player accessibility, persistent data management, private/public groups, account creation, player statistics, and automated game server initializations. Perhaps would be "easiest" if tagpro-clone was integrated into the full UnityPark Suite.
