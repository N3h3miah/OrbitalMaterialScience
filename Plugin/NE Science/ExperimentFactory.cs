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
