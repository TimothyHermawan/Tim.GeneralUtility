using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorUsingScanner
{
    // All known editor-only namespaces
    static readonly string[] editorNamespaces = new[]
    {
        "UnityEditor",
        "UnityEditorInternal",
        "UnityEditor.UI",
        "UnityEditor.Timeline",
        "UnityEditor.Animations"
    };

    [MenuItem("Tools/Scan for Editor Usings in Runtime Scripts")]
    public static void ScanEditorUsings()
    {
        var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        List<string> badUsings = new();

        foreach (var file in files)
        {
            if (file.Contains("/Editor/") || file.Contains("\\Editor\\"))
                continue; // Skip files inside Editor folders

            var lines = File.ReadAllLines(file);
            bool insideEditorGuard = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Track #if UNITY_EDITOR guards
                if (line.Contains("#if UNITY_EDITOR")) insideEditorGuard = true;
                if (line.Contains("#endif")) insideEditorGuard = false;

                if (!insideEditorGuard && line.StartsWith("using "))
                {
                    foreach (var ns in editorNamespaces)
                    {
                        if (line.Contains($"using {ns}"))
                        {
                            string path = file.Replace(Application.dataPath, "Assets");
                            badUsings.Add($"{path} (line {i + 1}): {line}");
                        }
                    }
                }
            }
        }

        if (badUsings.Count == 0)
        {
            Debug.Log("<color=green>✅ No invalid editor usings found!</color>");
        }
        else
        {
            Debug.LogError($"❌ Found {badUsings.Count} editor usings in runtime scripts:");
            foreach (var result in badUsings)
                Debug.LogError(result);
        }
    }
}
