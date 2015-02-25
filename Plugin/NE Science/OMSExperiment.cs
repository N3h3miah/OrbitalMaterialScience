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
using System.Text;
using UnityEngine;

namespace NE_Science
{
    public class OMSExperiment : ModuleScienceExperiment
    {
        public const string COMPLETED = "completed";

        public const int NOT_READY = 0;
        public const int READY = 1;
        public const int RUNNING = 2;
        public const int FINISHED = 3;
        public const int FINALIZED = 4;
        public const int ERROR = 5;

        [KSPField(isPersistant = true)]
        public string last_subjectId = "";

        [KSPField(isPersistant = true)]
        public int state = NOT_READY;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
        public string expStatus = "";

        public string notReadyStatus = "No Lab available";
        public string readyStatus = "Lab available";
        public string runningStatus = "Running";
        public string finishedStatus = "Research done";
        public string finalizedStatus = "Finalized";
        public string errorStatus = "Lab Failure";

        [KSPField(isPersistant = true)]
        public float launched = 0;

        [KSPField(isPersistant = true)]
        public float completed = 0;

        public static bool checkBoring(Vessel vessel, bool msg = false)
        {
            if (NE_Helper.debugging())
            {
                return false;
            }
            if ((vessel.orbit.referenceBody.name == "Kerbin") && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH || vessel.situation == Vessel.Situations.SPLASHED || vessel.altitude <= vessel.orbit.referenceBody.maxAtmosphereAltitude))
            {
                if (msg) ScreenMessages.PostScreenMessage("Too boring here. Go to space!", 6, ScreenMessageStyle.UPPER_CENTER);
                return true;
            }
            return false;
        }

    }
}
