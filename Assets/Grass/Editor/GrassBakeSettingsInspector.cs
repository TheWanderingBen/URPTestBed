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
            Mesh[,,] generatedMeshes = SaveMeshes(grassBakeSettings, meshDirectory);
            
            SavePrefabs(generatedMeshes, grassBakeSettings, path);
        }
    }

    private Mesh[,,] SaveMeshes(GrassBakeSettings grassBakeSettings, string directory)
    {
        Mesh[,,] generatedMeshes = new Mesh[(int)grassBakeSettings.numTiles.x, (int)grassBakeSettings.numTiles.y, grassBakeSettings.grassLODLevelSettings.Length];

        for (int x = 0; x < generatedMeshes.GetLength(0); ++x)
        {
            for (int y = 0; y < generatedMeshes.GetLength(1); ++y)
            {
                for (int i = 0; i < generatedMeshes.GetLength(2); ++i)
                {
                    bool success = GrassBuilder.Run(grassBakeSettings, i, out generatedMeshes[x, y, i]);

                    if (success)
                    {
                        string meshPath = directory + "/" +
                                          directory.Substring(directory.LastIndexOf('/') + 1, directory.Length - directory.LastIndexOf('/') - 1)
                                          + x + y + i + ".asset";
                
                        AssetDatabase.CreateAsset(generatedMeshes[x, y, i], meshPath);
                    }
                    else
                    {
                        Debug.LogError("Fail to create grass");
                        return null;
                    }
                }
            }
        }
        AssetDatabase.Refresh();

        return generatedMeshes;
    }

    private void SavePrefabs(Mesh[,,] generatedMeshes, GrassBakeSettings grassBakeSettings, string path)
    {
        GameObject topParentObject = new GameObject();
        Vector2 tileSize = grassBakeSettings.extents / grassBakeSettings.numTiles;

        for (int x = 0; x < generatedMeshes.GetLength(0); ++x)
        {
            for (int y = 0; y < generatedMeshes.GetLength(1); ++y)
            {
                GameObject tileParentObject = new GameObject();
                tileParentObject.transform.parent = topParentObject.transform;
                tileParentObject.name = "Tile" + x + y;
                
                LODGroup lodGroup = tileParentObject.AddComponent<LODGroup>();
                LOD[] lods = new LOD[generatedMeshes.GetLength(2)];
                
                for (int i = 0; i < generatedMeshes.GetLength(2); ++i)
                {
                    GameObject generatedGrass = new GameObject();
                    generatedGrass.name = "" + x + y + "_LOD" + i;
                    generatedGrass.transform.position = new Vector3(
                        x * tileSize.x - generatedMeshes.GetLength(0) * tileSize.x / 2f,
                        0f,
                        y * tileSize.y - generatedMeshes.GetLength(1) * tileSize.y / 2f);

                    MeshFilter meshFilter = generatedGrass.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = generatedMeshes[x, y, i];

                    MeshRenderer meshRenderer = generatedGrass.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = grassBakeSettings.grassLODLevelSettings[i].grassMaterial;

                    generatedGrass.transform.parent = tileParentObject.transform;

                    Renderer[] renderers = new Renderer[1];
                    renderers[0] = meshRenderer;
                    lods[i] = new LOD(1f / (i + 1), renderers);
                }

                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
            }
        }

        PrefabUtility.SaveAsPrefabAsset(topParentObject, path);
            
        DestroyImmediate(topParentObject);
        
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
