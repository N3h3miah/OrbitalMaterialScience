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


namespace NE_Science
{
    class EquipmentRackModule : PartModule
    {
        [KSPField(isPersistant = false)]
        public string RackType = "";

        [KSPField(isPersistant = true)]
        public bool empty = false;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Contains")]
        public string status = "";

        private PhysicsMaterialsLab.EquipmentRacks type;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            if (empty)
            {
                part.mass = 0.2f;
                type = PhysicsMaterialsLab.EquipmentRacks.NONE;
                status = "empty";
            }
            else
            {
                switch (RackType)
                {
                    case "CIR":
                        type = PhysicsMaterialsLab.EquipmentRacks.CIR;
                        status = "Combustion Integrated Rack (CIR)";
                        break;

                    case "FFR":
                        type = PhysicsMaterialsLab.EquipmentRacks.FFR;
                        status = "Fluid Flow Rack (FFR)";
                        break;

                    case "Printer":
                        type = PhysicsMaterialsLab.EquipmentRacks.PRINTER;
                        status = "3D Printer Rack (3PR)";
                        break;

                    default:
                        type = PhysicsMaterialsLab.EquipmentRacks.NONE;
                        status = "empty";
                        break;
                }
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (empty)
            {
                part.mass = 0.2f;
                type = PhysicsMaterialsLab.EquipmentRacks.NONE;
                status = "empty";
            }
        }

        public PhysicsMaterialsLab.EquipmentRacks getRackType()
        {
            return type;
        }

        public void install()
        {
            empty = true;
            type = PhysicsMaterialsLab.EquipmentRacks.NONE;
            status = "empty";
            part.mass = 0.2f;
        }

        public override string GetInfo()
        {
            string rackString = "";
            switch (type)
            {
                case PhysicsMaterialsLab.EquipmentRacks.FFR:
                    rackString = "Fluid Flow Rack (FFR)";
                    break;
                case PhysicsMaterialsLab.EquipmentRacks.CIR:
                    rackString = "Combustion Integrated Rack (CIR)";
                    break;
                case PhysicsMaterialsLab.EquipmentRacks.PRINTER:
                    rackString = "3D Printer Rack (3PR)";
                    break;
            }
            return "Contains a " + rackString + " that can be installed in a MSL-1000, to run additional experiments" ;
        }
    }
}
