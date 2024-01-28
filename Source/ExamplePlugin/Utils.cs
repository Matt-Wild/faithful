using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class Utils
    {
        // Toolbox
        protected Toolbox toolbox;

        // Constructor
        public Utils(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            Log.Debug("Utils initialised");
        }

        // Refresh chosen buff on chosen character
        public void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float duration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0)   
            {
                return; // Body not valid
            }

            // Cycle through buffs
            foreach (CharacterBody.TimedBuff buff in body.timedBuffs)   
            {
                // Check if correct buff
                if (buffDef.buffIndex == buff.buffIndex)    
                {
                    // Refresh buff
                    buff.timer = duration;
                }
            }
        }

        // Return Hurt Boxes from RoR2 Sphere Search
        public HurtBox[] GetHurtBoxesInSphere(Vector3 position, float radius)
        {
            if (radius <= 0)
            {
                return []; // Can't check 0 or negative radius
            }

            // Get hurt boxes in sphere search
            RoR2.HurtBox[] hurtBoxes = new SphereSearch
            {
                radius = radius,
                mask = LayerIndex.entityPrecise.mask,
                origin = position
            }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            return hurtBoxes;   // Return found hurt boxes
        }
    }
}
