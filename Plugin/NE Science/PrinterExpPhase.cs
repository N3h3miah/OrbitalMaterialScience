using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class PrinterExpPhase : ExperimentPhase
    {
        private int layersRequired;

        public PrinterExpPhase()
        {
            NE_Helper.log("Default C-tor");
        }

        public PrinterExpPhase(PhaseExperimentCore exp, int layers)
            : base(exp)
        {
            NE_Helper.log("Param C-tor");
            layersRequired = layers;
        }

        public override void checkForLabs(bool ready)
        {
            List<PhysicsMaterialsLab> allPhysicsLabs = new List<PhysicsMaterialsLab>(exp.UnityFindObjectsOfType(typeof(PhysicsMaterialsLab)) as PhysicsMaterialsLab[]);
            bool labFound = false;
            foreach (PhysicsMaterialsLab lab in allPhysicsLabs)
            {
                if (lab.vessel == exp.vessel && lab.hasEquipmentInstalled(PhysicsMaterialsLab.EquipmentRacks.PRINTER))
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
                    exp.notReadyStatus = "No MSL with 3D-Printer available";
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
                if (lab.vessel == exp.vessel && lab.hasEquipmentInstalled(PhysicsMaterialsLab.EquipmentRacks.PRINTER))
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
            PartResource testPoints = exp.setResourceMaxAmount(Resources.PRINT_LAYER, layersRequired);
        }

        public override bool isFinished()
        {
            double layers = exp.getResourceAmount(Resources.PRINT_LAYER);

            return Math.Round(layers, 2) >= layersRequired;
        }

        public override void stopResearch()
        {
            exp.stopResearch(Resources.PRINT_LAYER);
        }

        public override string getInfo()
        {
            return "3D rint layers required: " + layersRequired + "\n" + "You need a NE MSL-1000 with an installed CIR to run this Exeriment.";
        }

    }
}
