using System.Collections.Generic;
using RoR2;

namespace Faithful
{
    internal delegate void InHoldoutZoneCallback(CharacterBody _contained, HoldoutZoneController _zone);

    internal class Behaviour
    {
        // Toolbox
        protected Toolbox toolbox;

        // Behaviour callback functions
        protected List<InHoldoutZoneCallback> inHoldoutZoneCallbacks = new List<InHoldoutZoneCallback>();

        // Constructor
        public Behaviour(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Inject hooks
            On.RoR2.HoldoutZoneController.FixedUpdate += HookHoldoutZoneControllerFixedUpdate;

            Log.Debug("Behaviour initialised");
        }

        // Add In Holdout Zone callback
        public void CallInHoldoutZone(InHoldoutZoneCallback _callback)
        {
            inHoldoutZoneCallbacks.Add(_callback);

            Log.Debug("Added Holdout Zone behaviour");
        }

        protected void HookHoldoutZoneControllerFixedUpdate(On.RoR2.HoldoutZoneController.orig_FixedUpdate orig, HoldoutZoneController self)
        {
            // Get Hurt Boxes in range of Holdout Zone
            HurtBox[] hurtBoxes = toolbox.utils.GetHurtBoxesInSphere(self.transform.position, self.currentRadius);

            // Cycle through Hurt Boxes
            foreach (HurtBox hurtBox in hurtBoxes)
            {
                // Cycle through InHoldoutZone callbacks and call with Character Body
                foreach (InHoldoutZoneCallback callback in inHoldoutZoneCallbacks)
                {
                    callback(hurtBox.healthComponent.body, self);
                }
            }

            orig(self); // Run normal processes
        }
    }
}
