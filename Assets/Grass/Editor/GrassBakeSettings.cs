using UnityEngine;

[CreateAssetMenu(fileName = "GrassBakeSettings", menuName = "BensCoolScriptableObjects/GrassBakeSettings")]
public class GrassBakeSettings : ScriptableObject
{
    public ComputeShader computeShader;
    public Mesh groundMesh;
    public Mesh grassBladeMesh;
    public Vector3 scale;
    public Vector3 rotation;
    public Vector2 minMaxScale = new Vector2(0.8f, 1.2f);
}
