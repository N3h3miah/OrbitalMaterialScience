using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public abstract class MEPExperimentData : MultiStepExperimentData<MEPResourceExperimentStep>
    {

        private Guid cachedVesselID;
        private int partCount;
        private List<MEP_Module> physicsLabCache = null;

        protected MEPExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass, int numSteps)
            : base(id, type, name, abb, eq, mass, numSteps)
        { }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            List<MEP_Module> allPhysicsLabs;
            if (cachedVesselID == vessel.id && partCount == vessel.parts.Count && physicsLabCache != null)
            {
                allPhysicsLabs = physicsLabCache;
            }
            else
            {
                allPhysicsLabs = new List<MEP_Module>(UnityFindObjectsOfType(typeof(MEP_Module)) as MEP_Module[]);
                physicsLabCache = allPhysicsLabs;
                cachedVesselID = vessel.id;
                partCount = vessel.parts.Count;
                NE_Helper.log("Lab Cache refresh");
            }
            foreach (MEP_Module lab in allPhysicsLabs)
            {
                if (lab.vessel == vessel && lab.hasEquipmentFreeExperimentSlot(neededEquipment))
                {
                    ret.Add(lab);
                }
            }
            return ret;
        }

        public override bool canInstall(Vessel vessel)
        {
            List<Lab> labs = getFreeLabsWithEquipment(vessel);
            return labs.Count > 0 && state == ExperimentState.STORED;
        }
    }

    public class MEE1_ExperimentData : MEPExperimentData
    {
        public MEE1_ExperimentData(float mass)
            : base("NE_MEE1", "MEE1", "Material Exposure Experiment 1", "MEE1", EquipmentRacks.EXPOSURE, mass, 2)
        {
            steps[0] = new MEPResourceExperimentStep(this, Resources.LAB_TIME, 1, "Preparation", 0);
            steps[1] = new MEPResourceExperimentStep(this, Resources.EXPOSURE_TIME, 20, "Exposure", 1);

        }
    }

    public class MEE2_ExperimentData : MEPExperimentData
    {
        public MEE2_ExperimentData(float mass)
            : base("NE_MEE2", "MEE2", "Material Exposure Experiment 2", "MEE2", EquipmentRacks.EXPOSURE, mass, 3)
        {
            steps[0] = new MEPResourceExperimentStep(this, Resources.LAB_TIME, 1, "Preparation", 0);
            steps[1] = new MEPResourceExperimentStep(this, Resources.EXPOSURE_TIME, 40, "Exposure", 1);
            steps[2] = new MEPResourceExperimentStep(this, Resources.LAB_TIME, 2, "Store Samples", 2);
        }
    }
}
