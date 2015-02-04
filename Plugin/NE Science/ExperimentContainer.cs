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
    class ExperimentContainer : PartModule
    {
        private const float EMPTY_MASS = 0.1f;

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

        private PhaseExperimentCore exp;

        private List<PhaseExperimentCore> availableExperiments = new List<PhaseExperimentCore>();
        private bool showGui = false;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            //ConfigNode eqNode = node.GetNode(LabEquipment.CONFIG_NODE_NAME);
            //if (eqNode != null)
            //{
            //    setEquipment(LabEquipment.getLabEquipmentFromNode(eqNode));
            //}
            //else
            //{
            //    setEquipment(LabEquipment.getNullObject());
            //}
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            if (exp != null)
            {
                exp.OnSave(node);
            }
        }

        private void setExperiment(PhaseExperimentCore e)
        {
            if (e == null && exp != null)
            {
                part.RemoveModule(part.GetComponent<PhaseExperimentCore>());
            }
            this.exp = e;
            status = e.getName();
            if (exp != null)
            {
                Events["chooseEquipment"].guiName = "Add Lab Equipment";
                part.mass = EMPTY_MASS;
            }
            else
            {
                Events["chooseEquipment"].guiName = "Remove Equipment";
                part.mass += exp.getMass();
                ConfigNode moduleNode = new ConfigNode();
                exp.OnSave(moduleNode);
                part.AddModule(moduleNode);
            }
            
            setTexture(exp);
        }

        private void setTexture(PhaseExperimentCore type)
        {
            //GameDatabase.TextureInfo tex = getTextureForRack(type);
            //if (tex != null)
            //{
            //    changeTexture(tex);
            //}
            //else
            //{
            //    NE_Helper.logError("Change Container Texure: Texture Null");
            //}
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state.Equals(StartState.Editor))
            {
                Events["chooseExperiment"].active = true;
            }
            else
            {
                Events["chooseExperiment"].active = false;
            }
        }

        [KSPEvent(guiActiveEditor = true, guiName = "Add Experiment", active = false)]
        public void chooseExperiment()
        {
            if (exp == null)
            {
                availableExperiments = ExperimentRegistry.getAvailableExperiments(); ;
                showGui = true;
            }
            else
            {
                setExperiment(null);
                Events["chooseExperiment"].guiName = "Add Lab Equipment";
            }
        }

        void OnGUI()
        {
            if (showGui)
            {
                GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 250, 200, 500));
                GUI.Box(new Rect(0, 0, 200, 500), "Experiments");
                int top = 40;
                foreach (PhaseExperimentCore e in availableExperiments)
                {
                    if (GUI.Button(new Rect(10, top, 180, 30), e.getName()))
                    {
                        setExperiment(e);
                        showGui = false;
                    }
                    top += 35;
                }
                top += 20;
                if (GUI.Button(new Rect(10, top, 180, 30), "Close"))
                {
                    showGui = false;
                }
                GUI.EndGroup();
            }
        }

        public override string GetInfo()
        {
            return "Choose from the available experiments." ;
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
                NE_Helper.logError("Transform NOT found: " + "Exp Container");
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

    public class ExperimentRegistry
    {
        static readonly List<String> exps = new List<string> {"NE.CCFE" };

        public static List<PhaseExperimentCore> getAvailableExperiments()
        {
            List<PhaseExperimentCore> list = new List<PhaseExperimentCore>();
            foreach (string s in exps)
            {
                AvailablePart part = PartLoader.getPartInfoByName(s);
                if (part != null)
                {
                    Part pPf = part.partPrefab;
                    PhaseExperimentCore lem = pPf.GetComponent<PhaseExperimentCore>();
                    if (ResearchAndDevelopment.PartTechAvailable(part))
                    {
                        list.Add(lem);
                    }
                }
            }

            return list;
        }
    }
}
