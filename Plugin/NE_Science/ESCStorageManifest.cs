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
    class ESCStorageManifest : PartModule
    {
        private const int width = 400;
        private const int fixedHeight = 50;
        private const int slotHeight = 100;
        private const int maxHeight = 900;

        private Rect manifestWindowRect = new Rect(Screen.width / 2 - 300, Screen.height / 2 - 250, 250, 550);
        private Vector2 manifestScrollPos = new Vector2();
        private bool showManifest = false;
        List<ExperimentStorage> storageSlots = new List<ExperimentStorage>();
        private int windowID = 0;


        [KSPEvent(guiActiveEditor = true, guiActive = true, guiName = "Storage Manifest", active = true)]
        public void storageManifest()
        {
            storageSlots = new List<ExperimentStorage>(part.GetComponents<ExperimentStorage>());
            windowID = WindowCounter.getNextWindowID();
            int height = getHeight(storageSlots.Count);
            manifestWindowRect = new Rect(Screen.width / 2 - width, Screen.height / 2 - (height/2), width, height);
            showManifest = true;
        }

        private int getHeight(int p)
        {
            int height = fixedHeight + (p * slotHeight);
            return height > maxHeight ? maxHeight : height;
        }

        void OnGUI()
        {
            if (showManifest)
            {
                showManifestWindow();
            }
        }

        private void showManifestWindow()
        {
            manifestWindowRect = GUI.Window(windowID, manifestWindowRect, drawManifestGUI, "Storage Manifest");
        }

        private void drawManifestGUI(int id)
        {
            GUILayout.BeginVertical();
            manifestScrollPos = GUILayout.BeginScrollView(manifestScrollPos, GUILayout.Width(width-20), GUILayout.Height(getHeight(storageSlots.Count)-fixedHeight));
            string text = "";
            foreach (ExperimentStorage e in storageSlots)
            {
                text += "<b>" + e.identifier + "</b>\n";
                if(e.isEmpty()){
                    text += "\t empty\n";
                }else{
                    ExperimentData exp = e.getStoredExperimentData();
                    text += exp.getDescription("\t ") + "\n";
                    text += "\t State: " + exp.getStateString() + "\n";
                }
                text += "\n";
            }
            GUI.skin.label.wordWrap = true;
            GUILayout.Label(text, GUILayout.Height((slotHeight * storageSlots.Count)-10));
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                showManifest = false;
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
