using UnityEngine;
using System;
using UnityEditor;
using Unity.VisualScripting;

public static class ChainPrefabCreator
{
    private const string outputFolder = "Assets/_Data/Prefabs/Props/Chains";

    public static GameObject SavePrefab(GameObject chainRoot, string prefabName)
    {
        if(chainRoot == null)
            throw new ArgumentNullException(nameof(chainRoot), "Chain root GameObject cannot be null.");

        EnsureOutputFolderExists(outputFolder);

        string prefabPath = $"{outputFolder}/{prefabName}.prefab";
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
            chainRoot,
            prefabPath,
            out bool success);

        if (!success)
        {
            Debug.LogError($"Failed to save prefab at path: {prefabPath}");
            return null;
        }

        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(prefab);

        Debug.Log($"Prefab saved successfully at path: {prefabPath}");
        return prefab;
    }

    public static void EnsureOutputFolderExists(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string currentPath = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string nextPath = $"{currentPath}/{parts[i]}";

            if(!AssetDatabase.IsValidFolder(nextPath))
                AssetDatabase.CreateFolder(currentPath, parts[i]);
            
            currentPath = nextPath;
        }
    }
}
