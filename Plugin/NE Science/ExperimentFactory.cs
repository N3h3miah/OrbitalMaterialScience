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
    *Module used to add Experiments to the Tech tree. 
    */
    public class NE_ExperimentModule : PartModule
    {

        [KSPField(isPersistant = false)]
        public string type = "";

    } 


    public class ExperimentFactory
    {
        static readonly List<string> expRegistry = new List<string>() { "NE.TEST", "NE.CCFE", "NE.CFE",
            "NE.FLEX", "NE.CFI", "NE.MIS1", "NE.MIS2", "NE.MIS3" };

        public static List<ExperimentData> getAvailableExperiments()
        {
            List<ExperimentData> list = new List<ExperimentData>();
            foreach (string pn in expRegistry)
            {
                AvailablePart part = PartLoader.getPartInfoByName(pn);
                if (part != null)
                {
                    if (ResearchAndDevelopment.PartTechAvailable(part))
                    {
                        Part pPf = part.partPrefab;
                        NE_ExperimentModule exp = pPf.GetComponent<NE_ExperimentModule>();
                        list.Add(getExperiment(exp.type));
                    }
                }
            }

            return list;
        }

        public static ExperimentData getExperiment(string type)
        {
            switch (type)
            {
                case "Test":
                    return new TestExperimentData();
                case "CCF":
                    return new CCF_ExperimentData();
                case "CFE":
                    return new CFE_ExperimentData();
                case "FLEX":
                    return new FLEX_ExperimentData();
                case "CFI":
                    return new CFI_ExperimentData();
                case "MIS1":
                    return new MIS1_ExperimentData();
                case "MIS2":
                    return new MIS2_ExperimentData();
                case "MIS3":
                    return new MIS3_ExperimentData();
                default:
                    return ExperimentData.getNullObject();

            }
        }
    }
}
