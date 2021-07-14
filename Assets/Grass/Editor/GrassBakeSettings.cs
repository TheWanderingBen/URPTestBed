using UnityEngine;

[CreateAssetMenu(fileName = "GrassBakeSettings", menuName = "BensCoolScriptableObjects/GrassBakeSettings")]
public class GrassBakeSettings : ScriptableObject
{
    public ComputeShader computeShader;
    public Mesh groundMesh;
    public Mesh grassBladeMesh;
    public Vector3 scale;
    public Vector3 rotation;
}
