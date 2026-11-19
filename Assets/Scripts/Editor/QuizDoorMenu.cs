using UnityEditor;
using UnityEngine;

public static class QuizDoorMenu
{
	const string PrefabPath = "Assets/QuizDoor.prefab";

	[MenuItem("GameObject/Chat Vs Streamer/Quiz Door", false, 10)]
	static void CreateQuizDoor()
	{
		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
		if (prefab == null)
		{
			Debug.LogError("[QuizDoor] Missing prefab at " + PrefabPath);
			return;
		}

		GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
		Undo.RegisterCreatedObjectUndo(instance, "Create Quiz Door");
		Selection.activeGameObject = instance;
	}
}
