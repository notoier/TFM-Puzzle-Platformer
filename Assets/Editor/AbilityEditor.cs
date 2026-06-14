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

        Ability ability = (Ability)target;
        AbilityValidationResult validation = ability.Validate();
        DrawAbilityStatus(validation);
        EditorGUILayout.Space(10);

        SerializedProperty variables = serializedObject.FindProperty("variables");
        EditorGUILayout.PropertyField(variables, true);
        EditorGUILayout.Space(10);

        SerializedProperty nodes = serializedObject.FindProperty("nodes");
        EditorGUILayout.PropertyField(nodes, true);

        GUILayout.Space(10);

        if (GUILayout.Button("Add Node"))
        {
            ShowNodeMenu();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Clear Nodes", GUILayout.Height(30)))
        {
            Undo.RecordObject(ability, "Clear Nodes");

            nodes.arraySize = 0;
            serializedObject.ApplyModifiedProperties();

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
        serializedObject.Update();

        AbilityNode newNode = (AbilityNode)Activator.CreateInstance(type);
        SerializedProperty nodes = serializedObject.FindProperty("nodes");

        nodes.arraySize++;
        nodes.GetArrayElementAtIndex(nodes.arraySize - 1).managedReferenceValue = newNode;

        Ability ability = (Ability)target;
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

    private void DrawAbilityStatus(AbilityValidationResult validation)
    {
        Color prevColor = GUI.color;
        GUI.color = GetStateColor(validation.State);
        EditorGUILayout.LabelField($"Status: {GetStateLabel(validation.State)}", EditorStyles.boldLabel);
        GUI.color = prevColor;

        if (!string.IsNullOrEmpty(validation.Message))
            EditorGUILayout.HelpBox(validation.Message, GetMessageType(validation.State));
    }

    private Color GetStateColor(AbilityValidationState state)
    {
        return state switch
        {
            AbilityValidationState.Invalid => new Color32(255, 64, 64, 255),
            AbilityValidationState.Complete => new Color32(0, 220, 80, 255),
            AbilityValidationState.Ready => new Color32(64, 160, 255, 255),
            _ => new Color32(255, 196, 0, 255)
        };
    }

    private string GetStateLabel(AbilityValidationState state)
    {
        return state switch
        {
            AbilityValidationState.Invalid => "Invalid",
            AbilityValidationState.Complete => "Complete",
            AbilityValidationState.Ready => "Ready",
            _ => "Incomplete"
        };
    }

    private MessageType GetMessageType(AbilityValidationState state)
    {
        return state == AbilityValidationState.Invalid ? MessageType.Error : MessageType.Info;
    }
}
