using UnityEngine;

public class MapTraveler : MonoBehaviour
{
    [SerializeField] RenderTexture mapLinesRenderTexture;
    [SerializeField] Material mapLinesMaterial;
    [SerializeField] Vector2 worldSize;

    void Start()
    {
        ClearRenderTexture();
    }

    void Update()
    {
        mapLinesMaterial.SetVector("_Coordinate", new Vector4(transform.position.x/worldSize.x, 
                                                                transform.position.z/worldSize.y, 0, 0));
    }
    
    void ClearRenderTexture()
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = mapLinesRenderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
    }
}
