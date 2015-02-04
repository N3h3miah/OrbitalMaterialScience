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
using UnityEngine;

namespace NE_Science
{
    public interface ExperimentDataStorage
    {
        void removeExperimentData();

        GameObject getPartGo();
    }

    public class MoveableExperiment : ModuleScienceExperiment, ExperimentDataStorage
    {

        [KSPField(isPersistant = false)]
        public string identifier = "";

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Contains")]
        public string contains = "";

        private ExperimentData expData = ExperimentData.getNullObject();
        private int count = 0;

        private List<ExperimentData> availableExperiments
            = new List<ExperimentData>();
        private bool showGui = false;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("MoveableExperiment: OnLoad");

            ConfigNode expNode = node.GetNode(ExperimentData.CONFIG_NODE_NAME);
            if (expNode != null)
            {
                setExperiment(ExperimentData.getExperimentDataFromNode(expNode));
            }
            else
            {
                setExperiment(ExperimentData.getNullObject());
            }
        }

        private void setExperiment(ExperimentData experimentData)
        {
            expData = experimentData;
            expData.setStorage(this);
            experimentID = expData.getId();
            contains = expData.getAbbreviation();

            experimentActionName = "Finalize Results";
            resetActionName = "Throw Away Results";

            useStaging = false;
            useActionGroups = true;
            hideUIwhenUnavailable = true;
            resettable = true;
            resettableOnEVA = true;

            dataIsCollectable = false;
            collectActionName = "Collect Results";
            interactionRange = 1.2f;
            xmitDataScalar = 0.2f;
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            node.AddNode(expData.getNode());
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            NE_Helper.log("MoveableExperiment: OnStart");
            if (state.Equals(StartState.Editor))
            {
                Events["chooseEquipment"].active = true;
            }
            else
            {
                Events["chooseEquipment"].active = false;
            }
            
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (count == 0)
            {
                Events["DeployExperiment"].active = expData.canFinalize();
                Events["installExperiment"].active = expData.canInstall(part.vessel);
                Events["moveExp"].active = expData.canMove(part.vessel);
            }
            count = (count + 1) % 3;

        }

        [KSPEvent(guiActiveEditor = true, guiName = "Add Experiment", active = false)]
        public void chooseEquipment()
        {
            if (expData.getId() == "")
            {
                availableExperiments = ExperimentFactory.getAvailableExperiments();
                showGui = true;
                Events["chooseEquipment"].guiName = "Remove Experiment";
            }
            else
            {
                setExperiment(ExperimentData.getNullObject());
                Events["chooseEquipment"].guiName = "Add Experiment";
            }
        }

        [KSPEvent(guiActive = true, guiName = "Install Experiment", active = false)]
        public void installExperiment()
        {
            List<Lab> labs = expData.getFreeLabsWithEquipment(part.vessel);
            if (labs.Count > 0)
            {
                labs[0].installExperiment(expData);
                setExperiment(ExperimentData.getNullObject());
            }
            else
            {
                NE_Helper.logError("Experiment install: No lab found");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Move Experiment", active = false)]
        public void moveExp()
        {
            expData.move(part.vessel);
        }

        new public void DeployExperiment()
        {
            if (expData.canFinalize() )
            {
                base.DeployExperiment();
                expData.finalize();
            }
        }

        void OnGUI()
        {
            if (showGui)
            {
                GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 250, 200, 500));
                GUI.Box(new Rect(0, 0, 200, 500), "Add Experiment");
                int top = 40;
                foreach (ExperimentData e in availableExperiments)
                {
                    if (GUI.Button(new Rect(10, top, 180, 30), e.getAbbreviation()))
                    {
                        setExperiment(e);
                        showGui = false;
                    }
                    top += 35;
                }
                top += 20;
                if (GUI.Button(new Rect(10, top, 180, 30), "Close"))
                {
                    showGui = false;
                }
                GUI.EndGroup();
            }
        }

        public bool isEmpty()
        {
            return expData == null || expData.getId() == "";
        }

        internal void storeExperiment(ExperimentData experimentData)
        {
            setExperiment(experimentData);
        }

        public void removeExperimentData()
        {
            setExperiment(ExperimentData.getNullObject());
        }

        public GameObject getPartGo()
        {
            return part.gameObject;
        }
    }
}
