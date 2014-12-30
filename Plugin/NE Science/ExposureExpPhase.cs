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
    class ExposureExpPhase : ExperimentPhase
    {
        private const string EXP_ID_STRING = "ExpID";
        public int expID = 0;

        private int exposureTimeRequired;

        public ExposureExpPhase(PhaseExperimentCore exp, string n, int exposureTime)
            : base(exp, n)
        {
            exposureTimeRequired = exposureTime;
        }

        public override void checkForLabs(bool ready)
        {
            List<ExposureLab> allExpLabs = new List<ExposureLab>(exp.UnityFindObjectsOfType(typeof(ExposureLab)) as ExposureLab[]);
            bool labFound = false;
            
            if (!ready)
            {
                if (labFound)
                {
                    exp.labFound();
                    return;
                }
                else
                {
                    exp.notReadyStatus = "Install experiment on a KEES PEC";
                }
            }
        }

        public override void checkUndocked()
        {
            List<ExposureLab> allExpLabs = new List<ExposureLab>(exp.UnityFindObjectsOfType(typeof(ExposureLab)) as ExposureLab[]);
            bool labFound = false;
            foreach (ExposureLab lab in allExpLabs)
            {
                if (lab.vessel == exp.vessel && lab.isRunning() && lab.expID == expID)
                {
                    labFound = true;
                    break;
                }
                else if (lab.vessel == exp.vessel && lab.hasError() && lab.expID == expID)
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

        public override void checkLabFixed()
        {
            List<ExposureLab> allExpLabs = new List<ExposureLab>(exp.UnityFindObjectsOfType(typeof(ExposureLab)) as ExposureLab[]);
            foreach (ExposureLab lab in allExpLabs)
            {
                if (lab.vessel == exp.vessel && lab.isRunning() && lab.expID == expID)
                {
                    exp.labFixed();
                    break;
                }
            }
        }

        public override void biomeChanged()
        {
            stopLab(false);
        }

        public override void undockedRunningExp()
        {
            stopLab(false);
        }

        public override bool startExperiment()
        {

            List<ExposureLab> allExpLabs = new List<ExposureLab>(exp.UnityFindObjectsOfType(typeof(ExposureLab)) as ExposureLab[]);
            bool labFound = false;
            ExposureLab labf = null;
            foreach (ExposureLab lab in allExpLabs)
            {
                if (lab.vessel == exp.vessel && lab.isReady())
                {
                    labFound = true;
                    labf = lab;
                    break;
                }
            }

            if (labFound)
            {
                expID = new System.Random().Next();
                if (expID == Int32.MinValue)
                {
                    expID = new System.Random().Next();
                }
                if (labf != null)
                {
                    labf.startExperiment(exp.experiment.experimentTitle, expID);
                    return true;

                }
                else
                {
                    return false;
                }

            }
            return false;
        }

        public override bool finished()
        {
            if (stopLab(true))
            {
                return true;
            }
            else
            {
                exp.error();
                return false;
            }
        }

        private bool stopLab(bool finished)
        {
            List<ExposureLab> allExpLabs = new List<ExposureLab>(exp.UnityFindObjectsOfType(typeof(ExposureLab)) as ExposureLab[]);
            bool labFound = false;
            foreach (ExposureLab lab in allExpLabs)
            {
                if (lab.expID == expID)
                {
                    labFound = true;
                    return lab.stopExperiment(finished);
                }
            }
            return labFound;
        }

        public override void createResources()
        {
            PartResource exposureTime = exp.setResourceMaxAmount(Resources.EXPOSURE_TIME, exposureTimeRequired);
        }

        public override bool isFinished()
        {
            double numExposureTime = exp.getResourceAmount(Resources.EXPOSURE_TIME);
            return Math.Round(numExposureTime, 2) >= exposureTimeRequired;
        }

        public override void stopResearch()
        {
            exp.stopResearch(Resources.EXPOSURE_TIME);
        }

        public override string getInfo()
        {
            String ret = "Exposure time required: " + exposureTimeRequired;

            if (ret != "") ret += "\n";
            ret += "You need a NE MEP-825 to run this Exeriment";

            return ret;
        }

        public override void save(ConfigNode node)
        {
            base.save(node);
            node.AddValue(EXP_ID_STRING, expID);
        }

        public override void load(ConfigNode node)
        {
            base.load(node);
            string expIdstring = node.GetValue(EXP_ID_STRING);
            expID = Convert.ToInt32(expIdstring);
        }

        public override int getExperimentID()
        {
            return expID;
        }
    }
}
