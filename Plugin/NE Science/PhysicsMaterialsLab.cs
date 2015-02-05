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
    public class PhysicsMaterialsLab : Lab
    {

        private const string CIR_CONFIG_NODE_NAME = "NE_CIR_LabEquipmentSlot";
        private const string FFR_CONFIG_NODE_NAME = "NE_FFR_LabEquipmentSlot";
        private const string DPR_CONFIG_NODE_NAME = "NE_DPR_LabEquipmentSlot";

        [KSPField(isPersistant = false)]
        public float LabTimePerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerLabTime = 0;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Equipment")]
        public string equipment = "";

        [KSPField(isPersistant = false, guiActive = false, guiName = "CIR")]
        public string cirStatus = "";
        [KSPField(isPersistant = false, guiActive = false, guiName = "FFR")]
        public string ffrStatus = "";
        [KSPField(isPersistant = false, guiActive = false, guiName = "3PR")]
        public string prStatus = "";

        private GameObject cir;
        private GameObject ffr;
        private GameObject printer;

        public Generator labTimeGenerator;

        private LabEquipmentSlot cirSlot = new LabEquipmentSlot(EquipmentRacks.CIR);
        private LabEquipmentSlot ffrSlot = new LabEquipmentSlot(EquipmentRacks.FFR);
        private LabEquipmentSlot printerSlot = new LabEquipmentSlot(EquipmentRacks.PRINTER);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("MSL OnLoad");
            cirSlot = getLabEquipmentSlot(node.GetNode(CIR_CONFIG_NODE_NAME));
            ffrSlot = getLabEquipmentSlot(node.GetNode(FFR_CONFIG_NODE_NAME));
            printerSlot = getLabEquipmentSlot(node.GetNode(DPR_CONFIG_NODE_NAME));
        }

        private LabEquipmentSlot getLabEquipmentSlot(ConfigNode configNode)
        {
            if (configNode != null)
            {
                return LabEquipmentSlot.getLabEquipmentSlotFromConfigNode(configNode.GetNode(LabEquipmentSlot.CONFIG_NODE_NAME), this);
            }
            else
            {
                NE_Helper.logError("MSL onLoad: LabEquipmentSlotNode null");
                return new LabEquipmentSlot(EquipmentRacks.NONE);
            }
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("MSL OnSave");
            node.AddNode(getConfigNodeForSlot(CIR_CONFIG_NODE_NAME, cirSlot));
            node.AddNode(getConfigNodeForSlot(FFR_CONFIG_NODE_NAME, ffrSlot));
            node.AddNode(getConfigNodeForSlot(DPR_CONFIG_NODE_NAME, printerSlot));

        }

        private ConfigNode getConfigNodeForSlot(string nodeName, LabEquipmentSlot slot)
        {
            ConfigNode node = new ConfigNode(nodeName);
            node.AddNode(slot.getConfigNode());
            return node;
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
            cirSlot.onStart(this);
            ffrSlot.onStart(this);
            printerSlot.onStart(this);

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
                if (labIVA.GetComponent<MeshFilter>().name == "Lab1IVA")
                {
                    printer = labIVA.transform.GetChild(0).gameObject;
                    cir = labIVA.transform.GetChild(1).gameObject;
                    ffr = labIVA.transform.GetChild(2).gameObject;

                    if (ffrSlot.isEquipmentInstalled())
                    {
                        ffr.SetActive(true);
                    }
                    else
                    {
                        ffr.SetActive(false);
                    }

                    if (cirSlot.isEquipmentInstalled())
                    {
                        cir.SetActive(true);
                    }
                    else
                    {
                        cir.SetActive(false);
                    }

                    if (printerSlot.isEquipmentInstalled())
                    {
                        printer.SetActive(true);
                    }
                    else
                    {
                        printer.SetActive(false);
                    }
                }
                NE_Helper.log("init E Racks successfull");
            }
            else {
                NE_Helper.log("init E Racks internal model null");
            }
        }

        public override void installExperiment(ExperimentData exp)
        {
            switch (exp.getEquipmentNeeded())
            {
                case EquipmentRacks.CIR:
                    if (cirSlot.isEquipmentInstalled() && cirSlot.experimentSlotFree())
                    {
                        cirSlot.installExperiment(exp);
                        cirStatus = exp.getAbbreviation();
                        Fields["cirStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + cirSlot.isEquipmentInstalled() + "; free: " + cirSlot.experimentSlotFree());
                    }
                    break;
                case EquipmentRacks.FFR:
                    if (ffrSlot.isEquipmentInstalled() && ffrSlot.experimentSlotFree())
                    {
                        ffrSlot.installExperiment(exp);
                        ffrStatus = exp.getAbbreviation();
                        Fields["ffrStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + ffrSlot.isEquipmentInstalled() + "; free: " + ffrSlot.experimentSlotFree());
                    }
                    break;
                case EquipmentRacks.PRINTER:
                    if (printerSlot.isEquipmentInstalled() && printerSlot.experimentSlotFree())
                    {
                        printerSlot.installExperiment(exp);
                        prStatus = exp.getAbbreviation();
                        Fields["prStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + printerSlot.isEquipmentInstalled() + "; free: " + printerSlot.experimentSlotFree());
                    }
                    break;
            }
        }


        public void installEquipmentRack(LabEquipment le)
        {
            switch (le.getType())
            {
                case EquipmentRacks.FFR:
                    ffr.SetActive(true);
                    ffrSlot.install(le, this);
                    break;
                case EquipmentRacks.CIR:
                    cir.SetActive(true);
                    cirSlot.install(le, this);
                    break;
                case EquipmentRacks.PRINTER:
                    printer.SetActive(true);
                    printerSlot.install(le, this);
                    break;
            }
            part.mass += le.getMass();
        }

        private void setEquipmentActive(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.FFR:
                    if (ffr != null)
                    {
                        ffr.SetActive(ffrSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if(ffr != null)ffr.SetActive(ffrSlot.isEquipmentInstalled());
                    }
                    break;
                case EquipmentRacks.CIR:
                    if (cir != null)
                    {
                        cir.SetActive(cirSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if (cir != null) cir.SetActive(cirSlot.isEquipmentInstalled());
                    }
                    break;
                case EquipmentRacks.PRINTER:
                    if (printer != null)
                    {
                        printer.SetActive(printerSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if (printer != null) printer.SetActive(printerSlot.isEquipmentInstalled());
                    }
                    break;
            }
        }

        public bool hasEquipmentInstalled(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.CIR:
                    return cirSlot.isEquipmentInstalled();

                case EquipmentRacks.FFR:
                    return ffrSlot.isEquipmentInstalled();

                case EquipmentRacks.PRINTER:
                    return printerSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public bool hasEquipmentFreeExperimentSlot(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.CIR:
                    return cirSlot.experimentSlotFree();

                case EquipmentRacks.FFR:
                    return ffrSlot.experimentSlotFree();

                case EquipmentRacks.PRINTER:
                    return printerSlot.experimentSlotFree();

                default:
                    return false;
            }
        }

        public bool isEquipmentRunning(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.CIR:
                    return cirSlot.isEquipmentRunning();

                case EquipmentRacks.FFR:
                    return ffrSlot.isEquipmentRunning();

                case EquipmentRacks.PRINTER:
                    return printerSlot.isEquipmentRunning();

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
            if (cir == null || ffr == null || printer == null)
            {
                initERacksActive();
            }

            if (!cirSlot.isEquipmentInstalled())
            {
                Events["installCIR"].active = checkForRackModul(EquipmentRacks.CIR);
            }
            else
            {
                Events["installCIR"].active = false;
                Events["moveCIRExp"].active = cirSlot.canExperimentMove(part.vessel);
                if (Events["moveCIRExp"].active)
                {
                    Events["moveCIRExp"].guiName = "Move " + cirSlot.getExperiment().getAbbreviation();
                }
                string cirActionString = cirSlot.getActionString();
                if (cirActionString.Length > 0)
                {
                    Events["actionCIRExp"].guiName = cirActionString;
                }
                Events["actionCIRExp"].active = cirActionString.Length > 0;
                if (!cirSlot.experimentSlotFree())
                {
                    cirStatus = cirSlot.getExperiment().getAbbreviation() + ": " + cirSlot.getExperiment().getStateString();
                    Fields["cirStatus"].guiActive = true;
                }
                else
                {
                    Fields["cirStatus"].guiActive = false;
                }
            }
            if (!ffrSlot.isEquipmentInstalled())
            {
                Events["installFFR"].active = checkForRackModul(EquipmentRacks.FFR);
            }
            else
            {
                Events["installFFR"].active = false;
                Events["moveFFRExp"].active = ffrSlot.canExperimentMove(part.vessel);
                if (Events["moveFFRExp"].active)
                {
                    Events["moveFFRExp"].guiName = "Move " + ffrSlot.getExperiment().getAbbreviation();
                }
                string ffrActionString = ffrSlot.getActionString();
                if (ffrActionString.Length > 0)
                {
                    Events["actionFFRExp"].guiName = ffrActionString;
                }
                Events["actionFFRExp"].active = ffrActionString.Length > 0;
                if (!ffrSlot.experimentSlotFree())
                {
                    ffrStatus = ffrSlot.getExperiment().getAbbreviation() + ": " + ffrSlot.getExperiment().getStateString();
                    Fields["ffrStatus"].guiActive = true;
                }
                else
                {
                    Fields["ffrStatus"].guiActive = false;
                }
            }
            if (!printerSlot.isEquipmentInstalled())
            {
                Events["installPrinter"].active = checkForRackModul(EquipmentRacks.PRINTER);
            }
            else
            {
                Events["installPrinter"].active = false;
                Events["movePRExp"].active = printerSlot.canExperimentMove(part.vessel);
                if (Events["movePRExp"].active)
                {
                    Events["movePRExp"].guiName = "Move " + printerSlot.getExperiment().getAbbreviation();
                }
                string prActionString = printerSlot.getActionString();
                if (prActionString.Length > 0)
                {
                    Events["actionPRExp"].guiName = prActionString;
                }
                Events["actionPRExp"].active = prActionString.Length > 0;
                if (!printerSlot.experimentSlotFree())
                {
                    prStatus = printerSlot.getExperiment().getAbbreviation() + ": " + printerSlot.getExperiment().getStateString();
                    Fields["prStatus"].guiActive = true;
                }
                else
                {
                    Fields["prStatus"].guiActive = false;
                }
            }

        }

        //private void updateAnimaitonState()
        //{
        //    if (ffrSlot.isEquipmentInstalled())
        //    {
        //        double last = ffrGenerator.rates[Resources.FFR_TEST_RUN].last_produced;
        //        bool state = (last < -0.0000001);
        //        if (ffrRunning != state)
        //        {
        //            ffrRunning = state;
        //        }
        //    }

        //    if (printerSlot.isEquipmentInstalled())
        //    {
        //        double last = printerGenerator.rates[Resources.PRINT_LAYER].last_produced;
        //        bool state = (last < -0.0000001);
        //        if (printerRunning != state)
        //        {
        //            printerRunning = state;
        //        }
        //    }

        //    if (cirSlot.isEquipmentInstalled())
        //    {
        //        double last = cirGenerator.rates[Resources.CIR_BURN_TIME].last_produced;
        //        bool state = (last < -0.0000001);
        //        if (cirRunning != state)
        //        {
        //            cirRunning = state;
        //        }
        //    }
        //}

        private string getEquipmentString()
        {
            string ret = "";
            if (ffrSlot.isEquipmentInstalled())
            {
                ret += "FFR";
            }
            if (cirSlot.isEquipmentInstalled())
            {
                if (ret.Length > 0) ret += ", ";
                ret += "CIR";
            }
            if (printerSlot.isEquipmentInstalled())
            {
                if (ret.Length > 0) ret += ", ";
                ret += "3PR";
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

        [KSPEvent(guiActive = true, guiName = "Install CIR", active = false)]
        public void installCIR()
        {
            EquipmentRackContainer modul = getRackModul(EquipmentRacks.CIR);
            if (modul != null)
            {
                installEquipmentRack(modul.install());
            }
            else
            {
                displayStatusMessage("Equipment Rack Modul not found!");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Install FFR", active = false)]
        public void installFFR()
        {
            EquipmentRackContainer modul = getRackModul(EquipmentRacks.FFR);
            if (modul != null)
            {
                installEquipmentRack(modul.install());
            }
            else
            {
                displayStatusMessage("Equipment Rack Modul not found!");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Install 3D-Printer", active = false)]
        public void installPrinter()
        {
            EquipmentRackContainer modul = getRackModul(EquipmentRacks.PRINTER);
            if (modul != null)
            {
                installEquipmentRack(modul.install());
            }
            else
            {
                displayStatusMessage("Equipment Rack Modul not found!");
            }
        }

        [KSPEvent(guiActive = true, guiName = "Move FFR Experiment", active = false)]
        public void moveFFRExp()
        {
            ffrSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Action FFR Experiment", active = false)]
        public void actionFFRExp()
        {
            ffrSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "Move CIR Experiment", active = false)]
        public void moveCIRExp()
        {
            cirSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Action CIR Experiment", active = false)]
        public void actionCIRExp()
        {
            cirSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "Move Printer Experiment", active = false)]
        public void movePRExp()
        {
            printerSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "Action Printer Experiment", active = false)]
        public void actionPRExp()
        {
            printerSlot.experimentAction();
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
