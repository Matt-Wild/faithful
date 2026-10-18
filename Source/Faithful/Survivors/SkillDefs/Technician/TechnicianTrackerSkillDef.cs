using RoR2;
using RoR2.Skills;

namespace Faithful
{
    public class TechnicianTrackerSkillDef : SkillDef
    {
        public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
        {
            // Fetch technician tracker
            return new InstanceData
            {
                technicianTracker = skillSlot.GetComponent<TechnicianTracker>()
            };
        }

        private static bool HasTarget(GenericSkill skillSlot)
        {
            // Does the tracker exist and does it have a tracked target
            TechnicianTracker tracker = ((InstanceData)skillSlot.skillInstanceData).technicianTracker;
            return tracker != null && tracker.GetTrackingTarget();
        }

        public override bool CanExecute(GenericSkill skillSlot)
        {
            // Can only execute if Technician is targeting something
            return HasTarget(skillSlot) && base.CanExecute(skillSlot);
        }

        public override bool IsReady(GenericSkill skillSlot)
        {
            // Is only ready if Technician is targeting something
            return HasTarget(skillSlot) && base.IsReady(skillSlot);
        }

        protected class InstanceData : BaseSkillInstanceData
        {
            // Tracker used for many of Technician's skills
            public TechnicianTracker technicianTracker;
        }
    }
}
