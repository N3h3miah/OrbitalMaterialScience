/*
 *   This file is part of Orbital Material Science.
 *   
 *   Part of the code may originate from Station Science by ether net http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models)
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
    class MEP_Module : Lab
    {

        [KSPField(isPersistant = true)]
        public int MEPlabState = NE_Helper.MEP_NOT_READY;

        [KSPField(isPersistant = false)]
        public float ExposureTimePerHour = 0;

        [KSPField(isPersistant = false)]
        public bool failures = true;

        [KSPField(isPersistant = false)]
        public int failurePercentage = 1;

        [KSPField(isPersistant = true)]
        [Obsolete]
        public bool running = false;

        [KSPField(isPersistant = true)]
        public int expID = -1;

        [KSPField(isPersistant = true)]
        public int armOps = 0;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Exposure Time")]
        public string exposureTimeStatus = "";

        [KSPField(isPersistant = true, guiActive = true, guiName = "Experiment Name")]
        public string experimentName = "No Experiment";

        private string deployAnimName = "Deploy";
        private string startExpAnimName = "StartExperiment";
        private string errorOnStartAnimName = "ErrorOnStart";
        private string errorOnStopAnimName = "ErrorOnStop";

        public Generator ExposureTimeGenerator;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }

            ExposureTimeGenerator = new Generator(this.part);
            ExposureTimeGenerator.addRate("ExposureTime", -ExposureTimePerHour);
            generators.Add(ExposureTimeGenerator);

            switch (MEPlabState)
            {
                case NE_Helper.MEP_NOT_READY:
                    playAnimation(deployAnimName, -1f, 0f);
                    Events["FixArm"].guiActiveUnfocused = false;
                    Events["DeployPlatform"].guiActive = true;
                    break;
                case NE_Helper.MEP_READY:
                    playAnimation(startExpAnimName, -1f, 0f);
                    Events["FixArm"].guiActiveUnfocused = false;
                    Events["DeployPlatform"].guiActive = true;
                    break;

                case NE_Helper.MEP_RUNNING:
                    playAnimation(startExpAnimName, 1f, 1f);
                    Events["DeployPlatform"].guiActive = false;
                    Events["FixArm"].guiActiveUnfocused = false;
                    break;

                case NE_Helper.MEP_ERROR_ON_START:
                    playAnimation(errorOnStartAnimName, 1f, 1f);
                    Events["FixArm"].guiActiveUnfocused = true;
                    Events["DeployPlatform"].guiActive = false;
                    break;

                case NE_Helper.MEP_ERROR_ON_STOP:
                    playAnimation(errorOnStopAnimName, -1f, 0f);
                    Events["FixArm"].guiActiveUnfocused = true;
                    Events["DeployPlatform"].guiActive = false;
                    break;
            }

        }

        [KSPEvent(guiName = "Fix robotic arm", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void FixArm()
        {
            Events["FixArm"].guiActiveUnfocused = false;
            armOps = 0;
            switch (MEPlabState)
            {
                case NE_Helper.MEP_ERROR_ON_START:
                    MEPlabState = NE_Helper.MEP_RUNNING;
                    playAnimation(errorOnStartAnimName, -1f, 1f);
                    ScreenMessages.PostScreenMessage("Robotic arm fixed. Experiment will start soon.", 2, ScreenMessageStyle.UPPER_CENTER);
                    StartCoroutine(playAninimationAfter(5.8f,startExpAnimName, 1f, 0));
                    break;
                case NE_Helper.MEP_ERROR_ON_STOP:
                    ScreenMessages.PostScreenMessage("Robotic arm fixed.", 2, ScreenMessageStyle.UPPER_CENTER);
                    playAnimation(errorOnStopAnimName, 1f, 0f);
                    StartCoroutine(waitForAnimation(5.8f));
                    break;
            }
        }

        System.Collections.IEnumerator playAninimationAfter(float seconds, string animation, float speed, float normalizedTime)
        {
            yield return new WaitForSeconds(seconds);
            playAnimation(animation, speed, normalizedTime);  
        }

        System.Collections.IEnumerator waitForAnimation(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            MEPlabState = NE_Helper.MEP_RUNNING;

        }


        [KSPEvent(guiActive = true, guiName = "Deploy Platform", active = true)]
        public void DeployPlatform()
        {
            switch (MEPlabState)
            {
                case NE_Helper.MEP_NOT_READY:
                    playAnimation(deployAnimName, 1f, 0);
                    MEPlabState = NE_Helper.MEP_READY;
                    Events["DeployPlatform"].guiName = "Retract Platform";
                    break;
                case NE_Helper.MEP_READY:
                    playAnimation(deployAnimName, -1f, 1);
                    MEPlabState = NE_Helper.MEP_NOT_READY;
                    Events["DeployPlatform"].guiName = "Deploy Platform";
                    break;
            }
        }



        public bool isRunning()
        {
            return MEPlabState == NE_Helper.MEP_RUNNING;
        }

        public bool isReady()
        {
            return MEPlabState == NE_Helper.MEP_READY;
        }

        public bool hasError()
        {
            return MEPlabState == NE_Helper.MEP_ERROR_ON_START || MEPlabState == NE_Helper.MEP_ERROR_ON_STOP;
        }

        protected override bool isActive()
        {
            return doResearch && isRunning() && part.protoModuleCrew.Count >= minimumCrew && !PhaseExperimentCore.checkBoring(vessel, false);
        }

        protected override void updateLabStatus()
        {
            switch (MEPlabState)
            {
                case NE_Helper.MEP_NOT_READY:
                    displayStatusMessage("Platform retracted");
                    break;
                case NE_Helper.MEP_READY:
                    Events["DeployPlatform"].guiActive = true;
                    Events["FixArm"].guiActiveUnfocused = false;
                    displayStatusMessage("Idle");
                    break;
                case NE_Helper.MEP_RUNNING:
                    Fields["labStatus"].guiActive = false;
                    Fields["exposureTimeStatus"].guiActive = true;
                    Fields["experimentName"].guiActive = true;
                    Events["DeployPlatform"].guiActive = false;
                    Events["FixArm"].guiActiveUnfocused = false;
                    exposureTimeStatus = "";
                    var er = getOrDefault(ExposureTimeGenerator.rates, "ExposureTime");
                    if (er != null)
                    {
                        if (er.last_available == 0)
                            exposureTimeStatus = "No Experiments";
                        else
                            exposureTimeStatus = String.Format("{0:F2} per hour", -er.ratePerHour * er.rateMultiplier);
                    }
                    Fields["exposureTimeStatus"].guiActive = (exposureTimeStatus != "");
                    checkStatusLabRunning();
                    break;
                case NE_Helper.MEP_ERROR_ON_START:
                case NE_Helper.MEP_ERROR_ON_STOP:
                    Events["DeployPlatform"].guiActive = false;
                    Events["FixArm"].guiActiveUnfocused = true;
                    displayStatusMessage("Robotic Arm Failure");
                    break;
            }
            Events["FixArm"].active = Events["FixArm"].guiActiveUnfocused;
        }

        private void checkStatusLabRunning()
        {
            switch (MEPlabState)
            {
                case NE_Helper.MEP_RUNNING:
                    List<PhaseExperimentCore> allExps = new List<PhaseExperimentCore>(GameObject.FindObjectsOfType(typeof(PhaseExperimentCore)) as PhaseExperimentCore[]);
                    bool expFound = false;
                    foreach (PhaseExperimentCore exp in allExps)
                    {
                        if (exp.getExperimentID() == expID)
                        {
                            expFound = true;
                            break;
                        }
                    }
                    if (!expFound)
                    {
                        stopExperiment(false);
                    }
                    break;
            }
        }

        protected override void displayStatusMessage(string s)
        {
            base.displayStatusMessage(s);
            Fields["exposureTimeStatus"].guiActive = false;
            Fields["experimentName"].guiActive = false;
        }

        public bool startExperiment(string name, int expIDp)
        {
            if (isSuccessfull())
            {
                MEPlabState = NE_Helper.MEP_RUNNING;
                experimentName = name;
                expID = expIDp;
                playAnimation(startExpAnimName, 1f, 0f);
                armOps++;
                return true;
            }
            else
            {
                armOps++;
                errorOnStart(expIDp, name);
                return false;
            }
        }

        public bool stopExperiment(bool finished)
        {
            if (finished)
            {
                if (isSuccessfull())
                {
                    armOps++;
                    MEPlabState = NE_Helper.MEP_READY;
                    experimentName = "No Experiment";
                    expID = -1;
                    playAnimation(startExpAnimName, -1f, 1f);
                    return true;
                }
                else
                {
                    armOps++;
                    errorOnStop();
                    return false;
                }
            }
            else
            {
                MEPlabState = NE_Helper.MEP_READY;
                experimentName = "No Experiment";
                expID = -1;
                playAnimation(startExpAnimName, -1f, 1f);
                    return true;
            }
        }

        public void errorOnStart(int expIDp, string name)
        {
            MEPlabState = NE_Helper.MEP_RUNNING;
            experimentName = name;
            expID = expIDp;
            playAnimation(errorOnStartAnimName, 1f, 0f);
            StartCoroutine(ErrorCallback(5.8f, NE_Helper.MEP_ERROR_ON_START));
        }

        public void errorOnStop()
        {
            MEPlabState = NE_Helper.MEP_ERROR_ON_STOP;
            playAnimation(errorOnStopAnimName, -1f, 1);
            StartCoroutine(ErrorCallback(5.5f, NE_Helper.MEP_ERROR_ON_STOP));

        }

        System.Collections.IEnumerator ErrorCallback(float seconds, int targetState)
        {
            yield return new WaitForSeconds(seconds);
            ScreenMessages.PostScreenMessage("Warning: robotic arm failure", 6, ScreenMessageStyle.UPPER_CENTER);
            MEPlabState = targetState;
        }

        //private bool tempSuc = false;
        private bool isSuccessfull()
        {
            //bool ret = tempSuc;
            //tempSuc = !tempSuc;
            //return ret;
            if (failures)
            {
                if (failurePercentage > 100)
                {
                    failurePercentage = 100;
                }
                if (failurePercentage < 1)
                {
                    failurePercentage = 1;
                }
                int actFailurePerc = failurePercentage + (int)(armOps * 1.5f);
                int i = new System.Random().Next(1, (100 / actFailurePerc + 1));
                NE_Helper.log("ExpLab is successfull: " + !(i == 1) + " ; " + i + " ; percentage: " + failurePercentage + "% armOps: " + armOps + " actual percentage: " + actFailurePerc + "%:");
                return !(i == 1);
            }
            else
            {
                return true;
            }
        }

        private void playAnimation(string animName, float speed, float time)
        {
            Animation anim = part.FindModelAnimators(animName).FirstOrDefault();
            if (anim != null)
            {
                anim[animName].speed = speed;
                anim[animName].normalizedTime = time;

                anim.Play(animName);
            }
            else
            {
                NE_Helper.logError("no Animation; Name: " + animName);
            }
        }

       public override string GetInfo()
        {
            String ret = base.GetInfo();
            ret += (ret == "" ? "" : "\n") + "Exposure Time per hour: " + ExposureTimePerHour;
            return ret;
        }
    }
}
