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
    class EquipmentRackContainer : PartModule
    {
        private const float EMPTY_MASS = 0.4f;

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor= true ,guiName = "Contains")]
        public string status = "";

        [KSPField(isPersistant = false)]
        public string folder = "NehemiahInc/Parts/LabEquipmentContainer/";

        [KSPField(isPersistant = false)]
        public string noEquTexture = "ContainerTexture";

        [KSPField(isPersistant = false)]
        public string printTexture = "Container3PR_Texture";

        [KSPField(isPersistant = false)]
        public string CIRTexture = "ContainerCIR_Texture";

        [KSPField(isPersistant = false)]
        public string FFRTexture = "ContainerFFR_Texture";

        private GameDatabase.TextureInfo noEqu;
        private GameDatabase.TextureInfo printer;
        private GameDatabase.TextureInfo cir;
        private GameDatabase.TextureInfo ffr;

        private Material contMat = null;

        private LabEquipment leq = LabEquipment.getNullObject();

        private List<LabEquipment> availableRacks = new List<LabEquipment>();
        private bool showGui = false;
        private Rect addWindowRect = new Rect(Screen.width / 2 - 220, Screen.height / 2 - 220, 250, 400);
        private Vector2 addScrollPos = new Vector2();

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            ConfigNode eqNode = node.GetNode(LabEquipment.CONFIG_NODE_NAME);
            if (eqNode != null)
            {
                setEquipment(LabEquipment.getLabEquipmentFromNode(eqNode, null));
            }
            else
            {
                setEquipment(LabEquipment.getNullObject());
            }
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            node.AddNode(leq.getNode());
        }

        private void setEquipment(LabEquipment er)
        {
            leq = er;
            status = leq.getName();
            if (leq.getType() == EquipmentRacks.NONE)
            {
                Events["chooseEquipment"].guiName = "Add Lab Equipment";
                part.mass = EMPTY_MASS;
            }
            else
            {
                Events["chooseEquipment"].guiName = "Remove Equipment";
                part.mass += er.getMass();
            }
            
            setTexture(leq);
        }

        private void setTexture(LabEquipment type)
        {
            GameDatabase.TextureInfo tex = getTextureForRack(type);
            if (tex != null)
            {
                changeTexture(tex);
            }
            else
            {
                NE_Helper.logError("Change Container Texure: Texture Null");
            }
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
        }

        [KSPEvent(guiActiveEditor = true, guiName = "Add Lab Equipment", active = false)]
        public void chooseEquipment()
        {
            if (leq.getType() == EquipmentRacks.NONE)
            {
                availableRacks = EquipmentRackRegistry.getAvailableRacks();
                showGui = true;
            }
            else
            {
                setEquipment(LabEquipment.getNullObject());
                Events["chooseEquipment"].guiName = "Add Lab Equipment";
            }
        }

        void OnGUI()
        {
            if (showGui)
            {
                showAddGui();
            }
        }

        private void showAddGui()
        {
            addWindowRect = GUI.ModalWindow(7909031, addWindowRect, showAddGui, "Add Lab Equipment");
        }

        void showAddGui(int id)
        {
            GUILayout.BeginVertical();
            addScrollPos = GUILayout.BeginScrollView(addScrollPos, GUILayout.Width(200), GUILayout.Height(350));
            foreach (LabEquipment e in availableRacks)
            {
                if (GUILayout.Button(e.getName()))
                {
                    setEquipment(e);
                    showGui = false;
                }
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                showGui = false;
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public EquipmentRacks getRackType()
        {
            return leq.getType();
        }

        public LabEquipment install()
        {
            LabEquipment ret = leq;
            setEquipment(LabEquipment.getNullObject());
            return ret;
        }

        public override string GetInfo()
        {
            return "Choose from the available lab equipment." ;
        }

        private void changeTexture(GameDatabase.TextureInfo newTexture)
        {
            Material mat = getContainerMaterial();
            if (mat != null)
            {
                mat.mainTexture = newTexture.texture;
            }
            else
            {
                NE_Helper.logError("Transform NOT found: " + "Equipment Container");
            }
        }

        private Material getContainerMaterial()
        {
            if (contMat == null)
            {
                Transform t = part.FindModelTransform("Container");
                if (t != null)
                {
                    contMat = t.renderer.material;
                    return contMat;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return contMat;
            }
        }

        private GameDatabase.TextureInfo getTextureForRack(LabEquipment type)
        {
            switch (type.getType())
            {
                case EquipmentRacks.PRINTER:
                    if (printer == null) printer = getTexture(folder, printTexture);
                    return printer;
                case EquipmentRacks.CIR:
                    if (cir == null) cir = getTexture(folder, CIRTexture);
                    return cir;
                case EquipmentRacks.FFR:
                    if (ffr == null) ffr = getTexture(folder, FFRTexture);
                    return ffr;
                default:
                    if (noEqu == null) noEqu = getTexture(folder, noEquTexture);
                    return noEqu;
            }
        }

        private GameDatabase.TextureInfo getTexture(string folder, string textureName)
        {
            return GameDatabase.Instance.GetTextureInfoIn(folder, textureName);
        }
    }
}
