using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    /*
 * Experiments needing MSL-1000
 */
    public class MSLExperimentData : StepExperimentData
    {
        protected MSLExperimentData(string id, string type, string name, string abb, EquipmentRacks eq)
            : base(id, type, name, abb, eq)
        { }

        public override List<Lab> getFreeLabsWithEquipment(Vessel vessel)
        {
            List<Lab> ret = new List<Lab>();
            List<MSL_Module> allPhysicsLabs = new List<MSL_Module>(UnityFindObjectsOfType(typeof(MSL_Module)) as MSL_Module[]);
            foreach (MSL_Module lab in allPhysicsLabs)
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

    public class CCF_ExperimentData : MSLExperimentData
    {
        public CCF_ExperimentData()
            : base("NE_CCF", "CCF", "Capillary Channel Flow Experiment", "CCF", EquipmentRacks.FFR)
        {
            step = new ResourceExperimentStep(this, Resources.FFR_TEST_RUN, 22);
        }
    }

    public class CFE_ExperimentData : MSLExperimentData
    {
        public CFE_ExperimentData()
            : base("NE_CFE", "CFE", "Capillary Flow Experiment", "CFE", EquipmentRacks.FFR)
        {
            step = new ResourceExperimentStep(this, Resources.FFR_TEST_RUN, 40);
        }
    }

    public class FLEX_ExperimentData : MSLExperimentData
    {
        public FLEX_ExperimentData()
            : base("NE_FLEX", "FLEX", "Flame Extinguishment Experiment", "FLEX", EquipmentRacks.CIR)
        {
            step = new ResourceExperimentStep(this, Resources.CIR_BURN_TIME, 18);
        }
    }

    public class CFI_ExperimentData : MSLExperimentData
    {
        public CFI_ExperimentData()
            : base("NE_CFI", "CFI", "Cool Flames Investigation", "CFI", EquipmentRacks.CIR)
        {
            step = new ResourceExperimentStep(this, Resources.CIR_BURN_TIME, 50);
        }
    }

    public class MIS1_ExperimentData : MSLExperimentData
    {
        public MIS1_ExperimentData()
            : base("NE_MIS1", "MIS1", "3D Printer Demonstration Test", "MIS-1", EquipmentRacks.PRINTER)
        {
            step = new ResourceExperimentStep(this, Resources.PRINT_LAYER, 100);
        }
    }

    public class MIS2_ExperimentData : MSLExperimentData
    {
        public MIS2_ExperimentData()
            : base("NE_MIS2", "MIS2", "Made in Space: Tools", "MIS-2", EquipmentRacks.PRINTER)
        {
            step = new ResourceExperimentStep(this, Resources.PRINT_LAYER, 200);
        }
    }

    public class MIS3_ExperimentData : MSLExperimentData
    {
        public MIS3_ExperimentData()
            : base("NE_MIS3", "MIS3", "Made in Space: Jebediah figure", "MIS-3", EquipmentRacks.PRINTER)
        {
            step = new ResourceExperimentStep(this, Resources.PRINT_LAYER, 300);
        }
    }
}
