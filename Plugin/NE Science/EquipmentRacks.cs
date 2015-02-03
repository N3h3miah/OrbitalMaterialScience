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
    public class EquipmentRackRegistry
    {
        static readonly List<KeyValuePair<EquipmentRacks, String>> racks = new List<KeyValuePair<EquipmentRacks, string>> { new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.PRINTER, "NE.3PR"),
            new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.CIR, "NE.CIR"), new KeyValuePair<EquipmentRacks, String>(EquipmentRacks.FFR, "NE.FFR")};

        public static List<LabEquipment> getAvailableRacks()
        {
            List<LabEquipment> list = new List<LabEquipment>();
            foreach (KeyValuePair<EquipmentRacks, string> p in racks)
            {
                AvailablePart part = PartLoader.getPartInfoByName(p.Value);
                if (part != null)
                {
                    Part pPf = part.partPrefab;
                    NE_Helper.log("Mass" + pPf.mass);
                    LabEquipmentModule lem = pPf.GetComponent<LabEquipmentModule>();
                    NE_Helper.log(lem.eqName);
                    foreach (AvailablePart.ModuleInfo mi in part.moduleInfos)
                    {
                        NE_Helper.log(mi.ToString());
                    }
                    if(ResearchAndDevelopment.PartTechAvailable(part)){
                    list.Add(getLabEquipment(part.partPrefab, p.Key));
                    }
                }
            }

            return list;
        }

        private static LabEquipment getLabEquipment(Part part, EquipmentRacks type)
        {
            LabEquipmentModule lem = part.GetComponent<LabEquipmentModule>();
            return new LabEquipment(lem.abbreviation, lem.eqName, type, part.mass);
        }
    }

    public enum EquipmentRacks
    {
        CIR, FFR, PRINTER, NONE
    }
}
