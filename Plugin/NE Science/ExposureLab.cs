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
    class ExposureLab : Lab
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

        [KSPField(isPersistant = false, guiActive = false, guiName = "Exposure Time")]
        public string exposureTimeStatus = "";

        [KSPField(isPersistant = true, guiActive = true, guiName = "Experiment Name")]
        public string experimentName = "No Experiment";

        private string deployAnimName = "Deploy";
        private string startExpAnimName = "StartExperiment";
        private string errorOnStartAnimName = "ErrorOnStart";
        private string errorOnStopAnimName = "ErrorOnStop";
        private Light warnLight;
        private Light warnPointLight;

        public Generator ExposureTimeGenerator;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }

            foreach (Light child in gameObject.GetComponentsInChildren(typeof(Light)))
            {
                if (child.name == "rotationLight")
                {
                    warnLight = child;
                }
                else if (child.name == "WarnPointLlight")
                {
                    warnPointLight = child;
                }

            }
            ExposureTimeGenerator = new Generator(this.part);
            ExposureTimeGenerator.addRate("ExposureTime", -ExposureTimePerHour);
            generators.Add(ExposureTimeGenerator);

            switch (MEPlabState)
            {
                case NE_Helper.MEP_NOT_READY:
                    NE_Helper.log("onStart Lab NOT READY Set Animationstate");
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
                    NE_Helper.log("Running Lab onStart. Set Animationstate");
                    playAnimation(startExpAnimName, 1f, 1f);
                    Events["DeployPlatform"].guiActive = false;
                    Events["FixArm"].guiActiveUnfocused = false;
                    break;

                case NE_Helper.MEP_ERROR_ON_START:
                    NE_Helper.log("onStart Lab ERROR Set Animationstate");
                    playAnimation(errorOnStartAnimName, 1f, 1f);
                    startWarnLights();
                    Events["FixArm"].guiActiveUnfocused = true;
                    Events["DeployPlatform"].guiActive = false;
                    break;

                case NE_Helper.MEP_ERROR_ON_STOP:
                    NE_Helper.log("onStart Lab ERROR Set Animationstate");
                    playAnimation(errorOnStopAnimName, -1f, 0f);
                    startWarnLights();
                    Events["FixArm"].guiActiveUnfocused = true;
                    Events["DeployPlatform"].guiActive = false;
                    break;
            }

        }

        [KSPEvent(guiName = "Fix robotic arm", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void FixArm()
        {
            Events["FixArm"].guiActiveUnfocused = false;
            switch (MEPlabState)
            {
                case NE_Helper.MEP_ERROR_ON_START:
                    MEPlabState = NE_Helper.MEP_READY;
                    playAnimation(errorOnStartAnimName, -1f, 1f);
                    break;
                case NE_Helper.MEP_ERROR_ON_STOP:
                    MEPlabState = NE_Helper.MEP_RUNNING;
                    playAnimation(errorOnStopAnimName, 1f, 0f);
                    break;
            }
            stopWarnLights();
        }


        [KSPEvent(guiActive = true, guiName = "Deploy Platform", active = true)]
        public void DeployPlatform()
        {
            NE_Helper.log("Deploy Platform, state: " + MEPlabState);
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
            return doResearch && isRunning() && part.protoModuleCrew.Count >= minimumCrew && !ExperimentCore.checkBoring(vessel, false);
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
                    warnLight.transform.Rotate(Time.deltaTime * 180, 0, 0);
                    break;
            }
            Events["FixArm"].active = Events["FixArm"].guiActiveUnfocused;
        }

        private void checkStatusLabRunning()
        {
            switch (MEPlabState)
            {
                case NE_Helper.MEP_RUNNING:
                    List<ExposureExperiment> allExpExps = new List<ExposureExperiment>(GameObject.FindObjectsOfType(typeof(ExposureExperiment)) as ExposureExperiment[]);
                    bool expFound = false;
                    foreach (ExposureExperiment exp in allExpExps)
                    {
                        if (exp.expID == expID)
                        {
                            expFound = true;
                            break;
                        }
                    }
                    if (!expFound)
                    {
                        NE_Helper.log("Running experiment lost.");
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
            NE_Helper.log("Starting Experiment: " + name);
            if (isSuccessfull())
            {
                MEPlabState = NE_Helper.MEP_RUNNING;
                experimentName = name;
                expID = expIDp;
                playAnimation(startExpAnimName, 1f, 0f);
                NE_Helper.log("Experiment Started");
                return true;
            }
            else
            {
                errorOnStart(expIDp, name);
                NE_Helper.log("Failure during start");
                return false;
            }
        }

        public bool stopExperiment(bool finished)
        {
            if (finished)
            {
                if (isSuccessfull())
                {
                    MEPlabState = NE_Helper.MEP_READY;
                    experimentName = "No Experiment";
                    expID = -1;
                    playAnimation(startExpAnimName, -1f, 1f);
                    return true;
                }
                else
                {
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
            NE_Helper.log("Error On Start");
            MEPlabState = NE_Helper.MEP_RUNNING;
            experimentName = name;
            expID = expIDp;
            playAnimation(errorOnStartAnimName, 1f, 0f);
            NE_Helper.log("Calling Callback");
            StartCoroutine(ErrorCallback(5.8f, NE_Helper.MEP_ERROR_ON_START));
        }

        public void errorOnStop()
        {
            NE_Helper.log("Error On Stop");
            MEPlabState = NE_Helper.MEP_ERROR_ON_STOP;
            playAnimation(errorOnStopAnimName, -1f, 1);
            NE_Helper.log("Calling Callback");
            StartCoroutine(ErrorCallback(5.5f, NE_Helper.MEP_ERROR_ON_STOP));

        }

        private void startWarnLights()
        {

            if (warnLight != null)
            {
                warnLight.intensity = 6f;
            }
            else
            {
                NE_Helper.log("WarnLight null");
            }
            if (warnPointLight != null)
            {
                warnPointLight.intensity = 0.5f;
            }
            else
            {
                NE_Helper.log("WarnPointLight null");
            }
        }

        private void stopWarnLights()
        {

            if (warnLight != null)
            {
                warnLight.intensity = 0f;
            }
            else
            {
                NE_Helper.log("WarnLight null");
            }
            if (warnPointLight != null)
            {
                warnPointLight.intensity = 0.0f;
            }
            else
            {
                NE_Helper.log("WarnPointLight null");
            }
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

                int i = new System.Random().Next(1, (100 / failurePercentage)+1);
                NE_Helper.log("ExpLab is successfull: " + !(i == 1) + " ; " + i + " ; percentage: " + failurePercentage);
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
                NE_Helper.log("no Animation; Name: " + errorOnStartAnimName);
            }
        }

        System.Collections.IEnumerator ErrorCallback(float seconds, int targetState)
        {
            NE_Helper.log("Wait for animation: " + seconds);
            yield return new WaitForSeconds(seconds);
            NE_Helper.log("Time over");
            startWarnLights();
            ScreenMessages.PostScreenMessage("Warning: robotic arm failure", 6, ScreenMessageStyle.UPPER_CENTER);
            MEPlabState = targetState;
        }

        public override string GetInfo()
        {
            String ret = base.GetInfo();
            ret += (ret == "" ? "" : "\n") + "Exposure Time per hour: " + ExposureTimePerHour;
            return ret;
        }
    }
}
