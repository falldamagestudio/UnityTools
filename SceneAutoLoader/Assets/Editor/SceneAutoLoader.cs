using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

//#define SCENE_AUTO_LOADER_DEBUGGING

/// <summary>
/// Scene auto loader.
/// </summary>
/// <description>

/// This class adds a Window > Scene Auto Loader menu containing options to select
/// a "master scene" enable it to be auto-loaded when the user presses play
/// in the editor. When enabled, the selected scene will be loaded on play,
/// then the original scene will be reloaded on stop.
///
/// Based on an idea on this thread:
/// http://forum.unity3d.com/threads/157502-Executing-first-scene-in-build-settings-when-pressing-play-button-in-editor
/// </description>
[InitializeOnLoad]
public static class SceneAutoLoader
{
    // Static constructor binds a playmode-changed callback.
    // [InitializeOnLoad] above makes sure this gets execusted.
    static SceneAutoLoader()
    {
        EditorApplication.playmodeStateChanged += OnPlayModeChanged;
    }

    // Menu items to select the "master" scene and control whether or not to load it.
    [MenuItem("Window/Scene Auto Loader/Select Master Scene...")]
    private static void SelectMasterScene()
    {
        string masterScene = EditorUtility.OpenFilePanel("Select Master Scene", Application.dataPath, "unity");
        if (!string.IsNullOrEmpty(masterScene))
        {
            MasterScene = masterScene;
            LoadMasterOnPlay = true;
        }
    }

    [MenuItem("Window/Scene Auto Loader/Load Master On Play", true)]
    private static bool ShowLoadMasterOnPlay()
    {
        return !LoadMasterOnPlay;
    }
    [MenuItem("Window/Scene Auto Loader/Load Master On Play")]
    private static void EnableLoadMasterOnPlay()
    {
        LoadMasterOnPlay = true;
    }

    [MenuItem("Window/Scene Auto Loader/Don't Load Master On Play", true)]
    private static bool ShowDontLoadMasterOnPlay()
    {
        return LoadMasterOnPlay;
    }
    [MenuItem("Window/Scene Auto Loader/Don't Load Master On Play")]
    private static void DisableLoadMasterOnPlay()
    {
        LoadMasterOnPlay = false;
    }

    // Play mode change callback handles the scene load/reload.
    private static void OnPlayModeChanged()
    {
        if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // User pressed play

#if SCENE_AUTO_LOADER_DEBUGGING
            Debug.Log("Capturing old scene setup...");
#endif
            // Check if master scene is part of the current project
            // If the user switches between projects, the persistet "master scene" setting could potentially
            //   point to a scene that exists within another project but not this one
            bool masterSceneExistsInProject = (AssetDatabase.AssetPathToGUID(MasterScene) != "");

            // Store an empty list of scenes by default
            ScenesInHierarchyView = new SceneSetup[] { };

            // Only proceed if scene loading is active and the master scene exists in the current project
            if (LoadMasterOnPlay && masterSceneExistsInProject)
            {
                // Save current unsaved scenes if necessary
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // Persist the current list of scenes in the hierarchy view including their status (not loaded, loaded, active).
                    ScenesInHierarchyView = EditorSceneManager.GetSceneManagerSetup();

                    // User has no unsaved changes at this point; switch to master scene
                    Scene scene = EditorSceneManager.OpenScene(MasterScene, OpenSceneMode.Single);

                    // If scene switching failed, then cancel play and switch back to the previous set of scenes
                    if (!scene.IsValid())
                    {
                        EditorApplication.isPlaying = false;
                        EditorApplication.update += ReloadLastScene;
                    }
                }
                else
                {
                    // User cancelled the save operation -- cancel play as well.
                    EditorApplication.isPlaying = false;
                }
            }
        }
        if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // User pressed stop

            if (ScenesInHierarchyView.Length != 0)
            {
                // There is a list of previous scenes available; reload those
                EditorApplication.update += ReloadLastScene;
            }
        }
    }

    public static void ReloadLastScene()
    {
        if (EditorApplication.isPlaying)
            return;

#if SCENE_AUTO_LOADER_DEBUGGING
        Debug.Log("Reloading editor scene setup...");
#endif
        SceneSetup[] scenes = ScenesInHierarchyView;

        // Workaround for Unity 5.5.0f3 bug (and possibly in newer versions as well):
        //
        // If the first loaded scene in scenes is not active, then RestoreSceneManagerSetup() will not load that scene.
        // We work around this by ensuring that RestoreSceneManagerSetup() is given a list of scenes where the first loaded scene
        //  always is active, and if necessary switch the active scene to the appropriate one after the call.

        int activeSceneIndex = Array.FindIndex(scenes, scene => scene.isActive);
        int firstLoadedSceneIndex = Array.FindIndex(scenes, scene => scene.isLoaded);

        if (activeSceneIndex != firstLoadedSceneIndex)
        {
            scenes[firstLoadedSceneIndex].isActive = true;
            scenes[activeSceneIndex].isActive = false;
        }

        // Restore old scene list in Hierarchy view - not-loaded, loaded, and active scenes

        EditorSceneManager.RestoreSceneManagerSetup(scenes);

        // Workaround part two: switch to the appropriate active scene

        if (activeSceneIndex != firstLoadedSceneIndex)
            EditorSceneManager.SetActiveScene(EditorSceneManager.GetSceneAt(activeSceneIndex));


        ScenesInHierarchyView = new SceneSetup[] { };

        EditorApplication.update -= ReloadLastScene; ;
    }

    // Properties are remembered as editor preferences.
    private static string cEditorPrefLoadMasterOnPlay { get { return "SceneAutoLoader." + PlayerSettings.productName + ".LoadMasterOnPlay"; } }
    private static string cEditorPrefMasterScene { get { return "SceneAutoLoader." + PlayerSettings.productName + ".MasterScene"; } }
    private static string cEditorPrefLoadedScenes { get { return "SceneAutoLoader." + PlayerSettings.productName + ".LoadedScenes"; } }

    public static bool LoadMasterOnPlay
    {
        get { return EditorPrefs.GetBool(cEditorPrefLoadMasterOnPlay, false); }
        set { EditorPrefs.SetBool(cEditorPrefLoadMasterOnPlay, value); }
    }

    public static string MasterScene
    {
        get { return EditorPrefs.GetString(cEditorPrefMasterScene, "Master.unity"); }
        set { EditorPrefs.SetString(cEditorPrefMasterScene, value); }
    }

    public static SceneSetup[] ScenesInHierarchyView
    {
        get
        {
            string prefValue = EditorPrefs.GetString(cEditorPrefLoadedScenes, "");

            string[] tokens = prefValue.Split('|');

            int numScenes = tokens.Length / 3;

            SceneSetup[] scenes = new SceneSetup[numScenes];

            for (int i = 0; i < tokens.Length / 3; i++)
            {
                scenes[i] = new SceneSetup();
                scenes[i].isActive = (tokens[i * 3 + 0] != "false");
                scenes[i].isLoaded = (tokens[i * 3 + 1] != "false");
                scenes[i].path = tokens[i * 3 + 2];
            }
          
            return scenes;
        }

        set
        {
            string prefValue = string.Join("|", value.Select(scene => (scene.isActive ? "true" : "false") + "|" + (scene.isLoaded ? "true" : "false") + "|" + scene.path).ToArray());

            EditorPrefs.SetString(cEditorPrefLoadedScenes, prefValue);
        }
    }
}
