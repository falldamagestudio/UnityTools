using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene auto loader.
/// </summary>
/// <description>
/// This class adds a Window > Scene Auto Loader menu containing options to select
/// a "master scene" enable it to be auto-loaded when the user presses play
/// in the editor.
/// 
/// When enabled, the selected scene will be loaded on play; stopping play will however return you to the original editor scene(s).
///
/// The scene loading is triggered after play mode has begun. This results in all game objects in the scenes you had loaded in the editor
/// receiving the following callbacks:
///   Awake
///   OnEnable
///   OnDisable
///   OnDestroy
/// To be compatible with this script you should ensure that all scenes in your game support the above flow without any strange side effects.
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

	/// <summary>
	/// Play mode change callback detects when user presses Play, and schedules a scene load request.
	/// </summary>
	private static void OnPlayModeChanged()
	{
		if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
		{
			// User pressed Play, and the editor is about to enter Play mode

			// Schedule a load of the master scene, if SceneAutoLoader is active and the user has chosen a scene
			if (LoadMasterOnPlay && MasterScene != "")
				SceneLoadRequested = true;
		}
	}

	/// <summary>
	/// Load the desired scene, if requested.
	/// BeforeSceneLoad runtime callback is triggered after the editor has entered Play mode.
	/// </summary>
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void OnBeforeSceneLoadRuntimeMethod()
	{
		if (SceneLoadRequested)
		{
			SceneLoadRequested = false;

			// This LoadScene will be executed before any GameObjects have received any initialization messages.
			// LoadScene is however not an instant operation. Therefore, all GameObjects in the scenes that were
			// loaded in the editor will receive Awake/OnEnable callbacks before the scene loading command
			// takes effect (which in turn results in the GameObjects in the scene receiving OnDisable/OnDestroy callbacks).
			// After this, the MasterScene will begin loading.
			SceneManager.LoadScene(MasterScene);
		}
	}

	// Properties are remembered as editor preferences.
	private static string cEditorPrefLoadMasterOnPlay { get { return "SceneAutoLoader." + PlayerSettings.productName + ".LoadMasterOnPlay"; } }
	private static string cEditorPrefMasterScene { get { return "SceneAutoLoader." + PlayerSettings.productName + ".MasterScene"; } }
	private static string cEditorPrefSceneLoadRequested { get { return "SceneAutoLoader." + PlayerSettings.productName + ".SceneLoadRequested"; } }

	public static bool LoadMasterOnPlay
	{
		get { return EditorPrefs.GetBool(cEditorPrefLoadMasterOnPlay, false); }
		set { EditorPrefs.SetBool(cEditorPrefLoadMasterOnPlay, value); }
	}

	public static string MasterScene
	{
		get { return EditorPrefs.GetString(cEditorPrefMasterScene, ""); }
		set { EditorPrefs.SetString(cEditorPrefMasterScene, value); }
	}

	public static bool SceneLoadRequested
	{
		get { return EditorPrefs.GetBool(cEditorPrefSceneLoadRequested, false); }
		set { EditorPrefs.SetBool(cEditorPrefSceneLoadRequested, value); }
	}
}
