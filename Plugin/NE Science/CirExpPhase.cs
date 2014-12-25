using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class CirExpPhase : ExperimentPhase
    {
        private int cirBurnTimeRequired;

        public CirExpPhase()
        {
            NE_Helper.log("Default C-tor");
        }

        public CirExpPhase(PhaseExperimentCore exp, int time)
            : base(exp)
        {
            NE_Helper.log("Param C-tor");
            cirBurnTimeRequired = time;
        }

        public override void checkForLabs(bool ready)
        {
            List<PhysicsMaterialsLab> allPhysicsLabs = new List<PhysicsMaterialsLab>(exp.UnityFindObjectsOfType(typeof(PhysicsMaterialsLab)) as PhysicsMaterialsLab[]);
            bool labFound = false;
            foreach (PhysicsMaterialsLab lab in allPhysicsLabs)
            {
                if (lab.vessel == exp.vessel && lab.hasEquipmentInstalled(PhysicsMaterialsLab.EquipmentRacks.CIR))
                {
                    labFound = true;
                    break;
                }
            }
            if (!ready)
            {
                if (labFound)
                {
                    exp.labFound();
                    return;
                }
                else
                {
                    exp.notReadyStatus = "No MSL with CIR available";
                }
            }
            if (ready && !labFound)
            {
                exp.labLost();
            }
        }

        public override void checkUndocked()
        {
            List<PhysicsMaterialsLab> allPhysicsLabs = new List<PhysicsMaterialsLab>(exp.UnityFindObjectsOfType(typeof(PhysicsMaterialsLab)) as PhysicsMaterialsLab[]);
            bool labFound = false;
            foreach (PhysicsMaterialsLab lab in allPhysicsLabs)
            {
                if (lab.vessel == exp.vessel && lab.hasEquipmentInstalled(PhysicsMaterialsLab.EquipmentRacks.CIR))
                {
                    labFound = true;
                    break;
                }
            }
            if (!labFound)
            {
                exp.undockedRunningExp();
            }
        }

        public override void createResources()
        {
            PartResource testPoints = exp.setResourceMaxAmount(Resources.CIR_BURN_TIME, cirBurnTimeRequired);
        }

        public override bool isFinished()
        {
            double numTestPoints = exp.getResourceAmount(Resources.CIR_BURN_TIME);

            return Math.Round(numTestPoints, 2) >= cirBurnTimeRequired;
        }

        public override void stopResearch()
        {
            exp.stopResearch(Resources.CIR_BURN_TIME);
        }

        public override string getInfo()
        {
            return "CIR brun time required: " + cirBurnTimeRequired + "\n" + "You need a NE MSL-1000 with an installed CIR to run this Exeriment.";
        }

    }
}
