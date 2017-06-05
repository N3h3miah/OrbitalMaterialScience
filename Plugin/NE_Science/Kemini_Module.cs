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
using KSP.Localization;

namespace NE_Science
{
    public class Kemini_Module : Lab
    {

        private const string KEMINI_CONFIG_NODE_NAME = "NE_KEMINI_LabEquipmentSlot";

        /// <summary>
        /// Field to display the Kemini lab status in the popup menu
        /// </summary>
        [KSPField(isPersistant = false, guiActive = false, guiName = "#ne_Kemini_Lab")]
        public string keminiStatus = "";

        private LabEquipmentSlot keminiSlot = new LabEquipmentSlot(EquipmentRacks.KEMINI);

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            NE_Helper.log("KL OnLoad");
            keminiSlot = getLabEquipmentSlot(node.GetNode(KEMINI_CONFIG_NODE_NAME));
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            NE_Helper.log("KL OnSave");
            node.AddNode(getConfigNodeForSlot(KEMINI_CONFIG_NODE_NAME, keminiSlot));
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state == StartState.Editor)
            {
                return;
            }
            keminiSlot.onStart(this);
            if (!keminiSlot.isEquipmentInstalled())
            {
                LabEquipment keminiLab = new LabEquipment("KL", "Kemini Lab", EquipmentRacks.KEMINI, 0f, 0f, 1f, Resources.LAB_TIME, 10f, Resources.ELECTRIC_CHARGE);
                keminiSlot.install(keminiLab, this);
            }
        }


        public override void installExperiment(ExperimentData exp)
        {
            switch (exp.getEquipmentNeeded())
            {
                case EquipmentRacks.KEMINI:
                    if (keminiSlot.isEquipmentInstalled() && keminiSlot.experimentSlotFree())
                    {
                        keminiSlot.installExperiment(exp);
                        keminiStatus = keminiSlot.getExperiment().getAbbreviation() + ": " + keminiSlot.getExperiment().displayStateString();
                        Fields["keminiStatus"].guiActive = true;
                        keminiSlot.experimentAction();
                    }
                    else
                    {
                        NE_Helper.logError("installExperiment, installed: " + keminiSlot.isEquipmentInstalled() + "; free: " + keminiSlot.experimentSlotFree());
                    }
                    break;
            }
        }


        public void installEquipmentRack(LabEquipment le)
        {
            switch (le.getType())
            {
                case EquipmentRacks.KEMINI:

                    keminiSlot.install(le, this);
                    break;
            }
        }

        public bool hasEquipmentInstalled(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.KEMINI:
                    return keminiSlot.isEquipmentInstalled();

                default:
                    return false;
            }
        }

        public bool hasEquipmentFreeExperimentSlot(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.KEMINI:
                    return keminiSlot.experimentSlotFree();
                default:
                    return false;
            }
        }

        public bool isEquipmentRunning(EquipmentRacks rack)
        {
            switch (rack)
            {
                case EquipmentRacks.KEMINI:
                    return keminiSlot.isEquipmentRunning();

                default:
                    return false;
            }
        }

        protected override void displayStatusMessage(string s)
        {
            labStatus = s;
            Fields["labStatus"].guiActive = true;
        }

        protected override void updateLabStatus()
        {
            Fields["labStatus"].guiActive = false;

            if (keminiSlot.isEquipmentInstalled())
            {
                Events["moveKeminiExp"].active = keminiSlot.canExperimentMove(part.vessel);
                if (Events["moveKeminiExp"].active)
                {
                    Events["moveKeminiExp"].guiName = Localizer.Format("#ne_Move_1", keminiSlot.getExperiment().getAbbreviation());
                }

                if (keminiSlot.canActionRun())
                {
                    string keminiActionString = keminiSlot.getActionString();
                    Events["actionKeminiExp"].guiName = keminiActionString;
                }
                Events["actionKeminiExp"].active = keminiSlot.canActionRun();
                if (!keminiSlot.experimentSlotFree())
                {
                    keminiStatus = keminiSlot.getExperiment().getAbbreviation() + ": " + keminiSlot.getExperiment().displayStateString();
                    Fields["keminiStatus"].guiActive = true;
                }
                else
                {
                    Fields["keminiStatus"].guiActive = false;
                }
            }
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Move_Kemini_Experiment", active = false)]
        public void moveKeminiExp()
        {
            keminiSlot.moveExperiment(part.vessel);
        }

        [KSPEvent(guiActive = true, guiName = "#ne_Action_Kemini_Experiment", active = false)]
        public void actionKeminiExp()
        {
            keminiSlot.experimentAction();
        }

        public override string GetInfo()
        {
            String ret = base.GetInfo();
            return ret;
        }

    }
}
