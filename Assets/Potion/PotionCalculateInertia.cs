using UnityEngine;

public class PotionCalculateInertia : MonoBehaviour
{
    [SerializeField] Material liquidMaterial;
    [SerializeField] float shakeSensitivity = 1f;
    [SerializeField] float inertiaResetTime = 0.5f;
    
    Vector3 currentPosition;
    float inertia;
    
    void Awake()
    {
        currentPosition = transform.position;
    }

    void Update()
    {
        Vector3 movement = currentPosition - transform.position;
        inertia = Mathf.Min(1, inertia + movement.sqrMagnitude * shakeSensitivity);
        liquidMaterial.SetFloat("_CurrentInertia", inertia);
        currentPosition = transform.position;
        inertia = Mathf.Max(0, inertia - inertiaResetTime * Time.deltaTime);
    }
}
