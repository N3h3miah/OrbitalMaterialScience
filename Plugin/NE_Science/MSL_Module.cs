﻿/*
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
using UnityEngine;
using KSP.Localization;

namespace NE_Science
{
    public class MSL_Module : Lab
    {

        private const string CIR_LAB_EQUIPMENT_TYPE = "CIR";
        private const string FIR_LAB_EQUIPMENT_TYPE = "FIR";
        private const string PRINTER_LAB_EQUIPMENT_TYPE = "PRINTER";

        [KSPField(isPersistant = false)]
        public float LabTimePerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerLabTime = 0;

        [KSPField(isPersistant = false, guiActive = false, guiName = "CIR")]
        public string cirStatus = "";
        [KSPField(isPersistant = false, guiActive = false, guiName = "FIR")]
        public string ffrStatus = "";
        [KSPField(isPersistant = false, guiActive = false, guiName = "3PR")]
        public string prStatus = "";

        private GameObject cir;
        private GameObject fir;
        private GameObject printer;

        public Generator labTimeGenerator;

        private LabEquipmentSlot cirSlot = new LabEquipmentSlot(EquipmentRacks.CIR);
        private LabEquipmentSlot firSlot = new LabEquipmentSlot(EquipmentRacks.FIR);
        private LabEquipmentSlot printerSlot = new LabEquipmentSlot(EquipmentRacks.PRINTER);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("MSL OnLoad");
            cirSlot = getLabEquipmentSlotByType(node, CIR_LAB_EQUIPMENT_TYPE);
            firSlot = getLabEquipmentSlotByType(node, FIR_LAB_EQUIPMENT_TYPE);
            printerSlot = getLabEquipmentSlotByType(node, PRINTER_LAB_EQUIPMENT_TYPE);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("MSL OnSave");
            node.AddNode(cirSlot.getConfigNode());
            node.AddNode(firSlot.getConfigNode());
            node.AddNode(printerSlot.getConfigNode());

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
            firSlot.onStart(this);
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
                    fir = labIVA.transform.GetChild(2).gameObject;

                    if (firSlot.isEquipmentInstalled())
                    {
                        fir.SetActive(true);
                    }
                    else
                    {
                        fir.SetActive(false);
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
                case EquipmentRacks.FIR:
                    if (firSlot.isEquipmentInstalled() && firSlot.experimentSlotFree())
                    {
                        firSlot.installExperiment(exp);
                        ffrStatus = exp.getAbbreviation();
                        Fields["ffrStatus"].guiActive = true;
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + firSlot.isEquipmentInstalled() + "; free: " + firSlot.experimentSlotFree());
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
                case EquipmentRacks.FIR:
                    fir.SetActive(true);
                    firSlot.install(le, this);
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
                case EquipmentRacks.FIR:
                    if (fir != null)
                    {
                        fir.SetActive(firSlot.isEquipmentInstalled());
                    }
                    else
                    {
                        initERacksActive();
                        if(fir != null)fir.SetActive(firSlot.isEquipmentInstalled());
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

                case EquipmentRacks.FIR:
                    return firSlot.isEquipmentInstalled();

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

                case EquipmentRacks.FIR:
                    return firSlot.experimentSlotFree();

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

                case EquipmentRacks.FIR:
                    return firSlot.isEquipmentRunning();

                case EquipmentRacks.PRINTER:
                    return printerSlot.isEquipmentRunning();

                default:
                    return false;
            }
        }

        protected override void displayStatusMessage(string s)
        {
            try {
            labStatus = s;
            Fields["labStatus"].guiActive = true;
            } catch (Exception e) {
                NE_Helper.logError("MSL_Module.displayStatusMessage(): caught exception " + e +"\n" + e.StackTrace);
            }
        }

        protected override void updateLabStatus()
        {
            Fields["labStatus"].guiActive = false;
            if (cir == null || fir == null || printer == null)
            {
                initERacksActive();
            }

            if (cirSlot.isEquipmentRunning() || firSlot.isEquipmentRunning() || printerSlot.isEquipmentRunning())
            {
                Events["labAction"].guiName = doResearch? "#ne_Pause_Research" : "#ne_Resume_Research";
                Events["labAction"].active = true;
            }
            else
            {
                Events["labAction"].active = false;
            }

            if (!cirSlot.isEquipmentInstalled())
            {
                Events["installCIR"].active = checkForRackModule(EquipmentRacks.CIR);
                Fields["cirStatus"].guiActive = false;
            }
            else
            {
                Events["installCIR"].active = false;
                Events["moveCIRExp"].active = cirSlot.canExperimentMove(part.vessel);
                Fields["cirStatus"].guiActive = true;
                if (Events["moveCIRExp"].active)
                {
                    Events["moveCIRExp"].guiName = Localizer.Format("#ne_Move_1", cirSlot.getExperiment().getAbbreviation());
                }

                if (cirSlot.canActionRun())
                {
                    string cirActionString = cirSlot.getActionString();
                    Events["actionCIRExp"].guiName = cirActionString;
                }
                Events["actionCIRExp"].active = cirSlot.canActionRun();
                if (!cirSlot.experimentSlotFree())
                {
                    cirStatus = cirSlot.getExperiment().getAbbreviation() + ": " + cirSlot.getExperiment().stateString();
                }
                else
                {
                    cirStatus = Localizer.GetStringByTag("#ne_No_Experiment");
                }
            }
            if (!firSlot.isEquipmentInstalled())
            {
                Events["installFIR"].active = checkForRackModule(EquipmentRacks.FIR);
                Fields["ffrStatus"].guiActive = false;
            }
            else
            {
                Events["installFIR"].active = false;
                Events["moveFIRExp"].active = firSlot.canExperimentMove(part.vessel);
                Fields["ffrStatus"].guiActive = true;
                if (Events["moveFIRExp"].active)
                {
                    Events["moveFIRExp"].guiName = Localizer.Format("#ne_Move_1", firSlot.getExperiment().getAbbreviation());
                }
                if (firSlot.canActionRun())
                {
                    string ffrActionString = firSlot.getActionString();
                    Events["actionFIRExp"].guiName = ffrActionString;
                }
                Events["actionFIRExp"].active = firSlot.canActionRun();
                if (!firSlot.experimentSlotFree())
                {
                    ffrStatus = firSlot.getExperiment().getAbbreviation() + ": " + firSlot.getExperiment().stateString();

                }
                else
                {
                    ffrStatus = Localizer.GetStringByTag("#ne_No_Experiment");
                }
            }
            if (!printerSlot.isEquipmentInstalled())
            {
                Events["installPrinter"].active = checkForRackModule(EquipmentRacks.PRINTER);
                Fields["prStatus"].guiActive = false;
            }
            else
            {
                Events["installPrinter"].active = false;
                Events["movePRExp"].active = printerSlot.canExperimentMove(part.vessel);
                Fields["prStatus"].guiActive = true;
                if (Events["movePRExp"].active)
                {
                    Events["movePRExp"].guiName = Localizer.Format("#ne_Move_1", printerSlot.getExperiment().getAbbreviation());
                }

                if (printerSlot.canActionRun())
                {
                    string prActionString = printerSlot.getActionString();
                    Events["actionPRExp"].guiName = prActionString;
                }
                Events["actionPRExp"].active = printerSlot.canActionRun();
                if (!printerSlot.experimentSlotFree())
                {
                    prStatus = printerSlot.getExperiment().getAbbreviation() + ": " + printerSlot.getExperiment().stateString();
                }
                else
                {
                    prStatus = Localizer.GetStringByTag("#ne_No_Experiment");
                }
            }
        }

        protected override bool onLabPaused()
        {
            if(! base.onLabPaused() )
            {
                return false;
            }

            /* Delete all alarms */
            cirSlot?.getExperiment()?.onPaused();
            firSlot?.getExperiment()?.onPaused();
            printerSlot?.getExperiment()?.onPaused();
            return true;
        }

        protected override bool onLabStarted()
        {
            if(! base.onLabStarted() )
            {
                return false;
            }

            /* Create alarms for any running experiments */
            cirSlot?.getExperiment()?.onResumed();
            firSlot?.getExperiment()?.onResumed();
            printerSlot?.getExperiment()?.onResumed();
            return true;
        }

        private string getEquipmentString()
        {
            string ret = "";
            if (firSlot.isEquipmentInstalled())
            {
                ret += "FIR";
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
                ret = Localizer.GetStringByTag("#ne_none");
            }
            return ret;
        }

        private bool checkForRackModule(EquipmentRacks equipmentRack)
        {
            return getRackModule(equipmentRack) != null;
        }

        private EquipmentRackContainer getRackModule(EquipmentRacks equipmentRack)
        {
            EquipmentRackContainer[] modules = GameObject.FindObjectsOfType(typeof(EquipmentRackContainer)) as EquipmentRackContainer[];

            for (int idx = 0, count = modules.Length; idx < count; idx++)
            {
                var module = modules[idx];
                if (module.vessel == this.vessel && module.getRackType() == equipmentRack)
                {
                    return module;
                }
            }

            return null;
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Install_CIR", active = false)]
        public void installCIR()
        {
            EquipmentRackContainer module = getRackModule(EquipmentRacks.CIR);
            if (module != null)
            {
                installEquipmentRack(module.install());
            }
            else
            {
                displayStatusMessage(Localizer.GetStringByTag("#ne_Equipment_Rack_Module_not_found"));
            }
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Install_FIR", active = false)]
        public void installFIR()
        {
            EquipmentRackContainer module = getRackModule(EquipmentRacks.FIR);
            if (module != null)
            {
                installEquipmentRack(module.install());
            }
            else
            {
                displayStatusMessage(Localizer.GetStringByTag("#ne_Equipment_Rack_Module_not_found"));
            }
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Install_3DP", active = false)]
        public void installPrinter()
        {
            EquipmentRackContainer module = getRackModule(EquipmentRacks.PRINTER);
            if (module != null)
            {
                installEquipmentRack(module.install());
            }
            else
            {
                displayStatusMessage(Localizer.GetStringByTag("#ne_Equipment_Rack_Module_not_found"));
            }
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_FIR_Experiment", active = false)]
        public void moveFIRExp()
        {
            firSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_FIR_Experiment", active = false)]
        public void actionFIRExp()
        {
            firSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_CIR_Experiment", active = false)]
        public void moveCIRExp()
        {
            cirSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_CIR_Experiment", active = false)]
        public void actionCIRExp()
        {
            cirSlot.experimentAction();
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_3DP_Experiment", active = false)]
        public void movePRExp()
        {
            printerSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_3DP_Experiment", active = false)]
        public void actionPRExp()
        {
            printerSlot.experimentAction();
        }

        public override string GetInfo()
        {
            String ret = base.GetInfo();
            ret += (ret == "" ? "" : "\n") + Localizer.Format("#ne_Lab_Time_per_hour_1", LabTimePerHour);
            ret += "\n";
            ret += Localizer.GetStringByTag("#ne_You_can_install_equipment_racks_in_this_lab_to_run_experiments");
            return ret;
        }

        /// <summary>
        /// Returns the mass of installed equipment and experiments.
        /// </summary>
        /// <returns>The mass.</returns>
        protected override float getMass()
        {
            float mass = 0f;
            mass += cirSlot.getMass();
            mass += firSlot.getMass();
            mass += printerSlot.getMass();
            return mass;
        }
    }
}
