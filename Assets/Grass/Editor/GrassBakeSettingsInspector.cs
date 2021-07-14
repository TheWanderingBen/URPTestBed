using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrassBakeSettings))]
public class GrassBakeSettingsInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create"))
        {
            bool success = GrassBuilder.Run(serializedObject.targetObject as GrassBakeSettings, out Mesh generatedMesh);

            if (success)
            {
                SaveMesh(generatedMesh);
                Debug.Log("Grass created successfully");
            }
            else
            {
                Debug.LogError("Fail to create grass");
            }
        }
    }

    private void SaveMesh(Mesh mesh)
    {
        string path = EditorUtility.SaveFilePanel("Save Grass Asset", "Assets/", name, "asset");
        if (string.IsNullOrEmpty(path))
            return;

        path = FileUtil.GetProjectRelativePath(path);
        Mesh oldMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (oldMesh != null)
        {
            oldMesh.Clear();
            EditorUtility.CopySerialized(mesh, oldMesh);
        }
        else
        {
            AssetDatabase.CreateAsset(mesh, path);
        }
        
        AssetDatabase.SaveAssets();
    }
}
