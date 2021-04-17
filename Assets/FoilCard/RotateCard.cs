using UnityEngine;

namespace FoilCard
{
    public class RotateCard : MonoBehaviour
    {
        [SerializeField] float rotationSpeed = 10f;
        [SerializeField] float maxRotation = 45f;

        float currentRotation;
        int direction = 1;
        
        void Update()
        {
            float rotationAmount = rotationSpeed * direction * Time.deltaTime;
            transform.Rotate(new Vector3(0,0,1), rotationAmount);

            currentRotation += rotationAmount;
            if (Mathf.Abs(currentRotation) > maxRotation)
                direction *= -1;
        }
    }
}
