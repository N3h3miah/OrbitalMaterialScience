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
using UnityEngine;
using UnityEngine.UI;

namespace NE_Science
{
    class ChooseMoveTarget : MonoBehaviour
    {
        private List<ExperimentStorage> targets = null; //new List<ExperimentStorage>();
        private ExperimentData exp = null;

        private string lastTooltip = "";
        private PopupDialog pd = null;

        internal void showDialog(List<ExperimentStorage> targets, ExperimentData experimentData)
        {
            NE_Helper.log("start");
            this.targets = targets;
            exp = experimentData;
            NE_Helper.log("init done");

            showMoveWindow();
        }

        // Even though we're using the new GUI, we still need the OnGUI event to parse tooltips
        public void OnGUI()
        {
            if (Event.current.type == EventType.Repaint) {
                updateHighlighting();
            }
        }

        private void updateHighlighting()
        {
            if (GUI.tooltip == lastTooltip)
            {
                return;
            }

            if (lastTooltip != "")
            {
                // Event: leave control
            }
            if (GUI.tooltip != "")
            {
                // Event: enter control
                try
                {
                    int hoverIndex = int.Parse(GUI.tooltip);
                    targets[hoverIndex].part.SetHighlightColor(Color.cyan);
                    targets[hoverIndex].part.SetHighlightType(Part.HighlightType.AlwaysOn);
                    targets[hoverIndex].part.SetHighlight(true, false);
                }
                catch (FormatException)
                {
                    resetHighlight();
                }
            }
            lastTooltip = GUI.tooltip;
        }

        private void showMoveWindow()
        {
            // This is a list of content items to add to the dialog
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();

            dialog.Add(new DialogGUILabel("Choose destination experiment storage slot for " + exp.getName(), true, true));
            dialog.Add(new DialogGUISpace(4));

            // Build a button list of all available experiments with their descriptions
            int numTargets = targets.Count;
            DialogGUIBase[] scrollList = new DialogGUIBase[numTargets + 1];
            scrollList[0] = new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true);
            for (int i = 0; i < numTargets; i++)
            {
                var e = targets[i];
                var b = new DialogGUIButton<ExperimentStorage>(e.identifier, onSelectTarget, e, true);
                b.tooltipText = i.ToString(); // TODO: Work out hover
                b.size = new Vector2(120, 30);
                //var l = new DialogGUILabel(e.getDescription(), true, true);
                var h = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { b });

                scrollList[i + 1] = h;
            }

            dialog.Add(new DialogGUIScrollList(new Vector2(200,300), false, true, //Vector2.one, false, true,
                new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.UpperLeft, scrollList)
            ));
            dialog.Add(new DialogGUISpace(4));
            dialog.Add(new DialogGUILabel(getLastTooltip, true, true));
            dialog.Add(new DialogGUISpace(4));
            // Add a centered "Cancel" button
            dialog.Add(new DialogGUIHorizontalLayout(new DialogGUIBase[]
            {
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("Close", onCloseMoveWindow, true),
                new DialogGUIFlexibleSpace(),
            }));

            // Actually create and show the dialog
            pd = PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("", "Choose Target", HighLogic.UISkin, dialog.ToArray()),
                false, HighLogic.UISkin);
        }

        private string getLastTooltip()
        {
            return "Tooltip: " + lastTooltip;
        }

        void onSelectTarget(ExperimentStorage es)
        {
            exp.moveTo(es);
            onCloseMoveWindow();
        }

        private void onCloseMoveWindow()
        {
            resetHighlight();
            pd = null;
            targets = null;
            exp = null;
        }

        private void resetHighlight()
        {
            for (int i = 0, count = targets.Count; i < count; i++)
            {
                targets[i].part.SetHighlightDefault();
            }
        }
    }
}
