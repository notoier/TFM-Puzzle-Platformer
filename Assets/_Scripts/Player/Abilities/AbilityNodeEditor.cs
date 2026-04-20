using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbilityNode), true)]
public class AbilityNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AbilityNode node = (AbilityNode)target;

        if (node.icon != null)
        {
            Texture2D texture = GetTextureFromSprite(node.icon);
            EditorGUIUtility.SetIconForObject(node, texture);
        }
    }

    public static Texture2D GetTextureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height
            );
            newTex.SetPixels(pixels);
            newTex.Apply();
            return newTex;
        }
        else
        {
            return sprite.texture;
        }
    }
}

