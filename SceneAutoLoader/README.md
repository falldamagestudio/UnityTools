# Scene Auto Loader

A small Unity Editor extension: temporarily switch to a specific scene when the user presses Play in the Editor.

This is a continuation of a script developed in [a Unity forum thread](http://forum.unity3d.com/threads/157502-Executing-first-scene-in-build-settings-when-pressing-play-button-in-editor).

## How to use

* Drop this into your Unity project. Some new menu options will appear under "Window > Scene Auto Loader".
* If you activate it and pick a master scene, it will switch to that scene whenever you press Play in the Editor
* When you return to Edit mode, the Editor UI will switch back to its original UI configuration

## Gotchas

Unity will first begin loading all the scenes that were open in the Editor, before switching to the specified scene.
All GameObjects/Components in those scenes will receive Awake/OnEnable/OnDisable/OnDestroy callbacks. You need to ensure that
all your scenes behave well if a scene is quickly loaded/unloaded like this if you want to use this script.

## Previous approaches

The former implementation attempted to load the specific scene while the Editor still was in Edit mode (just before it switched to Play mode).
This resulted in no GameObjects receiving unintended Awake/OnEnable/OnDisable/OnDestroy callbacks. On the other hand, the scene load operation
would invalidate most of the Editor UI state: it would forget which hierarchies were expanded in the Scene Hierarchy,
it would lose focus of any Inspectors, it would forget where the Scene View camera was located, etc.

Triggering a LoadScene early during Play mode ensures that the Editor UI is not affected but places stricter requirements on the
runtime components in the scenes.
