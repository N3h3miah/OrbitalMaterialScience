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
                NE_Helper.log("Lab lost");
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
            NE_Helper.log("Looking for Exposure Lab");
            foreach (ExposureLab lab in allExpLabs)
            {
                if (lab.vessel == exp.vessel && lab.isReady())
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
                if (expID == Int32.MinValue)
                {
                    expID = new System.Random().Next();
                }
                if (labf != null)
                {
                    labf.startExperiment(exp.experiment.experimentTitle, expID);
                    NE_Helper.log("Lab started expID: " + expID);
                    return true;

                }
                else
                {
                    NE_Helper.log("labf null");
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
            NE_Helper.log("ExpExp load, calling base");
            base.load(node);
            string expIdstring = node.GetValue(EXP_ID_STRING);
            NE_Helper.log("ExpExp load, ExpID String: " + expIdstring);
            expID = Convert.ToInt32(expIdstring);
            NE_Helper.log("ExpExp load, ExpID: " + expID);
        }

        public override int getExperimentID()
        {
            return expID;
        }
    }
}
