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
    public class PhaseExperimentCore : ModuleScienceExperiment
    {
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

        [KSPField(isPersistant = false, guiActive = false, guiName = "Phase")]
        public string phaseStatus = "";

        [KSPField(isPersistant = false, guiActive = false, guiName = "Phase")]
        public string phaseName = "";

        [KSPField(isPersistant = false)]
        public string  phaseConfig;

        public string notReadyStatus = "No Lab available";
        public string readyStatus = "Lab available";
        public string runningStatus = "Running";
        public string finishedStatus = "Research done";
        public string finalizedStatus = "Finalized";
        public string errorStatus = "Lab Failure";

        protected List<ExperimentPhase> phases = new List<ExperimentPhase>();

        [KSPField(isPersistant = true)]
        public int activePhase = 0;


        public PhaseExperimentCore()
        {
            NE_Helper.log("ExperimentPhaseCore C-tor");
            phases.Add(new ExperimentPhase(this, ""));
        }

        private ExperimentPhase getActivePhase()
        {
            return phases.ElementAt(activePhase);
        }

        public static bool checkBoring(Vessel vessel, bool msg = false)
        {
            return false;
            //TODO eneable
            if ((vessel.orbit.referenceBody.name == "Kerbin") && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH || vessel.situation == Vessel.Situations.SPLASHED || vessel.altitude <= vessel.orbit.referenceBody.maxAtmosphereAltitude))
            {
                if (msg) ScreenMessages.PostScreenMessage("Too boring here. Go to space!", 6, ScreenMessageStyle.UPPER_CENTER);
                return true;
            }
            return false;
        }

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
            NE_Helper.log("OnStart");
            base.OnStart(state);
            if (state == StartState.Editor) { return; }
            
            this.part.force_activate();
            switch(this.state){
                case READY:
                    NE_Helper.log("ready");
                    Events["StartExperiment"].active = true;
                    Events["DeployExperiment"].active = false;
                    break;
                case FINISHED:
                    NE_Helper.log("ONStart: Finished");
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = true;
                    break;
            }
            NE_Helper.log("OnStart End");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            updateState();
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            foreach(ExperimentPhase phase in phases){
                phase.save(node);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            NE_Helper.log("PhaseExperimentCore OnLoad");
            base.OnLoad(node);
            NE_Helper.log("PhaseExperimentCore OnLoad setPhases config: " + phaseConfig);
            setPhases();
            foreach (ExperimentPhase phase in phases)
            {
                phase.load(node);
            }
            if (phases.Count > 1)
            {
                Fields["phaseStatus"].guiActive = true;
            }
            else
            {
                Fields["phaseStatus"].guiActive = false;
            }
        }

        protected virtual void setPhases()
        {
            phases = NE_ExperimentPhaseParser.getPhasesFromConfig(phaseConfig, this);
            if (activePhase == 0)
            {
                Events["StartExperiment"].guiName = "Start Experiment";
            }
            else
            {
                Events["StartExperiment"].guiName = "Start Next Phase";
            }
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

        [KSPEvent(guiActive = true, guiName = "Finish Phase", active = false)]
        public void finishPhase()
        {
            getActivePhase().done();
            activePhase++;
            state = READY;
        }

        //Create the resources needed to finish the Experiment. Subclasses should override this.
        public void createResources()
        {
            getActivePhase().createResources();
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

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }

        public void stopResearch(string resName)
        {
            setResourceMaxAmount(resName, 0);
        }

        public void stopResearch()
        {
            getActivePhase().stopResearch();
        }


        public void updateState()
        {
            //NE_Helper.log("[NE] update state: " + state);
            phaseStatus = (activePhase + 1) + " of " + phases.Count;
            if (getActivePhase().hasName())
            {
                Fields["phaseName"].guiActive = true;
                phaseName = getActivePhase().getName();
            }
            else
            {
                Fields["phaseName"].guiActive = false;
            }
            switch (state)
            {
                case NOT_READY:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    Events["finishPhase"].active = false;
                    expStatus = notReadyStatus;
                    checkForLabs(false);
                    break;
                case READY:
                    Events["StartExperiment"].active = true;
                    Events["DeployExperiment"].active = false;
                    Events["finishPhase"].active = false;
                    expStatus = readyStatus;
                    checkForLabs(true);
                    break;
                case RUNNING:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    Events["finishPhase"].active = false;
                    expStatus = runningStatus;
                    checkBiomeChange();
                    checkUndocked();
                    checkFinished();
                    break;
                case FINISHED:
                    Events["StartExperiment"].active = false;
                    if (isLastPhase())
                    {
                        Events["DeployExperiment"].active = deployChecks(false);
                        Events["finishPhase"].active = false;
                    }
                    else
                    {
                        Events["DeployExperiment"].active = false;
                        Events["finishPhase"].active = true;
                    }
                    expStatus = finishedStatus;
                    break;
                case FINALIZED:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    Events["finishPhase"].active = false;
                    expStatus = finalizedStatus;
                    break;
                case ERROR:
                    Events["StartExperiment"].active = false;
                    Events["DeployExperiment"].active = false;
                    Events["finishPhase"].active = false;
                    expStatus = errorStatus;
                    checkLabFixed();
                    break;
            }
        }

        private bool isLastPhase()
        {
            return activePhase + 1 == phases.Count;
        }

        public virtual void checkFinished()
        {
            
            if(getActivePhase().isFinished())
            {
                finished();
            }
        }

        public void checkLabFixed()
        {
            getActivePhase().checkLabFixed();
        }

        public void checkUndocked()
        {
            getActivePhase().checkUndocked();
        }

        public void checkBiomeChange()
        {
            getActivePhase().checkBiomeChange();
        }

        public void checkForLabs(bool ready)
        {
            getActivePhase().checkForLabs(ready);
        }

        public virtual void labLost()
        {
            NE_Helper.log("Lab lost");
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            state = NOT_READY;
        }

        public virtual void biomeChanged()
        {
            NE_Helper.log("biome chaned");
            getActivePhase().biomeChanged();
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            ScreenMessages.PostScreenMessage("Location changed mid-experiment! " + part.partInfo.title + " ruined.", 6, ScreenMessageStyle.UPPER_CENTER);
            stopResearch();
            state = NOT_READY;
        }

        public virtual void undockedRunningExp()
        {
            NE_Helper.log("Exp Undocked");
            getActivePhase().undockedRunningExp();
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            ScreenMessages.PostScreenMessage("Warning: " + part.partInfo.title + " has detached from the station without being finalized.", 2, ScreenMessageStyle.UPPER_CENTER);
            stopResearch();
            state = NOT_READY;
        }

        public virtual void labFound()
        {
            NE_Helper.log("Lab found");
            Events["StartExperiment"].active = true;
            Events["DeployExperiment"].active = false;
            state = READY;
        }

        public virtual bool experimentStarted()
        {
            if (getActivePhase().startExperiment())
            {
                NE_Helper.log("Exp started");
                Events["StartExperiment"].active = false;
                Events["DeployExperiment"].active = false;
                state = RUNNING;
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void finished()
        {
            NE_Helper.log("research finished");
            if (getActivePhase().finished())
            {
                Events["StartExperiment"].active = false;
                Events["DeployExperiment"].active = deployChecks(false);
                state = FINISHED;
            }
        }

        public virtual void error()
        {
            NE_Helper.log("Lab Error");
            
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = deployChecks(false);
            state = ERROR;
        }

        public virtual void labFixed()
        {
            NE_Helper.log("Lab Fixed");
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = deployChecks(false);
            state = RUNNING;
        }

        public virtual void finalized()
        {
            NE_Helper.log("Exp finalized");
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            stopResearch();
            state = FINALIZED;
        }

        public virtual void resetExp()
        {
            NE_Helper.log("Reset");
            ScreenMessages.PostScreenMessage("Reseting Experiment " + part.partInfo.title, 2, ScreenMessageStyle.UPPER_CENTER);
            Events["StartExperiment"].active = false;
            Events["DeployExperiment"].active = false;
            stopResearch();
            state = NOT_READY;
        }

        public override string GetInfo()
        {
            string ret = "";
            int i = 1;
            foreach (ExperimentPhase phase in phases)
            {
                if (phases.Count > 1)
                {
                    ret += "Phase: " + i++ + (phase.hasName() ? " " + phase.getName() + "\n" : "\n");
                }
                ret += phase.getInfo() + "\n\n" ;
            }
            return ret.Trim(); ;
        }

        public UnityEngine.Object[] UnityFindObjectsOfType(Type type)
        {
            return GameObject.FindObjectsOfType(type);
        }

        public int getExperimentID()
        {
            return getActivePhase().getExperimentID();
        }
    }
}