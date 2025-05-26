using UnityEditor;
using UnityEngine;

public static class KeywordSetter
{
    // Keywords
    private const string ditherKeyword = "DITHER";
    private const string limbRemovalKeyword = "LIMBREMOVAL";
    private const string noCullKeyword = "NOCULL";

    // Dither
    [MenuItem("Tools/Keywords/Dither/Set on Selected Materials")]
    public static void SetDitherKeyword()
    {
        // Set dither keyword on selection
        SetKeyword(ditherKeyword);
    }

    [MenuItem("Tools/Keywords/Dither/Remove from Selected Materials")]
    public static void RemoveDitherKeyword()
    {
        // Remove dither keyword on selection
        RemoveKeyword(ditherKeyword);
    }

    // Limb Removal
    [MenuItem("Tools/Keywords/Limb Removal/Set on Selected Materials")]
    public static void SetLimbRemovalKeyword()
    {
        // Set limb removal keyword on selection
        SetKeyword(limbRemovalKeyword);
    }

    [MenuItem("Tools/Keywords/Limb Removal/Remove from Selected Materials")]
    public static void RemoveLimbRemovalKeyword()
    {
        // Remove limb removal keyword on selection
        RemoveKeyword(limbRemovalKeyword);
    }

    // No Cull
    [MenuItem("Tools/Keywords/No Cull/Set on Selected Materials")]
    public static void SetNoCullKeyword()
    {
        // Set no cull keyword on selection
        SetKeyword(noCullKeyword);
    }

    [MenuItem("Tools/Keywords/No Cull/Remove from Selected Materials")]
    public static void RemoveNoCullKeyword()
    {
        // Remove no cull keyword on selection
        RemoveKeyword(noCullKeyword);
    }

    private static void SetKeyword(string _keyword)
    {
        // Cycle through selected objects
        foreach (var obj in Selection.objects)
        {
            // Check if object is material
            if (obj is Material mat)
            {
                // Enable keyword and mark as dirty
                mat.EnableKeyword(_keyword);
                EditorUtility.SetDirty(mat);
                Debug.Log($"✅ Enabled {_keyword} on: {mat.name}");
            }
        }

        // Save to disk
        AssetDatabase.SaveAssets();
    }

    private static void RemoveKeyword(string _keyword)
    {
        // Cycle through selected objects
        foreach (var obj in Selection.objects)
        {
            // Check if object is material
            if (obj is Material mat)
            {
                // Enable keyword and mark as dirty
                mat.DisableKeyword(_keyword);
                EditorUtility.SetDirty(mat);
                Debug.Log($"❌ Removed {_keyword} from: {mat.name}");
            }
        }

        // Save to disk
        AssetDatabase.SaveAssets();
    }
}