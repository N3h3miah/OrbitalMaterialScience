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
                GUI.BeginGroup(new Rect(Screen.width - 250, Screen.height / 2 - 250, 200, 500));
                GUI.Box(new Rect(0, 0, 200, 500), "Choose Target");
                int top = 40;
                int i = 0;
                foreach (MoveableExperiment e in targets)
                {
                    if (GUI.Button(new Rect(10, top, 180, 30), new GUIContent(e.identifier, i.ToString())))
                    {
                        exp.moveTo(e);
                        closeGui();
                    }
                    top += 35;
                    ++i;
                }
                top += 20;
                if (GUI.Button(new Rect(10, top, 180, 30), "Close"))
                {
                    closeGui();
                }
                GUI.EndGroup();

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
                
            }
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
