using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NE_Science 
{
    class ChooseMoveTarget : MonoBehaviour
    {
        private List<MoveableExperiment> targets = new List<MoveableExperiment>();
        private ExperimentData exp;

        private bool showGui = false;

        private Rect moveWindowRect = new Rect(Screen.width - 250, Screen.height / 2 - 250, 200, 400);
        private Vector2 moveScrollPos = new Vector2();

        internal void showDialog(List<MoveableExperiment> targets, ExperimentData experimentData)
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
            moveWindowRect = GUI.ModalWindow(7909031, moveWindowRect, showMoveGui, "Move Experiment");
        }

        void showMoveGui(int id)
        {

            GUILayout.BeginVertical();
            GUILayout.Label("Choose Target");
            moveScrollPos = GUILayout.BeginScrollView(moveScrollPos, GUILayout.Width(180), GUILayout.Height(320));
            int i = 0;
            foreach (MoveableExperiment e in targets)
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
                targets[hoverIndex].part.SetHighlightColor(Color.magenta);
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
            foreach (MoveableExperiment e in targets)
            {
                e.part.SetHighlightDefault();
            }
        }
    }
}
