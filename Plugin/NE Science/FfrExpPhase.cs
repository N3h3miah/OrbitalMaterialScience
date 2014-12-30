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
    public class FfrExpPhase : ExperimentPhase
    {
        private int ffrTestRunsRequired;

        public FfrExpPhase()
        {
        }

        public FfrExpPhase(PhaseExperimentCore exp, string n, int runs)
            : base(exp, n)
        {
            ffrTestRunsRequired = runs;
        }

        public override void checkForLabs(bool ready)
        {
            List<PhysicsMaterialsLab> allPhysicsLabs = new List<PhysicsMaterialsLab>(exp.UnityFindObjectsOfType(typeof(PhysicsMaterialsLab)) as PhysicsMaterialsLab[]);
            bool labFound = false;
            foreach (PhysicsMaterialsLab lab in allPhysicsLabs)
            {
                if (lab.vessel == exp.vessel && lab.hasEquipmentInstalled(PhysicsMaterialsLab.EquipmentRacks.FFR))
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
                    exp.notReadyStatus = "No MSL with FFR available";
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
                if (lab.vessel == exp.vessel && lab.hasEquipmentInstalled(PhysicsMaterialsLab.EquipmentRacks.FFR))
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
            PartResource testPoints = exp.setResourceMaxAmount(Resources.FFR_TEST_RUN, ffrTestRunsRequired);
        }

        public override bool isFinished()
        {
            double numTestPoints = exp.getResourceAmount(Resources.FFR_TEST_RUN);

            return Math.Round(numTestPoints, 2) >= ffrTestRunsRequired;
        }

        public override void stopResearch()
        {
            exp.stopResearch(Resources.FFR_TEST_RUN);
        }

        public override string getInfo()
        {
            return "FFR Test Runs required: " + ffrTestRunsRequired + "\n" + "You need a NE MSL-1000 with an installed FFR to run this Exeriment.";
        }

    }
}
