using UnityEditor;
using UnityEngine;

public static class Actions
{
	[MenuItem("GameObject/UI/Screencast Input")]
	public static void CreateScreencastInputInScene()
	{
		GameObject lScreencastInput = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ScreencastInput/Prefabs/Screencast Input.prefab");
		PrefabUtility.InstantiatePrefab(lScreencastInput);
	}
}
