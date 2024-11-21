using UnityEngine;

namespace Faithful
{
    internal class FaithfulRotatorBehaviour : MonoBehaviour
    {
        // Store rotation axis
        [SerializeField] Vector3 rotationAxis = Vector3.up;

        // Store rotation speed
        [SerializeField] float rotationSpeed = 1.0f;

        public void Init(Vector3 _rotationAxis, float _rotationSpeed)
        {
            // Apply rotation axis and speed
            rotationAxis = _rotationAxis.normalized;
            rotationSpeed = _rotationSpeed;
        }

        void Update()
        {
            // Rotate
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
