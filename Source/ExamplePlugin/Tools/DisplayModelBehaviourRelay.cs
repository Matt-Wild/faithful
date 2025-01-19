using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Faithful
{
    internal class DisplayModelBehaviourRelay : MonoBehaviour, IDisplayModelBehaviour
    {
        // The end point using this relay
        IDisplayModelBehaviour endPoint;

        public void Init(IDisplayModelBehaviour _endPoint)
        {
            // Assign end point
            endPoint = _endPoint;
        }

        public void OnDisplayModelCreated() { endPoint.OnDisplayModelCreated(); }

        public string relatedItemToken => endPoint.relatedItemToken;
    }
}
