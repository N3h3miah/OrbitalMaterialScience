using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class MEPExperimentData : MultiStepExperimentData<MEPResourceExperimentStep>
    {
        protected MEPExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass)
            : base(id, type, name, abb, eq, mass)
        { }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            List<MEP_Module> allPhysicsLabs = new List<MEP_Module>(UnityFindObjectsOfType(typeof(MEP_Module)) as MEP_Module[]);
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
            : base("NE_MEE1", "MEE1", "Material Exposure Experiment 1", "MEE1", EquipmentRacks.EXPOSURE, mass)
        {

            steps = new MEPResourceExperimentStep[2];
            steps[0] = new MEPResourceExperimentStep(this, Resources.LAB_TIME, 1, "Preparation", 0);
            steps[1] = new MEPResourceExperimentStep(this, Resources.EXPOSURE_TIME, 20, "Exposure", 1);

        }
    }

    public class MEE2_ExperimentData : MEPExperimentData
    {
        public MEE2_ExperimentData(float mass)
            : base("NE_MEE2", "MEE2", "Material Exposure Experiment 2", "MEE2", EquipmentRacks.EXPOSURE, mass)
        {
            steps = new MEPResourceExperimentStep[3];
            steps[0] = new MEPResourceExperimentStep(this, Resources.LAB_TIME, 1, "Preparation", 0);
            steps[1] = new MEPResourceExperimentStep(this, Resources.EXPOSURE_TIME, 40, "Exposure", 1);
            steps[2] = new MEPResourceExperimentStep(this, Resources.LAB_TIME, 2, "Store Samples", 2);
        }
    }
}
