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

        private Guid cachedVesselID;
        private int partCount;
        private List<MPL_Module> physicsLabCache = null;

        protected MPLExperimentData(string id, string type, string name, string abb, EquipmentRacks eq, float mass)
            : base(id, type, name, abb, eq, mass)
        { }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            List<MPL_Module> allPhysicsLabs;
            if (cachedVesselID == vessel.id && partCount == vessel.parts.Count && physicsLabCache != null)
            {
                allPhysicsLabs = physicsLabCache;
            }
            else
            {
                allPhysicsLabs = new List<MPL_Module>(UnityFindObjectsOfType(typeof(MPL_Module)) as MPL_Module[]);
                physicsLabCache = allPhysicsLabs;
                cachedVesselID = vessel.id;
                partCount = vessel.parts.Count;
                NE_Helper.log("Lab Cache refresh");
            }
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

    public class ADUM_ExperimentData : KerbalResearchExperimentData
    {
        public ADUM_ExperimentData(float mass)
            : base("NE_ADUM", "ADUM", "Advanced Diagnostic Ultrasound in Microgravity", "ADUM", EquipmentRacks.USU, mass, 4)
        {
            steps[0] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 2.5f, 0);
            steps[1] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 2.5f, 1);
            steps[2] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 2.5f, 2);
            steps[3] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 2.5f, 3);
        }
    }

    public class SpiU_ExperimentData : KerbalResearchExperimentData
    {
        public SpiU_ExperimentData(float mass)
            : base("NE_SpiU", "SpiU", "Sonographic Astronaut Vertebral Examination", "SpiU", EquipmentRacks.USU, mass, 6)
        {
            steps[0] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 3f, 0);
            steps[1] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 3f, 1);
            steps[2] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 3f, 2);
            steps[3] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 3f, 3);
            steps[4] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 3f, 3);
            steps[5] = new KerbalResearchStep(this, Resources.ULTRASOUND_GEL, 3f, 3);
        }
    }
}
