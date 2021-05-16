using UnityEngine;

public class MapTraveler : MonoBehaviour
{
    [SerializeField] CustomRenderTexture mapLinesRenderTexture;
    [SerializeField] Material mapLinesMaterial;
    [SerializeField] Vector2 worldSize;

    Vector3 position = Vector3.zero;

    void Start()
    {
        mapLinesRenderTexture.Initialize();
    }

    void Update()
    {
        if (position.x != transform.position.x || position.z != transform.position.z)
        {
            mapLinesMaterial.SetVector("_Coordinate", new Vector4(transform.position.x / worldSize.x,
                transform.position.z / worldSize.y, 0, 0));
            
            position = transform.position;
            mapLinesRenderTexture.Update();
        }
    }
}
