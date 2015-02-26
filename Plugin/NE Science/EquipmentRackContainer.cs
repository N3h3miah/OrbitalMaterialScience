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

        private Material contMat = null;

        private LabEquipment leq = LabEquipment.getNullObject();

        private EquipmentContainerTextureFactory texFac = new EquipmentContainerTextureFactory();
        private List<LabEquipment> availableRacks = new List<LabEquipment>();
        private bool showGui = false;
        private Rect addWindowRect = new Rect(Screen.width / 2 - 220, Screen.height / 2 - 220, 250, 500);
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
            GameDatabase.TextureInfo tex = texFac.getTextureForEquipment(type.getType());
            if (tex != null)
            {
                changeTexture(tex);
            }
            else
            {
                NE_Helper.logError("Change Equipment Container Texure: Texture Null");
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
            addScrollPos = GUILayout.BeginScrollView(addScrollPos, GUILayout.Width(210), GUILayout.Height(450));
            foreach (LabEquipment e in availableRacks)
            {
                if (GUILayout.Button(new GUIContent(e.getName(), e.getDescription())))
                {
                    setEquipment(e);
                    showGui = false;
                }
            }
            GUI.skin.label.wordWrap = true;
            GUILayout.Label(GUI.tooltip, GUILayout.Height(100));
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
    }

    class EquipmentContainerTextureFactory
    {
        private Dictionary<EquipmentRacks, GameDatabase.TextureInfo> textureReg = new Dictionary<EquipmentRacks, GameDatabase.TextureInfo>();
        private Dictionary<EquipmentRacks, KeyValuePair<string, string>> textureNameReg = new Dictionary<EquipmentRacks, KeyValuePair<string, string>>() {
        { EquipmentRacks.NONE, new KeyValuePair<string,string>("NehemiahInc/MultiPurposeParts/Parts/LabEquipmentContainer/", "ContainerTexture")},
        { EquipmentRacks.PRINTER, new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/LabEquipmentContainer/","Container3PR_Texture") },
        { EquipmentRacks.CIR,  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/LabEquipmentContainer/", "ContainerCIR_Texture") },
        { EquipmentRacks.FIR,  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/LabEquipmentContainer/", "ContainerFIR_Texture") },
        { EquipmentRacks.MSG,  new KeyValuePair<string,string>("NehemiahInc/OMS/Parts/LabEquipmentContainer/", "ContainerMSG_Texture") },
        { EquipmentRacks.EXPOSURE, new KeyValuePair<string,string>("NehemiahInc/MultiPurposeParts/Parts/LabEquipmentContainer/", "ContainerTexture") }, 
        { EquipmentRacks.USU,  new KeyValuePair<string,string>("NehemiahInc/KR/Parts/LabEquipmentContainer/", "ContainerUSU_Texture" )}};


        internal GameDatabase.TextureInfo getTextureForEquipment(EquipmentRacks type)
        {
            GameDatabase.TextureInfo tex;
            if (textureReg.TryGetValue(type, out tex))
            {
                return tex;
            }
            else
            {
                NE_Helper.log("Loading Texture for experiment: " + type);
                GameDatabase.TextureInfo newTex = getTexture(type);
                if (newTex != null)
                {
                    textureReg.Add(type, newTex);
                    return newTex;
                }
                else
                {
                    NE_Helper.logError("Texture for: " + type + " not found try to return default texture");
                    newTex = getTexture(EquipmentRacks.NONE);
                    return newTex;
                }
            }
        }

        private GameDatabase.TextureInfo getTexture(EquipmentRacks p)
        {
            KeyValuePair<string,string> textureName;
            if (textureNameReg.TryGetValue(p, out textureName))
            {
                GameDatabase.TextureInfo newTex = GameDatabase.Instance.GetTextureInfoIn(textureName.Key, textureName.Value);
                if (newTex != null)
                {
                    return newTex;
                }
            }
            NE_Helper.logError("Could not load texture for Exp: " + p);
            return null;
        }
    }
}
