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

    public class ExperimentStorage : ModuleScienceExperiment, ExperimentDataStorage
    {

        [KSPField(isPersistant = false)]
        public string identifier = "";

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Contains")]
        public string contains = "";

        private ExperimentData expData = ExperimentData.getNullObject();
        private int count = 0;

        private List<ExperimentData> availableExperiments
            = new List<ExperimentData>();
        List<Lab> availableLabs = new List<Lab>();

        private int showGui = 0;
        private Rect finalizeWindowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200);
        private Rect addWindowRect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 250, 200, 400);
        private Vector2 addScrollPos = new Vector2();
        private Rect labWindowRect = new Rect(Screen.width - 250, Screen.height / 2 - 250, 200, 400);
        private Vector2 labScrollPos = new Vector2();
        

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("ExperimentStorage: OnLoad");

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
            NE_Helper.log("MOVExp.setExp() id: " + experimentData.getId());
            expData = experimentData;
            contains = expData.getAbbreviation();
            expData.setStorage(this);

            experimentID = expData.getId();
            experiment = ResearchAndDevelopment.GetExperiment(experimentID);

            experimentActionName = "Results";
            resetActionName = "Throw Away Results";

            useStaging = false;
            useActionGroups = true;
            hideUIwhenUnavailable = true;
            resettable = false;
            resettableOnEVA = false;

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
            if (state.Equals(StartState.Editor))
            {
                Events["chooseEquipment"].active = true;
            }
            else
            {
                Events["chooseEquipment"].active = false;
            }
            Events["DeployExperiment"].active = false;
        }

        public override void OnUpdate()
        {

            base.OnUpdate();
            if (count == 0)
            {
                Events["installExperiment"].active = expData.canInstall(part.vessel);
                Events["moveExp"].active = expData.canMove(part.vessel);
                Events["finalize"].active = expData.canFinalize();
                Events["DeployExperiment"].active = false;
                if (expData.state == ExperimentState.FINISHED && GetScienceCount() > 0)
                {
                    NE_Helper.log("onupdate: setState to FINALIZED");
                    expData.state = ExperimentState.FINALIZED;
                }
            }
            count = (count + 1) % 3;

        }

        [KSPEvent(guiActiveEditor = true, guiName = "Add Experiment", active = false)]
        public void chooseEquipment()
        {
            if (expData.getId() == "")
            {
                availableExperiments = ExperimentFactory.getAvailableExperiments();
                showGui = 1;
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
            availableLabs = expData.getFreeLabsWithEquipment(part.vessel);
            if (availableLabs.Count > 0)
            {
                if (availableLabs.Count == 1)
                {
                    installExperimentInLab(availableLabs[0]);
                }
                else
                {
                    showGui = 3;
                }
            }
            else
            {
                NE_Helper.logError("Experiment install: No lab found");
            }
        }

        private void installExperimentInLab(Lab lab)
        {
            lab.installExperiment(expData);
            setExperiment(ExperimentData.getNullObject());
        }

        [KSPEvent(guiActive = true, guiName = "Move Experiment", active = false)]
        public void moveExp()
        {
            expData.move(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Finalize Experiment", active = false)]
        public void finalize()
        {
            showGui = 2;
        }

        void OnGUI()
        {
            switch (showGui)
            {
                case 1:
                    showAddWindow();
                    break;
                case 2:
                    showFinalizeWaring();
                    break;
                case 3:
                    showLabWindow();
                    break;

            }
        }

        private void showLabWindow()
        {
            labWindowRect = GUI.ModalWindow(7909034, labWindowRect, showLabGui, "Install Experiment");
        }

        void showLabGui(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Choose Lab");
            labScrollPos = GUILayout.BeginScrollView(labScrollPos, GUILayout.Width(180), GUILayout.Height(320));
            int i = 0;
            foreach (Lab l in availableLabs)
            {
                if (GUILayout.Button(new GUIContent(l.abbreviation, i.ToString())))
                {
                    installExperimentInLab(l);
                    closeGui();
                }
                ++i;
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                closeGui();
            }
            GUILayout.EndVertical();

            String hover = GUI.tooltip;
            try
            {
                int hoverIndex = int.Parse(hover);
                availableLabs[hoverIndex].part.SetHighlightColor(Color.cyan);
                availableLabs[hoverIndex].part.SetHighlightType(Part.HighlightType.AlwaysOn);
                availableLabs[hoverIndex].part.SetHighlight(true, false);
            }
            catch (FormatException)
            {
                resetHighlight();
            }
            GUI.DragWindow();
        }

        private void closeGui()
        {
            resetHighlight();
            showGui = 0;
        }

        private void resetHighlight()
        {
            foreach (Lab l in availableLabs)
            {
                l.part.SetHighlightDefault();
            }
        }

        private void showFinalizeWaring()
        {
            finalizeWindowRect = GUI.ModalWindow(7909032, finalizeWindowRect, finalizeWindow, "Finalize " + expData.getAbbreviation() + " Experiment");
        }

        void finalizeWindow(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("You can no longer move the Experiment after finalization.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                showGui = 0;
            }
            if (GUILayout.Button("OK"))
            {
                DeployExperiment();
                showGui = 0;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void showAddWindow()
        {
            addWindowRect = GUI.ModalWindow(7909031, addWindowRect, showAddGUI, "Add Experiment");
        }
        private void showAddGUI(int id)
        {

            GUILayout.BeginVertical();
            addScrollPos = GUILayout.BeginScrollView(addScrollPos, GUILayout.Width(180), GUILayout.Height(350));
            foreach (ExperimentData e in availableExperiments)
            {
                if (GUILayout.Button(e.getAbbreviation()))
                {
                    setExperiment(e);
                    Events["chooseEquipment"].guiName = "Remove Experiment";
                    showGui = 0;
                }
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                showGui = 0;
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
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
