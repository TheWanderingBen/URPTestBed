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

            string path = AssetDatabase.GetAssetPath(target);
            string meshDirectory = GetMeshDirectory(path, grassBakeSettings.objectName + "Meshes");
            Mesh[] generatedMeshes = SaveMeshes(grassBakeSettings, meshDirectory);
            
            GenerateGameObject(generatedMeshes, grassBakeSettings);
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
        AssetDatabase.SaveAssets();

        return generatedMeshes;
    }

    private void GenerateGameObject(Mesh[] generatedMeshes, GrassBakeSettings grassBakeSettings)
    {
        GameObject parentObject = new GameObject();
        parentObject.name = grassBakeSettings.objectName;
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
    }

    private string GetMeshDirectory(string path, string directoryName)
    {
        string meshDirectory = path.Substring(0, path.LastIndexOf('/') + 1) + directoryName;
        
        if(!AssetDatabase.IsValidFolder(meshDirectory))
        {
            AssetDatabase.CreateFolder(path.Substring(0, path.LastIndexOf('/')), directoryName);
            AssetDatabase.Refresh();
        }

        return meshDirectory;
    }
}
