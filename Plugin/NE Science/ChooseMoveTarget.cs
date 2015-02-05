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
    class ChooseMoveTarget : MonoBehaviour
    {
        private List<ExperimentStorage> targets = new List<ExperimentStorage>();
        private ExperimentData exp;

        private bool showGui = false;

        private Rect moveWindowRect = new Rect(Screen.width - 250, Screen.height / 2 - 250, 200, 400);
        private Vector2 moveScrollPos = new Vector2();

        internal void showDialog(List<ExperimentStorage> targets, ExperimentData experimentData)
        {
            NE_Helper.log("start");
            this.targets = targets;
            exp = experimentData;
            NE_Helper.log("init done");
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
            moveWindowRect = GUI.ModalWindow(7909033, moveWindowRect, showMoveGui, "Move Experiment");
        }

        void showMoveGui(int id)
        {

            GUILayout.BeginVertical();
            GUILayout.Label("Choose Target");
            moveScrollPos = GUILayout.BeginScrollView(moveScrollPos, GUILayout.Width(180), GUILayout.Height(320));
            int i = 0;
            foreach (ExperimentStorage e in targets)
            {
                if (GUILayout.Button( new GUIContent(e.identifier, i.ToString())))
                {
                    exp.moveTo(e);
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
                targets[hoverIndex].part.SetHighlightColor(Color.cyan);
                targets[hoverIndex].part.SetHighlightType(Part.HighlightType.AlwaysOn);
                targets[hoverIndex].part.SetHighlight(true, false);
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
            showGui = false;
        }

        private void resetHighlight()
        {
            foreach (ExperimentStorage e in targets)
            {
                e.part.SetHighlightDefault();
            }
        }
    }
}
