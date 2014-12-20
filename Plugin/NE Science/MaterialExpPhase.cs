using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class MaterialExpPhase : ExperimentPhase
    {
        private int testPointsRequired;

        public MaterialExpPhase(ExperimentPhaseCore exp, int points)
            : base(exp)
        {
           
            testPointsRequired = points;
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
            PartResource testPoints = exp.setResourceMaxAmount("TestPoints", testPointsRequired);
        }

        public override bool isFinished()
        {
            double numTestPoints = exp.getResourceAmount("TestPoints");

            return Math.Round(numTestPoints, 2) >= testPointsRequired;
        }

        public override void stopResearch()
        {
            exp.stopResearch("TestPoints");
        }

        public override string getInfo()
        {
            return "Testpoints required: " + testPointsRequired + "\n" + "You need a NE MSL-1000 to run this Exeriment.";
        }

    }
}
