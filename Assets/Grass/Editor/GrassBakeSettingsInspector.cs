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

            Mesh[] generatedMeshes = GenerateMeshes(grassBakeSettings);
            GenerateGameObject(generatedMeshes, grassBakeSettings);
        }
    }

    private Mesh[] GenerateMeshes(GrassBakeSettings grassBakeSettings)
    {
        Mesh[] generatedMeshes = new Mesh[grassBakeSettings.lodCount];
        
        for (int i = 0; i < grassBakeSettings.lodCount; ++i)
        {
            bool success = GrassBuilder.Run(grassBakeSettings, i, out generatedMeshes[i]);
            if (!success)
            {
                Debug.LogError("Grass generation failed");
                break;
            }
        }

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
}
