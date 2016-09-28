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
        private const string MEP_STATE_VALUE = "Mep_State";
        private const string SLOT_CONFIG_NODE_NAME = "EquipmentSlot";

        public MEPLabStatus MEPlabState = MEPLabStatus.NOT_READY;

        [KSPField(isPersistant = false)]
        public float ExposureTimePerHour = 0;

        [KSPField(isPersistant = false)]
        public bool failures = true;

        [KSPField(isPersistant = false)]
        public int failurePercentage = 1;

        [KSPField(isPersistant = true)]
        public int armOps = 0;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Experiment")]
        public string experimentName = "No Experiment";

        private string deployAnimName = "Deploy";
        private string startExpAnimName = "StartExperiment";
        private string errorOnStartAnimName = "ErrorOnStart";
        private string errorOnStopAnimName = "ErrorOnStop";

        private LabEquipmentSlot exposureSlot = new LabEquipmentSlot(EquipmentRacks.EXPOSURE);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            string stateString = node.GetValue(MEP_STATE_VALUE);
            if (stateString != null)
            {
                MEPlabState = MEPLabStatusFactory.getType(stateString);
            }
            exposureSlot = getLabEquipmentSlot(node.GetNode(SLOT_CONFIG_NODE_NAME));
            
        }


        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("MEP OnSave");
            node.AddValue(MEP_STATE_VALUE, MEPlabState);
            node.AddNode(getConfigNodeForSlot(SLOT_CONFIG_NODE_NAME, exposureSlot));
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }
            NE_Helper.log("MEP_Module OnStart");
            exposureSlot.onStart(this);
            if (!exposureSlot.isEquipmentInstalled())
            {
                exposureSlot.install(new LabEquipment("MEP", "MEP", EquipmentRacks.EXPOSURE, 0f, 0f, ExposureTimePerHour, Resources.EXPOSURE_TIME, 0, ""), this); ;
            }

            switch (MEPlabState)
            {
                case MEPLabStatus.NOT_READY:
                    playAnimation(deployAnimName, -1f, 0f);
                    Events["FixArm"].guiActiveUnfocused = false;
                    Events["DeployPlatform"].guiActive = true;
                    break;
                case MEPLabStatus.READY:
                    playAnimation(startExpAnimName, -1f, 0f);
                    Events["FixArm"].guiActiveUnfocused = false;
                    Events["DeployPlatform"].guiActive = true;
                    break;

                case MEPLabStatus.RUNNING:
                    playAnimation(startExpAnimName, 1f, 1f);
                    Events["DeployPlatform"].guiActive = false;
                    Events["FixArm"].guiActiveUnfocused = false;
                    break;

                case MEPLabStatus.ERROR_ON_START:
                    playAnimation(errorOnStartAnimName, 1f, 1f);
                    Events["FixArm"].guiActiveUnfocused = true;
                    Events["DeployPlatform"].guiActive = false;
                    break;

                case MEPLabStatus.ERROR_ON_STOP:
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
                case MEPLabStatus.ERROR_ON_START:
                    MEPlabState = MEPLabStatus.RUNNING;
                    playAnimation(errorOnStartAnimName, -1f, 1f);
                    ScreenMessages.PostScreenMessage("Robotic arm fixed. Experiment will start soon.", 2, ScreenMessageStyle.UPPER_CENTER);
                    StartCoroutine(playAninimationAfter(5.8f,startExpAnimName, 1f, 0));
                    break;
                case MEPLabStatus.ERROR_ON_STOP:
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
            MEPlabState = MEPLabStatus.RUNNING;

        }


        [KSPEvent(guiActive = true, guiName = "Deploy Platform", active = true)]
        public void DeployPlatform()
        {
            switch (MEPlabState)
            {
                case MEPLabStatus.NOT_READY:
                    playAnimation(deployAnimName, 1f, 0);
                    MEPlabState = MEPLabStatus.READY;
                    Events["DeployPlatform"].guiName = "Retract Platform";
                    break;
                case MEPLabStatus.READY:
                    playAnimation(deployAnimName, -1f, 1);
                    MEPlabState = MEPLabStatus.NOT_READY;
                    Events["DeployPlatform"].guiName = "Deploy Platform";
                    break;
            }
        }

        public override void installExperiment(ExperimentData exp)
        {
            if (exp.getEquipmentNeeded() == EquipmentRacks.EXPOSURE)
            {
                exposureSlot.installExperiment(exp);
                experimentName = exp.getAbbreviation() + ": " + exp.getStateString();
            }
        }

        public LabEquipmentSlot getExposureSlot()
        {
            return exposureSlot;
        }

        public bool isRunning()
        {
            return MEPlabState == MEPLabStatus.RUNNING;
        }

        public bool isReady()
        {
            return MEPlabState == MEPLabStatus.READY;
        }

        public bool hasError()
        {
            return MEPlabState == MEPLabStatus.ERROR_ON_START || MEPlabState == MEPLabStatus.ERROR_ON_STOP;
        }

        protected override bool isActive()
        {
            return doResearch && isRunning() && part.protoModuleCrew.Count >= minimumCrew && !OMSExperiment.checkBoring(vessel, false);
        }

        protected override void updateLabStatus()
        {
            switch (MEPlabState)
            {
                case MEPLabStatus.NOT_READY:
                    displayStatusMessage("Platform retracted");
                    break;
                case MEPLabStatus.READY:
                    Events["DeployPlatform"].guiActive = true;
                    Events["FixArm"].guiActiveUnfocused = false;
                    displayStatusMessage("Idle");
                    break;
                case MEPLabStatus.RUNNING:
                    Fields["labStatus"].guiActive = false;
                    Events["DeployPlatform"].guiActive = false;
                    Events["FixArm"].guiActiveUnfocused = false;
                    displayStatusMessage("Running");
                    break;
                case MEPLabStatus.ERROR_ON_START:
                case MEPLabStatus.ERROR_ON_STOP:
                    Events["DeployPlatform"].guiActive = false;
                    Events["FixArm"].guiActiveUnfocused = true;
                    displayStatusMessage("Robotic Arm Failure");
                    break;
            }

            Fields["experimentName"].guiActive = !exposureSlot.experimentSlotFree();
            experimentName = exposureSlot.getExperiment().getAbbreviation() + ": " + exposureSlot.getExperiment().getStateString();

            Events["moveExp"].active = exposureSlot.canExperimentMove(part.vessel);
            if (Events["moveExp"].active)
            {
                Events["moveExp"].guiName = "Move " + exposureSlot.getExperiment().getAbbreviation();
            }

            Events["actionExp"].active = exposureSlot.canActionRun();
            if (Events["actionExp"].active)
            {
                Events["actionExp"].guiName = exposureSlot.getActionString();
            }
            

            Events["FixArm"].active = Events["FixArm"].guiActiveUnfocused;
        }

        public void errorOnStart()
        {
            MEPlabState = MEPLabStatus.RUNNING;
            playAnimation(errorOnStartAnimName, 1f, 0f);
            StartCoroutine(ErrorCallback(5.8f, MEPLabStatus.ERROR_ON_START));
        }

        public void errorOnStop()
        {
            playAnimation(errorOnStopAnimName, -1f, 1);
            StartCoroutine(ErrorCallback(5.5f, MEPLabStatus.ERROR_ON_STOP));

        }

        System.Collections.IEnumerator ErrorCallback(float seconds, MEPLabStatus targetState)
        {
            yield return new WaitForSeconds(seconds);
            ScreenMessages.PostScreenMessage("Warning: robotic arm failure", 6, ScreenMessageStyle.UPPER_CENTER);
            MEPlabState = targetState;
        }

        private bool isSuccessfull()
        {

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

       internal bool hasEquipmentFreeExperimentSlot(EquipmentRacks neededEquipment)
       {
           return exposureSlot.experimentSlotFree();
       }

       [KSPEvent(guiActive = true, guiName = "Move Experiment", active = false)]
       public void moveExp()
       {
           exposureSlot.moveExperiment(part.vessel);
       }

       [KSPEvent(guiActive = true, guiName = "Action Experiment", active = false)]
       public void actionExp()
       {
           if (exposureSlot.isExposureAction())
           {
               if (isSuccessfull())
               {
                   exposureSlot.experimentAction();
                   ++armOps;
                   switch (MEPlabState)
                   {
                       case MEPLabStatus.READY:
                           playAnimation(startExpAnimName, 1f, 0f);
                           MEPlabState = MEPLabStatus.RUNNING;
                           break;
                       case MEPLabStatus.RUNNING:
                           playAnimation(startExpAnimName, -1f, 1f);
                           MEPlabState = MEPLabStatus.READY;
                           break;
                   }


               }
               else
               {
                   switch (MEPlabState)
                   {
                       case MEPLabStatus.READY:
                           errorOnStart();
                           break;
                       case MEPLabStatus.RUNNING:
                           errorOnStop();
                           break;
                   }
               }
           }
           else
           {
               exposureSlot.experimentAction();
           }
       }
    }

    public enum MEPLabStatus
    {
        NOT_READY, READY, RUNNING, ERROR_ON_START, ERROR_ON_STOP, NONE
    }

    public class MEPLabStatusFactory
    {

        public static MEPLabStatus getType(string p)
        {
            switch (p)
            {
                case "NOT_READY":
                    return MEPLabStatus.NOT_READY;
                case "READY":
                    return MEPLabStatus.READY;
                case "RUNNING":
                    return MEPLabStatus.RUNNING;
                case "ERROR_ON_START":
                    return MEPLabStatus.ERROR_ON_START;
                case "ERROR_ON_STOP":
                    return MEPLabStatus.ERROR_ON_STOP;
                case "NONE":
                    return MEPLabStatus.NONE;
                default:
                    return MEPLabStatus.NOT_READY;

            }
        }
    }
}
