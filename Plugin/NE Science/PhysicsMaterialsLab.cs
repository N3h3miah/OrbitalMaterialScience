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
    class PhysicsMaterialsLab : Lab
    {

        [KSPField(isPersistant = false)]
        public float LabTimePerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerLabTime = 0;

        [KSPField(isPersistant = false)]
        public float CirBurnTimePerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerCirBurnTime = 0;

        [KSPField(isPersistant = false)]
        public float FFRTestRunPerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerTestRun = 0;

        [KSPField(isPersistant = false)]
        public float PrintLayerRunPerHour = 0;
        [KSPField(isPersistant = false)]
        public float ChargePerLayer = 0;

        [KSPField(isPersistant = true)]
        public bool cirInstalled = false;

        [KSPField(isPersistant = true)]
        public bool ffrInstalled = false;

        [KSPField(isPersistant = true)]
        public bool printerInstalled = false;

        [KSPField(isPersistant = false, guiActive = false, guiName = "Equipment")]
        public string equipment = "";

        private const string FFR_ANIMATION = "FFR_Pump";

        private GameObject cir;
        private GameObject ffr;
        private GameObject printer;

        public Generator labTimeGenerator;
        public Generator ffrGenerator;
        public Generator cirGenerator;
        public Generator printerGenerator;

        public bool ffrRunning;
        public bool cirRunning;
        public bool printerRunning;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
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
                    NE_Helper.log("set euipment racks");
                    printer = labIVA.transform.GetChild(0).gameObject;
                    cir = labIVA.transform.GetChild(1).gameObject;
                    ffr = labIVA.transform.GetChild(2).gameObject;

                    if (ffrInstalled)
                    {
                        installEquipmentRack(EquipmentRacks.FFR);
                    }
                    else
                    {
                        ffr.SetActive(false);
                    }

                    if (cirInstalled)
                    {
                        installEquipmentRack(EquipmentRacks.CIR);
                    }
                    else
                    {
                        cir.SetActive(false);
                    }

                    if (printerInstalled)
                    {
                        installEquipmentRack(EquipmentRacks.PRINTER);
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

        

        public void installEquipmentRack(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.FFR:
                    ffrInstalled = true;
                    ffr.SetActive(ffrInstalled);
                    part.mass += 3;
                    ffrGenerator = createGenerator(Resources.FFR_TEST_RUN, FFRTestRunPerHour, Resources.ELECTRIC_CHARGE, ChargePerTestRun);
                    generators.Add(ffrGenerator);
                    break;
                case EquipmentRacks.CIR:
                    cirInstalled = true;
                    cir.SetActive(cirInstalled);
                    part.mass += 3;
                    cirGenerator = createGenerator(Resources.CIR_BURN_TIME, CirBurnTimePerHour, Resources.ELECTRIC_CHARGE, ChargePerCirBurnTime);
                    generators.Add(cirGenerator);
                    break;
                case EquipmentRacks.PRINTER:
                    printerInstalled = true;
                    printer.SetActive(printerInstalled);
                    part.mass += 2.7f;
                    printerGenerator = createGenerator(Resources.PRINT_LAYER, PrintLayerRunPerHour, Resources.ELECTRIC_CHARGE, ChargePerLayer);
                    generators.Add(printerGenerator);
                    break;
            }
        }

        private void setEquipmentActive(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.FFR:
                    if (ffr != null)
                    {
                        ffr.SetActive(ffrInstalled);
                    }
                    else
                    {
                        initERacksActive();
                        if(ffr != null)ffr.SetActive(ffrInstalled);
                    }
                    break;
                case EquipmentRacks.CIR:
                    if (cir != null)
                    {
                        cir.SetActive(cirInstalled);
                    }
                    else
                    {
                        initERacksActive();
                        if (cir != null) cir.SetActive(cirInstalled);
                    }
                    break;
                case EquipmentRacks.PRINTER:
                    if (printer != null)
                    {
                        printer.SetActive(printerInstalled);
                    }
                    else
                    {
                        initERacksActive();
                        if (printer != null) printer.SetActive(printerInstalled);
                    }
                    break;
            }
        }

        public bool hasEquipmentInstalled(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.CIR:
                    return cirInstalled;

                case EquipmentRacks.FFR:
                    return ffrInstalled;

                case EquipmentRacks.PRINTER:
                    return printerInstalled;

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

            if (!cirInstalled)
            {
                Events["installCIR"].active = checkForRackModul(EquipmentRacks.CIR);
            }
            else
            {
                Events["installCIR"].active = false;
            }
            if (!ffrInstalled)
            {
                Events["installFFR"].active = checkForRackModul(EquipmentRacks.FFR);
            }
            else
            {
                Events["installFFR"].active = false;
            }
            if (!printerInstalled)
            {
                Events["installPrinter"].active = checkForRackModul(EquipmentRacks.PRINTER);
            }
            else
            {
                Events["installPrinter"].active = false;
            }

            updateAnimaitonState();
        }

        private void updateAnimaitonState()
        {
            if (ffrInstalled)
            {
                double last = ffrGenerator.rates[Resources.FFR_TEST_RUN].last_produced;
                bool state = (last < -0.0000001);
                if (ffrRunning != state)
                {
                    ffrRunning = state;
                }
            }

            if (printerInstalled)
            {
                double last = printerGenerator.rates[Resources.PRINT_LAYER].last_produced;
                bool state = (last < -0.0000001);
                if (printerRunning != state)
                {
                    printerRunning = state;
                }
            }

            if (cirInstalled)
            {
                double last = cirGenerator.rates[Resources.CIR_BURN_TIME].last_produced;
                bool state = (last < -0.0000001);
                if (cirRunning != state)
                {
                    cirRunning = state;
                }
            }
        }

        private string getEquipmentString()
        {
            string ret = "";
            if (ffrInstalled)
            {
                ret += "FFR";
            }
            if (cirInstalled)
            {
                if (ret.Length > 0) ret += ", ";
                ret += "CIR";
            }
            if (printerInstalled)
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
                modul.install();
                installEquipmentRack(EquipmentRacks.CIR);
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
                modul.install();
                installEquipmentRack(EquipmentRacks.FFR);
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
                modul.install();
                installEquipmentRack(EquipmentRacks.PRINTER);
            }
            else
            {
                displayStatusMessage("Equipment Rack Modul not found!");
            }
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
