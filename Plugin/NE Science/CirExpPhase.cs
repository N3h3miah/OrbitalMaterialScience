/*
 *   This file is part of Orbital Material Science.
 *   
 *   Orbital Material Science is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   Orbital Material Sciencee is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Orbital Material Science.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NE_Science
{
    public class CirExpPhase : ExperimentPhase
    {
        private int cirBurnTimeRequired;

        public CirExpPhase():base()
        {
        }

        public CirExpPhase(PhaseExperimentCore exp, string n, int time)
            : base(exp, n)
        {
            cirBurnTimeRequired = time;
        }

        public override void checkForLabs(bool ready)
        {
            List<MSL_Module> allPhysicsLabs = new List<MSL_Module>(exp.UnityFindObjectsOfType(typeof(MSL_Module)) as MSL_Module[]);
            bool labFound = false;
            foreach (MSL_Module lab in allPhysicsLabs)
            {
                if (lab.vessel == exp.vessel && lab.hasEquipmentInstalled(EquipmentRacks.CIR))
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
            List<MSL_Module> allPhysicsLabs = new List<MSL_Module>(exp.UnityFindObjectsOfType(typeof(MSL_Module)) as MSL_Module[]);
            bool labFound = false;
            foreach (MSL_Module lab in allPhysicsLabs)
            {
                if (lab.vessel == exp.vessel && lab.hasEquipmentInstalled(EquipmentRacks.CIR))
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
