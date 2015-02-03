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

namespace NE_Science
{
    /*
     *Module used to add Lab Equipment to the Tech tree. 
     */
    public class LabEquipmentModule : PartModule
    {

        [KSPField(isPersistant = false)]
        public string abbreviation = "";

        [KSPField(isPersistant = false)]
        public string eqName = "";

    }

    /*
     * Class used to add LabEquipment to Containers
     */
    public class LabEquipment
    {
        public const string CONFIG_NODE_NAME = "NE_LabEquipment";
        private const string ABB_VALUE = "abb";
        private const string NAME_VALUE = "name";
        private const string TYPE_VALUE = "type";
        private const string MASS_VALUE = "mass";

        private string abb;
        private string name;
        private float mass;
        private EquipmentRacks type;

        public LabEquipment(string abb, string name, EquipmentRacks type, float mass)
        {
            this.abb = abb;
            this.name = name;
            this.type = type;
            this.mass = mass;
        }

        public string getAbbreviation()
        {
            return abb;
        }

        public string getName()
        {
            return name;
        }

        public EquipmentRacks getType()
        {
            return type;
        }

        public float getMass()
        {
            return mass;
        }

        static public LabEquipment getNullObject()
        {
             return new LabEquipment("empty", "empty", EquipmentRacks.NONE, 0f);
        }

        public ConfigNode getNode()
        {
            ConfigNode node = new ConfigNode(CONFIG_NODE_NAME);

            node.AddValue(ABB_VALUE, abb);
            node.AddValue(NAME_VALUE, name);
            node.AddValue(MASS_VALUE, mass);
            node.AddValue(TYPE_VALUE, type.ToString());

            return node;
        }

        public static LabEquipment getLabEquipmentFromNode(ConfigNode node)
        {
            if (node.name != CONFIG_NODE_NAME)
            {
                NE_Helper.logError("getLabEquipmentFromNode: invalid Node: " + node.name);
                return getNullObject();
            }

            string abb = node.GetValue(ABB_VALUE);
            string name = node.GetValue(NAME_VALUE);
            float mass = float.Parse(node.GetValue(MASS_VALUE));
            EquipmentRacks type = getType(node.GetValue(TYPE_VALUE));

            return new LabEquipment(abb, name, type, mass);
        }

        private static EquipmentRacks getType(string p)
        {
            switch (p)
            {
                case "FFR":
                    return EquipmentRacks.FFR;
                case "CIR":
                    return EquipmentRacks.CIR;
                case "PRINTER":
                    return EquipmentRacks.PRINTER;
                default:
                    return EquipmentRacks.NONE;

            }
        }

    }
}
