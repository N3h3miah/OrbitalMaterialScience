using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    /*
 * Experiments needing MPL-600
 */
    public class MPLExperimentData : StepExperimentData
    {
        protected MPLExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass)
            : base(id, type, name, abb, eq, mass)
        { }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            List<MPL_Module> allPhysicsLabs = new List<MPL_Module>(UnityFindObjectsOfType(typeof(MPL_Module)) as MPL_Module[]);
            foreach (MPL_Module lab in allPhysicsLabs)
            {
                if (lab.vessel == vessel && lab.hasEquipmentInstalled(neededEquipment) && lab.hasEquipmentFreeExperimentSlot(neededEquipment))
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

    public class CCF_ExperimentData : MPLExperimentData
    {
        public CCF_ExperimentData(float mass)
            : base("NE_CCF", "CCF", "Capillary Channel Flow Experiment", "CCF", EquipmentRacks.MSG, mass)
        {
            step = new ResourceExperimentStep(this, Resources.MSG_TIME, 22, "", 0);
        }
    }

    public class CFE_ExperimentData : MPLExperimentData
    {
        public CFE_ExperimentData(float mass)
            : base("NE_CFE", "CFE", "Capillary Flow Experiment", "CFE", EquipmentRacks.MSG, mass)
        {
            step = new ResourceExperimentStep(this, Resources.MSG_TIME, 40, "", 0);
        }
    }
}
