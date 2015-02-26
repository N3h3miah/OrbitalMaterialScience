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
    public class KEESExperiment : OMSExperiment
    {
        /* Overload from OMSExperiment */
        new public string notReadyStatus = "Not installed";
        new public string readyStatus = "Ready";
        new public string errorStatus = "Experiment Ruined";

        [KSPField(isPersistant = false)]
        public int exposureTimeRequired;

        [KSPField(isPersistant = true)]
        public bool docked = false;

        private const string deployAnimation = "Deploy";

        public PartResource getResource(string name)
        {
            return ResourceHelper.getResource(part, name);
        }

        public double getResourceAmount(string name)
        {
            return ResourceHelper.getResourceAmount(part, name);
        }

        public PartResource setResourceMaxAmount(string name, double max)
        {
            return ResourceHelper.setResourceMaxAmount(part, name, max);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor) { return; }
            
            this.part.force_activate();
            switch(this.state){
                case READY:
                    Events["StartExperiment"].active = true;
                    Events["DeployExperiment"].active = false;
                    break;
                case FINISHED:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = true;
                    break;
                case RUNNING:
                    playAnimation(deployAnimation, 1f, 1f);
                    break;
            }
            setEVAconfigForStart(true, 3);
            StartCoroutine(updateStatus());
        }

        public System.Collections.IEnumerator updateStatus()
        {
            while (true)
            {
                updateState();
                yield return new UnityEngine.WaitForSeconds(1f);
            }
        }

        private void setEVAconfigForStart(bool unfocused, float distance)
        {
            Events["StartExperiment"].guiActiveUnfocused = unfocused;
            Events["StartExperiment"].unfocusedRange = distance;
        }


        [KSPEvent(guiActive = true, guiName = "Start Experiment", active = false)]
        public void StartExperiment()
        {
            if (GetScienceCount() > 0)
            {
                ScreenMessages.PostScreenMessage("Experiment already finalized.", 6, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            if (checkBoring(vessel, true)) return;

            if (experimentStarted())
            {
                createResources();
                ScreenMessages.PostScreenMessage("Started experiment!", 6, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public void createResources()
        {
            PartResource exposureTime = setResourceMaxAmount(Resources.EXPOSURE_TIME, exposureTimeRequired);
        }


        public bool deployChecks(bool msg = true)
        {
            if (checkBoring(vessel, true)) return false;
            if (state == FINISHED)
            {
                return true;
            }
            else
            {
                if (msg)
                {
                    ScreenMessages.PostScreenMessage("Experiment not finished yet!", 6, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            return false;
        }

        new public void DeployExperiment()
        {
            if (deployChecks())
            {
                base.DeployExperiment();
                finalized();
            }
        }

        new public void ResetExperiment()
        {
            base.ResetExperiment();
            resetExp();
        }

        new public void ResetExperimentExternal()
        {
            base.ResetExperimentExternal();
            resetExp();
        }

        new public void ResetAction(KSPActionParam p)
        {
            base.ResetAction(p);
            resetExp();
        }

        public void stopResearch(string resName)
        {
            setResourceMaxAmount(resName, 0);
        }

        public void stopResearch()
        {
            stopResearch(Resources.EXPOSURE_TIME);
        }

        public void updateState()
        {
            switch (state)
            {
                case NOT_READY:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    expStatus = notReadyStatus;
                    checkForLabs(false);
                    break;
                case READY:
                    Events["StartExperiment"].active = true;
                    Events["DeployExperiment"].active = false;
                    expStatus = readyStatus;
                    checkForLabs(true);
                    break;
                case RUNNING:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    expStatus = runningStatus;
                    checkBiomeChange();
                    checkUndocked();
                    checkFinished();
                    break;
                case FINISHED:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = deployChecks(false);
                    expStatus = finishedStatus;
                    break;
                case FINALIZED:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    expStatus = finalizedStatus;
                    break;
                case ERROR:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    expStatus = errorStatus;
                    break;
            }
        }

        public virtual void checkFinished()
        {
            
            if(isFinished())
            {
                finished();
            }
        }

        public bool isFinished()
        {
            double numExposureTime = getResourceAmount(Resources.EXPOSURE_TIME);
            return Math.Round(numExposureTime, 2) >= exposureTimeRequired;
        }


        public void checkUndocked()
        {

            if (!docked)
            {
                undockedRunningExp();
            }
        }

        public void checkBiomeChange()
        {
            double numExposureTime = getResourceAmount("ExposureTime");
            int sciCount = GetScienceCount();


            var subject = ScienceHelper.getScienceSubject(experimentID, vessel);
            string subjectId = ((subject == null) ? "" : subject.id);
            if (subjectId != "" && last_subjectId != "" && last_subjectId != subjectId &&
                (numExposureTime > 0))
            {
                biomeChanged();
            }
            last_subjectId = subjectId;
        }

        public void checkForLabs(bool ready)
        {
            if (!ready)
            {
                if (docked)
                {
                    this.labFound();
                }
                else
                {
                    notReadyStatus = "Not installed on a PEC";
                }
            }
            else
            {
                if (!docked)
                {
                    labLost();
                    notReadyStatus = "Not installed on a PEC";
                }
            }
        }

        public virtual void labLost()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            state = NOT_READY;
        }

        public virtual void biomeChanged()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            ScreenMessages.PostScreenMessage("Location changed mid-experiment! " + part.partInfo.title + " ruined.", 6, ScreenMessageStyle.UPPER_CENTER);
            stopResearch();
            playAnimation(deployAnimation, -1, 1);
            state = ERROR;
        }

        public virtual void undockedRunningExp()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            ScreenMessages.PostScreenMessage("Warning: " + part.partInfo.title + " has detached from the station without being finalized.", 2, ScreenMessageStyle.UPPER_CENTER);
            stopResearch();
            playAnimation(deployAnimation, -1, 1);
            state = NOT_READY;
        }

        public virtual void labFound()
        {
            Events["StartExperiment"].active = true;
            Events["DeployExperiment"].active = false;
            state = READY;
        }

        public virtual bool experimentStarted()
        {
                Events["StartExperiment"].active = false;
                Events["DeployExperiment"].active = false;
                state = RUNNING;
                playAnimation(deployAnimation, 1, 0);
                return true;
        }

        public virtual void finished()
        {
                Events["StartExperiment"].active = false;
                Events["DeployExperiment"].active = deployChecks(false);
                state = FINISHED;
                completed = (float)Planetarium.GetUniversalTime();
        }

        public virtual void finalized()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            stopResearch();
            state = FINALIZED;
            playAnimation(deployAnimation, -1, 1);
        }

        public virtual void resetExp()
        {
            ScreenMessages.PostScreenMessage("Reseting Experiment " + part.partInfo.title, 2, ScreenMessageStyle.UPPER_CENTER);
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            stopResearch();
            state = NOT_READY;
        }

        public override string GetInfo()
        {
            String ret = "Exposure time required: " + exposureTimeRequired;

            if (ret != "") ret += "\n";
            ret += "You need to install the experiment on a KEES PEC.";

            return ret;
        }

        public UnityEngine.Object[] UnityFindObjectsOfType(Type type)
        {
            return GameObject.FindObjectsOfType(type);
        }


        public void dockedToPEC(bool docked)
        {
            this.docked = docked;
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


        internal void pecDecoupled()
        {
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            ScreenMessages.PostScreenMessage("Warning: " + part.partInfo.title + " has detached from the vessel.", 2, ScreenMessageStyle.UPPER_CENTER);
            stopResearch();
            playAnimation(deployAnimation, -1, 1);
            state = ERROR;
            docked = false;
        }
    }
}