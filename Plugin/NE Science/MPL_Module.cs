/*
 *   This file is part of Orbital Material Science.
 *   
 *   Part of the code may originate from Station Science ba ether net http://forum.kerbalspaceprogram.com/threads/54774-0-23-5-Station-Science-(fourth-alpha-low-tech-docking-port-experiment-pod-models)
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
    public class MPL_Module : Lab
    {

        private const string MSG_CONFIG_NODE_NAME = "NE_MSG_LabEquipmentSlot";

        [KSPField(isPersistant = false)]
        public float LabTimePerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerLabTime = 0;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Equipment")]
        public string equipment = "";

        [KSPField(isPersistant = false, guiActive = false, guiName = "MSG")]
        public string msgStatus = "";

        private GameObject msg;

        private GameObject cfe;

        public Generator labTimeGenerator;

        private LabEquipmentSlot msgSlot = new LabEquipmentSlot(EquipmentRacks.MSG);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("MPL OnLoad");
            msgSlot = getLabEquipmentSlot(node.GetNode(MSG_CONFIG_NODE_NAME));
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("MPL OnSave");
            node.AddNode(getConfigNodeForSlot(MSG_CONFIG_NODE_NAME, msgSlot));


        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }

            initERacksActive();

            labTimeGenerator = createGenerator(Resources.LAB_TIME, LabTimePerHour, Resources.ELECTRIC_CHARGE, ChargePerLabTime);
            generators.Add(labTimeGenerator);
            msgSlot.onStart(this);

        }

        private Generator createGenerator(string resToCreate, float creationRate, string useRes, float usePerUnit)
        {
            Generator gen = new Generator(this.part);
            gen.addRate(resToCreate, -creationRate);
            if (usePerUnit > 0)
                gen.addRate(useRes, usePerUnit);
            return gen;
        }

        private void initERacksActive()
        {
            if (part.internalModel != null)
            {
                GameObject labIVA = part.internalModel.gameObject.transform.GetChild(0).GetChild(0).gameObject;
                if (labIVA.GetComponent<MeshFilter>().name == "MPL_IVA")
                {
                    msg = labIVA.transform.GetChild(3).gameObject;
                    
                    cfe = msg.transform.GetChild(2).GetChild(0).gameObject;

                    cfe.SetActive(!msgSlot.experimentSlotFree());
                    msg.SetActive(msgSlot.isEquipmentInstalled());

                    NE_Helper.log("init E Racks successfull");
                }
                else
                {
                    NE_Helper.logError("MPL mesh not found");
                }
                
            }
            else {
                NE_Helper.log("init E Racks internal model null");
            }
        }

        public override GameObject getExperimentGO(string id)
        {
            switch (id)
            {
                case "NE_CFE":
                case "NE_CCF":
                    return cfe;
                default:
                    return null;

            }
        }

        public override void installExperiment(ExperimentData exp)
        {
            switch (exp.getEquipmentNeeded())
            {
                case EquipmentRacks.MSG:
                    if (msgSlot.isEquipmentInstalled() && msgSlot.experimentSlotFree())
                    {
                        msgSlot.installExperiment(exp);
                        msgStatus = exp.getAbbreviation();
                        Fields["msgStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + msgSlot.isEquipmentInstalled() + "; free: " + msgSlot.experimentSlotFree());
                    }
                    break;
            }
        }


        public void installEquipmentRack(LabEquipment le)
        {
            switch (le.getType())
            {
                case EquipmentRacks.MSG:
                    msg.SetActive(true);
                    msgSlot.install(le, this);
                    cfe.SetActive(false);
                    break;
            }
            part.mass += le.getMass();
        }

        private void setEquipmentActive(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.MSG:
                    if (msg != null)
                    {
                        msg.SetActive(msgSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if (msg != null) msg.SetActive(msgSlot.isEquipmentInstalled());
                    }
                    break;
            }
        }

        public bool hasEquipmentInstalled(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.MSG:
                    return msgSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public bool hasEquipmentFreeExperimentSlot(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.MSG:
                    return msgSlot.experimentSlotFree();

                default:
                    return false;
            }
        }

        public bool isEquipmentRunning(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.MSG:
                    return msgSlot.isEquipmentRunning();

                default:
                    return false;
            }
        }

        protected override void displayStatusMessage(string s)
        {
            labStatus = s;
            Fields["labStatus"].guiActive = true;
            Fields["equipment"].guiActive = false;
        }

        protected override void updateLabStatus()
        {
            Fields["labStatus"].guiActive = false;
            Fields["equipment"].guiActive = true;
            equipment = getEquipmentString();
            if (msg == null)
            {
                initERacksActive();
            }

            if (!msgSlot.isEquipmentInstalled())
            {
                Events["installMSG"].active = checkForRackModul(EquipmentRacks.MSG);
            }
            else
            {
                Events["installMSG"].active = false;
                Events["moveMSGExp"].active = msgSlot.canExperimentMove(part.vessel);
                if (Events["moveMSGExp"].active)
                {
                    Events["moveMSGExp"].guiName = "Move " + msgSlot.getExperiment().getAbbreviation();
                }
                
                if (msgSlot.canActionRun())
                {
                    string cirActionString = msgSlot.getActionString();
                    Events["actionMSGExp"].guiName = cirActionString;
                }
                Events["actionMSGExp"].active = msgSlot.canActionRun();
                if (!msgSlot.experimentSlotFree())
                {
                    msgStatus = msgSlot.getExperiment().getAbbreviation() + ": " + msgSlot.getExperiment().getStateString();
                    Fields["msgStatus"].guiActive = true;
                }
                else
                {
                    Fields["msgStatus"].guiActive = false;
                }
            }

        }

        private string getEquipmentString()
        {
            string ret = "";
            if (msgSlot.isEquipmentInstalled())
            {
                if (ret.Length > 0) ret += ", ";
                ret += "MSG";
            }
            if (ret.Length == 0)
            {
                ret = "none";
            }
            return ret;
        }

        private bool checkForRackModul(EquipmentRacks equipmentRack)
        {
            List<EquipmentRackContainer> moduls = new List<EquipmentRackContainer>(GameObject.FindObjectsOfType(typeof(EquipmentRackContainer)) as EquipmentRackContainer[]);
            foreach (EquipmentRackContainer modul in moduls)
            {
                if (modul.vessel == this.vessel && modul.getRackType() == equipmentRack)
                {
                    return true;
                }
            }

            return false;
        }

        private EquipmentRackContainer getRackModul(EquipmentRacks equipmentRack)
        {
            List<EquipmentRackContainer> moduls = new List<EquipmentRackContainer>(GameObject.FindObjectsOfType(typeof(EquipmentRackContainer)) as EquipmentRackContainer[]);

            foreach (EquipmentRackContainer modul in moduls)
            {
                if (modul.vessel == this.vessel && modul.getRackType() == equipmentRack)
                {
                    return modul;
                }
            }

            return null;
        }

        [KSPEvent(guiActive = true, guiName = "Install MSG", active = false)]
        public void installMSG()
        {
            EquipmentRackContainer modul = getRackModul(EquipmentRacks.MSG);
            if (modul != null)
            {
                installEquipmentRack(modul.install());
            }
            else
            {
                displayStatusMessage("Equipment Rack Modul not found!");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Move MSG Experiment", active = false)]
        public void moveMSGExp()
        {
            msgSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Action MSG Experiment", active = false)]
        public void actionMSGExp()
        {
            msgSlot.experimentAction();
        }

        public override string GetInfo()
        {
            String ret = base.GetInfo();
            ret += (ret == "" ? "" : "\n") + "Lab Time per hour: " + LabTimePerHour;
            ret += "\nYou can install equipment racks in this lab to run experiments.";
            return ret;
        }

    }
}
