/*
 *   This file is part of Orbital Material Science.
 *   
 *   Part of the code may originate from Station Science ba ether net http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models)
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
    class PhysicsMaterialsLab : Lab
    {
        [KSPField(isPersistant = false)]
        public float TestPointsPerHour = 0;

        [KSPField(isPersistant = false)]
        public float ChargePerTestPoint = 0;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Testpoints")]
        public string testRunsStatus = "";

        private Generator TestPointsGenerator;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }
            
            TestPointsGenerator = new Generator(this.part);
            TestPointsGenerator.addRate("TestPoints", -TestPointsPerHour);
            if (ChargePerTestPoint > 0)
                TestPointsGenerator.addRate("ElectricCharge", ChargePerTestPoint);
            generators.Add(TestPointsGenerator);

        }

        protected override void displayStatusMessage(string s)
        {
            labStatus = s;
            Fields["labStatus"].guiActive = true;
            Fields["testRunsStatus"].guiActive = false;
        }

        protected override void updateLabStatus()
        {
            Fields["labStatus"].guiActive = false;
            testRunsStatus = "";
            var r = getOrDefault(TestPointsGenerator.rates, "TestPoints");
            if (r != null && isActive())
            {
                if (r.last_available == 0)
                    testRunsStatus = "No Experiments";
                else
                    testRunsStatus = String.Format("{0:F2} per hour", -r.ratePerHour * r.rateMultiplier);
            }
            Fields["testRunsStatus"].guiActive = (testRunsStatus != "");
        }

        public override string GetInfo()
        {
            String ret = base.GetInfo();
            ret += (ret == "" ? "" : "\n") + "Testpoints per hour: " + TestPointsPerHour;
            return ret;
        }

    }
}
