using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
///	 This class adds an Inspector window wrapper for the Scene Auto Loader.
/// </summary>

public class SceneAutoLoaderEditorWindow : EditorWindow
{
	[MenuItem("Window/Scene Auto Loader/Open Inspector Window")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
		SceneAutoLoaderEditorWindow window = (SceneAutoLoaderEditorWindow)EditorWindow.GetWindow(typeof(SceneAutoLoaderEditorWindow));
		window.Show();
	}

	/// <summary>
	/// Return a list of all scene files that exist in project
	/// </summary>
	private string[] GetScenes()
	{
		string[] sceneGuids = AssetDatabase.FindAssets("t:SceneAsset");
		string[] sceneNames = sceneGuids.Select(sceneGuid => AssetDatabase.GUIDToAssetPath(sceneGuid)).ToArray();
		return sceneNames;
	}

	void OnApplicationQuit()
	{
		Debug.Log("SceneAutoLoaderEditorWindow: Application ending");
	}
	/// <summary>
	/// Show "Active" checkbox in editor window
	/// </summary>
	private bool OnGUI_Active()
	{
		bool previousLoadMasterOnPlay = SceneAutoLoader.LoadMasterOnPlay;

		bool newLoadMasterOnPlay = EditorGUILayout.Toggle("Active", previousLoadMasterOnPlay);

		if (newLoadMasterOnPlay != previousLoadMasterOnPlay)
			SceneAutoLoader.LoadMasterOnPlay = newLoadMasterOnPlay;

		return newLoadMasterOnPlay;
	}

	/// <summary>
	/// Show scene dropdown in editor window
	/// </summary>
	private void OnGUI_SelectMasterScene()
	{
		string previousMasterScene = SceneAutoLoader.MasterScene;

		string[] scenes = new string[] { "<No scene chosen>" }.Concat(GetScenes()).ToArray();

		// Map from scene name to index in list
		// If previousMasterScene cannot be found in the list of scenes (or it is set to ""), default to the <No scene chosen> option
		int previousSelectedIndex = Math.Max(Array.IndexOf(scenes, previousMasterScene), 0);

		// Hack: Forward slashes in paths like "Assets/Scenes/MyScene.unity" are converted to "division slash" characters + spacing for display
		// This is to prevent Unity from splitting the dropdown into sub-dropdowns at each forward slash
		//
		// Other people facing the same problem:
		//	 http://answers.unity3d.com/questions/46676/how-can-i-put-a-list-of-filenames-into-an-editor-m.html
		//	 http://answers.unity3d.com/questions/398495/can-genericmenu-item-content-display-.html
		// Suggestion to have the separator character user-configurable:
		//	 https://feedback.unity3d.com/suggestions/genericmenu-submenu-delimiter-character-override
		string[] scenesDisplayNames = scenes.Select(scene => scene.Replace("/", " \u2215 ").Replace(".unity", "")).ToArray();
		int newSelectedIndex = EditorGUILayout.Popup("Scene to auto load", previousSelectedIndex, scenesDisplayNames);

		// Map from index in list to full scene path
		// If <No scene chosen> is chosen, translate to empty path, otherwise keep as-is
		string newMasterScene = (newSelectedIndex > 0) ? scenes[newSelectedIndex] : "";

		if (newMasterScene != previousMasterScene)
			SceneAutoLoader.MasterScene = newMasterScene;
	}

	void OnGUI()
	{
		bool isActive = OnGUI_Active();

		if (isActive)
			OnGUI_SelectMasterScene();
	}
}
