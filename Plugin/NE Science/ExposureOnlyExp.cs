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

namespace NE_Science
{
    class ExposureExperiment:ExperimentCore
    {
        [KSPField(isPersistant = true)]
        public int expID = 0;

        public override void checkForLabs(bool ready)
        {
            List<ExposureLab> allExpLabs = new List<ExposureLab>(GameObject.FindObjectsOfType(typeof(ExposureLab)) as ExposureLab[]);
            bool labFound = false;
            foreach (ExposureLab lab in allExpLabs)
            {
                if (lab.vessel == this.vessel && !lab.running)
                {
                    labFound = true;
                    break;
                }
            }
            if (!ready)
            {
                if (labFound)
                {
                    this.labFound();
                    return;
                }
                else
                {
                    notReadyStatus = "No Exposure Lab available";
                }
            }
            if (ready && !labFound)
            {
                labLost();
            }
        }

        public override void checkUndocked()
        {
            List<ExposureLab> allExpLabs = new List<ExposureLab>(GameObject.FindObjectsOfType(typeof(ExposureLab)) as ExposureLab[]);
            bool labFound = false;
            foreach (ExposureLab lab in allExpLabs)
            {
                if (lab.vessel == this.vessel && lab.running && lab.expID == expID)
                {
                    labFound = true;
                    break;
                }
            }
            if (!labFound)
            {
                NE_Helper.log("Lab lost");
                undockedRunningExp();
                foreach (ExposureLab lab in allExpLabs)
                {
                    NE_Helper.log("Lab expID: " + lab.expID);
                }
            }
        }

        public override void biomeChanged()
        {
            base.biomeChanged();
            stopLab();
        }

        public override void undockedRunningExp()
        {
            base.undockedRunningExp();
            stopLab();
        }

        public override void experimentStarted()
        {
            
            List<ExposureLab> allExpLabs = new List<ExposureLab>(GameObject.FindObjectsOfType(typeof(ExposureLab)) as ExposureLab[]);
            bool labFound = false;
            ExposureLab labf = null;
            NE_Helper.log("Looking for Exposure Lab");
            foreach (ExposureLab lab in allExpLabs)
            {
                if (lab.vessel == this.vessel && !lab.running)
                {
                    labFound = true;
                    labf = lab;
                    NE_Helper.log("Lab found: " + lab.name);
                    break;
                }
            }

            if (labFound)
            {
                NE_Helper.log("starting Lab Experiment");
                expID = new System.Random().Next();
                if (labf != null)
                {
                    labf.startExperiment(this.experiment.experimentTitle, expID);
                    NE_Helper.log("Lab started expID: " + expID);
                }
                else
                {
                    NE_Helper.log("labf null");
                }
                base.experimentStarted();
            }
        }

        public override void finished()
        {
            base.finished();
            stopLab();
        }

        private bool stopLab()
        {
            List<ExposureLab> allExpLabs = new List<ExposureLab>(GameObject.FindObjectsOfType(typeof(ExposureLab)) as ExposureLab[]);
            bool labFound = false;
            foreach (ExposureLab lab in allExpLabs)
            {
                if (lab.expID == expID)
                {
                    labFound = true;
                    lab.stopExperiment();
                    break;
                }
            }
            return labFound;
        }

        public override string GetInfo()
        {
            string ret = base.GetInfo();
            if (ret != "") ret += "\n";
            ret += "You need a NE MEP-825 to run this Exeriment";

            return ret;
        }
    }
}
