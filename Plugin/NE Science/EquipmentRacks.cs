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

        public static List<EquipmentRacks> getAvailableRacks()
        {
            List<EquipmentRacks> list = new List<EquipmentRacks>();
            foreach (KeyValuePair<EquipmentRacks, string> p in racks)
            {
                AvailablePart part = PartLoader.getPartInfoByName(p.Value);
                if (part != null && ResearchAndDevelopment.PartTechAvailable(part))
                {
                    list.Add(p.Key);
                }
            }

            return list;
        }
    }

    public enum EquipmentRacks
    {
        CIR, FFR, PRINTER, NONE
    }
}
