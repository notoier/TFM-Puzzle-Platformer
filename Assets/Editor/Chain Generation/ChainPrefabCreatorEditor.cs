using UnityEngine;
using UnityEditor;

public class ChainPrefabCreatorEditor : EditorWindow
{
    private int chainLength = 10;
    private float segmentSpacing = 0.24f;
    private string prefabName = "New Chain Prefab";

    private GameObject chainRoot;
    private GameObject chainSegmentA;
    private GameObject chainSegmentB;

    [MenuItem("Tools/Chain Prefab Creator")]
    private static void ShowWindow()
    {
        GetWindow<ChainPrefabCreatorEditor>("Chain Prefab Creator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(
            "Chain Settings",
            EditorStyles.boldLabel);

        chainLength = EditorGUILayout.IntField(
            "Chain Length",
            chainLength);

        segmentSpacing = EditorGUILayout.FloatField(
            "Segment Spacing",
            segmentSpacing);

        prefabName = EditorGUILayout.TextField(
            "Prefab Name",
            prefabName);

        EditorGUILayout.Space();

        chainRoot = DrawPrefabField("Chain Root", chainRoot);
        chainSegmentA = DrawPrefabField("Segment A", chainSegmentA);
        chainSegmentB = DrawPrefabField("Segment B", chainSegmentB);

        EditorGUILayout.Space();

        bool missingPrefabs =
            chainRoot == null
            || chainSegmentA == null
            || chainSegmentB == null;

        EditorGUI.BeginDisabledGroup(missingPrefabs);

        if (GUILayout.Button("Generate and Save Chain"))
            GenerateAndSave();

        EditorGUI.EndDisabledGroup();
    }

    private static GameObject DrawPrefabField(
        string label,
        GameObject currentValue)
    {
        return (GameObject)EditorGUILayout.ObjectField(
            label,
            currentValue,
            typeof(GameObject),
            false);
    }

    private void GenerateAndSave()
    {
        GameObject generatedChain = null;

        try
        {
            generatedChain = ChainGenerator.GenerateChain(
                chainLength,
                segmentSpacing,
                prefabName,
                chainRoot,
                chainSegmentA,
                chainSegmentB);

            ChainPrefabCreator.SavePrefab(
                generatedChain,
                prefabName);
        }
        finally
        {
            if (generatedChain != null)
                DestroyImmediate(generatedChain);
        }
    }
}

