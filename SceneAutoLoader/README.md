Scene Auto Loader

A small Unity Editor extension: temporarily switch to a different scene when the user presses Play in the Editor.

This is a continuation of a script developed in [a Unity forum thread](http://forum.unity3d.com/threads/157502-Executing-first-scene-in-build-settings-when-pressing-play-button-in-editor).

This works well:

* Switches to another scene when the user presses Play
* Restores the original list of scenes in the Scene Hierarchy View, including not loaded / loaded / active status

This works not so well:

* All scenes in the hierarchy view will be in collapsed state.
* Any focus (selected object in scene view, inspector focus), ... will be lost.
* Not sure but I suspect that the current camera view in the scene view is lost as well.

Ideas

* Add a small ScriptableObject to each scene with the autoloader settings. Create a custom inspector for it. Make the autoloader pick up the scene to load from there. This setting would then be shared between all developers on the team. It would be lower priority than the per-user global override which the SceneAutoLoaderEditorWindow / menu options provide.
* See if there is a way to trigger the scene change after Unity has serialized the current editor state. Perhaps that will make Unity restore the entire UI state on its own when exiting play mode?
