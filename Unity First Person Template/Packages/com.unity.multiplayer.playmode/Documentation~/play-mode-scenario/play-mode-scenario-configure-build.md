# Configure and build local and remote instances

When you set up a Play Mode Scenario, you can create the following types of instances:

* Editor
* Local
  * Local
  * Cloud simulated
* Remote

## Configure and build a remote instance

A remote instance exists in the [Unity Cloud](https://docs.unity.com/cloud/en-us) and uses [Unity Gaming Services (UGS) Multiplay Hosting](https://docs.unity.com/ugs/en-us/manual/game-server-hosting/manual/welcome-to-multiplay).

You can only have one remote instance in a Play Mode Scenario and it can only run on a Linux platform.

To learn more about the properties that you can use to configure a remote instance, refer to [Play Mode Scenarios window reference](../mppm-reference/play-mode-scenario-window-reference.md).

**tip Multiplay Hosting cost**
You can start using Multiplay Hosting for free, but it can cost money to use a server even when it's not running a process or hosting any users. To reduce cost in a project that uses Multiplay Hosting, open the [Unity Cloud Dashboard](http://cloud.unity.com/) and reduce the **Minimum Available Scaling** property value to 0. To learn more about Multiplay Hosting pricing, refer to [UGS pricing](https://unity.com/products/gaming-services/pricing).

### Requirements for remote instances

Before you can build a remote instance in a Play Mode Scenario, your project must meet the following requirements:

* Your project must be connected to a cloud project. You can set this in **Edit** > **Project Settings** > **Services** by selecting your organization and project.
* Multiplay Hosting must be enabled in the project dashboard. Once your project is connected to a cloud project ID, a link to the project dashboard will appear in **Edit** > **Project Settings** > **Services**.
* An environment must be selected under **Edit** > **Project Settings** > **Services** > **Environment**.

## Configure and build a local instance

### Local instance

A local instance can run on the same platform as the Unity Editor, or on an Android device that is connected to that platform wirelessly or by a USB cable. To build a local instance that you have configured in the Play Mode Scenarios window, [enter Play Mode](https://docs.unity3d.com/Manual/GameView.html). All configured Play Mode Scenarios build automatically when you enter Play Mode.

### Local cloud simulated instance
A local cloud simulated instance runs on the same platform as the Unity Editor, but simulates a remote instance that runs in the Unity Cloud. To build a local cloud simulated instance that you have configured in the Play Mode Scenarios window, [enter Play Mode](https://docs.unity3d.com/Manual/GameView.html). All configured Play Mode Scenarios build automatically when you enter Play Mode.
#### Requirements for local cloud simulated instances
Before you can build a local cloud simulated instance in a Play Mode Scenario, your project must meet the following requirements:
* Your project must be connected to a cloud project. You can set this in **Edit** > **Project Settings** > **Services** by selecting your organization and project.
* An environment must be selected under **Edit** > **Project Settings** > **Services** > **Environment**.

To learn more about the properties that you can use to configure a local instance, refer to [Play Mode Scenarios window reference](../mppm-reference/play-mode-scenario-window-reference.md).

## Check the status of all instances
The Play Mode status window displays the following information for each Play Mode Scenario:
* Status
* Port
* Address
* Errors

To open the Play Mode status window:

1. Enter Play Mode.
2. Expand the Play Mode status dropdown.
3. Select the status window icon.

## Additional resources
* [Play Mode Scenario window reference](../mppm-reference/play-mode-scenario-window-reference.md)
* [Play Mode Scenarios requirements and limitations](play-mode-scenario-req.md)
* [Create a Play Mode Scenario](play-mode-scenario-create.md)
* [Unity Gaming Services (UGS) Multiplay Hosting](https://docs.unity.com/ugs/en-us/manual/game-server-hosting/manual/welcome-to-multiplay)
* [Troubleshoot a test build](../troubleshoot/play-mode-scenario-troubleshoot.md)
