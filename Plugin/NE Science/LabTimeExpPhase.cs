using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class LabTimeExpPhase : ExperimentPhase
    {
        private int labTimeRequired;

        public LabTimeExpPhase()
        {
            NE_Helper.log("Default C-tor");
        }

        public LabTimeExpPhase(PhaseExperimentCore exp, int time)
            : base(exp)
        {
            NE_Helper.log("Param C-tor");
            labTimeRequired = time;
        }

        public override void checkForLabs(bool ready)
        {
            List<PhysicsMaterialsLab> allPhysicsLabs = new List<PhysicsMaterialsLab>(exp.UnityFindObjectsOfType(typeof(PhysicsMaterialsLab)) as PhysicsMaterialsLab[]);
            bool labFound = false;
            foreach (PhysicsMaterialsLab lab in allPhysicsLabs)
            {
                if (lab.vessel == exp.vessel)
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
                    exp.notReadyStatus = "No Material Lab available";
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
                if (lab.vessel == exp.vessel)
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
            PartResource testPoints = exp.setResourceMaxAmount(Resources.LAB_TIME, labTimeRequired);
        }

        public override bool isFinished()
        {
            double numTestPoints = exp.getResourceAmount(Resources.LAB_TIME);

            return Math.Round(numTestPoints, 2) >= labTimeRequired;
        }

        public override void stopResearch()
        {
            exp.stopResearch(Resources.LAB_TIME);
        }

        public override string getInfo()
        {
            return "Lab Time required: " + labTimeRequired + "\n" + "You need a NE MSL-1000 to run this Exeriment.";
        }

    }
}
