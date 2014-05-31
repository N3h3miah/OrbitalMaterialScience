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
using UnityEngine;
using StationScience;

namespace NE_Science
{
    class AdvMaterialLabExperiment1 : MaterialLabExperiment
    {
        public override void checkForLabs(bool ready)
        {
            List<PhysicsMaterialsLab> allPhysicsLabs = new List<PhysicsMaterialsLab>(GameObject.FindObjectsOfType(typeof(PhysicsMaterialsLab)) as PhysicsMaterialsLab[]);
            bool physicsLabFound = false;
            foreach (PhysicsMaterialsLab lab in allPhysicsLabs)
            {
                if (lab.vessel == this.vessel)
                {
                    physicsLabFound = true;
                    break;
                }
            }
            //TODO check for ResearchFacility, not public :-(
            if (!ready)
            {
                if (physicsLabFound)
                {
                    this.labFound();
                    return;
                }
                else
                {
                    notReadyStatus = "No Material Lab available";
                }
            }
            if (ready && !physicsLabFound)
            {
                labLost();
            }
        }

        public override void checkUndocked()
        {
            List<PhysicsMaterialsLab> allPhysicsLabs = new List<PhysicsMaterialsLab>(GameObject.FindObjectsOfType(typeof(PhysicsMaterialsLab)) as PhysicsMaterialsLab[]);
            bool labFound = false;
            foreach (PhysicsMaterialsLab lab in allPhysicsLabs)
            {
                if (lab.vessel == this.vessel)
                {
                    labFound = true;
                    break;
                }
            }
            //TODO check for ResearchFacility, not public :-(
            if (!labFound)
            {
                undockedRunningExp();
            }
        }

        public override string GetInfo()
        {
            string ret = base.GetInfo();
            if (ret != "") ret += "\n";
            ret += "You need a TH-NKR Lab to run this Exeriment";

            return ret;
        }
    }
}
