# Player tags

Tags are similar to launch arguments for Players (both the main Editor Player and Virtual Players) that you can use to configure Players to behave in a specific way. For example:

- Automatically run as a member of a specific team (for example, “Red Team” or “Blue Team”)
- Move faster or slower to simulate their network connection

You can assign multiple tags to a Player.


## To create a tag
To create a tag, perform the following actions:

1. To open the Multiplayer Play mode window, navigate to **Window** > **Multiplayer Play Mode**.
2. Expand the Tags dropdown for a Player or the main Editor Player.
3. Select ![plus sign](../images/add.png) Create Tag.
4. Name the tag.
5. Select **Save**. The new tag is automatically added to the local project directory in `...\[example-project-name]\ProjectSettings\VirtualProjectsConfig.json`.
6. The new tag also appears under the **Player Tags** section of the **Multiplayer Play Mode** window and in the dropdown menu for the **Tag** option of each **Player**.

## To Attach tags to a Player

To assign one or more [tags](player-tags.md) to any Player, do the following:

1. Open the Multiplayer Play mode window (**Window** > **Multiplayer Play Mode**).
2. Expand the **Tags** dropdown for a Player or the main Editor Player.
3. Select **+ Create Tag**.
4. In the Project Settings window that appears, select the **Add** (**+**) icon.
5. In the New tag field that appears, type a name for your tag.
6. Select **Save**.
7. In the Multiplayer Play Mode window, expand the **Tags** dropdown.
8. Select the tag you just created.

## To use tags in a player script

Tags don't do anything until you configure them. To configure a tag, do the following:

1. Use `CurrentPlayer.ReadOnlyTags()` to target the tag in a script.
2. Attach the script to a [NetworkObject](https://docs-multiplayer.unity3d.com/netcode/current/basics/networkobject/), for example, the **Player**.

Use the following tag examples as guides for your own scripts:

- [To automatically assign a **Player** to a team](target-team.md)
- [To simulate network conditions](target-network.md)