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
            GrassBakeSettings grassBakeSettings = target as GrassBakeSettings;
            if (grassBakeSettings == null)
                return;
            
            string path = EditorUtility.SaveFilePanel("Save Grass Asset", "Assets/", name, "prefab");
            if (string.IsNullOrEmpty(path))
                return;

            path = FileUtil.GetProjectRelativePath(path);
            
            string meshDirectory = GetMeshDirectory(path);
            Mesh[] generatedMeshes = SaveMeshes(grassBakeSettings, meshDirectory);
            
            SavePrefabs(generatedMeshes, grassBakeSettings, path);
        }
    }

    private Mesh[] SaveMeshes(GrassBakeSettings grassBakeSettings, string directory)
    {
        Mesh[] generatedMeshes = new Mesh[grassBakeSettings.lodCount];
        
        for (int i = 0; i < grassBakeSettings.lodCount; ++i)
        {
            bool success = GrassBuilder.Run(grassBakeSettings, i, out generatedMeshes[i]);

            if (success)
            {
                string meshPath = directory + "/" +
                                  directory.Substring(directory.LastIndexOf('/') + 1, directory.Length - directory.LastIndexOf('/') - 1)
                                  + i + ".asset";
                
                AssetDatabase.CreateAsset(generatedMeshes[i], meshPath);
            }
            else
            {
                Debug.LogError("Fail to create grass");
                return null;
            }
        }
        AssetDatabase.Refresh();

        return generatedMeshes;
    }

    private void SavePrefabs(Mesh[] generatedMeshes, GrassBakeSettings grassBakeSettings, string path)
    {
        GameObject parentObject = new GameObject();
        LODGroup lodGroup = parentObject.AddComponent<LODGroup>();
        LOD[] lods = new LOD[generatedMeshes.Length];
        
        for (int i = 0; i < generatedMeshes.Length; ++i)
        {
            GameObject generatedGrass = new GameObject();
            generatedGrass.name = "LOD" + i;
            
            MeshFilter meshFilter = generatedGrass.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = generatedMeshes[i];
            
            MeshRenderer meshRenderer = generatedGrass.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = grassBakeSettings.grassMaterial;

            generatedGrass.transform.parent = parentObject.transform;
            
            Renderer[] renderers = new Renderer[1];
            renderers[0] = meshRenderer;
            lods[i] = new LOD(1f / (i + 1), renderers);
        }
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
        
        PrefabUtility.SaveAsPrefabAsset(parentObject, path);
            
        DestroyImmediate(parentObject);
        
        AssetDatabase.SaveAssets();
    }

    private string GetMeshDirectory(string path)
    {
        //using AssetDatabase because it's platform agnostic, even though it's a royal pain
        string meshDirectory = path.Substring(0, path.LastIndexOf('.')) + "Meshes";

        if (AssetDatabase.IsValidFolder(meshDirectory))
        {
            AssetDatabase.DeleteAsset(meshDirectory);
        }
        
        string parentDirectory = path.Substring(0, path.LastIndexOf('/'));
        string appendToParentDitrctory =
            path.Substring(path.LastIndexOf('/') + 1, path.LastIndexOf('.')- path.LastIndexOf('/') - 1) + "Meshes";
        AssetDatabase.CreateFolder(parentDirectory, appendToParentDitrctory);
        AssetDatabase.Refresh();

        return meshDirectory;
    }
}
