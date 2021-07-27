using UnityEngine;

[CreateAssetMenu(fileName = "GrassBakeSettings", menuName = "BensCoolScriptableObjects/GrassBakeSettings")]
public class GrassBakeSettings : ScriptableObject
{
    public ComputeShader computeShader;
    public Mesh grassBladeMesh;
    public Material grassMaterial;
    public int lodCount = 2;
    public Vector2 extents = new Vector2(500f, 500f);
    public Vector2 numTiles = new Vector2(10f, 10f);
    public float density = 0.1f;
    public float maxRandomPositionShift = 0.05f;
    public Vector2 minMaxScale = new Vector2(0.8f, 1.2f);
}
