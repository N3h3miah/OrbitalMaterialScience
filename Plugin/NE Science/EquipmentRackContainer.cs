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
        [KSPField(isPersistant = true)]
        public string RackType = "";

        [KSPField(isPersistant = true)]
        public bool empty = true;

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor= true ,guiName = "Contains")]
        public string status = "";

        [KSPField(isPersistant = false)]
        public string folder = "NehemiahInc/Parts/LabEquipment/";

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

        private EquipmentRacks type;

        private List<EquipmentRacks> availableRacks = new List<EquipmentRacks>();
        private bool showGui = false;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            if (empty)
            {
                part.mass = 0.2f;
                type = EquipmentRacks.NONE;
                status = "empty";
            }
            else
            {
                switch (RackType)
                {
                    case "CIR":
                        type = EquipmentRacks.CIR;
                        status = "Combustion Integrated Rack (CIR)";
                        break;

                    case "FFR":
                        type = EquipmentRacks.FFR;
                        status = "Fluid Flow Rack (FFR)";
                        break;

                    case "Printer":
                        type = EquipmentRacks.PRINTER;
                        status = "3D Printer Rack (3PR)";
                        break;

                    default:
                        type = EquipmentRacks.NONE;
                        status = "empty";
                        break;
                }
                setTexture(type);
            }
        }

        private void setEquipment(EquipmentRacks er)
        {
            type = er;
            empty = false;
            Events["chooseEquipment"].guiName = "Remove Equipment";
            switch (type)
            {
                case EquipmentRacks.PRINTER:
                    RackType = "Printer";
                    status = "3D Printer Rack (3PR)";
                    break;

                case EquipmentRacks.FFR:
                    RackType = "FFR";
                    status = "Fluid Flow Rack (FFR)";
                    break;

                case EquipmentRacks.CIR:
                    RackType = "CIR";
                    status = "Combustion Integrated Rack (CIR)";
                    break;

                default:
                    type = EquipmentRacks.NONE;
                    status = "empty";
                    RackType = "";
                    empty = true;
                    Events["chooseEquipment"].guiName = "Add Lab Equipment";
                    break;
            }
            setTexture(type);
        }

        private void setTexture(EquipmentRacks type)
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
            if (empty)
            {
                part.mass = 0.2f;
                type = EquipmentRacks.NONE;
                status = "empty";
            }
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
            NE_Helper.log("" + empty);
            if (empty)
            {
                availableRacks = EquipmentRackRegistry.getAvailableRacks();
                NE_Helper.log(availableRacks.ToString());
                showGui = true;
            }
            else
            {
                setEquipment(EquipmentRacks.NONE);
                Events["chooseEquipment"].guiName = "Add Lab Equipment";
            }
        }

        void OnGUI()
        {
            if (showGui)
            {
                GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 250, 200, 500));
                GUI.Box(new Rect(0, 0, 200, 500), "Add Lab Equipment");
                int top = 40;
                foreach (EquipmentRacks e in availableRacks)
                {
                    if (GUI.Button(new Rect(10, top, 180, 40), e.ToString()))
                    {
                        setEquipment(e);
                        showGui = false;
                    }
                    top += 45;
                }
                if (GUI.Button(new Rect(10, top, 180, 40), "Close"))
                {
                    showGui = false;
                }
                GUI.EndGroup();
            }
        }

        public EquipmentRacks getRackType()
        {
            return type;
        }

        public void install()
        {
            empty = true;
            type = EquipmentRacks.NONE;
            status = "empty";
            part.mass = 0.2f;
            setTexture(type);
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
                NE_Helper.logError("Transform NOT found: " + "MEP IVA Screen");
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

        private GameDatabase.TextureInfo getTextureForRack(EquipmentRacks type)
        {
            switch (type)
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
