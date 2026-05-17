# About Multiplayer Play Mode

Multiplayer Play Mode allows you to test multiplayer functionalities across distinct Players without leaving your development environment. This package enables quick creation of complex Play Mode scenarios by simulating multiple Editor instances, and adding further local and remote instances simultaneously. This significantly accelerates iteration speed, allowing for faster development and testing of multiplayer features.

## Compatibility

Multiplayer Play Mode version 1.6.0 is compatible with the following:

* Unity Editor versions 6000.0.50f1 or later.
* Windows and MacOS platforms.

# Technical details

## Requirements

This version of Multiplayer Play Mode is compatible with the following Unity versions and platforms:

* Unity 6 and later
* Windows, Mac platforms

## Limitations

Multiplayer Play Mode has some inherent technical limitations, specifically around [scale](#scale) and [authoring](#authoring).

### Scale

The Unity Editor and Editor Instances require a significant amount of system resources, so you shouldn't use Multiplayer Play Mode at scale. Multiplayer Play Mode is designed for small-scale, local testing environments that can only support up to four total Players (the main Editor Player and three Virtual Players).

### Authoring
Editor instances are essentially behaving as ligthweight editor instances. To maximise their performances, they are kept in synch with the main editor and have a lesser amount of abilities. Specifically:
- You can't create or change the properties of GameObjects in an Editor Instance. Instead, use the main Editor Player to make changes and an Editor instance to test multiplayer functionality. Any changes you make in Play Mode in the main Editor Player reset when you exit Play Mode.
- You can't access any main Editor Player functionality from Virtual Player.
- The package manager is non functional in Editor Instances.
