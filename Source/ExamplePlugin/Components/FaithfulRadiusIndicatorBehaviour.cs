using System.Collections;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulRadiusIndicatorBehaviour : MonoBehaviour
    {
        // Store reference to radius indicator parent
        Transform parent;

        // Store reference to indicator adjustment speed
        float adjustmentSpeed;

        // Store target size
        float m_targetSize;

        public void Init(Transform _parent, float _startingSize, Color _colour, float _adjustmentSpeed = 1.0f)
        {
            // Assign parent
            parent = _parent;

            // Ensure parent is applied
            transform.parent = parent;

            // Assign adjustment speed
            adjustmentSpeed = _adjustmentSpeed;

            // Set target size
            m_targetSize = _startingSize;

            // Set starting size
            SetSize(_startingSize);

            // Set colour
            SetColour(_colour);
        }

        private IEnumerator AdjustToSize(float _targetSize)
        {
            // Get starting size
            float startingSize = size;

            // Get difference between target size and starting size
            float sizeDif = _targetSize - startingSize;

            // Track time spent adjusting
            float timeSpent = 0.0f;

            // Cycle until fully adjusted
            while (timeSpent < 1.0f)
            {
                // Add to time spent
                timeSpent = Mathf.Min(timeSpent + Time.deltaTime * adjustmentSpeed, 1.0f);

                // Adjust size
                SetSize(startingSize + sizeDif * Mathf.SmoothStep(0.0f, 1.0f, timeSpent));

                // Next frame
                yield return null;
            }
        }

        public void SetColour(Color _colour)
        {
            // Set colour
            transform.Find("Donut").GetComponent<MeshRenderer>().material.SetColor("_Color", _colour);
            transform.Find("Donut").GetComponent<MeshRenderer>().material.SetColor("_TintColor", _colour);
            transform.Find("Radius, Spherical").GetComponent<MeshRenderer>().material.SetColor("_Color", _colour);
            transform.Find("Radius, Spherical").GetComponent<MeshRenderer>().material.SetColor("_TintColor", _colour);
        }

        public void SetTargetSize(float _targetSize)
        {
            // Set target size
            m_targetSize = _targetSize;

            // Stop all coroutines incase size is already being adjusted
            StopAllCoroutines();

            // Adjust to size
            StartCoroutine(AdjustToSize(_targetSize));
        }

        private void SetSize(float _size)
        {
            // Calculate scale multiplier
            float scaleMult = _size / 13.0f;

            // Set scale
            transform.parent = null;
            transform.localScale = new Vector3(scaleMult, scaleMult, scaleMult);
            transform.parent = parent;
        }

        public float size
        {
            get
            {
                // Return current size of radius
                return transform.lossyScale.x * 13.0f;
            }
        }

        public float targetSize
        {
            get
            {
                // Return target size
                return m_targetSize;
            }
        }
    }
}
