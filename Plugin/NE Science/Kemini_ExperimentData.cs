using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    /*
  * Experiments for the Kemini Research Program
  */
    public class KeminiExperimentData : StepExperimentData
    {
        protected KeminiExperimentData(string id, string type, string name, string abb, float mass)
            : base(id, type, name, abb, EquipmentRacks.KEMINI, mass)
        { }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            List<Kemini_Module> allKeminiLabs = new List<Kemini_Module>(UnityFindObjectsOfType(typeof(Kemini_Module)) as Kemini_Module[]);
            foreach (Kemini_Module lab in allKeminiLabs)
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

    public class KeminiD8_ExperimentData : KeminiExperimentData
    {
        public KeminiD8_ExperimentData(float mass)
            : base("NE_Kemini_D8", "KeminiD8", "Kemini D8: Spacecraft Radiation Level", "D8", mass)
        {
            step = new ResourceExperimentStep(this, Resources.LAB_TIME, 0.1f, "", 0);
        }
    }

}
