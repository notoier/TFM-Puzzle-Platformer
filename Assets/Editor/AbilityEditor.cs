using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ability))]
public class AbilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("ABILITY EDITOR IS RUNNING");
        EditorGUILayout.Space(10);

        serializedObject.Update();

        SerializedProperty nodes = serializedObject.FindProperty("nodes");
        EditorGUILayout.PropertyField(nodes, true);

        GUILayout.Space(10);

        if (GUILayout.Button("Add Node"))
        {
            ShowNodeMenu();
        }

        GUILayout.Space(900);

        if (GUILayout.Button("Clear Nodes", GUILayout.Height(30)))
        {
            Ability ability = (Ability)target;

            Undo.RecordObject(ability, "Clear Nodes");

            ability.nodes.Clear();

            EditorUtility.SetDirty(ability);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowNodeMenu()
    {
        GenericMenu menu = new GenericMenu();

        var nodeTypes = GetAllNodeTypes();

        var grouped = nodeTypes
            .GroupBy(GetMainCategoryType)
            .OrderBy(g => g.Key.Name);

        foreach (var group in grouped)
        {
            string categoryName = group.Key.Name.Replace("Node", "");

            foreach (var type in group.OrderBy(t => t.Name))
            {
                string nodeName = type.Name.Replace("Node", "");
                string path = $"{categoryName}/{nodeName}";

                menu.AddItem(new GUIContent(path), false, () =>
                {
                    AddNode(type);
                });
            }
        }

        menu.ShowAsContext();
    }

    private Type GetMainCategoryType(Type type)
    {
        // Walk up inheritance until we hit the direct child of AbilityNode
        Type current = type.BaseType;

        while (current != null && current.BaseType != typeof(AbilityNode))
        {
            current = current.BaseType;
        }

        return current ?? type;
    }

    private void AddNode(Type type)
    {
        Ability ability = (Ability)target;

        Undo.RecordObject(ability, "Add Ability Node");

        if (ability.nodes == null)
            ability.nodes = new List<AbilityNode>();

        AbilityNode newNode = (AbilityNode)Activator.CreateInstance(type);

        ability.nodes.Add(newNode);

        EditorUtility.SetDirty(ability);
        serializedObject.ApplyModifiedProperties();
    }

    private List<Type> GetAllNodeTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(AbilityNode).IsAssignableFrom(t))
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToList();
    }
}