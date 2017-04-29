using UnityEditor;

[InitializeOnLoad]
public class DisableAssemblyReloadInPlayMode
{
	private static bool Locked;

	static DisableAssemblyReloadInPlayMode()
	{
		EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
	}

	private static void OnPlaymodeStateChanged()
	{
		if (EditorApplication.isPlaying && !Locked)
		{
			EditorApplication.LockReloadAssemblies();
			Locked = true;
		}
		else if (!EditorApplication.isPlaying && Locked)
		{
			EditorApplication.UnlockReloadAssemblies();
			Locked = false;
		}
	}
}