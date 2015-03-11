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
    class ChooseTestSubject : MonoBehaviour
    {
        private List<string> avilableSubjects = new List<string>();
        private KerbalResearchExperimentData exp;

        private bool showGui = false;

        private Rect moveWindowRect = new Rect(Screen.width - 300, Screen.height / 2 - 200, 250, 400);
        private Vector2 moveScrollPos = new Vector2();
        private int windowID;
        private ExperimentStep.startCallback cbMethod;

        internal void showDialog(List<string> avilableSubjects, KerbalResearchExperimentData experimentData, ExperimentStep.startCallback cbMethod)
        {
            this.avilableSubjects = avilableSubjects;
            exp = experimentData;
            this.cbMethod = cbMethod;
            windowID = WindowCounter.getNextWindowID();
            showGui = true;
        }

        void OnGUI()
        {
            if (showGui)
            {
                showMoveWindow();
                
            }
        }

        void showMoveWindow()
        {
            moveWindowRect = GUI.Window(windowID, moveWindowRect, showMoveGui, "Choose Test Subject");
        }

        void showMoveGui(int id)
        {

            GUILayout.BeginVertical();
            moveScrollPos = GUILayout.BeginScrollView(moveScrollPos, GUILayout.Width(230), GUILayout.Height(350));
            GUILayout.Label("Test subjects needed: " + exp.getTestSubjectsNeeded());
            if (exp.isTestSubjectAvailable())
            {
                GUILayout.Label("Choose a Kerbal:");
                foreach (string s in avilableSubjects)
                {
                    if (GUILayout.Button(s))
                    {
                        exp.getActiveStep().start(s, cbMethod);
                        closeGui();
                    }
                }
            }
            if (exp.getActiveStepIndex() > 0)
            {
                GUILayout.Label("Already tested:");


                foreach (KerbalResearchStep krs in exp.getExperimentSteps())
                {
                    if (krs.getSubjectName() != "")
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label(krs.getSubjectName());
                        GUILayout.EndHorizontal();
                    }
                }

            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                closeGui();
                cbMethod(false);
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void closeGui()
        {
            showGui = false;
        }
    }
}
